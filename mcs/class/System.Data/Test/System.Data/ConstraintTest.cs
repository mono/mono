// ConstraintTest.cs - NUnit Test Cases for testing the abstract class System.Data.Constraint
// The tests use an inherited class (UniqueConstraint) to test the Constraint class.
//
// Authors:
//   Franklin Wise <gracenote@earthlink.net>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Franklin Wise
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

#if USE_MSUNITTEST
#if WINDOWS_PHONE || NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
#else // !WINDOWS_PHONE && !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute;
#endif // WINDOWS_PHONE || NETFX_CORE
#else // !USE_MSUNITTEST
using NUnit.Framework;
#endif // USE_MSUNITTEST
using System;
using System.Data;
#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
//	public class MyUniqueConstraint: UniqueConstraint {
//		public MyUniqueConstraint(DataColumn col, bool pk): base(col,pk){}
//		string _myval = "";
//		public override string ConstraintName {
//			get{
//				return _myval;
//				return base.ConstraintName;
//			}
//			set{
//				Console.WriteLine("NameSet = " + value);
//				base.ConstraintName = value;
//				_myval = value;
//			}
//		}
//	}

	[TestFixture]
	public class ConstraintTest
	{
		private DataTable _table;
		private Constraint _constraint1;
		private Constraint _constraint2;

		[SetUp]
		public void GetReady ()
		{

			//Setup DataTable
			_table = new DataTable ("TestTable");

			_table.Columns.Add ("Col1", typeof(int));
			_table.Columns.Add ("Col2", typeof(int));

			//Use UniqueConstraint to test Constraint Base Class
			_constraint1 = new UniqueConstraint (_table.Columns [0], false); 
			_constraint2 = new UniqueConstraint (_table.Columns [1], false); 

			// not sure why this is needed since a new _table was just created
			// for us, but this Clear() keeps the tests from throwing
			// an exception when the Add() is called.
			_table.Constraints.Clear ();
		}  

		[Test]
		public void SetConstraintNameNullOrEmptyExceptions ()
		{
			bool exceptionCaught = false;
			string name = null;

			_table.Constraints.Add (_constraint1);  

			for (int i = 0; i <= 1; i++) {
				exceptionCaught = false;
				if (0 == i)
					name = null;
				if (1 == i)
					name = String.Empty;
	
				try {
				
					//Next line should throw ArgumentException
					//Because ConstraintName can't be set to null
					//or empty while the constraint is part of the
					//collection
					_constraint1.ConstraintName = name; 
				} catch (ArgumentException) { 
					exceptionCaught = true;
				} catch {
					Assert.Fail ("Wrong exception type thrown.");
				}
				
				Assert.IsTrue (exceptionCaught, "Failed to throw exception.");
			}	
		}

		[Test]
		public void SetConstraintNameDuplicateException ()
		{
			_constraint1.ConstraintName = "Dog";
			_constraint2.ConstraintName = "Cat";

			_table.Constraints.Add (_constraint1);
			_table.Constraints.Add (_constraint2);

			//Should throw DuplicateNameException
			AssertHelpers.AssertThrowsException<DuplicateNameException> (() => {
			_constraint2.ConstraintName = "Dog";
			});
		}

		[Test]
		public void ToStringTest ()
		{
			_constraint1.ConstraintName = "Test";
			Assert.AreEqual (_constraint1.ToString (), _constraint1.ConstraintName,
				"ToString is the same as constraint name.");
			
			_constraint1.ConstraintName = null;
			Assert.IsNotNull (_constraint1.ToString (), "ToString should return empty.");
		}

		[Test]
		public void GetExtendedProperties ()
		{
			PropertyCollection col = _constraint1.ExtendedProperties as
				PropertyCollection;

			Assert.IsNotNull (col, "ExtendedProperties returned null or didn't " +
				"return the correct type");
		}
	}
}
