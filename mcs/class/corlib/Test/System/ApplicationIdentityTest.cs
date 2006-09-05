//
// ApplicationIdentityTest.cs - NUnit Test Cases for ApplicationIdentity
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;
using System;

namespace MonoTests.System {

	[TestFixture]
	public class ApplicationIdentityTest {

		[Test]
		public void ApplicationIdentity ()
		{
			ApplicationIdentity appid = new ApplicationIdentity ("Mono");
			Assert.IsNull (appid.CodeBase, "CodeBase");
			Assert.AreEqual ("Mono, Culture=neutral", appid.FullName);
			Assert.AreEqual ("Mono, Culture=neutral", appid.ToString ());
		}

		[Test]
		public void ApplicationIdentity_WithCulture ()
		{
			ApplicationIdentity appid = new ApplicationIdentity ("Mono, Culture=fr-ca");
			Assert.IsNull (appid.CodeBase, "CodeBase");
			Assert.AreEqual ("Mono, Culture=fr-ca", appid.FullName);
			Assert.AreEqual ("Mono, Culture=fr-ca", appid.ToString ());
		}

		[Test]
 		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplicationIdentity_Null ()
		{
			new ApplicationIdentity (null);
		}
	}
}

#endif