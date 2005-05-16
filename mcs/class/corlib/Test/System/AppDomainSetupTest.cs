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
		[Category("NotWorking")]
		public void ApplicationBase1 ()
		{
			string expected_path = tmpPath.Replace(@"\", @"/");
			AppDomainSetup setup = new AppDomainSetup ();
			string fileUri = "file:///" + expected_path;
			setup.ApplicationBase = fileUri;
			AssertEquals ("AB1 #01", expected_path, setup.ApplicationBase);
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
			Console.WriteLine (Environment.Version);
			AppDomainSetup setup = new AppDomainSetup ();
			string expected = Path.Combine (Environment.CurrentDirectory, "lalala");
			setup.ApplicationBase = "lalala";
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
		[Category("NotWorking")]
		public void ApplicationBase5 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "file:///lala:la";
			AssertEquals ("AB5 #01", "lala:la", setup.ApplicationBase);
		}
	}
}

