namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Formatting;
    public class SearchMetadata : IEquatable<SearchMetadata>
    {
        #region Constructors
        public SearchMetadata([NotNull] SearchMetadata searchMetadata, int newPosition)
        {
            if (searchMetadata == null) throw new ArgumentNullException(nameof(searchMetadata));
            this.Position = newPosition;
            this.SearchText = searchMetadata.SearchText;
            this.Filter = searchMetadata.Filter;
            this.Highlight = searchMetadata.Highlight;
            this.UseRegex = searchMetadata.UseRegex;
            this.IgnoreCase = searchMetadata.IgnoreCase;
            this.RegEx = searchMetadata.RegEx;
            this.Predicate = searchMetadata.Predicate;
            this.HighlightHue = searchMetadata.HighlightHue;
            this.IconKind = searchMetadata.IconKind;
            this.IsGlobal = searchMetadata.IsGlobal;
            this.IsExclusion = searchMetadata.IsExclusion;
        }
        public SearchMetadata([NotNull] SearchMetadata searchMetadata, int newPosition, bool isGlobal)
        {
            if (searchMetadata == null) throw new ArgumentNullException(nameof(searchMetadata));
            this.Position = newPosition;
            this.IsGlobal = isGlobal;
            this.SearchText = searchMetadata.SearchText;
            this.Filter = searchMetadata.Filter;
            this.Highlight = searchMetadata.Highlight;
            this.UseRegex = searchMetadata.UseRegex;
            this.IgnoreCase = searchMetadata.IgnoreCase;
            this.RegEx = searchMetadata.RegEx;
            this.Predicate = searchMetadata.Predicate;
            this.HighlightHue = searchMetadata.HighlightHue;
            this.IconKind = searchMetadata.IconKind;
            this.IsExclusion = searchMetadata.IsExclusion;
        }
        public SearchMetadata(int position, [NotNull] string searchText, bool filter, bool highlight, bool useRegex, bool ignoreCase, Hue highlightHue, string iconKind, bool isGlobal, bool isExclusion)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            this.Position = position;
            this.SearchText = searchText;
            this.Filter = filter;
            this.Highlight = highlight;
            this.UseRegex = useRegex;
            this.IgnoreCase = ignoreCase;
            this.HighlightHue = highlightHue;
            this.IconKind = iconKind;
            this.IsGlobal = isGlobal;
            this.IsExclusion = isExclusion;
            this.RegEx = this.BuildRegEx();
            this.Predicate = this.BuildPredicate();
        }
        #endregion
        #region Properties
        public static IEqualityComparer<SearchMetadata> EffectsFilterComparer { get; } = new EffectsFilterEqualityComparer();
        public static IEqualityComparer<SearchMetadata> EffectsHighlightComparer { get; } = new EffectsHighlightEqualityComparer();
        public static IEqualityComparer<SearchMetadata> SearchTextComparer { get; } = new SearchTextEqualityComparer();
        public bool Filter { get; }
        public bool Highlight { get; }
        public Hue HighlightHue { get; }
        public string IconKind { get; }
        public bool IgnoreCase { get; }
        public bool IsExclusion { get; }
        public bool IsGlobal { get; }
        public int Position { get; }
        public Func<string, bool> Predicate { get; }
        public Optional<Regex> RegEx { get; }
        public string SearchText { get; }
        public bool UseRegex { get; }
        #endregion
        #region Methods
        public static bool operator ==(SearchMetadata left, SearchMetadata right) => object.Equals(left, right);
        public static bool operator !=(SearchMetadata left, SearchMetadata right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((SearchMetadata)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Position;
                hashCode = (hashCode * 397) ^ (this.SearchText?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ this.Filter.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Highlight.GetHashCode();
                hashCode = (hashCode * 397) ^ this.UseRegex.GetHashCode();
                hashCode = (hashCode * 397) ^ this.IgnoreCase.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.HighlightHue?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.IconKind?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ this.IsGlobal.GetHashCode();
                hashCode = (hashCode * 397) ^ this.IsExclusion.GetHashCode();
                return hashCode;
            }
        }
        public override string ToString() => $"{this.SearchText} ({this.Position}) Filter: {this.Filter}, Highlight: {this.Highlight}, UseRegex: {this.UseRegex}, IgnoreCase: {this.IgnoreCase}";
        public bool Equals(SearchMetadata other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Position == other.Position && string.Equals(this.SearchText, other.SearchText) && this.Filter == other.Filter && this.Highlight == other.Highlight && this.UseRegex == other.UseRegex && this.IgnoreCase == other.IgnoreCase && object.Equals(this.HighlightHue, other.HighlightHue) && string.Equals(this.IconKind, other.IconKind) && this.IsGlobal == other.IsGlobal;
        }
        #endregion
        #region Classes
        private sealed class EffectsFilterEqualityComparer : IEqualityComparer<SearchMetadata>
        {
            #region Methods
            public bool Equals(SearchMetadata x, SearchMetadata y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                if (object.ReferenceEquals(x, null)) return false;
                if (object.ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                var stringComparison = x.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return string.Equals(x.SearchText, y.SearchText, stringComparison) && x.Filter == y.Filter && x.UseRegex == y.UseRegex && x.IgnoreCase == y.IgnoreCase && x.IsExclusion == y.IsExclusion;
            }
            public int GetHashCode(SearchMetadata obj)
            {
                unchecked
                {
                    var comparer = obj.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                    var hashCode = obj.SearchText != null ? comparer.GetHashCode(obj.SearchText) : 0;
                    hashCode = (hashCode * 397) ^ obj.Filter.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.UseRegex.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IgnoreCase.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IgnoreCase.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IsExclusion.GetHashCode();
                    return hashCode;
                }
            }
            #endregion
        }
        private sealed class EffectsHighlightEqualityComparer : IEqualityComparer<SearchMetadata>
        {
            #region Methods
            public bool Equals(SearchMetadata x, SearchMetadata y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                if (object.ReferenceEquals(x, null)) return false;
                if (object.ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                var stringComparison = x.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return string.Equals(x.SearchText, y.SearchText, stringComparison) && x.Highlight == y.Highlight && x.HighlightHue == y.HighlightHue && x.IconKind == y.IconKind && x.UseRegex == y.UseRegex && x.Position == y.Position && x.IgnoreCase == y.IgnoreCase;
            }
            public int GetHashCode(SearchMetadata obj)
            {
                unchecked
                {
                    var comparer = obj.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                    var hashCode = obj.SearchText != null ? comparer.GetHashCode(obj.SearchText) : 0;
                    hashCode = (hashCode * 397) ^ obj.Highlight.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.HighlightHue.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IconKind.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.UseRegex.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IgnoreCase.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Position.GetHashCode();
                    return hashCode;
                }
            }
            #endregion
        }
        private sealed class SearchTextEqualityComparer : IEqualityComparer<SearchMetadata>
        {
            #region Methods
            public bool Equals(SearchMetadata x, SearchMetadata y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                if (object.ReferenceEquals(x, null)) return false;
                if (object.ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.SearchText, y.SearchText);
            }
            public int GetHashCode(SearchMetadata obj) => obj.SearchText?.GetHashCode() ?? 0;
            #endregion
        }
        #endregion
    }
}