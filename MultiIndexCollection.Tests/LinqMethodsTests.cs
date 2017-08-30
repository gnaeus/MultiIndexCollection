using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiIndexCollection.Tests.Data;

namespace MultiIndexCollection.Tests
{
    [TestClass]
    public class LinqMethodsTests
    {
        [TestMethod]
        public void HavingMax()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "John", Age = 30 },
            };

            var maxAge = users.Max(u => u.Age);
            var expected = users.Where(u => u.Age == maxAge);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.HavingMax(u => u.Age);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void HavingMin()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "John", Age = 30 },
            };

            var minAge = users.Min(u => u.Age);
            var expected = users.Where(u => u.Age == minAge);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.HavingMin(u => u.Age);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void Max()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "John", Age = 30 },
            };

            var maxAge = users.Max(u => u.Age);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Max(u => u.Age);

            Assert.AreEqual(maxAge, actual);
        }

        [TestMethod]
        public void Min()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "John", Age = 30 },
            };

            var minAge = users.Min(u => u.Age);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Min(u => u.Age);

            Assert.AreEqual(minAge, actual);
        }

        [TestMethod]
        public void OrderBy()
        {
            var users = new[]
            {
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "John", Age = 15 },
                new User { Name = "Sara", Age = 40 },
                new User { Name = "Bob", Age = 35 },
            };

            var expected = users.OrderBy(u => u.Age);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.OrderBy(u => u.Age);

            Assert.That.SequenceEquals(expected, actual);
        }

        [TestMethod]
        public void OrderByDescending()
        {
            var users = new[]
            {
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "John", Age = 15 },
                new User { Name = "Sara", Age = 40 },
                new User { Name = "Bob", Age = 35 },
            };

            var expected = users.OrderByDescending(u => u.Age);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.OrderByDescending(u => u.Age);

            Assert.That.SequenceEquals(expected, actual);
        }

        [TestMethod]
        public void ToLookup()
        {
            var users = new[]
            {
                new User { Name = "Alice", Age = null },
                new User { Name = "John", Age = 20 },
                new User { Name = "Sara", Age = 30 },
                new User { Name = "Ted", Age = null },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Bob", Age = 30 },
            };

            var expected = users.ToLookup(u => u.Age).OrderBy(g => g.Key);

            var indexed = users.IndexBy(u => u.Age);

            var actual = indexed.ToLookup(u => u.Age).OrderBy(g => g.Key);

            Assert.That.SequenceEquals(
                expected.Select(g => g.Key),
                actual.Select(g => g.Key));

            expected
                .Zip(actual, (exp, act) => new { exp, act })
                .ToList()
                .ForEach(pair => Assert.That.SetEquals(pair.exp, pair.act));
        }

        [TestMethod]
        public void ToLookupSorted()
        {
            var users = new[]
            {
                new User { Name = "Alice", Age = null },
                new User { Name = "John", Age = 20 },
                new User { Name = "Sara", Age = 30 },
                new User { Name = "Ted", Age = null },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Bob", Age = 30 },
            };

            var expected = users.ToLookup(u => u.Age).OrderBy(g => g.Key);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.ToLookup(u => u.Age).OrderBy(g => g.Key);

            Assert.That.SequenceEquals(
                expected.Select(g => g.Key),
                actual.Select(g => g.Key));

            expected
                .Zip(actual, (exp, act) => new { exp, act })
                .ToList()
                .ForEach(pair => Assert.That.SetEquals(pair.exp, pair.act));
        }
    }
}
