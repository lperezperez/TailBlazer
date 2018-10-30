namespace TailBlazer.Domain.StateHandling
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Settings;
    public class StateBucketService : IDisposable, IStateBucketService
    {
        #region Constants
        private const string SettingStoreKey = "StateBucket";
        #endregion
        #region Fields
        private readonly ISourceCache<StateBucket, StateBucketKey> _cache = new SourceCache<StateBucket, StateBucketKey>(s => s.Key);
        private readonly IDisposable _cleanUp;
        #endregion
        #region Constructors
        public StateBucketService(ISettingsStore store)
        {
            var converter = new StateBucketConverter();
            var loading = false;
            var writer = this._cache.Connect().ToCollection().Select(buckets => converter.Convert(buckets.ToArray())).Subscribe
                (
                 state =>
                     {
                         if (loading) return;
                         store.Save(SettingStoreKey, state);
                     });

            //TODO: Make this error proof
            var initialState = store.Load(SettingStoreKey);
            var initialBuckets = converter.Convert(initialState);
            try
            {
                loading = true;
                this._cache.AddOrUpdate(initialBuckets);
            }
            finally
            {
                loading = false;
            }
            this._cleanUp = new CompositeDisposable(writer, this._cache);
        }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        public Optional<State> Lookup(string type, string id) { return this._cache.Lookup(new StateBucketKey(type, id)).Convert(container => container.State); }
        public void Write(string type, string id, State state) { this._cache.AddOrUpdate(new StateBucket(type, id, state, DateTime.UtcNow)); }
        #endregion
    }
}