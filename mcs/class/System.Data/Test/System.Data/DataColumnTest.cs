// DataColumnTest.cs - NUnit Test Cases for System.Data.DataColumn
//
// Author:
//   Rodrigo Moya <rodrigo@ximian.com>
//
// (C) Copyright 2002 Rodrigo Moya
//

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	public class DataColumnTest : TestCase
	{
		public DataColumnTest () : base ("System.Data.DataColumn") {}
		public DataColumnTest (string name) : base (name) {}

		protected override void SetUp () {}

		protected override void TearDown() {}

		public static ITest Suite {
			get { 
				return new TestSuite (typeof (DataColumnTest)); 
			}
		}

		public void TestBlank()	{} //Remove me when we add some tests
	}
}
