//
// System.Configuration.ConfigurationLockCollectionTest.cs - Unit
// tests for System.Configuration.ConfigurationLockCollection.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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

using System;
using System.Configuration;
using System.Collections;
using NUnit.Framework;
using SysConfig = System.Configuration.Configuration;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class ConfigurationLockCollectionTest
	{

		[Test]
		public void InitialState ()
		{
			SysConfig cfg = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			ConfigurationLockCollection col;

			col = cfg.AppSettings.LockAttributes;
			Assert.AreEqual (0, col.Count, "A1");
			Assert.IsFalse (col.Contains ("file"), "A2");
			Assert.IsFalse (col.HasParentElements, "A4");
			Assert.IsFalse (col.IsModified, "A5");
			Assert.IsFalse (col.IsSynchronized, "A6");
			Assert.AreEqual (col, col.SyncRoot, "A7");

			col = cfg.AppSettings.LockElements;
			Assert.AreEqual (0, col.Count, "A8");
			Assert.IsFalse (col.HasParentElements, "A11");
			Assert.IsFalse (col.IsModified, "A12");
			Assert.IsFalse (col.IsSynchronized, "A13");
			Assert.AreEqual (col, col.SyncRoot, "A14");

			col = cfg.ConnectionStrings.LockAttributes;
			Assert.AreEqual (0, col.Count, "A8");
			Assert.IsFalse (col.HasParentElements, "A11");
			Assert.IsFalse (col.IsModified, "A12");
			Assert.IsFalse (col.IsSynchronized, "A13");
			Assert.AreEqual (col, col.SyncRoot, "A14");

			col = cfg.ConnectionStrings.LockElements;
			Assert.AreEqual (0, col.Count, "A8");
			Assert.IsFalse (col.HasParentElements, "A11");
			Assert.IsFalse (col.IsModified, "A12");
			Assert.IsFalse (col.IsSynchronized, "A13");
			Assert.AreEqual (col, col.SyncRoot, "A14");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void NonExistantItem ()
		{
			SysConfig cfg = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			ConfigurationLockCollection col;

			col = cfg.AppSettings.LockAttributes;

			Assert.IsFalse (col.IsReadOnly ("file"), "A3");
		}

		[Test]
		public void Populate ()
		{
			SysConfig cfg = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			ConfigurationLockCollection col = cfg.AppSettings.LockAttributes;

			col.Add ("file");

			Assert.AreEqual (1, col.Count, "A1");
			Assert.IsFalse (col.HasParentElements, "A2");
			Assert.IsTrue (col.IsModified, "A3");
			Assert.IsTrue (col.Contains ("file"), "A4");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void Populate_Error ()
		{
			SysConfig cfg = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			ConfigurationLockCollection col = cfg.AppSettings.LockAttributes;

			col.Add ("boo");
		}

		[Test]
		public void Enumerator ()
		{
			SysConfig cfg = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			ConfigurationLockCollection col = cfg.AppSettings.LockAttributes;

			col.Add ("file");

			IEnumerator e = col.GetEnumerator ();
			Assert.IsTrue (e.MoveNext (), "A1");
			Assert.AreEqual ("file", (string)e.Current, "A2");
			Assert.IsFalse (e.MoveNext (), "A3");
		}

		[Test]
		public void SetFromList ()
		{
			SysConfig cfg = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			ConfigurationLockCollection col = cfg.AppSettings.LockAttributes;

			col.SetFromList ("file");
			Assert.AreEqual (1, col.Count, "A1");
			Assert.IsTrue (col.Contains ("file"), "A2");

			col.Clear ();
			Assert.AreEqual (0, col.Count, "A5");

			col.SetFromList (" file ");
			Assert.AreEqual (1, col.Count, "A1");
			Assert.IsTrue (col.Contains ("file"), "A2");
		}

		[Test]
		public void DuplicateAdd ()
		{
			SysConfig cfg = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			AppSettingsSection app = cfg.AppSettings;

			app.LockAttributes.Clear ();

			app.LockAttributes.Add ("file");
			app.LockAttributes.Add ("file");

			Assert.AreEqual (1, app.LockAttributes.Count, "A1");
		}

		[Test]
		public void IsReadOnly ()
		{
			SysConfig cfg = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);
			AppSettingsSection app = cfg.AppSettings;

			app.LockAttributes.Clear ();
			app.LockAllAttributesExcept.Clear ();

			app.LockAttributes.Add ("file");
			Assert.IsFalse (app.LockAttributes.IsReadOnly ("file"), "A1");

			app.LockAllAttributesExcept.Add ("file");
			Assert.IsFalse (app.LockAllAttributesExcept.IsReadOnly ("file"), "A2");
		}
	}
}

#endif
