// DataTableTest.cs - NUnit Test Cases for testing the DataTable 
//
// Authors:
//   Franklin Wise (gracenote@earthlink.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
// 
// (C) Franklin Wise
// (C) 2003 Martin Willemoes Hansen
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

using NUnit.Framework;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataTableTest : Assertion
	{
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
                	
                	DataRow [] Rows = Mom.Select ("Name = 'Teresa'");
                	AssertEquals ("test#01", 2, Rows.Length);
                	
                	Rows = Mom.Select ("Name = 'Teresa' and ChildName = 'Nick'");
                	AssertEquals ("test#02", 0, Rows.Length);

                	Rows = Mom.Select ("Name = 'Teresa' and ChildName = 'Jack'");
                	AssertEquals ("test#03", 1, Rows.Length);

                	Rows = Mom.Select ("Name = 'Teresa' and ChildName <> 'Jack'");
                	AssertEquals ("test#04", "Mack", Rows [0] [1]);
                	
                	Rows = Mom.Select ("Name = 'Teresa' or ChildName <> 'Jack'");
                	AssertEquals ("test#05", 5, Rows.Length);
			
                	Rows = Child.Select ("age = 20 - 1");
                	AssertEquals ("test#06", 1, Rows.Length);
			
                	Rows = Child.Select ("age <= 20");
                	AssertEquals ("test#07", 3, Rows.Length);
			
                	Rows = Child.Select ("age >= 20");
                	AssertEquals ("test#08", 3, Rows.Length);
			
                	Rows = Child.Select ("age >= 20 and name = 'Mack' or name = 'Nick'");
                	AssertEquals ("test#09", 2, Rows.Length);

                	Rows = Child.Select ("age >= 20 and (name = 'Mack' or name = 'Nick')");
                	AssertEquals ("test#10", 1, Rows.Length);
                	AssertEquals ("test#11", "Mack", Rows [0] [0]);
                	
                	Rows = Child.Select ("not (Name = 'Jack')");
                	AssertEquals ("test#12", 5, Rows.Length);
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
		public void ImportRow ()
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
			table.Rows.Add (row);
			table.AcceptChanges ();
                                                                                                     
			row = table.NewRow ();
			row ["Id"] = 47;
			row ["name"] = "Efg";
			table.Rows.Add (row);

			(table.Rows [0]) ["Name"] = "AaBbCc";
		
			table.ImportRow (table.Rows [0]);
			table.ImportRow (table.Rows [1]);

			AssertEquals ("#A01", 147, table.Rows [2]["Id"]);
			AssertEquals ("#A02", 47, table.Rows [3]["Id"]);
			AssertEquals ("#A03", DataRowState.Modified, table.Rows [2].RowState);
			AssertEquals ("#A04", DataRowState.Added, table.Rows [3].RowState);
		}
		[Test]
		public void ClearReset () //To test Clear and Reset methods
		{
			DataTable table = new DataTable ();
			DataTable table1 = new DataTable ();
                
			DataSet set = new DataSet ();
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
                
			DataRow row = table.NewRow ();
			row ["Id"] = 147;
			row ["name"] = "Roopa";
			table.Rows.Add (row);
                
			row = table.NewRow ();
			row ["Id"] = 47;
			row ["Name"] = "roopa";
			table.Rows.Add (row);
                
			try {
				table.Reset ();
				Fail ("#A01");
			}
                        catch (InvalidConstraintException) {
			}
			try {
				table.Clear ();
//#if NET_1_1
//#else
				Fail ("#A03");
//#endif
			}
			catch (Exception e) {

#if NET_1_1
				
				AssertEquals ("#A04", "Cannot clear table Parent because ForeignKeyConstraint DR enforces Child.", e.Message);
				AssertEquals ("#A11", typeof (InvalidConstraintException), e.GetType());
#else
				if (e.GetType () != typeof (AssertionException)) {
					// FIXME: Don't depend on localizable error messages.
					AssertEquals ("#A04", "Cannot clear table Parent because ForeignKeyConstraint DR enforces Child.", e.Message);
				}
				else throw e;
#endif
			}
			table1.Reset ();
			AssertEquals ("#A05", 0, table1.Rows.Count);
			AssertEquals ("#A06", 0, table1.Constraints.Count);
			AssertEquals ("#A07", 0, table1.ParentRelations.Count);
		
			table.Clear ();
			AssertEquals ("#A08", 0, table.Rows.Count);
#if NET_1_1
			AssertEquals ("#A09", 1, table.Constraints.Count);
#else
			AssertEquals ("#A09", 1, table.Constraints.Count);
#endif
			AssertEquals ("#A10", 0, table.ChildRelations.Count);
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
			AssertEquals (5, doc.DocumentElement.FirstChild.ChildNodes.Count);
		}

		 [Test]
                public void CloneSubClassTest()
                {
                        MyDataTable dt1 = new MyDataTable();
                        MyDataTable dt = (MyDataTable)(dt1.Clone());
                        AssertEquals("A#01",2,MyDataTable.count);
                }
                                                                                                    
        }
                                                                                                    
                                                                                                    
         public  class MyDataTable:DataTable {
                                                                                                    
             public static int count = 0;
                                                                                                    
             public MyDataTable() {
                                                                                                    
                    count++;
             }
                                                                                                    
         }

	
}
