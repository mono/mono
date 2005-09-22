//
// UIPermissionAttributeTest.cs - NUnit Test Cases for UIPermissionAttribute
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class UIPermissionAttributeTest {

		[Test]
		public void Default () 
		{
			UIPermissionAttribute a = new UIPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, a.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.NoWindows, a.Window, "Window");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			UIPermission perm = (UIPermission) a.CreatePermission ();
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, perm.Clipboard, "CreatePermission-Clipboard");
			Assert.AreEqual (UIPermissionWindow.NoWindows, perm.Window, "CreatePermission-Window");
		}

		[Test]
		public void Action () 
		{
			UIPermissionAttribute a = new UIPermissionAttribute (SecurityAction.Assert);
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
			UIPermissionAttribute a = new UIPermissionAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void Clipboard () 
		{
			UIPermissionAttribute a = new UIPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, a.Clipboard, "Clipboard=NoClipboard");
			a.Clipboard = UIPermissionClipboard.OwnClipboard;
			Assert.AreEqual (UIPermissionClipboard.OwnClipboard, a.Clipboard, "Clipboard=OwnClipboard");
			a.Clipboard = UIPermissionClipboard.AllClipboard;
			Assert.AreEqual (UIPermissionClipboard.AllClipboard, a.Clipboard, "Clipboard=AllClipboard");
		}

		[Test]
		public void Clipboard_Invalid ()
		{
			UIPermissionAttribute a = new UIPermissionAttribute (SecurityAction.Assert);
			a.Clipboard = (UIPermissionClipboard)Int32.MinValue;
			// no validation in attribute
		}

		[Test]
		public void Window () 
		{
			UIPermissionAttribute a = new UIPermissionAttribute (SecurityAction.Assert);
			Assert.AreEqual (UIPermissionWindow.NoWindows, a.Window, "Window=NoWindows");
			a.Window = UIPermissionWindow.SafeSubWindows;
			Assert.AreEqual (UIPermissionWindow.SafeSubWindows, a.Window, "Window=SafeSubWindows");
			a.Window = UIPermissionWindow.SafeTopLevelWindows;
			Assert.AreEqual (UIPermissionWindow.SafeTopLevelWindows, a.Window, "Window=SafeTopLevelWindows");
			a.Window = UIPermissionWindow.AllWindows;
			Assert.AreEqual (UIPermissionWindow.AllWindows, a.Window, "Window=AllWindows");
		}

		[Test]
		public void Window_Invalid () 
		{
			UIPermissionAttribute a = new UIPermissionAttribute (SecurityAction.Assert);
			a.Window = (UIPermissionWindow)Int32.MinValue;
			// no validation in attribute
		}

		[Test]
		public void Unrestricted () 
		{
			UIPermissionAttribute a = new UIPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			UIPermission perm = (UIPermission) a.CreatePermission ();
			Assert.AreEqual (UIPermissionClipboard.AllClipboard, perm.Clipboard, "Unrestricted-Clipboard");
			Assert.AreEqual (UIPermissionWindow.AllWindows, perm.Window, "Unrestricted-Window");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (UIPermissionAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
