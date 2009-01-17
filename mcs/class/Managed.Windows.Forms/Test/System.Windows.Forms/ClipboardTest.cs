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
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Carlos Alberto Cortez <ccortes@novell.com>
//

using System;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ClipboardTest
	{
#if NET_2_0
		[Test]
		public void TextTest ()
		{
			// Put some unicode chars
			string text = "hello \u1000 mono!";
			Clipboard.SetText (text);

			Assert.AreEqual (true, Clipboard.ContainsText (TextDataFormat.UnicodeText), "#A1");
			Assert.AreEqual (text, Clipboard.GetText (TextDataFormat.UnicodeText), "#A2");
		}
#endif
	}
}

