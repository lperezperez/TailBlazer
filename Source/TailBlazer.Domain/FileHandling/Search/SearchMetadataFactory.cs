namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using TailBlazer.Domain.FileHandling.TextAssociations;
    using TailBlazer.Domain.Formatting;
    public class SearchMetadataFactory : ISearchMetadataFactory
    {
        #region Fields
        private readonly IDefaultColourSelector _defaultColourSelector;
        private readonly IDefaultIconSelector _defaultIconSelector;
        private readonly ITextAssociationCollection _textAssociationCollection;
        #endregion
        #region Constructors
        public SearchMetadataFactory(IDefaultIconSelector defaultIconSelector, IDefaultColourSelector defaultColourSelector, ITextAssociationCollection textAssociationCollection)
        {
            this._defaultIconSelector = defaultIconSelector;
            this._defaultColourSelector = defaultColourSelector;
            this._textAssociationCollection = textAssociationCollection;
        }
        #endregion
        #region Methods
        public SearchMetadata Create(string searchText, bool useRegex, int index, bool filter, bool isGlobal = false)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            var isExclusion = false;
            if (!useRegex)
            {
                var withNegation = searchText.WithNegation();
                isExclusion = withNegation.IsNegation;
                searchText = withNegation.Text;
            }
            var association = this._textAssociationCollection.Lookup(searchText);
            string icon;
            Hue hue;
            if (association.HasValue)
            {
                icon = association.Value.Icon;
                hue = this._defaultColourSelector.Lookup(new HueKey(association.Value.Swatch, association.Value.Hue));
            }
            else
            {
                icon = this._defaultIconSelector.GetIconFor(searchText, useRegex);
                hue = this._defaultColourSelector.Select(searchText);
            }
            return new SearchMetadata(index, searchText, isExclusion ? false : filter, true, useRegex, true, hue, icon, isGlobal, isExclusion);
        }
        #endregion
    }
}