namespace TailBlazer.Domain.Formatting
{
    using System.Linq;
    using DynamicData.Binding;
    public class LineMatchCollection : AbstractNotifyPropertyChanged
    {
        #region Fields
        public static readonly LineMatchCollection Empty = new LineMatchCollection(new LineMatch[0]);
        #endregion
        #region Constructors
        public LineMatchCollection(LineMatch[] matches)
        {
            this.Matches = matches;
            this.FirstMatch = matches.FirstOrDefault();
        }
        #endregion
        #region Properties
        public int Count => this.Matches.Length;
        public LineMatch FirstMatch { get; }
        public bool HasMatches => this.Matches.Length != 0;
        public bool IsFilter => this.FirstMatch != null && !this.FirstMatch.UseRegex;
        public bool IsRegex => this.FirstMatch != null && this.FirstMatch.UseRegex;
        public LineMatch[] Matches { get; }
        #endregion
    }
}