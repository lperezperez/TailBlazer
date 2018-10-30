namespace TailBlazer.Fixtures
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    public class TestFile : IDisposable
    {
        #region Constructors
        public TestFile()
        {
            this.Name = Path.GetTempFileName();
            this.Info = new FileInfo(this.Name);
        }
        #endregion
        #region Properties
        public FileInfo Info { get; }
        public string Name { get; }
        #endregion
        #region Methods
        public void Append(IEnumerable<string> lines) { File.AppendAllLines(this.Name, lines); }
        public void Append(string line) { File.AppendAllLines(this.Name, new[] { line }); }
        public void Create() { File.Create(this.Name); }
        public void Delete() { File.Delete(this.Name); }
        public void Dispose() { File.Delete(this.Name); }
        #endregion
    }
}