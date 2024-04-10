namespace Cache.Tests
{
    [TestFixture]
    public class CacheTestsExended
    {

        /// <summary>
        /// Just an example how we could do some basic verificaiton for thread-safety.
        /// Unfortuantelly tesing to it is dificult and not 100% bullet proof.
        /// </summary>
        [Test, Parallelizable]
        [Repeat(10)]
        public void ThreadSafety_Check()
        {
            var rnd = new Random();
            int evictionCount = 0;

            var cache = Cache<int, string>.GetInstance(10);
            cache.OnItemEvicted += (_, _) => { evictionCount++; };

            Parallel.For(0, 100_000, i =>
            {
                cache.Add(i, i.ToString());
                cache.Get(rnd.Next(0, 100));
            });

            Assert.IsTrue(cache.Count == 10);
            Assert.IsTrue(evictionCount >= 100_000-10);
        }
    }
}
