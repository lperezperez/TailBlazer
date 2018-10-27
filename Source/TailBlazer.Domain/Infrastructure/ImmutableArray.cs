namespace TailBlazer.Domain.Infrastructure
{
    using System;
    /// <summary>whipped straight from rx.net</summary>
    /// <typeparam name="T"></typeparam>
    internal class ImmutableArray<T>
    {
        #region Fields
        public static readonly ImmutableArray<T> Empty = new ImmutableArray<T>();
        #endregion
        #region Constructors
        public ImmutableArray() { this.Data = new T[0]; }
        public ImmutableArray(T[] data) { this.Data = data; }
        #endregion
        #region Properties
        public T[] Data { get; }
        #endregion
        #region Methods
        public ImmutableArray<T> Add(T value)
        {
            var newData = new T[this.Data.Length + 1];
            Array.Copy(this.Data, newData, this.Data.Length);
            newData[this.Data.Length] = value;
            return new ImmutableArray<T>(newData);
        }
        public ImmutableArray<T> Add(T[] newItems)
        {
            var result = new T[this.Data.Length + newItems.Length];
            this.Data.CopyTo(result, 0);
            newItems.CopyTo(result, this.Data.Length);
            return new ImmutableArray<T>(result);
        }
        public ImmutableArray<T> Remove(T value)
        {
            var i = this.IndexOf(value);
            if (i < 0)
                return this;
            var length = this.Data.Length;
            if (length == 1)
                return Empty;
            var newData = new T[length - 1];
            Array.Copy(this.Data, 0, newData, 0, i);
            Array.Copy(this.Data, i + 1, newData, i, length - i - 1);
            return new ImmutableArray<T>(newData);
        }
        private int IndexOf(T value)
        {
            for (var i = 0; i < this.Data.Length; ++i)
                if (object.Equals(this.Data[i], value))
                    return i;
            return -1;
        }
        #endregion
    }
}