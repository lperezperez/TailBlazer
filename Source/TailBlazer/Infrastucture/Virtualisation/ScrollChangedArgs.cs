namespace TailBlazer.Infrastucture.Virtualisation
{
    public class ScrollChangedArgs
    {
        #region Constructors
        public ScrollChangedArgs(ScrollDirection scrollDirection, int value)
        {
            this.Direction = scrollDirection;
            this.Value = value;
        }
        #endregion
        #region Properties
        public ScrollDirection Direction { get; }
        public int Value { get; }
        #endregion
    }
}