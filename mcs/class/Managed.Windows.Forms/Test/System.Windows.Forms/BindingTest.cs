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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jackson Harper	jackson@ximian.com


using System;
using System.Data;
using System.Collections;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms.DataBinding {

	[TestFixture]
	public class BindingTest {

		[SetUp]
		protected virtual void SetUp ()
		{
		}

		[TearDown]
		protected virtual void TearDown ()
		{
		}

		[Test]
		public void CtorTest ()
		{
			string prop = "PROPERTY NAME";
			object data_source = new object ();
			string data_member = "DATA MEMBER";
			Binding b = new Binding (prop, data_source, data_member);

			Assert.IsNull (b.BindingManagerBase, "ctor1");
			Console.WriteLine ("MEMBER INFO:  " + b.BindingMemberInfo);
			Assert.IsNotNull (b.BindingMemberInfo, "ctor2");
			Assert.IsNull (b.Control, "ctor3");
			Assert.IsFalse (b.IsBinding, "ctor4");

			Assert.AreSame (b.PropertyName, prop, "ctor5");
			Assert.AreSame (b.DataSource, data_source, "ctor6");
		}

		[Test]
		public void CtorNullTest ()
		{
			Binding b = new Binding (null, null, null);

			Assert.IsNull (b.PropertyName, "ctornull1");
			Assert.IsNull (b.DataSource, "ctornull2");
		}

		// XXX this belongs in a ControlBindingsCollectionTest
		// file.
		[Test]
		[ExpectedException (typeof (ArgumentException))] // MS: "This would cause two bindings in the collection to bind to the same property."
		public void DuplicateBindingAdd ()
		{
			Control c1 = new Control ();
			Control c2 = new Control ();

			c2.DataBindings.Add ("Text", c1, "Text");
			c2.DataBindings.Add ("Text", c1, "Text");
		}

		[Test]
		public void BindingManagerBaseTest ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

			Control c1 = new Control ();
			Control c2 = new Control ();
			Binding binding;

			c1.BindingContext = new BindingContext ();
			c2.BindingContext = c1.BindingContext;

			binding = c2.DataBindings.Add ("Text", c1, "Text");

			Assert.IsNull (binding.BindingManagerBase, "1");

			c1.CreateControl ();
			c2.CreateControl ();

			Assert.IsNull (binding.BindingManagerBase, "2");

			c2.DataBindings.Remove (binding);
			binding = c2.DataBindings.Add ("Text", c1, "Text");

			Assert.IsTrue (binding.BindingManagerBase != null, "3");
		}

		[Test]
		/* create control and set binding context */
		public void BindingContextChangedTest ()
		{
			Control c = new Control ();
			// Test BindingContextChanged Event
			c.BindingContextChanged += new EventHandler (Event_Handler1);
			BindingContext bcG1 = new BindingContext ();
			eventcount = 0;
			c.BindingContext = bcG1;
			Assert.AreEqual (1, eventcount, "A1");
		}

		[Test]
		/* create control and show control */
		public void BindingContextChangedTest2 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			Control c = new Control ();
			f.Controls.Add (c);

			c.BindingContextChanged += new EventHandler (Event_Handler1);
			eventcount = 0;
			f.Show ();
#if NET_2_0
			Assert.AreEqual (1, eventcount, "A1");
#else
			Assert.AreEqual (2, eventcount, "A1");
#endif
			f.Dispose();
		}

		[Test]
		/* create control, set binding context, and show control */
		public void BindingContextChangedTest3 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Control c = new Control ();
			f.Controls.Add (c);

			c.BindingContextChanged += new EventHandler (Event_Handler1);
			eventcount = 0;
			c.BindingContext = new BindingContext ();;
			f.Show ();
			Assert.AreEqual (1, eventcount, "A1");
			f.Dispose ();
		}

		[Test]
		public void BindingContextChangedTest4 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			ContainerControl cc = new ContainerControl ();

			Control c = new Control ();
			f.Controls.Add (cc);
			cc.Controls.Add (c);

			c.BindingContextChanged += new EventHandler (Event_Handler1);
			cc.BindingContextChanged += new EventHandler (Event_Handler1);
			f.BindingContextChanged += new EventHandler (Event_Handler1);

			eventcount = 0;
			f.Show ();
#if NET_2_0
			Assert.AreEqual (5, eventcount, "A1");
#else
			Assert.AreEqual (8, eventcount, "A1");
#endif
			f.Dispose ();
		}

		int eventcount;
		public void Event_Handler1 (object sender, EventArgs e)
		{
			//Console.WriteLine (sender.GetType());
			//Console.WriteLine (Environment.StackTrace);
			eventcount++;
		}

		[Test]
		public void DataBindingCountTest1 ()
		{
			Control c = new Control ();
			Assert.AreEqual (0, c.DataBindings.Count, "1");
			c.DataBindings.Add (new Binding ("Text", c, "Name"));
			Assert.AreEqual (1, c.DataBindings.Count, "2");

			Binding b = c.DataBindings[0];
			Assert.AreEqual (c, b.Control, "3");
			Assert.AreEqual (c, b.DataSource, "4");
			Assert.AreEqual ("Text", b.PropertyName, "5");
			Assert.AreEqual ("Name", b.BindingMemberInfo.BindingField, "6");
		}

		[Test]
		public void DataBindingCountTest2 ()
		{
			Control c = new Control ();
			Control c2 = new Control ();
			Assert.AreEqual (0, c.DataBindings.Count, "1");
			c.DataBindings.Add (new Binding ("Text", c2, "Name"));
			Assert.AreEqual (1, c.DataBindings.Count, "2");
			Assert.AreEqual (0, c2.DataBindings.Count, "3");

			Binding b = c.DataBindings[0];
			Assert.AreEqual (c, b.Control, "4");
			Assert.AreEqual (c2, b.DataSource, "5");
			Assert.AreEqual ("Text", b.PropertyName, "6");
			Assert.AreEqual ("Name", b.BindingMemberInfo.BindingField, "7");
		}
	}

}

