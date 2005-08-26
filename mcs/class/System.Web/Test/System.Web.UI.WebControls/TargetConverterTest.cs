//
// Tests for System.Web.UI.WebControls.TargetConverter.cs 
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class TargetConverterTest {
		[Test]
		public void Basic () {
			string[]				result;
			int					i;
			TargetConverter				conv;
			TypeConverter.StandardValuesCollection	values;

			conv = new TargetConverter();

			values = conv.GetStandardValues(null);

			Assert.AreEqual(5, values.Count, "B1");
			result = new string[values.Count];
			i = 0;
			foreach (string s in values) {
				result[i++] = s;
			}

			Assert.AreEqual(new string[] { "_blank", "_parent", "_search", "_self", "_top"}, result, "B2");
			Assert.AreEqual(false, conv.GetStandardValuesExclusive(null), "B3");
			Assert.AreEqual(true, conv.GetStandardValuesSupported(null), "B4");
		}
	}
}
