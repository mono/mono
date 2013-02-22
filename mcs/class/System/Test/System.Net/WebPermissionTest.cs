//
// WebPermissionTest.cs - NUnit Test Cases for WebPermissionAttribute
//
// Author:
//   Miguel de Icaza (miguel@novell.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace MonoTests.System.Net {
	
	[TestFixture]
	[Category ("CAS")]
	public class WebPermissionTest {

		[Test]
		public void Serialization ()
		{
#if NET_4_0
			string result1 = "<IPermission class=\"System.Net.WebPermission, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"\n" + 
"version=\"1\">\n" + 
"<ConnectAccess>\n" + 
"<URI uri=\"Hello\"/>\n" + 
"</ConnectAccess>\n" + 
"</IPermission>\n";

			string result2 = "<IPermission class=\"System.Net.WebPermission, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"\n" + 
"version=\"1\">\n" + 
"<AcceptAccess>\n" + 
"<URI uri=\"Hello\"/>\n" + 
"</AcceptAccess>\n" + 
"</IPermission>\n";
#else
			string result1 = "<IPermission class=\"System.Net.WebPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"\n" + 
"version=\"1\">\n" + 
"<ConnectAccess>\n" + 
"<URI uri=\"Hello\"/>\n" + 
"</ConnectAccess>\n" + 
"</IPermission>\n";

			string result2 = "<IPermission class=\"System.Net.WebPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"\n" + 
"version=\"1\">\n" + 
"<AcceptAccess>\n" + 
"<URI uri=\"Hello\"/>\n" + 
"</AcceptAccess>\n" + 
"</IPermission>\n";
#endif   
			WebPermission pp = new WebPermission (NetworkAccess.Connect, "Hello");
			Assert.AreEqual (result1, pp.ToXml ().ToString ().Replace ("\r", ""));
			
			pp = new WebPermission (NetworkAccess.Accept, "Hello");
			Assert.AreEqual (result2, pp.ToXml ().ToString ().Replace ("\r", ""));
		}

	}
}
