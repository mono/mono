// DataTableTest.cs - NUnit Test Cases for testing the DataTable 
//
// Authors:
//   Franklin Wise (gracenote@earthlink.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Hagit Yidov (hagity@mainsoft.com)
// 
// (C) Franklin Wise
// (C) 2003 Martin Willemoes Hansen
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin Inc. (http://www.xamarin.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml;
using System.Text;

using MonoTests.System.Data.Utils;
using MonoTests.Helpers;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataTableTest :  DataSetAssertion
	{
		[SetUp]
		public void Setup ()
		{
			MyDataTable.count = 0;
		}

		string EOL = Environment.NewLine;

		[Test]
		public void Ctor()
		{
			DataTable dt = new DataTable();

			Assert.IsFalse (dt.CaseSensitive, "CaseSensitive must be false.");
			Assert.IsNotNull (dt.Columns, "Col");
			//Assert.IsTrue (dt.ChildRelations != null);
			Assert.IsNotNull (dt.Constraints, "Const");
			Assert.IsNull (dt.DataSet, "ds");
			Assert.IsNotNull (dt.DefaultView, "dv");
			Assert.AreEqual (string.Empty, dt.DisplayExpression, "de");
			Assert.IsNotNull (dt.ExtendedProperties, "ep");
			Assert.IsFalse (dt.HasErrors, "he");
			Assert.IsNotNull (dt.Locale, "lc");
			Assert.AreEqual (50, dt.MinimumCapacity, "mc"); //LAMESPEC:
			Assert.AreEqual (string.Empty, dt.Namespace, "ns");
			//Assert.IsTrue (dt.ParentRelations != null);
			Assert.AreEqual (string.Empty, dt.Prefix, "pf");
			Assert.IsNotNull (dt.PrimaryKey, "pk");
			Assert.IsNotNull (dt.Rows, "rows");
			Assert.IsNull (dt.Site, "Site");
			Assert.AreEqual (string.Empty, dt.TableName, "tname");
		}

		[Test]
		public void Select ()
		{
			DataSet Set = new DataSet ();
			DataTable Mom = new DataTable ("Mom");
			DataTable Child = new DataTable ("Child");
			Set.Tables.Add (Mom);
			Set.Tables.Add (Child);
			
			DataColumn Col = new DataColumn ("Name");
			DataColumn Col2 = new DataColumn ("ChildName");
			Mom.Columns.Add (Col);
			Mom.Columns.Add (Col2);
			
			DataColumn Col3 = new DataColumn ("Name");
			DataColumn Col4 = new DataColumn ("Age");
			Col4.DataType = typeof (short);
			Child.Columns.Add (Col3);
			Child.Columns.Add (Col4);

			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);

			DataRow Row = Mom.NewRow ();
			Row [0] = "Laura";
			Row [1] = "Nick";
			Mom.Rows.Add (Row);

			Row = Mom.NewRow ();
			Row [0] = "Laura";
			Row [1] = "Dick";
			Mom.Rows.Add (Row);

			Row = Mom.NewRow ();
			Row [0] = "Laura";
			Row [1] = "Mick";
			Mom.Rows.Add (Row);

			Row = Mom.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Jack";
			Mom.Rows.Add (Row);

			Row = Mom.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Mack";
			Mom.Rows.Add (Row);

			Row = Mom.NewRow ();
			Row [0] = "'Jhon O'' Collenal'";
			Row [1] = "Pack";
			Mom.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Nick";
			Row [1] = 15;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Dick";
			Row [1] = 25;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Mick";
			Row [1] = 35;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Jack";
			Row [1] = 10;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Mack";
			Row [1] = 19;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Mack";
			Row [1] = 99;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Pack";
			Row [1] = 66;
			Child.Rows.Add (Row);

			DataRow [] Rows = Mom.Select ("Name = 'Teresa'");
			Assert.AreEqual (2, Rows.Length, "test#01");

			// test with apos escaped
			Rows = Mom.Select ("Name = '''Jhon O'''' Collenal'''");
			Assert.AreEqual (1, Rows.Length, "test#01.1");

			Rows = Mom.Select ("Name = 'Teresa' and ChildName = 'Nick'");
			Assert.AreEqual (0, Rows.Length, "test#02");

			Rows = Mom.Select ("Name = 'Teresa' and ChildName = 'Jack'");
			Assert.AreEqual (1, Rows.Length, "test#03");

			Rows = Mom.Select ("Name = 'Teresa' and ChildName <> 'Jack'");
			Assert.AreEqual ("Mack", Rows [0] [1], "test#04");

			Rows = Mom.Select ("Name = 'Teresa' or ChildName <> 'Jack'");
			Assert.AreEqual (6, Rows.Length, "test#05");

			Rows = Child.Select ("age = 20 - 1");
			Assert.AreEqual (1, Rows.Length, "test#06");

			Rows = Child.Select ("age <= 20");
			Assert.AreEqual (3, Rows.Length, "test#07");

			Rows = Child.Select ("age >= 20");
			Assert.AreEqual (4, Rows.Length, "test#08");

			Rows = Child.Select ("age >= 20 and name = 'Mack' or name = 'Nick'");
			Assert.AreEqual (2, Rows.Length, "test#09");

			Rows = Child.Select ("age >= 20 and (name = 'Mack' or name = 'Nick')");
			Assert.AreEqual (1, Rows.Length, "test#10");
			Assert.AreEqual ("Mack", Rows [0] [0], "test#11");

			Rows = Child.Select ("not (Name = 'Jack')");
			Assert.AreEqual (6, Rows.Length, "test#12");
		}

		[Test]
		public void Select2 ()
		{
			DataSet Set = new DataSet ();
			DataTable Child = new DataTable ("Child");

			Set.Tables.Add (Child);

			DataColumn Col3 = new DataColumn ("Name");
			DataColumn Col4 = new DataColumn ("Age");
			Col4.DataType = typeof (short);
			Child.Columns.Add (Col3);
			Child.Columns.Add (Col4);

			DataRow Row = Child.NewRow ();
			Row [0] = "Nick";
			Row [1] = 15;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Dick";
			Row [1] = 25;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Mick";
			Row [1] = 35;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Jack";
			Row [1] = 10;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Mack";
			Row [1] = 19;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Mack";
			Row [1] = 99;
			Child.Rows.Add (Row);

			DataRow [] Rows = Child.Select ("age >= 20", "age DESC");
			Assert.AreEqual (3, Rows.Length, "test#01");
			Assert.AreEqual ("Mack", Rows [0] [0], "test#02");
			Assert.AreEqual ("Mick", Rows [1] [0], "test#03");
			Assert.AreEqual ("Dick", Rows [2] [0], "test#04");

			Rows = Child.Select ("age >= 20", "age asc");
			Assert.AreEqual (3, Rows.Length, "test#05");
			Assert.AreEqual ("Dick", Rows [0] [0], "test#06");
			Assert.AreEqual ("Mick", Rows [1] [0], "test#07");
			Assert.AreEqual ("Mack", Rows [2] [0], "test#08");

			Rows = Child.Select ("age >= 20", "name asc");
			Assert.AreEqual (3, Rows.Length, "test#09");
			Assert.AreEqual ("Dick", Rows [0] [0], "test#10");
			Assert.AreEqual ("Mack", Rows [1] [0], "test#11");
			Assert.AreEqual ("Mick", Rows [2] [0], "test#12");

			Rows = Child.Select ("age >= 20", "name desc");
			Assert.AreEqual (3, Rows.Length, "test#09");
			Assert.AreEqual ("Mick", Rows [0] [0], "test#10");
			Assert.AreEqual ("Mack", Rows [1] [0], "test#11");
			Assert.AreEqual ("Dick", Rows [2] [0], "test#12");
		}

		[Test]
		public void SelectParsing ()
		{
			DataTable T = new DataTable ("test");
			DataColumn C = new DataColumn ("name");
			T.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T.Columns.Add (C);
			C = new DataColumn ("id");
			T.Columns.Add (C);
			
			DataSet Set = new DataSet ("TestSet");
			Set.Tables.Add (T);
			
			DataRow Row = null;
			for (int i = 0; i < 100; i++) {
				Row = T.NewRow ();
				Row [0] = "human" + i;
				Row [1] = i;
				Row [2] = i;
				T.Rows.Add (Row);
			}
			
			Row = T.NewRow ();
			Row [0] = "h*an";
			Row [1] = 1;
			Row [2] = 1;
			T.Rows.Add (Row);

			Assert.AreEqual (12, T.Select ("age<=10").Length, "test#01");
			
			Assert.AreEqual (12, T.Select ("age\n\t<\n\t=\t\n10").Length, "test#02");

			try {
				T.Select ("name = 1human ");
				Assert.Fail ("test#03");
			} catch (SyntaxErrorException e) {
				// missing operand after 'human' operand 
				Assert.AreEqual (typeof (SyntaxErrorException), e.GetType (), "test#04");
			}
			
			try {
				T.Select ("name = 1");
				Assert.Fail ("test#05");
			} catch (EvaluateException e) {
				// Cannot perform '=' operation between string and Int32
				Assert.AreEqual (typeof (EvaluateException), e.GetType (), "test#06");
			}
			
			Assert.AreEqual (1, T.Select ("age = '13'").Length, "test#07");
		}

		[Test]
		public void SelectEscaping ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("SomeCol");
			dt.Rows.Add (new object [] {"\t"});
			dt.Rows.Add (new object [] {"\\"});
			
			Assert.AreEqual (0, dt.Select (@"SomeCol='\t'").Length, "test#01");
			Assert.AreEqual (0, dt.Select (@"SomeCol='\\'").Length, "test#02");
			
			Assert.AreEqual (0, dt.Select (@"SomeCol='\x'").Length, "test#03");
		}

		[Test]
		public void SelectOperators ()
		{
			DataTable T = new DataTable ("test");
			DataColumn C = new DataColumn ("name");
			T.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T.Columns.Add (C);
			C = new DataColumn ("id");
			T.Columns.Add (C);
			
			DataSet Set = new DataSet ("TestSet");
			Set.Tables.Add (T);
			
			DataRow Row = null;
			for (int i = 0; i < 100; i++) {
				Row = T.NewRow ();
				Row [0] = "human" + i;
				Row [1] = i;
				Row [2] = i;
				T.Rows.Add (Row);
			}
			
			Row = T.NewRow ();
			Row [0] = "h*an";
			Row [1] = 1;
			Row [2] = 1;
			T.Rows.Add (Row);
			
			Assert.AreEqual (11, T.Select ("age < 10").Length, "test#01");
			Assert.AreEqual (12, T.Select ("age <= 10").Length, "test#02");
			Assert.AreEqual (12, T.Select ("age< =10").Length, "test#03");
			Assert.AreEqual (89, T.Select ("age > 10").Length, "test#04");
			Assert.AreEqual (90, T.Select ("age >= 10").Length, "test#05");
			Assert.AreEqual (100, T.Select ("age <> 10").Length, "test#06");
			Assert.AreEqual (3, T.Select ("name < 'human10'").Length, "test#07");
			Assert.AreEqual (3, T.Select ("id < '10'").Length, "test#08");
			// FIXME: Somebody explain how this can be possible.
			// it seems that it is no matter between 10 - 30. The
			// result is allways 25 :-P
			//Assert.AreEqual (25, T.Select ("id < 10").Length, "test#09");
			
		}

		[Test]
		public void SelectExceptions ()
		{
			DataTable T = new DataTable ("test");
			DataColumn C = new DataColumn ("name");
			T.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T.Columns.Add (C);
			C = new DataColumn ("id");
			T.Columns.Add (C);
			
			for (int i = 0; i < 100; i++) {
				DataRow Row = T.NewRow ();
				Row [0] = "human" + i;
				Row [1] = i;
				Row [2] = i;
				T.Rows.Add (Row);
			}
			
			try {
				T.Select ("name = human1");
				Assert.Fail ("test#01");
			} catch (EvaluateException e) {
				// column name human not found
				Assert.AreEqual (typeof (EvaluateException), e.GetType (), "test#02");
			}
			
			Assert.AreEqual (1, T.Select ("id = '12'").Length, "test#04");
			Assert.AreEqual (1, T.Select ("id = 12").Length, "test#05");
			
			try {
				T.Select ("id = 1k3");
				Assert.Fail ("test#06");
			} catch (SyntaxErrorException e) {
				// no operands after k3 operator
				Assert.AreEqual (typeof (SyntaxErrorException), e.GetType (), "test#07");
			}
		}
		
		[Test]
		public void SelectStringOperators ()
		{
 			DataTable T = new DataTable ("test");
			DataColumn C = new DataColumn ("name");
			T.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T.Columns.Add (C);
			C = new DataColumn ("id");
			T.Columns.Add (C);
			
			DataSet Set = new DataSet ("TestSet");
			Set.Tables.Add (T);
			
			DataRow Row = null;
			for (int i = 0; i < 100; i++) {
				Row = T.NewRow ();
				Row [0] = "human" + i;
				Row [1] = i;
				Row [2] = i;
				T.Rows.Add (Row);
			}
			Row = T.NewRow ();
			Row [0] = "h*an";
			Row [1] = 1;
			Row [2] = 1;
			T.Rows.Add (Row);

			Assert.AreEqual (1, T.Select ("name = 'human' + 1").Length, "test#01");
			
			Assert.AreEqual ("human1", T.Select ("name = 'human' + 1") [0] ["name"], "test#02");
			Assert.AreEqual (1, T.Select ("name = 'human' + '1'").Length, "test#03");
			Assert.AreEqual ("human1", T.Select ("name = 'human' + '1'") [0] ["name"], "test#04");
			Assert.AreEqual (1, T.Select ("name = 'human' + 1 + 2").Length, "test#05");
			Assert.AreEqual ("human12", T.Select ("name = 'human' + '1' + '2'") [0] ["name"], "test#06");
			
			Assert.AreEqual (1, T.Select ("name = 'huMAn' + 1").Length, "test#07");
			
			Set.CaseSensitive = true;
			Assert.AreEqual (0, T.Select ("name = 'huMAn' + 1").Length, "test#08");
			
			T.CaseSensitive = false;
			Assert.AreEqual (1, T.Select ("name = 'huMAn' + 1").Length, "test#09");
			
			T.CaseSensitive = true;
			Assert.AreEqual (0, T.Select ("name = 'huMAn' + 1").Length, "test#10");
			
			Set.CaseSensitive = false;
			Assert.AreEqual (0, T.Select ("name = 'huMAn' + 1").Length, "test#11");
			
			T.CaseSensitive = false;
			Assert.AreEqual (1, T.Select ("name = 'huMAn' + 1").Length, "test#12");
			
			Assert.AreEqual (0, T.Select ("name = 'human1*'").Length, "test#13");
			Assert.AreEqual (11, T.Select ("name like 'human1*'").Length, "test#14");
			Assert.AreEqual (11, T.Select ("name like 'human1%'").Length, "test#15");
			
			try {
				Assert.AreEqual (11, T.Select ("name like 'h*an1'").Length, "test#16");
				Assert.Fail ("test#16");
			} catch (EvaluateException e) {
				// 'h*an1' is invalid
				Assert.AreEqual (typeof (EvaluateException), e.GetType (), "test#17");
			}
			
			try {
				Assert.AreEqual (11, T.Select ("name like 'h%an1'").Length, "test#18");
				Assert.Fail ("test#19");
			} catch (EvaluateException e) {
				// 'h%an1' is invalid
				Assert.AreEqual (typeof (EvaluateException), e.GetType (), "test#20");
			}
			
			Assert.AreEqual (0, T.Select ("name like 'h[%]an'").Length, "test#21");
			Assert.AreEqual (1, T.Select ("name like 'h[*]an'").Length, "test#22");
		}

		[Test]
		public void SelectAggregates ()
		{
			DataTable T = new DataTable ("test");
			DataColumn C = new DataColumn ("name");
			T.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T.Columns.Add (C);
			C = new DataColumn ("id");
			T.Columns.Add (C);
			DataRow Row = null;
			
			for (int i = 0; i < 1000; i++) {
				Row = T.NewRow ();
				Row [0] = "human" + i;
				Row [1] = i;
				Row [2] = i;
				T.Rows.Add (Row);
			}
			
			Assert.AreEqual (1000, T.Select ("Sum(age) > 10").Length, "test#01");
			Assert.AreEqual (1000, T.Select ("avg(age) = 499").Length, "test#02");
			Assert.AreEqual (1000, T.Select ("min(age) = 0").Length, "test#03");
			Assert.AreEqual (1000, T.Select ("max(age) = 999").Length, "test#04");
			Assert.AreEqual (1000, T.Select ("count(age) = 1000").Length, "test#05");
			Assert.AreEqual (1000, T.Select ("stdev(age) > 287 and stdev(age) < 289").Length, "test#06");
			Assert.AreEqual (1000, T.Select ("var(age) < 83417 and var(age) > 83416").Length, "test#07");
		}
		
		[Test]
		public void SelectFunctions ()
		{
			DataTable T = new DataTable ("test");
			DataColumn C = new DataColumn ("name");
			T.Columns.Add (C);
			C = new DataColumn ("age");
			C.DataType = typeof (int);
			T.Columns.Add (C);
			C = new DataColumn ("id");
			T.Columns.Add (C);
			DataRow Row = null;
			
			for (int i = 0; i < 1000; i++) {
				Row = T.NewRow ();
				Row [0] = "human" + i;
				Row [1] = i;
				Row [2] = i;
				T.Rows.Add (Row);
			}
			
			Row = T.NewRow ();
			Row [0] = "human" + "test";
			Row [1] = DBNull.Value;
			Row [2] = DBNull.Value;
			T.Rows.Add (Row);

			//TODO: How to test Convert-function
			Assert.AreEqual (25, T.Select ("age = 5*5") [0]["age"], "test#01");
			Assert.AreEqual (901, T.Select ("len(name) > 7").Length, "test#02");
			Assert.AreEqual (125, T.Select ("age = 5*5*5 AND len(name)>7") [0]["age"], "test#03");
			Assert.AreEqual (1, T.Select ("isnull(id, 'test') = 'test'").Length, "test#04");
			Assert.AreEqual (1000, T.Select ("iif(id = '56', 'test', 'false') = 'false'").Length, "test#05");
			Assert.AreEqual (1, T.Select ("iif(id = '56', 'test', 'false') = 'test'").Length, "test#06");
			Assert.AreEqual (9, T.Select ("substring(id, 2, 3) = '23'").Length, "test#07");
			Assert.AreEqual ("123", T.Select ("substring(id, 2, 3) = '23'") [0] ["id"], "test#08");
			Assert.AreEqual ("423", T.Select ("substring(id, 2, 3) = '23'") [3] ["id"], "test#09");
			Assert.AreEqual ("923", T.Select ("substring(id, 2, 3) = '23'") [8] ["id"], "test#10");
		}

		[Test]
		public void SelectRelations ()
		{
			DataSet Set = new DataSet ();
			DataTable Mom = new DataTable ("Mom");
			DataTable Child = new DataTable ("Child");

			Set.Tables.Add (Mom);
			Set.Tables.Add (Child);

			DataColumn Col = new DataColumn ("Name");
			DataColumn Col2 = new DataColumn ("ChildName");
			Mom.Columns.Add (Col);
			Mom.Columns.Add (Col2);

			DataColumn Col3 = new DataColumn ("Name");
			DataColumn Col4 = new DataColumn ("Age");
			Col4.DataType = typeof (short);
			Child.Columns.Add (Col3);
			Child.Columns.Add (Col4);

			DataRelation Relation = new DataRelation ("Rel", Mom.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);

			DataRow Row = Mom.NewRow ();
			Row [0] = "Laura";
			Row [1] = "Nick";
			Mom.Rows.Add (Row);

			Row = Mom.NewRow ();
			Row [0] = "Laura";
			Row [1] = "Dick";
			Mom.Rows.Add (Row);

			Row = Mom.NewRow ();
			Row [0] = "Laura";
			Row [1] = "Mick";
			Mom.Rows.Add (Row);

			Row = Mom.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Jack";
			Mom.Rows.Add (Row);

			Row = Mom.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Mack";
			Mom.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Nick";
			Row [1] = 15;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Dick";
			Row [1] = 25;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Mick";
			Row [1] = 35;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Jack";
			Row [1] = 10;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Mack";
			Row [1] = 19;
			Child.Rows.Add (Row);

			Row = Child.NewRow ();
			Row [0] = "Mack";
			Row [1] = 99;
			Child.Rows.Add (Row);
			
			DataRow [] Rows = Child.Select ("name = Parent.Childname");
			Assert.AreEqual (6, Rows.Length, "test#01");
			Rows = Child.Select ("Parent.childname = 'Jack'");
			Assert.AreEqual (1, Rows.Length, "test#02");
			
			/*
			try {
				// FIXME: LAMESPEC: Why the exception is thrown why... why... 
				Mom.Select ("Child.Name = 'Jack'");
				Assert.Fail ("test#03");
			} catch (Exception e) {
				Assert.AreEqual (typeof (SyntaxErrorException), e.GetType (), "test#04");
				Assert.AreEqual ("Cannot interpret token 'Child' at position 1.", e.Message, "test#05");
			}
			*/
			
			Rows = Child.Select ("Parent.name = 'Laura'");
			Assert.AreEqual (3, Rows.Length, "test#06");
			
			DataTable Parent2 = new DataTable ("Parent2");
			Col = new DataColumn ("Name");
			Col2 = new DataColumn ("ChildName");

			Parent2.Columns.Add (Col);
			Parent2.Columns.Add (Col2);
			Set.Tables.Add (Parent2);

			Row = Parent2.NewRow ();
			Row [0] = "Laura";
			Row [1] = "Nick";
			Parent2.Rows.Add (Row);

			Row = Parent2.NewRow ();
			Row [0] = "Laura";
			Row [1] = "Dick";
			Parent2.Rows.Add (Row);

			Row = Parent2.NewRow ();
			Row [0] = "Laura";
			Row [1] = "Mick";
			Parent2.Rows.Add (Row);

			Row = Parent2.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Jack";
			Parent2.Rows.Add (Row);

			Row = Parent2.NewRow ();
			Row [0] = "Teresa";
			Row [1] = "Mack";
			Parent2.Rows.Add (Row);

			Relation = new DataRelation ("Rel2", Parent2.Columns [1], Child.Columns [0]);
			Set.Relations.Add (Relation);

			try {
				Rows = Child.Select ("Parent.ChildName = 'Jack'");
				Assert.Fail ("test#07");
			} catch (EvaluateException e) {
				Assert.AreEqual (typeof (EvaluateException), e.GetType (), "test#08");
				// Do not compare exception messages!
				//Assert.AreEqual ("The table [Child] involved in more than one relation. You must explicitly mention a relation name in the expression 'parent.[ChildName]'.", e.Message, "test#09");
			}
			
			Rows = Child.Select ("Parent(rel).ChildName = 'Jack'");
			Assert.AreEqual (1, Rows.Length, "test#10");

			Rows = Child.Select ("Parent(Rel2).ChildName = 'Jack'");
			Assert.AreEqual (1, Rows.Length, "test#10");
			
			try {
				Mom.Select ("Parent.name  = 'John'");
			} catch (IndexOutOfRangeException e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (), "test#11");
				// Do not compare exception messages!
				//Assert.AreEqual ("Cannot find relation 0.", e.Message, "test#12");
			}
		}

		[Test]
		public void SelectRowState()
		{
			DataTable d = new DataTable();
			d.Columns.Add (new DataColumn ("aaa"));
			DataRow [] rows = d.Select (null, null, DataViewRowState.Deleted);
			Assert.AreEqual (0, rows.Length);
			d.Rows.Add (new object [] {"bbb"});
			d.Rows.Add (new object [] {"bbb"});
			rows = d.Select (null, null, DataViewRowState.Deleted);
			Assert.AreEqual (0, rows.Length);
		}

		[Test]
		public void ToStringTest()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("Col1", typeof(int));

			dt.TableName = "Mytable";
			dt.DisplayExpression = "Col1";

			string cmpr = dt.TableName + " + " + dt.DisplayExpression;
			Assert.AreEqual (cmpr, dt.ToString());
		}

		[Test]
		public void PrimaryKey ()
		{
			DataTable dt = new DataTable ();
			DataColumn Col = new DataColumn ();
			Col.AllowDBNull = false;
			Col.DataType = typeof (int);
			dt.Columns.Add (Col);
			dt.Columns.Add ();
			dt.Columns.Add ();
			dt.Columns.Add ();

			Assert.AreEqual (0, dt.PrimaryKey.Length, "test#01");

			dt.PrimaryKey = new DataColumn [] {dt.Columns [0]};
			Assert.AreEqual (1, dt.PrimaryKey.Length, "test#02");
			Assert.AreEqual ("Column1", dt.PrimaryKey [0].ColumnName, "test#03");

			dt.PrimaryKey = null;
			Assert.AreEqual (0, dt.PrimaryKey.Length, "test#04");

			Col = new DataColumn ("failed");

			try {
				dt.PrimaryKey = new DataColumn [] {Col};
				Assert.Fail ("test#05");
			} catch (ArgumentException e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#06");
				// Never expect English message
				// Assert.AreEqual ("Column must belong to a table.", e.Message, "test#07");
			}

			DataTable dt2 = new DataTable ();
			dt2.Columns.Add ();

			try {
				dt.PrimaryKey = new DataColumn [] {dt2.Columns [0]};
				Assert.Fail ("test#08");
			} catch (ArgumentException e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "test#09");
				// Never expect English message
				// Assert.AreEqual ("PrimaryKey columns do not belong to this table.", e.Message, "test#10");
			}

			Assert.AreEqual (0, dt.Constraints.Count, "test#11");

			dt.PrimaryKey = new DataColumn [] {dt.Columns [0], dt.Columns [1]};
			Assert.AreEqual (2, dt.PrimaryKey.Length, "test#12");
			Assert.AreEqual (1, dt.Constraints.Count, "test#13");
			Assert.IsTrue (dt.Constraints [0] is UniqueConstraint, "test#14");
			Assert.AreEqual ("Column1", dt.PrimaryKey [0].ColumnName, "test#15");
			Assert.AreEqual ("Column2", dt.PrimaryKey [1].ColumnName, "test#16");
		}
		
		[Test]
		[SetCulture("en-US")]
		public void PropertyExceptions ()
		{
			DataSet set = new DataSet ();
			DataTable table = new DataTable ();
			DataTable table1 =  new DataTable ();
			set.Tables.Add (table);
			set.Tables.Add (table1);

			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = typeof (int);
			table.Columns.Add (col);
			UniqueConstraint uc = new UniqueConstraint ("UK1", table.Columns[0] );
			table.Constraints.Add (uc);
			table.CaseSensitive = false;

			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = typeof (string);
			table.Columns.Add (col);

			col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = typeof (int);
			table1.Columns.Add (col);
			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = typeof (string);
			table1.Columns.Add (col);

			DataRelation dr = new DataRelation ("DR", table.Columns[0], table1.Columns[0]);
			set.Relations.Add (dr);

			try {
				table.CaseSensitive = true;
				table1.CaseSensitive = true;
				Assert.Fail ("#A01");
			} catch (ArgumentException) {
			}

			try {
				CultureInfo cultureInfo = new CultureInfo ("en-gb");
				table.Locale = cultureInfo;
				table1.Locale = cultureInfo;
				Assert.Fail ("#A03");
			} catch (ArgumentException) {
			}

			try {
				table.Prefix = "Prefix#1";
				Assert.Fail ("#A05");
			} catch (DataException) {
			}
		}

		[Test]
		public void GetErrors ()
		{
			DataTable table = new DataTable ();

			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = typeof (int);
			table.Columns.Add (col);

			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = typeof (string);
			table.Columns.Add (col);
			
			DataRow row = table.NewRow ();
			row ["Id"] = 147;
			row ["name"] = "Abc";
			row.RowError = "Error#1";
			table.Rows.Add (row);

			Assert.AreEqual (1, table.GetErrors ().Length, "#A01");
			Assert.AreEqual ("Error#1", (table.GetErrors ())[0].RowError, "#A02");
		}

		[Test]
		public void NewRowAddedTest ()
		{
			DataTable table = new DataTable ();

			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = typeof (int);
			table.Columns.Add (col);

			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = typeof (string);
			table.Columns.Add (col);
			
			_tableNewRowAddedEventFired = false;
			table.TableNewRow += new DataTableNewRowEventHandler (OnTableNewRowAdded);
			DataRow row = table.NewRow ();
			row ["Id"] = 147;
			row ["name"] = "Abc";
			table.Rows.Add (row);

			Assert.IsTrue (_tableNewRowAddedEventFired, "#NewRowAdded Event #01");
		}

		[Test]
		public void CloneCopyTest ()
		{
			DataTable table = new DataTable ();
			table.TableName = "Table#1";
			DataTable table1 = new DataTable ();
			table1.TableName = "Table#2";

			table.AcceptChanges ();

			DataSet set = new DataSet ("Data Set#1");
			set.DataSetName = "Dataset#1";
			set.Tables.Add (table);
			set.Tables.Add (table1);

			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = typeof (int);
			table.Columns.Add (col);
			UniqueConstraint uc = new UniqueConstraint ("UK1", table.Columns[0] );
			table.Constraints.Add (uc);

			col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = typeof (int);
			table1.Columns.Add (col);

			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = typeof (string);
			table.Columns.Add (col);
			
			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = typeof (string);
			table1.Columns.Add (col);
			DataRow row = table.NewRow ();
			row ["Id"] = 147;
			row ["name"] = "Abc";
			row.RowError = "Error#1";
			table.Rows.Add (row);

			row = table.NewRow ();
			row ["Id"] = 47;
			row ["name"] = "Efg";
			table.Rows.Add (row);
			table.AcceptChanges ();

			table.CaseSensitive = true;
			table1.CaseSensitive = true;
			table.MinimumCapacity = 100;
			table.Prefix = "PrefixNo:1";
			table.Namespace = "Namespace#1";
			table.DisplayExpression = "Id / Name + (Id * Id)";
			DataColumn[] colArray = {table.Columns[0]};
			table.PrimaryKey = colArray;
			table.ExtendedProperties.Add ("TimeStamp", DateTime.Now);

			row = table1.NewRow ();
			row ["Name"] = "Abc";
			row ["Id"] = 147;
			table1.Rows.Add (row);

			row = table1.NewRow ();
			row ["Id"] = 47;
			row ["Name"] = "Efg";
			table1.Rows.Add (row);

			DataRelation dr = new DataRelation ("DR", table.Columns[0], table1.Columns[0]);
			set.Relations.Add (dr);

			//Testing properties of clone
			DataTable cloneTable = table.Clone ();
			Assert.IsTrue (cloneTable.CaseSensitive, "#A01");
			Assert.AreEqual (0 , cloneTable.ChildRelations.Count, "#A02");
			Assert.AreEqual (0 , cloneTable.ParentRelations.Count, "#A03");
			Assert.AreEqual (2,  cloneTable.Columns.Count, "#A04");
			Assert.AreEqual (1, cloneTable.Constraints.Count, "#A05");
			Assert.AreEqual ("Id / Name + (Id * Id)", cloneTable.DisplayExpression, "#A06");
			Assert.AreEqual (1, cloneTable.ExtendedProperties.Count, "#A07");
			Assert.IsFalse (cloneTable.HasErrors, "#A08");
			Assert.AreEqual (100, cloneTable.MinimumCapacity, "#A10");
			Assert.AreEqual ("Namespace#1", cloneTable.Namespace, "#A11");
			Assert.AreEqual ("PrefixNo:1", cloneTable.Prefix, "#A12");
			Assert.AreEqual ("Id",  cloneTable.PrimaryKey[0].ColumnName, "#A13");
			Assert.AreEqual (0, cloneTable.Rows.Count , "#A14");
			Assert.AreEqual ("Table#1", cloneTable.TableName, "#A15");

			//Testing properties of copy
			DataTable copyTable = table.Copy ();
			Assert.IsTrue (copyTable.CaseSensitive, "#A16");
			Assert.AreEqual (0 , copyTable.ChildRelations.Count, "#A17");
			Assert.AreEqual (0 , copyTable.ParentRelations.Count, "#A18");
			Assert.AreEqual (2,  copyTable.Columns.Count, "#A19");
			Assert.AreEqual (1, copyTable.Constraints.Count, "#A20");
			Assert.AreEqual ("Id / Name + (Id * Id)", copyTable.DisplayExpression, "#A21");
			Assert.AreEqual (1, copyTable.ExtendedProperties.Count, "#A22");
			Assert.IsTrue (copyTable.HasErrors, "#A23");
			Assert.AreEqual (100, copyTable.MinimumCapacity, "#A25");
			Assert.AreEqual ("Namespace#1", copyTable.Namespace, "#A26");
			Assert.AreEqual ("PrefixNo:1", copyTable.Prefix, "#A27");
			Assert.AreEqual ("Id",  copyTable.PrimaryKey[0].ColumnName, "#A28");
			Assert.AreEqual (2, copyTable.Rows.Count, "#A29");
			Assert.AreEqual ("Table#1", copyTable.TableName, "#A30");
		}

		[Test]
		public void CloneExtendedProperties ()
		{
			// bug 668
			DataTable t1 = new DataTable ("t1");
			DataColumn c1 = t1.Columns.Add ("c1");
			c1.ExtendedProperties.Add ("Company", "Xamarin");
			
			DataTable t2 = t1.Clone ();
			Assert.AreEqual ("Xamarin", t1.Columns["c1"].ExtendedProperties["Company"], "CEP1");
			Assert.AreEqual ("Xamarin", t2.Columns["c1"].ExtendedProperties["Company"], "CEP2");
		}
	
		[Test]
		[ExpectedException (typeof (EvaluateException))]
		public void CloneExtendedProperties1 ()
		{
			// Xamarin bug 666
			DataTable table1 = new DataTable("Table1") ;

			DataColumn c1 = table1.Columns.Add("c1", typeof(string), "'hello ' + c2") ; /* Should cause an exception */
		}

		[Test]
		public void CloneExtendedProperties2 ()
		{
			// Xamarin bug 666
			DataTable table1 = new 	DataTable("Table1") ;

			DataColumn c1 = table1.Columns.Add("c1") ;
			DataColumn c2 = table1.Columns.Add("c2") ;

			c1.Expression = "'hello ' + c2";

			DataTable t2 = table1.Clone(); // this should not cause an exception
		}

		[Test]
		public void LoadDataException ()
		{
			DataTable table = new DataTable ();
			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = typeof (int);
			col.DefaultValue = 47;
			table.Columns.Add (col);
			UniqueConstraint uc = new UniqueConstraint ("UK1", table.Columns[0] );
			table.Constraints.Add (uc);

			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = typeof (string);
			col.DefaultValue = "Hello";
			table.Columns.Add (col);

			table.BeginLoadData();
			object[] row = {147, "Abc"};
			DataRow newRow = table.LoadDataRow (row, true);

			object[] row1 = {147, "Efg"};
			DataRow newRow1 = table.LoadDataRow (row1, true);

			object[] row2 = {143, "Hij"};
			DataRow newRow2 = table.LoadDataRow (row2, true);

			try {
				table.EndLoadData ();
				Assert.Fail ("#A01");
			} catch (ConstraintException) {
			}
		}

		[Test]
		public void Changes () //To test GetChanges and RejectChanges
		{
			DataTable table = new DataTable ();

			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = typeof (int);
			table.Columns.Add (col);
			UniqueConstraint uc = new UniqueConstraint ("UK1", table.Columns[0] );
			table.Constraints.Add (uc);

			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = typeof (string);
			table.Columns.Add (col);

			DataRow row = table.NewRow ();
			row ["Id"] = 147;
			row ["name"] = "Abc";
			table.Rows.Add (row);
			table.AcceptChanges ();

			row = table.NewRow ();
			row ["Id"] = 47;
			row ["name"] = "Efg";
			table.Rows.Add (row);

			//Testing GetChanges
			DataTable changesTable = table.GetChanges ();
			Assert.AreEqual (1, changesTable.Rows.Count, "#A01");
 			Assert.AreEqual ("Efg", changesTable.Rows[0]["Name"], "#A02");
			table.AcceptChanges ();
			changesTable = table.GetChanges ();

			try {
				int cnt = changesTable.Rows.Count;
				Assert.Fail ();
			} catch (NullReferenceException) {
			}
			
			//Testing RejectChanges
			row = table.NewRow ();
			row ["Id"] = 247;
			row ["name"] = "Hij";
			table.Rows.Add (row);

			(table.Rows [0])["Name"] = "AaBbCc";
			table.RejectChanges ();
			Assert.AreEqual ("Abc" , (table.Rows [0]) ["Name"], "#A03");
			Assert.AreEqual (2, table.Rows.Count, "#A04");
		}

		[Test]
		public void ImportRowTest ()
		{
			// build source table
			DataTable src = new DataTable ();
			src.Columns.Add ("id", typeof (int));
			src.Columns.Add ("name", typeof (string));

			src.PrimaryKey = new DataColumn [] {src.Columns [0]} ;

			src.Rows.Add (new object [] { 1, "mono 1" });
			src.Rows.Add (new object [] { 2, "mono 2" });
			src.Rows.Add (new object [] { 3, "mono 3" });
			src.AcceptChanges ();

			src.Rows [0] [1] = "mono changed 1";  // modify 1st row
			src.Rows [1].Delete ();              // delete 2nd row
			// 3rd row is unchanged
			src.Rows.Add (new object [] { 4, "mono 4" }); // add 4th row

			// build target table
			DataTable target = new DataTable ();
			target.Columns.Add ("id", typeof (int));
			target.Columns.Add ("name", typeof (string));

			target.PrimaryKey = new DataColumn [] {target.Columns [0]} ;

			// import all rows
			target.ImportRow (src.Rows [0]);     // import 1st row
			target.ImportRow (src.Rows [1]);     // import 2nd row
			target.ImportRow (src.Rows [2]);     // import 3rd row
			target.ImportRow (src.Rows [3]);     // import 4th row

			try {
				target.ImportRow (src.Rows [2]); // import 3rd row again
				Assert.Fail ("#C1");
			} catch (ConstraintException ex) {
				// Column 'id' is constrained to be unique.
				// Value '3' is already present
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf ("'id'") != -1, "#C5");
				Assert.IsTrue (ex.Message.IndexOf ("'3'") != -1, "#C6");
			}

			// check row states
			Assert.AreEqual (src.Rows [0].RowState, target.Rows [0].RowState, "#A1");
			Assert.AreEqual (src.Rows [1].RowState, target.Rows [1].RowState, "#A2");
			Assert.AreEqual (src.Rows [2].RowState, target.Rows [2].RowState, "#A3");
			Assert.AreEqual (src.Rows [3].RowState, target.Rows [3].RowState, "#A4");

			// check for modified row (1st row)
			Assert.AreEqual ((string) src.Rows [0] [1], (string) target.Rows [0] [1], "#B1");
			Assert.AreEqual ((string) src.Rows [0] [1, DataRowVersion.Default], (string) target.Rows [0] [1, DataRowVersion.Default], "#B2");
			Assert.AreEqual ((string) src.Rows [0] [1, DataRowVersion.Original], (string) target.Rows [0] [1, DataRowVersion.Original], "#B3");
			Assert.AreEqual ((string) src.Rows [0] [1, DataRowVersion.Current], (string) target.Rows [0] [1, DataRowVersion.Current], "#B4");
			Assert.IsFalse (target.Rows [0].HasVersion(DataRowVersion.Proposed), "#B5");

			// check for deleted row (2nd row)
			Assert.AreEqual ((string) src.Rows [1] [1, DataRowVersion.Original], (string) target.Rows [1] [1, DataRowVersion.Original], "#C1");

			// check for unchanged row (3rd row)
			Assert.AreEqual ((string) src.Rows [2] [1], (string) target.Rows [2] [1], "#D1");
			Assert.AreEqual ((string) src.Rows [2] [1, DataRowVersion.Default], (string) target.Rows [2] [1, DataRowVersion.Default], "#D2");
			Assert.AreEqual ((string) src.Rows [2] [1, DataRowVersion.Original], (string) target.Rows [2] [1, DataRowVersion.Original], "#D3");
			Assert.AreEqual ((string) src.Rows [2] [1, DataRowVersion.Current], (string) target.Rows [2] [1, DataRowVersion.Current], "#D4");

			// check for newly added row (4th row)
			Assert.AreEqual ((string) src.Rows [3] [1], (string) target.Rows [3] [1], "#E1");
			Assert.AreEqual ((string) src.Rows [3] [1, DataRowVersion.Default], (string) target.Rows [3] [1, DataRowVersion.Default], "#E2");
			Assert.AreEqual ((string) src.Rows [3] [1, DataRowVersion.Current], (string) target.Rows [3] [1, DataRowVersion.Current], "#E3");
		}

		[Test]
		public void ImportRowDetachedTest ()
		{
			DataTable table = new DataTable ();
			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = typeof (int);
			table.Columns.Add (col);

			table.PrimaryKey = new DataColumn [] {col};

			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = typeof (string);
			table.Columns.Add (col);

			DataRow row = table.NewRow ();
			row ["Id"] = 147;
			row ["name"] = "Abc";

			// keep silent as ms.net ;-), though this is not useful.
			table.ImportRow (row);

			//if RowState is detached, then dont import the row.
			Assert.AreEqual (0, table.Rows.Count, "#1");
		}

		[Test]
		public void ImportRowDeletedTest ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col", typeof (int));
			table.Columns.Add ("col1", typeof (int));

			DataRow row = table.Rows.Add (new object[] {1,2});
			table.PrimaryKey = new DataColumn[] {table.Columns[0]};
			table.AcceptChanges ();

			// If row is in Deleted state, then ImportRow loads the
			// row.
			row.Delete ();
			table.ImportRow (row);
			Assert.AreEqual (2, table.Rows.Count, "#A1");

			// Both the deleted rows shud be now gone
			table.AcceptChanges ();
			Assert.AreEqual (0, table.Rows.Count, "#A2");

			//just add another row
			row = table.Rows.Add (new object[] {1,2});
			// no exception shud be thrown
			table.AcceptChanges ();

			// If row is in Deleted state, then ImportRow loads the
			// row and validate only on RejectChanges
			row.Delete ();
			table.ImportRow (row);
			Assert.AreEqual (2, table.Rows.Count, "#A3");
			Assert.AreEqual (DataRowState.Deleted, table.Rows[1].RowState, "#A4");

			try {
				table.RejectChanges ();
				Assert.Fail ("#B1");
			} catch (ConstraintException ex) {
				// Column 'col' is constrained to be unique.
				// Value '1' is already present
				Assert.AreEqual (typeof (ConstraintException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'col'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("'1'") != -1, "#B6");
			}
		}
		
		[Test]
		public void ImportRowTypeChangeTest ()
		{
			// this is from http://bugzilla.xamarin.com/show_bug.cgi?id=2926
	
			Type [] types = new Type [] { typeof (string), typeof (sbyte), typeof (byte), typeof (short), typeof (ushort), typeof (int), typeof (uint), typeof (long), typeof (ulong), typeof (float), typeof (double), typeof (char), typeof (decimal), typeof (DateTime) };
			object [] values = new object [] { "1", (sbyte) 1, (byte) 2, (short) 3, (ushort) 4, (int) 5, (uint) 6, (long) 7, (ulong) 8, (float) 9, (double) 10, 'z', (decimal) 13, new DateTime (24) };
			int length = types.Length;
	
			HashSet<Tuple<Type, Type>> invalid = new HashSet<Tuple<Type, Type>> () {
				Tuple.Create (typeof (string), typeof (DateTime)), 
				Tuple.Create (typeof (sbyte), typeof (DateTime)), 
				Tuple.Create (typeof (byte), typeof (DateTime)), 
				Tuple.Create (typeof (short), typeof (DateTime)), 
				Tuple.Create (typeof (ushort), typeof (DateTime)), 
				Tuple.Create (typeof (int), typeof (DateTime)), 
				Tuple.Create (typeof (uint), typeof (DateTime)), 
				Tuple.Create (typeof (long), typeof (DateTime)), 
				Tuple.Create (typeof (ulong), typeof (DateTime)), 
				Tuple.Create (typeof (float), typeof (char)), 
				Tuple.Create (typeof (float), typeof (DateTime)), 
				Tuple.Create (typeof (double), typeof (char)), 
				Tuple.Create (typeof (double), typeof (DateTime)), 
				Tuple.Create (typeof (char), typeof (float)), 
				Tuple.Create (typeof (char), typeof (double)), 
				Tuple.Create (typeof (char), typeof (decimal)), 
				Tuple.Create (typeof (char), typeof (DateTime)), 
				Tuple.Create (typeof (Decimal), typeof (char)), 
				Tuple.Create (typeof (Decimal), typeof (DateTime)), 
				Tuple.Create (typeof (DateTime), typeof (sbyte)), 
				Tuple.Create (typeof (DateTime), typeof (byte)), 
				Tuple.Create (typeof (DateTime), typeof (short)), 
				Tuple.Create (typeof (DateTime), typeof (ushort)), 
				Tuple.Create (typeof (DateTime), typeof (int)), 
				Tuple.Create (typeof (DateTime), typeof (uint)), 
				Tuple.Create (typeof (DateTime), typeof (long)), 
				Tuple.Create (typeof (DateTime), typeof (ulong)), 
				Tuple.Create (typeof (DateTime), typeof (float)), 
				Tuple.Create (typeof (DateTime), typeof (double)), 
				Tuple.Create (typeof (DateTime), typeof (char)), 
				Tuple.Create (typeof (DateTime), typeof (decimal)), 
			};
	
			for (int a = 0; a < length; a++) {
				for (int b = 0; b < length; b++) {
					DataSet ds = new DataSet ();
					DataTable dt1 = ds.Tables.Add ("T1");
					DataTable dt2 = ds.Tables.Add ("T2");
	
					string name = "C-" + types [a].Name + "-to-" + types [b].Name;
					dt1.Columns.Add (name, types [a]);
					dt2.Columns.Add (name, types [b]);
	
					DataRow r1 = dt1.NewRow ();
					dt1.Rows.Add (r1);
	
					r1 [0] = values [a];
	
					if (invalid.Contains (Tuple.Create (types [a], types [b]))) {
						try {
							dt2.ImportRow (r1);
							Assert.Fail ("#B: " + name + " expected ArgumentException");
						} catch /*(ArgumentException)*/ {
							continue;
						}
					} else {
						dt2.ImportRow (r1);
						DataRow r2 = dt2.Rows [0];
						Assert.AreEqual (types [b], r2 [0].GetType (), "#A: " + name);
					}
				}
			}
		}
			
		[Test]
		public void ClearReset () //To test Clear and Reset methods
		{
			DataTable table = new DataTable ("table");
			DataTable table1 = new DataTable ("table1");

			DataSet set = new DataSet ();
			set.Tables.Add (table);
			set.Tables.Add (table1);

			table.Columns.Add ("Id", typeof (int));
			table.Columns.Add ("Name", typeof (string));
			table.Constraints.Add (new UniqueConstraint ("UK1", table.Columns [0]));
			table.CaseSensitive = false;

			table1.Columns.Add ("Id", typeof (int));
			table1.Columns.Add ("Name", typeof (string));

			DataRelation dr = new DataRelation ("DR", table.Columns[0], table1.Columns[0]);
			set.Relations.Add (dr);

			DataRow row = table.NewRow ();
			row ["Id"] = 147;
			row ["name"] = "Roopa";
			table.Rows.Add (row);

			row = table.NewRow ();
			row ["Id"] = 47;
			row ["Name"] = "roopa";
			table.Rows.Add (row);

			Assert.AreEqual (2, table.Rows.Count);
			Assert.AreEqual (1, table.ChildRelations.Count);
			try {
				table.Reset ();
				Assert.Fail ("#A01, should have thrown ArgumentException");
			} catch (ArgumentException) {
			}

			Assert.AreEqual (0, table.Rows.Count, "#CT01");
			Assert.AreEqual (0, table.ChildRelations.Count, "#CT02");
			Assert.AreEqual (0, table.ParentRelations.Count, "#CT03");
			Assert.AreEqual (0, table.Constraints.Count, "#CT04");

			table1.Reset ();
			Assert.AreEqual (0, table1.Rows.Count, "#A05");
			Assert.AreEqual (0, table1.Constraints.Count, "#A06");
			Assert.AreEqual (0, table1.ParentRelations.Count, "#A07");
		
			// clear test
			table.Clear ();
			Assert.AreEqual (0, table.Rows.Count, "#A08");
			Assert.AreEqual (0, table.Constraints.Count, "#A09");
			Assert.AreEqual (0, table.ChildRelations.Count, "#A10");
		}

		[Test]
		public void ClearTest ()
		{
			DataTable table = new DataTable ("test");
			table.Columns.Add ("id", typeof (int));
			table.Columns.Add ("name", typeof (string));

			table.PrimaryKey = new DataColumn [] { table.Columns [0] } ;

			table.Rows.Add (new object [] { 1, "mono 1" });
			table.Rows.Add (new object [] { 2, "mono 2" });
			table.Rows.Add (new object [] { 3, "mono 3" });
			table.Rows.Add (new object [] { 4, "mono 4" });

			table.AcceptChanges ();
			_tableClearedEventFired = false;
			table.TableCleared += new DataTableClearEventHandler (OnTableCleared);
			_tableClearingEventFired = false;
			table.TableClearing += new DataTableClearEventHandler (OnTableClearing);

			table.Clear ();
			Assert.IsTrue (_tableClearingEventFired, "#3 should have fired cleared event");
			Assert.IsTrue (_tableClearedEventFired, "#0 should have fired cleared event");

			DataRow r = table.Rows.Find (1);
			Assert.IsTrue (r == null, "#1 should have cleared");

			// try adding new row. indexes should have cleared
			table.Rows.Add (new object [] { 2, "mono 2" });
			Assert.AreEqual (1, table.Rows.Count, "#2 should add row");
		}

		private bool _tableClearedEventFired;
		private void OnTableCleared (object src, DataTableClearEventArgs args)
		{
			_tableClearedEventFired = true;
		}

		private bool _tableClearingEventFired;
		private void OnTableClearing (object src, DataTableClearEventArgs args)
		{
			_tableClearingEventFired = true;
		}

		private bool _tableNewRowAddedEventFired;
		private void OnTableNewRowAdded (object src, DataTableNewRowEventArgs args)
		{
			_tableNewRowAddedEventFired = true;
		}

		[Test]
		public void TestWriteXmlSchema1 ()
		{
			DataTable dt = new DataTable("TestWriteXmlSchema");
			dt.Columns.Add("Col1", typeof(int));
			dt.Columns.Add("Col2", typeof(int));
			DataRow dr = dt.NewRow();
			dr[0] = 10;
			dr[1] = 20;
			dt.Rows.Add (dr);
			DataTable dt1 = new DataTable("HelloWorld");
			dt1.Columns.Add("T1", typeof(int));
			dt1.Columns.Add("T2", typeof(int));
			DataRow dr1 = dt1.NewRow();
			dr1[0] = 10;
			dr1[1] = 20;
			dt1.Rows.Add(dr1);
			TextWriter writer = new StringWriter ();
			dt.WriteXmlSchema (writer);
			string TextString = writer.ToString ();
			string substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring, "test#01");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring, "test#02");
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:MainDataTable=\"TestWriteXmlSchema\" msdata:UseCurrentLocale=\"true\">", substring, "test#03");
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:complexType>", substring, "test#04");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">", substring, "test#05");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element name=\"TestWriteXmlSchema\">", substring, "test#06");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          <xs:complexType>", substring, "test#07");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            <xs:sequence>", substring, "test#08");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element name=\"Col1\" type=\"xs:int\" minOccurs=\"0\" />", substring, "test#09");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element name=\"Col2\" type=\"xs:int\" minOccurs=\"0\" />", substring, "test#10");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            </xs:sequence>", substring, "test#11");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          </xs:complexType>", substring, "test#12");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        </xs:element>", substring, "test#13");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      </xs:choice>", substring, "test#14");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:complexType>", substring, "test#15");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:element>", substring, "test#16");
			Assert.AreEqual ("</xs:schema>", TextString, "test#17");
		}

		[Test]
		public void TestWriteXmlSchema2()
		{
			DataTable dt = new DataTable("TestWriteXmlSchema");
			dt.Columns.Add("Col1", typeof(int));
			dt.Columns.Add("Col2", typeof(int));
			DataRow dr = dt.NewRow();
			dr[0] = 10;
			dr[1] = 20;
			dt.Rows.Add (dr);
			DataTable dt1 = new DataTable("HelloWorld");
			dt1.Columns.Add("T1", typeof(int));
			dt1.Columns.Add("T2", typeof(int));
			DataRow dr1 = dt1.NewRow();
			dr1[0] = 10;
			dr1[1] = 20;
			dt1.Rows.Add(dr1);
			DataSet ds = new DataSet();
			ds.Tables.Add(dt);
			ds.Tables.Add(dt1);
			DataRelation rel = new DataRelation("Relation1", dt.Columns["Col1"], dt1.Columns["T1"]);
			ds.Relations.Add(rel);
			TextWriter writer = new StringWriter ();
			dt.WriteXmlSchema (writer);
			string TextString = writer.ToString ();
			string substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring, "test#01");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring, "test#02");
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:MainDataTable=\"TestWriteXmlSchema\" msdata:UseCurrentLocale=\"true\">", substring, "test#03");
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:complexType>", substring, "test#04");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">", substring, "test#05");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element name=\"TestWriteXmlSchema\">", substring, "test#06");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          <xs:complexType>", substring, "test#07");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            <xs:sequence>", substring, "test#08");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element name=\"Col1\" type=\"xs:int\" minOccurs=\"0\" />", substring, "test#09");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element name=\"Col2\" type=\"xs:int\" minOccurs=\"0\" />", substring, "test#10");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            </xs:sequence>", substring, "test#11");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          </xs:complexType>", substring, "test#12");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        </xs:element>", substring, "test#13");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      </xs:choice>", substring, "test#14");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:complexType>", substring, "test#15");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:unique name=\"Constraint1\">", substring, "test#16");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:selector xpath=\".//TestWriteXmlSchema\" />", substring, "test#17");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:field xpath=\"Col1\" />", substring, "test#18");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:unique>", substring, "test#19");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:element>", substring, "test#20");
			Assert.AreEqual ("</xs:schema>", TextString, "test#21");
		}

		[Test]
		public void TestWriteXmlSchema3()
		{
			DataTable dt = new DataTable("TestWriteXmlSchema");
			dt.Columns.Add("Col1", typeof(int));
			dt.Columns.Add("Col2", typeof(int));
			DataRow dr = dt.NewRow();
			dr[0] = 10;
			dr[1] = 20;
			dt.Rows.Add (dr);
			DataTable dt1 = new DataTable("HelloWorld");
			dt1.Columns.Add("T1", typeof(int));
			dt1.Columns.Add("T2", typeof(int));
			DataRow dr1 = dt1.NewRow();
			dr1[0] = 10;
			dr1[1] = 20;
			dt1.Rows.Add(dr1);
			DataSet ds = new DataSet();
			ds.Tables.Add(dt);
			ds.Tables.Add(dt1);
			DataRelation rel = new DataRelation("Relation1", dt.Columns["Col1"], dt1.Columns["T1"]);
			ds.Relations.Add(rel);
			TextWriter writer = new StringWriter ();
			dt.WriteXmlSchema (writer, true);
			string TextString = writer.ToString ();
			string substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring, "test#01");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring, "test#02");
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:MainDataTable=\"TestWriteXmlSchema\" msdata:UseCurrentLocale=\"true\">", substring, "test#03");
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:complexType>", substring, "test#04");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">", substring, "test#05");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element name=\"TestWriteXmlSchema\">", substring, "test#06");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          <xs:complexType>", substring, "test#07");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            <xs:sequence>", substring, "test#08");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element name=\"Col1\" type=\"xs:int\" minOccurs=\"0\" />", substring, "test#09");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element name=\"Col2\" type=\"xs:int\" minOccurs=\"0\" />", substring, "test#10");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            </xs:sequence>", substring, "test#11");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          </xs:complexType>", substring, "test#12");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        </xs:element>", substring, "test#13");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element name=\"HelloWorld\">", substring, "test#14");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          <xs:complexType>", substring, "test#15");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            <xs:sequence>", substring, "test#16");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element name=\"T1\" type=\"xs:int\" minOccurs=\"0\" />", substring, "test#17");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element name=\"T2\" type=\"xs:int\" minOccurs=\"0\" />", substring, "test#18");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            </xs:sequence>", substring, "test#19");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          </xs:complexType>", substring, "test#20");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        </xs:element>", substring, "test#21");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      </xs:choice>", substring, "test#22");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:complexType>", substring, "test#23");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:unique name=\"Constraint1\">", substring, "test#24");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:selector xpath=\".//TestWriteXmlSchema\" />", substring, "test#25");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:field xpath=\"Col1\" />", substring, "test#26");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:unique>", substring, "test#27");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:keyref name=\"Relation1\" refer=\"Constraint1\">", substring, "test#28");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:selector xpath=\".//HelloWorld\" />", substring, "test#29");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:field xpath=\"T1\" />", substring, "test#30");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:keyref>", substring, "test#31");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:element>", substring, "test#32");
			Assert.AreEqual ("</xs:schema>", TextString, "test#33");
		}

		[Test]
		public void Serialize ()
		{
			MemoryStream fs = new MemoryStream ();

			// Construct a BinaryFormatter and use it 
			// to serialize the data to the stream.
			BinaryFormatter formatter = new BinaryFormatter();

			// Create an array with multiple elements refering to 
			// the one Singleton object.
			DataTable dt = new DataTable();

			dt.Columns.Add(new DataColumn("Id", typeof(string)));
			dt.Columns.Add(new DataColumn("ContactName", typeof(string)));
			dt.Columns.Add(new DataColumn("ContactTitle", typeof(string)));
			dt.Columns.Add(new DataColumn("ContactAreaCode", typeof(string)));
			dt.Columns.Add(new DataColumn("ContactPhone", typeof(string)));

			DataRow loRowToAdd;
			loRowToAdd = dt.NewRow();
			loRowToAdd[0] = "a";
			loRowToAdd[1] = "b";
			loRowToAdd[2] = "c";
			loRowToAdd[3] = "d";
			loRowToAdd[4] = "e";
			dt.Rows.Add(loRowToAdd);

			DataTable[] dtarr = new DataTable[] {dt}; 

			// Serialize the array elements.
			formatter.Serialize(fs, dtarr);

			// Deserialize the array elements.
			fs.Position = 0;
			DataTable[] a2 = (DataTable[]) formatter.Deserialize(fs);

			DataSet ds = new DataSet();
			ds.Tables.Add(a2[0]);

			StringWriter sw = new StringWriter ();
			ds.WriteXml(sw);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sw.ToString ());
			Assert.AreEqual (5, doc.DocumentElement.FirstChild.ChildNodes.Count);
		}

		[Test]
		public void SetPrimaryKeyAssertsNonNull ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Constraints.Add (new UniqueConstraint (dt.Columns [0]));
			dt.Rows.Add (new object [] {1, 3});
			dt.Rows.Add (new object [] {DBNull.Value, 3});

			try {
				dt.PrimaryKey = new DataColumn [] { dt.Columns [0] };
				Assert.Fail ("#1");
			} catch (DataException) {
			}
		}

		[Test]
		public void PrimaryKeyColumnChecksNonNull ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Constraints.Add (new UniqueConstraint (dt.Columns [0]));
			dt.PrimaryKey = new DataColumn [] {dt.Columns [0]};
			dt.Rows.Add (new object [] {1, 3});

			try {
				dt.Rows.Add (new object [] { DBNull.Value, 3 });
				Assert.Fail ("#1");
			} catch (NoNullAllowedException) {
			}
		}

		[Test]
		public void PrimaryKey_CheckSetsAllowDBNull ()
		{
			DataTable table = new DataTable ();
			DataColumn col1 = table.Columns.Add ("col1", typeof (int));
			DataColumn col2 = table.Columns.Add ("col2", typeof (int));
	
			Assert.IsTrue (col1.AllowDBNull, "#1" );
			Assert.IsTrue (col2.AllowDBNull, "#2" );
			Assert.IsFalse (col2.Unique, "#3" );
			Assert.IsFalse (col2.Unique, "#4" );

			table.PrimaryKey = new DataColumn[] {col1,col2};
			Assert.IsFalse (col1.AllowDBNull, "#5" );
			Assert.IsFalse (col2.AllowDBNull, "#6" );
			// LAMESPEC or bug ?? 
			Assert.IsFalse (col1.Unique, "#7" );
			Assert.IsFalse (col2.Unique, "#8" );
		}

		void RowChanging (object o, DataRowChangeEventArgs e)
		{
			Assert.AreEqual (rowChangingExpectedAction, e.Action, "changing.Action");
			rowChangingRowChanging = true;
		}

		void RowChanged (object o, DataRowChangeEventArgs e)
		{
			Assert.AreEqual (rowChangingExpectedAction, e.Action, "changed.Action");
			rowChangingRowChanged = true;
		}

		bool rowChangingRowChanging, rowChangingRowChanged;
		DataRowAction rowChangingExpectedAction;

		[Test]
		public void RowChanging ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.RowChanging += new DataRowChangeEventHandler (RowChanging);
			dt.RowChanged += new DataRowChangeEventHandler (RowChanged);
			rowChangingExpectedAction = DataRowAction.Add;
			dt.Rows.Add (new object [] {1, 2});
			Assert.IsTrue (rowChangingRowChanging, "changing,Added");
			Assert.IsTrue (rowChangingRowChanged, "changed,Added");
			rowChangingExpectedAction = DataRowAction.Change;
			dt.Rows [0] [0] = 2;
			Assert.IsTrue (rowChangingRowChanging, "changing,Changed");
			Assert.IsTrue (rowChangingRowChanged, "changed,Changed");
		}

		[Test]
		public void CloneSubClassTest()
		{
			MyDataTable dt1 = new MyDataTable();
			MyDataTable dt = (MyDataTable)(dt1.Clone());
			Assert.AreEqual (2, MyDataTable.count, "A#01");
		}

		DataRowAction rowActionChanging = DataRowAction.Nothing;
		DataRowAction rowActionChanged  = DataRowAction.Nothing;
		[Test]
		public void AcceptChangesTest ()
		{
			DataTable dt = new DataTable ("test");
			dt.Columns.Add ("id", typeof (int));
			dt.Columns.Add ("name", typeof (string));

			dt.Rows.Add (new object [] { 1, "mono 1" });

			dt.RowChanged  += new DataRowChangeEventHandler (OnRowChanged);
			dt.RowChanging += new DataRowChangeEventHandler (OnRowChanging);

			try {
				rowActionChanged = rowActionChanging = DataRowAction.Nothing;
				dt.AcceptChanges ();

				Assert.AreEqual (DataRowAction.Commit, rowActionChanging,
						 "#1 should have fired event and set action to commit");
				Assert.AreEqual (DataRowAction.Commit, rowActionChanged,
						 "#2 should have fired event and set action to commit");
			} finally {
				dt.RowChanged  -= new DataRowChangeEventHandler (OnRowChanged);
				dt.RowChanging -= new DataRowChangeEventHandler (OnRowChanging);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ColumnObjectTypeTest() {
			DataTable dt = new DataTable();
			dt.Columns.Add("Series Label", typeof(SqlInt32));
			dt.Rows.Add(new object[] {"sss"});
		}

		private bool tableInitialized;
		[Test]
		public void TableInitializedEventTest1 ()
		{
			DataTable dt = new DataTable();
			tableInitialized = false;
			dt.Initialized += new EventHandler (OnTableInitialized);
			dt.Columns.Add("Series Label", typeof(SqlInt32));
			dt.Rows.Add(new object[] {123});
			Assert.IsFalse (tableInitialized, "TableInitialized #01");
			dt.Initialized -= new EventHandler (OnTableInitialized);
		}

		[Test]
		public void TableInitializedEventTest2 ()
		{
			DataTable dt = new DataTable();
			dt.BeginInit ();
			tableInitialized = false;
			dt.Initialized += new EventHandler (OnTableInitialized);
			dt.Columns.Add("Series Label", typeof(SqlInt32));
			dt.Rows.Add(new object[] {123});
			dt.EndInit ();
			dt.Initialized -= new EventHandler (OnTableInitialized);
			Assert.IsTrue (tableInitialized, "TableInitialized #02");
		}

		[Test]
		public void TableInitializedEventTest3 ()
		{
			DataTable dt = new DataTable();
			tableInitialized = true;
			dt.Initialized += new EventHandler (OnTableInitialized);
			dt.Columns.Add("Series Label", typeof(SqlInt32));
			dt.Rows.Add(new object[] {123});
			Assert.AreEqual (tableInitialized, dt.IsInitialized, "TableInitialized #03");
			dt.Initialized -= new EventHandler (OnTableInitialized);
		}

		[Test]
		public void TableInitializedEventTest4 ()
		{
			DataTable dt = new DataTable();
			Assert.IsTrue (dt.IsInitialized, "TableInitialized #04");
			dt.BeginInit ();
			tableInitialized = false;
			dt.Initialized += new EventHandler (OnTableInitialized);
			dt.Columns.Add("Series Label", typeof(SqlInt32));
			dt.Rows.Add(new object[] {123});
			Assert.IsFalse (dt.IsInitialized, "TableInitialized #05");
			dt.EndInit ();
			Assert.IsTrue (dt.IsInitialized, "TableInitialized #06");
			Assert.IsTrue (tableInitialized, "TableInitialized #07");
			dt.Initialized -= new EventHandler (OnTableInitialized);
		}

		private void OnTableInitialized (object src, EventArgs args)
		{
			tableInitialized = true;
		}

		public void OnRowChanging (object src, DataRowChangeEventArgs args)
		{
			rowActionChanging = args.Action;
		}

		public void OnRowChanged (object src, DataRowChangeEventArgs args)
		{
			rowActionChanged = args.Action;
		}

		private DataTable dt;
		private void localSetup () {
			dt = new DataTable ("test");
			dt.Columns.Add ("id", typeof (int));
			dt.Columns.Add ("name", typeof (string));
			dt.PrimaryKey = new DataColumn[] { dt.Columns["id"] };

			dt.Rows.Add (new object[] { 1, "mono 1" });
			dt.Rows.Add (new object[] { 2, "mono 2" });
			dt.Rows.Add (new object[] { 3, "mono 3" });

			dt.AcceptChanges ();
		}

		#region DataTable.CreateDataReader Tests

		[Test]
		public void CreateDataReader1 ()
		{
			localSetup ();
			DataTableReader dtr = dt.CreateDataReader ();
			Assert.IsTrue (dtr.HasRows, "HasRows");
			Assert.AreEqual (dt.Columns.Count, dtr.FieldCount, "CountCols");
			int ri = 0;
			while (dtr.Read ()) {
				for (int i = 0; i < dtr.FieldCount; i++) {
					Assert.AreEqual (dt.Rows[ri][i], dtr[i], "RowData-" + ri + "-" + i);
				}
				ri++;
			}
		}

		[Test]
		public void CreateDataReader2 ()
		{
			localSetup ();
			DataTableReader dtr = dt.CreateDataReader ();
			Assert.IsTrue (dtr.HasRows, "HasRows");
			Assert.AreEqual (dt.Columns.Count, dtr.FieldCount, "CountCols");
			dtr.Read ();
			Assert.AreEqual (1, dtr[0], "RowData0-0");
			Assert.AreEqual ("mono 1", dtr[1], "RowData0-1");
			dtr.Read ();
			Assert.AreEqual (2, dtr[0], "RowData1-0");
			Assert.AreEqual ("mono 2", dtr[1], "RowData1-1");
			dtr.Read ();
			Assert.AreEqual (3, dtr[0], "RowData2-0");
			Assert.AreEqual ("mono 3", dtr[1], "RowData2-1");
		}

		#endregion // DataTable.CreateDataReader Tests

		#region DataTable.Load Tests

		[Test]
		public void Load_Basic ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadBasic");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.Columns["id"].ReadOnly = true;
			dtLoad.Columns["name"].ReadOnly = true;
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "load 1" });
			dtLoad.Rows.Add (new object[] { 2, "load 2" });
			dtLoad.Rows.Add (new object[] { 3, "load 3" });
			dtLoad.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr);
			Assert.AreEqual (2, dtLoad.Columns.Count, "NColumns");
			Assert.AreEqual (3, dtLoad.Rows.Count, "NRows");
			Assert.AreEqual (1, dtLoad.Rows[0][0], "RowData0-0");
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1], "RowData0-1");
			Assert.AreEqual (2, dtLoad.Rows[1][0], "RowData1-0");
			Assert.AreEqual ("mono 2", dtLoad.Rows[1][1], "RowData1-1");
			Assert.AreEqual (3, dtLoad.Rows[2][0], "RowData2-0");
			Assert.AreEqual ("mono 3", dtLoad.Rows[2][1], "RowData2-1");
		}

		[Test]
		public void Load_NoSchema ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadNoSchema");
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr);
			Assert.AreEqual (2, dtLoad.Columns.Count, "NColumns");
			Assert.AreEqual (3, dtLoad.Rows.Count, "NRows");
			Assert.AreEqual (1, dtLoad.Rows[0][0], "RowData0-0");
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1], "RowData0-1");
			Assert.AreEqual (2, dtLoad.Rows[1][0], "RowData1-0");
			Assert.AreEqual ("mono 2", dtLoad.Rows[1][1], "RowData1-1");
			Assert.AreEqual (3, dtLoad.Rows[2][0], "RowData2-0");
			Assert.AreEqual ("mono 3", dtLoad.Rows[2][1], "RowData2-1");
		}

		internal struct fillErrorStruct
		{
			internal string error;
			internal string tableName;
			internal int rowKey;
			internal bool contFlag;

			internal void init (string tbl, int row, bool cont, string err)
			{
				tableName = tbl;
				rowKey = row;
				contFlag = cont;
				error = err;
			}
		}
		private fillErrorStruct[] fillErr = new fillErrorStruct[3];
		private int fillErrCounter;
		private void fillErrorHandler (object sender, FillErrorEventArgs e)
		{
			e.Continue = fillErr[fillErrCounter].contFlag;
			Assert.AreEqual (fillErr[fillErrCounter].tableName, e.DataTable.TableName, "fillErr-T");
			//Assert.AreEqual (fillErr[fillErrCounter].rowKey, e.Values[0], "fillErr-R");
			Assert.AreEqual (fillErr[fillErrCounter].contFlag, e.Continue, "fillErr-C");
			//Assert.AreEqual (fillErr[fillErrCounter].error, e.Errors.Message, "fillErr-E");
			fillErrCounter++;
		}

		[Test]
		public void Load_Incompatible ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadIncompatible");
			dtLoad.Columns.Add ("name", typeof (double));
			DataTableReader dtr = dt.CreateDataReader ();
			try {
				dtLoad.Load (dtr);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}
		[Test]
		// Load doesn't have a third overload in System.Data
		// and is commented-out below
		public void Load_IncompatibleEHandlerT ()
		{
			fillErrCounter = 0;
			fillErr[0].init ("LoadIncompatible", 1, true,
				 "Input string was not in a correct format.Couldn't store <mono 1> in name Column.  Expected type is Double.");
			fillErr[1].init ("LoadIncompatible", 2, true,
				"Input string was not in a correct format.Couldn't store <mono 2> in name Column.  Expected type is Double.");
			fillErr[2].init ("LoadIncompatible", 3, true,
				"Input string was not in a correct format.Couldn't store <mono 3> in name Column.  Expected type is Double.");
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadIncompatible");
			dtLoad.Columns.Add ("name", typeof (double));
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr,LoadOption.PreserveChanges,fillErrorHandler);
		}

		[Test]
		// Load doesn't have a third overload in System.Data
		// and is commented-out below
		public void Load_IncompatibleEHandlerF ()
		{
			fillErrCounter = 0;
			fillErr[0].init ("LoadIncompatible", 1, false,
				"Input string was not in a correct format.Couldn't store <mono 1> in name Column.  Expected type is Double.");
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadIncompatible");
			dtLoad.Columns.Add ("name", typeof (double));
			DataTableReader dtr = dt.CreateDataReader ();
			try {
				dtLoad.Load (dtr, LoadOption.PreserveChanges, fillErrorHandler);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Load_ExtraColsEqualVal ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadExtraCols");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1 });
			dtLoad.Rows.Add (new object[] { 2 });
			dtLoad.Rows.Add (new object[] { 3 });
			dtLoad.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr);
			Assert.AreEqual (2, dtLoad.Columns.Count, "NColumns");
			Assert.AreEqual (3, dtLoad.Rows.Count, "NRows");
			Assert.AreEqual (1, dtLoad.Rows[0][0], "RowData0-0");
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1], "RowData0-1");
			Assert.AreEqual (2, dtLoad.Rows[1][0], "RowData1-0");
			Assert.AreEqual ("mono 2", dtLoad.Rows[1][1], "RowData1-1");
			Assert.AreEqual (3, dtLoad.Rows[2][0], "RowData2-0");
			Assert.AreEqual ("mono 3", dtLoad.Rows[2][1], "RowData2-1");
		}

		[Test]
		public void Load_ExtraColsNonEqualVal ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadExtraCols");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 4 });
			dtLoad.Rows.Add (new object[] { 5 });
			dtLoad.Rows.Add (new object[] { 6 });
			dtLoad.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr);
			Assert.AreEqual (2, dtLoad.Columns.Count, "NColumns");
			Assert.AreEqual (6, dtLoad.Rows.Count, "NRows");
			Assert.AreEqual (4, dtLoad.Rows[0][0], "RowData0-0");
			Assert.AreEqual (5, dtLoad.Rows[1][0], "RowData1-0");
			Assert.AreEqual (6, dtLoad.Rows[2][0], "RowData2-0");
			Assert.AreEqual (1, dtLoad.Rows[3][0], "RowData3-0");
			Assert.AreEqual ("mono 1", dtLoad.Rows[3][1], "RowData3-1");
			Assert.AreEqual (2, dtLoad.Rows[4][0], "RowData4-0");
			Assert.AreEqual ("mono 2", dtLoad.Rows[4][1], "RowData4-1");
			Assert.AreEqual (3, dtLoad.Rows[5][0], "RowData5-0");
			Assert.AreEqual ("mono 3", dtLoad.Rows[5][1], "RowData5-1");
		}

		[Test]
		public void Load_MissingColsNonNullable ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadMissingCols");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.Columns.Add ("missing", typeof (string));
			dtLoad.Columns["missing"].AllowDBNull = false;
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 4, "mono 4", "miss4" });
			dtLoad.Rows.Add (new object[] { 5, "mono 5", "miss5" });
			dtLoad.Rows.Add (new object[] { 6, "mono 6", "miss6" });
			dtLoad.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			try {
				dtLoad.Load (dtr);
				Assert.Fail ("#1");
			} catch (ConstraintException) {
			}
		}

		[Test]
		public void Load_MissingColsDefault ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadMissingCols");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.Columns.Add ("missing", typeof (string));
			dtLoad.Columns["missing"].AllowDBNull = false;
			dtLoad.Columns["missing"].DefaultValue = "DefaultValue";
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 4, "mono 4", "miss4" });
			dtLoad.Rows.Add (new object[] { 5, "mono 5", "miss5" });
			dtLoad.Rows.Add (new object[] { 6, "mono 6", "miss6" });
			dtLoad.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr);
			Assert.AreEqual (3, dtLoad.Columns.Count, "NColumns");
			Assert.AreEqual (6, dtLoad.Rows.Count, "NRows");
			Assert.AreEqual (4, dtLoad.Rows[0][0], "RowData0-0");
			Assert.AreEqual ("mono 4", dtLoad.Rows[0][1], "RowData0-1");
			Assert.AreEqual ("miss4", dtLoad.Rows[0][2], "RowData0-2");
			Assert.AreEqual (5, dtLoad.Rows[1][0], "RowData1-0");
			Assert.AreEqual ("mono 5", dtLoad.Rows[1][1], "RowData1-1");
			Assert.AreEqual ("miss5", dtLoad.Rows[1][2], "RowData1-2");
			Assert.AreEqual (6, dtLoad.Rows[2][0], "RowData2-0");
			Assert.AreEqual ("mono 6", dtLoad.Rows[2][1], "RowData2-1");
			Assert.AreEqual ("miss6", dtLoad.Rows[2][2], "RowData2-2");
			Assert.AreEqual (1, dtLoad.Rows[3][0], "RowData3-0");
			Assert.AreEqual ("mono 1", dtLoad.Rows[3][1], "RowData3-1");
			Assert.AreEqual ("DefaultValue", dtLoad.Rows[3][2], "RowData3-2");
			Assert.AreEqual (2, dtLoad.Rows[4][0], "RowData4-0");
			Assert.AreEqual ("mono 2", dtLoad.Rows[4][1], "RowData4-1");
			Assert.AreEqual ("DefaultValue", dtLoad.Rows[4][2], "RowData4-2");
			Assert.AreEqual (3, dtLoad.Rows[5][0], "RowData5-0");
			Assert.AreEqual ("mono 3", dtLoad.Rows[5][1], "RowData5-1");
			Assert.AreEqual ("DefaultValue", dtLoad.Rows[5][2], "RowData5-2");
		}

		[Test]
		public void Load_MissingColsNullable ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadMissingCols");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.Columns.Add ("missing", typeof (string));
			dtLoad.Columns["missing"].AllowDBNull = true;
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 4, "mono 4", "miss4" });
			dtLoad.Rows.Add (new object[] { 5, "mono 5", "miss5" });
			dtLoad.Rows.Add (new object[] { 6, "mono 6", "miss6" });
			dtLoad.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr);
			Assert.AreEqual (3, dtLoad.Columns.Count, "NColumns");
			Assert.AreEqual (6, dtLoad.Rows.Count, "NRows");
			Assert.AreEqual (4, dtLoad.Rows[0][0], "RowData0-0");
			Assert.AreEqual ("mono 4", dtLoad.Rows[0][1], "RowData0-1");
			Assert.AreEqual ("miss4", dtLoad.Rows[0][2], "RowData0-2");
			Assert.AreEqual (5, dtLoad.Rows[1][0], "RowData1-0");
			Assert.AreEqual ("mono 5", dtLoad.Rows[1][1], "RowData1-1");
			Assert.AreEqual ("miss5", dtLoad.Rows[1][2], "RowData1-2");
			Assert.AreEqual (6, dtLoad.Rows[2][0], "RowData2-0");
			Assert.AreEqual ("mono 6", dtLoad.Rows[2][1], "RowData2-1");
			Assert.AreEqual ("miss6", dtLoad.Rows[2][2], "RowData2-2");
			Assert.AreEqual (1, dtLoad.Rows[3][0], "RowData3-0");
			Assert.AreEqual ("mono 1", dtLoad.Rows[3][1], "RowData3-1");
			//Assert.IsNull (dtLoad.Rows[3][2], "RowData3-2");
			Assert.AreEqual (2, dtLoad.Rows[4][0], "RowData4-0");
			Assert.AreEqual ("mono 2", dtLoad.Rows[4][1], "RowData4-1");
			//Assert.IsNull (dtLoad.Rows[4][2], "RowData4-2");
			Assert.AreEqual (3, dtLoad.Rows[5][0], "RowData5-0");
			Assert.AreEqual ("mono 3", dtLoad.Rows[5][1], "RowData5-1");
			//Assert.IsNull (dtLoad.Rows[5][2], "RowData5-2");
		}

		private DataTable setupRowState ()
		{
			DataTable tbl = new DataTable ("LoadRowStateChanges");
			tbl.RowChanged += new DataRowChangeEventHandler (dtLoad_RowChanged);
			tbl.RowChanging += new DataRowChangeEventHandler (dtLoad_RowChanging);
			tbl.Columns.Add ("id", typeof (int));
			tbl.Columns.Add ("name", typeof (string));
			tbl.PrimaryKey = new DataColumn[] { tbl.Columns["id"] };
			tbl.Rows.Add (new object[] { 1, "RowState 1" });
			tbl.Rows.Add (new object[] { 2, "RowState 2" });
			tbl.Rows.Add (new object[] { 3, "RowState 3" });
			tbl.AcceptChanges ();
			// Update Table with following changes: Row0 unmodified, 
			// Row1 modified, Row2 deleted, Row3 added, Row4 not-present.
			tbl.Rows[1]["name"] = "Modify 2";
			tbl.Rows[2].Delete ();
			DataRow row = tbl.NewRow ();
			row["id"] = 4;
			row["name"] = "Add 4";
			tbl.Rows.Add (row);
			return (tbl);
		}

		private DataRowAction[] rowChangeAction = new DataRowAction[5];
		private bool checkAction;
		private int rowChagedCounter, rowChangingCounter;
		private void rowActionInit (DataRowAction[] act)
		{
			checkAction = true;
			rowChagedCounter = 0;
			rowChangingCounter = 0;
			for (int i = 0; i < 5; i++)
				rowChangeAction[i] = act[i];
		}

		private void rowActionEnd ()
		{
			checkAction = false;
		}

		private void dtLoad_RowChanged (object sender, DataRowChangeEventArgs e)
		{
			if (checkAction) {
				Assert.AreEqual (rowChangeAction[rowChagedCounter], e.Action, "RowChanged" + rowChagedCounter);
				rowChagedCounter++;
			}
		}

		private void dtLoad_RowChanging (object sender, DataRowChangeEventArgs e)
		{
			if (checkAction) {
				Assert.AreEqual (rowChangeAction[rowChangingCounter], e.Action, "RowChanging" + rowChangingCounter);
				rowChangingCounter++;
			}
		}

		[Test]
		public void Load_RowStateChangesDefault ()
		{
			localSetup ();
			dt.Rows.Add (new object[] { 4, "mono 4" });
			dt.Rows.Add (new object[] { 5, "mono 5" });
			dt.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			DataTable dtLoad = setupRowState ();
			DataRowAction[] dra = new DataRowAction[] {
				DataRowAction.ChangeCurrentAndOriginal,
				DataRowAction.ChangeOriginal,
				DataRowAction.ChangeOriginal,
				DataRowAction.ChangeOriginal,
				DataRowAction.ChangeCurrentAndOriginal};
			rowActionInit (dra);
			dtLoad.Load (dtr);
			rowActionEnd ();
			// asserting Unchanged Row0
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1,DataRowVersion.Current], "RowData0-C");
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1,DataRowVersion.Original], "RowData0-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[0].RowState, "RowState0");
			// asserting Modified Row1
			Assert.AreEqual ("Modify 2", dtLoad.Rows[1][1, DataRowVersion.Current], "RowData1-C");
			Assert.AreEqual ("mono 2", dtLoad.Rows[1][1, DataRowVersion.Original], "RowData1-O");
			Assert.AreEqual (DataRowState.Modified, dtLoad.Rows[1].RowState, "RowState1");
			// asserting Deleted Row2
			Assert.AreEqual ("mono 3", dtLoad.Rows[2][1, DataRowVersion.Original], "RowData1-O");
			Assert.AreEqual (DataRowState.Deleted, dtLoad.Rows[2].RowState, "RowState2");
			// asserting Added Row3
			Assert.AreEqual ("Add 4", dtLoad.Rows[3][1, DataRowVersion.Current], "RowData3-C");
			Assert.AreEqual ("mono 4", dtLoad.Rows[3][1, DataRowVersion.Original], "RowData3-O");
			Assert.AreEqual (DataRowState.Modified, dtLoad.Rows[3].RowState, "RowState3");
			// asserting Unpresent Row4
			Assert.AreEqual ("mono 5", dtLoad.Rows[4][1, DataRowVersion.Current], "RowData4-C");
			Assert.AreEqual ("mono 5", dtLoad.Rows[4][1, DataRowVersion.Original], "RowData4-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[4].RowState, "RowState4");
		}

		[Test]
		public void Load_RowStateChangesDefaultDelete ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadRowStateChanges");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "RowState 1" });
			dtLoad.Rows.Add (new object[] { 2, "RowState 2" });
			dtLoad.Rows.Add (new object[] { 3, "RowState 3" });
			dtLoad.AcceptChanges ();
			dtLoad.Rows[2].Delete ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr);

			try {
				Assert.AreEqual (" ", dtLoad.Rows[2][1, DataRowVersion.Current], "RowData2-C");
				Assert.Fail ("#1");
			} catch (VersionNotFoundException) {
			}
		}

		[Test]
		public void Load_RowStatePreserveChanges ()
		{
			localSetup ();
			dt.Rows.Add (new object[] { 4, "mono 4" });
			dt.Rows.Add (new object[] { 5, "mono 5" });
			dt.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			DataTable dtLoad = setupRowState ();
			DataRowAction[] dra = new DataRowAction[] {
				DataRowAction.ChangeCurrentAndOriginal,
				DataRowAction.ChangeOriginal,
				DataRowAction.ChangeOriginal,
				DataRowAction.ChangeOriginal,
				DataRowAction.ChangeCurrentAndOriginal};
			rowActionInit (dra);
			dtLoad.Load (dtr, LoadOption.PreserveChanges);
			rowActionEnd ();
			// asserting Unchanged Row0
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1, DataRowVersion.Current], "RowData0-C");
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1, DataRowVersion.Original], "RowData0-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[0].RowState, "RowState0");
			// asserting Modified Row1
			Assert.AreEqual ("Modify 2", dtLoad.Rows[1][1, DataRowVersion.Current], "RowData1-C");
			Assert.AreEqual ("mono 2", dtLoad.Rows[1][1, DataRowVersion.Original], "RowData1-O");
			Assert.AreEqual (DataRowState.Modified, dtLoad.Rows[1].RowState, "RowState1");
			// asserting Deleted Row2
			Assert.AreEqual ("mono 3", dtLoad.Rows[2][1, DataRowVersion.Original], "RowData1-O");
			Assert.AreEqual (DataRowState.Deleted, dtLoad.Rows[2].RowState, "RowState2");
			// asserting Added Row3
			Assert.AreEqual ("Add 4", dtLoad.Rows[3][1, DataRowVersion.Current], "RowData3-C");
			Assert.AreEqual ("mono 4", dtLoad.Rows[3][1, DataRowVersion.Original], "RowData3-O");
			Assert.AreEqual (DataRowState.Modified, dtLoad.Rows[3].RowState, "RowState3");
			// asserting Unpresent Row4
			Assert.AreEqual ("mono 5", dtLoad.Rows[4][1, DataRowVersion.Current], "RowData4-C");
			Assert.AreEqual ("mono 5", dtLoad.Rows[4][1, DataRowVersion.Original], "RowData4-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[4].RowState, "RowState4");
		}

		[Test]
		public void Load_RowStatePreserveChangesDelete () {
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadRowStateChanges");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "RowState 1" });
			dtLoad.Rows.Add (new object[] { 2, "RowState 2" });
			dtLoad.Rows.Add (new object[] { 3, "RowState 3" });
			dtLoad.AcceptChanges ();
			dtLoad.Rows[2].Delete ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr,LoadOption.PreserveChanges);

			try {
				Assert.AreEqual (" ", dtLoad.Rows[2][1, DataRowVersion.Current], "RowData2-C");
				Assert.Fail ("#1");
			} catch (VersionNotFoundException) {
			}
		}

		[Test]
		public void Load_RowStateOverwriteChanges ()
		{
			localSetup ();
			dt.Rows.Add (new object[] { 4, "mono 4" });
			dt.Rows.Add (new object[] { 5, "mono 5" });
			dt.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			DataTable dtLoad = setupRowState ();
			DataRowAction[] dra = new DataRowAction[] {
				DataRowAction.ChangeCurrentAndOriginal,
				DataRowAction.ChangeCurrentAndOriginal,
				DataRowAction.ChangeCurrentAndOriginal,
				DataRowAction.ChangeCurrentAndOriginal,
				DataRowAction.ChangeCurrentAndOriginal};
			rowActionInit (dra);
			dtLoad.Load (dtr, LoadOption.OverwriteChanges);
			rowActionEnd ();
			// asserting Unchanged Row0
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1, DataRowVersion.Current], "RowData0-C");
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1, DataRowVersion.Original], "RowData0-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[0].RowState, "RowState0");
			// asserting Modified Row1
			Assert.AreEqual ("mono 2", dtLoad.Rows[1][1, DataRowVersion.Current], "RowData1-C");
			Assert.AreEqual ("mono 2", dtLoad.Rows[1][1, DataRowVersion.Original], "RowData1-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[1].RowState, "RowState1");
			// asserting Deleted Row2
			Assert.AreEqual ("mono 3", dtLoad.Rows[2][1, DataRowVersion.Current], "RowData1-C");
			Assert.AreEqual ("mono 3", dtLoad.Rows[2][1, DataRowVersion.Original], "RowData1-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[2].RowState, "RowState2");
			// asserting Added Row3
			Assert.AreEqual ("mono 4", dtLoad.Rows[3][1, DataRowVersion.Current], "RowData3-C");
			Assert.AreEqual ("mono 4", dtLoad.Rows[3][1, DataRowVersion.Original], "RowData3-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[3].RowState, "RowState3");
			// asserting Unpresent Row4
			Assert.AreEqual ("mono 5", dtLoad.Rows[4][1, DataRowVersion.Current], "RowData4-C");
			Assert.AreEqual ("mono 5", dtLoad.Rows[4][1, DataRowVersion.Original], "RowData4-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[4].RowState, "RowState4");
		}

		[Test]
		public void Load_RowStateUpsert ()
		{
			localSetup ();
			dt.Rows.Add (new object[] { 4, "mono 4" });
			dt.Rows.Add (new object[] { 5, "mono 5" });
			dt.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			DataTable dtLoad = setupRowState ();
			// Notice rowChange-Actions only occur 5 times, as number 
			// of actual rows, ignoring row duplication of the deleted row.
			DataRowAction[] dra = new DataRowAction[] {
				DataRowAction.Change,
				DataRowAction.Change,
				DataRowAction.Add,
				DataRowAction.Change,
				DataRowAction.Add};
			rowActionInit (dra);
			dtLoad.Load (dtr, LoadOption.Upsert);
			rowActionEnd ();
			// asserting Unchanged Row0
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1, DataRowVersion.Current], "RowData0-C");
			Assert.AreEqual ("RowState 1", dtLoad.Rows[0][1, DataRowVersion.Original], "RowData0-O");
			Assert.AreEqual (DataRowState.Modified, dtLoad.Rows[0].RowState, "RowState0");
			// asserting Modified Row1
			Assert.AreEqual ("mono 2", dtLoad.Rows[1][1, DataRowVersion.Current], "RowData1-C");
			Assert.AreEqual ("RowState 2", dtLoad.Rows[1][1, DataRowVersion.Original], "RowData1-O");
			Assert.AreEqual (DataRowState.Modified, dtLoad.Rows[1].RowState, "RowState1");
			// asserting Deleted Row2 and "Deleted-Added" Row4
			Assert.AreEqual ("RowState 3", dtLoad.Rows[2][1, DataRowVersion.Original], "RowData2-O");
			Assert.AreEqual (DataRowState.Deleted, dtLoad.Rows[2].RowState, "RowState2");
			Assert.AreEqual ("mono 3", dtLoad.Rows[4][1, DataRowVersion.Current], "RowData4-C");
			Assert.AreEqual (DataRowState.Added, dtLoad.Rows[4].RowState, "RowState4");
			// asserting Added Row3
			Assert.AreEqual ("mono 4", dtLoad.Rows[3][1, DataRowVersion.Current], "RowData3-C");
			Assert.AreEqual (DataRowState.Added, dtLoad.Rows[3].RowState, "RowState3");
			// asserting Unpresent Row5
			// Notice row4 is used for added row of deleted row2 and so
			// unpresent row4 moves to row5
			Assert.AreEqual ("mono 5", dtLoad.Rows[5][1, DataRowVersion.Current], "RowData5-C");
			Assert.AreEqual (DataRowState.Added, dtLoad.Rows[5].RowState, "RowState5");
		}

		[Test]
		public void Load_RowStateUpsertDuplicateKey1 ()
		{
			localSetup ();
			dt.Rows.Add (new object[] { 4, "mono 4" });
			DataTable dtLoad = new DataTable ("LoadRowStateChanges");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "RowState 1" });
			dtLoad.Rows.Add (new object[] { 2, "RowState 2" });
			dtLoad.Rows.Add (new object[] { 3, "RowState 3" });
			dtLoad.AcceptChanges ();
			dtLoad.Rows[2].Delete ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr, LoadOption.Upsert);
			dtLoad.Rows[3][1] = "NEWVAL";
			Assert.AreEqual (DataRowState.Deleted, dtLoad.Rows[2].RowState, "A-RowState2");
			Assert.AreEqual (3, dtLoad.Rows[2][0, DataRowVersion.Original], "A-RowData2-id");
			Assert.AreEqual ("RowState 3", dtLoad.Rows[2][1, DataRowVersion.Original], "A-RowData2-name");
			Assert.AreEqual (DataRowState.Added, dtLoad.Rows[3].RowState, "A-RowState3");
			Assert.AreEqual (3, dtLoad.Rows[3][0, DataRowVersion.Current], "A-RowData3-id");
			Assert.AreEqual ("NEWVAL", dtLoad.Rows[3][1, DataRowVersion.Current], "A-RowData3-name");
			Assert.AreEqual (DataRowState.Added, dtLoad.Rows[4].RowState, "A-RowState4");
			Assert.AreEqual (4, dtLoad.Rows[4][0, DataRowVersion.Current], "A-RowData4-id");
			Assert.AreEqual ("mono 4", dtLoad.Rows[4][1, DataRowVersion.Current], "A-RowData4-name");

			dtLoad.AcceptChanges ();

			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[2].RowState, "B-RowState2");
			Assert.AreEqual (3, dtLoad.Rows[2][0, DataRowVersion.Current], "B-RowData2-id");
			Assert.AreEqual ("NEWVAL", dtLoad.Rows[2][1, DataRowVersion.Current], "B-RowData2-name");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[3].RowState, "B-RowState3");
			Assert.AreEqual (4, dtLoad.Rows[3][0, DataRowVersion.Current], "B-RowData3-id");
			Assert.AreEqual ("mono 4", dtLoad.Rows[3][1, DataRowVersion.Current], "B-RowData3-name");
		}

		[Test]
		public void Load_RowStateUpsertDuplicateKey2 ()
		{
			localSetup ();
			dt.Rows.Add (new object[] { 4, "mono 4" });
			DataTable dtLoad = new DataTable ("LoadRowStateChanges");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "RowState 1" });
			dtLoad.Rows.Add (new object[] { 2, "RowState 2" });
			dtLoad.Rows.Add (new object[] { 3, "RowState 3" });
			dtLoad.AcceptChanges ();
			dtLoad.Rows[2].Delete ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr, LoadOption.Upsert);
			dtLoad.AcceptChanges ();

			try {
				Assert.AreEqual (" ", dtLoad.Rows[4][1], "RowData4");
				Assert.Fail ("#1");
			} catch (IndexOutOfRangeException) {
			}
		}

		[Test]
		public void Load_RowStateUpsertDelete1 ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadRowStateChanges");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "RowState 1" });
			dtLoad.Rows.Add (new object[] { 2, "RowState 2" });
			dtLoad.Rows.Add (new object[] { 3, "RowState 3" });
			dtLoad.AcceptChanges ();
			dtLoad.Rows[2].Delete ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr, LoadOption.Upsert);

			try {
				Assert.AreEqual (" ", dtLoad.Rows[2][1, DataRowVersion.Current], "RowData2-C");
				Assert.Fail ("#1");
			} catch (VersionNotFoundException) {
			}
		}

		[Test]
		public void Load_RowStateUpsertDelete2 ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadRowStateChanges");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "RowState 1" });
			dtLoad.Rows.Add (new object[] { 2, "RowState 2" });
			dtLoad.Rows.Add (new object[] { 3, "RowState 3" });
			dtLoad.AcceptChanges ();
			dtLoad.Rows[2].Delete ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr, LoadOption.Upsert);

			try {
				Assert.AreEqual (" ", dtLoad.Rows[3][1, DataRowVersion.Original], "RowData3-O");
				Assert.Fail ("#1");
			} catch (VersionNotFoundException) {
			}
		}

		[Test]
		public void Load_RowStateUpsertAdd ()
		{
			localSetup ();
			dt.Rows.Add (new object[] { 4, "mono 4" });
			DataTable dtLoad = new DataTable ("LoadRowStateChanges");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "RowState 1" });
			dtLoad.Rows.Add (new object[] { 2, "RowState 2" });
			dtLoad.Rows.Add (new object[] { 3, "RowState 3" });
			dtLoad.AcceptChanges ();
			DataRow row = dtLoad.NewRow ();
			row["id"] = 4;
			row["name"] = "Add 4";
			dtLoad.Rows.Add (row);
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr, LoadOption.Upsert);

			try {
				Assert.AreEqual (" ", dtLoad.Rows[3][1, DataRowVersion.Original], "RowData3-O");
				Assert.Fail ("#1");
			} catch (VersionNotFoundException) {
			}
		}

		[Test]
		public void Load_RowStateUpsertUnpresent () {
			localSetup ();
			dt.Rows.Add (new object[] { 4, "mono 4" });
			DataTable dtLoad = new DataTable ("LoadRowStateChanges");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "RowState 1" });
			dtLoad.Rows.Add (new object[] { 2, "RowState 2" });
			dtLoad.Rows.Add (new object[] { 3, "RowState 3" });
			dtLoad.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr, LoadOption.Upsert);

			try {
				Assert.AreEqual (" ", dtLoad.Rows[3][1, DataRowVersion.Original], "RowData3-O");
				Assert.Fail ("#1");
			} catch (VersionNotFoundException) {
			}
		}

		[Test]
		public void Load_RowStateUpsertUnchangedEqualVal ()
		{
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadRowStateChanges");
			dtLoad.Columns.Add ("id", typeof (int));
			dtLoad.Columns.Add ("name", typeof (string));
			dtLoad.PrimaryKey = new DataColumn[] { dtLoad.Columns["id"] };
			dtLoad.Rows.Add (new object[] { 1, "mono 1" });
			dtLoad.AcceptChanges ();
			DataTableReader dtr = dt.CreateDataReader ();
			DataRowAction[] dra = new DataRowAction[] {
				DataRowAction.Nothing,// REAL action
				DataRowAction.Nothing,// dummy  
				DataRowAction.Nothing,// dummy  
				DataRowAction.Nothing,// dummy  
				DataRowAction.Nothing};// dummy  
			rowActionInit (dra);
			dtLoad.Load (dtr, LoadOption.Upsert);
			rowActionEnd ();
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1, DataRowVersion.Current], "RowData0-C");
			Assert.AreEqual ("mono 1", dtLoad.Rows[0][1, DataRowVersion.Original], "RowData0-O");
			Assert.AreEqual (DataRowState.Unchanged, dtLoad.Rows[0].RowState, "RowState0");
		}

		[Test]
		public void LoadDataRow_LoadOptions ()
		{
			// LoadDataRow is covered in detail (without LoadOptions) in DataTableTest2
			// LoadOption tests are covered in detail in DataTable.Load().
			// Therefore only minimal tests of LoadDataRow with LoadOptions are covered here.
			DataTable dt;
			DataRow dr;
			dt = CreateDataTableExample ();
			dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };	//add ParentId as Primary Key
			dt.Columns["String1"].DefaultValue = "Default";

			dr = dt.Select ("ParentId=1")[0];

			//Update existing row with LoadOptions = OverwriteChanges
			dt.BeginLoadData ();
			dt.LoadDataRow (new object[] { 1, null, "Changed" },
				LoadOption.OverwriteChanges);
			dt.EndLoadData ();

			// LoadDataRow(update1) - check column String2
			Assert.AreEqual ("Changed", dr["String2", DataRowVersion.Current], "DT72-C");
			Assert.AreEqual ("Changed", dr["String2", DataRowVersion.Original], "DT72-O");

			// LoadDataRow(update1) - check row state
			Assert.AreEqual (DataRowState.Unchanged, dr.RowState, "DT73-LO");

			//Add New row with LoadOptions = Upsert
			dt.BeginLoadData ();
			dt.LoadDataRow (new object[] { 99, null, "Changed" },
				LoadOption.Upsert);
			dt.EndLoadData ();

			// LoadDataRow(insert1) - check column String2
			dr = dt.Select ("ParentId=99")[0];
			Assert.AreEqual ("Changed", dr["String2", DataRowVersion.Current], "DT75-C");

			// LoadDataRow(insert1) - check row state
			Assert.AreEqual (DataRowState.Added, dr.RowState, "DT76-LO");
		}

		public static DataTable CreateDataTableExample ()
		{
			DataTable dtParent = new DataTable ("Parent");

			dtParent.Columns.Add ("ParentId", typeof (int));
			dtParent.Columns.Add ("String1", typeof (string));
			dtParent.Columns.Add ("String2", typeof (string));

			dtParent.Columns.Add ("ParentDateTime", typeof (DateTime));
			dtParent.Columns.Add ("ParentDouble", typeof (double));
			dtParent.Columns.Add ("ParentBool", typeof (bool));

			dtParent.Rows.Add (new object[] { 1, "1-String1", "1-String2", new DateTime (2005, 1, 1, 0, 0, 0, 0), 1.534, true });
			dtParent.Rows.Add (new object[] { 2, "2-String1", "2-String2", new DateTime (2004, 1, 1, 0, 0, 0, 1), -1.534, true });
			dtParent.Rows.Add (new object[] { 3, "3-String1", "3-String2", new DateTime (2003, 1, 1, 0, 0, 1, 0), double.MinValue * 10000, false });
			dtParent.Rows.Add (new object[] { 4, "4-String1", "4-String2", new DateTime (2002, 1, 1, 0, 1, 0, 0), double.MaxValue / 10000, true });
			dtParent.Rows.Add (new object[] { 5, "5-String1", "5-String2", new DateTime (2001, 1, 1, 1, 0, 0, 0), 0.755, true });
			dtParent.Rows.Add (new object[] { 6, "6-String1", "6-String2", new DateTime (2000, 1, 1, 0, 0, 0, 0), 0.001, false });
			dtParent.AcceptChanges ();
			return dtParent;
		}

		#endregion // DataTable.Load Tests

		#region Read/Write XML Tests

		[Test]
		public void ReadXmlSchema ()
		{
			DataTable Table = new DataTable ();
			Table.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/own_schema1.xsd"));

			Assert.AreEqual ("test_table", Table.TableName, "test#02");
			Assert.AreEqual ("", Table.Namespace, "test#03");
			Assert.AreEqual (2, Table.Columns.Count, "test#04");
			Assert.AreEqual (0, Table.Rows.Count, "test#05");
			Assert.IsFalse (Table.CaseSensitive, "test#06");
			Assert.AreEqual (1, Table.Constraints.Count, "test#07");
			Assert.AreEqual ("", Table.Prefix, "test#08");

			Constraint cons = Table.Constraints[0];
			Assert.AreEqual ("Constraint1", cons.ConstraintName.ToString (), "test#09");
			Assert.AreEqual ("Constraint1", cons.ToString (), "test#10");

			DataColumn column = Table.Columns[0];
			Assert.IsTrue (column.AllowDBNull, "test#11");
			Assert.IsFalse (column.AutoIncrement, "test#12");
			Assert.AreEqual (0L, column.AutoIncrementSeed, "test#13");
			Assert.AreEqual (1L, column.AutoIncrementStep, "test#14");
			Assert.AreEqual ("test", column.Caption, "test#15");
			Assert.AreEqual ("Element", column.ColumnMapping.ToString (), "test#16");
			Assert.AreEqual ("first", column.ColumnName, "test#17");
			Assert.AreEqual (typeof (string), column.DataType, "test#18");
			Assert.AreEqual ("test_default_value", column.DefaultValue.ToString (), "test#19");
			Assert.IsFalse (column.DesignMode, "test#20");
			Assert.AreEqual ("", column.Expression, "test#21");
			Assert.AreEqual (100, column.MaxLength, "test#22");
			Assert.AreEqual ("", column.Namespace, "test#23");
			Assert.AreEqual (0, column.Ordinal, "test#24");
			Assert.AreEqual ("", column.Prefix, "test#25");
			Assert.IsFalse (column.ReadOnly, "test#26");
			Assert.IsTrue (column.Unique, "test#27");

			DataColumn column2 = Table.Columns[1];
			Assert.IsTrue (column2.AllowDBNull, "test#28");
			Assert.IsFalse (column2.AutoIncrement, "test#29");
			Assert.AreEqual (0L, column2.AutoIncrementSeed, "test#30");
			Assert.AreEqual (1L, column2.AutoIncrementStep, "test#31");
			Assert.AreEqual ("second", column2.Caption, "test#32");
			Assert.AreEqual ("Element", column2.ColumnMapping.ToString (), "test#33");
			Assert.AreEqual ("second", column2.ColumnName, "test#34");
			Assert.AreEqual (typeof (SqlGuid), column2.DataType, "test#35");
			Assert.AreEqual (SqlGuid.Null, column2.DefaultValue, "test#36");
			Assert.AreEqual (typeof (SqlGuid), column2.DefaultValue.GetType (), "test#36-2");
			Assert.IsFalse (column2.DesignMode, "test#37");
			Assert.AreEqual ("", column2.Expression, "test#38");
			Assert.AreEqual (-1, column2.MaxLength, "test#39");
			Assert.AreEqual ("", column2.Namespace, "test#40");
			Assert.AreEqual (1, column2.Ordinal, "test#41");
			Assert.AreEqual ("", column2.Prefix, "test#42");
			Assert.IsFalse (column2.ReadOnly, "test#43");
			Assert.IsFalse (column2.Unique, "test#44");

			DataTable Table2 = new DataTable ();
			Table2.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/own_schema2.xsd"));

			Assert.AreEqual ("second_test_table", Table2.TableName, "test#45");
			Assert.AreEqual ("", Table2.Namespace, "test#46");
			Assert.AreEqual (1, Table2.Columns.Count, "test#47");
			Assert.AreEqual (0, Table2.Rows.Count, "test#48");
			Assert.IsFalse (Table2.CaseSensitive, "test#49");
			Assert.AreEqual (1, Table2.Constraints.Count, "test#50");
			Assert.AreEqual ("", Table2.Prefix, "test#51");

			DataColumn column3 = Table2.Columns[0];
			Assert.IsTrue (column3.AllowDBNull, "test#52");
			Assert.IsFalse (column3.AutoIncrement, "test#53");
			Assert.AreEqual (0L, column3.AutoIncrementSeed, "test#54");
			Assert.AreEqual (1L, column3.AutoIncrementStep, "test#55");
			Assert.AreEqual ("second_first", column3.Caption, "test#56");
			Assert.AreEqual ("Element", column3.ColumnMapping.ToString (), "test#57");
			Assert.AreEqual ("second_first", column3.ColumnName, "test#58");
			Assert.AreEqual (typeof (string), column3.DataType, "test#59");
			Assert.AreEqual ("default_value", column3.DefaultValue.ToString (), "test#60");
			Assert.IsFalse (column3.DesignMode, "test#61");
			Assert.AreEqual ("", column3.Expression, "test#62");
			Assert.AreEqual (100, column3.MaxLength, "test#63");
			Assert.AreEqual ("", column3.Namespace, "test#64");
			Assert.AreEqual (0, column3.Ordinal, "test#65");
			Assert.AreEqual ("", column3.Prefix, "test#66");
			Assert.IsFalse (column3.ReadOnly, "test#67");
			Assert.IsTrue (column3.Unique, "test#68");
		}

		[Test]
		public void ReadXmlSchema_2 ()
		{
			DataTable dt = new DataTable ();
			string xmlData = string.Empty;
			xmlData += "<?xml version=\"1.0\"?>";
			xmlData += "<xs:schema id=\"SiteConfiguration\" targetNamespace=\"http://tempuri.org/PortalCfg.xsd\" xmlns:mstns=\"http://tempuri.org/PortalCfg.xsd\" xmlns=\"http://tempuri.org/PortalCfg.xsd\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" attributeFormDefault=\"qualified\" elementFormDefault=\"qualified\">";
			xmlData += "<xs:element name=\"SiteConfiguration\" msdata:IsDataSet=\"true\" msdata:EnforceConstraints=\"False\">";
			xmlData += "<xs:complexType>";
			xmlData += "<xs:choice  minOccurs=\"0\" maxOccurs=\"unbounded\">";
			xmlData += "<xs:element name=\"Tab\">";
			xmlData += "<xs:complexType>";
			xmlData += "<xs:sequence>";
			xmlData += "<xs:element name=\"Module\" minOccurs=\"0\" maxOccurs=\"unbounded\">";
			xmlData += "<xs:complexType>";
			xmlData += "<xs:attribute name=\"ModuleId\" form=\"unqualified\" type=\"xs:int\" />";
			xmlData += "</xs:complexType>";
			xmlData += "</xs:element>";
			xmlData += "</xs:sequence>";
			xmlData += "<xs:attribute name=\"TabId\" form=\"unqualified\" type=\"xs:int\" />";
			xmlData += "</xs:complexType>";
			xmlData += "</xs:element>";
			xmlData += "</xs:choice>";
			xmlData += "</xs:complexType>";
			xmlData += "<xs:key name=\"TabKey\" msdata:PrimaryKey=\"true\">";
			xmlData += "<xs:selector xpath=\".//mstns:Tab\" />";
			xmlData += "<xs:field xpath=\"@TabId\" />";
			xmlData += "</xs:key>";
			xmlData += "<xs:key name=\"ModuleKey\" msdata:PrimaryKey=\"true\">";
			xmlData += "<xs:selector xpath=\".//mstns:Module\" />";
			xmlData += "<xs:field xpath=\"@ModuleID\" />";
			xmlData += "</xs:key>";
			xmlData += "</xs:element>";
			xmlData += "</xs:schema>";
			dt.ReadXmlSchema (new StringReader (xmlData));
		}

		[Test]
		public void ReadXmlSchema_ByStream ()
		{
			DataSet ds1 = new DataSet ();
			ds1.Tables.Add (DataProvider.CreateParentDataTable ());
			ds1.Tables.Add (DataProvider.CreateChildDataTable ());

			MemoryStream ms1 = new MemoryStream ();
			MemoryStream ms2 = new MemoryStream ();
			//write xml  schema only
			//ds1.WriteXmlSchema (ms);
			ds1.Tables[0].WriteXmlSchema (ms1);
			ds1.Tables[1].WriteXmlSchema (ms2);

			MemoryStream ms11 = new MemoryStream (ms1.GetBuffer ());
			MemoryStream ms22 = new MemoryStream (ms2.GetBuffer ());
			//copy schema
			//DataSet ds2 = new DataSet ();
			DataTable dt1 = new DataTable ();
			DataTable dt2 = new DataTable ();

			//ds2.ReadXmlSchema (ms1);
			dt1.ReadXmlSchema (ms11);
			dt2.ReadXmlSchema (ms22);

			//check xml schema
			// ReadXmlSchema - Tables count
			//Assert.AreEqual (ds2.Tables.Count, ds1.Tables.Count, "DS269");

			// ReadXmlSchema - Tables 0 Col count
			Assert.AreEqual (ds1.Tables[0].Columns.Count, dt1.Columns.Count, "DS270");

			// ReadXmlSchema - Tables 1 Col count
			Assert.AreEqual (ds1.Tables[1].Columns.Count, dt2.Columns.Count, "DS271");

			//check some colummns types
			// ReadXmlSchema - Tables 0 Col type
			Assert.AreEqual (ds1.Tables[0].Columns[0].GetType (), dt1.Columns[0].GetType (), "DS272");

			// ReadXmlSchema - Tables 1 Col type
			Assert.AreEqual (ds1.Tables[1].Columns[3].GetType (), dt2.Columns[3].GetType (), "DS273");

			//check that no data exists
			// ReadXmlSchema - Table 1 row count
			Assert.AreEqual (0, dt1.Rows.Count, "DS274");

			// ReadXmlSchema - Table 2 row count
			Assert.AreEqual (0, dt2.Rows.Count, "DS275");
		}

		[Test]
		public void ReadWriteXmlSchema_ByFileName ()
		{
			string sTempFileName1 = Path.Combine (Path.GetTempPath (), "tmpDataSet_ReadWriteXml_43899-1.xml");
			string sTempFileName2 = Path.Combine (Path.GetTempPath (), "tmpDataSet_ReadWriteXml_43899-2.xml");

			DataSet ds1 = new DataSet ();
			ds1.Tables.Add (DataProvider.CreateParentDataTable ());
			ds1.Tables.Add (DataProvider.CreateChildDataTable ());

			ds1.Tables[0].WriteXmlSchema (sTempFileName1);
			ds1.Tables[1].WriteXmlSchema (sTempFileName2);

			DataTable dt1 = new DataTable ();
			DataTable dt2 = new DataTable ();

			dt1.ReadXmlSchema (sTempFileName1);
			dt2.ReadXmlSchema (sTempFileName2);

			Assert.AreEqual (ds1.Tables[0].Columns.Count, dt1.Columns.Count, "DS277");
			Assert.AreEqual (ds1.Tables[1].Columns.Count, dt2.Columns.Count, "DS278");
			Assert.AreEqual (ds1.Tables[0].Columns[0].GetType (), dt1.Columns[0].GetType (), "DS279");
			Assert.AreEqual (ds1.Tables[1].Columns[3].GetType (), dt2.Columns[3].GetType (), "DS280");
			Assert.AreEqual (0, dt1.Rows.Count, "DS281");
			Assert.AreEqual (0, dt2.Rows.Count, "DS282");

			File.Delete (sTempFileName1);
			File.Delete (sTempFileName2);
		}

		[Test]
		public void ReadXmlSchema_ByTextReader ()
		{
			DataSet ds1 = new DataSet ();
			ds1.Tables.Add (DataProvider.CreateParentDataTable ());
			ds1.Tables.Add (DataProvider.CreateChildDataTable ());

			StringWriter sw1 = new StringWriter ();
			StringWriter sw2 = new StringWriter ();
			//write xml file, schema only
			//ds1.WriteXmlSchema (sw);
			ds1.Tables[0].WriteXmlSchema (sw1);
			ds1.Tables[1].WriteXmlSchema (sw2);

			StringReader sr1 = new StringReader (sw1.GetStringBuilder ().ToString ());
			StringReader sr2 = new StringReader (sw2.GetStringBuilder ().ToString ());
			//copy both data and schema
			//DataSet ds2 = new DataSet ();
			DataTable dt1 = new DataTable ();
			DataTable dt2 = new DataTable ();

			//ds2.ReadXmlSchema (sr);
			dt1.ReadXmlSchema (sr1);
			dt2.ReadXmlSchema (sr2);

			//check xml schema
			// ReadXmlSchema - Tables count
			//Assert.AreEqual (ds2.Tables.Count, ds1.Tables.Count, "DS283");

			// ReadXmlSchema - Tables 0 Col count
			Assert.AreEqual (ds1.Tables[0].Columns.Count, dt1.Columns.Count, "DS284");

			// ReadXmlSchema - Tables 1 Col count
			Assert.AreEqual (ds1.Tables[1].Columns.Count, dt2.Columns.Count, "DS285");

			//check some colummns types
			// ReadXmlSchema - Tables 0 Col type
			Assert.AreEqual (ds1.Tables[0].Columns[0].GetType (), dt1.Columns[0].GetType (), "DS286");

			// ReadXmlSchema - Tables 1 Col type
			Assert.AreEqual (ds1.Tables[1].Columns[3].GetType (), dt2.Columns[3].GetType (), "DS287");

			//check that no data exists
			// ReadXmlSchema - Table 1 row count
			Assert.AreEqual (0, dt1.Rows.Count, "DS288");

			// ReadXmlSchema - Table 2 row count
			Assert.AreEqual (0, dt2.Rows.Count, "DS289");
		}

		[Test]
		public void ReadXmlSchema_ByXmlReader ()
		{
			DataSet ds1 = new DataSet ();
			ds1.Tables.Add (DataProvider.CreateParentDataTable ());
			ds1.Tables.Add (DataProvider.CreateChildDataTable ());

			StringWriter sw1 = new StringWriter ();
			XmlTextWriter xmlTW1 = new XmlTextWriter (sw1);
			StringWriter sw2 = new StringWriter ();
			XmlTextWriter xmlTW2 = new XmlTextWriter (sw2);

			//write xml file, schema only
			ds1.Tables[0].WriteXmlSchema (xmlTW1);
			xmlTW1.Flush ();
			ds1.Tables[1].WriteXmlSchema (xmlTW2);
			xmlTW2.Flush ();

			StringReader sr1 = new StringReader (sw1.ToString ());
			XmlTextReader xmlTR1 = new XmlTextReader (sr1);
			StringReader sr2 = new StringReader (sw2.ToString ());
			XmlTextReader xmlTR2 = new XmlTextReader (sr2);

			//copy both data and schema
			//DataSet ds2 = new DataSet ();
			DataTable dt1 = new DataTable ();
			DataTable dt2 = new DataTable ();

			//ds2.ReadXmlSchema (xmlTR);
			dt1.ReadXmlSchema (xmlTR1);
			dt2.ReadXmlSchema (xmlTR2);

			//check xml schema
			// ReadXmlSchema - Tables count
			//Assert.AreEqual (ds2.Tables.Count, ds1.Tables.Count, "DS290");

			// ReadXmlSchema - Tables 0 Col count
			Assert.AreEqual (ds1.Tables[0].Columns.Count, dt1.Columns.Count, "DS291");

			// ReadXmlSchema - Tables 1 Col count
			Assert.AreEqual (ds1.Tables[1].Columns.Count, dt2.Columns.Count, "DS292");

			//check some colummns types
			// ReadXmlSchema - Tables 0 Col type
			Assert.AreEqual (ds1.Tables[0].Columns[0].GetType (), dt1.Columns[0].GetType (), "DS293");

			// ReadXmlSchema - Tables 1 Col type
			Assert.AreEqual (ds1.Tables[1].Columns[3].GetType (), dt2.Columns[3].GetType (), "DS294");

			//check that no data exists
			// ReadXmlSchema - Table 1 row count
			Assert.AreEqual (0, dt1.Rows.Count, "DS295");

			// ReadXmlSchema - Table 2 row count
			Assert.AreEqual (0, dt2.Rows.Count, "DS296");
		}

		[Test]
		[SetCulture ("en-GB")]
		public void WriteXmlSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/region.xml"));
			TextWriter writer = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (writer);

			string TextString = GetNormalizedSchema (writer.ToString ());
			//string TextString = writer.ToString ();

			string substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring, "test#01");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("<xs:schema id=\"Root\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring, "test#02");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			// Looks like whoever added this test depended on English culture, which is wrong.
			Assert.AreEqual ("  <xs:element msdata:IsDataSet=\"true\" msdata:Locale=\"en-US\" msdata:MainDataTable=\"Region\" name=\"Root\">", substring, "test#03");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:complexType>", substring, "test#04");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:choice maxOccurs=\"unbounded\" minOccurs=\"0\">", substring, "test#05");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element name=\"Region\">", substring, "test#06");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("          <xs:complexType>", substring, "test#07");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("            <xs:sequence>", substring, "test#08");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element minOccurs=\"0\" name=\"RegionID\" type=\"xs:string\" />", substring, "test#09");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("              <xs:element minOccurs=\"0\" name=\"RegionDescription\" type=\"xs:string\" />", substring, "test#10");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("            </xs:sequence>", substring, "test#11");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("          </xs:complexType>", substring, "test#12");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("        </xs:element>", substring, "test#13");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("      </xs:choice>", substring, "test#14");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:complexType>", substring, "test#15");

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:element>", substring, "test#16");

			Assert.AreEqual ("</xs:schema>", TextString, "test#17");
		}

		[Test]
		public void WriteXmlSchema2 ()
		{
			string xml = @"<myDataSet xmlns='NetFrameWork'><myTable><id>0</id><item>item 0</item></myTable><myTable><id>1</id><item>item 1</item></myTable><myTable><id>2</id><item>item 2</item></myTable><myTable><id>3</id><item>item 3</item></myTable><myTable><id>4</id><item>item 4</item></myTable><myTable><id>5</id><item>item 5</item></myTable><myTable><id>6</id><item>item 6</item></myTable><myTable><id>7</id><item>item 7</item></myTable><myTable><id>8</id><item>item 8</item></myTable><myTable><id>9</id><item>item 9</item></myTable></myDataSet>";
			string schema = @"<?xml version='1.0' encoding='utf-16'?>
<xs:schema id='myDataSet' targetNamespace='NetFrameWork' xmlns:mstns='NetFrameWork' xmlns='NetFrameWork' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' attributeFormDefault='qualified' elementFormDefault='qualified'>
  <xs:element name='myDataSet' msdata:IsDataSet='true' msdata:MainDataTable='NetFrameWork_x003A_myTable' msdata:UseCurrentLocale='true'>
    <xs:complexType>
      <xs:choice minOccurs='0' maxOccurs='unbounded'>
        <xs:element name='myTable'>
          <xs:complexType>
            <xs:sequence>
              <xs:element name='id' msdata:AutoIncrement='true' type='xs:int' minOccurs='0' />
              <xs:element name='item' type='xs:string' minOccurs='0' />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			DataSet OriginalDataSet = new DataSet ("myDataSet");
			OriginalDataSet.Namespace = "NetFrameWork";
			DataTable myTable = new DataTable ("myTable");
			DataColumn c1 = new DataColumn ("id", typeof (int));
			c1.AutoIncrement = true;
			DataColumn c2 = new DataColumn ("item");
			myTable.Columns.Add (c1);
			myTable.Columns.Add (c2);
			OriginalDataSet.Tables.Add (myTable);
			// Add ten rows.
			DataRow newRow;
			for (int i = 0; i < 10; i++) {
				newRow = myTable.NewRow ();
				newRow["item"] = "item " + i;
				myTable.Rows.Add (newRow);
			}
			OriginalDataSet.AcceptChanges ();

			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			OriginalDataSet.WriteXml (xtw);
			string result = sw.ToString ();

			Assert.AreEqual (xml, result);

			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			xtw.Formatting = Formatting.Indented;
			OriginalDataSet.Tables[0].WriteXmlSchema (xtw);
			result = sw.ToString ();

			result = result.Replace ("\r\n", "\n").Replace ('"', '\'');
			Assert.AreEqual (schema.Replace ("\r\n", "\n"), result);
		}

		[Test]
		public void WriteXmlSchema3 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""ExampleDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""ExampleDataSet"" msdata:IsDataSet=""true"" msdata:MainDataTable=""ExampleDataTable"" msdata:UseCurrentLocale=""true"">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:element name=""ExampleDataTable"">
          <xs:complexType>
            <xs:attribute name=""PrimaryKeyColumn"" type=""xs:int"" use=""required"" />
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
    <xs:unique name=""PK_ExampleDataTable"" msdata:PrimaryKey=""true"">
      <xs:selector xpath="".//ExampleDataTable"" />
      <xs:field xpath=""@PrimaryKeyColumn"" />
    </xs:unique>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet ("ExampleDataSet");

			ds.Tables.Add (new DataTable ("ExampleDataTable"));
			ds.Tables["ExampleDataTable"].Columns.Add (
				new DataColumn ("PrimaryKeyColumn", typeof (int), "", MappingType.Attribute));
			ds.Tables["ExampleDataTable"].Columns["PrimaryKeyColumn"].AllowDBNull = false;

			ds.Tables["ExampleDataTable"].Constraints.Add (
				"PK_ExampleDataTable",
				ds.Tables["ExampleDataTable"].Columns["PrimaryKeyColumn"],
				true);

			ds.AcceptChanges ();
			StringWriter sw = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (sw);

			string result = sw.ToString ();

			Assert.AreEqual (xmlschema.Replace ("\r\n", "\n"), result.Replace ("\r\n", "\n"));
			//Assert.AreEqual (xmlschema, result.Replace ("\r\n", "\n"));
		}

		[Test]
		public void WriteXmlSchema4 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:MainDataTable=""MyType"" msdata:UseCurrentLocale=""true"">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:element name=""MyType"">
          <xs:complexType>
            <xs:attribute name=""ID"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Desc"" type=""xs:string"" />
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet ("Example");

			// Add MyType DataTable
			DataTable dt = new DataTable ("MyType");
			ds.Tables.Add (dt);

			dt.Columns.Add (new DataColumn ("ID", typeof (int), "",
				MappingType.Attribute));
			dt.Columns["ID"].AllowDBNull = false;

			dt.Columns.Add (new DataColumn ("Desc", typeof
				(string), "", MappingType.Attribute));

			ds.AcceptChanges ();

			StringWriter sw = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (sw);

			string result = sw.ToString ();

			Assert.AreEqual (xmlschema.Replace ("\r\n", "\n"), result.Replace ("\r\n", "\n"));
		}

		[Test]
		public void WriteXmlSchema5 ()
		{
			string xmlschema1 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:MainDataTable=""StandAlone"" msdata:UseCurrentLocale=""true"">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:element name=""StandAlone"">
          <xs:complexType>
            <xs:attribute name=""ID"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Desc"" type=""xs:string"" use=""required"" />
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			string xmlschema2 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:MainDataTable=""Dimension"" msdata:UseCurrentLocale=""true"">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:element name=""Dimension"">
          <xs:complexType>
            <xs:attribute name=""Number"" msdata:ReadOnly=""true"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Title"" type=""xs:string"" use=""required"" />
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
    <xs:unique name=""PK_Dimension"" msdata:PrimaryKey=""true"">
      <xs:selector xpath="".//Dimension"" />
      <xs:field xpath=""@Number"" />
    </xs:unique>
  </xs:element>
</xs:schema>";
			string xmlschema3 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:MainDataTable=""Element"" msdata:UseCurrentLocale=""true"">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:element name=""Element"">
          <xs:complexType>
            <xs:attribute name=""Dimension"" msdata:ReadOnly=""true"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Number"" msdata:ReadOnly=""true"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Title"" type=""xs:string"" use=""required"" />
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
    <xs:unique name=""PK_Element"" msdata:PrimaryKey=""true"">
      <xs:selector xpath="".//Element"" />
      <xs:field xpath=""@Dimension"" />
      <xs:field xpath=""@Number"" />
    </xs:unique>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet ("Example");

			// Add a DataTable with no ReadOnly columns
			DataTable dt1 = new DataTable ("StandAlone");
			ds.Tables.Add (dt1);

			// Add a ReadOnly column
			dt1.Columns.Add (new DataColumn ("ID", typeof (int), "",
				MappingType.Attribute));
			dt1.Columns["ID"].AllowDBNull = false;

			dt1.Columns.Add (new DataColumn ("Desc", typeof
				(string), "", MappingType.Attribute));
			dt1.Columns["Desc"].AllowDBNull = false;

			// Add related DataTables with ReadOnly columns
			DataTable dt2 = new DataTable ("Dimension");
			ds.Tables.Add (dt2);
			dt2.Columns.Add (new DataColumn ("Number", typeof
				(int), "", MappingType.Attribute));
			dt2.Columns["Number"].AllowDBNull = false;
			dt2.Columns["Number"].ReadOnly = true;

			dt2.Columns.Add (new DataColumn ("Title", typeof
				(string), "", MappingType.Attribute));
			dt2.Columns["Title"].AllowDBNull = false;

			dt2.Constraints.Add ("PK_Dimension", dt2.Columns["Number"], true);

			DataTable dt3 = new DataTable ("Element");
			ds.Tables.Add (dt3);

			dt3.Columns.Add (new DataColumn ("Dimension", typeof
				(int), "", MappingType.Attribute));
			dt3.Columns["Dimension"].AllowDBNull = false;
			dt3.Columns["Dimension"].ReadOnly = true;

			dt3.Columns.Add (new DataColumn ("Number", typeof
				(int), "", MappingType.Attribute));
			dt3.Columns["Number"].AllowDBNull = false;
			dt3.Columns["Number"].ReadOnly = true;

			dt3.Columns.Add (new DataColumn ("Title", typeof
				(string), "", MappingType.Attribute));
			dt3.Columns["Title"].AllowDBNull = false;

			dt3.Constraints.Add ("PK_Element", new DataColumn[] { 
				dt3.Columns ["Dimension"],
				dt3.Columns ["Number"] }, true);

			ds.AcceptChanges ();

			StringWriter sw1 = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (sw1);
			string result1 = sw1.ToString ();
			Assert.AreEqual (xmlschema1.Replace ("\r\n", "\n"), result1.Replace ("\r\n", "\n"));

			StringWriter sw2 = new StringWriter ();
			ds.Tables[1].WriteXmlSchema (sw2);
			string result2 = sw2.ToString ();
			Assert.AreEqual (xmlschema2.Replace ("\r\n", "\n"), result2.Replace ("\r\n", "\n"));

			StringWriter sw3 = new StringWriter ();
			ds.Tables[2].WriteXmlSchema (sw3);
			string result3 = sw3.ToString ();
			Assert.AreEqual (xmlschema3.Replace ("\r\n", "\n"), result3.Replace ("\r\n", "\n"));
		}

		[Test]
		public void WriteXmlSchema6 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:MainDataTable=""MyType"" msdata:UseCurrentLocale=""true"">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
        <xs:element name=""MyType"">
          <xs:complexType>
            <xs:attribute name=""Desc"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:maxLength value=""32"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:attribute>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet ("Example");

			// Add MyType DataTable
			ds.Tables.Add ("MyType");

			ds.Tables["MyType"].Columns.Add (new DataColumn (
				"Desc", typeof (string), "", MappingType.Attribute));
			ds.Tables["MyType"].Columns["Desc"].MaxLength = 32;

			ds.AcceptChanges ();

			StringWriter sw = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (sw);

			string result = sw.ToString ();

			Assert.AreEqual (xmlschema.Replace ("\r\n", "\n"), result.Replace ("\r\n", "\n"));
		}

		[Test]
		public void WriteXmlSchema7 ()
		{
			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			ds.Tables.Add (dt);
			dt.Rows.Add (new object[] { "foo", "bar" });
			StringWriter sw = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (sw);
			Assert.IsTrue (sw.ToString ().IndexOf ("xmlns=\"\"") > 0);
		}

		[Test]
		public void WriteXmlSchema_ConstraintNameWithSpaces ()
		{
			DataSet ds = new DataSet ();
			DataTable table1 = ds.Tables.Add ("table1");
			DataTable table2 = ds.Tables.Add ("table2");

			table1.Columns.Add ("col1", typeof (int));
			table2.Columns.Add ("col1", typeof (int));

			table1.Constraints.Add ("uc 1", table1.Columns[0], false);
			table2.Constraints.Add ("fc 1", table1.Columns[0], table2.Columns[0]);

			StringWriter sw1 = new StringWriter ();
			StringWriter sw2 = new StringWriter ();

			//should not throw an exception
			ds.Tables[0].WriteXmlSchema (sw1);
			ds.Tables[1].WriteXmlSchema (sw2);
		}

		[Test]
		public void WriteXmlSchema_ForignKeyConstraint ()
		{
			DataSet ds1 = new DataSet ();

			DataTable table1 = ds1.Tables.Add ();
			DataTable table2 = ds1.Tables.Add ();

			DataColumn col1_1 = table1.Columns.Add ("col1", typeof (int));
			DataColumn col2_1 = table2.Columns.Add ("col1", typeof (int));

			table2.Constraints.Add ("fk", col1_1, col2_1);

			StringWriter sw1 = new StringWriter ();
			ds1.Tables[0].WriteXmlSchema (sw1);
			String xml1 = sw1.ToString ();
			Assert.IsTrue (xml1.IndexOf (@"<xs:unique name=""Constraint1"">") != -1, "#1");

			StringWriter sw2 = new StringWriter ();
			ds1.Tables[1].WriteXmlSchema (sw2);
			String xml2 = sw2.ToString ();
			Assert.IsTrue (xml2.IndexOf (@"<xs:unique name=""Constraint1"">") == -1, "#2");
		}

		[Test]
		public void WriteXmlSchema_Relations_ForeignKeys ()
		{
			MemoryStream ms1 = null;
			MemoryStream ms2 = null;
			MemoryStream msA = null;
			MemoryStream msB = null;

			DataSet ds1 = new DataSet ();

			DataTable table1 = ds1.Tables.Add ("Table 1");
			DataTable table2 = ds1.Tables.Add ("Table 2");

			DataColumn col1_1 = table1.Columns.Add ("col 1", typeof (int));
			DataColumn col1_2 = table1.Columns.Add ("col 2", typeof (int));
			DataColumn col1_3 = table1.Columns.Add ("col 3", typeof (int));
			DataColumn col1_4 = table1.Columns.Add ("col 4", typeof (int));
			DataColumn col1_5 = table1.Columns.Add ("col 5", typeof (int));
			DataColumn col1_6 = table1.Columns.Add ("col 6", typeof (int));
			DataColumn col1_7 = table1.Columns.Add ("col 7", typeof (int));

			DataColumn col2_1 = table2.Columns.Add ("col 1", typeof (int));
			DataColumn col2_2 = table2.Columns.Add ("col 2", typeof (int));
			DataColumn col2_3 = table2.Columns.Add ("col 3", typeof (int));
			DataColumn col2_4 = table2.Columns.Add ("col 4", typeof (int));
			DataColumn col2_5 = table2.Columns.Add ("col 5", typeof (int));
			DataColumn col2_6 = table2.Columns.Add ("col 6", typeof (int));
			DataColumn col2_7 = table2.Columns.Add ("col 7", typeof (int));

			ds1.Relations.Add ("rel 1",
				new DataColumn[] { col1_1, col1_2 },
				new DataColumn[] { col2_1, col2_2 },
				false);
			ds1.Relations.Add ("rel 2",
				new DataColumn[] { col1_3, col1_4 },
				new DataColumn[] { col2_3, col2_4 },
				true);
			table2.Constraints.Add ("fk 1",
				new DataColumn[] { col1_5, col1_6 },
				new DataColumn[] { col2_5, col2_6 });
			table1.Constraints.Add ("fk 2",
				new DataColumn[] { col2_5, col2_6 },
				new DataColumn[] { col1_5, col1_6 });

			table1.Constraints.Add ("pk 1", col1_7, true);
			table2.Constraints.Add ("pk 2", col2_7, true);

			ms1 = new MemoryStream ();
			ds1.Tables[0].WriteXmlSchema (ms1);
			ms2 = new MemoryStream ();
			ds1.Tables[1].WriteXmlSchema (ms2);

			msA = new MemoryStream (ms1.GetBuffer ());
			DataTable dtA = new DataTable ();
			dtA.ReadXmlSchema (msA);

			msB = new MemoryStream (ms2.GetBuffer ());
			DataTable dtB = new DataTable ();
			dtB.ReadXmlSchema (msB);

			Assert.AreEqual (3, dtA.Constraints.Count, "#2");
			Assert.AreEqual (2, dtB.Constraints.Count, "#3");

			Assert.IsTrue (dtA.Constraints.Contains ("pk 1"), "#5");
			Assert.IsTrue (dtA.Constraints.Contains ("Constraint1"), "#6");
			Assert.IsTrue (dtA.Constraints.Contains ("Constraint2"), "#7");
			Assert.IsTrue (dtB.Constraints.Contains ("pk 2"), "#9");
			Assert.IsTrue (dtB.Constraints.Contains ("Constraint1"), "#10");
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteXmlSchema_DifferentNamespace ()
		{
			string schema = @"<xs:schema id='NewDataSet' targetNamespace='urn:bar' xmlns:mstns='urn:bar' xmlns='urn:bar' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' attributeFormDefault='qualified' elementFormDefault='qualified' xmlns:app1='urn:baz' xmlns:app2='urn:foo' msdata:schemafragmentcount='3'>
  <xs:import namespace='urn:foo' />
  <xs:import namespace='urn:baz' />
  <xs:element name='NewDataSet' msdata:IsDataSet='true' msdata:MainDataTable='urn_x003A_foo_x003A_NS1Table' msdata:UseCurrentLocale='true'>
    <xs:complexType>
      <xs:choice minOccurs='0' maxOccurs='unbounded'>
        <xs:element ref='app2:NS1Table' />
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>
<xs:schema targetNamespace='urn:baz' xmlns:mstns='urn:bar' xmlns='urn:baz' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' attributeFormDefault='qualified' elementFormDefault='qualified' xmlns:app1='urn:baz' xmlns:app2='urn:foo'>
  <xs:import namespace='urn:foo' />
  <xs:import namespace='urn:bar' />
  <xs:element name='column2' type='xs:string' />
</xs:schema>
<xs:schema targetNamespace='urn:foo' xmlns:mstns='urn:bar' xmlns='urn:foo' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' attributeFormDefault='qualified' elementFormDefault='qualified' xmlns:app2='urn:foo' xmlns:app1='urn:baz'>
  <xs:import namespace='urn:bar' />
  <xs:import namespace='urn:baz' />
  <xs:element name='NS1Table'>
    <xs:complexType>
      <xs:sequence>
        <xs:element name='column1' type='xs:string' minOccurs='0' />
        <xs:element ref='app1:column2' minOccurs='0' />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ();
			dt.TableName = "NS1Table";
			dt.Namespace = "urn:foo";
			dt.Columns.Add ("column1");
			dt.Columns.Add ("column2");
			dt.Columns[1].Namespace = "urn:baz";
			ds.Tables.Add (dt);
			DataTable dt2 = new DataTable ();
			dt2.TableName = "NS2Table";
			dt2.Namespace = "urn:bar";
			ds.Tables.Add (dt2);
			ds.Namespace = "urn:bar";

			StringWriter sw1 = new StringWriter ();
			XmlTextWriter xw1 = new XmlTextWriter (sw1);
			xw1.Formatting = Formatting.Indented;
			xw1.QuoteChar = '\'';
			ds.Tables[0].WriteXmlSchema (xw1);
			string result1 = sw1.ToString ();
			Assert.AreEqual (schema, result1.Replace ("\r\n", "\n"), "#1");

			StringWriter sw2 = new StringWriter ();
			XmlTextWriter xw2 = new XmlTextWriter (sw2);
			xw2.Formatting = Formatting.Indented;
			xw2.QuoteChar = '\'';
			ds.Tables[0].WriteXmlSchema (xw2);
			string result2 = sw2.ToString ();
			Assert.AreEqual (schema, result2.Replace ("\r\n", "\n"), "#2");
		}

		[Test]
		public void WriteXmlSchema_Hierarchy ()
		{
			DataSet ds = new DataSet ();
			DataTable table1 = new DataTable ();
			DataColumn idColumn = table1.Columns.Add ("ID", typeof (Int32));
			table1.Columns.Add ("Name", typeof (String));
			table1.PrimaryKey = new DataColumn[] { idColumn };
			DataTable table2 = new DataTable ();
			table2.Columns.Add (new DataColumn ("OrderID", typeof (Int32)));
			table2.Columns.Add (new DataColumn ("CustomerID", typeof (Int32)));
			table2.Columns.Add (new DataColumn ("OrderDate", typeof (DateTime)));
			table2.PrimaryKey = new DataColumn[] { table2.Columns[0] };
			ds.Tables.Add (table1);
			ds.Tables.Add (table2);
			ds.Relations.Add ("CustomerOrder",
				new DataColumn[] { table1.Columns[0] },
				new DataColumn[] { table2.Columns[1] }, true);

			StringWriter writer1 = new StringWriter ();
			table1.WriteXmlSchema (writer1, false);
			string expected1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\n  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:MainDataTable=\"Table1\" msdata:UseCurrentLocale=\"true\">\n    <xs:complexType>\n      <xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">\n        <xs:element name=\"Table1\">\n          <xs:complexType>\n            <xs:sequence>\n              <xs:element name=\"ID\" type=\"xs:int\" />\n              <xs:element name=\"Name\" type=\"xs:string\" minOccurs=\"0\" />\n            </xs:sequence>\n          </xs:complexType>\n        </xs:element>\n      </xs:choice>\n    </xs:complexType>\n    <xs:unique name=\"Constraint1\" msdata:PrimaryKey=\"true\">\n      <xs:selector xpath=\".//Table1\" />\n      <xs:field xpath=\"ID\" />\n    </xs:unique>\n  </xs:element>\n</xs:schema>";
			Assert.AreEqual (expected1, writer1.ToString().Replace("\r\n", "\n"), "#1");

			StringWriter writer2 = new StringWriter ();
			table1.WriteXmlSchema (writer2, true);
			string expected2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\n  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:MainDataTable=\"Table1\" msdata:UseCurrentLocale=\"true\">\n    <xs:complexType>\n      <xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">\n        <xs:element name=\"Table1\">\n          <xs:complexType>\n            <xs:sequence>\n              <xs:element name=\"ID\" type=\"xs:int\" />\n              <xs:element name=\"Name\" type=\"xs:string\" minOccurs=\"0\" />\n            </xs:sequence>\n          </xs:complexType>\n        </xs:element>\n        <xs:element name=\"Table2\">\n          <xs:complexType>\n            <xs:sequence>\n              <xs:element name=\"OrderID\" type=\"xs:int\" />\n              <xs:element name=\"CustomerID\" type=\"xs:int\" minOccurs=\"0\" />\n              <xs:element name=\"OrderDate\" type=\"xs:dateTime\" minOccurs=\"0\" />\n            </xs:sequence>\n          </xs:complexType>\n        </xs:element>\n      </xs:choice>\n    </xs:complexType>\n    <xs:unique name=\"Constraint1\" msdata:PrimaryKey=\"true\">\n      <xs:selector xpath=\".//Table1\" />\n      <xs:field xpath=\"ID\" />\n    </xs:unique>\n    <xs:unique name=\"Table2_Constraint1\" msdata:ConstraintName=\"Constraint1\" msdata:PrimaryKey=\"true\">\n      <xs:selector xpath=\".//Table2\" />\n      <xs:field xpath=\"OrderID\" />\n    </xs:unique>\n    <xs:keyref name=\"CustomerOrder\" refer=\"Constraint1\">\n      <xs:selector xpath=\".//Table2\" />\n      <xs:field xpath=\"CustomerID\" />\n    </xs:keyref>\n  </xs:element>\n</xs:schema>";
			Assert.AreEqual (expected2, writer2.ToString ().Replace("\r\n", "\n"), "#2");
		}

		[Test]
		[Ignore ("MS behavior is far from consistent to be regarded as a reference implementation.")]
		// See the same-named tests in DataSetTest.cs
		// WriteXmlSchema doesn't have overload wityh 2 parameters in System.Data
		// and is commented-out TWICE below
		public void ReadWriteXmlSchema()
		{
			DataSet ds = new DataSet();
			ds.ReadXmlSchema(TestResourceHelper.GetFullPathOfResource ("Test/System.Data/store.xsd"));
			// check dataset properties before testing write
			AssertDataSet("ds", ds, "NewDataSet", 3, 2);
			AssertDataTable("tab1", ds.Tables[0], "bookstore", 1, 0, 0, 1, 1, 1);
			AssertDataTable("tab2", ds.Tables[1], "book", 5, 0, 1, 1, 2, 1);
			AssertDataTable("tab3", ds.Tables[2], "author", 3, 0, 1, 0, 1, 0);
			// FIXME: currently order is not compatible. Use name as index
			AssertDataRelation("rel1", ds.Relations["book_author"], "book_author", true, new string[] { "book_Id" }, new string[] { "book_Id" }, true, true);
			AssertDataRelation("rel2", ds.Relations["bookstore_book"], "bookstore_book", true, new string[] { "bookstore_Id" }, new string[] { "bookstore_Id" }, true, true);

			ds.ReadXml(TestResourceHelper.GetFullPathOfResource ("Test/System.Data/region.xml"), XmlReadMode.InferSchema);
			ds.Relations.Clear(); // because can not call WriteXmlSchema with nested relations.

			TextWriter writer1 = new StringWriter();
			ds.Tables[0].WriteXmlSchema(writer1);
			//string TextString1 = GetNormalizedSchema(writer1.ToString());
			string TextString1 = writer1.ToString();
			string expected1 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""Root"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">" +
  @"<xs:complexType name=""bookstoreType"">" +
  @"</xs:complexType>" +
  @"<xs:element name=""bookstore"" type=""bookstoreType"" />" +
  @"<xs:element name=""Root"" msdata:IsDataSet=""true"" msdata:MainDataTable=""bookstore"" msdata:Locale=""en-US"">" +
    @"<xs:complexType>" +
      @"<xs:choice minOccurs=""0"" maxOccurs=""unbounded"">" +
    @"<xs:element ref=""bookstore"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			Assert.AreEqual(expected1.Replace("\n", ""), TextString1.Replace("\r\n", "").Replace("  ", "").Replace("\n", ""), "#1");

			TextWriter writer2 = new StringWriter();
			ds.Tables[1].WriteXmlSchema(writer2, false);
			//string TextString2 = GetNormalizedSchema(writer2.ToString());
			string TextString2 = writer2.ToString();
			string expected2 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""Root"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">" +
  @"<xs:complexType name=""bookType"">" +
    @"<xs:sequence>" +
      @"<xs:element name=""title"" type=""xs:string"" msdata:Ordinal=""1"" />" +
      @"<xs:element name=""price"" type=""xs:decimal"" msdata:Ordinal=""2"" />" +
    @"</xs:sequence>" +
    @"<xs:attribute name=""genre"" type=""xs:string"" />" +
    @"<xs:attribute name=""bookstore_Id"" type=""xs:int"" use=""prohibited"" />" +
  @"</xs:complexType>" +
  @"<xs:element name=""book"" type=""bookType"" />" +
  @"<xs:element name=""Root"" msdata:IsDataSet=""true"" msdata:MainDataTable=""book"" msdata:Locale=""en-US"">" +
    @"<xs:complexType>" +
      @"<xs:choice minOccurs=""0"" maxOccurs=""unbounded"">" +
    @"<xs:element ref=""book"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			Assert.AreEqual(expected2, TextString2.Replace("\r\n", "").Replace("  ", ""), "#2");

			TextWriter writer3 = new StringWriter();
			ds.Tables[2].WriteXmlSchema(writer3);
			//string TextString3 = GetNormalizedSchema(writer3.ToString());
			string TextString3 = writer3.ToString();
			string expected3 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""Root"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">" +
  @"<xs:complexType name=""authorName"">" +
    @"<xs:sequence>" +
      @"<xs:element name=""first-name"" type=""xs:string"" msdata:Ordinal=""0"" />" +
      @"<xs:element name=""last-name"" type=""xs:string"" msdata:Ordinal=""1"" />" +
    @"</xs:sequence>" +
    @"<xs:attribute name=""book_Id"" type=""xs:int"" use=""prohibited"" />" +
  @"</xs:complexType>" +
  @"<xs:element name=""author"" type=""authorName"" />" +
  @"<xs:element name=""Root"" msdata:IsDataSet=""true"" msdata:MainDataTable=""author"" msdata:Locale=""en-US"">" +
    @"<xs:complexType>" +
      @"<xs:choice minOccurs=""0"" maxOccurs=""unbounded"">" +
        @"<xs:element ref=""author"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			Assert.AreEqual(expected3, TextString3.Replace("\r\n", "").Replace("  ", ""), "#3");

			TextWriter writer4 = new StringWriter();
			ds.Tables[3].WriteXmlSchema(writer4);
			//string TextString4 = GetNormalizedSchema(writer4.ToString());
			string TextString4 = writer4.ToString();
			string expected4 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""Root"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">" +
  @"<xs:element name=""Root"" msdata:IsDataSet=""true"" msdata:MainDataTable=""Region"" msdata:Locale=""en-US"">" +
    @"<xs:complexType>" +
      @"<xs:choice minOccurs=""0"" maxOccurs=""unbounded"">" +
        @"<xs:element name=""Region"">" +
          @"<xs:complexType>" +
            @"<xs:sequence>" +
              @"<xs:element name=""RegionID"" type=""xs:string"" minOccurs=""0"" />" +
              @"<xs:element name=""RegionDescription"" type=""xs:string"" minOccurs=""0"" />" +
            @"</xs:sequence>" +
          @"</xs:complexType>" +
        @"</xs:element>" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			Assert.AreEqual(expected4, TextString4.Replace("\r\n", "").Replace("  ", ""), "#4");
		}

		[Test]
		[Ignore ("MS behavior is far from consistent to be regarded as a reference implementation.")]
		// See the same-named tests in DataSetTest.cs
		public void ReadWriteXmlSchema_IgnoreSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/store.xsd"));
			// check dataset properties before testing write
			AssertDataSet ("ds", ds, "NewDataSet", 3, 2);
			AssertDataTable ("tab1", ds.Tables[0], "bookstore", 1, 0, 0, 1, 1, 1);
			AssertDataTable ("tab2", ds.Tables[1], "book", 5, 0, 1, 1, 2, 1);
			AssertDataTable ("tab3", ds.Tables[2], "author", 3, 0, 1, 0, 1, 0);
			// FIXME: currently order is not compatible. Use name as index
			AssertDataRelation ("rel1", ds.Relations["book_author"], "book_author", true, new string[] { "book_Id" }, new string[] { "book_Id" }, true, true);
			AssertDataRelation ("rel2", ds.Relations["bookstore_book"], "bookstore_book", true, new string[] { "bookstore_Id" }, new string[] { "bookstore_Id" }, true, true);

			ds.ReadXml (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/region.xml"), XmlReadMode.IgnoreSchema);
			ds.Relations.Clear (); // because can not call WriteXmlSchema with nested relations.

			TextWriter writer1 = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (writer1);
			//string TextString1 = GetNormalizedSchema (writer1.ToString ());
			string TextString1 = writer1.ToString ();
			string expected1 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""NewDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">" +
  @"<xs:complexType name=""bookstoreType"">" +
  @"</xs:complexType>" +
  @"<xs:element name=""bookstore"" type=""bookstoreType"" />" +
  @"<xs:element name=""NewDataSet"" msdata:IsDataSet=""true"" msdata:MainDataTable=""bookstore"" msdata:UseCurrentLocale=""true"">" +
    @"<xs:complexType>" +
      @"<xs:choice minOccurs=""0"" maxOccurs=""unbounded"">" +
	@"<xs:element ref=""bookstore"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			Console.WriteLine ("{0} - {1}", TextString1, expected1);
			Assert.AreEqual (expected1, TextString1.Replace ("\r\n", "").Replace ("  ", "").Replace ("\n", ""), "#1");

			TextWriter writer2 = new StringWriter ();
			ds.Tables[1].WriteXmlSchema (writer2, false);
			string TextString2 = GetNormalizedSchema (writer2.ToString ());
			string expected2 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""NewDataSet"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">" +
  @"<xs:complexType name=""bookType"">" +
    @"<xs:sequence>" +
      @"<xs:element msdata:Ordinal=""1"" name=""title"" type=""xs:string"" />" +
      @"<xs:element msdata:Ordinal=""2"" name=""price"" type=""xs:decimal"" />" +
    @"</xs:sequence>" +
    @"<xs:attribute name=""genre"" type=""xs:string"" />" +
    @"<xs:attribute name=""bookstore_Id"" type=""xs:int"" use=""prohibited"" />" +
  @"</xs:complexType>" +
  @"<xs:element name=""book"" type=""bookType"" />" +
  @"<xs:element msdata:IsDataSet=""true"" msdata:MainDataTable=""book"" msdata:UseCurrentLocale=""true"" name=""NewDataSet"">" +
    @"<xs:complexType>" +
      @"<xs:choice maxOccurs=""unbounded"" minOccurs=""0"">" +
	@"<xs:element ref=""book"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			Assert.AreEqual (expected2, TextString2.Replace ("\r\n", "").Replace ("  ", ""), "#2");

			TextWriter writer3 = new StringWriter ();
			ds.Tables[2].WriteXmlSchema (writer3);
			string TextString3 = GetNormalizedSchema (writer3.ToString ());
			string expected3 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""NewDataSet"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">" +
  @"<xs:complexType name=""authorName"">" +
    @"<xs:sequence>" +
      @"<xs:element msdata:Ordinal=""0"" name=""first-name"" type=""xs:string"" />" +
      @"<xs:element msdata:Ordinal=""1"" name=""last-name"" type=""xs:string"" />" +
    @"</xs:sequence>" +
    @"<xs:attribute name=""book_Id"" type=""xs:int"" use=""prohibited"" />" +
  @"</xs:complexType>" +
  @"<xs:element name=""author"" type=""authorName"" />" +
  @"<xs:element msdata:IsDataSet=""true"" msdata:MainDataTable=""author"" msdata:UseCurrentLocale=""true"" name=""NewDataSet"">" +
    @"<xs:complexType>" +
      @"<xs:choice maxOccurs=""unbounded"" minOccurs=""0"">" +
	@"<xs:element ref=""author"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			Assert.AreEqual (expected3, TextString3.Replace ("\r\n", "").Replace ("  ", ""), "#3");

			TextWriter writer4 = new StringWriter ();

			try {
				ds.Tables [3].WriteXmlSchema (writer4);
				Assert.Fail ("expected exception");
			} catch (InvalidOperationException ex) {
				throw ex;
			}
		}

		[Test]
		public void ReadWriteXmlSchema_2 ()
		{
			DataSet ds = new DataSet ("dataset");
			ds.Tables.Add ("table1");
			ds.Tables.Add ("table2");
			ds.Tables[0].Columns.Add ("col");
			ds.Tables[1].Columns.Add ("col");
			ds.Relations.Add ("rel", ds.Tables[0].Columns[0], ds.Tables[1].Columns[0], true);

			MemoryStream ms1 = new MemoryStream ();
			ds.Tables[0].WriteXmlSchema (ms1);
			MemoryStream ms2 = new MemoryStream ();
			ds.Tables[1].WriteXmlSchema (ms2);

			DataSet ds1 = new DataSet ();
			ds1.Tables.Add ();
			ds1.Tables.Add ();
			ds1.Tables[0].ReadXmlSchema (new MemoryStream (ms1.GetBuffer ()));
			ds1.Tables[1].ReadXmlSchema (new MemoryStream (ms2.GetBuffer ()));

			Assert.AreEqual (0, ds1.Relations.Count, "#1");
			Assert.AreEqual (1, ds1.Tables[0].Columns.Count, "#2");
			Assert.AreEqual (1, ds1.Tables[1].Columns.Count, "#3");
		}

		[Test]
		public void ReadWriteXmlSchemaExp_NoRootElmnt ()
		{
			MemoryStream ms = new MemoryStream ();
			DataTable dtr = new DataTable ();
			try {
				dtr.ReadXmlSchema (ms);
				Assert.Fail ("#1");
			} catch (XmlException) {
			}
		}

		[Test]
		public void ReadWriteXmlSchemaExp_NoTableName ()
		{
			DataTable dtw = new DataTable ();
			MemoryStream ms = new MemoryStream ();
			try {
				dtw.WriteXmlSchema (ms);
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void ReadWriteXmlSchemaExp_NoFileName ()
		{
			DataTable dtw = new DataTable ();
			try {
				dtw.WriteXmlSchema (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void ReadWriteXmlSchemaExp_TableNameConflict ()
		{
			DataTable dtw = new DataTable ("Table1");
			StringWriter writer1 = new StringWriter ();
			dtw.WriteXmlSchema (writer1);
			DataTable dtr = new DataTable ("Table2");
			StringReader reader1 = new StringReader (writer1.ToString());
			try {
				dtr.ReadXmlSchema (reader1);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void ReadXmlSchemeWithoutScheme ()
		{
			const string xml = @"<CustomElement />";
			using (var s = new StringReader (xml)) {
				DataTable dt = new DataTable ();
				dt.ReadXmlSchema (s);
				Assert.AreEqual ("", dt.TableName);
			}
		}

		[Test]
		public void ReadXmlSchemeWithScheme ()
		{
			const string xml = @"<CustomElement>
				  <xs:schema id='NewDataSet' xmlns='' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
					<xs:element name='NewDataSet' msdata:IsDataSet='true' msdata:MainDataTable='row' msdata:Locale=''>
					  <xs:complexType>
						<xs:choice minOccurs='0' maxOccurs='unbounded'>
						  <xs:element name='row' msdata:Locale=''>
							<xs:complexType>
							  <xs:sequence>
								<xs:element name='Text' type='xs:string' minOccurs='0' />
							  </xs:sequence>
							</xs:complexType>
						  </xs:element>
						</xs:choice>
					  </xs:complexType>
					</xs:element>
				  </xs:schema>
				</CustomElement>";
			using (var s = new StringReader (xml)) {
				DataTable dt = new DataTable ();
				dt.ReadXmlSchema (s);
				Assert.AreEqual ("row", dt.TableName);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ReadXmlSchemeWithBadScheme ()
		{
			const string xml = @"<CustomElement>
				  <xs:schema id='NewDataSet' xmlns='' xmlns:xs='http://www.w3.org/2001/BAD' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
				  </xs:schema>
				</CustomElement>";
			using (var s = new StringReader (xml)) {
				DataTable dt = new DataTable ();
				dt.ReadXmlSchema (s);
			}
		}

		#endregion // Read/Write XML Tests

	}

	public  class MyDataTable : DataTable
	{
		public static int count;

		public MyDataTable()
		{
			count++;
		}
	}

	[Serializable]
	[TestFixture]
	public class AppDomainsAndFormatInfo
	{
		public void Remote ()
		{
			int n = (int) Convert.ChangeType ("5", typeof (int));
			Assert.AreEqual (5, n, "n");
		}

#if !MONOTOUCH && !FULL_AOT_RUNTIME && !FULL_AOT_INTERP && !DISABLE_REMOTING
		[Test]
		public void NFIFromBug55978 ()
		{
			AppDomain domain = AppDomain.CreateDomain ("testdomain", null,
				AppDomain.CurrentDomain.SetupInformation);
			AppDomainsAndFormatInfo test = new AppDomainsAndFormatInfo ();
			test.Remote ();
			domain.DoCallBack (new CrossAppDomainDelegate (test.Remote));
			AppDomain.Unload (domain);
		}
#endif

		[Test]
		[SetCulture ("en-US")]
		public void Bug55978 ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("StartDate", typeof (DateTime));
	 
			DataRow dr;
			DateTime date = DateTime.Now;
	 
			for (int i = 0; i < 10; i++) {
				dr = dt.NewRow ();
				dr ["StartDate"] = date.AddDays (i);
				dt.Rows.Add (dr);
			}
	 
			DataView dv = dt.DefaultView;
			dv.RowFilter = String.Format (CultureInfo.InvariantCulture,
						      "StartDate >= '{0}' and StartDate <= '{1}'",
						      DateTime.Now.AddDays (2),
						      DateTime.Now.AddDays (4));
			Assert.AreEqual (10, dt.Rows.Count, "Table");
			Assert.AreEqual (2, dv.Count, "View");
		}

		[Test]
		public void Bug82109 ()
		{
			DataTable tbl = new DataTable ();
			tbl.Columns.Add ("data", typeof (DateTime));
			DataRow row = tbl.NewRow ();
			row ["Data"] = new DateTime (2007, 7, 1);
			tbl.Rows.Add (row);

			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Select (tbl);

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("it-IT");
			Select (tbl);

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("fr-FR");
			Select (tbl);
			Thread.CurrentThread.CurrentCulture = currentCulture;
		}

		private static void Select (DataTable tbl)
		{
			tbl.Locale = CultureInfo.InvariantCulture;
			string filter = string.Format ("Data = '{0}'", new DateTime (2007, 7, 1).ToString (CultureInfo.InvariantCulture));
			DataRow [] rows = tbl.Select (filter);
			Assert.AreEqual (1, rows.Length, "Incorrect number of rows found");
		}
	}
}
