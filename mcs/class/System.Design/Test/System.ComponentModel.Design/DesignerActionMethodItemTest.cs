//
// DesignerActionMethodItemTest.cs
//
// Author:
//	  Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.

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
using System.ComponentModel;
using System.ComponentModel.Design;
using NUnit.Framework;

namespace MonoTests.System.ComponentModel.Design
{
	[TestFixture]
	public class DesignerActionMethodItemTest
	{
		[Test]
		public void Constructor ()
		{
			DesignerActionMethodItem item = new DesignerActionMethodItem (new DesignerActionList (new Component ()),
				"myMember", "myDisplay");
			Assert.AreEqual ("myMember", item.MemberName, "#1");
			Assert.AreEqual ("myDisplay", item.DisplayName, "#2");
			Assert.IsNull (item.Category, "#3");
			Assert.IsNull (item.Description, "#4");
			Assert.IsFalse (item.IncludeAsDesignerVerb, "#5");
		}
	}
}
