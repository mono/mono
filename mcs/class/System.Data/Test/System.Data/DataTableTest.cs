// DataTableTest.cs - NUnit Test Cases for testing the DataTable 
//
// Authors:
//   Franklin Wise (gracenote@earthlink.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
// 
// (C) Franklin Wise
// (C) 2003 Martin Willemoes Hansen
// 

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataTableTest
	{
		[Test]
		public void Ctor()
		{
			DataTable dt = new DataTable();

			Assertion.AssertEquals("CaseSensitive must be false." ,false,dt.CaseSensitive);
			Assertion.Assert("Col",dt.Columns != null);
			//Assertion.Assert(dt.ChildRelations != null);
			Assertion.Assert("Const", dt.Constraints != null);
			Assertion.Assert("ds", dt.DataSet == null); 
			Assertion.Assert("dv", dt.DefaultView != null);
			Assertion.Assert("de", dt.DisplayExpression == "");
			Assertion.Assert("ep", dt.ExtendedProperties != null);
			Assertion.Assert("he", dt.HasErrors == false);
			Assertion.Assert("lc", dt.Locale != null);
			Assertion.Assert("mc", dt.MinimumCapacity == 50); //LAMESPEC:
			Assertion.Assert("ns", dt.Namespace == "");
			//Assertion.Assert(dt.ParentRelations != null);
			Assertion.Assert("pf", dt.Prefix == "");
			Assertion.Assert("pk", dt.PrimaryKey != null);
			Assertion.Assert("rows", dt.Rows != null);
			Assertion.Assert("Site", dt.Site == null);
			Assertion.Assert("tname", dt.TableName == "");
			
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
                	Assertion.AssertEquals ("test#01", 2, Rows.Length);
                	
                	Rows = Mom.Select ("Name = 'Teresa' and ChildName = 'Nick'");
                	Assertion.AssertEquals ("test#02", 0, Rows.Length);

                	Rows = Mom.Select ("Name = 'Teresa' and ChildName = 'Jack'");
                	Assertion.AssertEquals ("test#03", 1, Rows.Length);

                	Rows = Mom.Select ("Name = 'Teresa' and ChildName <> 'Jack'");
                	Assertion.AssertEquals ("test#04", "Mack", Rows [0] [1]);
                	
                	Rows = Mom.Select ("Name = 'Teresa' or ChildName <> 'Jack'");
                	Assertion.AssertEquals ("test#05", 5, Rows.Length);
			
                	Rows = Child.Select ("age = 20 - 1");
                	Assertion.AssertEquals ("test#06", 1, Rows.Length);
			
                	Rows = Child.Select ("age <= 20");
                	Assertion.AssertEquals ("test#07", 3, Rows.Length);
			
                	Rows = Child.Select ("age >= 20");
                	Assertion.AssertEquals ("test#08", 3, Rows.Length);
			
                	Rows = Child.Select ("age >= 20 and name = 'Mack' or name = 'Nick'");
                	Assertion.AssertEquals ("test#09", 2, Rows.Length);

                	Rows = Child.Select ("age >= 20 and (name = 'Mack' or name = 'Nick')");
                	Assertion.AssertEquals ("test#10", 1, Rows.Length);
                	Assertion.AssertEquals ("test#11", "Mack", Rows [0] [0]);
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
                	Assertion.AssertEquals ("test#01", 3, Rows.Length);
                	Assertion.AssertEquals ("test#02", "Mack", Rows [0] [0]);
                	Assertion.AssertEquals ("test#03", "Mick", Rows [1] [0]);                	
                	Assertion.AssertEquals ("test#04", "Dick", Rows [2] [0]);                	
                	
                	Rows = Child.Select ("age >= 20", "age asc");
                	Assertion.AssertEquals ("test#05", 3, Rows.Length);
                	Assertion.AssertEquals ("test#06", "Dick", Rows [0] [0]);
                	Assertion.AssertEquals ("test#07", "Mick", Rows [1] [0]);                	
                	Assertion.AssertEquals ("test#08", "Mack", Rows [2] [0]);                	
                
                	Rows = Child.Select ("age >= 20", "name asc");
                	Assertion.AssertEquals ("test#09", 3, Rows.Length);
                	Assertion.AssertEquals ("test#10", "Dick", Rows [0] [0]);
                	Assertion.AssertEquals ("test#11", "Mack", Rows [1] [0]);                	
                	Assertion.AssertEquals ("test#12", "Mick", Rows [2] [0]);                	

                	Rows = Child.Select ("age >= 20", "name desc");
                	Assertion.AssertEquals ("test#09", 3, Rows.Length);
                	Assertion.AssertEquals ("test#10", "Mick", Rows [0] [0]);
                	Assertion.AssertEquals ("test#11", "Mack", Rows [1] [0]);                	
                	Assertion.AssertEquals ("test#12", "Dick", Rows [2] [0]);                	

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

			Assertion.AssertEquals ("test#01", 12, T.Select ("age<=10").Length);
			
			Assertion.AssertEquals ("test#02", 12, T.Select ("age\n\t<\n\t=\t\n10").Length);

			try {
				T.Select ("name = 1human ");
				Assertion.Fail ("test#03");
			} catch (Exception e) {
				
				// missing operand after 'human' operand 
				Assertion.AssertEquals ("test#04", typeof (SyntaxErrorException), e.GetType ());				
			}
			
			try {			
				T.Select ("name = 1");
				Assertion.Fail ("test#05");
			} catch (Exception e) {
				
				// Cannot perform '=' operation between string and Int32
				Assertion.AssertEquals ("test#06", typeof (EvaluateException), e.GetType ());
			}
			
			Assertion.AssertEquals ("test#07", 1, T.Select ("age = '13'").Length);

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
			
			Assertion.AssertEquals ("test#01", 11, T.Select ("age < 10").Length);
			Assertion.AssertEquals ("test#02", 12, T.Select ("age <= 10").Length);			
			Assertion.AssertEquals ("test#03", 12, T.Select ("age< =10").Length);			
			Assertion.AssertEquals ("test#04", 89, T.Select ("age > 10").Length);
			Assertion.AssertEquals ("test#05", 90, T.Select ("age >= 10").Length);			
			Assertion.AssertEquals ("test#06", 100, T.Select ("age <> 10").Length);
			Assertion.AssertEquals ("test#07", 3, T.Select ("name < 'human10'").Length);
			Assertion.AssertEquals ("test#08", 3, T.Select ("id < '10'").Length);
			// FIXME: Somebody explain how this can be possible.
			// it seems that it is no matter between 10 - 30. The
			// result is allways 25 :-P
			Assertion.AssertEquals ("test#09", 25, T.Select ("id < 10").Length);
			
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
				Assertion.Fail ("test#01");
			} catch (Exception e) {
				
				// column name human not found
				Assertion.AssertEquals ("test#02", typeof (EvaluateException), e.GetType ());
			}
			
			Assertion.AssertEquals ("test#04", 1, T.Select ("id = '12'").Length);
			Assertion.AssertEquals ("test#05", 1, T.Select ("id = 12").Length);
			
			try {
				T.Select ("id = 1k3");
				Assertion.Fail ("test#06");
			} catch (Exception e) {
				
				// no operands after k3 operator
				Assertion.AssertEquals ("test#07", typeof (SyntaxErrorException), e.GetType ());
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
					
			Assertion.AssertEquals ("test#01", 1, T.Select ("name = 'human' + 1").Length);
			
			Assertion.AssertEquals ("test#02", "human1", T.Select ("name = 'human' + 1") [0] ["name"]);			
			Assertion.AssertEquals ("test#03", 1, T.Select ("name = 'human' + '1'").Length);
			Assertion.AssertEquals ("test#04", "human1", T.Select ("name = 'human' + '1'") [0] ["name"]);			
			Assertion.AssertEquals ("test#05", 1, T.Select ("name = 'human' + 1 + 2").Length);
			Assertion.AssertEquals ("test#06", "human12", T.Select ("name = 'human' + '1' + '2'") [0] ["name"]);
			
			Assertion.AssertEquals ("test#07", 1, T.Select ("name = 'huMAn' + 1").Length);
			
			Set.CaseSensitive = true;
			Assertion.AssertEquals ("test#08", 0, T.Select ("name = 'huMAn' + 1").Length);
			
			T.CaseSensitive = false;
			Assertion.AssertEquals ("test#09", 1, T.Select ("name = 'huMAn' + 1").Length);
			
			T.CaseSensitive = true;
			Assertion.AssertEquals ("test#10", 0, T.Select ("name = 'huMAn' + 1").Length);
			
			Set.CaseSensitive = false;
			Assertion.AssertEquals ("test#11", 0, T.Select ("name = 'huMAn' + 1").Length);
			
			T.CaseSensitive = false;
			Assertion.AssertEquals ("test#12", 1, T.Select ("name = 'huMAn' + 1").Length);
			
			Assertion.AssertEquals ("test#13", 0, T.Select ("name = 'human1*'").Length);
			Assertion.AssertEquals ("test#14", 11, T.Select ("name like 'human1*'").Length);
			Assertion.AssertEquals ("test#15", 11, T.Select ("name like 'human1%'").Length);
			
			try {
				Assertion.AssertEquals ("test#16", 11, T.Select ("name like 'h*an1'").Length);
				Assertion.Fail ("test#16");
			} catch (Exception e) {
				
				// 'h*an1' is invalid
				Assertion.AssertEquals ("test#17", typeof (EvaluateException), e.GetType ());
			}
			
			try {
				Assertion.AssertEquals ("test#18", 11, T.Select ("name like 'h%an1'").Length);
				Assertion.Fail ("test#19");
			} catch (Exception e) {
				
				// 'h%an1' is invalid
				Assertion.AssertEquals ("test#20", typeof (EvaluateException), e.GetType ());
			}
			
			Assertion.AssertEquals ("test#21", 0, T.Select ("name like 'h[%]an'").Length);
			Assertion.AssertEquals ("test#22", 1, T.Select ("name like 'h[*]an'").Length);
			
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
			
			Assertion.AssertEquals ("test#01", 1000, T.Select ("Sum(age) > 10").Length);
			Assertion.AssertEquals ("test#02", 1000, T.Select ("avg(age) = 499").Length);
			Assertion.AssertEquals ("test#03", 1000, T.Select ("min(age) = 0").Length);
			Assertion.AssertEquals ("test#04", 1000, T.Select ("max(age) = 999").Length);
			Assertion.AssertEquals ("test#05", 1000, T.Select ("count(age) = 1000").Length);
			Assertion.AssertEquals ("test#06", 1000, T.Select ("stdev(age) > 287 and stdev(age) < 289").Length);
			Assertion.AssertEquals ("test#07", 1000, T.Select ("var(age) < 83417 and var(age) > 83416").Length);
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
			Assertion.AssertEquals ("test#01", 25, T.Select ("age = 5*5") [0]["age"]);			
			Assertion.AssertEquals ("test#02", 901, T.Select ("len(name) > 7").Length);
			Assertion.AssertEquals ("test#03", 125, T.Select ("age = 5*5*5 AND len(name)>7") [0]["age"]);
			Assertion.AssertEquals ("test#04", 1, T.Select ("isnull(id, 'test') = 'test'").Length);			
			Assertion.AssertEquals ("test#05", 1000, T.Select ("iif(id = '56', 'test', 'false') = 'false'").Length);			
			Assertion.AssertEquals ("test#06", 1, T.Select ("iif(id = '56', 'test', 'false') = 'test'").Length);
			Assertion.AssertEquals ("test#07", 9, T.Select ("substring(id, 2, 3) = '23'").Length);
			Assertion.AssertEquals ("test#08", "123", T.Select ("substring(id, 2, 3) = '23'") [0] ["id"]);
			Assertion.AssertEquals ("test#09", "423", T.Select ("substring(id, 2, 3) = '23'") [3] ["id"]);
			Assertion.AssertEquals ("test#10", "923", T.Select ("substring(id, 2, 3) = '23'") [8] ["id"]);
			
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
			Assertion.AssertEquals ("test#01", 6, Rows.Length);
			Rows = Child.Select ("Parent.childname = 'Jack'");
			Assertion.AssertEquals ("test#02", 1, Rows.Length);
			
			try {
				// FIXME: LAMESPEC: Why the exception is thrown why... why... 
				Mom.Select ("Child.Name = 'Jack'");
				Assertion.Fail ("test#03");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#04", typeof (SyntaxErrorException), e.GetType ());
				Assertion.AssertEquals ("test#05", "Cannot interpret token 'Child' at position 1.", e.Message);
			}
			
			Rows = Child.Select ("Parent.name = 'Laura'");
			Assertion.AssertEquals ("test#06", 3, Rows.Length);
			
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
				Assertion.Fail ("test#07");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#08", typeof (EvaluateException), e.GetType ());
				Assertion.AssertEquals ("test#09", "The table [Child] involved in more than one relation. You must explicitly mention a relation name in the expression 'parent.[ChildName]'.", e.Message);
			}
			
			Rows = Child.Select ("Parent(rel).ChildName = 'Jack'");
			Assertion.AssertEquals ("test#10", 1, Rows.Length);

			Rows = Child.Select ("Parent(Rel2).ChildName = 'Jack'");
			Assertion.AssertEquals ("test#10", 1, Rows.Length);
			
			try {
			     	Mom.Select ("Parent.name  = 'John'");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#11", typeof (IndexOutOfRangeException), e.GetType ());
				Assertion.AssertEquals ("test#12", "Cannot find relation 0.", e.Message);
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
			Assertion.AssertEquals(cmpr,dt.ToString());
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
			
			Assertion.AssertEquals ("test#01", 0, dt.PrimaryKey.Length);
			
			dt.PrimaryKey = new DataColumn [] {dt.Columns [0]};
			Assertion.AssertEquals ("test#02", 1, dt.PrimaryKey.Length);
			Assertion.AssertEquals ("test#03", "Column1", dt.PrimaryKey [0].ColumnName);
			
			dt.PrimaryKey = null;
			Assertion.AssertEquals ("test#04", 0, dt.PrimaryKey.Length);
			
			Col = new DataColumn ("failed");
			
			try {
				dt.PrimaryKey = new DataColumn [] {Col};
				Assertion.Fail ("test#05");					
			} catch (Exception e) {
				Assertion.AssertEquals ("test#06", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#07", "Column must belong to a table.", e.Message);
			}
			
			DataTable dt2 = new DataTable ();
			dt2.Columns.Add ();
			
			try {
				dt.PrimaryKey = new DataColumn [] {dt2.Columns [0]};
				Assertion.Fail ("test#08");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#09", typeof (ArgumentException), e.GetType ());
				Assertion.AssertEquals ("test#10", "PrimaryKey columns do not belong to this table.", e.Message);
			}
			
			Assertion.AssertEquals ("test#11", 0, dt.Constraints.Count);
			
			dt.PrimaryKey = new DataColumn [] {dt.Columns [0], dt.Columns [1]};
			Assertion.AssertEquals ("test#12", 2, dt.PrimaryKey.Length);
			Assertion.AssertEquals ("test#13", 1, dt.Constraints.Count);
			Assertion.AssertEquals ("test#14", true, dt.Constraints [0] is UniqueConstraint);
			Assertion.AssertEquals ("test#15", "Column1", dt.PrimaryKey [0]);
			Assertion.AssertEquals ("test#16", "Column2", dt.PrimaryKey [1]);
		}

	}
}
