// DataRowCollectionTest.cs - NUnit Test Cases for System.DataRowCollection
//
// Franklin Wise (gracenote@earthlink.net)
//
// (C) Copyright 2002 Franklin Wise
// 


using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{

	public class DataRowCollectionTest : TestCase 
	{
	
		public DataRowCollectionTest() : base ("MonoTests.System.Data.DataRowCollectionTest") {}
		public DataRowCollectionTest(string name) : base(name) {}

		private DataTable _tbl;	

		protected override void SetUp()
		{
			_tbl = new DataTable();
		}

		protected override void TearDown() {}

		public static ITest Suite 
		{
			get 
			{ 
				return new TestSuite(typeof(DataRowCollectionTest)); 
			}
		}

		//FINISHME
		public void TestAutoIncrement()
		{
			DataColumn col = new DataColumn();
			col.AutoIncrement = true;
			col.AutoIncrementSeed = 0;
			col.AutoIncrementStep = 1;
			
			_tbl.Columns.Add(col);
			_tbl.Rows.Add(_tbl.NewRow());

			//Assertion.AssertEquals("Inc 0" , 0, Convert.ToInt32(_tbl.Rows[0]["Auto"] ));
				
			_tbl.Rows.Add(_tbl.NewRow());
			//Assertion.AssertEquals("Inc 1" , 1, Convert.ToInt32(_tbl.Rows[0]["Auto"] ));
		}
	}
}
