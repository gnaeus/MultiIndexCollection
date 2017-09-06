using System.Collections.ObjectModel;
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
        public void UpdateInIndex()
        {
            var users = new[]
            {
                new User { Id = 1, Name = "John" },
                new User { Id = 2, Name = "Fred" },
            };

            var indexed = users
                .IndexBy(u => u.Id, isSorted: true)
                .IndexBy(u => u.Name);

            users[0].Id = 3;
            users[0].Name = "Alice";

            var foundById = indexed.FirstOrDefault(u => u.Id == 3);
            var foundByName = indexed.FirstOrDefault(u => u.Name == "Alice");

            Assert.IsNull(foundById);
            Assert.IsNull(foundByName);

            indexed.Update(users[0]);

            foundById = indexed.FirstOrDefault(u => u.Id == 3);
            foundByName = indexed.FirstOrDefault(u => u.Name == "Alice");

            Assert.AreEqual(users[0], foundById);
            Assert.AreEqual(users[0], foundByName);

            Assert.That.SetEquals(users, indexed);
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

        [TestMethod]
        public void ClearCollection()
        {
            var users = new[]
            {
                new User { Id = 1, Name = "John", Age = 20 },
                new User { Id = 2, Name = "Fred", Age = 30 },
                new User { Id = 3, Name = "Alice", Age = 20 },
            };

            var indexed = users
                .IndexBy(u => u.Id)
                .IndexBy(u => u.Age, true)
                .IndexByIgnoreCase(u => u.Name);

            indexed.Clear();

            Assert.AreEqual(0, indexed.Count);
            Assert.IsNull(indexed.FirstOrDefault(u => u.Id == 1));
            Assert.IsNull(indexed.FirstOrDefault(u => u.Name == "Fred"));
            Assert.IsNull(indexed.FirstOrDefault(u => u.Age == 20));
        }

        [TestMethod]
        public void ListenPropertyChanged()
        {
            var articles = new[]
            {
                new Article { Id = 1, Title = "First", Content = "Test test test..." },
                new Article { Id = 2, Title = "Second", Content = "My awesome article..." },
            };

            // index
            var indexed = articles
                .IndexBy(a => a.Id)
                .IndexByIgnoreCase(a => a.Title);

            Assert.IsNotNull(articles[0].GetPropertyChanged());
            Assert.IsNotNull(articles[1].GetPropertyChanged());

            Assert.IsNotNull(indexed.FirstOrDefault(a => a.Id == 1));
            Assert.IsNotNull(indexed.FirstOrDefault(a => a.Title == "second"));

            // change
            articles[0].Id = 3;
            articles[1].Title = "Second Article";

            Assert.IsNull(indexed.FirstOrDefault(a => a.Id == 1));
            Assert.IsNull(indexed.FirstOrDefault(a => a.Title == "second"));

            Assert.IsNotNull(indexed.FirstOrDefault(a => a.Id == 3));
            Assert.IsNotNull(indexed.FirstOrDefault(a => a.Title == "Second Article"));
            
            // add
            var added = new Article { Id = 4, Title = "Fourth" };

            indexed.Add(added);

            Assert.IsNotNull(added.GetPropertyChanged());

            Assert.IsNotNull(indexed.FirstOrDefault(a => a.Id == 4));
            Assert.IsNotNull(indexed.FirstOrDefault(a => a.Title == "fourth"));

            added.Id = 5;
            added.Title = "Fifth";

            Assert.IsNull(indexed.FirstOrDefault(a => a.Id == 4));
            Assert.IsNull(indexed.FirstOrDefault(a => a.Title == "fourth"));

            Assert.IsNotNull(indexed.FirstOrDefault(a => a.Id == 5));
            Assert.IsNotNull(indexed.FirstOrDefault(a => a.Title == "fifth"));

            // remove
            indexed.Remove(added);

            Assert.IsNull(added.GetPropertyChanged());

            // clear
            indexed.Clear();

            Assert.IsNull(articles[0].GetPropertyChanged());
            Assert.IsNull(articles[1].GetPropertyChanged());
        }

        [TestMethod]
        public void ListenObservableCollection()
        {
            var users = new ObservableCollection<User>
            {
                new User { Id = 1, Name = "John", Age = 20 },
                new User { Id = 2, Name = "Fred", Age = 30 },
                new User { Id = 3, Name = "Alice", Age = 20 },
            };

            var indexed = users
                .IndexBy(u => u.Id)
                .IndexBy(u => u.Age, true)
                .IndexByIgnoreCase(u => u.Name);

            var testUser = new User { Id = 4, Name = "Sara", Age = 30 };

            Assert.That.SetEquals(users, indexed);

            // add
            users.Add(testUser);

            Assert.AreEqual(4, indexed.Count);
            Assert.AreSame(testUser, indexed.FirstOrDefault(u => u.Id == 4));
            Assert.AreSame(testUser, indexed.FirstOrDefault(u => u.Name == "sara"));
            Assert.AreSame(testUser, indexed.LastOrDefault(u => u.Age == 30));
            Assert.That.SetEquals(users, indexed);

            // remove
            users.RemoveAt(0);

            Assert.AreEqual(3, indexed.Count);
            Assert.IsNull(indexed.FirstOrDefault(u => u.Id == 1));
            Assert.IsNull(indexed.FirstOrDefault(u => u.Name == "John"));
            Assert.That.SetEquals(users, indexed);

            // replace
            users[0] = testUser;

            Assert.AreEqual(2, indexed.Count);
            Assert.IsNull(indexed.FirstOrDefault(u => u.Id == 2));
            Assert.IsNull(indexed.FirstOrDefault(u => u.Name == "Fred"));
            Assert.That.SetEquals(users, indexed);

            // reset
            users.Clear();

            Assert.AreEqual(0, indexed.Count);
            Assert.IsNull(indexed.FirstOrDefault(u => u.Id == 1));
            Assert.IsNull(indexed.FirstOrDefault(u => u.Name == "Fred"));
            Assert.IsNull(indexed.FirstOrDefault(u => u.Age == 20));
            Assert.That.SetEquals(users, indexed);
        }
    }
}
