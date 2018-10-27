namespace TailBlazer.Infrastucture
{
    using System.IO;
    using DynamicData.Binding;
    public class FileHeader : AbstractNotifyPropertyChanged
    {
        #region Fields
        private readonly FileInfo _info;
        private string _displayName;
        #endregion
        #region Constructors
        public FileHeader(FileInfo info)
        {
            this._info = info;
            this._displayName = info.Name;
        }
        #endregion
        #region Properties
        public string DisplayName { get => this._displayName; set => this.SetAndRaise(ref this._displayName, value); }
        public string FullName => this._info.FullName;
        #endregion
    }
}