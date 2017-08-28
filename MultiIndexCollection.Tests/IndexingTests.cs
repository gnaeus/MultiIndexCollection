using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiIndexCollection.Tests.Data;

namespace MultiIndexCollection.Tests
{
    [TestClass]
    public class IndexingTests
    {
        [TestMethod]
        public void IndexByProperty()
        {
            var users = new[]
            {
                new User { Id = 1, Name = "John" },
                new User { Id = 2, Name = "Fred" },
                new User { Id = 3, Name = "Alice" },
            };

            var indexed = users.IndexBy(u => u.Name);

            Assert.IsTrue(users.ToHashSet().SetEquals(indexed.ToHashSet()));
        }

        [TestMethod]
        public void IndexBySortedProperty()
        {
            var users = new[]
            {
                new User { Id = 1, Name = "John" },
                new User { Id = 2, Name = "Fred" },
                new User { Id = 3, Name = "Alice" },
            };

            var indexed = users.IndexBy(u => u.Id, isSorted: true);

            Assert.IsTrue(users.ToHashSet().SetEquals(indexed.ToHashSet()));
        }

        [TestMethod]
        public void IndexByMulpipleProperties()
        {
            var users = new[]
            {
                new User { Id = 1, Name = "John" },
                new User { Id = 2, Name = "Fred" },
                new User { Id = 3, Name = "Alice" },
            };

            var indexed = users
                .IndexBy(u => u.Id, isSorted: true)
                .IndexBy(u => u.Name);

            Assert.That.SetEquals(users, indexed);
        }

        [TestMethod]
        public void IndexByNonUniqueProperties()
        {
            var users = new[]
            {
                new User { Id = 1, Name = "John", Age = 20 },
                new User { Id = 2, Name = "John", Age = 25 },
                new User { Id = 3, Name = "Alice", Age = 20 },
            };

            var indexed = users
                .IndexBy(u => u.Id)
                .IndexBy(u => u.Name)
                .IndexBy(u => u.Age, isSorted: true);

            Assert.That.SetEquals(users, indexed);
        }

        [TestMethod]
        public void IndexByNullableProperties()
        {
            var users = new[]
            {
                new User { Id = 1, Name = null, Age = 20 },
                new User { Id = 2, Name = null, Age = null },
                new User { Id = 3, Name = "Alice", Age = null },
            };

            var indexed = users
                .IndexBy(u => u.Id)
                .IndexBy(u => u.Name)
                .IndexBy(u => u.Age, isSorted: true);

            Assert.That.SetEquals(users, indexed);
        }
    }
}
