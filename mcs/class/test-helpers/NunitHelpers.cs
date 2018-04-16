using System.IO;
using System.Collections;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
	static class CollectionAssert
	{
		public static void DoesNotContain (IEnumerable collection, object val)
		{
			 Assert.That(collection, Has.No.Member(val));
		}

		public static void Contains (IEnumerable collection, object val)
		{
			 Assert.That(collection, Has.Member(val));
		}

		public static void AreEqual (IEnumerable expected, IEnumerable actual, string message = null, params object[] args) 
		{
			Assert.That(actual, Is.EqualTo(expected), message, args);
		}

		public static void AreEquivalent (IEnumerable expected, IEnumerable actual, string message = null, params object[] args) 
		{
			Assert.That(actual, Is.EquivalentTo(expected), message, args);
		}

		public static void IsEmpty(IEnumerable collection, string message = null, params object[] args)
		{
			Assert.That(collection, new EmptyCollectionConstraint(), message, args);
		}

		public static void IsNotEmpty(IEnumerable collection, string message = null, params object[] args)
		{
			Assert.That(collection, Is.Not.Empty, message, args);
		}
	}

	static class FileAssert
	{
		public static void AreEqual(Stream expected, Stream actual, string message, params object[] args)
		{
			Assert.That(actual, Is.EqualTo(expected), message, args);
		}

		public static void AreEqual(string expected, string actual, string message, params object[] args)
		{
			using (FileStream exStream = File.OpenRead(expected))
			using (FileStream acStream = File.OpenRead(actual))
			{
				AreEqual(exStream, acStream, message, args);
			}
		}
	}

	static class StringAssert
	{
		public static void Contains(string expected, string actual, string message = null, params object[] args)
		{
			Assert.That(actual, Is.StringContaining (expected), message, args);
		}

		public static void StartsWith(string expected, string actual, string message = null, params object[] args)
		{
			Assert.IsTrue (actual.StartsWith (expected), message, args);
		}
	}

	static class AssertHelper
	{
		public static void IsEmpty (string aString, string message = null, params object[] args )
		{
			Assert.That(aString, Is.Empty, message, args);
		}

		public static void IsNotEmpty (string aString, string message = null, params object[] args )
		{
			Assert.That(aString, Is.Not.Empty, message, args);
		}

		public static void Less(int arg1, int arg2, string message = null, params object[] args) 
		{
			Assert.That(arg1, Is.LessThan(arg2), message, args);
		}

		public static void Greater(int arg1, int arg2, string message = null, params object[] args) 
		{
			Assert.That(arg1, Is.GreaterThan(arg2), message, args);
		}

		public static void Greater(double arg1, double arg2, string message = null, params object[] args) 
		{
			Assert.That(arg1, Is.GreaterThan(arg2), message, args);
		}

		public static void Greater(System.DateTime arg1, System.DateTime arg2, string message = null, params object[] args) 
		{
			Assert.That(arg1, Is.GreaterThan(arg2), message, args);
		}

		public static void GreaterOrEqual(int arg1, int arg2, string message = null, params object[] args)
		{
			Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
		}

		public static void GreaterOrEqual(long arg1, long arg2, string message = null, params object[] args)
		{
			Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
		}

		public static void GreaterOrEqual(System.DateTime arg1, System.DateTime arg2, string message = null, params object[] args)
		{
			Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
		}

		public static void GreaterOrEqual(System.TimeSpan arg1, System.TimeSpan arg2, string message = null, params object[] args)
		{
			Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
		}

		public static void LessOrEqual (int arg1, int arg2, string message = null, params object[] args)
		{
			Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
		}

		public static void LessOrEqual(long arg1, long arg2, string message = null, params object[] args)
		{
			Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
		}

		public static void LessOrEqual(System.DateTime arg1, System.DateTime arg2, string message = null, params object[] args)
		{
			Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
		}

		public static void LessOrEqual(System.TimeSpan arg1, System.TimeSpan arg2, string message = null, params object[] args)
		{
			Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
		}

		public static void IsNotInstanceOfType(System.Type expected, object actual, string message, params object[] args )
		{
			Assert.IsFalse (actual.GetType ().IsInstanceOfType (expected), message, args);
		}
	}
}
