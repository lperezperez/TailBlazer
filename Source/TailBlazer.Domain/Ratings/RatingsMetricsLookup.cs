namespace TailBlazer.Domain.Ratings
{
    using System.Collections.Generic;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Settings;
    public class RatingsMetricsLookup : IRatingsMetricsLookup
    {
        #region Constructors
        public RatingsMetricsLookup()
        {
            this.RatingMetrics = new Dictionary<int, RatingsMetaData>
                                     {
                                         [1] = new RatingsMetaData(30, 1000), [2] = new RatingsMetaData(30, 750), [3] = new RatingsMetaData(45, 600), [4] = new RatingsMetaData(60, 400),
                                         [5] = new RatingsMetaData(60, 250)
                                     };
        }
        #endregion
        #region Properties
        private IDictionary<int, RatingsMetaData> RatingMetrics { get; }
        #endregion
        #region Methods
        public RatingsMetaData Lookup(int rating) { return this.RatingMetrics.Lookup(rating).ValueOr(() => RatingsMetaData.Default); }
        #endregion
    }
}