namespace TailBlazer.Domain.FileHandling
{
    using System;
    public class TailInfo
    {
        #region Fields
        public static readonly TailInfo None = new TailInfo();
        #endregion
        #region Constructors
        public TailInfo(long tailStartsAt)
        {
            this.TailStartsAt = tailStartsAt;
            this.LastTail = DateTime.UtcNow;
        }
        private TailInfo() { }
        #endregion
        #region Properties
        public DateTime LastTail { get; }
        public long TailStartsAt { get; }
        #endregion
    }
}