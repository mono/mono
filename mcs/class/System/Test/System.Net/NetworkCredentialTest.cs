//
// Unit tests for System.Net.NetworkCredential
//
// Contact:
//   Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Net;
using System.Security;

using NUnit.Framework;

namespace MonoTests.System.Net {

	[TestFixture]
	public class NetworkCredentialTest {

		void CheckDefaults (NetworkCredential nc)
		{
			Assert.AreSame (String.Empty, nc.Domain, "Domain");
			Assert.AreSame (String.Empty, nc.Password, "Password");
			Assert.AreSame (String.Empty, nc.UserName, "UserName");
			Assert.AreSame (nc, nc.GetCredential (null, null), "GetCredential");
		}

		void CheckCustom (NetworkCredential nc)
		{
			Assert.AreEqual ("dom", nc.Domain, "Domain");
			Assert.AreEqual ("********", nc.Password, "Password");
			Assert.AreEqual ("user", nc.UserName, "UserName");
			Assert.AreSame (nc, nc.GetCredential (new Uri ("http://www.example.com"), "basic"), "GetCredential");
		}

		[Test]
		public void Constructor_0 ()
		{
			NetworkCredential nc = new NetworkCredential ();
			CheckDefaults (nc);

			nc.UserName = null;
			nc.Domain = null;
			nc.Password = null;
			CheckDefaults (nc);

			nc.UserName = "user";
			nc.Domain = "dom";
			nc.Password = "********";
			CheckCustom (nc);
		}

		[Test]
		public void Constructor_2 ()
		{
			NetworkCredential nc = new NetworkCredential ((string)null, (string)null);
			CheckDefaults (nc);

			nc.UserName = String.Empty;
			nc.Domain = String.Empty;
			nc.Password = null;
			CheckDefaults (nc);

			nc = new NetworkCredential ("user", "********");
			nc.Domain = "dom";
			CheckCustom (nc);
		}

		[Test]
		public void Constructor_3 ()
		{
			NetworkCredential nc = new NetworkCredential ((string)null, (string)null, (string)null);
			CheckDefaults (nc);

			nc.UserName = String.Empty;
			nc.Domain = null;
			nc.Password = String.Empty;
			CheckDefaults (nc);

			nc = new NetworkCredential ("user", "********", "dom");
			CheckCustom (nc);
		}

		[Test]
		public void DecipherSecureString ()
		{
			// many code snippets suggest using the following to get the decrypted string from a SecureString
			var ss = new SecureString ();
			ss.AppendChar('h');
			ss.AppendChar('e');
			ss.AppendChar('l');
			ss.AppendChar('l');
			ss.AppendChar('o');
 
			string plain = new NetworkCredential (string.Empty, ss).Password;
			Assert.AreEqual ("hello", plain);
		}
	}
}

