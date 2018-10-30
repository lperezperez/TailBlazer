namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.IO;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Ratings;
    public class FileWatcher : IFileWatcher
    {
        #region Fields
        private readonly ISubject<long> _scanFrom = new BehaviorSubject<long>(0);
        #endregion
        #region Constructors
        public FileWatcher([NotNull] FileInfo fileInfo, IRatingService ratingsMetrics, IScheduler scheduler = null)
        {
            this.FileInfo = fileInfo;
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            scheduler = scheduler ?? Scheduler.Default;
            var refreshRate = ratingsMetrics.Metrics.Take(1).Select(metrics => TimeSpan.FromMilliseconds(metrics.RefreshRate)).Wait();
            var shared = this._scanFrom.Select(start => start == 0 ? fileInfo.WatchFile(scheduler: scheduler, refreshPeriod: refreshRate) : fileInfo.WatchFile(scheduler: scheduler, refreshPeriod: refreshRate).ScanFromEnd()).Switch();
            this.Latest = shared.TakeWhile(notification => notification.Exists).Repeat().Replay(1).RefCount();
            this.Status = fileInfo.WatchFile(scheduler: scheduler).Select
                (
                 notificiation =>
                     {
                         if (!notificiation.Exists || notificiation.Error != null)
                             return FileStatus.Error;
                         return FileStatus.Loaded;
                     }).StartWith(FileStatus.Loading).DistinctUntilChanged();
        }
        #endregion
        #region Properties
        public string Extension => this.FileInfo.Extension;
        public string Folder => this.FileInfo.DirectoryName;
        public string FullName => this.FileInfo.FullName;
        public IObservable<FileNotification> Latest { get; }
        public string Name => this.FileInfo.Name;
        public IObservable<FileStatus> Status { get; }
        private FileInfo FileInfo { get; }
        #endregion
        #region Methods
        public void Clear() { this._scanFrom.OnNext(this.FileInfo.Length); }
        public void Reset() { this._scanFrom.OnNext(0); }
        public void ScanFrom(long scanFrom) { this._scanFrom.OnNext(scanFrom); }
        #endregion
    }
}