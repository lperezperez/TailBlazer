namespace TailBlazer.Domain.Formatting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using DynamicData.Kernel;
    using TailBlazer.Domain.FileHandling.Search;
    public class MatchedStringEnumerator : IEnumerable<MatchedString>
    {
        #region Fields
        private readonly string _input;
        private readonly IEnumerable<string> _itemsToMatch;
        private readonly string _textToMatch;
        #endregion
        #region Constructors
        public MatchedStringEnumerator(string input, string textToMatch)
        {
            if (textToMatch == null) throw new ArgumentNullException(nameof(textToMatch));
            this._input = input;
            this._textToMatch = textToMatch;
        }
        public MatchedStringEnumerator(string input, IEnumerable<string> itemsToMatch)
        {
            this._input = input;
            this._itemsToMatch = itemsToMatch;
        }
        #endregion
        #region Methods
        private static IEnumerable<MatchedString> Yield(string input, string tomatch)
        {
            if (string.IsNullOrEmpty(input))
                yield break;

            //TODO: Check whether there are perf-issues with RegEx
            var split = Regex.Split(input, tomatch, RegexOptions.IgnoreCase);
            var length = split.Length;
            if (length == 0) yield break;
            if (length == 1)
            {
                yield return new MatchedString(input);
                yield break;
            }

            //  int start =0;
            var currentLength = 0;
            for (var i = 0; i < split.Length; i++)
            {
                var current = split[i] ?? string.Empty;
                if (string.IsNullOrEmpty(current))
                {
                    //Get original string back as the user may have searched in a different case
                    var originalString = input.Substring(currentLength, tomatch.Length);
                    yield return new MatchedString(originalString, true);
                    currentLength = current.Length + currentLength + tomatch.Length;
                    if (currentLength + tomatch.Length > input.Length)
                        yield break;
                }
                else if (i > 0 && !string.IsNullOrEmpty(split[i - 1]))
                {
                    if (currentLength + tomatch.Length > input.Length)
                        yield break;

                    //Get original string back as the user may have searched in a different case
                    var originalString = input.Substring(currentLength, tomatch.Length);
                    yield return new MatchedString(originalString, true);
                    yield return new MatchedString(current);
                    currentLength = current.Length + currentLength + tomatch.Length;
                }
                else
                {
                    yield return new MatchedString(current);
                    currentLength = current.Length + currentLength;
                }
            }
        }
        public IEnumerator<MatchedString> GetEnumerator()
        {
            if (this._textToMatch != null)
            {
                foreach (var result in MatchedStringEnumerator.Yield(this._input, this._textToMatch))
                    yield return result;
            }
            else
            {
                var strings = this._itemsToMatch.AsArray();
                var matches = new MatchedString[0];
                for (var i = 0; i < strings.Length; i++)
                {
                    var stringToMatch = strings[i];
                    if (i == 0)
                        matches = MatchedStringEnumerator.Yield(this._input, stringToMatch).ToArray();
                    else
                        matches = matches.SelectMany(ms => ms.IsMatch ? new[] { ms } : MatchedStringEnumerator.Yield(ms.Part, stringToMatch)).ToArray();
                }
                foreach (var matchedString in matches)
                    yield return matchedString;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        #endregion
    }
    public class MatchedStringEnumerator2 : IEnumerable<MatchedString>
    {
        #region Fields
        private readonly string _input;
        private readonly IEnumerable<string> _itemsToMatch;
        private readonly SearchMetadata _tomatch;
        #endregion
        #region Constructors
        public MatchedStringEnumerator2(string input, IEnumerable<string> itemsToMatch, SearchMetadata tomatch)
        {
            this._input = input;
            this._itemsToMatch = itemsToMatch;
            this._tomatch = tomatch;
        }
        #endregion
        #region Methods
        public IEnumerator<MatchedString> GetEnumerator()
        {
            var strings = this._itemsToMatch.AsArray();
            var matches = new MatchedString[0];
            for (var i = 0; i < strings.Length; i++)
            {
                var stringToMatch = strings[i];
                if (i == 0)
                    matches = this.Yield(this._input, stringToMatch).ToArray();
                else
                    matches = matches.SelectMany(ms => ms.IsMatch ? new[] { ms } : this.Yield(ms.Part, stringToMatch)).ToArray();
            }
            foreach (var matchedString in matches)
                yield return matchedString;
        }
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        private IEnumerable<MatchedString> Yield(string input, string tomatch)
        {
            if (string.IsNullOrEmpty(input))
                yield break;
            var pattern = "(" + Regex.Escape(tomatch) + ")";
            var split = Regex.Split(input, pattern, RegexOptions.IgnoreCase);
            var length = split.Length;
            if (length == 0) yield break;
            if (length == 1)
            {
                yield return new MatchedString(input);
                yield break;
            }
            foreach (var item in split)
                if (item.Equals(tomatch, StringComparison.OrdinalIgnoreCase))
                    yield return new MatchedString(item, this._tomatch);
                else
                    yield return new MatchedString(item);
        }
        #endregion
    }
}