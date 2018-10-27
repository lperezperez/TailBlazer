namespace TailBlazer.Views.Formatting
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using DynamicData;
    using MaterialDesignThemes.Wpf;
    using TailBlazer.Domain.FileHandling.Search;
    public class IconProvider : IIconProvider, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ISourceList<IconDescription> _icons = new SourceList<IconDescription>();
        #endregion
        #region Constructors
        public IconProvider(IDefaultIconSelector defaultIconSelector)
        {
            this.DefaultIconSelector = defaultIconSelector;
            this.Icons = this._icons.AsObservableList();
            var icons = Enum.GetNames(typeof(PackIconKind)).Select
                (
                 str =>
                     {
                         var value = (PackIconKind)Enum.Parse(typeof(PackIconKind), str);
                         return new IconDescription(value, str);
                     });
            this._icons.AddRange(icons);
            this._cleanUp = new CompositeDisposable(this.Icons, this._icons);
        }
        #endregion
        #region Properties
        public IDefaultIconSelector DefaultIconSelector { get; }
        public IObservableList<IconDescription> Icons { get; }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}