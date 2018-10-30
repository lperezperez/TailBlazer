﻿namespace TailBlazer.Fixtures
{
    using System.Linq;
    using FluentAssertions;
    using TailBlazer.Views.Formatting;
    using Xunit;
    public class DefaultColourSelectorFixture
    {
        #region Methods
        [Fact]
        public void DefaultColourSelectorLookupShouldWork()
        {
            var provider = new ColourProvider();
            var selector = new DefaultColourSelector(provider);
            var key = provider.Hues.First().Key;
            var result = selector.Lookup(key);
            result.Key.Should().Be(key);
        }
        [Fact]
        public void DefaultColourSelectorSelectShouldWork()
        {
            var provider = new ColourProvider();
            var selector = new DefaultColourSelector(provider);
            var result = selector.Select("DEBUG");
            result.Should().NotBeNull();
        }
        #endregion
    }
}