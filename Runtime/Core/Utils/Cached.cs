using System;

namespace SolFrame.Utils
{
    /// <summary>
    ///   A value cache
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class Cached<T>
    {
        private T value;
        private int cachedEpoch = 0;
        private DateTime lastUpdated;

        public T Value => value;

        public DateTime LastUpdated => lastUpdated;

        public int CachedEpoch => cachedEpoch;

        public Cached() : this(default)
        { }

        public Cached(T value)
        {
            this.value = value;
        }

        public static implicit operator T(Cached<T> cached) => cached.value;

        public void Clear()
        {
            cachedEpoch = 0;
            value = default;
            lastUpdated = DateTime.Now;
        }

        public void Update(T newVal)
        {
            value = newVal;
            cachedEpoch++;
            lastUpdated = DateTime.Now;
        }
    }
}