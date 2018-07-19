//
// Test FontConverter class testing unit
//
// Author:
//
// 	 Miguel de Icaza (miguel@gnome.org)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Security.Permissions;

namespace MonoTests.System.Drawing{

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class FontNameConverterTest {

		[Test]
		public void TestConvertFrom ()
		{
			FontConverter.FontNameConverter f = new FontConverter.FontNameConverter ();
			// returns "Times" under Linux and "Times New Roman" under Windows
			Assert.IsTrue ((f.ConvertFrom ("Times") as string).StartsWith ("Times"), "string test");
			Assert.IsTrue (f.GetStandardValuesSupported (), "standard values supported");
			Assert.IsFalse (f.GetStandardValuesExclusive (), "standard values exclusive");
		}

		[Test]
		public void ExTestConvertFrom ()
		{
			FontConverter.FontNameConverter f = new FontConverter.FontNameConverter ();
			Assert.Throws<NotSupportedException> (() => f.ConvertFrom (null));
		}

		[Test]
		public void ExTestConvertFrom2 ()
		{
			FontConverter.FontNameConverter f = new FontConverter.FontNameConverter ();
			Assert.Throws<NotSupportedException> (() => f.ConvertFrom (1));
		}
	}
}
