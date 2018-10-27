namespace TailBlazer.Views.Recent
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using DynamicData;
    using DynamicData.Binding;
    using TailBlazer.Domain.FileHandling.Recent;
    using TailBlazer.Domain.Infrastructure;
    public class RecentFilesViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ISubject<FileInfo> _fileOpenRequest = new Subject<FileInfo>();
        private readonly IRecentFileCollection _recentFileCollection;
        #endregion
        #region Constructors
        public RecentFilesViewModel(IRecentFileCollection recentFileCollection, ISchedulerProvider schedulerProvider)
        {
            this._recentFileCollection = recentFileCollection;
            if (recentFileCollection == null) throw new ArgumentNullException(nameof(recentFileCollection));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            var recentLoader = recentFileCollection.Items.Connect().Transform(rf => new RecentFileProxy(rf, toOpen => this._fileOpenRequest.OnNext(new FileInfo(toOpen.Name)), recentFileCollection.Remove)).Sort(SortExpressionComparer<RecentFileProxy>.Descending(proxy => proxy.Timestamp)).ObserveOn(schedulerProvider.MainThread).Bind(out var data).Subscribe();
            this.Files = data;
            this._cleanUp = Disposable.Create
                (
                 () =>
                     {
                         recentLoader.Dispose();
                         this._fileOpenRequest.OnCompleted();
                     });
        }
        #endregion
        #region Properties
        public ReadOnlyObservableCollection<RecentFileProxy> Files { get; }
        public IObservable<FileInfo> OpenFileRequest => this._fileOpenRequest.AsObservable();
        #endregion
        #region Methods
        public void Add(FileInfo fileInfo) { this._recentFileCollection.Add(new RecentFile(fileInfo)); }
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}