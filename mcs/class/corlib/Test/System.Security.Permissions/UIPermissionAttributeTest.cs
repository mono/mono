//
// UIPermissionAttributeTest.cs - NUnit Test Cases for UIPermissionAttribute
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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
