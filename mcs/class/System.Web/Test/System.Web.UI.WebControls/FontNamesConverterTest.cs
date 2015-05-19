//
// Tests for System.Web.UI.WebControls.FontNamesConverter.cs 
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
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

using NUnit.Framework;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class FontNamesConverterTest {
		[Test]
		public void Basic ()
		{
			FontNamesConverter	conv;
			string[]		name_array;
			string			name_list;

			conv = new FontNamesConverter();
			name_array = new string[] { "Arial", "Courier" };
			name_list = "Arial,Courier";

			Assert.AreEqual (true, conv.CanConvertFrom(null, typeof(string)), "B1");
			Assert.AreEqual (true, conv.CanConvertTo(null, typeof(string)), "B2");
			Assert.AreEqual (false, conv.CanConvertFrom(null, typeof(string[])), "B3");
			Assert.AreEqual (false, conv.CanConvertTo(null, typeof(string[])), "B4");

			Assert.AreEqual (false, conv.CanConvertFrom(null, typeof(int)), "B5");
			Assert.AreEqual (false, conv.CanConvertTo(null, typeof(int)), "B6");

			// ASP.NET in a Nutshell 2nd Edition (O'Reilly), pg855:
			// FontNamesConverter converts between a font name array and a string that contains a 
			// list of font names separated by comma.
			// Why does the CanConvertFrom() and CanConvertTo() then indicate that it cannot handle string[]???
			// It obviously works:
			Assert.AreEqual ("", conv.ConvertTo(null, null, null, typeof(string)), "B10");
			Assert.AreEqual (new string[0], conv.ConvertFrom(null, null, ""), "B11");
		}
	}
}
