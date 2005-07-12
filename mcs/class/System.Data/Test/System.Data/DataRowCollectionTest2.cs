// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Data;
using MonoTests.System.Data.Test.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class DataRowCollectionTest2
	{
		[Test] public void CopyTo()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow[] arr = new DataRow[dt.Rows.Count];
			dt.Rows.CopyTo(arr,0);
			Assert.AreEqual(dt.Rows.Count, arr.Length, "DRWC1");

			int index=0;
			foreach (DataRow dr in dt.Rows)
			{
				Assert.AreEqual(dr, arr[index], "DRWC2");
				index++;
			}
		}

		[Test] public void Count()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			Assert.AreEqual(6, dt.Rows.Count, "DRWC3");
			dt.Rows.Remove(dt.Rows[0]);
			Assert.AreEqual(5, dt.Rows.Count, "DRWC4");
			dt.Rows.Add(new object[] {1,"1-String1","1-String2",new DateTime(2005,1,1,0,0,0,0),1.534,true});
			Assert.AreEqual(6, dt.Rows.Count, "DRWC5");
		}

		[Test] public void GetEnumerator()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			IEnumerator myEnumerator = dt.Rows.GetEnumerator();
			int index=0;
			while (myEnumerator.MoveNext())
			{
				Assert.AreEqual(dt.Rows[index], (DataRow)myEnumerator.Current, "DRWC6");
				index++;
			}
			Assert.AreEqual(index, dt.Rows.Count, "DRWC7");
		}

		[Test] public void RemoveAt_ByIndex()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			int counter = dt.Rows.Count;
			dt.PrimaryKey=  new DataColumn[] {dt.Columns[0]};
			dt.Rows.RemoveAt(3);
			Assert.AreEqual(counter-1, dt.Rows.Count, "DRWC8");
			Assert.AreEqual(null, dt.Rows.Find(4), "DRWC9");
		}

		[Test] public void Remove_ByDataRow()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			int counter = dt.Rows.Count;
			dt.PrimaryKey=  new DataColumn[] {dt.Columns[0]};
			Assert.AreEqual(dt.Rows[0], dt.Rows.Find(1), "DRWC10");
			dt.Rows.Remove(dt.Rows[0]);
			Assert.AreEqual(counter-1, dt.Rows.Count, "DRWC11");
			Assert.AreEqual(null, dt.Rows.Find(1), "DRWC12");
		}
	}
}
