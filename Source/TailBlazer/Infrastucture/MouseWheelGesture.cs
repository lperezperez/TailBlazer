namespace TailBlazer.Infrastucture
{
    using System.Windows.Input;
    public class MouseWheelGesture : MouseGesture
    {
        #region Constructors
        public MouseWheelGesture()
            : base(MouseAction.WheelClick) { }
        public MouseWheelGesture(ModifierKeys modifiers)
            : base(MouseAction.WheelClick, modifiers) { }
        #endregion
        #region Enums
        public enum WheelDirection
        {
            None,
            Up,
            Down
        }
        #endregion
        #region Properties
        public static MouseWheelGesture ControlDown => new MouseWheelGesture(ModifierKeys.Control) { Direction = WheelDirection.Down };
        public static MouseWheelGesture ControlUp => new MouseWheelGesture(ModifierKeys.Control) { Direction = WheelDirection.Up };
        public WheelDirection Direction { get; set; }
        #endregion
        #region Methods
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!base.Matches(targetElement, inputEventArgs)) return false;
            if (!(inputEventArgs is MouseWheelEventArgs)) return false;
            var args = (MouseWheelEventArgs)inputEventArgs;
            switch (this.Direction)
            {
                case WheelDirection.None:
                    return args.Delta == 0;
                case WheelDirection.Up:
                    return args.Delta > 0;
                case WheelDirection.Down:
                    return args.Delta < 0;
                default:
                    return false;
            }
        }
        #endregion
    }
}