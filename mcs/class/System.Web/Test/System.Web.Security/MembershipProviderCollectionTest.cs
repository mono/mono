//
// MembershipProviderCollectionTest.cs
//	- Unit tests for System.Web.Security.MembershipProviderCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Security;

using NUnit.Framework;

namespace MonoTests.System.Web.Security {

	class TestProviderBase : ProviderBase {
	}

	[TestFixture]
	public class MembershipProviderCollectionTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null ()
		{
			MembershipProviderCollection mpc = new MembershipProviderCollection ();
			mpc.Add (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Add_ProviderBase ()
		{
			TestProviderBase pb = new TestProviderBase ();
			MembershipProviderCollection mpc = new MembershipProviderCollection ();
			mpc.Add (pb);
			// Add accept ProviderBase but docs says it throws 
			// an exception if it's not a MembershipProvider
		}

		[Test]
		public void UnexistingProvider ()
		{
			MembershipProviderCollection mpc = new MembershipProviderCollection ();
			Assert.IsNull (mpc["uho"]);
			// but this will throw an HttpException if we're using an unknown provider
			// in an ASP.NET control (like Login)
		}
	}
}

#endif
