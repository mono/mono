//
// PolicyStatementTest.cs - NUnit Test Cases for PolicyStatement
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
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class PolicyStatementTest {

		static PermissionSet Empty = new PermissionSet (PermissionState.None);
		static PermissionSet Unrestricted = new PermissionSet (PermissionState.Unrestricted);

		[Test]
		public void Constructor_PermissionSet_Null ()
		{
			PolicyStatement ps = new PolicyStatement (null);
			Assert.AreEqual (PolicyStatementAttribute.Nothing, ps.Attributes, "Attributes");
			Assert.AreEqual (String.Empty, ps.AttributeString, "AttributeString");
			Assert.AreEqual (Empty.ToString (), ps.PermissionSet.ToString (), "PermissionSet");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.Copy ().ToXml ().ToString (), "Copy");
		}

		[Test]
		public void Constructor_PermissionSet_None ()
		{
			PermissionSet pset = new PermissionSet (PermissionState.None);
			PolicyStatement ps = new PolicyStatement (pset);
			Assert.AreEqual (PolicyStatementAttribute.Nothing, ps.Attributes, "Attributes");
			Assert.AreEqual (String.Empty, ps.AttributeString, "AttributeString");
			Assert.AreEqual (Empty.ToString (), ps.PermissionSet.ToString (), "PermissionSet");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.Copy ().ToXml ().ToString (), "Copy");
		}

		[Test]
		public void Constructor_PermissionSet_Unrestricted ()
		{
			PermissionSet pset = new PermissionSet (PermissionState.Unrestricted);
			PolicyStatement ps = new PolicyStatement (pset);
			Assert.AreEqual (PolicyStatementAttribute.Nothing, ps.Attributes, "Attributes");
			Assert.AreEqual (String.Empty, ps.AttributeString, "AttributeString");
			Assert.AreEqual (Unrestricted.ToString (), ps.PermissionSet.ToString (), "PermissionSet");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.Copy ().ToXml ().ToString (), "Copy");
		}

		[Test]
		public void Constructor_PermissionSetPolicyStatementAttribute_Null ()
		{
			PolicyStatement ps = new PolicyStatement (null, PolicyStatementAttribute.All);
			Assert.AreEqual (PolicyStatementAttribute.All, ps.Attributes, "Attributes");
			Assert.AreEqual ("Exclusive LevelFinal", ps.AttributeString, "AttributeString");
			Assert.AreEqual (Empty.ToString (), ps.PermissionSet.ToString (), "PermissionSet");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.Copy ().ToXml ().ToString (), "Copy");
		}

		[Test]
		public void Constructor_Copy ()
		{
			PermissionSet original = new PermissionSet (PermissionState.None);
			PolicyStatement ps = new PolicyStatement (original, PolicyStatementAttribute.All);
			Assert.AreEqual (Empty.ToString (), ps.PermissionSet.ToString (), "PermissionSet");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.Copy ().ToXml ().ToString (), "Copy");

			original.AddPermission (new SecurityPermission (SecurityPermissionFlag.AllFlags));
			Assert.AreEqual (Empty.ToString (), ps.PermissionSet.ToString (), "PermissionSet");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.Copy ().ToXml ().ToString (), "Copy");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Attribute_Invalid ()
		{
			PolicyStatement ps = new PolicyStatement (null);
			ps.Attributes = (PolicyStatementAttribute)Int32.MinValue;
		}

		[Test]
		public void AttributeString ()
		{
			PolicyStatement ps = new PolicyStatement (null);
			Assert.AreEqual (String.Empty, ps.AttributeString, "Nothing");
			ps.Attributes = PolicyStatementAttribute.LevelFinal;
			Assert.AreEqual ("LevelFinal", ps.AttributeString, "LevelFinal");
			ps.Attributes = PolicyStatementAttribute.Exclusive;
			Assert.AreEqual ("Exclusive", ps.AttributeString, "Exclusive");
			ps.Attributes = PolicyStatementAttribute.All;
			Assert.AreEqual ("Exclusive LevelFinal", ps.AttributeString, "All");
		}

		[Test]
		public void AddToPermissionSet ()
		{
			PolicyStatement ps = new PolicyStatement (null);
			Assert.AreEqual (Empty.ToString (), ps.PermissionSet.ToString (), "Empty");

			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Execution);
			IPermission p = ps.PermissionSet.AddPermission (sp);
			Assert.AreEqual (sp.ToXml ().ToString (), p.ToXml ().ToString (), "AddPermission");

			// but nothing was added
			Assert.AreEqual (Empty.ToString (), ps.PermissionSet.ToString (), "Still Empty");
			// and (strangely) it's not considered a read-only permission set 
			// as this property is always false for PermissionSet
			Assert.IsFalse (ps.PermissionSet.IsReadOnly, "IsReadOnly");
		}

		[Test]
		public void SetPermissionSet ()
		{
			PolicyStatement ps = new PolicyStatement (null);
			Assert.AreEqual (Empty.ToString (), ps.PermissionSet.ToString (), "Empty");
			ps.PermissionSet = new PermissionSet (PermissionState.Unrestricted);
			Assert.AreEqual (Unrestricted.ToString (), ps.PermissionSet.ToString (), "Unrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			PolicyStatement ps = new PolicyStatement (null);
			ps.FromXml (null);
		}

		[Test]
#if MOBILE
		[Ignore]
#endif
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_SecurityElementNull ()
		{
			PolicyStatement ps = new PolicyStatement (null);
			ps.FromXml (null, PolicyLevel.CreateAppDomainLevel ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_BadSecurityElement ()
		{
			PolicyStatement ps = new PolicyStatement (null);
			ps.FromXml (new SecurityElement ("Bad"));
		}

		[Test]
		public void ToFromXml_PolicyLevelNull ()
		{
			PolicyStatement ps = new PolicyStatement (null);
			SecurityElement se = ps.ToXml (null);
			ps.FromXml (se, null);
		}

		[Test]
		public void ToFromXml_RoundTrip ()
		{
			PolicyStatement ps1 = new PolicyStatement (Unrestricted, PolicyStatementAttribute.All);
			SecurityElement se = ps1.ToXml ();

			PolicyStatement ps2 = new PolicyStatement (null);
			ps2.FromXml (se, null);

			Assert.AreEqual (ps1.ToXml ().ToString (), ps2.ToXml ().ToString (), "Xml");
		}
#if NET_2_0
		[Test]
		[Category ("NotWorking")]
		public void Equals ()
		{
			PolicyStatement empty1 = new PolicyStatement (null);
			PolicyStatement empty2 = new PolicyStatement (null);
			Assert.IsTrue (empty1.Equals (empty2), "empty1.Equals (empty2)");
			Assert.IsTrue (empty2.Equals (empty1), "empty2.Equals (empty1)");
			Assert.IsFalse (Object.ReferenceEquals (empty1, empty2), "!ReferenceEquals");

			PolicyStatement unr1 = new PolicyStatement (Unrestricted, PolicyStatementAttribute.All);
			Assert.IsFalse (unr1.Equals (empty1), "unr1.Equals (empty1)");
			Assert.IsFalse (empty1.Equals (unr1), "empty1.Equals (unr1)");

			PolicyStatement unr2 = new PolicyStatement (Unrestricted, PolicyStatementAttribute.Exclusive);
			Assert.IsFalse (unr1.Equals (unr2), "unr1.Equals (unr2)");
			Assert.IsFalse (unr2.Equals (unr1), "unr2.Equals (unr1)");

			PolicyStatement unr3 = unr2.Copy ();
			Assert.IsTrue (unr3.Equals (unr2), "unr3.Equals (unr2)");
			Assert.IsTrue (unr2.Equals (unr3), "unr2.Equals (unr3)");
		}
#endif
	}
}
