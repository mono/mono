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
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using MonoTests.System.Data.Utils;
using System.Collections;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataTableTest :  DataSetAssertion
	{
		string EOL = Environment.NewLine;

		[Test]
		public void Ctor()
		{
			DataTable dt = new DataTable();

			AssertEquals("CaseSensitive must be false." ,false,dt.CaseSensitive);
			Assert("Col",dt.Columns != null);
			//Assert(dt.ChildRelations != null);
			Assert("Const", dt.Constraints != null);
			Assert("ds", dt.DataSet == null); 
			Assert("dv", dt.DefaultView != null);
			Assert("de", dt.DisplayExpression == "");
			Assert("ep", dt.ExtendedProperties != null);
			Assert("he", dt.HasErrors == false);
			Assert("lc", dt.Locale != null);
			Assert("mc", dt.MinimumCapacity == 50); //LAMESPEC:
			Assert("ns", dt.Namespace == "");
			//Assert(dt.ParentRelations != null);
			Assert("pf", dt.Prefix == "");
			Assert("pk", dt.PrimaryKey != null);
			Assert("rows", dt.Rows != null);
			Assert("Site", dt.Site == null);
			Assert("tname", dt.TableName == "");
			
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
			Col4.DataType = Type.GetType ("System.Int16");
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
                	AssertEquals ("test#01", 2, Rows.Length);

			// test with apos escaped
			Rows = Mom.Select ("Name = '''Jhon O'''' Collenal'''");
                	AssertEquals ("test#01.1", 1, Rows.Length);
                	
                	Rows = Mom.Select ("Name = 'Teresa' and ChildName = 'Nick'");
                	AssertEquals ("test#02", 0, Rows.Length);

                	Rows = Mom.Select ("Name = 'Teresa' and ChildName = 'Jack'");
                	AssertEquals ("test#03", 1, Rows.Length);

                	Rows = Mom.Select ("Name = 'Teresa' and ChildName <> 'Jack'");
                	AssertEquals ("test#04", "Mack", Rows [0] [1]);
                	
                	Rows = Mom.Select ("Name = 'Teresa' or ChildName <> 'Jack'");
                	AssertEquals ("test#05", 6, Rows.Length);
			
                	Rows = Child.Select ("age = 20 - 1");
                	AssertEquals ("test#06", 1, Rows.Length);
			
                	Rows = Child.Select ("age <= 20");
                	AssertEquals ("test#07", 3, Rows.Length);
			
                	Rows = Child.Select ("age >= 20");
                	AssertEquals ("test#08", 4, Rows.Length);
			
                	Rows = Child.Select ("age >= 20 and name = 'Mack' or name = 'Nick'");
                	AssertEquals ("test#09", 2, Rows.Length);

                	Rows = Child.Select ("age >= 20 and (name = 'Mack' or name = 'Nick')");
                	AssertEquals ("test#10", 1, Rows.Length);
                	AssertEquals ("test#11", "Mack", Rows [0] [0]);
                	
                	Rows = Child.Select ("not (Name = 'Jack')");
                	AssertEquals ("test#12", 6, Rows.Length);
                }
                
		[Test]
                public void Select2 ()
                {
			DataSet Set = new DataSet ();
			DataTable Child = new DataTable ("Child");

			Set.Tables.Add (Child);
						
			DataColumn Col3 = new DataColumn ("Name");
			DataColumn Col4 = new DataColumn ("Age");
			Col4.DataType = Type.GetType ("System.Int16");
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
                	AssertEquals ("test#01", 3, Rows.Length);
                	AssertEquals ("test#02", "Mack", Rows [0] [0]);
                	AssertEquals ("test#03", "Mick", Rows [1] [0]);                	
                	AssertEquals ("test#04", "Dick", Rows [2] [0]);                	
                	
                	Rows = Child.Select ("age >= 20", "age asc");
                	AssertEquals ("test#05", 3, Rows.Length);
                	AssertEquals ("test#06", "Dick", Rows [0] [0]);
                	AssertEquals ("test#07", "Mick", Rows [1] [0]);                	
                	AssertEquals ("test#08", "Mack", Rows [2] [0]);                	
                
                	Rows = Child.Select ("age >= 20", "name asc");
                	AssertEquals ("test#09", 3, Rows.Length);
                	AssertEquals ("test#10", "Dick", Rows [0] [0]);
                	AssertEquals ("test#11", "Mack", Rows [1] [0]);                	
                	AssertEquals ("test#12", "Mick", Rows [2] [0]);                	

                	Rows = Child.Select ("age >= 20", "name desc");
                	AssertEquals ("test#09", 3, Rows.Length);
                	AssertEquals ("test#10", "Mick", Rows [0] [0]);
                	AssertEquals ("test#11", "Mack", Rows [1] [0]);                	
                	AssertEquals ("test#12", "Dick", Rows [2] [0]);                	

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

			AssertEquals ("test#01", 12, T.Select ("age<=10").Length);
			
			AssertEquals ("test#02", 12, T.Select ("age\n\t<\n\t=\t\n10").Length);

			try {
				T.Select ("name = 1human ");
				Fail ("test#03");
			} catch (Exception e) {
				
				// missing operand after 'human' operand 
				AssertEquals ("test#04", typeof (SyntaxErrorException), e.GetType ());				
			}
			
			try {			
				T.Select ("name = 1");
				Fail ("test#05");
			} catch (Exception e) {
				
				// Cannot perform '=' operation between string and Int32
				AssertEquals ("test#06", typeof (EvaluateException), e.GetType ());
			}
			
			AssertEquals ("test#07", 1, T.Select ("age = '13'").Length);

		}
		
		[Test]
		public void SelectEscaping () {
			DataTable dt = new DataTable ();
			dt.Columns.Add ("SomeCol");
			dt.Rows.Add (new object [] {"\t"});
			dt.Rows.Add (new object [] {"\\"});
			
			AssertEquals ("test#01", 1, dt.Select (@"SomeCol='\t'").Length);
			AssertEquals ("test#02", 1, dt.Select (@"SomeCol='\\'").Length);
			
			try {
				dt.Select (@"SomeCol='\x'");
				Fail("test#03");
			} catch (SyntaxErrorException) {}
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
			
			AssertEquals ("test#01", 11, T.Select ("age < 10").Length);
			AssertEquals ("test#02", 12, T.Select ("age <= 10").Length);			
			AssertEquals ("test#03", 12, T.Select ("age< =10").Length);			
			AssertEquals ("test#04", 89, T.Select ("age > 10").Length);
			AssertEquals ("test#05", 90, T.Select ("age >= 10").Length);			
			AssertEquals ("test#06", 100, T.Select ("age <> 10").Length);
			AssertEquals ("test#07", 3, T.Select ("name < 'human10'").Length);
			AssertEquals ("test#08", 3, T.Select ("id < '10'").Length);
			// FIXME: Somebody explain how this can be possible.
			// it seems that it is no matter between 10 - 30. The
			// result is allways 25 :-P
			//AssertEquals ("test#09", 25, T.Select ("id < 10").Length);
			
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
				Fail ("test#01");
			} catch (Exception e) {
				
				// column name human not found
				AssertEquals ("test#02", typeof (EvaluateException), e.GetType ());
			}
			
			AssertEquals ("test#04", 1, T.Select ("id = '12'").Length);
			AssertEquals ("test#05", 1, T.Select ("id = 12").Length);
			
			try {
				T.Select ("id = 1k3");
				Fail ("test#06");
			} catch (Exception e) {
				
				// no operands after k3 operator
				AssertEquals ("test#07", typeof (SyntaxErrorException), e.GetType ());
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
					
			AssertEquals ("test#01", 1, T.Select ("name = 'human' + 1").Length);
			
			AssertEquals ("test#02", "human1", T.Select ("name = 'human' + 1") [0] ["name"]);			
			AssertEquals ("test#03", 1, T.Select ("name = 'human' + '1'").Length);
			AssertEquals ("test#04", "human1", T.Select ("name = 'human' + '1'") [0] ["name"]);			
			AssertEquals ("test#05", 1, T.Select ("name = 'human' + 1 + 2").Length);
			AssertEquals ("test#06", "human12", T.Select ("name = 'human' + '1' + '2'") [0] ["name"]);
			
			AssertEquals ("test#07", 1, T.Select ("name = 'huMAn' + 1").Length);
			
			Set.CaseSensitive = true;
			AssertEquals ("test#08", 0, T.Select ("name = 'huMAn' + 1").Length);
			
			T.CaseSensitive = false;
			AssertEquals ("test#09", 1, T.Select ("name = 'huMAn' + 1").Length);
			
			T.CaseSensitive = true;
			AssertEquals ("test#10", 0, T.Select ("name = 'huMAn' + 1").Length);
			
			Set.CaseSensitive = false;
			AssertEquals ("test#11", 0, T.Select ("name = 'huMAn' + 1").Length);
			
			T.CaseSensitive = false;
			AssertEquals ("test#12", 1, T.Select ("name = 'huMAn' + 1").Length);
			
			AssertEquals ("test#13", 0, T.Select ("name = 'human1*'").Length);
			AssertEquals ("test#14", 11, T.Select ("name like 'human1*'").Length);
			AssertEquals ("test#15", 11, T.Select ("name like 'human1%'").Length);
			
			try {
				AssertEquals ("test#16", 11, T.Select ("name like 'h*an1'").Length);
				Fail ("test#16");
			} catch (Exception e) {
				
				// 'h*an1' is invalid
				AssertEquals ("test#17", typeof (EvaluateException), e.GetType ());
			}
			
			try {
				AssertEquals ("test#18", 11, T.Select ("name like 'h%an1'").Length);
				Fail ("test#19");
			} catch (Exception e) {
				
				// 'h%an1' is invalid
				AssertEquals ("test#20", typeof (EvaluateException), e.GetType ());
			}
			
			AssertEquals ("test#21", 0, T.Select ("name like 'h[%]an'").Length);
			AssertEquals ("test#22", 1, T.Select ("name like 'h[*]an'").Length);
			
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
			
			AssertEquals ("test#01", 1000, T.Select ("Sum(age) > 10").Length);
			AssertEquals ("test#02", 1000, T.Select ("avg(age) = 499").Length);
			AssertEquals ("test#03", 1000, T.Select ("min(age) = 0").Length);
			AssertEquals ("test#04", 1000, T.Select ("max(age) = 999").Length);
			AssertEquals ("test#05", 1000, T.Select ("count(age) = 1000").Length);
			AssertEquals ("test#06", 1000, T.Select ("stdev(age) > 287 and stdev(age) < 289").Length);
			AssertEquals ("test#07", 1000, T.Select ("var(age) < 83417 and var(age) > 83416").Length);
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
			AssertEquals ("test#01", 25, T.Select ("age = 5*5") [0]["age"]);			
			AssertEquals ("test#02", 901, T.Select ("len(name) > 7").Length);
			AssertEquals ("test#03", 125, T.Select ("age = 5*5*5 AND len(name)>7") [0]["age"]);
			AssertEquals ("test#04", 1, T.Select ("isnull(id, 'test') = 'test'").Length);			
			AssertEquals ("test#05", 1000, T.Select ("iif(id = '56', 'test', 'false') = 'false'").Length);			
			AssertEquals ("test#06", 1, T.Select ("iif(id = '56', 'test', 'false') = 'test'").Length);
			AssertEquals ("test#07", 9, T.Select ("substring(id, 2, 3) = '23'").Length);
			AssertEquals ("test#08", "123", T.Select ("substring(id, 2, 3) = '23'") [0] ["id"]);
			AssertEquals ("test#09", "423", T.Select ("substring(id, 2, 3) = '23'") [3] ["id"]);
			AssertEquals ("test#10", "923", T.Select ("substring(id, 2, 3) = '23'") [8] ["id"]);
			
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
                        Col4.DataType = Type.GetType ("System.Int16");
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
			AssertEquals ("test#01", 6, Rows.Length);
			Rows = Child.Select ("Parent.childname = 'Jack'");
			AssertEquals ("test#02", 1, Rows.Length);
			
			/*
			try {
				// FIXME: LAMESPEC: Why the exception is thrown why... why... 
				Mom.Select ("Child.Name = 'Jack'");
				Fail ("test#03");
			} catch (Exception e) {
				AssertEquals ("test#04", typeof (SyntaxErrorException), e.GetType ());
				AssertEquals ("test#05", "Cannot interpret token 'Child' at position 1.", e.Message);
			}
			*/
			
			Rows = Child.Select ("Parent.name = 'Laura'");
			AssertEquals ("test#06", 3, Rows.Length);
			
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
				Fail ("test#07");
			} catch (Exception e) {
				AssertEquals ("test#08", typeof (EvaluateException), e.GetType ());
				//AssertEquals ("test#09", "The table [Child] involved in more than one relation. You must explicitly mention a relation name in the expression 'parent.[ChildName]'.", e.Message);
			}
			
			Rows = Child.Select ("Parent(rel).ChildName = 'Jack'");
			AssertEquals ("test#10", 1, Rows.Length);

			Rows = Child.Select ("Parent(Rel2).ChildName = 'Jack'");
			AssertEquals ("test#10", 1, Rows.Length);
			
			try {
			     	Mom.Select ("Parent.name  = 'John'");
			} catch (Exception e) {
				AssertEquals ("test#11", typeof (IndexOutOfRangeException), e.GetType ());
				AssertEquals ("test#12", "Cannot find relation 0.", e.Message);
			}
			
		}

		[Test]
		public void SelectRowState()
		{
			DataTable d = new DataTable();
			d.Columns.Add (new DataColumn ("aaa"));
			DataRow [] rows = d.Select (null, null, DataViewRowState.Deleted);
			AssertEquals(0, rows.Length);
			d.Rows.Add (new object [] {"bbb"});
			d.Rows.Add (new object [] {"bbb"});
			rows = d.Select (null, null, DataViewRowState.Deleted);
			AssertEquals(0, rows.Length);
		}

		[Test]
		public void ToStringTest()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("Col1",typeof(int));
			
			dt.TableName = "Mytable";
			dt.DisplayExpression = "Col1";
			
			
			string cmpr = dt.TableName + " + " + dt.DisplayExpression;
			AssertEquals(cmpr,dt.ToString());
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
			
			AssertEquals ("test#01", 0, dt.PrimaryKey.Length);
			
			dt.PrimaryKey = new DataColumn [] {dt.Columns [0]};
			AssertEquals ("test#02", 1, dt.PrimaryKey.Length);
			AssertEquals ("test#03", "Column1", dt.PrimaryKey [0].ColumnName);
			
			dt.PrimaryKey = null;
			AssertEquals ("test#04", 0, dt.PrimaryKey.Length);
			
			Col = new DataColumn ("failed");
			
			try {
				dt.PrimaryKey = new DataColumn [] {Col};
				Fail ("test#05");					
			} catch (Exception e) {
				AssertEquals ("test#06", typeof (ArgumentException), e.GetType ());
				AssertEquals ("test#07", "Column must belong to a table.", e.Message);
			}
			
			DataTable dt2 = new DataTable ();
			dt2.Columns.Add ();
			
			try {
				dt.PrimaryKey = new DataColumn [] {dt2.Columns [0]};
				Fail ("test#08");
			} catch (Exception e) {
				AssertEquals ("test#09", typeof (ArgumentException), e.GetType ());
				AssertEquals ("test#10", "PrimaryKey columns do not belong to this table.", e.Message);
			}
			
			
			AssertEquals ("test#11", 0, dt.Constraints.Count);
			
			dt.PrimaryKey = new DataColumn [] {dt.Columns [0], dt.Columns [1]};
			AssertEquals ("test#12", 2, dt.PrimaryKey.Length);
			AssertEquals ("test#13", 1, dt.Constraints.Count);
			AssertEquals ("test#14", true, dt.Constraints [0] is UniqueConstraint);
			AssertEquals ("test#15", "Column1", dt.PrimaryKey [0].ColumnName);
			AssertEquals ("test#16", "Column2", dt.PrimaryKey [1].ColumnName);
			
		}
		
		[Test]
		public void PropertyExceptions ()
		{
			DataSet set = new DataSet ();
			DataTable table = new DataTable ();
			DataTable table1 =  new DataTable ();
			set.Tables.Add (table);
			set.Tables.Add (table1);

			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = Type.GetType ("System.Int32");
			table.Columns.Add (col);
			UniqueConstraint uc = new UniqueConstraint ("UK1", table.Columns[0] );
			table.Constraints.Add (uc);
			table.CaseSensitive = false;
                                                                                                                           
			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = Type.GetType ("System.String");
			table.Columns.Add (col);
        	        
			col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = Type.GetType ("System.Int32");
			table1.Columns.Add (col);
			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = Type.GetType ("System.String");
			table1.Columns.Add (col);

			DataRelation dr = new DataRelation ("DR", table.Columns[0], table1.Columns[0]);
			set.Relations.Add (dr);

			try {
				table.CaseSensitive = true;
				table1.CaseSensitive = true;
				Fail ("#A01");
			}
			catch (Exception e) {
				if (e.GetType () != typeof (AssertionException))
					AssertEquals ("#A02", "Cannot change CaseSensitive or Locale property. This change would lead to at least one DataRelation or Constraint to have different Locale or CaseSensitive settings between its related tables.",e.Message);
				else
					Console.WriteLine (e);
			}
			try {
				CultureInfo cultureInfo = new CultureInfo ("en-gb");
				table.Locale = cultureInfo;
				table1.Locale = cultureInfo;
				Fail ("#A03");
			}
			catch (Exception e) {
				 if (e.GetType () != typeof (AssertionException))
					AssertEquals ("#A04", "Cannot change CaseSensitive or Locale property. This change would lead to at least one DataRelation or Constraint to have different Locale or CaseSensitive settings between its related tables.",e.Message);
				else
					Console.WriteLine (e);
			}
			try {
				table.Prefix = "Prefix#1";
				Fail ("#A05");
			}
			catch (Exception e){
				if (e.GetType () != typeof (AssertionException))
					AssertEquals ("#A06", "Prefix 'Prefix#1' is not valid, because it contains special characters.",e.Message);
				else
					Console.WriteLine (e);

			}
		}

		[Test]
		public void GetErrors ()
		{
			DataTable table = new DataTable ();

			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = Type.GetType ("System.Int32");
			table.Columns.Add (col);
                                                                                                                             
			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = Type.GetType ("System.String");
			table.Columns.Add (col);
			
			DataRow row = table.NewRow ();
			row ["Id"] = 147;
			row ["name"] = "Abc";
			row.RowError = "Error#1";
			table.Rows.Add (row);

			AssertEquals ("#A01", 1, table.GetErrors ().Length);
			AssertEquals ("#A02", "Error#1", (table.GetErrors ())[0].RowError);
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
			col.DataType = Type.GetType ("System.Int32");
			table.Columns.Add (col);
			UniqueConstraint uc = new UniqueConstraint ("UK1", table.Columns[0] );
			table.Constraints.Add (uc);
                
			col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = Type.GetType ("System.Int32");
			table1.Columns.Add (col);
                                                                                                                             
			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = Type.GetType ("System.String");
			table.Columns.Add (col);
			
			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = Type.GetType ("System.String");
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
#if NET_1_1 // This prevents further tests after .NET 1.1.
#else
			CultureInfo cultureInfo = new CultureInfo ("en-gb");
			table.Locale = cultureInfo;
#endif

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
			AssertEquals ("#A01",true ,cloneTable.CaseSensitive);
			AssertEquals ("#A02", 0 , cloneTable.ChildRelations.Count);
			AssertEquals ("#A03", 0 , cloneTable.ParentRelations.Count);
			AssertEquals ("#A04", 2,  cloneTable.Columns.Count);
			AssertEquals ("#A05", 1, cloneTable.Constraints.Count);
			AssertEquals ("#A06", "Id / Name + (Id * Id)", cloneTable.DisplayExpression);
			AssertEquals ("#A07", 1 ,cloneTable.ExtendedProperties.Count);
			AssertEquals ("#A08", false ,cloneTable.HasErrors);
#if NET_1_1
#else
			AssertEquals ("#A09", 2057, cloneTable.Locale.LCID);
#endif
			AssertEquals ("#A10", 100, cloneTable.MinimumCapacity);
			AssertEquals ("#A11","Namespace#1", cloneTable.Namespace);
			AssertEquals ("#A12", "PrefixNo:1",cloneTable.Prefix);
			AssertEquals ("#A13", "Id",  cloneTable.PrimaryKey[0].ColumnName);
			AssertEquals ("#A14",0 , cloneTable.Rows.Count );
			AssertEquals ("#A15", "Table#1", cloneTable.TableName);

			//Testing properties of copy
			DataTable copyTable = table.Copy ();
			AssertEquals ("#A16",true ,copyTable.CaseSensitive);
			AssertEquals ("#A17", 0 , copyTable.ChildRelations.Count);
			AssertEquals ("#A18", 0 , copyTable.ParentRelations.Count);
			AssertEquals ("#A19", 2,  copyTable.Columns.Count);
			AssertEquals ("#A20", 1, copyTable.Constraints.Count);
			AssertEquals ("#A21", "Id / Name + (Id * Id)", copyTable.DisplayExpression);
			AssertEquals ("#A22", 1 ,copyTable.ExtendedProperties.Count);
			AssertEquals ("#A23", true ,copyTable.HasErrors);
#if NET_1_1
#else
			AssertEquals ("#A24", 2057, copyTable.Locale.LCID);
#endif
			AssertEquals ("#A25", 100, copyTable.MinimumCapacity);
			AssertEquals ("#A26","Namespace#1", copyTable.Namespace);
			AssertEquals ("#A27", "PrefixNo:1",copyTable.Prefix);
			AssertEquals ("#A28", "Id",  copyTable.PrimaryKey[0].ColumnName);
			AssertEquals ("#A29", 2 , copyTable.Rows.Count );
			AssertEquals ("#A30", "Table#1", copyTable.TableName);
		}

		[Test]
		public void LoadDataException ()
		{
			DataTable table = new DataTable ();
			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = Type.GetType ("System.Int32");
			col.DefaultValue = 47;
			table.Columns.Add (col);
			UniqueConstraint uc = new UniqueConstraint ("UK1", table.Columns[0] );
			table.Constraints.Add (uc);
                
			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = Type.GetType ("System.String");
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
				Fail ("#A01");
			}
			catch (ConstraintException) {
			}
		}
		[Test]
		public void Changes () //To test GetChanges and RejectChanges
		{
			DataTable table = new DataTable ();

			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = Type.GetType ("System.Int32");
			table.Columns.Add (col);
			UniqueConstraint uc = new UniqueConstraint ("UK1", table.Columns[0] );
			table.Constraints.Add (uc);
                                                                                                                             
			col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = Type.GetType ("System.String");
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
			AssertEquals ("#A01", 1 ,changesTable.Rows.Count);
 			AssertEquals ("#A02","Efg" ,changesTable.Rows[0]["Name"]);               	
			table.AcceptChanges ();
			changesTable = table.GetChanges ();
			try {
				int cnt = changesTable.Rows.Count;
			}
			catch(Exception e) {
				if (e.GetType () != typeof (AssertionException))
					AssertEquals ("#A03",typeof(NullReferenceException) ,e.GetType ());
				else
					Console.WriteLine (e);
			}
			
			//Testing RejectChanges
			row = table.NewRow ();
                        row ["Id"] = 247;
                        row ["name"] = "Hij";
                        table.Rows.Add (row);

			(table.Rows [0])["Name"] = "AaBbCc";
			table.RejectChanges ();
			AssertEquals ("#A03", "Abc" , (table.Rows [0]) ["Name"]);
			AssertEquals ("#A04", 2, table.Rows.Count);
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
                                Fail ("#AA1 Should have thrown exception violativ PK");
                        } catch (ConstraintException e) {}

                        // check row states
                        AssertEquals ("#A1", src.Rows [0].RowState, target.Rows [0].RowState);
                        AssertEquals ("#A2", src.Rows [1].RowState, target.Rows [1].RowState);
                        AssertEquals ("#A3", src.Rows [2].RowState, target.Rows [2].RowState);
                        AssertEquals ("#A4", src.Rows [3].RowState, target.Rows [3].RowState);

                        // check for modified row (1st row)
                        AssertEquals ("#B1", (string) src.Rows [0] [1], (string) target.Rows [0] [1]);
                        AssertEquals ("#B2", (string) src.Rows [0] [1, DataRowVersion.Default], (string) target.Rows [0] [1, DataRowVersion.Default]);
                        AssertEquals ("#B3", (string) src.Rows [0] [1, DataRowVersion.Original], (string) target.Rows [0] [1, DataRowVersion.Original]);
                        AssertEquals ("#B4", (string) src.Rows [0] [1, DataRowVersion.Current], (string) target.Rows [0] [1, DataRowVersion.Current]);
                        AssertEquals ("#B5", false, target.Rows [0].HasVersion(DataRowVersion.Proposed));

                        // check for deleted row (2nd row)
                        AssertEquals ("#C1", (string) src.Rows [1] [1, DataRowVersion.Original], (string) target.Rows [1] [1, DataRowVersion.Original]);

                        // check for unchanged row (3rd row)
                        AssertEquals ("#D1", (string) src.Rows [2] [1], (string) target.Rows [2] [1]);
                        AssertEquals ("#D2", (string) src.Rows [2] [1, DataRowVersion.Default], (string) target.Rows [2] [1, DataRowVersion.Default]);
                        AssertEquals ("#D3", (string) src.Rows [2] [1, DataRowVersion.Original], (string) target.Rows [2] [1, DataRowVersion.Original]);
                        AssertEquals ("#D4", (string) src.Rows [2] [1, DataRowVersion.Current], (string) target.Rows [2] [1, DataRowVersion.Current]);

                        // check for newly added row (4th row)
                        AssertEquals ("#E1", (string) src.Rows [3] [1], (string) target.Rows [3] [1]);
                        AssertEquals ("#E2", (string) src.Rows [3] [1, DataRowVersion.Default], (string) target.Rows [3] [1, DataRowVersion.Default]);
                        AssertEquals ("#E3", (string) src.Rows [3] [1, DataRowVersion.Current], (string) target.Rows [3] [1, DataRowVersion.Current]);
                }

                [Test]
		public void ImportRowDetachedTest ()
		{
			DataTable table = new DataTable ();
			DataColumn col = new DataColumn ();
			col.ColumnName = "Id";
			col.DataType = Type.GetType ("System.Int32");
			table.Columns.Add (col);

                        table.PrimaryKey = new DataColumn [] {col};

                        col = new DataColumn ();
			col.ColumnName = "Name";
			col.DataType = Type.GetType ("System.String");
			table.Columns.Add (col);
                        
			DataRow row = table.NewRow ();
			row ["Id"] = 147;
			row ["name"] = "Abc";

                        // keep silent as ms.net ;-), though this is not useful.
                        table.ImportRow (row);

			//if RowState is detached, then dont import the row.
			AssertEquals ("#1", 0, table.Rows.Count);
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
			AssertEquals ("#1", 2, table.Rows.Count);

			// Both the deleted rows shud be now gone
			table.AcceptChanges ();
			AssertEquals ("#2", 0, table.Rows.Count);

			//just add another row
			row = table.Rows.Add (new object[] {1,2});
			// no exception shud be thrown
			table.AcceptChanges ();

			// If row is in Deleted state, then ImportRow loads the
			// row and validate only on RejectChanges
			row.Delete ();
			table.ImportRow (row);
			AssertEquals ("#3", 2, table.Rows.Count);
			AssertEquals ("#4", DataRowState.Deleted, table.Rows[1].RowState);

			try {
				table.RejectChanges ();
				Fail ("#5");
			} catch (ConstraintException e) {
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
                
			AssertEquals (2, table.Rows.Count);
			AssertEquals (1, table.ChildRelations.Count);
			try {
				table.Reset ();
				Fail ("#A01, should have thrown ArgumentException");
			}
                        catch (ArgumentException) {
			}
			AssertEquals ("#CT01", 0, table.Rows.Count);
			AssertEquals ("#CT02", 0, table.ChildRelations.Count);
			AssertEquals ("#CT03", 0, table.ParentRelations.Count);
			AssertEquals ("#CT04", 0, table.Constraints.Count);

			table1.Reset ();
			AssertEquals ("#A05", 0, table1.Rows.Count);
			AssertEquals ("#A06", 0, table1.Constraints.Count);
			AssertEquals ("#A07", 0, table1.ParentRelations.Count);
		
                        // clear test
			table.Clear ();
			AssertEquals ("#A08", 0, table.Rows.Count);
#if NET_1_1
			AssertEquals ("#A09", 0, table.Constraints.Count);
#else
			AssertEquals ("#A09", 1, table.Constraints.Count);
#endif
			AssertEquals ("#A10", 0, table.ChildRelations.Count);

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
#if NET_2_0
                        _tableClearedEventFired = false;
                        table.TableCleared += new DataTableClearEventHandler (OnTableCleared);
#endif // NET_2_0
                        
                        table.Clear ();
#if NET_2_0
                        AssertEquals ("#0 should have fired cleared event", true, _tableClearedEventFired);
#endif // NET_2_0
                        
                        DataRow r = table.Rows.Find (1);
                        AssertEquals ("#1 should have cleared", true, r == null);

                        // try adding new row. indexes should have cleared
                        table.Rows.Add (new object [] { 2, "mono 2" });
                        AssertEquals ("#2 should add row", 1, table.Rows.Count);
                }
#if NET_2_0
                private bool _tableClearedEventFired = false;
                private void OnTableCleared (object src, DataTableClearEventArgs args)
                {
                        _tableClearedEventFired = true;
                }
#endif // NET_2_0
                

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
			AssertEquals (5, doc.DocumentElement.FirstChild.ChildNodes.Count);
		}

		[Test]
		[ExpectedException (typeof (DataException))]
		public void SetPrimaryKeyAssertsNonNull ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Constraints.Add (new UniqueConstraint (dt.Columns [0]));
			dt.Rows.Add (new object [] {1, 3});
			dt.Rows.Add (new object [] {DBNull.Value, 3});

			dt.PrimaryKey = new DataColumn [] {dt.Columns [0]};
		}

		[Test]
		[ExpectedException (typeof (NoNullAllowedException))]
		public void PrimaryKeyColumnChecksNonNull ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			dt.Constraints.Add (new UniqueConstraint (dt.Columns [0]));
			dt.PrimaryKey = new DataColumn [] {dt.Columns [0]};
			dt.Rows.Add (new object [] {1, 3});
			dt.Rows.Add (new object [] {DBNull.Value, 3});
		}

		[Test]
		public void PrimaryKey_CheckSetsAllowDBNull ()
		{
			DataTable table = new DataTable ();
			DataColumn col1 = table.Columns.Add ("col1", typeof (int));
			DataColumn col2 = table.Columns.Add ("col2", typeof (int));
	
			AssertEquals ("#1" , true, col1.AllowDBNull);
			AssertEquals ("#2" , true, col2.AllowDBNull);
			AssertEquals ("#3" , false, col2.Unique);
			AssertEquals ("#4" , false, col2.Unique);

			table.PrimaryKey = new DataColumn[] {col1,col2};
			AssertEquals ("#5" , false, col1.AllowDBNull);
			AssertEquals ("#6" , false, col2.AllowDBNull);
			// LAMESPEC or bug ?? 
			AssertEquals ("#7" , false, col1.Unique);
			AssertEquals ("#8" , false, col2.Unique);
		}

		void RowChanging (object o, DataRowChangeEventArgs e)
		{
			AssertEquals ("changing.Action", rowChangingExpectedAction, e.Action);
			rowChangingRowChanging = true;
		}

		void RowChanged (object o, DataRowChangeEventArgs e)
		{
			AssertEquals ("changed.Action", rowChangingExpectedAction, e.Action);
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
			Assert ("changing,Added", rowChangingRowChanging);
			Assert ("changed,Added", rowChangingRowChanged);
			rowChangingExpectedAction = DataRowAction.Change;
			dt.Rows [0] [0] = 2;
			Assert ("changing,Changed", rowChangingRowChanging);
			Assert ("changed,Changed", rowChangingRowChanged);
		}

		 [Test]
                public void CloneSubClassTest()
                {
                        MyDataTable dt1 = new MyDataTable();
                        MyDataTable dt = (MyDataTable)(dt1.Clone());
                        AssertEquals("A#01",2,MyDataTable.count);
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

                                AssertEquals ("#1 should have fired event and set action to commit",
                                              DataRowAction.Commit, rowActionChanging);
                                AssertEquals ("#2 should have fired event and set action to commit",
                                              DataRowAction.Commit, rowActionChanged);

                        } finally {
                                dt.RowChanged  -= new DataRowChangeEventHandler (OnRowChanged);
                                dt.RowChanging -= new DataRowChangeEventHandler (OnRowChanging);

                        }
                }

				[Test]
				public void ColumnObjectTypeTest() {
					DataTable dt = new DataTable();
					dt.Columns.Add("Series Label", typeof(SqlInt32));
					dt.Rows.Add(new object[] {"sss"});
					AssertEquals(1, dt.Rows.Count);
				}

                public void OnRowChanging (object src, DataRowChangeEventArgs args)
                {
                        rowActionChanging = args.Action;
                }
                
                public void OnRowChanged (object src, DataRowChangeEventArgs args)
                {
                        rowActionChanged = args.Action;
		}


