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
using NUnit.Framework;
using System;
using System.Data;
#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif

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
			Assert.That (drcol [0].RelationName, Is.EqualTo ("CustOrder"), "test#1");
			drcol.Clear ();
			
			drcol.Add (parentCol, childCol);
			Assert.That (drcol.Count, Is.EqualTo (1), "test#2");
			drcol.Clear ();
			
			drcol.Add ("NewRelation", parentCol, childCol);
			Assert.That (drcol [0].RelationName, Is.EqualTo ("NewRelation"), "test#3");
			drcol.Clear ();
			
			drcol.Add ("NewRelation", parentCol, childCol, false);
			Assert.That (drcol.Count, Is.EqualTo (1), "test#4");
			drcol.Clear ();
			
			drcol.Add ("NewRelation", parentCol, childCol, true);
			Assert.That (drcol.Count, Is.EqualTo (1), "test#5");
			drcol.Clear ();
		}

		[Test]		
		[ExpectedException(typeof(ArgumentNullException))]
		[Ignore ("It does not pass under MS.NET")]
		public void AddException1 ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation drnull = null;
			drcol.Add (drnull);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void AddException2 ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1 = new DataRelation ("CustOrder"
							, _dataset.Tables ["Customer"].Columns ["custid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.Add (dr1);			
			drcol.Add (dr1);
		}

		[Test]
		[ExpectedException(typeof(DuplicateNameException))]
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
			drcol.Add (dr2);
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
			
			Assert.That (drcol [0].RelationName, Is.EqualTo ("CustOrder"), "test#1");
			Assert.That (drcol [1].RelationName, Is.EqualTo ("ItemOrder"), "test#2");
		}

		[Test]
		public void CanRemove ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables ["Customer"].Columns ["custid"];
			DataColumn childCol = _dataset.Tables ["Order"].Columns ["custid"];
			DataRelation dr = new DataRelation ("CustOrder", parentCol, childCol);
			
			drcol.Add (dr);
			Assert.That (drcol.CanRemove (dr), Is.True, "test#1");
			Assert.That (drcol.CanRemove (null), Is.False, "test#2");
			DataRelation dr2 = new DataRelation ("ItemOrder"
						, _dataset.Tables ["Item"].Columns ["itemid"]
						, _dataset.Tables ["Order"].Columns ["custid"]);
			Assert.That (drcol.CanRemove (dr2), Is.False, "test#3");
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
			Assert.That (drcol.Count, Is.EqualTo (0), "test#1");
		}

		[Test]
		public void Contains ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataColumn parentCol = _dataset.Tables ["Customer"].Columns ["custid"];
			DataColumn childCol = _dataset.Tables ["Order"].Columns ["custid"];
			DataRelation dr = new DataRelation ("CustOrder", parentCol, childCol);
			
			drcol.Add (dr);
			Assert.That (drcol.Contains (dr.RelationName), Is.True, "test#1");
			string drnull = "";
			Assert.That (drcol.Contains (drnull), Is.False, "test#2");
			dr = new DataRelation ("newRelation", childCol, parentCol);
			Assert.That (drcol.Contains ("NoSuchRelation"), Is.False, "test#3");
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
			Assert.That (array.Length, Is.EqualTo (2), "test#1");
			Assert.That (array [0].RelationName, Is.EqualTo ("CustOrder"), "test#2");
			Assert.That (array [1].RelationName, Is.EqualTo ("ItemOrder"), "test#3");
			
			DataRelation [] array1 = new DataRelation[4];
			drcol.CopyTo (array1, 2);
			Assert.That (array1 [0], Is.Null, "test#4");
			Assert.That (array1 [1], Is.Null, "test#5");
			Assert.That (array1 [2].RelationName, Is.EqualTo ("CustOrder"), "test#6");
			Assert.That (array1 [3].RelationName, Is.EqualTo ("ItemOrder"), "test#7");
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

			Assert.That (drcol.Equals (drcol), Is.True, "test#1");
			Assert.That (drcol.Equals (drcol2), Is.True, "test#2");
			
			Assert.That (drcol1.Equals (drcol), Is.False, "test#3");
			Assert.That (drcol.Equals (drcol1), Is.False, "test#4");
			
			Assert.That (Object.Equals (drcol, drcol2), Is.True, "test#5");
			Assert.That (Object.Equals (drcol, drcol1), Is.False, "test#6");
			
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
			
			Assert.That (drcol.IndexOf (dr1), Is.EqualTo (0), "test#1");
			Assert.That (drcol.IndexOf (dr2), Is.EqualTo (1), "test#2");
			
			Assert.That (drcol.IndexOf ("CustOrder"), Is.EqualTo (0), "test#3");
			Assert.That (drcol.IndexOf ("ItemOrder"), Is.EqualTo (1), "test#4");
			
			Assert.That (drcol.IndexOf (drcol [0]), Is.EqualTo (0), "test#5");
			Assert.That (drcol.IndexOf (drcol [1]), Is.EqualTo (1), "test#6");
			
			Assert.That (drcol.IndexOf ("_noRelation_"), Is.EqualTo (-1), "test#7");
			DataRelation newdr = new DataRelation ("newdr"
					, _dataset.Tables ["Customer"].Columns ["custid"]
					, _dataset.Tables ["Order"].Columns ["custid"]);
			
			Assert.That (drcol.IndexOf (newdr), Is.EqualTo (-1), "test#8");
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
			Assert.That (drcol.Contains (dr1.RelationName), Is.False, "test#1");
			drcol.Add (dr1);
			
			drcol.Remove ("CustOrder");
			Assert.That (drcol.Contains ("CustOrder"), Is.False, "test#2");
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
				Assert.That (e, Is.TypeOf (typeof(ArgumentException)), "test#4");
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
			Assert.That (drcol.Contains (dr2.RelationName), Is.False, "test#5");
			drcol.RemoveAt (0);
			Assert.That (drcol.Contains (dr1.RelationName), Is.False, "test#6");
		}
		
		//[Test]
		public void ToStringTest ()
		{
			DataRelationCollection drcol = _dataset.Relations;
			DataRelation dr1 = new DataRelation ("CustOrder"
							, _dataset.Tables ["Customer"].Columns ["custid"]
							, _dataset.Tables ["Order"].Columns ["custid"]);
			drcol.Add (dr1);	
			Assert.That (drcol.ToString (), Is.EqualTo ("System.Data.DataRelationCollection"), "test#1");
			Console.WriteLine (drcol.ToString ());
		}
	}
}
