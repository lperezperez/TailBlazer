namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Reactive.Linq;
    using DynamicData;
    using DynamicData.Aggregation;
    using TailBlazer.Domain.Annotations;
    public interface ILineScroller : IDisposable
    {
        #region Properties
        IObservableCache<Line, LineKey> Lines { get; }
        #endregion
    }
    public static class LineScrollerEx
    {
        #region Methods
        public static IObservable<int> MaximumLines([NotNull] this ILineScroller source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Lines.Connect().Maximum(l => l.Text?.Length ?? 0).StartWith(0).DistinctUntilChanged();
        }
        #endregion
    }
}