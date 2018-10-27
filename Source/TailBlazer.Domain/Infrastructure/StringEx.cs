// ReSharper disable once CheckNamespace
namespace System
{
    public static class StringEx
    {
        #region Methods
        public static bool Contains(this string source, string toCheck, StringComparison comp) => source.IndexOf(toCheck, comp) >= 0;

        //// Based on http://stackoverflow.com/a/11124118/619960
        //// Credit to user humbads 
        public static string FormatWithAbbreviation(this long source)
        {
            // Get absolute value
            var absolute_i = source < 0 ? -source : source;

            // Determine the suffix and readable value
            string suffix;
            double len;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                len = source >> 50;
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                len = source >> 40;
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                len = source >> 30;
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                len = source >> 20;
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                len = source >> 10;
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                len = source;
            }
            else
            {
                return source.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            len = len / 1024;
            // Return formatted number with suffix
            return $"{len:0.##} {suffix}";
        }
        public static bool IsLongerThan(this string source, int length) => !string.IsNullOrEmpty(source) && source.Length > length;
        public static bool IsLongerThanOrEqualTo(this string source, int length) => !string.IsNullOrEmpty(source) && source.Length >= length;
        public static bool NextBoolean(this Random source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.NextDouble() > 0.5;
        }
        public static string Pluralise(this string source, int count) => count == 1 ? $"{count} {source}" : $"{count} {source}s";
        public static StringWithNegation WithNegation(this string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return StringWithNegation.Empty;
            var isExclusion = searchText.Substring(0, 1) == "-";
            if (!isExclusion)
                return new StringWithNegation(false, searchText);
            return new StringWithNegation(true, searchText.Substring(1, searchText.Length - 1));
        }
        #endregion
        #region Classes
        public sealed class StringWithNegation
        {
            #region Fields
            public static readonly StringWithNegation Empty = new StringWithNegation(false, string.Empty);
            #endregion
            #region Constructors
            public StringWithNegation(bool isNegation, string text)
            {
                this.IsNegation = isNegation;
                this.Text = text;
            }
            #endregion
            #region Properties
            public bool IsNegation { get; }
            public string Text { get; }
            #endregion
        }
        #endregion
    }
}