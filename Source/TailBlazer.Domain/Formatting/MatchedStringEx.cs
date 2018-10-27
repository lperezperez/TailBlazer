namespace TailBlazer.Domain.Formatting
{
    using System.Collections.Generic;
    using System.Linq;
    using TailBlazer.Domain.FileHandling.Search;
    public static class MatchedStringEx
    {
        #region Methods
        public static IEnumerable<MatchedString> MatchString(this string source, string textToMatch) => new MatchedStringEnumerator(source, textToMatch);
        public static IEnumerable<MatchedString> MatchString(this string source, IEnumerable<string> itemsToMatch) => new MatchedStringEnumerator(source, itemsToMatch);

        //public static IEnumerable<MatchedString> MatchString(this string source, SearchMetadata itemsToMatch)
        //{
        //    return new SearchMetadataEnumerator(source, new []{ itemsToMatch });
        //}
        public static IEnumerable<MatchedString> MatchString(this string source, IEnumerable<SearchMetadata> itemsToMatch) => new SearchMetadataEnumerator(source, itemsToMatch).ToArray();
        #endregion
    }
}