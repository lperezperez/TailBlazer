namespace TailBlazer.Domain.Settings
{
    using System;
    using TailBlazer.Domain.Annotations;
    public class ViewState
    {
        #region Constructors
        public ViewState([NotNull] string key, State state = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            this.Key = key;
            this.State = state;
        }
        #endregion
        #region Properties
        public string Key { get; }
        public State State { get; }
        #endregion
    }
}