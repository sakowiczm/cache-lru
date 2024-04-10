namespace Cache.Tests
{
    [TestFixture, NonParallelizable]
    public class CacheTests
    {
        [Test]
        public void AddItems()
        {
            var cache = Cache<int, string>.GetInstance(5);

            cache.Add(1, "one");
            cache.Add(2, "two");
            cache.Add(3, "three");
            cache.Add(4, "four");

            // Assert
            Assert.IsTrue(cache.Count == 4);

            var one = cache.Get(1);

            Assert.IsTrue(one.Found == true && one.Value == "one");

            Assert.IsTrue(cache.Get(2).Value == "two");
            Assert.IsTrue(cache.Get(3).Value == "three");
            Assert.IsTrue(cache.Get(4).Value == "four");
        }

        [Test]
        public void AddItems_Small_Threshold()
        {
            var cache = Cache<int, string>.GetInstance(1);

            cache.Add(1, "one");
            cache.Add(2, "two");
            cache.Add(3, "three");
            cache.Add(4, "four");

            // Assert
            Assert.IsTrue(cache.Count == 1);

            var (found, value) = cache.Get(4);

            Assert.IsTrue(found == true && value == "four");
        }

        [Test]
        public void AddItems_KeyAlready_Exists()
        {
            var cache = Cache<int, string>.GetInstance(5);

            cache.Add(1, "one");
            cache.Add(2, "one");
            cache.Add(1, "three");

            var (found, value) = cache.Get(1);

            Assert.IsTrue(found == true && value == "three");
        }

        [Test]
        public void AddItems_KeyDoesNot_Exists()
        {
            var cache = Cache<int, string>.GetInstance(5);

            cache.Add(1, "one");
            cache.Add(2, "one");
            cache.Add(1, "three");

            var (found, value) = cache.Get(3);

            Assert.IsTrue(found is false && value is null);
        }

        [Test]
        public void AddItems_KeyDoesNot_Exits_DifferentType()
        {
            var cache = Cache<int, int>.GetInstance(5);

            cache.Add(1, 1);
            cache.Add(2, 2);
            cache.Add(1, 3);

            var (found, value) = cache.Get(3);

            Assert.IsTrue(found is false && value is 0);
        }

        [Test]
        public void AddItems_Item_Exists_But_IsDefaultValue()
        {
            var cache = Cache<int, int>.GetInstance(5);

            cache.Add(1, 1);
            cache.Add(2, 0);
            cache.Add(1, 3);

            var (found, value) = cache.Get(2);

            Assert.IsTrue(found is true && value is 0);
        }

        [Test]
        public void AddItems_KeyNull()
        {
#pragma warning disable CS8714 // Ignore nullable warning for valid test

            Assert.Throws<ArgumentNullException>(() => {
                var cache = Cache<int?, string>.GetInstance(1);

                cache.Add(null, "empty");
            });

#pragma warning restore CS8714
        }

        [Test]
        public void AddItems_And_Evict_FirstItem()
        {
            var cache = Cache<int, string>.GetInstance(4);

            cache.Add(1, "one");
            cache.Add(2, "two");
            cache.Add(3, "three");
            cache.Add(4, "four");
            cache.Add(5, "five");

            Console.WriteLine(cache.Count);

            // Assert
            Assert.IsTrue(cache.Count == 4);

            Assert.IsTrue(cache.Get(2).Value == "two");
            Assert.IsTrue(cache.Get(3).Value == "three");
            Assert.IsTrue(cache.Get(4).Value == "four");
            Assert.IsTrue(cache.Get(5).Value == "five");
        }

        [Test]
        public void AddItems_And_Evict_OldestItem()
        {
            var cache = Cache<int, string>.GetInstance(4);

            cache.Add(1, "one");
            cache.Add(2, "two");
            cache.Add(3, "three");
            cache.Add(4, "four");

            cache.Get(1);
            cache.Get(2);
            cache.Get(3);

            // item 4 would be the oldest one
            cache.Add(5, "five");

            // Assert
            Assert.IsTrue(cache.Count == 4);

            Assert.IsTrue(cache.Get(1).Value == "one");
            Assert.IsTrue(cache.Get(2).Value == "two");
            Assert.IsTrue(cache.Get(3).Value == "three");
            Assert.IsTrue(cache.Get(5).Value == "five");
        }

        [TestCase(-1)]
        [TestCase(-99)]
        [TestCase(0)]
        public void Threshold_Constructor_CheckBoundary(int threshold)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Cache<int, string>.GetInstance(threshold));
        }

        [Test]
        public void EvictedItem_Notification()
        {
            bool evicted = false;
            int evictedKey = -1;
            string evictedValue = string.Empty;

            var cache = Cache<int, string>.GetInstance(2);

            var notification = (int key, string value) =>
            {
                evicted = true;
                evictedKey = key;
                evictedValue = value;
            };

            cache.OnItemEvicted += notification;

            cache.Add(1, "one");
            cache.Add(2, "two");

            cache.Get(1);

            // item 2 would be the oldest one
            cache.Add(3, "three");

            // Assert
            Assert.IsTrue(cache.Count == 2);

            Assert.IsTrue(evicted);
            Assert.IsTrue(evictedKey == 2);
            Assert.IsTrue(evictedValue == "two");


            Assert.IsTrue(cache.Get(1).Value == "one");
            Assert.IsTrue(cache.Get(3).Value == "three");

            cache.OnItemEvicted -= notification;
        }

        [TearDown]
        public void Clean()
        {
            Cache<int, string>.Clean();
        }
    }
}