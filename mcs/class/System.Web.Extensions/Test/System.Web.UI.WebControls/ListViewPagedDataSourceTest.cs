//
// System.Web.UI.WebControls.ListView
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2009 Novell, Inc
//

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
#if NET_3_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class ListViewPagedDataSourceTest
	{
		string EnumerableToString (IEnumerable data)
		{
			var sb = new StringBuilder ();

			foreach (object d in data)
				sb.Append (d.ToString ());

			return sb.ToString ();
		}
		
		List<string> GetData (int count)
		{
			var ret = new List<string> ();

			for (int i = 0; i < count; i++)
				ret.Add (i.ToString ());

			return ret;
		}

		ListViewPagedDataSource GetDataSource (List<string> ds, int startRowIndex, int maximumRows, int totalRowCount, bool allowServerPaging)
		{
			var ret = new ListViewPagedDataSource ();

			ret.DataSource = ds;
			ret.StartRowIndex = startRowIndex;
			ret.MaximumRows = maximumRows;
			ret.TotalRowCount = totalRowCount;
			ret.AllowServerPaging = allowServerPaging;

			return ret;
		}

		[Test]
		public void Counts ()
		{
			List<string> l = GetData (10);
			ListViewPagedDataSource pds = GetDataSource (l, 0, 10, 25, false);

			Assert.AreEqual (10, pds.Count, "#A1-1");
			Assert.AreEqual (10, pds.DataSourceCount, "#A1-2");

			pds = GetDataSource (l, 0, 10, 25, true);
			Assert.AreEqual (10, pds.Count, "#A2-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#A2-2");

			pds = GetDataSource (l, 10, 10, 25, false);
			Assert.AreEqual (0, pds.Count, "#A3-1");
			Assert.AreEqual (10, pds.DataSourceCount, "#A3-2");

			pds = GetDataSource (l, 10, 10, 25, true);
			Assert.AreEqual (10, pds.Count, "#A4-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#A4-2");

			pds = GetDataSource (l, 15, 10, 25, false);
			Assert.AreEqual (-5, pds.Count, "#A5-1");
			Assert.AreEqual (10, pds.DataSourceCount, "#A5-2");

			pds = GetDataSource (l, 15, 10, 25, true);
			Assert.AreEqual (10, pds.Count, "#A6-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#A6-2");

			pds = GetDataSource (l, 20, 10, 25, false);
			Assert.AreEqual (-10, pds.Count, "#A7-1");
			Assert.AreEqual (10, pds.DataSourceCount, "#A7-2");

			pds = GetDataSource (l, 20, 10, 25, true);
			Assert.AreEqual (5, pds.Count, "#A8-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#A8-2");

			pds = GetDataSource (l, 25, 10, 25, false);
			Assert.AreEqual (-15, pds.Count, "#A9-1");
			Assert.AreEqual (10, pds.DataSourceCount, "#A9-2");

			pds = GetDataSource (l, 25, 10, 25, true);
			Assert.AreEqual (0, pds.Count, "#A10-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#A10-2");

			pds = GetDataSource (l, 30, 10, 25, false);
			Assert.AreEqual (-20, pds.Count, "#A11-1");
			Assert.AreEqual (10, pds.DataSourceCount, "#A11-2");

			pds = GetDataSource (l, 30, 10, 25, true);
			Assert.AreEqual (-5, pds.Count, "#A12-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#A12-2");

			l = GetData (11);
			pds = GetDataSource (l, 0, 11, 25, false);

			Assert.AreEqual (11, pds.Count, "#B1-1");
			Assert.AreEqual (11, pds.DataSourceCount, "#B1-2");

			pds = GetDataSource (l, 0, 11, 25, true);
			Assert.AreEqual (11, pds.Count, "#B2-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#B2-2");

			pds = GetDataSource (l, 10, 11, 25, false);
			Assert.AreEqual (1, pds.Count, "#B3-1");
			Assert.AreEqual (11, pds.DataSourceCount, "#B3-2");

			pds = GetDataSource (l, 10, 11, 25, true);
			Assert.AreEqual (11, pds.Count, "#B4-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#B4-2");

			pds = GetDataSource (l, 15, 11, 25, false);
			Assert.AreEqual (-4, pds.Count, "#B5-1");
			Assert.AreEqual (11, pds.DataSourceCount, "#B5-2");

			pds = GetDataSource (l, 15, 11, 25, true);
			Assert.AreEqual (10, pds.Count, "#B6-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#B6-2");

			pds = GetDataSource (l, 20, 11, 25, false);
			Assert.AreEqual (-9, pds.Count, "#B7-1");
			Assert.AreEqual (11, pds.DataSourceCount, "#B7-2");

			pds = GetDataSource (l, 20, 11, 25, true);
			Assert.AreEqual (5, pds.Count, "#B8-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#B8-2");

			pds = GetDataSource (l, 25, 11, 25, false);
			Assert.AreEqual (-14, pds.Count, "#B9-1");
			Assert.AreEqual (11, pds.DataSourceCount, "#B9-2");

			pds = GetDataSource (l, 25, 11, 25, true);
			Assert.AreEqual (0, pds.Count, "#B10-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#B10-2");

			pds = GetDataSource (l, 30, 11, 25, false);
			Assert.AreEqual (-19, pds.Count, "#B11-1");
			Assert.AreEqual (11, pds.DataSourceCount, "#B11-2");

			pds = GetDataSource (l, 30, 11, 25, true);
			Assert.AreEqual (-5, pds.Count, "#B12-1");
			Assert.AreEqual (25, pds.DataSourceCount, "#B12-2");
		}

		[Test]
		public void Enumerator ()
		{
			List<string> l = GetData (10);
			ListViewPagedDataSource pds = GetDataSource (l, 0, 10, 25, false);
			Assert.AreEqual ("0123456789", EnumerableToString (pds), "#A1");

			pds = GetDataSource (l, 5, 10, 25, false);
			Assert.AreEqual ("56789", EnumerableToString (pds), "#A2");

			pds = GetDataSource (l, 9, 10, 25, false);
			Assert.AreEqual ("9", EnumerableToString (pds), "#A3");

			pds = GetDataSource (l, 10, 10, 25, false);
			Assert.AreEqual (String.Empty, EnumerableToString (pds), "#A4");

			pds = GetDataSource (l, 20, 10, 25, false);
			Assert.AreEqual (String.Empty, EnumerableToString (pds), "#A5");

			pds = GetDataSource (l, 25, 10, 25, false);
			Assert.AreEqual (String.Empty, EnumerableToString (pds), "#A6");

			pds = GetDataSource (l, 30, 10, 25, false);
			Assert.AreEqual (String.Empty, EnumerableToString (pds), "#A7");

			pds = GetDataSource (l, 0, 10, 25, true);
			Assert.AreEqual ("0123456789", EnumerableToString (pds), "#B1");

			pds = GetDataSource (l, 5, 10, 25, true);
			Assert.AreEqual ("0123456789", EnumerableToString (pds), "#B2");

			pds = GetDataSource (l, 9, 10, 25, true);
			Assert.AreEqual ("0123456789", EnumerableToString (pds), "#B3");

			pds = GetDataSource (l, 10, 10, 25, true);
			Assert.AreEqual ("0123456789", EnumerableToString (pds), "#B4");

			pds = GetDataSource (l, 20, 10, 25, true);
			Assert.AreEqual ("01234", EnumerableToString (pds), "#B5");

			pds = GetDataSource (l, 25, 10, 25, true);
			Assert.AreEqual (String.Empty, EnumerableToString (pds), "#B6");

			pds = GetDataSource (l, 30, 10, 25, true);
			Assert.AreEqual (String.Empty, EnumerableToString (pds), "#B7");
		}
	}
}
#endif