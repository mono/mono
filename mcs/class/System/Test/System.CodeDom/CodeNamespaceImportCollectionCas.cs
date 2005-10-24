//
// CodeNamespaceImportCollectionCas.cs 
//	- CAS unit tests for System.CodeDom.CodeNamespaceImportCollection
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
using System.CodeDom;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.CodeDom {

	[TestFixture]
	[Category ("CAS")]
	public class CodeNamespaceImportCollectionCas {

		private CodeNamespaceImport cni;
		private CodeNamespaceImport[] array;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			cni = new CodeNamespaceImport ();
			array = new CodeNamespaceImport[1] { cni };
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			CodeNamespaceImportCollection coll = new CodeNamespaceImportCollection ();
			coll.Add (cni);
			Assert.AreSame (cni, coll[0], "this[int]");
			coll[0] = cni;
			coll.Clear ();
			coll.AddRange (array);
			Assert.IsNotNull (coll.GetEnumerator (), "GetEnumerator");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void IList_Deny_Unrestricted ()
		{
			IList coll = (IList) new CodeNamespaceImportCollection ();
			Assert.IsFalse (coll.IsFixedSize, "IsFixedSize");
			Assert.IsFalse (coll.IsReadOnly, "IsReadOnly");
			Assert.AreEqual (0, coll.Add (cni), "Add");
			Assert.AreSame (cni, coll[0], "this[int]");
			coll[0] = cni;
			Assert.IsTrue (coll.Contains (cni), "Contains");
			Assert.AreEqual (0, coll.IndexOf (cni), "IndexOf");
			coll.Insert (0, cni);
			coll.Remove (cni);
			coll.RemoveAt (0);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ICollection_Deny_Unrestricted ()
		{
			ICollection coll = (ICollection) new CodeNamespaceImportCollection ();
			Assert.IsNull (coll.SyncRoot, "SyncRoot");
			Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
			coll.CopyTo (array, 0);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			ConstructorInfo ci = typeof (CodeTypeReferenceCollection).GetConstructor (new Type[0]);
			Assert.IsNotNull (ci, "default .ctor");
			Assert.IsNotNull (ci.Invoke (null), "invoke");
		}
	}
}
