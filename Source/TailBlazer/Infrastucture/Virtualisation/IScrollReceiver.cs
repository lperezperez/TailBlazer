namespace TailBlazer.Infrastucture.Virtualisation
{
    public interface IScrollReceiver
    {
        #region Methods
        void ScrollBoundsChanged(ScrollBoundsArgs boundsArgs);
        void ScrollChanged(ScrollChangedArgs scrollChangedArgs);
        void ScrollDiff(int lineChanged);
        #endregion
    }
}