//
// System.Web.Hosting.ApplicationHost.cs 
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.IO;
using System.Web.Hosting;
using NUnit.Framework;
using System.Web;

namespace MonoTests.System.Web.Hosting {

	public class MBR : MarshalByRefObject {

		public string GetDomainName ()
		{
			return AppDomain.CurrentDomain.FriendlyName;
		}

		public AppDomain GetDomain ()
		{
			return AppDomain.CurrentDomain;
		}
	}

	[Serializable]
	public class MySerializable {
		public string GetDomainName ()
		{
			return AppDomain.CurrentDomain.FriendlyName;
		}
	}
	
	[TestFixture]
	public class ApplicationHostTest {

		[SetUp] 
		public void Setup ()
		{
			try {
				//
				// Only needed in Windows, where we need to have a bin/ASSEMBLY.DLL
				//
				string p = typeof (ApplicationHostTest).Assembly.Location;
				try {
					Directory.Delete ("bin", true);
				} catch {}

				try {
					Directory.CreateDirectory ("bin");
				} catch {}

				string fn = Path.GetFileName (p);
				File.Copy (p, Path.Combine ("bin", fn));
			} catch {}
		}

		[TearDown] public void Shutdown ()
		{
			try {
				Directory.Delete ("bin", true);
			} catch {}
		}
		
		[Test][ExpectedException (typeof (NullReferenceException))]
		public void ConstructorTestNull ()
		{
			ApplicationHost.CreateApplicationHost (null, null, null);
		}

		[Test]
        [ExpectedException (typeof (ArgumentException))]
		public void ConstructorTestNull2 ()
		{
			ApplicationHost.CreateApplicationHost (null, "/app", ".");
		}

		[Test]
        [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorTestNull3 ()
		{
			ApplicationHost.CreateApplicationHost (typeof (MBR), null, ".");
		}

		[Test][ExpectedException (typeof (NullReferenceException))]
		public void ConstructorTestNull4 ()
		{
			ApplicationHost.CreateApplicationHost (typeof (MBR), "/app", null);
		}

		[Test]
		[ExpectedException(typeof(SerializationException))]
#if TARGET_JVM //System.Security.Policy.Evidence not implemented
		[Category ("NotWorking")]
#endif
		public void Constructor_PlainType ()
		{
			ApplicationHost.CreateApplicationHost (typeof (ApplicationHostTest), "/app", Environment.CurrentDirectory);
		}

		[Test]
		public void Constructor_SerializableType ()
		{
			object o = ApplicationHost.CreateApplicationHost (typeof (MySerializable), "/app", Environment.CurrentDirectory);
			Assert.AreEqual (typeof (MySerializable), o.GetType (), "C2");
			MySerializable m = (MySerializable) o;

			Assert.AreEqual (m.GetDomainName (), AppDomain.CurrentDomain.FriendlyName, "C4");
		}

		static void p (string s, object x)
		{
			Console.WriteLine ("{0} {1}", s, x);
		}
		
		[Test]
		[Category ("NotDotNet")] // D2 and D3 asserts will fail in windows on because file system environment 
		public void ConstructorTest ()
		{
			object o = ApplicationHost.CreateApplicationHost (typeof (MBR), "/app", Environment.CurrentDirectory);
			Assert.AreEqual (typeof (MBR), o.GetType (), "C5");
			MBR m = (MBR) o;

			Assert.AreEqual (true, m.GetDomainName () != AppDomain.CurrentDomain.FriendlyName);

			AppDomain other = m.GetDomain ();
			AppDomainSetup setup = other.SetupInformation;

			string tb = Environment.CurrentDirectory;
			if (tb[tb.Length - 1] == Path.DirectorySeparatorChar)
				tb = tb.Substring (0, tb.Length - 1);

			// Need to fix an issue in AppDomainSetup
			// Assert.AreEqual (tb + "/", setup.ApplicationBase, "D1");
			
			p ("AppDomain's Applicationname is: ", setup.ApplicationName);
			// IGNORE this test. We are setting CachePath to DynamicBase by now.
			// Assert.AreEqual (null, setup.CachePath, "D2");

			Assert.AreEqual (0, String.Compare (tb + Path.DirectorySeparatorChar + "Web.Config", setup.ConfigurationFile, StringComparison.OrdinalIgnoreCase), "D3");
			Assert.AreEqual (false, setup.DisallowBindingRedirects, "D4");
			Assert.AreEqual (true, setup.DisallowCodeDownload, "D5");
			Assert.AreEqual (false, setup.DisallowPublisherPolicy, "D6");
			// Disabling D7 test, as we set it there to avoid locking in sys.web.compilation
			// Assert.AreEqual (null, setup.DynamicBase, "D7");
			Assert.AreEqual (null, setup.LicenseFile, "D8");
			//Assert.AreEqual (LoaderOptimization.NotSpecified, setup.LoaderOptimization);
			p ("LoaderOptimization is: ", setup.LoaderOptimization);
			Assert.AreEqual (0, string.Compare (
#if NET_2_0
				String.Format ("{0}{1}bin", tb, Path.DirectorySeparatorChar),
#else
				"bin",
#endif
				setup.PrivateBinPath, true), "D9"
			);
			Assert.AreEqual (setup.PrivateBinPathProbe, "*", "D10");
			p ("ShadowCopyDirs: ", setup.ShadowCopyDirectories);
			Assert.AreEqual (true, setup.ShadowCopyDirectories.EndsWith ("bin") || setup.ShadowCopyDirectories.EndsWith ("Bin"), "D11");
			Assert.AreEqual (false, setup.ShadowCopyDirectories.StartsWith ("file:"), "D12");
			Assert.AreEqual ("true", setup.ShadowCopyFiles, "D13");

			p ("ApsInstal", HttpRuntime.AspInstallDirectory);
		}
	}
	
}
