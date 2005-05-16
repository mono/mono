//
// UIPermissionTest.cs - NUnit Test Cases for UIPermission
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
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class UIPermissionTest {

		[Test]
		public void PermissionStateNone ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, uip.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.NoWindows, uip.Window, "Window");
			Assert.IsFalse (uip.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = uip.ToXml ();
			Assert.IsNull (se.Attribute ("Unrestricted"), "Xml-Unrestricted");

			UIPermission copy = (UIPermission)uip.Copy ();
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, uip.Clipboard, "Copy-Clipboard");
			Assert.AreEqual (UIPermissionWindow.NoWindows, uip.Window, "Copy-Window");
			Assert.IsFalse (uip.IsUnrestricted (), "Copy-IsUnrestricted");
		}

		[Test]
		public void PermissionStateUnrestricted ()
		{
			UIPermission uip = new UIPermission (PermissionState.Unrestricted);
			Assert.AreEqual (UIPermissionClipboard.AllClipboard, uip.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.AllWindows, uip.Window, "Window");
			Assert.IsTrue (uip.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = uip.ToXml ();
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");

			UIPermission copy = (UIPermission)uip.Copy ();
			Assert.AreEqual (UIPermissionClipboard.AllClipboard, uip.Clipboard, "Copy-Clipboard");
			Assert.AreEqual (UIPermissionWindow.AllWindows, uip.Window, "Copy-Window");
			Assert.IsTrue (uip.IsUnrestricted (), "Copy-IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionStateInvalid ()
		{
			UIPermission uip = new UIPermission ((PermissionState)2);
		}

		[Test]
		public void UIPermission_Clipboard_All ()
		{
			UIPermission uip = new UIPermission (UIPermissionClipboard.AllClipboard);
			Assert.AreEqual (UIPermissionClipboard.AllClipboard, uip.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.NoWindows, uip.Window, "Window");
			Assert.IsFalse (uip.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void UIPermission_Clipboard_Own ()
		{
			UIPermission uip = new UIPermission (UIPermissionClipboard.OwnClipboard);
			Assert.AreEqual (UIPermissionClipboard.OwnClipboard, uip.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.NoWindows, uip.Window, "Window");
			Assert.IsFalse (uip.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void UIPermission_Clipboard_No ()
		{
			UIPermission uip = new UIPermission (UIPermissionClipboard.NoClipboard);
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, uip.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.NoWindows, uip.Window, "Window");
			Assert.IsFalse (uip.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UIPermission_Clipboard_Bad ()
		{
			UIPermission uip = new UIPermission ((UIPermissionClipboard)128);
		}

		[Test]
		public void UIPermission_Windows_All ()
		{
			UIPermission uip = new UIPermission (UIPermissionWindow.AllWindows);
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, uip.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.AllWindows, uip.Window, "Window");
			Assert.IsFalse (uip.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void UIPermission_Windows_SafeSubWindows ()
		{
			UIPermission uip = new UIPermission (UIPermissionWindow.SafeSubWindows);
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, uip.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.SafeSubWindows, uip.Window, "Window");
			Assert.IsFalse (uip.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void UIPermission_Windows_SafeTopLevelWindows ()
		{
			UIPermission uip = new UIPermission (UIPermissionWindow.SafeTopLevelWindows);
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, uip.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.SafeTopLevelWindows, uip.Window, "Window");
			Assert.IsFalse (uip.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		public void UIPermission_Windows_No ()
		{
			UIPermission uip = new UIPermission (UIPermissionWindow.NoWindows);
			Assert.AreEqual (UIPermissionClipboard.NoClipboard, uip.Clipboard, "Clipboard");
			Assert.AreEqual (UIPermissionWindow.NoWindows, uip.Window, "Window");
			Assert.IsFalse (uip.IsUnrestricted (), "IsUnrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UIPermission_Windows_Bad ()
		{
			UIPermission uip = new UIPermission ((UIPermissionWindow)128);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Clipboard_Bad ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			uip.Clipboard = (UIPermissionClipboard) 128;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Window_Bad ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			uip.Window = (UIPermissionWindow) 128;
		}

		[Test]
		public void Unrestricted () 
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();
			// attribute value is not case-sensitive
			se.AddAttribute ("Unrestricted", "TRUE");
			uip.FromXml (se);
			Assert.IsTrue (uip.IsUnrestricted (), "IsUnrestricted-TRUE");

			uip = new UIPermission (PermissionState.None);
			// attribute name is not case-sensitive either!!!
			se.AddAttribute ("UNRESTRICTED", "TRUE");
			uip.FromXml (se);
			Assert.IsTrue (uip.IsUnrestricted (), "IsUnrestricted-UPPER");
		}

		private void Compare (UIPermission uip1, UIPermission uip2, string prefix)
		{
			Assert.AreEqual (uip1.Clipboard, uip2.Clipboard, prefix + ".Clipboard");
			Assert.AreEqual (uip1.Window, uip2.Window, prefix + ".Window");
			Assert.AreEqual (uip1.IsUnrestricted (), uip2.IsUnrestricted (), prefix + ".IsUnrestricted ()");
		}

		[Test]
		public void Intersect ()
		{
			UIPermission clip_all = new UIPermission (UIPermissionClipboard.AllClipboard);
			UIPermission clip_own = new UIPermission (UIPermissionClipboard.OwnClipboard);
			UIPermission intersect = (UIPermission)clip_all.Intersect (clip_own);
			Compare (clip_own, intersect, "clip_all N clip_own");
			Assert.IsFalse (Object.ReferenceEquals (clip_own, intersect), "!ReferenceEquals1");
			Assert.IsTrue (intersect.IsSubsetOf (clip_all), "intersect.IsSubsetOf (clip_all)");
			Assert.IsTrue (intersect.IsSubsetOf (clip_own), "intersect.IsSubsetOf (clip_own)");

			UIPermission win_all = new UIPermission (UIPermissionWindow.AllWindows);
			UIPermission win_safe = new UIPermission (UIPermissionWindow.SafeSubWindows);
			intersect = (UIPermission) win_all.Intersect (win_safe);
			Compare (win_safe, intersect, "win_all N win_safe");
			Assert.IsFalse (Object.ReferenceEquals (win_safe, intersect), "!ReferenceEquals2");
			Assert.IsTrue (intersect.IsSubsetOf (win_all), "intersect.IsSubsetOf (win_all)");
			Assert.IsTrue (intersect.IsSubsetOf (win_safe), "intersect.IsSubsetOf (win_safe)");

			intersect = (UIPermission)win_all.Intersect (clip_all);
			Assert.IsNull (intersect, "win_all N clip_all");

			intersect = (UIPermission)win_all.Intersect (null);
			Assert.IsNull (intersect, "win_all N null");

			intersect = (UIPermission)clip_all.Intersect (null);
			Assert.IsNull (intersect, "clip_all N null");

			UIPermission empty = new UIPermission (PermissionState.None);
			intersect = (UIPermission)clip_all.Intersect (empty);
			Assert.IsNull (intersect, "clip_all N null");

			UIPermission unrestricted = new UIPermission (PermissionState.Unrestricted);
			intersect = (UIPermission)clip_all.Intersect (unrestricted);
			Compare (clip_all, intersect, "clip_all N unrestricted");
			Assert.IsFalse (Object.ReferenceEquals (clip_all, intersect), "!ReferenceEquals4");
			Assert.IsTrue (intersect.IsSubsetOf (clip_all), "intersect.IsSubsetOf (clip_all)");
			Assert.IsTrue (intersect.IsSubsetOf (unrestricted), "intersect.IsSubsetOf (unrestricted)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_DifferentPermissions ()
		{
			UIPermission a = new UIPermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.Intersect (b);
		}

		[Test]
		public void IsSubsetOf ()
		{
			UIPermission unrestricted = new UIPermission (PermissionState.Unrestricted);
			UIPermission empty = new UIPermission (PermissionState.None);

			Assert.IsFalse (unrestricted.IsSubsetOf (empty), "unrestricted.IsSubsetOf (empty)");
			Assert.IsTrue (empty.IsSubsetOf (unrestricted), "empty.IsSubsetOf (unrestricted)");
			Assert.IsTrue (empty.IsSubsetOf (null), "empty.IsSubsetOf (null)");
			Assert.IsFalse (unrestricted.IsSubsetOf (null), "unrestricted.IsSubsetOf (null)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOf_DifferentPermissions ()
		{
			UIPermission a = new UIPermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.IsSubsetOf (b);
		}

		[Test]
		public void Union ()
		{
			UIPermission none = new UIPermission (PermissionState.None);
			UIPermission union = (UIPermission)none.Union (null);
			Compare (none, union, "none U null");
			Assert.IsFalse (Object.ReferenceEquals (none, union), "!ReferenceEquals1");
			Assert.IsTrue (none.IsSubsetOf (union), "none.IsSubsetOf (union)");

			union = (UIPermission)none.Union (new UIPermission (PermissionState.None));
			Assert.IsNull (union, "none U none");
			Assert.IsTrue (none.IsSubsetOf (null), "none.IsSubsetOf (null)");

			UIPermission unrestricted = new UIPermission (PermissionState.Unrestricted);
			union = (UIPermission)none.Union (unrestricted);
			Compare (unrestricted, union, "none U unrestricted");
			Assert.IsTrue (union.IsUnrestricted (), "IsUnrestricted-1");
			Assert.IsFalse (Object.ReferenceEquals (unrestricted, union), "!ReferenceEquals2");
			Assert.IsTrue (none.IsSubsetOf (union), "none.IsSubsetOf (union)");
			Assert.IsTrue (unrestricted.IsSubsetOf (union), "unrestricted.IsSubsetOf (union)");

			union = (UIPermission)unrestricted.Union (unrestricted);
			Compare (unrestricted, union, "unrestricted U unrestricted");
			Assert.IsTrue (union.IsUnrestricted (), "IsUnrestricted-2");
			Assert.IsFalse (Object.ReferenceEquals (unrestricted, union), "!ReferenceEquals3");
			Assert.IsTrue (unrestricted.IsSubsetOf (union), "unrestricted.IsSubsetOf (union)");

			UIPermission clip_all = new UIPermission (UIPermissionClipboard.AllClipboard);
			UIPermission win_all = new UIPermission (UIPermissionWindow.AllWindows);
			union = (UIPermission)clip_all.Union (win_all);
			Compare (unrestricted, union, "clip_all U win_all");
			Assert.IsTrue (union.IsUnrestricted (), "IsUnrestricted-3");
			Assert.IsTrue (clip_all.IsSubsetOf (union), "clip_all.IsSubsetOf (union)");
			Assert.IsTrue (win_all.IsSubsetOf (union), "win_all.IsSubsetOf (union)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_DifferentPermissions ()
		{
			UIPermission a = new UIPermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.Union (b);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			uip.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();
			se.Tag = "IMono"; // instead of IPermission
			uip.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			uip.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			uip.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			uip.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "2");
			uip.FromXml (w);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			UIPermission uip = new UIPermission (PermissionState.None);
			SecurityElement se = uip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			uip.FromXml (w);
		}
	}
}
