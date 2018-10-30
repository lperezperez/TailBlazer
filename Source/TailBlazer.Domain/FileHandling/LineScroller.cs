namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Infrastructure;
    public class LineScroller : ILineScroller
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        #endregion
        #region Constructors
        [Obsolete("USE OTHER OVERLOAD")]
        public LineScroller(FileInfo file, IObservable<ILineProvider> latest, IObservable<ScrollRequest> scrollRequest, ILogger logger, IScheduler scheduler = null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.Info($"Constructing file tailer for {file.FullName}");
            var lines = new SourceCache<Line, LineKey>(l => l.Key);
            this.Lines = lines.AsObservableCache();
            var locker = new object();
            scrollRequest = scrollRequest.Synchronize(locker);
            var aggregator = latest.CombineLatest(scrollRequest, (currentLines, scroll) => currentLines.ReadLines(scroll).ToArray()).RetryWithBackOff<Line[], Exception>((ex, i) => TimeSpan.FromSeconds(1)).Subscribe
                (
                 currentPage =>
                     {
                         var previous = lines.Items.ToArray();
                         var added = currentPage.Except(previous, Line.TextStartComparer).ToArray();
                         var removed = previous.Except(currentPage, Line.TextStartComparer).ToArray();
                         lines.Edit
                             (
                              innerCache =>
                                  {
                                      if (removed.Any()) innerCache.Remove(removed);
                                      if (added.Any()) innerCache.AddOrUpdate(added);
                                  });
                     });
            this._cleanUp = new CompositeDisposable(this.Lines, lines, aggregator);
        }
        public LineScroller([NotNull] IObservable<ILineProvider> latest, [NotNull] IObservable<ScrollRequest> scrollRequest)
        {
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            if (scrollRequest == null) throw new ArgumentNullException(nameof(scrollRequest));
            var lines = new SourceCache<Line, LineKey>(l => l.Key);
            this.Lines = lines.Connect().IgnoreUpdateWhen((current, previous) => current.Key == previous.Key).AsObservableCache();
            var locker = new object();
            scrollRequest = scrollRequest.Synchronize(locker);
            latest = latest.Synchronize(locker);
            var aggregator = latest.CombineLatest(scrollRequest, (currentLines, scroll) => new { currentLines, scroll }).Sample(TimeSpan.FromMilliseconds(50)).Select
                (
                 x =>
                     {
                         if (x.scroll == ScrollRequest.None || x.scroll.PageSize == 0 || x.currentLines.Count == 0)
                             return new Line[0];
                         return x.currentLines.ReadLines(x.scroll).ToArray();
                     }).RetryWithBackOff<Line[], Exception>((ex, i) => TimeSpan.FromSeconds(1)).Subscribe
                (
                 currentPage =>
                     {
                         var previous = lines.Items.ToArray();
                         var added = currentPage.Except(previous, Line.TextStartComparer).ToArray();
                         var removed = previous.Except(currentPage, Line.TextStartComparer).ToArray();
                         lines.Edit
                             (
                              innerCache =>
                                  {
                                      if (currentPage.Length == 0) innerCache.Clear();
                                      if (removed.Any()) innerCache.Remove(removed);
                                      if (added.Any()) innerCache.AddOrUpdate(added);
                                  });
                     });
            this._cleanUp = new CompositeDisposable(this.Lines, lines, aggregator);
        }
        #endregion
        #region Properties
        public IObservableCache<Line, LineKey> Lines { get; }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}