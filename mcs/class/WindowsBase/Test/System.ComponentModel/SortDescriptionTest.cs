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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Brian O'Keefe (zer0keefie@gmail.com)
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Reflection;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class SortDescriptionTest
	{
		public SortDescriptionTest()
		{
		}

		[Test]
		public void ConstructorTest()
		{
			string propertyName = "SampleProperty";
			SortDescription sd = new SortDescription (propertyName, ListSortDirection.Ascending);

			Assert.AreEqual (propertyName, sd.PropertyName, "CTOR_#1");
			Assert.AreEqual (ListSortDirection.Ascending, sd.Direction, "CTOR_#2");
			Assert.IsFalse (sd.IsSealed, "CTOR_#3");

			sd = new SortDescription (propertyName, ListSortDirection.Descending);

			Assert.AreEqual (ListSortDirection.Descending, sd.Direction, "CTOR_#3");

			sd.Direction = ListSortDirection.Ascending;
			Assert.AreEqual (ListSortDirection.Ascending, sd.Direction, "CTOR_#4");

			sd.PropertyName = "NewProperty";
			Assert.AreEqual("NewProperty", sd.PropertyName, "CTOR_#5");
		}

		[Test]
		public void NullArgumentTest() {
			SortDescription sd = new SortDescription(null, ListSortDirection.Ascending);
			Assert.IsNull(sd.PropertyName, "NullArg_#1");
		}
		
		[Test]
		public void EmptyArgumentTest() {
			SortDescription sd = new SortDescription(string.Empty, ListSortDirection.Ascending);
			Assert.AreEqual(string.Empty, sd.PropertyName, "EmptyArg_#1");
		}
		
		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void InvalidEnumArgumentTest() {
			new SortDescription("Test", (ListSortDirection)3);
		}

		[Test]
		public void NullArgumentAssignmentTest() {
			SortDescription sd = new SortDescription("Test", ListSortDirection.Ascending);
			sd.PropertyName = null;

			Assert.IsNull(sd.PropertyName, "AssignNull_#1");
		}

		[Test]
		public void EmptyArgumentAssignmentTest() {
			SortDescription sd = new SortDescription("Test", ListSortDirection.Ascending);
			sd.PropertyName = string.Empty;

			Assert.AreEqual(string.Empty, sd.PropertyName, "AssignEmpty_#1");
		}

		[Test]
		public void OperatorTest()
		{
			SortDescription left = new SortDescription ("A", ListSortDirection.Ascending);
			SortDescription same = new SortDescription ("A", ListSortDirection.Ascending);
			SortDescription diffProp = new SortDescription ("B", ListSortDirection.Ascending);
			SortDescription diffDir = new SortDescription ("A", ListSortDirection.Descending);

			Assert.IsTrue (left == same, "OP_#1");
			Assert.IsFalse (left == diffProp, "OP_#2");
			Assert.IsFalse (left == diffDir, "OP_#3");
			Assert.IsFalse (left == null, "OP_#4");

			Assert.IsFalse (left != same, "OP_#5");
			Assert.IsTrue (left != diffProp, "OP_#6");
			Assert.IsTrue (left != diffDir, "OP_#7");
			Assert.IsTrue (left != null, "OP_#8");

			Assert.IsTrue (left.Equals (same), "OP_#9");
			Assert.IsFalse (left.Equals (diffProp), "OP_#10");
			Assert.IsFalse (left.Equals (diffDir), "OP_#11");
			Assert.IsFalse (left.Equals (null), "OP_#12");
		}

		[Test]
		public void ToStringAndHashCodeTest()
		{
			SortDescription sd = new SortDescription ("Sample", ListSortDirection.Ascending);

			Assert.AreEqual ("System.ComponentModel.SortDescription", sd.ToString (), "TSHC_#1");
			Assert.AreEqual ("Sample".GetHashCode () ^ ListSortDirection.Ascending.GetHashCode(),
					 sd.GetHashCode (), "TSHC_#2");

			sd = new SortDescription ("Sample", ListSortDirection.Descending);

			Assert.AreEqual ("Sample".GetHashCode () ^ ListSortDirection.Descending.GetHashCode (),
					 sd.GetHashCode (), "TSHC_#3");

			sd = new SortDescription(null, ListSortDirection.Descending);

			Assert.AreEqual (ListSortDirection.Descending.GetHashCode (), sd.GetHashCode( ), "TSHC_#4");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void SetSealedPropertyNameTest() {
			SortDescription sd = new SortDescription("Test", ListSortDirection.Ascending);

			// Need to borrow the add method of SortDescriptionCollection to seal the
			// SortDescription (Seal is internal)
			SortDescriptionCollection sdc = new SortDescriptionCollection ();
			sdc.Add (sd);

			sd = sdc [0];
			// SD is sealed now.
			Assert.IsTrue (sd.IsSealed, "SealedProp_#1");

			sd.PropertyName = "NewProperty";
			Assert.AreEqual ("NewProperty", sd.PropertyName, "SealedProp_#1");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void SetSealedDirectionTest() {
			SortDescription sd = new SortDescription ("Test", ListSortDirection.Ascending);

			// Need to borrow the add method of SortDescriptionCollection to seal the
			// SortDescription (Seal is internal)
			SortDescriptionCollection sdc = new SortDescriptionCollection ();
			sdc.Add (sd);

			sd = sdc [0];
			// SD is sealed now.
			Assert.IsTrue(sd.IsSealed, "SealedProp_#1");

			sd.Direction = ListSortDirection.Descending;
			Assert.AreEqual (ListSortDirection.Descending, sd.Direction, "SealedProp_#1");
		}
	}
}
