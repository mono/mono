//
// EncodingTest.cs - Unit Tests for System.Text.Encoding
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2007 Gert Driesen
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
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class EncodingTest
	{
		[Test]
		public void GetEncoding_CodePage_Default ()
		{
			Assert.AreEqual (Encoding.Default, Encoding.GetEncoding (0));
		}

		[Test]
		public void GetEncoding_CodePage_Invalid ()
		{
			try {
				Encoding.GetEncoding (-1);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("codepage", ex.ParamName, "#A6");
			}

			try {
				Encoding.GetEncoding (65536);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("codepage", ex.ParamName, "#B6");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEncoding_Name_NotSupported ()
		{
			Encoding.GetEncoding ("doesnotexist");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetEncoding_Name_Null ()
		{
			Encoding.GetEncoding ((string) null);
		}
	}
}
