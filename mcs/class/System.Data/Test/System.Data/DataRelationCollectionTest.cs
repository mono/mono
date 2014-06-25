// DataRelationCollection.cs - Nunit Test Cases for for testing the DataRelationCollection
// class
// Author:
//
// 	Punit Kumar Todi ( punit_todi@da-iict.org )
// (C) Punit Todi

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
using System.Diagnostics;
#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{

	[TestFixture]
	public class DataRelationCollectionTest
	{
		DataSet _dataset;
		DataTable _tblparent, _tblchild;
		DataRelation _relation;

		[SetUp]
		public void GetReady ()
		{
			_dataset = new DataSet ();
			_tblparent = new DataTable ("Customer");
			_tblchild = new DataTable ("Order");
			_dataset.Tables.Add (_tblchild);
			_dataset.Tables.Add (_tblparent);
			_dataset.Tables.Add ("Item");
			_dataset.Tables ["Customer"].Columns.Add ("custid");
			_dataset.Tables ["Customer"].Columns.Add ("custname");
			_dataset.Tables ["Order"].Columns.Add ("oid");
			_dataset.Tables ["Order"].Columns.Add ("custid");
			_dataset.Tables ["Order"].Columns.Add ("itemid");
			_dataset.Tables ["Order"].Columns.Add ("desc");
			_dataset.Tables ["Item"].Columns.Add ("itemid");
			_dataset.Tables ["Item"].Columns.Add ("desc");
			
		}

		[TearDown]
		public void Clean ()
		{
			_dataset.Relations.Clear ();
		}

		[Test]
		public void Add ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables ["Customer"].Columns ["custid"];
			DataColumn childCol = _dataset.Tables ["Order"].Columns ["custid"];
			DataRelation dr = new DataRelation ("CustOrder", parentCol, childCol);
			
			drcol.Add (dr);
			Assert.AreEqual ("CustOrder", drcol [0].RelationName, "test#1");
			drcol.Clear ();
			
			drcol.Add (parentCol, childCol);
			Assert.AreEqual (1, drcol.Count, "test#2");
			drcol.Clear ();
			
			drcol.Add ("NewRelation", parentCol, childCol);
			Assert.AreEqual ("NewRelation", drcol [0].RelationName, "test#3");
			drcol.Clear ();
			
			drcol.Add ("NewRelation", parentCol, childCol, false);
			Assert.AreEqual (1, drcol.Count, "test#4");
			drcol.Clear ();
			
			drcol.Add ("NewRelation", parentCol, childCol, true);
			Assert.AreEqual (1, drcol.Count, "test#5");
			drcol.Clear ();
		}

		//It does not pass under MS.NET
		[Test]		
		[Ignore]
		public void AddException1 ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation drnull = null;
			AssertHelpers.AssertThrowsException<ArgumentNullException> (() => {
			drcol.Add (drnull);
			});
		}

		[Test]
		public void AddException2 ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1 = new DataRelation ("CustOrder"
							, _dataset.Tables ["Customer"].Columns ["custid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.Add (dr1);			
			AssertHelpers.AssertThrowsException<ArgumentException> (() => {
			drcol.Add (dr1);
			});
		}

		[Test]
		public void AddException3 ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1 = new DataRelation ("DuplicateName"
							, _dataset.Tables ["Customer"].Columns ["custid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			DataRelation dr2 = new DataRelation ("DuplicateName"
							, _dataset.Tables ["Item"].Columns ["itemid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			
			drcol.Add (dr1);			
			AssertHelpers.AssertThrowsException<DuplicateNameException> (() => {
			drcol.Add (dr2);
			});			
		}

		[Test]
		public void AddRange ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1 = new DataRelation ("CustOrder"
							, _dataset.Tables ["Customer"].Columns ["custid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			DataRelation dr2 = new DataRelation ("ItemOrder"
							, _dataset.Tables ["Item"].Columns ["itemid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.AddRange (new DataRelation[] {dr1,dr2});
			
			Assert.AreEqual ("CustOrder", drcol [0].RelationName, "test#1");
			Assert.AreEqual ("ItemOrder", drcol [1].RelationName, "test#2");
		}

		[Test]
		public void CanRemove ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables ["Customer"].Columns ["custid"];
			DataColumn childCol = _dataset.Tables ["Order"].Columns ["custid"];
			DataRelation dr = new DataRelation ("CustOrder", parentCol, childCol);
			
			drcol.Add (dr);
			Assert.IsTrue (drcol.CanRemove (dr), "test#1");
			Assert.IsFalse (drcol.CanRemove (null), "test#2");
			DataRelation dr2 = new DataRelation ("ItemOrder"
						, _dataset.Tables ["Item"].Columns ["itemid"]
						, _dataset.Tables ["Order"].Columns ["custid"]);
			Assert.IsFalse (drcol.CanRemove (dr2), "test#3");
		}

		[Test]
		public void Clear ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables ["Customer"].Columns ["custid"];
			DataColumn childCol = _dataset.Tables ["Order"].Columns ["custid"];
			drcol.Add (new DataRelation ("CustOrder", parentCol, childCol));
			drcol.Add ("ItemOrder", _dataset.Tables ["Item"].Columns ["itemid"]
								 , _dataset.Tables ["Order"].Columns ["itemid"]);
			drcol.Clear ();
			Assert.AreEqual (0, drcol.Count, "test#1");
		}

		[Test]
		public void Contains ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables ["Customer"].Columns ["custid"];
			DataColumn childCol = _dataset.Tables ["Order"].Columns ["custid"];
			DataRelation dr = new DataRelation ("CustOrder", parentCol, childCol);
			
			drcol.Add (dr);
			Assert.IsTrue (drcol.Contains (dr.RelationName), "test#1");
			string drnull = "";
			Assert.IsFalse (drcol.Contains (drnull), "test#2");
			dr = new DataRelation ("newRelation", childCol, parentCol);
			Assert.IsFalse (drcol.Contains ("NoSuchRelation"), "test#3");
		}

		[Test]
		public void CopyTo ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			drcol.Add ("CustOrder"
					, _dataset.Tables ["Customer"].Columns ["custid"]
					, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.Add ("ItemOrder"
					, _dataset.Tables ["Item"].Columns ["itemid"]
					, _dataset.Tables ["Order"].Columns ["custid"]);
			
			DataRelation [] array = new DataRelation[2];
			drcol.CopyTo (array, 0);
			Assert.AreEqual (2, array.Length, "test#1");
			Assert.AreEqual ("CustOrder", array [0].RelationName, "test#2");
			Assert.AreEqual ("ItemOrder", array [1].RelationName, "test#3");
			
			DataRelation [] array1 = new DataRelation[4];
			drcol.CopyTo (array1, 2);
			Assert.IsNull (array1 [0], "test#4");
			Assert.IsNull (array1 [1], "test#5");
			Assert.AreEqual ("CustOrder", array1 [2].RelationName, "test#6");
			Assert.AreEqual ("ItemOrder", array1 [3].RelationName, "test#7");
		}

		[Test]
		public void Equals ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			drcol.Add ("CustOrder"
					, _dataset.Tables ["Customer"].Columns ["custid"]
					, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.Add ("ItemOrder"
					, _dataset.Tables ["Item"].Columns ["itemid"]
					, _dataset.Tables ["Order"].Columns ["custid"]);
			DataSet newds = new DataSet ();
			DataRelationCollection drcol1 = newds.Relations;
			DataRelationCollection drcol2 = _dataset.Relations;

			Assert.IsTrue (drcol.Equals (drcol), "test#1");
			Assert.IsTrue (drcol.Equals (drcol2), "test#2");
			
			Assert.IsFalse (drcol1.Equals (drcol), "test#3");
			Assert.IsFalse (drcol.Equals (drcol1), "test#4");
			
			Assert.IsTrue (Object.Equals (drcol, drcol2), "test#5");
			Assert.IsFalse (Object.Equals (drcol, drcol1), "test#6");
			
		}

		[Test]
		public void IndexOf ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1 = new DataRelation ("CustOrder"
							, _dataset.Tables ["Customer"].Columns ["custid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			DataRelation dr2 = new DataRelation ("ItemOrder"
							, _dataset.Tables ["Item"].Columns ["itemid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.Add (dr1);
			drcol.Add (dr2);
			
			Assert.AreEqual (0, drcol.IndexOf (dr1), "test#1");
			Assert.AreEqual (1, drcol.IndexOf (dr2), "test#2");
			
			Assert.AreEqual (0, drcol.IndexOf ("CustOrder"), "test#3");
			Assert.AreEqual (1, drcol.IndexOf ("ItemOrder"), "test#4");
			
			Assert.AreEqual (0, drcol.IndexOf (drcol [0]), "test#5");
			Assert.AreEqual (1, drcol.IndexOf (drcol [1]), "test#6");
			
			Assert.AreEqual (-1, drcol.IndexOf ("_noRelation_"), "test#7");
			DataRelation newdr = new DataRelation ("newdr"
					, _dataset.Tables ["Customer"].Columns ["custid"]
					, _dataset.Tables ["Order"].Columns ["custid"]);
			
			Assert.AreEqual (-1, drcol.IndexOf (newdr), "test#8");
		}

		[Test]
		public void Remove ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1 = new DataRelation ("CustOrder"
							, _dataset.Tables ["Customer"].Columns ["custid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			DataRelation dr2 = new DataRelation ("ItemOrder"
							, _dataset.Tables ["Item"].Columns ["itemid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.Add (dr1);
			drcol.Add (dr2);
			
			drcol.Remove (dr1);
			Assert.IsFalse (drcol.Contains (dr1.RelationName), "test#1");
			drcol.Add (dr1);
			
			drcol.Remove ("CustOrder");
			Assert.IsFalse (drcol.Contains ("CustOrder"), "test#2");
			drcol.Add (dr1);
			
			DataRelation drnull = null;
			drcol.Remove (drnull);
			
			DataRelation newdr = new DataRelation ("newdr"
								, _dataset.Tables ["Customer"].Columns ["custid"]
								, _dataset.Tables ["Order"].Columns ["custid"]);
			try {
				drcol.Remove (newdr);
				Assert.Fail ("Err: removed relation which not part of collection");
			} catch (Exception e) {
				AssertHelpers.AssertIsInstanceOfType<ArgumentException>(e, "test#4");
			}
			try {
				drcol.Remove ("newdr");
				Assert.Fail ("Err: removed relation which not part of collection");
			} catch (ArgumentException e) {
			}
		}

		[Test]
		public void RemoveAt ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1 = new DataRelation ("CustOrder"
							, _dataset.Tables ["Customer"].Columns ["custid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			DataRelation dr2 = new DataRelation ("ItemOrder"
							, _dataset.Tables ["Item"].Columns ["itemid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.Add (dr1);
			drcol.Add (dr2);

			try {
				drcol.RemoveAt (-1);
				Assert.Fail ("the index was out of bound: must have failed");
			} catch (IndexOutOfRangeException e) {
			}
			try {
				drcol.RemoveAt (101);
				Assert.Fail ("the index was out of bound: must have failed");
			} catch (IndexOutOfRangeException e) {
			}
			
			drcol.RemoveAt (1);
			Assert.IsFalse (drcol.Contains (dr2.RelationName), "test#5");
			drcol.RemoveAt (0);
			Assert.IsFalse (drcol.Contains (dr1.RelationName), "test#6");
		}
		
		//[Test]
		public void ToStringTest ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1 = new DataRelation ("CustOrder"
							, _dataset.Tables ["Customer"].Columns ["custid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.Add (dr1);	
			Assert.AreEqual ("System.Data.DataRelationCollection", drcol.ToString (), "test#1");
			Debug.WriteLine (drcol.ToString ());
		}
	}
}
