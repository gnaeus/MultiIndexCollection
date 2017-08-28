using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MultiIndexCollection.Tests
{
    public static class AssertExtensions
    {
        public static bool SetEquals<T>(
            this Assert assert, IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return expected.ToHashSet().SetEquals(actual.ToHashSet());
        }

        public static bool SequenceEquals<T>(
            this Assert assert, IEnumerable<T> expected, IEnumerable<T> actual)
        {
            return expected.SequenceEqual(actual);
        }
    }
}
