// AppDomainSetupTest.cs - NUnit Test Cases for the System.AppDomainSetup class
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
		// Invalid path format
		[ExpectedException (typeof (NotSupportedException))]
#endif
		[Category ("NotWorking")]
		public void ApplicationBase1 ()
		{
			string expected_path = tmpPath.Replace(@"\", @"/");
			AppDomainSetup setup = new AppDomainSetup ();
			string fileUri = "file://" + expected_path;
			setup.ApplicationBase = fileUri;
			// with MS 1.1 SP1 the expected_path starts with "//"
			AssertEquals ("AB1 #01", "//" + expected_path, setup.ApplicationBase);
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
			string expected = Path.Combine (Environment.CurrentDirectory, "lalala");
			setup.ApplicationBase = "lalala";
			AssertEquals ("AB3 #01", expected, setup.ApplicationBase);
		}

		[Test]
#if NET_2_0
		// Invalid path format
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NotWorking")]
#endif
		public void ApplicationBase4 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "lala:la";
			AssertEquals ("AB4 #01", "lala:la", setup.ApplicationBase);
		}

		[Test]
#if NET_2_0
		// Invalid path format
		[ExpectedException (typeof (NotSupportedException))]
#endif
		[Category ("NotWorking")]
		public void ApplicationBase5 ()
		{
			// This is failing because of (probably) a windows-ism, so don't worry
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "file:///lala:la";
			AssertEquals ("AB5 #01", "lala:la", setup.ApplicationBase);
		}
	}
}
