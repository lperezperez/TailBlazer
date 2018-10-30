namespace TailBlazer.Infrastucture.Virtualisation
{
    public class ScrollBoundsArgs
    {
        #region Constructors
        public ScrollBoundsArgs(int pageSize, int firstIndex)
        {
            this.PageSize = pageSize;
            this.FirstIndex = firstIndex;
        }
        #endregion
        #region Properties
        public int FirstIndex { get; }
        public int PageSize { get; }
        #endregion
    }
}