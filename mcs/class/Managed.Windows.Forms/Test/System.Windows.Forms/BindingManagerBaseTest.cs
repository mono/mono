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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Chris Toshok	toshok@ximian.com

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms.DataBinding
{
	[TestFixture]
	public class BindingManagerBaseTest : TestHelper {
		
		[Test]
		public void BindingsTest ()
		{
			Control c1 = new Control ();
			Control c2 = new Control ();

			c1.CreateControl ();
			c2.CreateControl ();

			Binding binding;
			BindingManagerBase bm, bm2;

			c1.BindingContext = new BindingContext ();
			c2.BindingContext = c1.BindingContext;

			bm = c2.BindingContext[c1, "Text"];
			bm2 = c2.BindingContext[c1];

#if NET_2_0
			bm.BindingComplete += delegate (object sender, BindingCompleteEventArgs e) { Console.WriteLine (Environment.StackTrace); };
			bm2.BindingComplete += delegate (object sender, BindingCompleteEventArgs e) { Console.WriteLine (Environment.StackTrace); };
#endif

			binding = c2.DataBindings.Add ("Text", c1, "Text");

			Assert.AreEqual (0, bm.Bindings.Count, "1");
			Assert.AreEqual (1, bm2.Bindings.Count, "2");

			Assert.AreEqual (bm2.Bindings[0], binding, "3");
		}

#if NET_2_0
		[Test]
		public void IsBindingSuspendedTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();

			MockItem item = new MockItem ("A", 0);
			MockItem [] items = new MockItem [] { item };

			BindingManagerBase manager = c.BindingContext [item];
			BindingManagerBase manager2 = c.BindingContext [items];
			Assert.IsFalse (manager.IsBindingSuspended, "#A1");
			Assert.IsFalse (manager2.IsBindingSuspended, "#A2");

			manager.SuspendBinding ();
			manager2.SuspendBinding ();
			Assert.IsFalse (manager.IsBindingSuspended, "#B1");
			Assert.IsTrue (manager2.IsBindingSuspended, "#B2");

			manager.ResumeBinding ();
			manager2.ResumeBinding ();
			Assert.IsFalse (manager.IsBindingSuspended, "#C1");
			Assert.IsFalse (manager2.IsBindingSuspended, "#C2");
		}
#endif
	}
}
