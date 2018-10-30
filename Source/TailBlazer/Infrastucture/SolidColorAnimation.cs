namespace TailBlazer.Infrastucture
{
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    public class SolidColorAnimation : ColorAnimation
    {
        #region Properties
        public SolidColorBrush FromBrush { get => this.From == null ? null : new SolidColorBrush(this.From.Value); set => this.From = value?.Color; }
        public SolidColorBrush ToBrush { get => this.To == null ? null : new SolidColorBrush(this.To.Value); set => this.To = value?.Color; }
        #endregion
    }
}