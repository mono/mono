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
// (C) iain@mccoy.id.au
//
// Authors:
//	Iain McCoy (iain@mccoy.id.au)
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	class X {
		public static readonly DependencyProperty AProperty = DependencyProperty.RegisterAttached("A", typeof(int), typeof(X));
		public static void SetA(DependencyObject obj, int value)
		{
			obj.SetValue(AProperty, value);
		}
		public static int GetA(DependencyObject obj)
		{
			return (int)obj.GetValue(AProperty);
		}

		public static readonly DependencyProperty BProperty = DependencyProperty.RegisterAttached("B", typeof(string), typeof(X));
		public static void SetB(DependencyObject obj, string value)
		{
			obj.SetValue(BProperty, value);
		}
		public static string GetB(DependencyObject obj)
		{
			return (string)obj.GetValue(BProperty);
		}

	}

	class Y : DependencyObject {
	}

	class DefaultValueTest : DependencyObject {
		public static readonly DependencyProperty AProperty = DependencyProperty.Register("A", typeof(string), typeof(DefaultValueTest), new PropertyMetadata("defaultValueTest"));
	}

	[TestFixture]
	public class DependencyObjectTest {
		[Test]
		[Category ("NotWorking")]
		public void TestAttachedProperty()
		{
			Y y1 = new Y();
			X.SetA(y1, 2);
			Assert.AreEqual(2, X.GetA(y1));
		}
	
		[Test]
		[Category ("NotWorking")]
		public void Test2AttachedProperties()
		{
			Y y1 = new Y();
			Y y2 = new Y();
			X.SetA(y1, 2);
			X.SetA(y2, 3);
			Assert.AreEqual(2, X.GetA(y1));
			Assert.AreEqual(3, X.GetA(y2));
		}

		[Test]
		[Category ("NotWorking")]
		public void TestEnumerationOfAttachedProperties()
		{
			int count = 0;
			Y y = new Y();
			X.SetA(y, 2);
			X.SetB(y, "Hi");

			LocalValueEnumerator e = y.GetLocalValueEnumerator();
			while (e.MoveNext()) {
				count++;
				if (e.Current.Property == X.AProperty)
					Assert.AreEqual(e.Current.Value, 2);
				else if (e.Current.Property == X.BProperty)
					Assert.AreEqual(e.Current.Value, "Hi");
				else
					Assert.Fail("Wrong sort of property" + e.Current.Property);
			}

			Assert.AreEqual(2, count);
		}

		[Test]
		public void TestDefaultValue()
		{
			DefaultValueTest obj = new DefaultValueTest ();
			Assert.AreEqual (obj.GetValue(DefaultValueTest.AProperty), "defaultValueTest");
		}

	}
}
