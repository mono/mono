using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.Common
{
	delegate void AssertThrowsDelegate();

	static class AssertExtensions
	{
		public static void AreEqual (byte[] expected, byte[] data, string message)
		{
			if (expected == null) {
				if (data == null)
					return;
				Assert.Fail ("{0}{1}Expected: null{1}Got: byte array with {2} elements and of rank {3}{1}",
					message, Environment.NewLine, data.Length, data.Rank);
			}

			if (data == null)
				Assert.Fail ("{0}{1}Expected: byte array with {2} elements and rank {3}{1}Got: null{1}",
					message, Environment.NewLine, expected.Length, expected.Rank);

			if (expected.Rank > 1)
				Assert.Fail ("Only single-dimensional arrays are supported.");

			if (expected.Rank != data.Rank || expected.Length != data.Length)
				Assert.Fail ("{0}{1}Expected: byte array with {2} elements and rank {3}{1}Got: byte array with {4} elements and rank {5}{1}",
					message, Environment.NewLine, expected.Length, expected.Rank, data.Length, data.Rank);

			int max = expected.Length;
			for (int i = 0; i < max; i++) {
				if (expected[i] != data[i])
					Assert.Fail ("{0}{1}Arrays differ at index {2}.{1}Expected 0x{3:X} got 0x{4:X}{1}",
						message, Environment.NewLine, i, expected[i], data[i]);
			}
		}

		public static void Throws<ET> (AssertThrowsDelegate code, string message)
		{
			Throws(typeof(ET), code, message);
		}

		public static void Throws(Type exceptionType, AssertThrowsDelegate code, string message)
		{
			if (code == null)
				Assert.Fail("No code provided for the test.");

			Exception exception = null;
			try
			{
				code();
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			if (exceptionType == null)
			{
				if (exception == null)
					Assert.Fail("{0}{1}Expected: any exception thrown{1}But was: no exception thrown{1}",
						message, Environment.NewLine);
				return;
			}

			if (exception == null || exception.GetType() != exceptionType)
				Assert.Fail("{0}{1}Expected: {2}{1}But was: {3}{1}{4}{1}",
				    message,
				    Environment.NewLine,
				    exceptionType,
				    exception == null ? "no exception" : exception.GetType().ToString(),
				    exception == null ? "no exception" : exception.ToString());
		}

		public static void Throws(AssertThrowsDelegate code, string message)
		{
			Throws(null, code, message);
		}
	}
}
