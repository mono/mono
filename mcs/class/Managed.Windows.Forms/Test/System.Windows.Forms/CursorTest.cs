//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Miguel de Icaza
//

using System;
using System.Windows.Forms;
using System.Drawing;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class CursorTest : TestHelper
	{
		[Test]
		public void LoadCursorKind2 ()
		{
			//
			// This test tries to load a cursor with type 1
			// this contains an and mask, it used to crash
			//

			new Cursor (typeof (CursorTest).Assembly.GetManifestResourceStream ("a.cur"));
		}

#if NET_2_0
		[Test]
		public void CursorPropertyTag ()
		{
			Cursor md = Cursor.Current;
			object s = "MyString";

			Assert.AreEqual (null, md.Tag, "A1");

			md.Tag = s;
			Assert.AreSame (s, md.Tag, "A2");
		}
#endif
	}
}
