//
// OdbcTest.cs - Test for the ODBC ADO.NET Provider in System.Data.Odbc 
//
// The test works on Windows XP using Microsoft .NET Framework 1.1 Beta
//
// To compile under Windows using Microsoft .NET 1.1
// E:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\csc OdbcTest.cs /reference:System.Data.dll
//
// To compile under Windows using Mono:
// mcs OdbcTest.cs -r System.Data.dll
//
// I have not tested it on Linux using unixODBC
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//

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

using System;
using System.Data;
using System.Data.Odbc;

namespace Test.OdbcTest
{
	class OdbcTest
	{
		[STAThread]
		static void Main(string[] args)
		{
			OdbcConnection dbcon = new OdbcConnection();
			// connection string to a Microsoft SQL Server 2000 database
			// that does not use a DSN
			//dbcon.ConnectionString = 
			//	"DRIVER={SQL Server};" + 
			//	"SERVER=(local);" + 
			//	"Trusted_connection=true;" +
			//	"DATABASE=pubs;";

			// connection string that uses a DSN.
			dbcon.ConnectionString = 
				"DSN=LocalServer;UID=sa;PWD=";
				
			dbcon.Open();

			OdbcCommand dbcmd = new OdbcCommand();
			dbcmd.Connection = dbcon;
			dbcmd.CommandType = CommandType.Text;
			dbcmd.CommandText = "SELECT lname FROM employee";
			
			OdbcDataReader reader;
			reader = (OdbcDataReader) dbcmd.ExecuteReader();

			while(reader.Read()) {
				Console.WriteLine("Last Name: " + reader[0].ToString());
			}
			reader.Close();
			dbcmd.Dispose();
			dbcon.Close();
		}
	}
}
