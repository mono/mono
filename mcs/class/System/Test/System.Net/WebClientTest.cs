//
// WebClientTest.cs - NUnit Test Cases for System.Net.WebClient
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Runtime.Serialization;

namespace MonoTests.System.Net {
	[TestFixture]
	public class WebClientTest {

		[Test]
		[Category ("InetAccess")]
		public void DownloadTwice ()
		{
			WebClient wc = new WebClient();
			string filename = Path.GetTempFileName();
			
			// A new, but empty file has been created. This is a test case
			// for bug 81005
			wc.DownloadFile("http://google.com/", filename);
			
			// Now, remove the file and attempt to download again.
			File.Delete(filename);
			wc.DownloadFile("http://google.com/", filename);

			// We merely want this to reach this point, bug 81066.
		}
	}
	
}
