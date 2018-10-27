namespace TailBlazer.Infrastucture.Virtualisation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Formatting;
    public static class TextScrollEx
    {
        #region Methods
        public static string Virtualise(this string source, TextScrollInfo scroll)
        {
            if (scroll == null || scroll.TotalChars == 0)
                return source;
            return new string(source.Skip(scroll.FirstIndex).Take(scroll.TotalChars).ToArray());
        }
        public static IEnumerable<DisplayText> Virtualise(this IEnumerable<DisplayText> source, TextScrollInfo scroll)
        {
            var items = source.AsArray();
            if (scroll == null || scroll.TotalChars == 0)
                return items;

            // var list = new List<DisplayText>(items.Length);
            var lastIndex = scroll.FirstIndex + scroll.TotalChars;
            var displayBounds = items.Aggregate
                (
                 new List<DisplayWithIndex>(),
                 (state, latest) =>
                     {
                         if (state.Count == 0)
                         {
                             state.Add(new DisplayWithIndex(latest, 0));
                         }
                         else
                         {
                             var last = state.Last();
                             state.Add(new DisplayWithIndex(latest, last.StartIndex + last.Text.Length));
                         }
                         return state;
                     }).ToArray();
            var result = displayBounds.Select
                (
                 item =>
                     {
                         if (item.Inbounds(scroll.FirstIndex, lastIndex))
                             return item.Clip(scroll.FirstIndex, lastIndex);
                         return null;
                     }).Where(item => item != null).ToArray();
            return result;
        }
        #endregion
        #region Classes
        private class DisplayWithIndex
        {
            #region Constructors
            public DisplayWithIndex(DisplayText text, int startIndex)
            {
                this.Text = text;
                this.StartIndex = startIndex;
            }
            #endregion
            #region Properties
            public int EndIndex => this.StartIndex + this.Text.Length;
            public int StartIndex { get; }
            public DisplayText Text { get; }
            #endregion
            #region Methods
            public override string ToString() => $"{this.Text}, {this.StartIndex}->{this.EndIndex}";
            public DisplayText Clip(int start, int lastIndex)
            {
                var clippedStart = Math.Max(start - this.StartIndex, 0);
                var maxLength = lastIndex - this.StartIndex;
                var clippedLength = Math.Min(maxLength, this.Text.Length - clippedStart);
                var clipped = this.Text.Text.Substring(clippedStart, clippedLength);
                return new DisplayText(this.Text, clipped);
            }
            public bool Inbounds(int start, int end) => start <= this.EndIndex && end >= this.StartIndex;
            #endregion
        }
        #endregion
    }
}