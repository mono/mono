// DataRowTest.cs - NUnit Test Cases for System.DataRow
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

	public class DataRowTest : TestCase 
	{
	
		public DataRowTest() : base ("MonoTests.System.Data.DataRowTest") {}
		public DataRowTest(string name) : base(name) {}

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
				return new TestSuite(typeof(DataRowTest)); 
			}
		}

		//Remove when real tests are added.
		public void TestRemoveMe(){}
	}
}
