namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using TailBlazer.Domain.Annotations;
    public sealed class SearchInfo : IEquatable<SearchInfo>
    {
        #region Constructors
        public SearchInfo([NotNull] string searchText, bool isGlobal, [NotNull] IObservable<ILineProvider> latest, SearchType searchType)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            this.SearchText = searchText;
            this.IsGlobal = isGlobal;
            this.Latest = latest;
            this.SearchType = searchType;
        }
        #endregion
        #region Properties
        public bool IsGlobal { get; }
        public IObservable<ILineProvider> Latest { get; }
        public string SearchText { get; }
        public SearchType SearchType { get; }
        #endregion
        #region Methods
        public static bool operator ==(SearchInfo left, SearchInfo right) => object.Equals(left, right);
        public static bool operator !=(SearchInfo left, SearchInfo right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((SearchInfo)obj);
        }
        public override int GetHashCode() => this.SearchText?.GetHashCode() ?? 0;
        public override string ToString() => $"{this.SearchText}";
        public bool Equals(SearchInfo other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return string.Equals(this.SearchText, other.SearchText);
        }
        #endregion
    }
}