using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MultiIndexCollection.Tests
{
    public static class AssertExtensions
    {
        public static void SetEquals<T>(
            this Assert assert, IEnumerable<T> expected, IEnumerable<T> actual)
        {
            Assert.IsTrue(expected.ToHashSet().SetEquals(actual.ToHashSet()));
        }

        public static void SequenceEquals<T>(
            this Assert assert, IEnumerable<T> expected, IEnumerable<T> actual)
        {
            Assert.IsTrue(expected.SequenceEqual(actual));
        }
    }
}
