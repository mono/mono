//
// MembershipUserCollectionTest.cs
//	- Unit tests for System.Web.Security.MembershipUserCollection
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
using System.Collections;
using System.Web.Security;

using NUnit.Framework;

namespace MonoTests.System.Web.Security {

	[TestFixture]
	public class MembershipUserCollectionTest {

		private MembershipUser GetMember (string name)
		{
			return new MembershipUser (Membership.Provider.Name, name, null, String.Empty, String.Empty, String.Empty,
				true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now);
		}

		[Test]
		public void Default ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			Assert.IsFalse (muc.IsSynchronized, "IsSynchronized");
			Assert.IsTrue (Object.ReferenceEquals (muc, muc.SyncRoot), "SyncRoot");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			muc.Add (null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void Add_ReadOnly ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			muc.SetReadOnly ();
			muc.Add (GetMember ("me"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Add_Twice ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			muc.Add (GetMember ("me"));
			muc.Add (GetMember ("me"));
		}

		[Test]
		public void Count ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			Assert.AreEqual (0, muc.Count, "0");
			muc.Add (GetMember ("me"));
			Assert.AreEqual (1, muc.Count, "1");
			muc.Add (GetMember ("me too"));
			Assert.AreEqual (2, muc.Count, "2");
			muc.SetReadOnly ();
			Assert.AreEqual (2, muc.Count, "2b");
		}

		[Test]
		public void GetEnumerator ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			int i = 0;
			muc.Add (GetMember ("me"));
			muc.Add (GetMember ("me too"));
			IEnumerator e = muc.GetEnumerator ();
			Assert.IsNotNull (e, "GetEnumerator");
			while (e.MoveNext ()) i++;
			Assert.AreEqual (2, i, "2");
		}

		[Test]
		public void Item ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			muc.Add (GetMember ("me"));
			Assert.IsNotNull (muc["me"], "me");
			Assert.IsNull (muc["me too"], "me too");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			muc.Remove (null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void Remove_ReadOnly ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			muc.Add (GetMember ("me"));
			muc.SetReadOnly ();
			muc.Remove ("me");
		}

		[Test]
		public void Remove_Unexisting ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			muc.Add (GetMember ("me"));
			muc.Remove ("me too");
		}

		[Test]
		public void SetReadOnly ()
		{
			MembershipUserCollection muc = new MembershipUserCollection ();
			muc.Add (GetMember ("me"));
			muc.SetReadOnly ();
			muc.SetReadOnly ();	// twice
		}
	}
}

#endif
