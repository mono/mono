// AppDomainSetupTest.cs - NUnit Test Cases for the System.AppDomainSetup class
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System
{
	[TestFixture]
	public class AppDomainSetupTest : Assertion
	{
		static readonly string tmpPath = Path.GetTempPath ();
		static readonly string curDir = Directory.GetCurrentDirectory ();

		[Test]
		public void ApplicationBase1 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			string fileUri = "file://" + tmpPath;
			setup.ApplicationBase = fileUri;
			AssertEquals ("AB1 #01", tmpPath, setup.ApplicationBase);
		}

		[Test]
		public void ApplicationBase2 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = curDir;
			AssertEquals ("AB2 #01", curDir, setup.ApplicationBase);
		}

		[Test]
		public void ApplicationBase3 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "lalala";
			string expected = Path.Combine (curDir, "lalala");
			AssertEquals ("AB3 #01", expected, setup.ApplicationBase);
		}

		[Test]
		public void ApplicationBase4 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "lala:la";
			AssertEquals ("AB4 #01", "lala:la", setup.ApplicationBase);
		}

		[Test]
		public void ApplicationBase5 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "file://lala:la";
			AssertEquals ("AB5 #01", "lala:la", setup.ApplicationBase);
		}
	}
}

