using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiIndexCollection.Tests.Data;

namespace MultiIndexCollection.Tests
{
    [TestClass]
    public class FilteringTests
    {
        [TestMethod]
        public void WhereEquals()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "Alice", Age = 20 },
            };

            var expected = users.Where(u => u.Age == 20);

            var indexed = users.IndexBy(u => u.Age);
            
            var actual = indexed.Where(u => u.Age == 20);
            
            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereSortedEquals()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "Alice", Age = 20 },
            };

            var expected = users.Where(u => u.Age == 20);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age == 20);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereIsNull()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "Alice", Age = null },
            };

            var expected = users.Where(u => u.Age == null);

            var indexed = users.IndexBy(u => u.Age);

            var actual = indexed.Where(u => u.Age == null);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereSortedIsNull()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "Alice", Age = null },
            };

            var expected = users.Where(u => u.Age == null);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age == null);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereGreaterThan()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "Alice", Age = 30 },
            };

            var expected = users.Where(u => u.Age > 20);

            var indexed = users
                .IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age > 20);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereGreaterThanOrEqual()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "Alice", Age = 30 },
            };

            var expected = users.Where(u => u.Age >= 30);

            var indexed = users
                .IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age >= 30);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereLessThan()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "Alice", Age = 30 },
            };

            var expected = users.Where(u => u.Age < 30);

            var indexed = users
                .IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age < 30);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereLessThanOrEqual()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "Alice", Age = 30 },
            };

            var expected = users.Where(u => u.Age <= 20);

            var indexed = users
                .IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age <= 20);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereAndAlso()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "John", Age = 30 },
            };

            var expected = users.Where(u => u.Name == "John" && u.Age == 30);

            var indexed = users
                .IndexBy(u => u.Name)
                .IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Name == "John" && u.Age == 30);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereOrElse()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 20 },
                new User { Name = "Fred", Age = 30 },
                new User { Name = "John", Age = 30 },
            };

            var expected = users.Where(u => u.Name == "John" || u.Age == 30).ToArray();

            var indexed = users
                .IndexBy(u => u.Name)
                .IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Name == "John" || u.Age == 30).ToArray();

            Assert.That.SetEquals(expected, actual);
        }

        #region WhereBeetween

        [TestMethod]
        public void WhereBeetween()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 15 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Bob", Age = 35 },
                new User { Name = "Sara", Age = 40 },
            };

            var expected = users.Where(u => u.Age >= 20 && u.Age <= 35);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age >= 20 && u.Age <= 35);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereBeetweenGreaterExclusive()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 15 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Bob", Age = 35 },
                new User { Name = "Sara", Age = 40 },
            };

            var expected = users.Where(u => u.Age >= 20 && u.Age < 35);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age >= 20 && u.Age < 35);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereBeetweenLessExclusive()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 15 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Bob", Age = 35 },
                new User { Name = "Sara", Age = 40 },
            };

            var expected = users.Where(u => u.Age > 20 && u.Age <= 35);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age > 20 && u.Age <= 35);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereBeetweenExclusive()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 15 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Bob", Age = 35 },
                new User { Name = "Sara", Age = 40 },
            };

            var expected = users.Where(u => u.Age > 20 && u.Age < 35);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age > 20 && u.Age < 35);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereBeetweenInclusive()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 15 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Bob", Age = 35 },
                new User { Name = "Sara", Age = 40 },
            };

            var expected = users.Where(u => u.Age <= 35 && u.Age >= 20);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age <= 35 && u.Age >= 20);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereBeetweenGreaterInclusive()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 15 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Bob", Age = 35 },
                new User { Name = "Sara", Age = 40 },
            };

            var expected = users.Where(u => u.Age <= 35 && u.Age > 20);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age <= 35 && u.Age > 20);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereBeetweenLessInclusive()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 15 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Bob", Age = 35 },
                new User { Name = "Sara", Age = 40 },
            };

            var expected = users.Where(u => u.Age < 35 && u.Age >= 20);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age < 35 && u.Age >= 20);

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereBeetweenExclusiveReverse()
        {
            var users = new[]
            {
                new User { Name = "John", Age = 15 },
                new User { Name = "Fred", Age = 20 },
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Bob", Age = 35 },
                new User { Name = "Sara", Age = 40 },
            };

            var expected = users.Where(u => u.Age < 35 && u.Age > 20);

            var indexed = users.IndexBy(u => u.Age, true);

            var actual = indexed.Where(u => u.Age < 35 && u.Age > 20);

            Assert.That.SetEquals(expected, actual);
        }

        #endregion

        [TestMethod]
        public void WhereStringStartsWith()
        {
            var users = new[]
            {
                new User { Name = "Alice", Age = 30 },
                new User { Name = "Bob", Age = 35 },
                new User { Name = "Bill", Age = 40 },
            };

            var expected = users.Where(u => u.Name.StartsWith("B"));

            var indexed = users.IndexBy(u => u.Name, true);

            var actual = indexed.Where(u => u.Name.StartsWith("B"));

            Assert.That.SetEquals(expected, actual);
        }

        [TestMethod]
        public void WhereStringStartsWithIgnoreCase()
        {
            var users = new[]
            {
                new User { Name = "Alice", Age = 30 },
                new User { Name = "bob", Age = 35 },
                new User { Name = "Bill", Age = 40 },
            };

            var expected = users.Where(u => u.Name
                .StartsWith("B", StringComparison.InvariantCultureIgnoreCase));

            var indexed = users.IndexByIgnoreCase(u => u.Name);

            var actual = indexed.Where(u => u.Name.StartsWith("B"));

            Assert.That.SetEquals(expected, actual);
        }
    }
}
