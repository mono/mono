//
// DataViewManagerTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2005 Novell Inc,
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

using NUnit.Framework;
using System;
using System.Data;
using System.ComponentModel;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataViewManagerTest 
	{
		[Test]
		public void Ctor ()
		{
			string defaultString = "<DataViewSettingCollectionString></DataViewSettingCollectionString>";
			string current = @"<DataViewSettingCollectionString><table2-1 Sort="""" RowFilter="""" RowStateFilter=""CurrentRows""/></DataViewSettingCollectionString>";
			string deleted = @"<DataViewSettingCollectionString><table2-1 Sort="""" RowFilter="""" RowStateFilter=""Deleted""/></DataViewSettingCollectionString>";

			DataViewManager m = new DataViewManager (null);
			Assert.IsNull (m.DataSet);
			Assert.AreEqual ("", m.DataViewSettingCollectionString);
			Assert.IsNotNull (m.DataViewSettings);
			DataSet ds = new DataSet ("ds");
			m.DataSet = ds;
			Assert.AreEqual (defaultString, m.DataViewSettingCollectionString,
				"default#1");

			DataSet ds2 = new DataSet ("ds2");
			Assert.AreEqual (defaultString, ds.DefaultViewManager.DataViewSettingCollectionString,
				"default#2");
			DataTable dt2_1 = new DataTable ("table2-1");
			dt2_1.Namespace ="urn:foo"; // It is ignored though.
			ds2.Tables.Add (dt2_1);
			m.DataSet = ds2;
			Assert.AreEqual (current, m.DataViewSettingCollectionString, "#3");

			// Note that " Deleted " is trimmed.
			m.DataViewSettingCollectionString = @"<DataViewSettingCollectionString><table2-1 Sort='' RowFilter='' RowStateFilter=' Deleted '/></DataViewSettingCollectionString>";
			Assert.AreEqual (deleted, m.DataViewSettingCollectionString, "#4");

			m.DataSet = ds2; //resets modified string.
			Assert.AreEqual (current, m.DataViewSettingCollectionString, "#5");

			m.DataViewSettingCollectionString = @"<DataViewSettingCollectionString><table2-1 Sort='' RowFilter='' RowStateFilter='Deleted'/></DataViewSettingCollectionString>";
			// it does not clear anything.
			m.DataViewSettingCollectionString = "<DataViewSettingCollectionString/>";
			Assert.AreEqual (deleted, m.DataViewSettingCollectionString, "#6");

			// text node is not rejected (ignored).
			// RowFilter is not examined.
			m.DataViewSettingCollectionString = "<DataViewSettingCollectionString>blah<table2-1 RowFilter='a=b' ApplyDefaultSort='true' /></DataViewSettingCollectionString>";
			// LAMESPEC: MS.NET ignores ApplyDefaultSort.
//			Assert.AreEqual (@"<DataViewSettingCollectionString><table2-1 Sort="""" RowFilter=""a=b"" RowStateFilter=""Deleted""/></DataViewSettingCollectionString>", m.DataViewSettingCollectionString, "#7");
		}

		[Test]
		[ExpectedException (typeof (DataException))]
		public void SetNullDataSet ()
		{
			DataViewManager m = new DataViewManager (null);
			m.DataSet = null; // DataException
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void SpecifyNonExistentTable ()
		{
			DataViewManager m = new DataViewManager (null);
			// NullReferenceException is thrown.
			m.DataViewSettingCollectionString = "<DataViewSettingCollectionString><table1-1 RowFilter='a=b' /></DataViewSettingCollectionString>";
		}

		[Test]
		[ExpectedException (typeof (DataException))]
		public void SetIncorrectRootElement ()
		{
			DataViewManager m = new DataViewManager (null);
			m.DataViewSettingCollectionString = "<foo/>";
		}
	}
}
