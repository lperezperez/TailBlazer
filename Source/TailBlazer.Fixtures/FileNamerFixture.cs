namespace TailBlazer.Fixtures
{
    using System.IO;
    using System.Linq;
    using TailBlazer.Infrastucture;
    using Xunit;
    public class FileNamerFixture
    {
        #region Methods
        [Fact]
        public void ReturnsCorrectDistinctPath()
        {
            var paths = new[] { Path.Combine("logger.log"), Path.Combine("Debug", "logger.log"), Path.Combine("bin", "Debug", "logger.log"), Path.Combine("C:\\", "App", "bin", "Debug", "logger.log"), Path.Combine("D:\\", "App", "bin", "Debug", "logger.log"), Path.Combine("C:\\", "App", "bin", "Release", "logger.log"), Path.Combine("C:\\", "App", "obj", "Release", "logger.log") };
            var expected = new[] { Path.Combine("logger.log"), Path.Combine("Debug", "logger.log"), Path.Combine("bin", "..", "logger.log"), Path.Combine("C:\\", "..", "logger.log"), Path.Combine("D:\\", "..", "logger.log"), Path.Combine("bin", "..", "logger.log"), Path.Combine("obj", "..", "logger.log") };
            var trie = new FileNamer(paths);
            var result = paths.Select(path => trie.GetName(path)).ToArray();
            Assert.Equal(expected, result);
        }
        #endregion
    }
}