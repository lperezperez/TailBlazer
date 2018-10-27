namespace TailBlazer.Domain.FileHandling.Recent
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using DynamicData;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    public class RecentFileCollection : IRecentFileCollection, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ISourceCache<RecentFile, string> _files = new SourceCache<RecentFile, string>(fi => fi.Name);
        private readonly ILogger _logger;
        #endregion
        #region Constructors
        public RecentFileCollection(ILogger logger, ISetting<RecentFile[]> setting)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            this._logger = logger;
            this.Items = this._files.Connect().RemoveKey().AsObservableList();
            var loader = setting.Value.Subscribe
                (
                 files =>
                     {
                         this._files.Edit
                             (
                              innerCache =>
                                  {
                                      //all files are loaded when state changes, so only add new ones
                                      var newItems = files.Where(f => !innerCache.Lookup(f.Name).HasValue).ToArray();
                                      innerCache.AddOrUpdate(newItems);
                                  });
                     });
            var settingsWriter = this._files.Connect().ToCollection().Subscribe(items => { setting.Write(items.ToArray()); });
            this._cleanUp = new CompositeDisposable(settingsWriter, loader, this._files, this.Items);
        }
        #endregion
        #region Properties
        public IObservableList<RecentFile> Items { get; }
        #endregion
        #region Methods
        public void Add(RecentFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            this._files.AddOrUpdate(file);
        }
        public void Dispose() { this._cleanUp.Dispose(); }
        public void Remove(RecentFile file) { this._files.Remove(file); }
        #endregion
    }
}