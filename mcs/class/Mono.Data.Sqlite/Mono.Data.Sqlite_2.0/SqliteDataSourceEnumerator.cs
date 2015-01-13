//
// Mono.Data.Sqlite.SqliteDataSourceEnumerator.cs
//
// Author(s):
//   Chris Toshok (toshok@ximian.com)
//   Marek Habersack (grendello@gmail.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2007 Marek Habersack
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


using System;
using System.Data;
using System.Data.Common;
        
namespace Mono.Data.Sqlite
{
        public class SqliteDataSourceEnumerator : DbDataSourceEnumerator
	{		
                public SqliteDataSourceEnumerator ()
                {
                }

                public override DataTable GetDataSources ()
                {
			DataTable dt = new DataTable ();
			DataColumn col;

			col = new DataColumn ("ServerName", typeof (string));
			dt.Columns.Add (col);
			
			col = new DataColumn ("InstanceName", typeof (string));
			dt.Columns.Add (col);

			col = new DataColumn ("IsClustered", typeof (bool));
			dt.Columns.Add (col);
			
			col = new DataColumn ("Version", typeof (string));
			dt.Columns.Add (col);
			
			col = new DataColumn ("FactoryName", typeof (string));
			dt.Columns.Add (col);

			DataRow dr = dt.NewRow ();
			dr [0] = "Sqlite Embedded Database";
			dr [1] = "Sqlite Default Instance";
			dr [2] = false;
			dr [3] = "?";
			dr [4] = "Mono.Data.Sqlite.SqliteConnectionFactory";
			dt.Rows.Add (dr);
			
			return dt;
                }                
        }
}

