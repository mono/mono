// DataColumnCollectionTest.cs - NUnit Test Cases for System.Data.DataColumnCollection
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//
// (C) Copyright 2002 Franklin Wise
//

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	public class DataColumnCollectionTest : TestCase
	{
		public DataColumnCollectionTest () : base ("MonoTest.System.Data.DataColumnCollectionTest") {}
		public DataColumnCollectionTest (string name) : base (name) {}

		private DataTable _tbl;

		protected override void SetUp () 
		{
			_tbl = new DataTable();
		}

		protected override void TearDown() {}

		public static ITest Suite {
			get { 
				return new TestSuite (typeof (DataColumnCollectionTest)); 
			}
		}

		//TODO
		public void TestAddValidationExceptions()
		{
			
			//Set DefaultValue and AutoIncr == true
			//And get an exception
		}


		
	}
}
