namespace TailBlazer.Views.Searching
{
    using System.ComponentModel;
    public class SearchHintMessage : INotifyPropertyChanged
    {
        #region Fields
        public static readonly SearchHintMessage Valid = new SearchHintMessage(true, null);
        #endregion
        #region Constructors
        public SearchHintMessage(bool isValid, string message)
        {
            this.IsValid = isValid;
            this.Message = message;
        }
        #endregion
        #region Events
        //implemented to prevent memory leaks
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region Properties
        public bool IsValid { get; }
        public string Message { get; }
        #endregion
    }
}