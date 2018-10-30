namespace TailBlazer.Views.Recent
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using DynamicData;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    using TailBlazer.Views.Searching;
    public class RecentSearchCollection : IRecentSearchCollection, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ISourceCache<RecentSearch, string> _files = new SourceCache<RecentSearch, string>(fi => fi.Text);
        private readonly ILogger _logger;
        #endregion
        #region Constructors
        public RecentSearchCollection(ILogger logger, ISetting<RecentSearch[]> setting)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                                      var newItems = files.Where(f => !innerCache.Lookup(f.Text).HasValue).ToArray();
                                      innerCache.AddOrUpdate(newItems);
                                  });
                     });
            var settingsWriter = this._files.Connect().ToCollection().Subscribe(items => { setting.Write(items.ToArray()); });
            this._cleanUp = new CompositeDisposable(settingsWriter, loader, this._files, this.Items);
        }
        #endregion
        #region Properties
        public IObservableList<RecentSearch> Items { get; }
        #endregion
        #region Methods
        public void Add(RecentSearch file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            this._files.AddOrUpdate(file);
        }
        public void Dispose() { this._cleanUp.Dispose(); }
        public void Remove(RecentSearch file) { this._files.Remove(file); }
        #endregion
    }
}