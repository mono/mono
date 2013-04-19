//
// SqlClientPermissionTest.cs - NUnit Test Cases for SqlClientPermission
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
using System.Data.Common;
using System.Data.SqlClient;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Data.SqlClient {

	// NOTE: Most tests are are located in the base class, DBDataPermission

	[TestFixture]
#if MOBILE
	[Ignore ("CAS is not supported and parts will be linked away")]
#endif
	public class SqlClientPermissionTest {

		private void Check (string msg, DBDataPermission dbdp, bool blank, bool unrestricted, int count)
		{
			Assert.AreEqual (blank, dbdp.AllowBlankPassword, msg + ".AllowBlankPassword");
			Assert.AreEqual (unrestricted, dbdp.IsUnrestricted (), msg + ".IsUnrestricted");
			if (count == 0)
				Assert.IsNull (dbdp.ToXml ().Children, msg + ".Count != 0");
			else
				Assert.AreEqual (count, dbdp.ToXml ().Children.Count, msg + ".Count");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void PermissionState_Invalid ()
		{
			PermissionState ps = (PermissionState)Int32.MinValue;
			SqlClientPermission perm = new SqlClientPermission (ps);
		}

		[Test]
		public void None ()
		{
			SqlClientPermission perm = new SqlClientPermission (PermissionState.None);
			Check ("None-1", perm, false, false, 0);
			perm.AllowBlankPassword = true;
			Check ("None-2", perm, true, false, 0);

			SqlClientPermission copy = (SqlClientPermission)perm.Copy ();
			Check ("Copy_None-1", copy, true, false, 0);
			copy.AllowBlankPassword = false;
			Check ("Copy_None-2", copy, false, false, 0);
		}

		[Test]
		public void None_Childs ()
		{
			SqlClientPermission perm = new SqlClientPermission (PermissionState.None);
			perm.Add ("data source=localhost;", String.Empty, KeyRestrictionBehavior.AllowOnly);
			perm.Add ("data source=127.0.0.1;", "password=;", KeyRestrictionBehavior.PreventUsage);

			Check ("None-Childs-1", perm, false, false, 2);
			perm.AllowBlankPassword = true;
			Check ("None-Childs-2", perm, true, false, 2);

			SqlClientPermission copy = (SqlClientPermission)perm.Copy ();
			Check ("Copy_None-Childs-1", copy, true, false, 2);
			copy.AllowBlankPassword = false;
			Check ("Copy_None-Childs-2", copy, false, false, 2);
		}

		[Test]
		public void Unrestricted ()
		{
			SqlClientPermission perm = new SqlClientPermission (PermissionState.Unrestricted);
			Check ("Unrestricted-1", perm, false, true, 0);
			perm.AllowBlankPassword = true;
			Check ("Unrestricted-2", perm, true, true, 0);

			SqlClientPermission copy = (SqlClientPermission)perm.Copy ();
			// note: Unrestricted is always created with default values (so AllowBlankPassword is false)
			Check ("Copy_Unrestricted-1", copy, false, true, 0);
			copy.AllowBlankPassword = true;
			Check ("Copy_Unrestricted-2", copy, true, true, 0);
		}

		[Test]
		public void Unrestricted_Add ()
		{
			SqlClientPermission perm = new SqlClientPermission (PermissionState.Unrestricted);
			Check ("Unrestricted-NoChild", perm, false, true, 0);
			perm.Add ("data source=localhost;", String.Empty, KeyRestrictionBehavior.AllowOnly);
			// note: Lost unrestricted state when children was added
			Check ("Unrestricted-WithChild", perm, false, false, 1);
		}
	}
}
