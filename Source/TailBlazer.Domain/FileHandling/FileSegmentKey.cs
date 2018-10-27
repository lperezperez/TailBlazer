namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Collections.Generic;
    using DynamicData.Binding;
    public struct FileSegmentKey : IEquatable<FileSegmentKey>, IComparable<FileSegmentKey>
    {
        #region Fields
        public static readonly FileSegmentKey Tail = new FileSegmentKey(-1, FileSegmentType.Tail);
        private static readonly IComparer<FileSegmentKey> DefaultOrder = SortExpressionComparer<FileSegmentKey>.Ascending(fsk => fsk._type == FileSegmentType.Head ? 1 : 2).ThenByAscending(fsk => fsk._value);
        private readonly FileSegmentType _type;
        private readonly int _value;
        #endregion
        #region Constructors
        public FileSegmentKey(int index, FileSegmentType type)
        {
            this._type = type;
            if (type == FileSegmentType.Tail)
            {
                this._value = -1;
            }
            else
            {
                if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
                this._value = index;
            }
        }
        #endregion
        #region Methods
        public static bool operator ==(FileSegmentKey left, FileSegmentKey right) => left.Equals(right);
        public static bool operator !=(FileSegmentKey left, FileSegmentKey right) => !left.Equals(right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            return obj is FileSegmentKey && this.Equals((FileSegmentKey)obj);
        }
        public override int GetHashCode() => this._value;
        public override string ToString()
        {
            if (this._type == FileSegmentType.Tail)
                return "Tail";
            return $"Head: {this._value}";
        }
        public int CompareTo(FileSegmentKey other) => DefaultOrder.Compare(this, other);
        public bool Equals(FileSegmentKey other) => this._value == other._value;
        #endregion
    }
}