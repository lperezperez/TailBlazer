namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using System.Text.RegularExpressions;
    public class RegexInspector
    {
        #region Fields
        private readonly Regex _isPlainText;
        #endregion
        #region Constructors
        public RegexInspector() { this._isPlainText = new Regex("^[a-zA-Z0-9 ]*$"); }
        #endregion
        #region Methods
        public bool DoesThisLookLikeRegEx(string text)
        {
            var withNegation = text.WithNegation();
            return !string.IsNullOrEmpty(text) && !this._isPlainText.IsMatch(withNegation.Text);
        }
        #endregion
    }
}