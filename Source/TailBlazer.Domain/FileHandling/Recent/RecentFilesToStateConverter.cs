namespace TailBlazer.Domain.FileHandling.Recent
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using TailBlazer.Domain.Settings;
    public class RecentFilesToStateConverter : IConverter<RecentFile[]>
    {
        #region Methods
        public RecentFile[] Convert(State state)
        {
            if (state == null || state == State.Empty)
                return new RecentFile[0];

            //v1 format is csv
            if (state.Version == 1)
                return state.Value.FromDelimited(s => new RecentFile(new FileInfo(s)), Environment.NewLine).ToArray();

            //v2 format is xml
            var doc = XDocument.Parse(state.Value);
            var root = doc.ElementOrThrow(Structure.Root);
            var files = root.Elements(Structure.File).Select
                (
                 element =>
                     {
                         var name = element.Attribute(Structure.Name).Value;
                         var dateTime = element.Attribute(Structure.Date).Value;
                         return new RecentFile(DateTime.Parse(dateTime).ToUniversalTime(), name);
                     }).ToArray();
            return files;
        }
        public State Convert(RecentFile[] files)
        {
            if (files == null || !files.Any())
                return State.Empty;
            var root = new XElement(new XElement(Structure.Root));
            var fileNodeArray = files.Select(f => new XElement(Structure.File, new XAttribute(Structure.Name, f.Name), new XAttribute(Structure.Date, f.Timestamp)));
            fileNodeArray.ForEach(root.Add);
            var doc = new XDocument(root);
            return new State(2, doc.ToString());
        }
        public RecentFile[] GetDefaultValue() => new RecentFile[0];
        #endregion
        #region Classes
        private static class Structure
        {
            #region Constants
            public const string Date = "Date";
            public const string File = "File";
            public const string Name = "Name";
            public const string Root = "Files";
            #endregion
        }
        #endregion
    }
}