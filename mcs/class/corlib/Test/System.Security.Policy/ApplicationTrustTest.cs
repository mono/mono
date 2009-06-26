//
// ApplicationTrustTest.cs - NUnit tests for ApplicationTrust
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

using NUnit.Framework;

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class ApplicationTrustTest {

		private string AdjustLineEnds (string s)
		{
			return s.Replace ("\r\n", "\n");
		}

		[Test]
		public void Constructor_Empty ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			Assert.IsNull (at.ApplicationIdentity, "ApplicationIdentity");
			Assert.AreEqual (PolicyStatementAttribute.Nothing, at.DefaultGrantSet.Attributes, "DefaultGrantSet.Attributes");
			Assert.AreEqual (String.Empty, at.DefaultGrantSet.AttributeString, "DefaultGrantSet.AttributeString");
			Assert.IsTrue (at.DefaultGrantSet.PermissionSet.IsEmpty (), "DefaultGrantSet.PermissionSet.IsEmpty");
			Assert.IsFalse (at.DefaultGrantSet.PermissionSet.IsUnrestricted (), "DefaultGrantSet.PermissionSet.IsUnrestricted");
			Assert.IsNull (at.ExtraInfo, "ExtraInfo");
			Assert.IsFalse (at.IsApplicationTrustedToRun, "IsApplicationTrustedToRun");
			Assert.IsFalse (at.Persist, "Persist");
			string expected = AdjustLineEnds ("<ApplicationTrust version=\"1\">\r\n<DefaultGrant>\r\n<PolicyStatement version=\"1\">\r\n<PermissionSet class=\"System.Security.PermissionSet\"\r\nversion=\"1\"/>\r\n</PolicyStatement>\r\n</DefaultGrant>\r\n</ApplicationTrust>\r\n");
			Assert.AreEqual (expected, AdjustLineEnds (at.ToXml ().ToString ()), "XML");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Null ()
		{
			ApplicationTrust at = new ApplicationTrust (null);
		}

		[Test]
		public void ApplicationIdentity ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.ApplicationIdentity = new ApplicationIdentity ("Mono Unit Test");
			Assert.IsNotNull (at.ApplicationIdentity, "not null");
			string expected = AdjustLineEnds ("<ApplicationTrust version=\"1\"\r\nFullName=\"Mono Unit Test, Culture=neutral\">\r\n<DefaultGrant>\r\n<PolicyStatement version=\"1\">\r\n<PermissionSet class=\"System.Security.PermissionSet\"\r\nversion=\"1\"/>\r\n</PolicyStatement>\r\n</DefaultGrant>\r\n</ApplicationTrust>\r\n");
			Assert.AreEqual (expected, AdjustLineEnds (at.ToXml ().ToString ()), "XML");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplicationIdentity_Null ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.ApplicationIdentity = new ApplicationIdentity ("Mono Unit Test");
			// once set it cannot be "unset" ...
			at.ApplicationIdentity = null;
		}

		[Test]
		public void ApplicationIdentity_Change ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.ApplicationIdentity = new ApplicationIdentity ("Mono Unit Test");
			// ... but it can be changed
			at.ApplicationIdentity = new ApplicationIdentity ("Mono Unit Test Too");
		}

		[Test]
		public void DefaultGrantSet ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.DefaultGrantSet = new PolicyStatement (new PermissionSet (PermissionState.Unrestricted));
			Assert.IsNotNull (at.DefaultGrantSet, "not null");
			string expected = AdjustLineEnds ("<ApplicationTrust version=\"1\">\r\n<DefaultGrant>\r\n<PolicyStatement version=\"1\">\r\n<PermissionSet class=\"System.Security.PermissionSet\"\r\nversion=\"1\"\r\nUnrestricted=\"true\"/>\r\n</PolicyStatement>\r\n</DefaultGrant>\r\n</ApplicationTrust>\r\n");
			Assert.AreEqual (expected, AdjustLineEnds (at.ToXml ().ToString ()), "XML");

			at.DefaultGrantSet = null;
			// returns to defaults
			Assert.IsNotNull (at.DefaultGrantSet, "null");
			Assert.AreEqual (PolicyStatementAttribute.Nothing, at.DefaultGrantSet.Attributes, "DefaultGrantSet.Attributes");
			Assert.AreEqual (String.Empty, at.DefaultGrantSet.AttributeString, "DefaultGrantSet.AttributeString");
			Assert.IsTrue (at.DefaultGrantSet.PermissionSet.IsEmpty (), "DefaultGrantSet.PermissionSet.IsEmpty");
			Assert.IsFalse (at.DefaultGrantSet.PermissionSet.IsUnrestricted (), "DefaultGrantSet.PermissionSet.IsUnrestricted");
		}

		[Test]
		public void ExtraInfo ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.ExtraInfo = "Mono";
			Assert.IsNotNull (at.ExtraInfo, "not null");

			string expected = AdjustLineEnds ("<ApplicationTrust version=\"1\">\r\n<DefaultGrant>\r\n<PolicyStatement version=\"1\">\r\n<PermissionSet class=\"System.Security.PermissionSet\"\r\nversion=\"1\"/>\r\n</PolicyStatement>\r\n</DefaultGrant>\r\n<ExtraInfo Data=\"0001000000FFFFFFFF01000000000000000601000000044D6F6E6F0B\"/>\r\n</ApplicationTrust>\r\n");
			Assert.AreEqual (expected, AdjustLineEnds (at.ToXml ().ToString ()), "XML");

			at.ExtraInfo = null;
			Assert.IsNull (at.ExtraInfo, "null");
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void ExtraInfo_NotSerializable ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.ExtraInfo = this;
			SecurityElement se = at.ToXml ();
		}

		[Test]
		public void IsApplicationTrustedToRun ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.IsApplicationTrustedToRun = true;
			Assert.IsTrue (at.IsApplicationTrustedToRun);

			string expected = AdjustLineEnds ("<ApplicationTrust version=\"1\"\r\nTrustedToRun=\"true\">\r\n<DefaultGrant>\r\n<PolicyStatement version=\"1\">\r\n<PermissionSet class=\"System.Security.PermissionSet\"\r\nversion=\"1\"/>\r\n</PolicyStatement>\r\n</DefaultGrant>\r\n</ApplicationTrust>\r\n");
			Assert.AreEqual (expected, AdjustLineEnds (at.ToXml ().ToString ()), "XML");

			at.IsApplicationTrustedToRun = false;
			Assert.IsFalse (at.IsApplicationTrustedToRun);
		}

		[Test]
		public void Persist ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.Persist = true;
			Assert.IsTrue (at.Persist, "true");

			string expected = AdjustLineEnds ("<ApplicationTrust version=\"1\"\r\nPersist=\"true\">\r\n<DefaultGrant>\r\n<PolicyStatement version=\"1\">\r\n<PermissionSet class=\"System.Security.PermissionSet\"\r\nversion=\"1\"/>\r\n</PolicyStatement>\r\n</DefaultGrant>\r\n</ApplicationTrust>\r\n");
			Assert.AreEqual (expected, AdjustLineEnds (at.ToXml ().ToString ()), "XML");

			at.Persist = false;
			Assert.IsFalse (at.Persist, "false");
		}

		[Test]
		public void ToFromXmlRoundtrip ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.ApplicationIdentity = new ApplicationIdentity ("Mono Unit Test");
			at.DefaultGrantSet = new PolicyStatement (new PermissionSet (PermissionState.Unrestricted));
			at.ExtraInfo = "Mono";
			at.IsApplicationTrustedToRun = true;
			at.Persist = true;

			SecurityElement se = at.ToXml ();
			string expected = AdjustLineEnds ("<ApplicationTrust version=\"1\"\r\nFullName=\"Mono Unit Test, Culture=neutral\"\r\nTrustedToRun=\"true\"\r\nPersist=\"true\">\r\n<DefaultGrant>\r\n<PolicyStatement version=\"1\">\r\n<PermissionSet class=\"System.Security.PermissionSet\"\r\nversion=\"1\"\r\nUnrestricted=\"true\"/>\r\n</PolicyStatement>\r\n</DefaultGrant>\r\n<ExtraInfo Data=\"0001000000FFFFFFFF01000000000000000601000000044D6F6E6F0B\"/>\r\n</ApplicationTrust>\r\n");
			Assert.AreEqual (expected, AdjustLineEnds (at.ToXml ().ToString ()), "XML");

			ApplicationTrust copy = new ApplicationTrust ();
			copy.FromXml (se);
			se = copy.ToXml ();
			Assert.AreEqual (expected, AdjustLineEnds (at.ToXml ().ToString ()), "Copy");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			at.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidTag ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			SecurityElement se = at.ToXml ();
			se.Tag = "MonoTrust";
			at.FromXml (se);
		}

		[Test]
		public void FromXml_InvalidVersion ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			SecurityElement se = at.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", "2");
			foreach (SecurityElement child in se.Children)
				w.AddChild (child);

			at.FromXml (w);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			SecurityElement se = at.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			foreach (SecurityElement child in se.Children)
				w.AddChild (child);

			at.FromXml (w);
		}

		[Test]
		public void FromXml_NoChild ()
		{
			ApplicationTrust at = new ApplicationTrust ();
			SecurityElement se = at.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", "1");

			at.FromXml (w);

			Assert.IsNull (at.ApplicationIdentity, "ApplicationIdentity");
			Assert.AreEqual (PolicyStatementAttribute.Nothing, at.DefaultGrantSet.Attributes, "DefaultGrantSet.Attributes");
			Assert.AreEqual (String.Empty, at.DefaultGrantSet.AttributeString, "DefaultGrantSet.AttributeString");
			Assert.IsTrue (at.DefaultGrantSet.PermissionSet.IsEmpty (), "DefaultGrantSet.PermissionSet.IsEmpty");
			Assert.IsFalse (at.DefaultGrantSet.PermissionSet.IsUnrestricted (), "DefaultGrantSet.PermissionSet.IsUnrestricted");
			Assert.IsNull (at.ExtraInfo, "ExtraInfo");
			Assert.IsFalse (at.IsApplicationTrustedToRun, "IsApplicationTrustedToRun");
			Assert.IsFalse (at.Persist, "Persist");
		}
	}
}

#endif
