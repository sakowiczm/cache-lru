namespace Cache
{
    /// <summary>
    /// InMemory cache class that implements LRU alghoritm.
    /// As per requirements it uses singletion pattern and locking for thread-safety.
    /// </summary>
    public class Cache<TKey, TValue> where TKey : notnull
    {
        int _threshold;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _items = new Dictionary<TKey, LinkedListNode<CacheItem>>();
        private readonly LinkedList<LinkedListNode<CacheItem>> _order = new LinkedList<LinkedListNode<CacheItem>>();

        public event Action<TKey, TValue>? OnItemEvicted;

        private static Lazy<Cache<TKey, TValue>>? _lazy = null;
        private readonly object cacheLock = new object();

        private Cache(int threshold)
        {
            if (threshold <= 0)
                throw new ArgumentOutOfRangeException(nameof(threshold));

            _threshold = threshold;
        }

        public static Cache<TKey, TValue> GetInstance(int threshold)
        {
            if (_lazy == null)
            {
                _lazy = new Lazy<Cache<TKey, TValue>>(() => new Cache<TKey, TValue>(threshold));
            }

            return _lazy.Value;
        }

        /// <summary>
        /// Returns tuple 
        /// - first value (bool) determines if key was found in the collection
        /// - second element is actual value if found or default if not
        /// </summary>
        public (bool Found, TValue? Value) Get(TKey key)
        {
            lock(cacheLock) 
            {
                if (!_items.ContainsKey(key))
                    return (false, default(TValue));

                var node = _items[key];

                // move to the top
                _order.Remove(node);
                _order.AddFirst(node);

                return (true, node.Value.CacheValue);
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (cacheLock)
            {
                if (_items.ContainsKey(key))
                {
                    var node = _items[key];
                    node.Value.CacheValue = value;

                    // move to the top
                    _order.Remove(node);
                    _order.AddFirst(node);
                }
                else
                {
                    // at capacity
                    if (_threshold == _items.Count)
                    {
                        var lastNode = _order.Last;
                        _order.RemoveLast();
                        _items.Remove(lastNode.Value.Value.CacheKey);

                        OnItemEvicted?.Invoke(lastNode.Value.Value.CacheKey, lastNode.Value.Value.CacheValue);
                    }

                    var node = new LinkedListNode<CacheItem>(new CacheItem(key, value));
                    _items.Add(key, node);
                    _order.AddFirst(node);
                }
            }
        }

        public int Count { get =>  _items.Count; }

        /// <summary>
        /// Requirement ask to use it as singleton. To test we need need special case funtion to clear the cache.
        /// </summary>
        public static void Clean() => _lazy = null;

        private class CacheItem
        {
            public TKey CacheKey { get; set; }
            public TValue CacheValue { get; set; }

            public CacheItem(TKey key, TValue value)
            {
                CacheKey = key;
                CacheValue = value;
            }
        }
    }

}
