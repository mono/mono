//
// DataBinderTest.cs
//
// Author:
//      Marek Habersack (mhabersack@novell.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoTests.System.Web.UI
{
	class InnerObject
	{
		public string Property {
			get { return "InnerObject.Property"; }
		}

		public string InnerProperty {
			get { return "InnerObject.InnerProperty"; }
		}
	}

	class OuterObject
	{
		InnerObject inner;
		ArrayList items;
		
		public InnerObject InnerObject {
			get { return inner; }
		}
		
		public string Property {
			get { return "OuterObject.Property"; }
		}

		public IList Items {
			get { return items; }
		}
		
		public OuterObject ()
		{
			inner = new InnerObject ();
			items = new ArrayList ();

			items.Add ("Item One");
			items.Add ("Item Two");
		}
	}

	class IndexedOuterObject : OuterObject
	{
		public string this [int index] {
 			get { return Items [index] as string; }
 		}
	}
	
        [TestFixture]
        public class DataBinderTest 
	{
		[Test]
		public void DataBinder_Eval ()
		{
			IndexedOuterObject oo = new IndexedOuterObject ();
			object val;

			val = DataBinder.Eval (oo, "Property");
			Assert.IsTrue (val is string, "#A1");
			Assert.AreEqual ("OuterObject.Property", val, "#A2");

			val = DataBinder.Eval (oo, "InnerObject.Property");
			Assert.IsTrue (val is string, "#B1");
			Assert.AreEqual ("InnerObject.Property", val, "#B2");

			val = DataBinder.Eval (oo, "InnerObject.InnerProperty");
			Assert.IsTrue (val is string, "#C1");
			Assert.AreEqual ("InnerObject.InnerProperty", val, "#C2");
			
			val = DataBinder.Eval (oo, "Items[0]");
			Assert.IsTrue (val is string, "#D1");
			Assert.AreEqual ("Item One", val, "#D2");

			val = DataBinder.Eval (oo, "[1]");
			Assert.IsTrue (val is string, "#E1");
			Assert.AreEqual ("Item Two", val, "#E2");

			Hashtable hash = new Hashtable ();
			hash.Add ("item1", "Item One");
			val = DataBinder.Eval (hash, "[item1]");
			Assert.IsTrue (val is string, "#F1");
			Assert.AreEqual ("Item One", val, "#F2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DataBinder_Eval_NoIndexer ()
		{
			OuterObject oo = new OuterObject ();
			object val = DataBinder.Eval (oo, "[1]");
		}
		
		[Test]
		[ExpectedException (typeof (HttpException))]
		public void DataBinder_Eval_MissingProperty ()
		{
			OuterObject oo = new OuterObject ();
			object val = DataBinder.Eval (oo, "InternalObject.Property");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void DataBinder_Eval_InvalidIndexedPropertyName ()
		{
			OuterObject oo = new OuterObject ();
			object val = DataBinder.Eval (oo, "Items [0]");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void DataBinder_Eval_MissingIndexedPropertyName ()
		{
			OuterObject oo = new OuterObject ();
			object val = DataBinder.Eval (oo, "MyItems[0]");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DataBinder_Eval_InvalidIndexedPropertyIndexerType ()
		{
			OuterObject oo = new OuterObject ();
			object val = DataBinder.Eval (oo, "Items['item']");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DataBinder_Eval_InvalidIndexerType ()
		{
			OuterObject oo = new OuterObject ();
			object val = DataBinder.Eval (oo, "[\"item\"]");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DataBinder_Eval_InvalidIndexerType2 ()
		{
			OuterObject oo = new OuterObject ();
			object val = DataBinder.Eval (oo, "[item]");
		}
	}
}
