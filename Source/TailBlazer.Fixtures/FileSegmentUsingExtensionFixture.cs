namespace TailBlazer.Fixtures
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Subjects;
    using FluentAssertions;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.Infrastructure;
    using Xunit;
    public class FileSegment_UsingExtensionFixture
    {
        #region Methods
        [Fact]
        public void ExistingFileChaned()
        {
            //need to make this test
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            File.AppendAllLines(file, Enumerable.Range(1, 10000).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());
            var refresher = new Subject<Unit>();
            FileSegmentCollection result = null;
            using (var indexer = info.WatchFile(refresher).WithSegments().Subscribe(segment => result = segment))
            {
                result.Should().NotBeNull();
                result.Count.Should().BeGreaterOrEqualTo(2);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Head);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Tail);
                result.FileLength.Should().Be(info.Length);
                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));
                refresher.Once();
                info.Refresh();
                result.FileLength.Should().Be(info.Length);
                File.Delete(file);
            }
            File.Delete(file);
        }
        [Fact]
        public void NewFileCreated()
        {
            //need to make this test
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            var refresher = new Subject<Unit>();
            FileSegmentCollection result = null;
            using (var indexer = info.WatchFile(refresher).WithSegments(1000).Subscribe(segment => result = segment))
            {
                result.Should().NotBeNull();
                File.AppendAllLines(file, Enumerable.Range(1, 10000).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());
                refresher.Once();
                result.Should().NotBeNull();
                result.Count.Should().BeGreaterOrEqualTo(2);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Head);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Tail);
                result.FileLength.Should().Be(info.Length);
                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));
                refresher.Once();
                info.Refresh();
                result.FileLength.Should().Be(info.Length);
                File.Delete(file);
            }
            File.Delete(file);
        }
        #endregion
    }
}