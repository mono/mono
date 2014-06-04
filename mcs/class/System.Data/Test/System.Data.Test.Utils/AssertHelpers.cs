// Authors:
//   Matthew Leibowitz <matthew@xamarin.com>
// 
// Copyright (c) 2014 Xamarin Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
#if USE_MSUNITTEST
#if WINDOWS_PHONE || NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
#else // !WINDOWS_PHONE && !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute;
#endif // WINDOWS_PHONE || NETFX_CORE
#else // !USE_MSUNITTEST
using NUnit.Framework;
#endif // USE_MSUNITTEST

namespace MonoTests.System.Data.Utils
{
	public static class AssertHelpers
	{
		public static void AssertThrowsException<T>(Action action) 
			where T : Exception
		{
			AssertThrowsException<T>(action, "Expected an exception of type: " + typeof(T).Name);
		}

#if WINDOWS_PHONE || NETFX_CORE
		public static void AssertThrowsException<T>(Action action, string message) 
			where T : Exception
		{
			Assert.ThrowsException<T>(action, message);
		}
#else
		public static void AssertThrowsException<T>(Action action, string message) 
			where T : Exception
		{
			try
			{
				action();
				Assert.Fail(message);
			}
			catch (T)
			{
				// do nothing as this was expected
			}
			catch (Exception ex)
			{
				AssertIsInstanceOfType<T>(ex, message);
			}
		}
#endif

		public static void AssertIsInstanceOfType<T>(object value, string message)
		{
			AssertIsInstanceOfType(value, typeof(T), message);
		}

		public static void AssertIsInstanceOfType(object value, Type expectedType, string message)
		{
#if USE_MSUNITTEST
			Assert.IsInstanceOfType(value, expectedType, message);
#else
			Assert.IsInstanceOf(expectedType, value, message);
#endif
		}

		public static void AreEqualArray<T>(T[] expected, T[] actual, string message)
		{
#if USE_MSUNITTEST
			if (expected != actual)
			{
				Assert.AreEqual(expected.Length, actual.Length, "Expected arrays of equal length.");

				for (int i = 0; i < expected.Length; i++)
				{
					Assert.AreEqual(expected[i], actual[i], message);
				}
			}
#else
			Assert.AreEqual(expected, actual, message);
#endif
		}
	} 
}
