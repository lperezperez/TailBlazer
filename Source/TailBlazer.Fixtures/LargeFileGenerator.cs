﻿namespace TailBlazer.Fixtures
{
    using System.IO;
    using System.Linq;
    using System.Text;
    public class LargeFileGenerator
    {
        #region Methods
        public void AddToFile() { }
        //  [Fact]
        public void GenerateFile()
        {
            var fileName = @"U:\Large Files\SuperGiantFile.txt";

            //var file = File.Create(@"U:\GigFile.txt");
            for (var i = 0; i < 1000; i++)
            {
                var start = 1000000 * i + 1;
                File.AppendAllLines(fileName, Enumerable.Range(start, 1000000).Select(line => $"This is line number {line.ToString("0000000000")} in a very large file"));
            }
        }

        //  [Fact]
        public void GenerateWideLinesInFile()
        {
            // string fileName = @"U:\Large Files\WideFile.txt";
            var fileName = @"c:\work\LargeFiles\WideFile.txt";
            //s var file = File.Create();
            var template = "0123456789abcdefghijklmnopqrstuvwxyz";
            var sb = new StringBuilder();
            for (var i = 0; i < 1000; i++)
            {
                for (var j = 0; i < 200; i++)
                {
                    sb.Append(j);
                    sb.Append("_");
                    sb.Append(template);
                }
                File.AppendAllLines(fileName, new[] { sb.Append(i).ToString() });
            }
        }
        #endregion
    }
}