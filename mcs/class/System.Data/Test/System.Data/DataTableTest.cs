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
