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
	public class UIPermissionAttributeTest : Assertion {

		[Test]
		public void Default () 
		{
			UIPermissionAttribute a = new UIPermissionAttribute (SecurityAction.Assert);
			AssertEquals ("Clipboard", UIPermissionClipboard.NoClipboard, a.Clipboard);
			AssertEquals ("Window", UIPermissionWindow.NoWindows, a.Window);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);

			UIPermission perm = (UIPermission) a.CreatePermission ();
			AssertEquals ("CreatePermission-Clipboard", UIPermissionClipboard.NoClipboard, perm.Clipboard);
			AssertEquals ("CreatePermission-Window", UIPermissionWindow.NoWindows, perm.Window);
		}

		[Test]
		public void Action () 
		{
			UIPermissionAttribute a = new UIPermissionAttribute (SecurityAction.Assert);
			AssertEquals ("Action=Assert", SecurityAction.Assert, a.Action);
			a.Action = SecurityAction.Demand;
			AssertEquals ("Action=Demand", SecurityAction.Demand, a.Action);
			a.Action = SecurityAction.Deny;
			AssertEquals ("Action=Deny", SecurityAction.Deny, a.Action);
			a.Action = SecurityAction.InheritanceDemand;
			AssertEquals ("Action=InheritanceDemand", SecurityAction.InheritanceDemand, a.Action);
			a.Action = SecurityAction.LinkDemand;
			AssertEquals ("Action=LinkDemand", SecurityAction.LinkDemand, a.Action);
			a.Action = SecurityAction.PermitOnly;
			AssertEquals ("Action=PermitOnly", SecurityAction.PermitOnly, a.Action);
			a.Action = SecurityAction.RequestMinimum;
			AssertEquals ("Action=RequestMinimum", SecurityAction.RequestMinimum, a.Action);
			a.Action = SecurityAction.RequestOptional;
			AssertEquals ("Action=RequestOptional", SecurityAction.RequestOptional, a.Action);
			a.Action = SecurityAction.RequestRefuse;
			AssertEquals ("Action=RequestRefuse", SecurityAction.RequestRefuse, a.Action);
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
			AssertEquals ("Clipboard=NoClipboard", UIPermissionClipboard.NoClipboard, a.Clipboard);
			a.Clipboard = UIPermissionClipboard.OwnClipboard;
			AssertEquals ("Clipboard=OwnClipboard", UIPermissionClipboard.OwnClipboard, a.Clipboard);
			a.Clipboard = UIPermissionClipboard.AllClipboard;
			AssertEquals ("Clipboard=AllClipboard", UIPermissionClipboard.AllClipboard, a.Clipboard);
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
			AssertEquals ("Window=NoWindows", UIPermissionWindow.NoWindows, a.Window);
			a.Window = UIPermissionWindow.SafeSubWindows;
			AssertEquals ("Window=SafeSubWindows", UIPermissionWindow.SafeSubWindows, a.Window);
			a.Window = UIPermissionWindow.SafeTopLevelWindows;
			AssertEquals ("Window=SafeTopLevelWindows", UIPermissionWindow.SafeTopLevelWindows, a.Window);
			a.Window = UIPermissionWindow.AllWindows;
			AssertEquals ("Window=AllWindows", UIPermissionWindow.AllWindows, a.Window);
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
			AssertEquals ("Unrestricted-Clipboard", UIPermissionClipboard.AllClipboard, perm.Clipboard);
			AssertEquals ("Unrestricted-Window", UIPermissionWindow.AllWindows, perm.Window);
		}
	}
}
