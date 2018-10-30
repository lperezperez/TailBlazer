namespace TailBlazer.Fixtures
{
    using System;
    using Microsoft.Reactive.Testing;
    public static class TextEx
    {
        #region Methods
        public static void AdvanceByMilliSeconds(this TestScheduler source, int seconds)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            source.AdvanceBy(TimeSpan.FromMilliseconds(seconds).Ticks);
        }
        public static void AdvanceBySeconds(this TestScheduler source, int seconds)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            source.AdvanceBy(TimeSpan.FromSeconds(seconds).Ticks);
        }
        #endregion
    }
}