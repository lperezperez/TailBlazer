namespace TailBlazer.Views.Tail
{
    using System.Linq;
    using TailBlazer.Domain.Settings;
    public class TailViewPersister : IPersistentView
    {
        #region Fields
        private readonly TailViewModel _tailView;
        private readonly ITailViewStateRestorer _tailViewStateRestorer;
        #endregion
        #region Constructors
        public TailViewPersister(TailViewModel tailView, ITailViewStateRestorer tailViewStateRestorer)
        {
            this._tailView = tailView;
            this._tailViewStateRestorer = tailViewStateRestorer;
        }
        #endregion
        #region Methods
        public void Restore(ViewState state) { this._tailViewStateRestorer.Restore(this._tailView, state.State); }
        ViewState IPersistentView.CaptureState()
        {
            var coverter = new TailViewToStateConverter();
            var state = coverter.Convert(this._tailView.Name, this._tailView.SearchCollection.Selected.Text, this._tailView.SearchMetadataCollection.Metadata.Items.ToArray());
            return new ViewState(TailViewModelConstants.ViewKey, state);
        }
        #endregion
    }
}