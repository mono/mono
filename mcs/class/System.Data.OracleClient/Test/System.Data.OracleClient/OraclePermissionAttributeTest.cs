//
// OraclePermissionAttributeTest.cs -
//	NUnit Test Cases for OraclePermissionAttribute
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using NUnit.Framework;
using System;
using System.Data;
using System.Data.OracleClient;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Data.OracleClient {

	[TestFixture]
	public class OraclePermissionAttributeTest {

		[Test]
		public void Default ()
		{
			OraclePermissionAttribute a = new OraclePermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.IsFalse (a.AllowBlankPassword, "AllowBlankPassword");
			Assert.AreEqual (String.Empty, a.ConnectionString, "ConnectionString");
			Assert.AreEqual (KeyRestrictionBehavior.AllowOnly, a.KeyRestrictionBehavior, "KeyRestrictionBehavior");
			Assert.AreEqual (String.Empty, a.KeyRestrictions, "KeyRestrictions");
			Assert.IsFalse (a.ShouldSerializeConnectionString (), "ShouldSerializeConnectionString");
			Assert.IsFalse (a.ShouldSerializeKeyRestrictions (), "ShouldSerializeConnectionString");
			OraclePermission ocp = (OraclePermission)a.CreatePermission ();
			Assert.IsFalse (ocp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void Action ()
		{
			OraclePermissionAttribute a = new OraclePermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (SecurityAction.Assert, a.Action, "Action=Assert");
			a.Action = SecurityAction.Demand;
			Assert.AreEqual (SecurityAction.Demand, a.Action, "Action=Demand");
			a.Action = SecurityAction.Deny;
			Assert.AreEqual (SecurityAction.Deny, a.Action, "Action=Deny");
			a.Action = SecurityAction.InheritanceDemand;
			Assert.AreEqual (SecurityAction.InheritanceDemand, a.Action, "Action=InheritanceDemand");
			a.Action = SecurityAction.LinkDemand;
			Assert.AreEqual (SecurityAction.LinkDemand, a.Action, "Action=LinkDemand");
			a.Action = SecurityAction.PermitOnly;
			Assert.AreEqual (SecurityAction.PermitOnly, a.Action, "Action=PermitOnly");
			a.Action = SecurityAction.RequestMinimum;
			Assert.AreEqual (SecurityAction.RequestMinimum, a.Action, "Action=RequestMinimum");
			a.Action = SecurityAction.RequestOptional;
			Assert.AreEqual (SecurityAction.RequestOptional, a.Action, "Action=RequestOptional");
			a.Action = SecurityAction.RequestRefuse;
			Assert.AreEqual (SecurityAction.RequestRefuse, a.Action, "Action=RequestRefuse");
		}

		[Test]
		public void Action_Invalid ()
		{
			OraclePermissionAttribute a = new OraclePermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Unrestricted ()
		{
			OraclePermissionAttribute a = new OraclePermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			OraclePermission ocp = (OraclePermission)a.CreatePermission ();
			Assert.IsTrue (ocp.IsUnrestricted (), "IsUnrestricted");
			Assert.IsFalse (a.AllowBlankPassword, "AllowBlankPassword");
			Assert.AreEqual (String.Empty, a.ConnectionString, "ConnectionString");
			Assert.AreEqual (KeyRestrictionBehavior.AllowOnly, a.KeyRestrictionBehavior, "KeyRestrictionBehavior");
			Assert.AreEqual (String.Empty, a.KeyRestrictions, "KeyRestrictions");
			a.Unrestricted = false;
			ocp = (OraclePermission)a.CreatePermission ();
			Assert.IsFalse (ocp.IsUnrestricted (), "!IsUnrestricted");
		}

		[Test]
		public void AllowBlankPassword ()
		{
			OraclePermissionAttribute a = new OraclePermissionAttribute (SecurityAction.Assert);
			Assert.IsFalse (a.AllowBlankPassword, "Default");
			a.AllowBlankPassword = true;
			Assert.IsTrue (a.AllowBlankPassword, "True");
			a.AllowBlankPassword = false;
			Assert.IsFalse (a.AllowBlankPassword, "False");
		}

		[Test]
		public void ConnectionString ()
		{
			OraclePermissionAttribute a = new OraclePermissionAttribute (SecurityAction.Assert);
			a.ConnectionString = String.Empty;
			Assert.AreEqual (String.Empty, a.ConnectionString, "Empty");
			a.ConnectionString = "Mono";
			Assert.AreEqual ("Mono", a.ConnectionString, "Mono");
			a.ConnectionString = null;
			Assert.AreEqual (String.Empty, a.ConnectionString, "Empty(null)");
		}

		[Test]
		public void KeyRestrictionBehavior_All ()
		{
			OraclePermissionAttribute a = new OraclePermissionAttribute (SecurityAction.Assert);
			a.KeyRestrictionBehavior = KeyRestrictionBehavior.AllowOnly;
			Assert.AreEqual (KeyRestrictionBehavior.AllowOnly, a.KeyRestrictionBehavior, "AllowOnly");
			a.KeyRestrictionBehavior = KeyRestrictionBehavior.PreventUsage;
			Assert.AreEqual (KeyRestrictionBehavior.PreventUsage, a.KeyRestrictionBehavior, "PreventUsage");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void KeyRestrictionBehavior_Invalid ()
		{
			OraclePermissionAttribute a = new OraclePermissionAttribute (SecurityAction.Assert);
			a.KeyRestrictionBehavior = (KeyRestrictionBehavior)Int32.MinValue;
		}

		[Test]
		public void KeyRestriction ()
		{
			OraclePermissionAttribute a = new OraclePermissionAttribute (SecurityAction.Assert);
			a.KeyRestrictions = String.Empty;
			Assert.AreEqual (String.Empty, a.KeyRestrictions, "Empty");
			a.KeyRestrictions = "Mono";
			Assert.AreEqual ("Mono", a.KeyRestrictions, "Mono");
			a.KeyRestrictions = null;
			Assert.AreEqual (String.Empty, a.KeyRestrictions, "Empty(null)");
		}
		[Test]
		public void Attributes ()
		{
			Type t = typeof (OraclePermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method;
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
