namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TailBlazer.Domain.Annotations;
    public class ExludedLinesProvider : ILineProvider
    {
        #region Fields
        private readonly Func<string, bool> _itemsToExclude;
        private readonly ILineProvider _lines;
        #endregion
        #region Constructors
        public ExludedLinesProvider([NotNull] ILineProvider lines, Func<string, bool> itemsToExclude)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));
            this._lines = lines;
            this._itemsToExclude = itemsToExclude;
            this.Count = lines.Count;
        }
        #endregion
        #region Properties
        public int Count { get; }
        #endregion
        #region Methods
        public IEnumerable<Line> ReadLines(ScrollRequest scroll) { return this.ReadLinesImpl(scroll).OrderBy(line => line.Index); }
        public IEnumerable<Line> ReadLinesImpl(ScrollRequest scroll)
        {
            var currentPage = this._lines.ReadLines(scroll).Where(line => !this._itemsToExclude(line.Text)).ToArray();
            foreach (var line in currentPage)
                yield return line;
            if (currentPage.Length == scroll.PageSize)
                yield break;
            var deficit = scroll.PageSize - currentPage.Length;

            //work backwards through the file until we have enough lines
            var traverseUpTail = this.YieldTail(scroll.PageSize, scroll.FirstIndex - scroll.PageSize, deficit);
            foreach (var line in traverseUpTail)
                yield return line;
        }
        private IEnumerable<Line> YieldTail(int pageSize, int firstIndex, int deficit)
        {
            if (firstIndex < 0)
                pageSize = pageSize + firstIndex;

            //ensure first index is non-zero
            firstIndex = Math.Max(0, firstIndex);
            return this._lines.ReadLines(new ScrollRequest(ScrollReason.User, pageSize, firstIndex)).Where(line => !this._itemsToExclude(line.Text)).Reverse().Take(deficit);
        }
        #endregion
    }
}