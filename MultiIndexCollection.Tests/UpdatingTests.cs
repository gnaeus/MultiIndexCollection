using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiIndexCollection.Tests.Data;

namespace MultiIndexCollection.Tests
{
    [TestClass]
    public class UpdatingTests
    {
        [TestMethod]
        public void AddToIndex()
        {
            var users = new[]
            {
                new User { Id = 1, Name = "John" },
                new User { Id = 2, Name = "Fred" },
            };

            var indexed = users
                .IndexBy(u => u.Id, isSorted: true)
                .IndexBy(u => u.Name);

            var user = new User { Id = 3, Name = "Alice" };

            indexed.Add(user);

            var foundById = indexed.FirstOrDefault(u => u.Id == 3);
            var foundByName = indexed.FirstOrDefault(u => u.Name == "Alice");

            Assert.AreEqual(user, foundById);
            Assert.AreEqual(user, foundByName);

            Assert.That.SetEquals(users.Append(user), indexed);
        }

        [TestMethod]
        public void RemoveFromIndex()
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

            var user = users[0];

            indexed.Remove(user);

            var foundById = indexed.FirstOrDefault(u => u.Id == 1);
            var foundByName = indexed.FirstOrDefault(u => u.Name == "John");

            Assert.IsNull(foundById);
            Assert.IsNull(foundByName);

            Assert.That.SetEquals(users.Skip(1), indexed);
        }

        [TestMethod]
        public void AddNullablePropertyToIndex()
        {
            var users = new[]
            {
                new User { Id = 1, Name = "John", Age = 20 },
                new User { Id = 2, Name = "Fred", Age = 30 },
            };

            var indexed = users
                .IndexBy(u => u.Name)
                .IndexBy(u => u.Age, isSorted: true);

            var user = new User { Id = 3 };

            indexed.Add(user);

            var foundByName = indexed.FirstOrDefault(u => u.Name == null);
            var foundByAge = indexed.FirstOrDefault(u => u.Age == null);

            Assert.AreEqual(user, foundByName);
            Assert.AreEqual(user, foundByAge);

            Assert.That.SetEquals(users.Append(user), indexed);
        }

        [TestMethod]
        public void RemoveNullablePropertyFromIndex()
        {
            var users = new[]
            {
                new User { Id = 1 },
                new User { Id = 2, Name = "Fred", Age = 30 },
                new User { Id = 3, Name = "Alice", Age = 20 },
            };

            var indexed = users
                .IndexBy(u => u.Name)
                .IndexBy(u => u.Age, isSorted: true);

            var user = users[0];

            indexed.Remove(user);

            var foundByName = indexed.FirstOrDefault(u => u.Name == null);
            var foundByAge = indexed.FirstOrDefault(u => u.Age == null);

            Assert.IsNull(foundByName);
            Assert.IsNull(foundByAge);

            Assert.That.SetEquals(users.Skip(1), indexed);
        }
    }
}
