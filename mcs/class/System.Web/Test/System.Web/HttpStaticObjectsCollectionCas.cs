//
// HttpStaticObjectsCollectionCas.cs 
//	- CAS unit tests for System.Web.HttpStaticObjectsCollection
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

using NUnit.Framework;

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpStaticObjectsCollectionCas : AspNetHostingMinimal {

		private HttpStaticObjectsCollection hsoc;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			hsoc = new HttpStaticObjectsCollection ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor_Deny_Unrestricted ()
		{
			new HttpStaticObjectsCollection ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Serialization ()
		{
			MemoryStream ms = new MemoryStream ();
			BinaryWriter writer = new BinaryWriter (ms);
			HttpStaticObjectsCollection hsoc = new HttpStaticObjectsCollection ();
			hsoc.Serialize (writer);

			ms.Position = 0;
			BinaryReader reader = new BinaryReader (ms);
			Assert.IsNotNull (HttpStaticObjectsCollection.Deserialize (reader));
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			Assert.AreEqual (0, hsoc.Count, "Count");
			Assert.IsNotNull (hsoc.GetEnumerator (), "GetEnumerator");
			Assert.IsNull (hsoc.GetObject ("mono"), "GetObject");
			Assert.IsNull (hsoc["mono"], "this[string]");
			Assert.IsTrue (hsoc.IsReadOnly, "IsReadOnly");
			Assert.IsFalse (hsoc.IsSynchronized, "IsSynchronized");
			Assert.IsNotNull (hsoc.SyncRoot, "SyncRoot");
			Assert.IsTrue (hsoc.NeverAccessed, "NeverAccessed");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			hsoc.CopyTo (new object[0], 0);
		}

		// LinkDemand
		public override Type Type {
			get { return typeof (HttpStaticObjectsCollection); }
		}
	}
}
