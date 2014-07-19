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
using System.Globalization;
using System.IO;
#if WINDOWS_STORE_APP
using Windows.Storage;
#endif
#if NETFX_CORE
using Windows.Globalization;
#endif
#if WINDOWS_STORE_APP
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
using AssertionException = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.UnitTestAssertException;
#else
using NUnit.Framework;
using NUnit.Framework.Constraints;
#endif

namespace MonoTests.System.Data.Utils {
	public static class AssertHelpers {
		public static void AssertThrowsException<T> (Action action) 
			where T : Exception
		{
			AssertThrowsException<T> (action, "Expected an exception of type: " + typeof (T).Name);
		}

#if WINDOWS_STORE_APP
		public static void AssertThrowsException<T> (Action action, string message) 
			where T : Exception
		{
			Assert.ThrowsException<T> (action, message);
		}
#else
		public static void AssertThrowsException<T> (Action action, string message) 
			where T : Exception
		{
			try {
				action ();
				Assert.Fail (message);
			}
			catch (T) {
				// do nothing as this was expected
			}
			catch (Exception ex) {
				AssertIsInstanceOfType<T> (ex, message);
			}
		}
#endif

		public static void AssertIsInstanceOfType<T> (object value, string message)
		{
			AssertIsInstanceOfType (value, typeof (T), message);
		}

		public static void AssertIsInstanceOfType (object value, Type expectedType, string message)
		{
#if WINDOWS_STORE_APP
			Assert.IsInstanceOfType (value, expectedType, message);
#else
			Assert.That (value, new InstanceOfTypeConstraint (expectedType), message);
#endif
		}

		public static void AreEqualArray<T>(T[] expected, T[] actual, string message)
		{
#if WINDOWS_STORE_APP
			if (expected != actual) {
				Assert.AreEqual (expected.Length, actual.Length, "Expected arrays of equal length.");
				for (int i = 0; i < expected.Length; i++) {
					Assert.AreEqual (expected [i], actual [i], message);
				}
			}
#else
			Assert.AreEqual (expected, actual, message);
#endif
		}

		public static string GetTempFileName ()
		{
#if !WINDOWS_STORE_APP
			return Path.GetTempFileName ();
#else
			return GetTempFileName ("temp_" + Guid.NewGuid ().ToString ("N") + ".tmp");
#endif
		}

		public static string GetTempFileName (string filename)
		{
#if WINDOWS_PHONE
			return filename;
#elif NETFX_CORE
			return Path.Combine (ApplicationData.Current.TemporaryFolder.Path, filename);
#else
			return Path.Combine (Path.GetTempPath (), filename);
#endif
		}
	}
}

#if NETFX_CORE
// dummy class and namespace to forward the culture changing to the new property
namespace System.Threading {
	static class Thread {
		internal static class CurrentThread {
			internal static CultureInfo CurrentCulture {
				get { return new CultureInfo (ApplicationLanguages.PrimaryLanguageOverride); }
				set { ApplicationLanguages.PrimaryLanguageOverride = value.Name; }
			}
		}
	}
}
#endif
