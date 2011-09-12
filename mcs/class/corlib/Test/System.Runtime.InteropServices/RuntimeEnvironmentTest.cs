//
// RuntimeEnvironmentTest.cs - NUnit tests for RuntimeEnvironment
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Runtime.InteropServices {

	[TestFixture]
	public class RuntimeEnvironmentTest {

		[Test]
		public void SystemConfigurationFile ()
		{
			string fname = RuntimeEnvironment.SystemConfigurationFile;
			Assert.IsNotNull (fname, "SystemConfigurationFile");
			Assert.IsTrue (File.Exists (fname), "Exists");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void FromGlobalAccessCache_Null ()
		{
			RuntimeEnvironment.FromGlobalAccessCache (null);
		}

		[Test]
#if NET_2_1
		[Ignore ("There's no GAC for the NET_2_1 based profiles (Moonlight, MonoTouch and Mono for Android")]
#endif
		public void FromGlobalAccessCache ()
		{
			Assembly corlib = typeof (int).Assembly;
#if NET_2_0
			// FIXME: This doesn't work when doing make distcheck (probably because the corlib used isn't the GAC)
//			Assert.IsTrue (RuntimeEnvironment.FromGlobalAccessCache (corlib), "corlib");
#else
			// note: mscorlib.dll wasn't in the GAC for 1.x
			Assert.IsFalse (RuntimeEnvironment.FromGlobalAccessCache (corlib), "corlib");
#endif
			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			Assert.IsFalse (RuntimeEnvironment.FromGlobalAccessCache (corlib_test), "corlib_test");
		}

		[Test]
		public void GetRuntimeDirectory ()
		{
			string dirname = RuntimeEnvironment.GetRuntimeDirectory ();
			Assert.IsNotNull (dirname, "GetRuntimeDirectory");
			Assert.IsTrue (Directory.Exists (dirname), "Exists");
		}

		[Test]
		public void GetSystemVersion ()
		{
			Assert.IsNotNull (RuntimeEnvironment.GetSystemVersion (), "GetSystemVersion");
		}
	}
}
