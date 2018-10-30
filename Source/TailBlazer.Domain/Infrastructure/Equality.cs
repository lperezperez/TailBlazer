namespace TailBlazer.Domain.Infrastructure
{
    using System;
    using System.Collections.Generic;
    public static class Equality
    {
        #region Methods
        public static IEqualityComparer<T> AndOn<T, TValue>(this IEqualityComparer<T> rootComparer, Func<T, TValue> valueSelector) { return new GenericEqualityComparer<T>(rootComparer, (t1, t2) => EqualityComparer<TValue>.Default.Equals(valueSelector(t1), valueSelector(t2)), t => EqualityComparer<TValue>.Default.GetHashCode(valueSelector(t))); }
        public static IEqualityComparer<T> CompareOn<T, TValue>(Func<T, TValue> valueSelector) { return new GenericEqualityComparer<T>((t1, t2) => EqualityComparer<TValue>.Default.Equals(valueSelector(t1), valueSelector(t2)), t => EqualityComparer<TValue>.Default.GetHashCode(valueSelector(t))); }
        public static IEqualityComparer<T> Create<T, TValue>(Func<T, TValue> projection) { return new GenericEqualityComparer<T>((t1, t2) => EqualityComparer<TValue>.Default.Equals(projection(t1), projection(t2)), t => EqualityComparer<TValue>.Default.GetHashCode(projection(t))); }
        #endregion
    }
    public class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        #region Fields
        private readonly Func<T, T, bool> _compareFunction;
        private readonly Func<T, int> _hashFunction;
        #endregion
        #region Constructors
        public GenericEqualityComparer(IEqualityComparer<T> rootComparer, Func<T, T, bool> compareFunction, Func<T, int> hashFunction)
        {
            this._hashFunction = t =>
                {
                    unchecked
                    {
                        var hashCode = rootComparer.GetHashCode(t);
                        hashCode = (hashCode * 397) ^ hashFunction(t);
                        return hashCode;
                    }
                };
            this._compareFunction = (x, y) => rootComparer.Equals(x, y) && compareFunction(x, y);
        }
        public GenericEqualityComparer(Func<T, T, bool> compareFunction, Func<T, int> hashFunction)
        {
            this._compareFunction = compareFunction;
            this._hashFunction = hashFunction;
        }
        #endregion
        #region Methods
        public bool Equals(T x, T y) => this._compareFunction(x, y);
        public int GetHashCode(T obj) => this._hashFunction(obj);
        #endregion
    }
}