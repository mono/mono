// DataTableTest.cs - NUnit Test Cases for testing the DataTable 
//
// Franklin Wise (gracenote@earthlink.net)
// 
// (C) Franklin Wise
// 

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{

	public class DataTableTest : TestCase 
	{
	
		public DataTableTest() : base ("MonoTest.System.Data.DataTableTest") {}
		public DataTableTest(string name) : base(name) {}

		protected override void SetUp() {}

		protected override void TearDown() {}

		public static ITest Suite 
		{
			get 
			{ 
				return new TestSuite(typeof(DataTableTest)); 
			}
		}

		public void TestCtor()
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

                public void TestSelect ()
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

                }
                
                public void TestSelect2 ()
                {
			DataSet Set = new DataSet ();
			DataTable Child = new DataTable ("Child");						
			//Set.Tables.Add (Child);
						
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

		public void TestSelectExceptions ()
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
		
		public void TestSelectStringOperators ()
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

		public void TestSelectAggregates ()
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
		
		public void TestSelectFunctions ()
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

		public void TestToString()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("Col1",typeof(int));
			
			dt.TableName = "Myzable";
			dt.DisplayExpression = "Col1";
			
			
			string cmpr = dt.TableName + " " + dt.DisplayExpression;
			Assertion.AssertEquals(cmpr,dt.ToString());
		}
	}
}
