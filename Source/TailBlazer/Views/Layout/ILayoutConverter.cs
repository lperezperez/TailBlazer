namespace TailBlazer.Views.Layout
{
    using System.Xml.Linq;
    public interface ILayoutConverter
    {
        #region Methods
        XElement CaptureState();
        void Restore(XElement element);
        #endregion
    }
}