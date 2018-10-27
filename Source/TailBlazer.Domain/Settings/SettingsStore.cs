namespace TailBlazer.Domain.Settings
{
    using System;
    using System.IO;
    using System.Xml.Linq;
    using TailBlazer.Domain.Infrastructure;
    public class FileSettingsStore : ISettingsStore
    {
        #region Fields
        private readonly ILogger _logger;
        #endregion
        #region Constructors
        public FileSettingsStore(ILogger logger)
        {
            this._logger = logger;
            this.Location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TailBlazer");
            var dir = new DirectoryInfo(this.Location);
            if (!dir.Exists) dir.Create();
            this._logger.Info($"Settings folder is {this.Location}");
        }
        #endregion
        #region Properties
        private string Location { get; }
        #endregion
        #region Methods
        public State Load(string key)
        {
            this._logger.Info($"Reading setting for {key}");
            var file = Path.Combine(this.Location, $"{key}.setting");
            var info = new FileInfo(file);
            if (!info.Exists || info.Length == 0) return State.Empty;
            var doc = XDocument.Load(file);
            var root = doc.ElementOrThrow("Setting");
            var versionString = root.AttributeOrThrow("Version");
            var version = int.Parse(versionString);
            var state = root.ElementOrThrow("State");

            //  _logger.Debug($"{key} has the value {state}");
            return new State(version, state);
        }
        public void Save(string key, State state)
        {
            var file = Path.Combine(this.Location, $"{key}.setting");
            this._logger.Info($"Creating setting for {key}");
            var root = new XElement(new XElement(Structure.Root, new XAttribute(Structure.Version, state.Version)));
            root.Add(new XElement(Structure.State, state.Value));
            var doc = new XDocument(root);
            var fileText = doc.ToString();
            this._logger.Info($"Writing settings for {key} to {file}");
            File.WriteAllText(file, fileText);
            this._logger.Info($"Setting  for {key} committed");
        }
        #endregion
        #region Classes
        private static class Structure
        {
            #region Constants
            public const string Root = "Setting";
            public const string State = "State";
            public const string Version = "Version";
            #endregion
        }
        #endregion
    }
}