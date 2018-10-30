namespace TailBlazer.Domain.FileHandling
{
    public sealed class Page
    {
        #region Constructors
        public Page(int start, int size)
        {
            this.Start = start;
            this.Size = size;
        }
        #endregion
        #region Properties
        public int Size { get; }
        public int Start { get; }
        #endregion
    }
}