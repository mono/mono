//
// Runtests.cs : A driver for running the tests for all or specific databases 
//
// Author:
//   Satya Sudha K (ksathyasudha@novell.com)
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
using System.Configuration;
using MonoTests.System.Data;

class RunTest {

	public static void Main (string [] args) 
	{
		string [] databases = null;
		if (args.Length == 0 || (args.Length == 1 && args [0].Equals ("all"))) {
			// Run test for all databases
			string listOfDbs = ConfigurationSettings.AppSettings ["Databases"];
			databases = listOfDbs.Split (';');
		} else {
			databases = (string []) args.Clone ();
		}
		
		BaseAdapter dbAdapter = null;

		foreach (string str in databases) {

			switch  (str) {
/*
			case "mysql" :
				Console.WriteLine ("\n ****** Running tests for MYSQL ***** \n");
				dbAdapter = new MySqlAdapter ("mysql");
				dbAdapter.RunTest ();
				break;
*/
			
			case "mssql" :
				Console.WriteLine ("\n ****** Running tests for MS SQL ***** \n");
				dbAdapter = new MsSqlAdapter ("mssql");
				dbAdapter.RunTest ();
				break;
			case "oracle" :
				Console.WriteLine ("\n ****** Running tests for ORACLE ***** \n");
				dbAdapter = new OraAdapter ("oracle");
				dbAdapter.RunTest ();
				break;
/*
			case "postgres" :
				Console.WriteLine ("\n ****** Running tests for POSTGRE ***** \n");
				dbAdapter = new PostgresAdapter ("postgres");
				dbAdapter.RunTest ();
				break;
*/
			}
		}
	}
}
