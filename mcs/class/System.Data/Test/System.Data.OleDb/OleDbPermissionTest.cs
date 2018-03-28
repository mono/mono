//
// OleDbPermissionTest.cs - NUnit Test Cases for OleDbPermission
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

#if !NO_OLEDB

using NUnit.Framework;
using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Data.OleDb {

	// NOTE: Most tests are are located in the base class, DBDataPermission

	[TestFixture]
	public class OleDbPermissionTest {

		private void Check (string msg, OleDbPermission perm, bool blank, bool unrestricted, int count)
		{
			Assert.AreEqual (blank, perm.AllowBlankPassword, msg + ".AllowBlankPassword");
			Assert.AreEqual (unrestricted, perm.IsUnrestricted (), msg + ".IsUnrestricted");
			if (count == 0)
				Assert.IsNull (perm.ToXml ().Children, msg + ".Count != 0");
			else
				Assert.AreEqual (count, perm.ToXml ().Children.Count, msg + ".Count");
			Assert.AreEqual (String.Empty, perm.Provider, "Provider");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void PermissionState_Invalid ()
		{
			PermissionState ps = (PermissionState)Int32.MinValue;
			OleDbPermission perm = new OleDbPermission (ps);
		}

		[Test]
		public void None ()
		{
			OleDbPermission perm = new OleDbPermission (PermissionState.None);
			Check ("None-1", perm, false, false, 0);
			perm.AllowBlankPassword = true;
			Check ("None-2", perm, true, false, 0);

			OleDbPermission copy = (OleDbPermission)perm.Copy ();
			Check ("Copy_None-1", copy, true, false, 0);
			copy.AllowBlankPassword = false;
			Check ("Copy_None-2", copy, false, false, 0);
		}

		[Test]
		public void None_Childs ()
		{
			OleDbPermission perm = new OleDbPermission (PermissionState.None);
			perm.Add ("data source=localhost;", String.Empty, KeyRestrictionBehavior.AllowOnly);
			perm.Add ("data source=127.0.0.1;", "password=;", KeyRestrictionBehavior.PreventUsage);

			Check ("None-Childs-1", perm, false, false, 2);
			perm.AllowBlankPassword = true;
			Check ("None-Childs-2", perm, true, false, 2);

			OleDbPermission copy = (OleDbPermission)perm.Copy ();
			Check ("Copy_None-Childs-1", copy, true, false, 2);
			copy.AllowBlankPassword = false;
			Check ("Copy_None-Childs-2", copy, false, false, 2);
		}

		[Test]
		public void Unrestricted ()
		{
			OleDbPermission perm = new OleDbPermission (PermissionState.Unrestricted);
			Check ("Unrestricted-1", perm, false, true, 0);
			perm.AllowBlankPassword = true;
			Check ("Unrestricted-2", perm, true, true, 0);

			OleDbPermission copy = (OleDbPermission)perm.Copy ();
			// note: Unrestricted is always created with default values (so AllowBlankPassword is false)
			Check ("Copy_Unrestricted-1", copy, false, true, 0);
			copy.AllowBlankPassword = true;
			Check ("Copy_Unrestricted-2", copy, true, true, 0);
		}

		[Test]
		public void Unrestricted_Add ()
		{
			OleDbPermission perm = new OleDbPermission (PermissionState.Unrestricted);
			Check ("Unrestricted-NoChild", perm, false, true, 0);
			perm.Add ("data source=localhost;", String.Empty, KeyRestrictionBehavior.AllowOnly);
			// note: Lost unrestricted state when children was added
			Check ("Unrestricted-WithChild", perm, false, false, 1);
		}

		[Test]
		public void Provider ()
		{
			OleDbPermission perm = new OleDbPermission (PermissionState.None);
			perm.Provider = String.Empty;
			Assert.AreEqual (String.Empty, perm.Provider, "Empty");
			perm.Provider = "Mono";
			Assert.AreEqual ("Mono", perm.Provider, "Mono");
			perm.Provider = null;
			Assert.AreEqual (String.Empty, perm.Provider, "Empty(null)");
		}
	}
}

#endif