namespace TailBlazer.Domain.FileHandling.TextAssociations
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using DynamicData;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    public class TextAssociationCollection : ITextAssociationCollection, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ILogger _logger;
        private readonly ISourceCache<TextAssociation, CaseInsensitiveString> _textAssociations = new SourceCache<TextAssociation, CaseInsensitiveString>(fi => fi.Text);
        #endregion
        #region Constructors
        public TextAssociationCollection(ILogger logger, ISetting<TextAssociation[]> setting)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.Items = this._textAssociations.Connect().RemoveKey().AsObservableList();
            var loader = setting.Value.Subscribe
                (
                 files =>
                     {
                         this._textAssociations.Edit
                             (
                              innerCache =>
                                  {
                                      //all files are loaded when state changes, so only add new ones
                                      var newItems = files.Where(f => !innerCache.Lookup(f.Text).HasValue).ToArray();
                                      innerCache.AddOrUpdate(newItems);
                                  });
                     });
            var settingsWriter = this._textAssociations.Connect().ToCollection().Subscribe(items => { setting.Write(items.ToArray()); });
            this._cleanUp = new CompositeDisposable(settingsWriter, loader, this._textAssociations, this.Items);
        }
        #endregion
        #region Properties
        public IObservableList<TextAssociation> Items { get; }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        public Optional<TextAssociation> Lookup(string key) => this._textAssociations.Lookup(key);
        public void MarkAsChanged(TextAssociation file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            this._textAssociations.AddOrUpdate(file);
        }
        public void Remove(TextAssociation file) { this._textAssociations.Remove(file); }
        #endregion
    }
}