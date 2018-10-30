namespace TailBlazer.Views.Recent
{
    using System;
    using System.Windows.Input;
    using DynamicData.Binding;
    using TailBlazer.Domain.FileHandling.Recent;
    using TailBlazer.Infrastucture;
    public class RecentFileProxy : AbstractNotifyPropertyChanged, IEquatable<RecentFileProxy>
    {
        #region Fields
        private readonly RecentFile _recentFile;
        private string _label;
        #endregion
        #region Constructors
        public RecentFileProxy(RecentFile recentFile, Action<RecentFile> openAction, Action<RecentFile> removeAction)
        {
            this._recentFile = recentFile;
            this.OpenCommand = new Command(() => openAction(recentFile));
            this.RemoveCommand = new Command(() => removeAction(recentFile));
        }
        #endregion
        #region Properties
        public string Label { get => this._label; set => this.SetAndRaise(ref this._label, value); }
        public string Name => this._recentFile.Name;
        public ICommand OpenCommand { get; }
        public string OpenToolTip => $"Open {this._recentFile.Name}";
        public ICommand RemoveCommand { get; }
        public string RemoveToolTip => $"Remove {this._recentFile.Name} from recent list";
        public DateTime Timestamp => this._recentFile.Timestamp;
        #endregion
        #region Methods
        public static bool operator ==(RecentFileProxy left, RecentFileProxy right) => object.Equals(left, right);
        public static bool operator !=(RecentFileProxy left, RecentFileProxy right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RecentFileProxy)obj);
        }
        public override int GetHashCode() => this._recentFile?.GetHashCode() ?? 0;
        public override string ToString() => $"{this.Name}";
        public bool Equals(RecentFileProxy other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this._recentFile, other._recentFile);
        }
        #endregion
    }
}