namespace TailBlazer.Domain.Infrastructure
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using DynamicData;
    public class ImmutableList<T>
    {
        #region Fields
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();
        private readonly List<T> _data;
        #endregion
        #region Constructors
        public ImmutableList() { this._data = new List<T>(); }
        public ImmutableList(List<T> data) { this._data = new List<T>(data); }
        public ImmutableList(IReadOnlyList<T> data) { this._data = new List<T>(data); }
        public ImmutableList(IReadOnlyCollection<T> data) { this._data = new List<T>(data); }
        #endregion
        #region Properties
        public int Count => this._data.Count;
        public IReadOnlyCollection<T> Data => new ReadOnlyCollection<T>(this._data);
        #endregion
        #region Indexers
        public T this[int index] => this._data[index];
        #endregion
        #region Methods
        public ImmutableList<T> Add(T value)
        {
            var newData = new List<T>(this._data) { value };
            return new ImmutableList<T>(newData);
        }
        public ImmutableList<T> Add(T[] newItems)
        {
            var list = new List<T>(this._data);
            list.AddRange(newItems);
            return new ImmutableList<T>(list);
        }
        public ImmutableList<T> Add(IList<T> newItems)
        {
            var list = new List<T>(this._data);
            list.AddRange(newItems);
            return new ImmutableList<T>(list);
        }
        public ImmutableList<T> Add(ImmutableList<T> newItems)
        {
            var list = new List<T>(this._data);
            list.AddRange(newItems.Data);
            return new ImmutableList<T>(list);
        }
        public int IndexOf(T value) => this._data.IndexOf(value);
        public ImmutableList<T> Remove(T value)
        {
            var list = new List<T>(this._data);
            if (!list.Remove(value))
                return this;
            return new ImmutableList<T>(list);
        }
        public ImmutableList<T> Remove(IEnumerable<T> oldItems)
        {
            var list = new List<T>(this._data);
            list.RemoveMany(oldItems);
            return new ImmutableList<T>(list);
        }
        #endregion
    }
}