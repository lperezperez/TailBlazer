namespace TailBlazer.Domain.FileHandling.TextAssociations
{
    using DynamicData;
    using DynamicData.Kernel;
    public interface ITextAssociationCollection
    {
        #region Properties
        IObservableList<TextAssociation> Items { get; }
        #endregion
        #region Methods
        Optional<TextAssociation> Lookup(string key);
        void MarkAsChanged(TextAssociation file);
        #endregion
    }
}