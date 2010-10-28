//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

using Category = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Markup
{
	[TestFixture]
	public class ArrayExtensionTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullType ()
		{
			new ArrayExtension ((Type) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullElement ()
		{
			new ArrayExtension ((object []) null);
		}

		[Test]
		public void AddChild ()
		{
			var x = new ArrayExtension ();
			x.AddChild (new object ());
			x.AddChild (5);
			x.AddChild ("test");
		}

		[Test]
		public void AddChild2 ()
		{
			var x = new ArrayExtension (new int [0]);
			x.AddChild (new object ());
			x.AddChild (5);
			x.AddChild ("test");
		}

		[Test]
		public void AddInconsistent ()
		{
			var x = new ArrayExtension (typeof (int));
			Assert.AreEqual (typeof (int), x.Type, "#1");
			// adding inconsistent items is not rejected, while calling ProvideValue() results in an error.
			x.AddChild (new object ());
			Assert.AreEqual (typeof (int), x.Type, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ProvideValueInconsistent ()
		{
			var x = new ArrayExtension (typeof (int));
			x.AddChild (new object ());
			x.ProvideValue (null);
		}

		[Test]
		public void AddInconsistent2 ()
		{
			var x = new ArrayExtension (new int [] {1, 3});
			Assert.AreEqual (typeof (int), x.Type, "#1");
			x.AddChild (new object ());
			Assert.AreEqual (typeof (int), x.Type, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ProvideValueWithoutType ()
		{
			var x = new ArrayExtension ();
			x.ProvideValue (null); // Type must be set first.
		}

		[Test]
		public void ProvideValueEmpty ()
		{
			var x = new ArrayExtension (typeof (int));
			x.ProvideValue (null); // allowed.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ProvideValueInconsistent2 ()
		{
			var x = new ArrayExtension (new int [] {1, 3});
			x.AddChild (new object ());
			x.AddChild (null); // allowed
			Assert.AreEqual (4, x.Items.Count);
			x.ProvideValue (null);
		}

		[Test]
		public void ProvideValue ()
		{
			var x = new ArrayExtension (new int [] {1, 3});
			x.AddChild (5);
			Assert.AreEqual (3, x.Items.Count);
			var ret = x.ProvideValue (null);
			Assert.IsNotNull (ret, "#1");
			var arr = ret as int [];
			Assert.IsNotNull (arr, "#2");
			Assert.AreEqual (3, arr.Length, "#3");
			Assert.AreEqual (5, arr [2], "#4");
		}

		[Test]
		public void AddTextInconsistent ()
		{
			var x = new ArrayExtension (new int [] {1, 3});
			Assert.AreEqual (typeof (int), x.Type, "#1");
			x.AddText ("test");
			x.AddText (null); // allowed
			Assert.AreEqual (4, x.Items.Count);
			Assert.AreEqual (typeof (int), x.Type, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ProvideValueInconsistent3 ()
		{
			var x = new ArrayExtension (new int [] {1, 3});
			x.AddText ("test");
			x.ProvideValue (null);
		}
	}
}