#if NET_2_0
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
		public void CreateDataReader1 () {
			localSetup ();
			DataTableReader dtr = dt.CreateDataReader ();
			Assert ("HasRows", dtr.HasRows);
			AssertEquals ("CountCols", dt.Columns.Count, dtr.FieldCount);
			int ri = 0;
			while (dtr.Read ()) {
				for (int i = 0; i < dtr.FieldCount; i++) {
					AssertEquals ("RowData-" + ri + "-" + i, dt.Rows[ri][i],
						dtr[i]);
				}
				ri++;
			}
		}

		[Test]
		public void CreateDataReader2 () {
			localSetup ();
			DataTableReader dtr = dt.CreateDataReader ();
			Assert ("HasRows", dtr.HasRows);
			AssertEquals ("CountCols", dt.Columns.Count, dtr.FieldCount);
			dtr.Read ();
			AssertEquals ("RowData0-0", 1, dtr[0]);
			AssertEquals ("RowData0-1", "mono 1", dtr[1]);
			dtr.Read ();
			AssertEquals ("RowData1-0", 2, dtr[0]);
			AssertEquals ("RowData1-1", "mono 2", dtr[1]);
			dtr.Read ();
			AssertEquals ("RowData2-0", 3, dtr[0]);
			AssertEquals ("RowData2-1", "mono 3", dtr[1]);
		}

		#endregion // DataTable.CreateDataReader Tests

		#region DataTable.Load Tests

		[Test]
		public void Load_Basic () {
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
			AssertEquals ("NColumns", 2, dtLoad.Columns.Count);
			AssertEquals ("NRows", 3, dtLoad.Rows.Count);
			AssertEquals ("RowData0-0", 1, dtLoad.Rows[0][0]);
			AssertEquals ("RowData0-1", "mono 1", dtLoad.Rows[0][1]);
			AssertEquals ("RowData1-0", 2, dtLoad.Rows[1][0]);
			AssertEquals ("RowData1-1", "mono 2", dtLoad.Rows[1][1]);
			AssertEquals ("RowData2-0", 3, dtLoad.Rows[2][0]);
			AssertEquals ("RowData2-1", "mono 3", dtLoad.Rows[2][1]);
		}

		[Test]
		public void Load_NoSchema () {
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadNoSchema");
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr);
			AssertEquals ("NColumns", 2, dtLoad.Columns.Count);
			AssertEquals ("NRows", 3, dtLoad.Rows.Count);
			AssertEquals ("RowData0-0", 1, dtLoad.Rows[0][0]);
			AssertEquals ("RowData0-1", "mono 1", dtLoad.Rows[0][1]);
			AssertEquals ("RowData1-0", 2, dtLoad.Rows[1][0]);
			AssertEquals ("RowData1-1", "mono 2", dtLoad.Rows[1][1]);
			AssertEquals ("RowData2-0", 3, dtLoad.Rows[2][0]);
			AssertEquals ("RowData2-1", "mono 3", dtLoad.Rows[2][1]);
		}

		internal struct fillErrorStruct {
			internal string error;
			internal string tableName;
			internal int rowKey;
			internal bool contFlag;
			internal void init (string tbl, int row, bool cont, string err) {
				tableName = tbl;
				rowKey = row;
				contFlag = cont;
				error = err;
			}
		}
		private fillErrorStruct[] fillErr = new fillErrorStruct[3];
		private int fillErrCounter;
		private void fillErrorHandler (object sender, FillErrorEventArgs e) {
			e.Continue = fillErr[fillErrCounter].contFlag;
			AssertEquals ("fillErr-T", fillErr[fillErrCounter].tableName, e.DataTable.TableName);
			AssertEquals ("fillErr-R", fillErr[fillErrCounter].rowKey, e.Values[0]);
			AssertEquals ("fillErr-C", fillErr[fillErrCounter].contFlag, e.Continue);
			AssertEquals ("fillErr-E", fillErr[fillErrCounter].error, e.Errors.Message);
			fillErrCounter++;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Load_Incompatible () {
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadIncompatible");
			dtLoad.Columns.Add ("name", typeof (double));
			DataTableReader dtr = dt.CreateDataReader ();
			dtLoad.Load (dtr);
		}
		[Test]
		// Load doesn't have a third overload in System.Data
		// and is commented-out below
		public void Load_IncompatibleEHandlerT () {
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
			//dtLoad.Load (dtr,LoadOption.PreserveChanges,fillErrorHandler);
		}
		[Test]
		[Category ("NotWorking")]
		// Load doesn't have a third overload in System.Data
		// and is commented-out below
		[ExpectedException (typeof (ArgumentException))]
		public void Load_IncompatibleEHandlerF () {
			fillErrCounter = 0;
			fillErr[0].init ("LoadIncompatible", 1, false,
				"Input string was not in a correct format.Couldn't store <mono 1> in name Column.  Expected type is Double.");
			localSetup ();
			DataTable dtLoad = new DataTable ("LoadIncompatible");
			dtLoad.Columns.Add ("name", typeof (double));
			DataTableReader dtr = dt.CreateDataReader ();
			//dtLoad.Load (dtr, LoadOption.PreserveChanges, fillErrorHandler);
		}

		[Test]
		public void Load_ExtraColsEqualVal () {
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
			AssertEquals ("NColumns", 2, dtLoad.Columns.Count);
			AssertEquals ("NRows", 3, dtLoad.Rows.Count);
			AssertEquals ("RowData0-0", 1, dtLoad.Rows[0][0]);
			AssertEquals ("RowData0-1", "mono 1", dtLoad.Rows[0][1]);
			AssertEquals ("RowData1-0", 2, dtLoad.Rows[1][0]);
			AssertEquals ("RowData1-1", "mono 2", dtLoad.Rows[1][1]);
			AssertEquals ("RowData2-0", 3, dtLoad.Rows[2][0]);
			AssertEquals ("RowData2-1", "mono 3", dtLoad.Rows[2][1]);
		}

		[Test]
		public void Load_ExtraColsNonEqualVal () {
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
			AssertEquals ("NColumns", 2, dtLoad.Columns.Count);
			AssertEquals ("NRows", 6, dtLoad.Rows.Count);
			AssertEquals ("RowData0-0", 4, dtLoad.Rows[0][0]);
			AssertEquals ("RowData1-0", 5, dtLoad.Rows[1][0]);
			AssertEquals ("RowData2-0", 6, dtLoad.Rows[2][0]);
			AssertEquals ("RowData3-0", 1, dtLoad.Rows[3][0]);
			AssertEquals ("RowData3-1", "mono 1", dtLoad.Rows[3][1]);
			AssertEquals ("RowData4-0", 2, dtLoad.Rows[4][0]);
			AssertEquals ("RowData4-1", "mono 2", dtLoad.Rows[4][1]);
			AssertEquals ("RowData5-0", 3, dtLoad.Rows[5][0]);
			AssertEquals ("RowData5-1", "mono 3", dtLoad.Rows[5][1]);
		}

		[Test]
		[ExpectedException (typeof (ConstraintException))]
		public void Load_MissingColsNonNullable () {
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
			dtLoad.Load (dtr);
		}

		[Test]
		public void Load_MissingColsDefault () {
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
			AssertEquals ("NColumns", 3, dtLoad.Columns.Count);
			AssertEquals ("NRows", 6, dtLoad.Rows.Count);
			AssertEquals ("RowData0-0", 4, dtLoad.Rows[0][0]);
			AssertEquals ("RowData0-1", "mono 4", dtLoad.Rows[0][1]);
			AssertEquals ("RowData0-2", "miss4", dtLoad.Rows[0][2]);
			AssertEquals ("RowData1-0", 5, dtLoad.Rows[1][0]);
			AssertEquals ("RowData1-1", "mono 5", dtLoad.Rows[1][1]);
			AssertEquals ("RowData1-2", "miss5", dtLoad.Rows[1][2]);
			AssertEquals ("RowData2-0", 6, dtLoad.Rows[2][0]);
			AssertEquals ("RowData2-1", "mono 6", dtLoad.Rows[2][1]);
			AssertEquals ("RowData2-2", "miss6", dtLoad.Rows[2][2]);
			AssertEquals ("RowData3-0", 1, dtLoad.Rows[3][0]);
			AssertEquals ("RowData3-1", "mono 1", dtLoad.Rows[3][1]);
			AssertEquals ("RowData3-2", "DefaultValue", dtLoad.Rows[3][2]);
			AssertEquals ("RowData4-0", 2, dtLoad.Rows[4][0]);
			AssertEquals ("RowData4-1", "mono 2", dtLoad.Rows[4][1]);
			AssertEquals ("RowData4-2", "DefaultValue", dtLoad.Rows[4][2]);
			AssertEquals ("RowData5-0", 3, dtLoad.Rows[5][0]);
			AssertEquals ("RowData5-1", "mono 3", dtLoad.Rows[5][1]);
			AssertEquals ("RowData5-2", "DefaultValue", dtLoad.Rows[5][2]);
		}

		[Test]
		public void Load_MissingColsNullable () {
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
			AssertEquals ("NColumns", 3, dtLoad.Columns.Count);
			AssertEquals ("NRows", 6, dtLoad.Rows.Count);
			AssertEquals ("RowData0-0", 4, dtLoad.Rows[0][0]);
			AssertEquals ("RowData0-1", "mono 4", dtLoad.Rows[0][1]);
			AssertEquals ("RowData0-2", "miss4", dtLoad.Rows[0][2]);
			AssertEquals ("RowData1-0", 5, dtLoad.Rows[1][0]);
			AssertEquals ("RowData1-1", "mono 5", dtLoad.Rows[1][1]);
			AssertEquals ("RowData1-2", "miss5", dtLoad.Rows[1][2]);
			AssertEquals ("RowData2-0", 6, dtLoad.Rows[2][0]);
			AssertEquals ("RowData2-1", "mono 6", dtLoad.Rows[2][1]);
			AssertEquals ("RowData2-2", "miss6", dtLoad.Rows[2][2]);
			AssertEquals ("RowData3-0", 1, dtLoad.Rows[3][0]);
			AssertEquals ("RowData3-1", "mono 1", dtLoad.Rows[3][1]);
			//AssertEquals ("RowData3-2", null, dtLoad.Rows[3][2]);
			AssertEquals ("RowData4-0", 2, dtLoad.Rows[4][0]);
			AssertEquals ("RowData4-1", "mono 2", dtLoad.Rows[4][1]);
			//AssertEquals ("RowData4-2", null, dtLoad.Rows[4][2]);
			AssertEquals ("RowData5-0", 3, dtLoad.Rows[5][0]);
			AssertEquals ("RowData5-1", "mono 3", dtLoad.Rows[5][1]);
			//AssertEquals ("RowData5-2", null, dtLoad.Rows[5][2]);
		}

		private DataTable setupRowState () {
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
		private bool checkAction = false;
		private int rowChagedCounter, rowChangingCounter;
		private void rowActionInit (DataRowAction[] act) {
			checkAction = true;
			rowChagedCounter = 0;
			rowChangingCounter = 0;
			for (int i = 0; i < 5; i++)
				rowChangeAction[i] = act[i];
		}
		private void rowActionEnd () {
			checkAction = false;
		}
		private void dtLoad_RowChanged (object sender, DataRowChangeEventArgs e) {
			if (checkAction) {
				AssertEquals ("RowChanged" + rowChagedCounter,
					rowChangeAction[rowChagedCounter], e.Action);
				rowChagedCounter++;
			}
		}
		private void dtLoad_RowChanging (object sender, DataRowChangeEventArgs e) {
			if (checkAction) {
				AssertEquals ("RowChanging" + rowChangingCounter,
					rowChangeAction[rowChangingCounter], e.Action);
				rowChangingCounter++;
			}
		}

		[Test]
		public void Load_RowStateChangesDefault () {
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
			AssertEquals ("RowData0-C", "mono 1",
				dtLoad.Rows[0][1,DataRowVersion.Current]);
			AssertEquals ("RowData0-O", "mono 1",
				dtLoad.Rows[0][1,DataRowVersion.Original]);
			AssertEquals ("RowState0", DataRowState.Unchanged,
				dtLoad.Rows[0].RowState);
			// asserting Modified Row1
			AssertEquals ("RowData1-C", "Modify 2",
				dtLoad.Rows[1][1, DataRowVersion.Current]);
			AssertEquals ("RowData1-O", "mono 2",
				dtLoad.Rows[1][1, DataRowVersion.Original]);
			AssertEquals ("RowState1", DataRowState.Modified,
				dtLoad.Rows[1].RowState);
			// asserting Deleted Row2
			AssertEquals ("RowData1-O", "mono 3",
				dtLoad.Rows[2][1, DataRowVersion.Original]);
			AssertEquals ("RowState2", DataRowState.Deleted,
				dtLoad.Rows[2].RowState);
			// asserting Added Row3
			AssertEquals ("RowData3-C", "Add 4",
				dtLoad.Rows[3][1, DataRowVersion.Current]);
			AssertEquals ("RowData3-O", "mono 4",
				dtLoad.Rows[3][1, DataRowVersion.Original]);
			AssertEquals ("RowState3", DataRowState.Modified,
				dtLoad.Rows[3].RowState);
			// asserting Unpresent Row4
			AssertEquals ("RowData4-C", "mono 5",
				dtLoad.Rows[4][1, DataRowVersion.Current]);
			AssertEquals ("RowData4-O", "mono 5",
				dtLoad.Rows[4][1, DataRowVersion.Original]);
			AssertEquals ("RowState4", DataRowState.Unchanged,
				dtLoad.Rows[4].RowState);
		}

		[Test]
		[ExpectedException (typeof (VersionNotFoundException))]
		public void Load_RowStateChangesDefaultDelete () {
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
			AssertEquals ("RowData2-C", " ",
				dtLoad.Rows[2][1, DataRowVersion.Current]);
		}

		[Test]
		public void Load_RowStatePreserveChanges () {
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
			AssertEquals ("RowData0-C", "mono 1",
				dtLoad.Rows[0][1, DataRowVersion.Current]);
			AssertEquals ("RowData0-O", "mono 1",
				dtLoad.Rows[0][1, DataRowVersion.Original]);
			AssertEquals ("RowState0", DataRowState.Unchanged,
				dtLoad.Rows[0].RowState);
			// asserting Modified Row1
			AssertEquals ("RowData1-C", "Modify 2",
				dtLoad.Rows[1][1, DataRowVersion.Current]);
			AssertEquals ("RowData1-O", "mono 2",
				dtLoad.Rows[1][1, DataRowVersion.Original]);
			AssertEquals ("RowState1", DataRowState.Modified,
				dtLoad.Rows[1].RowState);
			// asserting Deleted Row2
			AssertEquals ("RowData1-O", "mono 3",
				dtLoad.Rows[2][1, DataRowVersion.Original]);
			AssertEquals ("RowState2", DataRowState.Deleted,
				dtLoad.Rows[2].RowState);
			// asserting Added Row3
			AssertEquals ("RowData3-C", "Add 4",
				dtLoad.Rows[3][1, DataRowVersion.Current]);
			AssertEquals ("RowData3-O", "mono 4",
				dtLoad.Rows[3][1, DataRowVersion.Original]);
			AssertEquals ("RowState3", DataRowState.Modified,
				dtLoad.Rows[3].RowState);
			// asserting Unpresent Row4
			AssertEquals ("RowData4-C", "mono 5",
				dtLoad.Rows[4][1, DataRowVersion.Current]);
			AssertEquals ("RowData4-O", "mono 5",
				dtLoad.Rows[4][1, DataRowVersion.Original]);
			AssertEquals ("RowState4", DataRowState.Unchanged,
				dtLoad.Rows[4].RowState);
		}

		[Test]
		[ExpectedException (typeof (VersionNotFoundException))]
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
			AssertEquals ("RowData2-C", " ",
				dtLoad.Rows[2][1, DataRowVersion.Current]);
		}

		[Test]
		public void Load_RowStateOverwriteChanges () {
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
			AssertEquals ("RowData0-C", "mono 1",
				dtLoad.Rows[0][1, DataRowVersion.Current]);
			AssertEquals ("RowData0-O", "mono 1",
				dtLoad.Rows[0][1, DataRowVersion.Original]);
			AssertEquals ("RowState0", DataRowState.Unchanged,
				dtLoad.Rows[0].RowState);
			// asserting Modified Row1
			AssertEquals ("RowData1-C", "mono 2",
				dtLoad.Rows[1][1, DataRowVersion.Current]);
			AssertEquals ("RowData1-O", "mono 2",
				dtLoad.Rows[1][1, DataRowVersion.Original]);
			AssertEquals ("RowState1", DataRowState.Unchanged,
				dtLoad.Rows[1].RowState);
			// asserting Deleted Row2
			AssertEquals ("RowData1-C", "mono 3",
			        dtLoad.Rows[2][1, DataRowVersion.Current]);
			AssertEquals ("RowData1-O", "mono 3",
				dtLoad.Rows[2][1, DataRowVersion.Original]);
			AssertEquals ("RowState2", DataRowState.Unchanged,
				dtLoad.Rows[2].RowState);
			// asserting Added Row3
			AssertEquals ("RowData3-C", "mono 4",
				dtLoad.Rows[3][1, DataRowVersion.Current]);
			AssertEquals ("RowData3-O", "mono 4",
				dtLoad.Rows[3][1, DataRowVersion.Original]);
			AssertEquals ("RowState3", DataRowState.Unchanged,
				dtLoad.Rows[3].RowState);
			// asserting Unpresent Row4
			AssertEquals ("RowData4-C", "mono 5",
				dtLoad.Rows[4][1, DataRowVersion.Current]);
			AssertEquals ("RowData4-O", "mono 5",
				dtLoad.Rows[4][1, DataRowVersion.Original]);
			AssertEquals ("RowState4", DataRowState.Unchanged,
				dtLoad.Rows[4].RowState);
		}

		[Test]
		public void Load_RowStateUpsert () {
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
			AssertEquals ("RowData0-C", "mono 1",
				dtLoad.Rows[0][1, DataRowVersion.Current]);
			AssertEquals ("RowData0-O", "RowState 1",
				dtLoad.Rows[0][1, DataRowVersion.Original]);
			AssertEquals ("RowState0", DataRowState.Modified,
				dtLoad.Rows[0].RowState);
			// asserting Modified Row1
			AssertEquals ("RowData1-C", "mono 2",
				dtLoad.Rows[1][1, DataRowVersion.Current]);
			AssertEquals ("RowData1-O", "RowState 2",
				dtLoad.Rows[1][1, DataRowVersion.Original]);
			AssertEquals ("RowState1", DataRowState.Modified,
				dtLoad.Rows[1].RowState);
			// asserting Deleted Row2 and "Deleted-Added" Row4
			AssertEquals ("RowData2-O", "RowState 3",
				dtLoad.Rows[2][1, DataRowVersion.Original]);
			AssertEquals ("RowState2", DataRowState.Deleted,
				dtLoad.Rows[2].RowState);
			AssertEquals ("RowData4-C", "mono 3",
				dtLoad.Rows[4][1, DataRowVersion.Current]);
			AssertEquals ("RowState4", DataRowState.Added,
				dtLoad.Rows[4].RowState);
			// asserting Added Row3
			AssertEquals ("RowData3-C", "mono 4",
				dtLoad.Rows[3][1, DataRowVersion.Current]);
			AssertEquals ("RowState3", DataRowState.Added,
				dtLoad.Rows[3].RowState);
			// asserting Unpresent Row5
			// Notice row4 is used for added row of deleted row2 and so
			// unpresent row4 moves to row5
			AssertEquals ("RowData5-C", "mono 5",
				dtLoad.Rows[5][1, DataRowVersion.Current]);
			AssertEquals ("RowState5", DataRowState.Added,
				dtLoad.Rows[5].RowState);
		}

		[Test]
		public void Load_RowStateUpsertDuplicateKey1 () {
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
			AssertEquals ("A-RowState2", DataRowState.Deleted,
				dtLoad.Rows[2].RowState);
			AssertEquals ("A-RowData2-id", 3,
				dtLoad.Rows[2][0, DataRowVersion.Original]);
			AssertEquals ("A-RowData2-name", "RowState 3",
				dtLoad.Rows[2][1, DataRowVersion.Original]);
			AssertEquals ("A-RowState3", DataRowState.Added,
				dtLoad.Rows[3].RowState);
			AssertEquals ("A-RowData3-id", 3,
				dtLoad.Rows[3][0, DataRowVersion.Current]);
			AssertEquals ("A-RowData3-name", "NEWVAL",
				dtLoad.Rows[3][1, DataRowVersion.Current]);
			AssertEquals ("A-RowState4", DataRowState.Added,
				dtLoad.Rows[4].RowState);
			AssertEquals ("A-RowData4-id", 4,
				dtLoad.Rows[4][0, DataRowVersion.Current]);
			AssertEquals ("A-RowData4-name", "mono 4",
				dtLoad.Rows[4][1, DataRowVersion.Current]);

			dtLoad.AcceptChanges ();

			AssertEquals ("B-RowState2", DataRowState.Unchanged,
				dtLoad.Rows[2].RowState);
			AssertEquals ("B-RowData2-id", 3,
				dtLoad.Rows[2][0, DataRowVersion.Current]);
			AssertEquals ("B-RowData2-name", "NEWVAL",
				dtLoad.Rows[2][1, DataRowVersion.Current]);
			AssertEquals ("B-RowState3", DataRowState.Unchanged,
				dtLoad.Rows[3].RowState);
			AssertEquals ("B-RowData3-id", 4,
				dtLoad.Rows[3][0, DataRowVersion.Current]);
			AssertEquals ("B-RowData3-name", "mono 4",
				dtLoad.Rows[3][1, DataRowVersion.Current]);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Load_RowStateUpsertDuplicateKey2 () {
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
			AssertEquals ("RowData4", " ", dtLoad.Rows[4][1]);
		}

		[Test]
		[ExpectedException (typeof (VersionNotFoundException))]
		public void Load_RowStateUpsertDelete1 () {
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
			AssertEquals ("RowData2-C", " ",
				dtLoad.Rows[2][1, DataRowVersion.Current]);
		}

		[Test]
		[ExpectedException (typeof (VersionNotFoundException))]
		public void Load_RowStateUpsertDelete2 () {
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
			AssertEquals ("RowData3-O", " ",
				dtLoad.Rows[3][1, DataRowVersion.Original]);
		}

		[Test]
		[ExpectedException (typeof (VersionNotFoundException))]
		public void Load_RowStateUpsertAdd () {
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
			AssertEquals ("RowData3-O", " ",
				dtLoad.Rows[3][1, DataRowVersion.Original]);
		}

		[Test]
		[ExpectedException (typeof (VersionNotFoundException))]
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
			AssertEquals ("RowData3-O", " ",
				dtLoad.Rows[3][1, DataRowVersion.Original]);
		}

		[Test]
		public void Load_RowStateUpsertUnchangedEqualVal () {
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
			AssertEquals ("RowData0-C", "mono 1",
				dtLoad.Rows[0][1, DataRowVersion.Current]);
			AssertEquals ("RowData0-O", "mono 1",
				dtLoad.Rows[0][1, DataRowVersion.Original]);
			AssertEquals ("RowState0", DataRowState.Unchanged,
				dtLoad.Rows[0].RowState);
		}

		[Test]
		public void LoadDataRow_LoadOptions () {
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
			AssertEquals ("DT72-C", "Changed",
				dr["String2", DataRowVersion.Current]);
			AssertEquals ("DT72-O", "Changed",
				dr["String2", DataRowVersion.Original]);

			// LoadDataRow(update1) - check row state
			AssertEquals ("DT73-LO", DataRowState.Unchanged, dr.RowState);

			//Add New row with LoadOptions = Upsert
			dt.BeginLoadData ();
			dt.LoadDataRow (new object[] { 99, null, "Changed" },
				LoadOption.Upsert);
			dt.EndLoadData ();

			// LoadDataRow(insert1) - check column String2
			dr = dt.Select ("ParentId=99")[0];
			AssertEquals ("DT75-C", "Changed",
				dr["String2", DataRowVersion.Current]);

			// LoadDataRow(insert1) - check row state
			AssertEquals ("DT76-LO", DataRowState.Added, dr.RowState);
		}

		public static DataTable CreateDataTableExample () {
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
		[Category ("NotWorking")]
		public void ReadXmlSchema () {
			DataTable Table = new DataTable ();
			Table.ReadXmlSchema ("Test/System.Data/own_schema1.xsd");

			AssertEquals ("test#02", "test_table", Table.TableName);
			AssertEquals ("test#03", "", Table.Namespace);
			AssertEquals ("test#04", 2, Table.Columns.Count);
			AssertEquals ("test#05", 0, Table.Rows.Count);
			AssertEquals ("test#06", false, Table.CaseSensitive);
			AssertEquals ("test#07", 1, Table.Constraints.Count);
			AssertEquals ("test#08", "", Table.Prefix);

			Constraint cons = Table.Constraints[0];
			AssertEquals ("test#09", "Constraint1", cons.ConstraintName.ToString ());
			AssertEquals ("test#10", "Constraint1", cons.ToString ());

			DataColumn column = Table.Columns[0];
			AssertEquals ("test#11", true, column.AllowDBNull);
			AssertEquals ("test#12", false, column.AutoIncrement);
			AssertEquals ("test#13", 0L, column.AutoIncrementSeed);
			AssertEquals ("test#14", 1L, column.AutoIncrementStep);
			AssertEquals ("test#15", "test", column.Caption);
			AssertEquals ("test#16", "Element", column.ColumnMapping.ToString ());
			AssertEquals ("test#17", "first", column.ColumnName);
			AssertEquals ("test#18", "System.String", column.DataType.ToString ());
			AssertEquals ("test#19", "test_default_value", column.DefaultValue.ToString ());
			AssertEquals ("test#20", false, column.DesignMode);
			AssertEquals ("test#21", "", column.Expression);
			AssertEquals ("test#22", 100, column.MaxLength);
			AssertEquals ("test#23", "", column.Namespace);
			AssertEquals ("test#24", 0, column.Ordinal);
			AssertEquals ("test#25", "", column.Prefix);
			AssertEquals ("test#26", false, column.ReadOnly);
			AssertEquals ("test#27", true, column.Unique);

			DataColumn column2 = Table.Columns[1];
			AssertEquals ("test#28", true, column2.AllowDBNull);
			AssertEquals ("test#29", false, column2.AutoIncrement);
			AssertEquals ("test#30", 0L, column2.AutoIncrementSeed);
			AssertEquals ("test#31", 1L, column2.AutoIncrementStep);
			AssertEquals ("test#32", "second", column2.Caption);
			AssertEquals ("test#33", "Element", column2.ColumnMapping.ToString ());
			AssertEquals ("test#34", "second", column2.ColumnName);
			AssertEquals ("test#35", "System.Data.SqlTypes.SqlGuid", column2.DataType.ToString ());
			AssertEquals ("test#36", "Null", column2.DefaultValue.ToString ());
			AssertEquals ("test#37", false, column2.DesignMode);
			AssertEquals ("test#38", "", column2.Expression);
			AssertEquals ("test#39", -1, column2.MaxLength);
			AssertEquals ("test#40", "", column2.Namespace);
			AssertEquals ("test#41", 1, column2.Ordinal);
			AssertEquals ("test#42", "", column2.Prefix);
			AssertEquals ("test#43", false, column2.ReadOnly);
			AssertEquals ("test#44", false, column2.Unique);

			DataTable Table2 = new DataTable ();
			Table2.ReadXmlSchema ("Test/System.Data/own_schema2.xsd");

			AssertEquals ("test#45", "second_test_table", Table2.TableName);
			AssertEquals ("test#46", "", Table2.Namespace);
			AssertEquals ("test#47", 1, Table2.Columns.Count);
			AssertEquals ("test#48", 0, Table2.Rows.Count);
			AssertEquals ("test#49", false, Table2.CaseSensitive);
			AssertEquals ("test#50", 1, Table2.Constraints.Count);
			AssertEquals ("test#51", "", Table2.Prefix);

			DataColumn column3 = Table2.Columns[0];
			AssertEquals ("test#52", true, column3.AllowDBNull);
			AssertEquals ("test#53", false, column3.AutoIncrement);
			AssertEquals ("test#54", 0L, column3.AutoIncrementSeed);
			AssertEquals ("test#55", 1L, column3.AutoIncrementStep);
			AssertEquals ("test#56", "second_first", column3.Caption);
			AssertEquals ("test#57", "Element", column3.ColumnMapping.ToString ());
			AssertEquals ("test#58", "second_first", column3.ColumnName);
			AssertEquals ("test#59", "System.String", column3.DataType.ToString ());
			AssertEquals ("test#60", "default_value", column3.DefaultValue.ToString ());
			AssertEquals ("test#61", false, column3.DesignMode);
			AssertEquals ("test#62", "", column3.Expression);
			AssertEquals ("test#63", 100, column3.MaxLength);
			AssertEquals ("test#64", "", column3.Namespace);
			AssertEquals ("test#65", 0, column3.Ordinal);
			AssertEquals ("test#66", "", column3.Prefix);
			AssertEquals ("test#67", false, column3.ReadOnly);
			AssertEquals ("test#68", true, column3.Unique);
		}

		[Test]
		public void ReadXmlSchema_2 () {
			DataTable dt = new DataTable ();
			string xmlData = string.Empty;
			xmlData += "<?xml version=\"1.0\"?>";
			xmlData += "<xs:schema id=\"SiteConfiguration\" targetNamespace=\"http://tempuri.org/PortalCfg.xsd\" xmlns:mstns=\"http://tempuri.org/PortalCfg.xsd\" xmlns=\"http://tempuri.org/PortalCfg.xsd\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" attributeFormDefault=\"qualified\" elementFormDefault=\"qualified\">";
			xmlData += "<xs:element name=\"SiteConfiguration\" msdata:IsDataSet=\"true\" msdata:EnforceConstraints=\"False\">";
			xmlData += "<xs:complexType>";
			xmlData += "<xs:choice maxOccurs=\"unbounded\">";
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
		public void ReadXmlSchema_ByStream () {
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
			AssertEquals ("DS270", ds1.Tables[0].Columns.Count, dt1.Columns.Count);

			// ReadXmlSchema - Tables 1 Col count
			AssertEquals ("DS271", ds1.Tables[1].Columns.Count, dt2.Columns.Count);

			//check some colummns types
			// ReadXmlSchema - Tables 0 Col type
			AssertEquals ("DS272", ds1.Tables[0].Columns[0].GetType (), dt1.Columns[0].GetType ());

			// ReadXmlSchema - Tables 1 Col type
			AssertEquals ("DS273", ds1.Tables[1].Columns[3].GetType (), dt2.Columns[3].GetType ());

			//check that no data exists
			// ReadXmlSchema - Table 1 row count
			AssertEquals ("DS274",0, dt1.Rows.Count);

			// ReadXmlSchema - Table 2 row count
			AssertEquals ("DS275",0, dt2.Rows.Count);
		}

		[Test]
		[Category ("NotWorking")]
		public void ReadWriteXmlSchema_ByFileName () {
			string sTempFileName1 = "tmpDataSet_ReadWriteXml_43899-1.xml";
			string sTempFileName2 = "tmpDataSet_ReadWriteXml_43899-2.xml";

			DataSet ds1 = new DataSet ();
			ds1.Tables.Add (DataProvider.CreateParentDataTable ());
			ds1.Tables.Add (DataProvider.CreateChildDataTable ());

			ds1.Tables[0].WriteXmlSchema (sTempFileName1);
			ds1.Tables[1].WriteXmlSchema (sTempFileName2);

			DataTable dt1 = new DataTable ();
			DataTable dt2 = new DataTable ();

			dt1.ReadXmlSchema (sTempFileName1);
			dt2.ReadXmlSchema (sTempFileName2);

			AssertEquals ("DS277", ds1.Tables[0].Columns.Count, dt1.Columns.Count);
			AssertEquals ("DS278", ds1.Tables[1].Columns.Count, dt2.Columns.Count);
			AssertEquals ("DS279", ds1.Tables[0].Columns[0].GetType (), dt1.Columns[0].GetType ());
			AssertEquals ("DS280", ds1.Tables[1].Columns[3].GetType (), dt2.Columns[3].GetType ());
			AssertEquals ("DS281", 0, dt1.Rows.Count);
			AssertEquals ("DS282", 0, dt2.Rows.Count);

			File.Delete (sTempFileName1);
			File.Delete (sTempFileName2);
		}

		[Test]
		public void ReadXmlSchema_ByTextReader () {
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
			AssertEquals ("DS284", ds1.Tables[0].Columns.Count, dt1.Columns.Count);

			// ReadXmlSchema - Tables 1 Col count
			AssertEquals ("DS285", ds1.Tables[1].Columns.Count, dt2.Columns.Count);

			//check some colummns types
			// ReadXmlSchema - Tables 0 Col type
			AssertEquals ("DS286", ds1.Tables[0].Columns[0].GetType (), dt1.Columns[0].GetType ());

			// ReadXmlSchema - Tables 1 Col type
			AssertEquals ("DS287", ds1.Tables[1].Columns[3].GetType (), dt2.Columns[3].GetType ());

			//check that no data exists
			// ReadXmlSchema - Table 1 row count
			AssertEquals ("DS288", 0, dt1.Rows.Count);

			// ReadXmlSchema - Table 2 row count
			AssertEquals ("DS289", 0, dt2.Rows.Count);
		}

		[Test]
		public void ReadXmlSchema_ByXmlReader () {
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
			AssertEquals ("DS291", ds1.Tables[0].Columns.Count, dt1.Columns.Count);

			// ReadXmlSchema - Tables 1 Col count
			AssertEquals ("DS292", ds1.Tables[1].Columns.Count, dt2.Columns.Count);

			//check some colummns types
			// ReadXmlSchema - Tables 0 Col type
			AssertEquals ("DS293", ds1.Tables[0].Columns[0].GetType (), dt1.Columns[0].GetType ());

			// ReadXmlSchema - Tables 1 Col type
			AssertEquals ("DS294", ds1.Tables[1].Columns[3].GetType (), dt2.Columns[3].GetType ());

			//check that no data exists
			// ReadXmlSchema - Table 1 row count
			AssertEquals ("DS295", 0, dt1.Rows.Count);

			// ReadXmlSchema - Table 2 row count
			AssertEquals ("DS296", 0, dt2.Rows.Count);
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteXmlSchema () {
			DataSet ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/region.xml");
			TextWriter writer = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (writer);


			string TextString = GetNormalizedSchema (writer.ToString ());
			//string TextString = writer.ToString ();

			string substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#02", "<xs:schema id=\"Root\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#03", "  <xs:element msdata:IsDataSet=\"true\" msdata:MainDataTable=\"Region\" msdata:UseCurrentLocale=\"true\" name=\"Root\">", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#04", "    <xs:complexType>", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#05", "      <xs:choice maxOccurs=\"unbounded\" minOccurs=\"0\">", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#06", "        <xs:element name=\"Region\">", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#07", "          <xs:complexType>", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#08", "            <xs:sequence>", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#09", "              <xs:element minOccurs=\"0\" name=\"RegionID\" type=\"xs:string\" />", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#10", "              <xs:element minOccurs=\"0\" name=\"RegionDescription\" type=\"xs:string\" />", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#11", "            </xs:sequence>", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#12", "          </xs:complexType>", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#13", "        </xs:element>", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#14", "      </xs:choice>", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#15", "    </xs:complexType>", substring);

			substring = TextString.Substring (0, TextString.IndexOf (EOL));
			TextString = TextString.Substring (TextString.IndexOf (EOL) + EOL.Length);
			AssertEquals ("test#16", "  </xs:element>", substring);

			AssertEquals ("test#17", "</xs:schema>", TextString);
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteXmlSchema2 () {
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

			AssertEquals (xml, result);

			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			xtw.Formatting = Formatting.Indented;
			OriginalDataSet.Tables[0].WriteXmlSchema (xtw);
			result = sw.ToString ();

			result = result.Replace ("\r\n", "\n").Replace ('"', '\'');
			AssertEquals (schema.Replace ("\r\n", "\n"), result);
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteXmlSchema3 () {
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

			AssertEquals (xmlschema.Replace ("\r\n", "\n"), result.Replace ("\r\n", "\n"));
			//AssertEquals (xmlschema, result.Replace ("\r\n", "\n"));
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteXmlSchema4 () {
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

			AssertEquals (xmlschema.Replace ("\r\n", "\n"), result.Replace ("\r\n", "\n"));
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteXmlSchema5 () {
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
			AssertEquals (xmlschema1.Replace ("\r\n", "\n"), result1.Replace ("\r\n", "\n"));

			StringWriter sw2 = new StringWriter ();
			ds.Tables[1].WriteXmlSchema (sw2);
			string result2 = sw2.ToString ();
			AssertEquals (xmlschema2.Replace ("\r\n", "\n"), result2.Replace ("\r\n", "\n"));

			StringWriter sw3 = new StringWriter ();
			ds.Tables[2].WriteXmlSchema (sw3);
			string result3 = sw3.ToString ();
			AssertEquals (xmlschema3.Replace ("\r\n", "\n"), result3.Replace ("\r\n", "\n"));
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteXmlSchema6 () {
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

			AssertEquals (xmlschema.Replace ("\r\n", "\n"), result.Replace ("\r\n", "\n"));
		}

		[Test]
		public void WriteXmlSchema7 () {
			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			ds.Tables.Add (dt);
			dt.Rows.Add (new object[] { "foo", "bar" });
			StringWriter sw = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (sw);
			Assert (sw.ToString ().IndexOf ("xmlns=\"\"") > 0);
		}

		[Test]
		public void WriteXmlSchema_ConstraintNameWithSpaces () {
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
		public void WriteXmlSchema_ForignKeyConstraint () {
			DataSet ds1 = new DataSet ();

			DataTable table1 = ds1.Tables.Add ();
			DataTable table2 = ds1.Tables.Add ();

			DataColumn col1_1 = table1.Columns.Add ("col1", typeof (int));
			DataColumn col2_1 = table2.Columns.Add ("col1", typeof (int));

			table2.Constraints.Add ("fk", col1_1, col2_1);

			StringWriter sw1 = new StringWriter ();
			ds1.Tables[0].WriteXmlSchema (sw1);
			String xml1 = sw1.ToString ();
			Assert ("#1", xml1.IndexOf (@"<xs:unique name=""Constraint1"">") != -1);

			StringWriter sw2 = new StringWriter ();
			ds1.Tables[1].WriteXmlSchema (sw2);
			String xml2 = sw2.ToString ();
			Assert ("#2", xml2.IndexOf (@"<xs:unique name=""Constraint1"">") == -1);
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteXmlSchema_Relations_ForeignKeys () {
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

		        AssertEquals ("#2", 3, dtA.Constraints.Count);
		        AssertEquals ("#3", 2, dtB.Constraints.Count);

			Assert ("#5", dtA.Constraints.Contains ("pk 1"));
			Assert ("#6", dtA.Constraints.Contains ("Constraint1"));
			Assert ("#7", dtA.Constraints.Contains ("Constraint2"));
			Assert ("#9", dtB.Constraints.Contains ("pk 2"));
			Assert ("#10", dtB.Constraints.Contains ("Constraint1"));
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteXmlSchema_DifferentNamespace () {
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
			AssertEquals ("#1", schema, result1);

			StringWriter sw2 = new StringWriter ();
			XmlTextWriter xw2 = new XmlTextWriter (sw2);
			xw2.Formatting = Formatting.Indented;
			xw2.QuoteChar = '\'';
			ds.Tables[0].WriteXmlSchema (xw2);
			string result2 = sw2.ToString ();
			AssertEquals ("#2", schema, result2);
		}

		[Test]
		[Category ("NotWorking")]
		// WriteXmlSchema doesn't have overload wityh 2 parameters in System.Data
		// and is commented-out TWICE below
		public void WriteXmlSchema_Hierarchy () {
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
			//table1.WriteXmlSchema (writer1, false);
			string expected1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:MainDataTable=\"Table1\" msdata:UseCurrentLocale=\"true\">\r\n    <xs:complexType>\r\n      <xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n        <xs:element name=\"Table1\">\r\n          <xs:complexType>\r\n            <xs:sequence>\r\n              <xs:element name=\"ID\" type=\"xs:int\" />\r\n              <xs:element name=\"Name\" type=\"xs:string\" minOccurs=\"0\" />\r\n            </xs:sequence>\r\n          </xs:complexType>\r\n        </xs:element>\r\n      </xs:choice>\r\n    </xs:complexType>\r\n    <xs:unique name=\"Constraint1\" msdata:PrimaryKey=\"true\">\r\n      <xs:selector xpath=\".//Table1\" />\r\n      <xs:field xpath=\"ID\" />\r\n    </xs:unique>\r\n  </xs:element>\r\n</xs:schema>";
			AssertEquals ("#1", expected1, writer1.ToString());

			StringWriter writer2 = new StringWriter ();
			//table1.WriteXmlSchema (writer2, true);
			string expected2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:MainDataTable=\"Table1\" msdata:UseCurrentLocale=\"true\">\r\n    <xs:complexType>\r\n      <xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n        <xs:element name=\"Table1\">\r\n          <xs:complexType>\r\n            <xs:sequence>\r\n              <xs:element name=\"ID\" type=\"xs:int\" />\r\n              <xs:element name=\"Name\" type=\"xs:string\" minOccurs=\"0\" />\r\n            </xs:sequence>\r\n          </xs:complexType>\r\n        </xs:element>\r\n        <xs:element name=\"Table2\">\r\n          <xs:complexType>\r\n            <xs:sequence>\r\n              <xs:element name=\"OrderID\" type=\"xs:int\" />\r\n              <xs:element name=\"CustomerID\" type=\"xs:int\" minOccurs=\"0\" />\r\n              <xs:element name=\"OrderDate\" type=\"xs:dateTime\" minOccurs=\"0\" />\r\n            </xs:sequence>\r\n          </xs:complexType>\r\n        </xs:element>\r\n      </xs:choice>\r\n    </xs:complexType>\r\n    <xs:unique name=\"Constraint1\" msdata:PrimaryKey=\"true\">\r\n      <xs:selector xpath=\".//Table1\" />\r\n      <xs:field xpath=\"ID\" />\r\n    </xs:unique>\r\n    <xs:unique name=\"Table2_Constraint1\" msdata:ConstraintName=\"Constraint1\" msdata:PrimaryKey=\"true\">\r\n      <xs:selector xpath=\".//Table2\" />\r\n      <xs:field xpath=\"OrderID\" />\r\n    </xs:unique>\r\n    <xs:keyref name=\"CustomerOrder\" refer=\"Constraint1\">\r\n      <xs:selector xpath=\".//Table2\" />\r\n      <xs:field xpath=\"CustomerID\" />\r\n    </xs:keyref>\r\n  </xs:element>\r\n</xs:schema>";
			AssertEquals ("#2", expected2, writer2.ToString ());
		}

		[Test]
		[Category ("NotWorking")]
		// WriteXmlSchema doesn't have overload wityh 2 parameters in System.Data
		// and is commented-out TWICE below
		public void ReadWriteXmlSchema () {
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("Test/System.Data/store.xsd");
			// check dataset properties before testing write
			AssertDataSet ("ds", ds, "NewDataSet", 3, 2);
			AssertDataTable ("tab1", ds.Tables[0], "bookstore", 1, 0, 0, 1, 1, 1);
			AssertDataTable ("tab2", ds.Tables[1], "book", 5, 0, 1, 1, 2, 1);
			AssertDataTable ("tab3", ds.Tables[2], "author", 3, 0, 1, 0, 1, 0);
			// FIXME: currently order is not compatible. Use name as index
			AssertDataRelation ("rel1", ds.Relations["book_author"], "book_author", true, new string[] { "book_Id" }, new string[] { "book_Id" }, true, true);
			AssertDataRelation ("rel2", ds.Relations["bookstore_book"], "bookstore_book", true, new string[] { "bookstore_Id" }, new string[] { "bookstore_Id" }, true, true);

			ds.ReadXml ("Test/System.Data/region.xml", XmlReadMode.InferSchema);
			ds.Relations.Clear (); // because can not call WriteXmlSchema with nested relations.

			TextWriter writer1 = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (writer1);
			string TextString1 = GetNormalizedSchema (writer1.ToString ());
			string expected1 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""Root"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">" +
  @"<xs:complexType name=""bookstoreType"">" +
  @"</xs:complexType>" +
  @"<xs:element name=""bookstore"" type=""bookstoreType"" />" +
  @"<xs:element msdata:IsDataSet=""true"" msdata:MainDataTable=""bookstore"" msdata:UseCurrentLocale=""true"" name=""Root"">" +
    @"<xs:complexType>" +
      @"<xs:choice maxOccurs=""unbounded"" minOccurs=""0"">" +
	@"<xs:element ref=""bookstore"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			AssertEquals ("#1", expected1, TextString1.Replace("\r\n","").Replace("  ",""));

			TextWriter writer2 = new StringWriter ();
			//ds.Tables[1].WriteXmlSchema (writer2,false);
			string TextString2 = GetNormalizedSchema (writer2.ToString ());
			string expected2 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""Root"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">" +
  @"<xs:complexType name=""bookType"">" +
    @"<xs:sequence>" +
      @"<xs:element msdata:Ordinal=""1"" name=""title"" type=""xs:string"" />" +
      @"<xs:element msdata:Ordinal=""2"" name=""price"" type=""xs:decimal"" />" +
    @"</xs:sequence>" +
    @"<xs:attribute name=""genre"" type=""xs:string"" />" +
    @"<xs:attribute name=""bookstore_Id"" type=""xs:int"" use=""prohibited"" />" +
  @"</xs:complexType>" +
  @"<xs:element name=""book"" type=""bookType"" />" +
  @"<xs:element msdata:IsDataSet=""true"" msdata:MainDataTable=""book"" msdata:UseCurrentLocale=""true"" name=""Root"">" +
    @"<xs:complexType>" +
      @"<xs:choice maxOccurs=""unbounded"" minOccurs=""0"">" +
	@"<xs:element ref=""book"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			AssertEquals ("#2", expected2, TextString2.Replace ("\r\n", "").Replace ("  ", ""));
			
			TextWriter writer3 = new StringWriter ();
			ds.Tables[2].WriteXmlSchema (writer3);
			string TextString3 = GetNormalizedSchema (writer3.ToString ());
			string expected3 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""Root"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">" +
  @"<xs:complexType name=""authorName"">" +
    @"<xs:sequence>" +
      @"<xs:element msdata:Ordinal=""0"" name=""first-name"" type=""xs:string"" />" +
      @"<xs:element msdata:Ordinal=""1"" name=""last-name"" type=""xs:string"" />" +
    @"</xs:sequence>" +
    @"<xs:attribute name=""book_Id"" type=""xs:int"" use=""prohibited"" />" +
  @"</xs:complexType>" +
  @"<xs:element name=""author"" type=""authorName"" />" +
  @"<xs:element msdata:IsDataSet=""true"" msdata:MainDataTable=""author"" msdata:UseCurrentLocale=""true"" name=""Root"">" +
    @"<xs:complexType>" +
      @"<xs:choice maxOccurs=""unbounded"" minOccurs=""0"">" +
        @"<xs:element ref=""author"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			AssertEquals ("#3", expected3, TextString3.Replace ("\r\n", "").Replace ("  ", ""));
			
			TextWriter writer4 = new StringWriter ();
			ds.Tables[3].WriteXmlSchema (writer4);
			string TextString4 = GetNormalizedSchema (writer4.ToString ());
			string expected4 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""Root"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">" +
  @"<xs:element msdata:IsDataSet=""true"" msdata:MainDataTable=""Region"" msdata:UseCurrentLocale=""true"" name=""Root"">" +
    @"<xs:complexType>" +
      @"<xs:choice maxOccurs=""unbounded"" minOccurs=""0"">" +
        @"<xs:element name=""Region"">" +
          @"<xs:complexType>" +
            @"<xs:sequence>" +
              @"<xs:element minOccurs=""0"" name=""RegionID"" type=""xs:string"" />" +
              @"<xs:element minOccurs=""0"" name=""RegionDescription"" type=""xs:string"" />" +
            @"</xs:sequence>" +
          @"</xs:complexType>" +
        @"</xs:element>" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			AssertEquals ("#4", expected4, TextString4.Replace ("\r\n", "").Replace ("  ", ""));
		}

		[Test]
		[Category ("NotWorking")]
		public void ReadWriteXmlSchema_IgnoreSchema () {
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("Test/System.Data/store.xsd");
			// check dataset properties before testing write
			AssertDataSet ("ds", ds, "NewDataSet", 3, 2);
			AssertDataTable ("tab1", ds.Tables[0], "bookstore", 1, 0, 0, 1, 1, 1);
			AssertDataTable ("tab2", ds.Tables[1], "book", 5, 0, 1, 1, 2, 1);
			AssertDataTable ("tab3", ds.Tables[2], "author", 3, 0, 1, 0, 1, 0);
			// FIXME: currently order is not compatible. Use name as index
			AssertDataRelation ("rel1", ds.Relations["book_author"], "book_author", true, new string[] { "book_Id" }, new string[] { "book_Id" }, true, true);
			AssertDataRelation ("rel2", ds.Relations["bookstore_book"], "bookstore_book", true, new string[] { "bookstore_Id" }, new string[] { "bookstore_Id" }, true, true);

			ds.ReadXml ("Test/System.Data/region.xml", XmlReadMode.IgnoreSchema);
			ds.Relations.Clear (); // because can not call WriteXmlSchema with nested relations.

			TextWriter writer1 = new StringWriter ();
			ds.Tables[0].WriteXmlSchema (writer1);
			string TextString1 = GetNormalizedSchema (writer1.ToString ());
			string expected1 = @"<?xml version=""1.0"" encoding=""utf-16""?>" +
@"<xs:schema id=""NewDataSet"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">" +
  @"<xs:complexType name=""bookstoreType"">" +
  @"</xs:complexType>" +
  @"<xs:element name=""bookstore"" type=""bookstoreType"" />" +
  @"<xs:element msdata:IsDataSet=""true"" msdata:MainDataTable=""bookstore"" msdata:UseCurrentLocale=""true"" name=""NewDataSet"">" +
    @"<xs:complexType>" +
      @"<xs:choice maxOccurs=""unbounded"" minOccurs=""0"">" +
	@"<xs:element ref=""bookstore"" />" +
      @"</xs:choice>" +
    @"</xs:complexType>" +
  @"</xs:element>" +
@"</xs:schema>";
			AssertEquals ("#1", expected1, TextString1.Replace ("\r\n", "").Replace ("  ", ""));

			TextWriter writer2 = new StringWriter ();
			//ds.Tables[1].WriteXmlSchema (writer2, false);
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
			AssertEquals ("#2", expected2, TextString2.Replace ("\r\n", "").Replace ("  ", ""));

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
			AssertEquals ("#3", expected3, TextString3.Replace ("\r\n", "").Replace ("  ", ""));

			TextWriter writer4 = new StringWriter ();
			string expStr = "";
			try {
				ds.Tables[3].WriteXmlSchema (writer4);
			}
			catch (Exception ex) {
				expStr = ex.Message;
			}
			AssertEquals ("#4", "Cannot find table 3.", expStr);
		}

		[Test]
		[Category ("NotWorking")]
		public void ReadWriteXmlSchema_2 () {
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

			AssertEquals ("#1", 0, ds1.Relations.Count);
			AssertEquals ("#2", 1, ds1.Tables[0].Columns.Count);
			AssertEquals ("#3", 1, ds1.Tables[1].Columns.Count);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadWriteXmlSchemaExp_NoRootElmnt () {
			MemoryStream ms = new MemoryStream ();
			DataTable dtr = new DataTable ();
			dtr.ReadXmlSchema (ms);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadWriteXmlSchemaExp_NoTableName () {
			DataTable dtw = new DataTable ();
			MemoryStream ms = new MemoryStream ();
			dtw.WriteXmlSchema (ms);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ReadWriteXmlSchemaExp_TableNameConflict () {
			DataTable dtw = new DataTable ("Table1");
			StringWriter writer1 = new StringWriter ();
			dtw.WriteXmlSchema (writer1);
			DataTable dtr = new DataTable ("Table2");
			StringReader reader1 = new StringReader (writer1.ToString());
			dtr.ReadXmlSchema (reader1);
		}

		#endregion // Read/Write XML Tests

#endif // NET_2_0

	}
                                                                                                    
                                                                                                    
         public  class MyDataTable:DataTable {
                                                                                                    
             public static int count = 0;
                                                                                                    
             public MyDataTable() {
                                                                                                    
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
			Assertion.AssertEquals ("n", 5, n);
		}
#if !TARGET_JVM
		[Test]
		public void NFIFromBug55978 ()
		{
			AppDomain domain = AppDomain.CreateDomain ("testdomain");
			AppDomainsAndFormatInfo test = new AppDomainsAndFormatInfo ();
			test.Remote ();
			domain.DoCallBack (new CrossAppDomainDelegate (test.Remote));
			AppDomain.Unload (domain);
		}
#endif

		[Test]
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
			dv.RowFilter = "StartDate >= '" + DateTime.Now.AddDays (2) + "' and StartDate <= '" + DateTime.Now.AddDays (4) + "'";
			Assertion.AssertEquals ("Table", 10, dt.Rows.Count);
			Assertion.AssertEquals ("View", 2, dv.Count);
		}
	}
}
