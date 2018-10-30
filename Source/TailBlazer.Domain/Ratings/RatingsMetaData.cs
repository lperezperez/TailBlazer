namespace TailBlazer.Domain.Settings
{
    public class RatingsMetaData
    {
        #region Fields
        public static readonly RatingsMetaData Default = new RatingsMetaData(60, 250);
        #endregion
        #region Constructors
        public RatingsMetaData(int frameRate, int refreshRate)
        {
            this.FrameRate = frameRate;
            this.RefreshRate = refreshRate;
        }
        #endregion
        #region Properties
        public int FrameRate { get; }
        public int RefreshRate { get; }
        #endregion
    }
}