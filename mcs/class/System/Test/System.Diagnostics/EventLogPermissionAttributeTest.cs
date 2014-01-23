//
// EventLogPermissionAttributeTest.cs -
//	NUnit Test Cases for EventLogPermissionAttribute
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

#if !MOBILE

using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace MonoTests.System.Diagnostics {

	[TestFixture]
	public class EventLogPermissionAttributeTest {

		[Test]
		public void Default ()
		{
			EventLogPermissionAttribute a = new EventLogPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");
			Assert.AreEqual (".", a.MachineName, "MachineName");
#if NET_2_0
			Assert.AreEqual (EventLogPermissionAccess.Write, a.PermissionAccess, "PermissionAccess");
#else
			Assert.AreEqual (EventLogPermissionAccess.Browse, a.PermissionAccess, "PermissionAccess");
#endif
			EventLogPermission sp = (EventLogPermission)a.CreatePermission ();
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void Action ()
		{
			EventLogPermissionAttribute a = new EventLogPermissionAttribute (SecurityAction.Assert);
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
			EventLogPermissionAttribute a = new EventLogPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Unrestricted ()
		{
			EventLogPermissionAttribute a = new EventLogPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			EventLogPermission wp = (EventLogPermission)a.CreatePermission ();
			Assert.IsTrue (wp.IsUnrestricted (), "IsUnrestricted");

			a.Unrestricted = false;
			wp = (EventLogPermission)a.CreatePermission ();
			Assert.IsFalse (wp.IsUnrestricted (), "!IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MachineName_Null ()
		{
			EventLogPermissionAttribute a = new EventLogPermissionAttribute (SecurityAction.Assert);
			a.MachineName = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MachineName_Empty ()
		{
			EventLogPermissionAttribute a = new EventLogPermissionAttribute (SecurityAction.Assert);
			a.MachineName = String.Empty;
		}

		[Test]
		public void MachineName ()
		{
			EventLogPermissionAttribute a = new EventLogPermissionAttribute (SecurityAction.Assert);
			for (int i=0; i < 256; i++) {
				try {
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
							Assert.Fail (i.ToString());
							break;
					}
				}
			}
			// all first 256 characters seems to be valid
			// is there other rules ?
		}

		[Test]
		public void PermissionAccess ()
		{
			EventLogPermissionAttribute a = new EventLogPermissionAttribute (SecurityAction.Assert);
			a.PermissionAccess = EventLogPermissionAccess.Audit;
			Assert.AreEqual (EventLogPermissionAccess.Audit, a.PermissionAccess, "Audit");
			a.PermissionAccess = EventLogPermissionAccess.Browse;
			Assert.AreEqual (EventLogPermissionAccess.Browse, a.PermissionAccess, "Browse");
			a.PermissionAccess = EventLogPermissionAccess.Instrument;
			Assert.AreEqual (EventLogPermissionAccess.Instrument, a.PermissionAccess, "Instrument");
			a.PermissionAccess = EventLogPermissionAccess.None;
			Assert.AreEqual (EventLogPermissionAccess.None, a.PermissionAccess, "None");
#if NET_2_0
			a.PermissionAccess = EventLogPermissionAccess.Administer;
			Assert.AreEqual (EventLogPermissionAccess.Administer, a.PermissionAccess, "Administer");
			a.PermissionAccess = EventLogPermissionAccess.Write;
			Assert.AreEqual (EventLogPermissionAccess.Write, a.PermissionAccess, "Write");
#endif
		}

		[Test]
		public void PermissionAccess_Invalid ()
		{
			EventLogPermissionAttribute a = new EventLogPermissionAttribute (SecurityAction.Assert);
			a.PermissionAccess = (EventLogPermissionAccess) Int32.MinValue;
			Assert.AreEqual ((EventLogPermissionAccess)Int32.MinValue, a.PermissionAccess, "None");
			// no exception thrown
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (EventLogPermissionAttribute);
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

#endif