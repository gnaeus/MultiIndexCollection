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
            throw new NotImplementedException();
        }

        [TestMethod]
        public void WhereGreaterThanOrEqual()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void WhereLessThan()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void WhereLessThanOrEqual()
        {
            throw new NotImplementedException();
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

        [TestMethod]
        public void WhereBeetween()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void WhereStringStartsWith()
        {
            throw new NotImplementedException();
        }
    }
}
