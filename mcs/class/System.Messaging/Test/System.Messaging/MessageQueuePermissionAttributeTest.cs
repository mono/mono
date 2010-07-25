//
// MessageQueuePermissionAttributeTest.cs -
//	NUnit Test Cases for MessageQueuePermissionAttribute
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Messaging;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Messaging {

	[TestFixture]
	public class MessageQueuePermissionAttributeTest {

		[Test]
		public void Default ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.IsNull (a.Category, "Category");
			Assert.IsNull (a.Label, "Label");
			Assert.IsNull (a.MachineName, "MachineName");
			Assert.IsNull (a.Path, "Path");
			Assert.AreEqual (MessageQueuePermissionAccess.None, a.PermissionAccess, "PermissionAccess");

			a.MachineName = "localhost";
			MessageQueuePermission sp = (MessageQueuePermission)a.CreatePermission ();
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void Action ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
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
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Unrestricted ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.MachineName = "localhost";
			a.Unrestricted = true;
			MessageQueuePermission mqp = (MessageQueuePermission)a.CreatePermission ();
			Assert.IsTrue (mqp.IsUnrestricted (), "IsUnrestricted");

			a.Unrestricted = false;
			mqp = (MessageQueuePermission)a.CreatePermission ();
			Assert.IsFalse (mqp.IsUnrestricted (), "!IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Category_Null ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.Category = null;
		}

		[Test]
		public void Category ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.Category = "Mono";
			Assert.AreEqual ("Mono", a.Category, "Category-1");
			a.Category = String.Empty;
			Assert.AreEqual (String.Empty, a.Category, "Category-2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Label_Null ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.Label = null;
		}

		[Test]
		public void Label ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.Label = "Mono";
			Assert.AreEqual ("Mono", a.Label, "Label-1");
			a.Label = String.Empty;
			Assert.AreEqual (String.Empty, a.Label, "Label-2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MachineName_Null ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.MachineName = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MachineName_Invalid ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.MachineName = String.Empty;
		}

		[Test]
		public void MachineName ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.MachineName = "Mono";
			Assert.AreEqual ("Mono", a.MachineName, "MachineName-1");

			for (int i = 0; i < 256; i++) {
				try{
					a.MachineName = Convert.ToChar (i).ToString ();
					Assert.AreEqual (i, (int)a.MachineName [0], i.ToString ());
				}
				catch {
					switch (i) {
						case 9:
						case 10:
						case 11:
						case 12:
						case 13:
						case 32:
						case 92:
#if NET_2_0
						case 133:
#endif
						case 160:
							// known invalid chars
							break;
						default:
							Assert.Fail (i.ToString ());
							break;
					}
				}
			}
			// all first 256 characters seems to be valid
			// is there other rules ?
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Path_Null ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.Path = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Path_Invalid ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.Path = "Mono";
		}

		[Test]
		public void Path ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.Path = "\\Mono";
			Assert.AreEqual ("\\Mono", a.Path, "Path-1");
			a.Path = "\\";
			Assert.AreEqual ("\\", a.Path, "Path-2");
			a.Path = String.Empty;
			Assert.AreEqual (String.Empty, a.Path, "Path-3");
		}

		[Test]
		public void PermissionAccess_Invalid ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.PermissionAccess = (MessageQueuePermissionAccess) Int32.MinValue;
			Assert.AreEqual (Int32.MinValue, (int)a.PermissionAccess);
		}

		[Test]
		public void PermissionAccess ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.PermissionAccess = MessageQueuePermissionAccess.None;
			Assert.AreEqual (MessageQueuePermissionAccess.None, a.PermissionAccess, "None");
			a.PermissionAccess = MessageQueuePermissionAccess.Browse;
			Assert.AreEqual (MessageQueuePermissionAccess.Browse, a.PermissionAccess, "Browse");
			a.PermissionAccess = MessageQueuePermissionAccess.Send;
			Assert.AreEqual (MessageQueuePermissionAccess.Send, a.PermissionAccess, "Send");
			a.PermissionAccess = MessageQueuePermissionAccess.Peek;
			Assert.AreEqual (MessageQueuePermissionAccess.Peek, a.PermissionAccess, "Peek");
			a.PermissionAccess = MessageQueuePermissionAccess.Receive;
			Assert.AreEqual (MessageQueuePermissionAccess.Receive, a.PermissionAccess, "Receive");
			a.PermissionAccess = MessageQueuePermissionAccess.Administer;
			Assert.AreEqual (MessageQueuePermissionAccess.Administer, a.PermissionAccess, "Administer");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreatePermission_WithoutMachineName ()
		{
			MessageQueuePermissionAttribute a = new MessageQueuePermissionAttribute (SecurityAction.Assert);
			a.CreatePermission ();
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (MessageQueuePermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Event);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
