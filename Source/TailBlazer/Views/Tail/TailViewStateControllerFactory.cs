namespace TailBlazer.Views.Tail
{
    using System;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.StateHandling;
    public class TailViewStateControllerFactory : ITailViewStateControllerFactory
    {
        #region Fields
        private readonly ILogFactory _loggerFactory;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly IStateBucketService _stateBucketService;
        private readonly ITailViewStateRestorer _tailViewStateRestorer;
        #endregion
        #region Constructors
        public TailViewStateControllerFactory([NotNull] IStateBucketService stateBucketService, [NotNull] ISchedulerProvider schedulerProvider, [NotNull] ITailViewStateRestorer tailViewStateRestorer, [NotNull] ILogFactory loggerFactory)
        {
            this._stateBucketService = stateBucketService ?? throw new ArgumentNullException(nameof(stateBucketService));
            this._schedulerProvider = schedulerProvider ?? throw new ArgumentNullException(nameof(schedulerProvider));
            this._tailViewStateRestorer = tailViewStateRestorer ?? throw new ArgumentNullException(nameof(tailViewStateRestorer));
            this._loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }
        #endregion
        #region Methods
        public IDisposable Create(TailViewModel tailView, bool loadDefaults)
        {
            if (tailView == null) throw new ArgumentNullException(nameof(tailView));
            var logger = this._loggerFactory.Create<TailViewStateController>();
            return new TailViewStateController(tailView, this._stateBucketService, this._schedulerProvider, this._tailViewStateRestorer, logger, loadDefaults);
        }
        #endregion
    }
}