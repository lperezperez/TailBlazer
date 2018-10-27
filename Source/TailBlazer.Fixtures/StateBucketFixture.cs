namespace TailBlazer.Fixtures
{
    using System;
    using FluentAssertions;
    using TailBlazer.Domain.Settings;
    using TailBlazer.Domain.StateHandling;
    using Xunit;
    public class StateBucketFixture
    {
        #region Methods
        [Fact]
        public void WriteState()
        {
            var converter = new StateBucketConverter();
            var buckets = new[] { new StateBucket("TestType1", "123", new State(1, "SomeThingOrOther"), DateTime.UtcNow.AddMinutes(-1)), new StateBucket("TestType2", "124", new State(1, "Type 2 State"), DateTime.UtcNow) };
            var state = converter.Convert(buckets);
            var restored = converter.Convert(state);
            buckets.Should().BeEquivalentTo(restored);
        }
        #endregion

        //[Fact]
        //public void WriteComplexState()
        //{
        //    var state = new State(1, "<<something weird<> which breaks xml {}");

        //    var store = new FileSettingsStore(new NullLogger());
        //    store.Save("wierdfile", state);

        //    var restored = store.Load("wierdfile");
        //    restored.Should().Be(state);
        //}
    }
}