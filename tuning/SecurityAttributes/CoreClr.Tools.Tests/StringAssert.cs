using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
	public static class StringAssert
	{
		public static void AssertLinesAreEquivalent(string expected, string actual)
		{	
			Assert.AreEqual(SortedLines(expected), SortedLines(actual));
		}

		public static void AssertLinesAreEquivalent(IEnumerable<string> expectedLines, IEnumerable<string> actualLines)
		{
			Assert.AreEqual(SortedArray(expectedLines), SortedArray(actualLines));
		}

		private static string[] SortedLines(string expected)
		{
			return SortedArray(expected.NonEmptyLines());
		}

		private static string[] SortedArray(IEnumerable<string> lines)
		{
			return lines.OrderBy(s => s).ToArray();
		}
	}
}
