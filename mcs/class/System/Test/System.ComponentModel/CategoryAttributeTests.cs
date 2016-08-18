//
// Authors:
//	Alan McGovern  <amcgovern@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using CM = System.ComponentModel;
using NUnit.Framework;

namespace MonoTests.System.ComponentModel {

	[TestFixture]
	public class CategoryAttributeTest {

		[Test]
		public void CategoryNamesTest()
		{
		    Assert.AreEqual(CM.CategoryAttribute.Action.Category, "Action", "#1");
		    Assert.AreEqual(CM.CategoryAttribute.Appearance.Category, "Appearance", "#2");
		    Assert.AreEqual(CM.CategoryAttribute.Asynchronous.Category, "Asynchronous", "#3");
		    Assert.AreEqual(CM.CategoryAttribute.Behavior.Category, "Behavior", "#4");
		    Assert.AreEqual(CM.CategoryAttribute.Data.Category, "Data", "#5");
		    Assert.AreEqual(CM.CategoryAttribute.Design.Category, "Design", "#6");
		    Assert.AreEqual(CM.CategoryAttribute.Focus.Category, "Focus", "#7");
		    Assert.AreEqual(CM.CategoryAttribute.Format.Category, "Format", "#8");
		    Assert.AreEqual(CM.CategoryAttribute.Key.Category, "Key", "#9");
		    Assert.AreEqual(CM.CategoryAttribute.Layout.Category, "Layout", "#10");
		    Assert.AreEqual(CM.CategoryAttribute.Mouse.Category, "Mouse", "#11");
#if MOBILE
		    Assert.AreEqual(CM.CategoryAttribute.Default.Category, "Default", "#12");
		    Assert.AreEqual(CM.CategoryAttribute.DragDrop.Category, "DragDrop", "#13");
		    Assert.AreEqual(CM.CategoryAttribute.WindowStyle.Category, "WindowStyle", "#14");
#else
		    Assert.AreEqual(CM.CategoryAttribute.Default.Category, "Misc", "#12");
		    Assert.AreEqual(CM.CategoryAttribute.DragDrop.Category, "Drag Drop", "#13");
		    Assert.AreEqual(CM.CategoryAttribute.WindowStyle.Category, "Window Style", "#14");
#endif
		}

		[Test]
		public void CustomCategory ()
		{
			var ca = new CM.CategoryAttribute ("Foo Category");
			Assert.AreEqual ("Foo Category", ca.Category);
		}
	}
}
