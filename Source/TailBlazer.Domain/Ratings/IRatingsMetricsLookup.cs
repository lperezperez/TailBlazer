namespace TailBlazer.Domain.Ratings
{
    using TailBlazer.Domain.Settings;
    public interface IRatingsMetricsLookup
    {
        #region Methods
        RatingsMetaData Lookup(int rating);
        #endregion
    }
}