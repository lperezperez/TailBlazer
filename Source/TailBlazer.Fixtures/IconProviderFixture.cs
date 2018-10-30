namespace TailBlazer.Fixtures
{
    using System.Linq;
    using FluentAssertions;
    using TailBlazer.Views.Formatting;
    using Xunit;
    public class IconProviderFixture
    {
        #region Methods
        [Fact]
        public void IconProviderShouldHaveIcons()
        {
            using (var provider = new IconProvider(new DefaultIconSelector()))
            {
                var result = provider.Icons;
                result.Items.Any().Should().BeTrue();
            }
        }
        #endregion
    }
}