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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
// 	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//


using System;
using System.ComponentModel;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms.DataBinding {

	[TestFixture]
	public class BindingsCollectionTest : TestHelper 
	{
#if NET_2_0
		// 
		// CollectionChanging event test section
		//
		bool collection_changing_called;
		int collection_expected_count;
		string collection_expected_assert;
		CollectionChangeAction collection_action_expected;
		object collection_element_expected;

		void CollectionChangingHandler (object o, CollectionChangeEventArgs args)
		{
			BindingsCollection coll = (BindingsCollection)o;

			collection_changing_called = true;
			Assert.AreEqual (collection_expected_count, coll.Count, collection_expected_assert + "-0");
			Assert.AreEqual (collection_action_expected, args.Action, collection_expected_assert + "-1");
			Assert.AreEqual (collection_element_expected, args.Element, collection_expected_assert + "-2");
		}

		[Test]
		public void CollectionChangingTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			ControlBindingsCollection binding_coll = c.DataBindings;

			Binding binding = new Binding ("Text", new MockItem ("A", 0), "Text");
			Binding binding2 = new Binding ("Name", new MockItem ("B", 0), "Text");
			binding_coll.Add (binding);

			binding_coll.CollectionChanging += CollectionChangingHandler;

			collection_expected_count = 1;
			collection_action_expected = CollectionChangeAction.Add;
			collection_element_expected = binding2;
			collection_expected_assert = "#A0";
			binding_coll.Add (binding2);
			Assert.IsTrue (collection_changing_called, "#A1");

			collection_changing_called = false;
			collection_expected_count = 2;
			collection_action_expected = CollectionChangeAction.Remove;
			collection_element_expected = binding;
			collection_expected_assert = "#B0";
			binding_coll.Remove (binding);
			Assert.IsTrue (collection_changing_called, "#B1");

			collection_changing_called = false;
			collection_expected_count = 1;
			collection_element_expected = null;
			collection_action_expected = CollectionChangeAction.Refresh;
			collection_expected_assert = "#C0";
			binding_coll.Clear ();
			Assert.IsTrue (collection_changing_called, "#C1");
		}
#endif
	}
}

