namespace TailBlazer.Domain.Ratings
{
    using System;
    using System.Reactive.Linq;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Settings;
    public class RatingService : IRatingService
    {
        #region Constructors
        public RatingService(ISetting<GeneralOptions> setting, IRatingsMetricsLookup ratingMetricsLookup) { this.Metrics = setting.Value.Select(options => options.Rating).DistinctUntilChanged().Select(ratingMetricsLookup.Lookup); }
        #endregion
        #region Properties
        public IObservable<RatingsMetaData> Metrics { get; }
        #endregion
    }
}