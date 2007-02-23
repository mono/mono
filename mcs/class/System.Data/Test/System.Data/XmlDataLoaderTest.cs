// Authors:
//   Nagappan A <anagappan@novell.com>
//
// Copyright (c) 2007 Novell, Inc
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
using System.Data;
using System.Collections;
using System.IO;	
using System.Xml;
using NUnit.Framework;

namespace Monotests_System.Data
{
	[TestFixture]	
	public class XmlDataLoaderTest
	{
		DataSet _ds;
		
		public XmlDataLoaderTest ()
		{
		}

		private DataSet Create ()
		{
			DataSet ds = new DataSet ("Set");
			DataTable dt = new DataTable ("Test");
			dt.Columns.Add ("CustName",  typeof (String));
			dt.Columns.Add ("Type",  typeof (System.Type));
			ds.Tables.Add (dt);
			return ds;
		}
				
		public void Write ()
		{
			_ds = Create ();
			DataTable dt = _ds.Tables [0];
			DataRow dr = dt.NewRow ();
			dr["CustName"] = System.DBNull.Value;
			dr["Type"] = typeof (System.DBNull);
			dt.Rows.Add (dr);
			_ds.WriteXml ("Test/System.Data/TestReadWriteXml1.xml", XmlWriteMode.DiffGram);
		}
		
		public void Read ()
		{
			_ds = Create ();
			_ds.ReadXml ("Test/System.Data/TestReadWriteXml1.xml", XmlReadMode.DiffGram);
		}
		[Test]
		public void XmlLoadTest ()
		{
			try {
				XmlDataLoaderTest t = new XmlDataLoaderTest ();
				t.Write ();
				t.Read ();
			} catch {
				Assert.Fail ("#1 Should not throw Exception");
			}
		}
	}
}
#endif
