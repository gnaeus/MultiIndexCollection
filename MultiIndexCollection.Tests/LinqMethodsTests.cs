﻿using System.Linq;
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
    }
}
