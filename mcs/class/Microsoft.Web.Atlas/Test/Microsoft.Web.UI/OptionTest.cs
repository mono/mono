//
// Tests for Microsoft.Web.UI.Option
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Web;
using Microsoft.Web.UI;

namespace MonoTests.Microsoft.Web.UI
{
	[TestFixture]
	public class OptionTest
	{
		[Test]
		public void Ctor1 ()
		{
			Option o = new Option ();

			Assert.AreEqual ("", o.Text, "A1");
			Assert.AreEqual ("", o.Value, "A2");
		}

		[Test]
		public void Ctor2 ()
		{
			Option o = new Option ("Text", "Value");

			Assert.AreEqual ("Text", o.Text, "A1");
			Assert.AreEqual ("Value", o.Value, "A2");
		}

		[Test]
		public void Properties ()
		{
			Option o = new Option ();

			// non ctor defaults
			Assert.AreEqual ("", o.CssClass, "A1");

			// get/set
			o.Text = "Text";
			Assert.AreEqual ("Text", o.Text, "A2");
			o.Value = "Value";
			Assert.AreEqual ("Value", o.Value, "A3");
			o.CssClass = "CssClass";
			Assert.AreEqual ("CssClass", o.CssClass, "A4");

			// null setters
			o.Text = null;
			Assert.AreEqual ("", o.Text, "A2");
			o.Value = null;
			Assert.AreEqual ("", o.Value, "A3");
			o.CssClass = null;
			Assert.AreEqual ("", o.CssClass, "A4");
		}
	}
}
#endif
