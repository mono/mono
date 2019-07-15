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
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class EncodingTest
	{
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void IsBrowserDisplay ()
		{
			MyEncoding enc = new MyEncoding ();
			Assert.IsFalse (enc.IsBrowserDisplay);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void IsBrowserSave ()
		{
			MyEncoding enc = new MyEncoding ();
			Assert.IsFalse (enc.IsBrowserSave);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void IsMailNewsDisplay ()
		{
			MyEncoding enc = new MyEncoding ();
			Assert.IsFalse (enc.IsMailNewsDisplay);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void IsMailNewsSave ()
		{
			MyEncoding enc = new MyEncoding ();
			Assert.IsFalse (enc.IsMailNewsSave);
		}

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
		public void GetEncoding_Name_NotSupported ()
		{
			try {
				Encoding.GetEncoding ("doesnotexist");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("doesnotexist") != -1, "#5");
				Assert.IsNotNull (ex.ParamName, "#6");
				Assert.AreEqual ("name", ex.ParamName, "#7");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetEncoding_Name_Null ()
		{
			Encoding.GetEncoding ((string) null);
		}

		[Test]
		public void EncodingName ()
		{
			Assert.AreEqual ("Unicode (UTF-8)", Encoding.UTF8.EncodingName);
		}

#if !MOBILE && !XAMMAC_4_5 // all encodings aren't always available on mobile / XM, e.g. with linking
		[Test] // https://github.com/mono/mono/issues/11529
		public void AllEncodingsAreSerializable ()
		{
			foreach (var encoding in Encoding.GetEncodings ().Select(e => Encoding.GetEncoding (e.Name)))
			{
				var hashCode = encoding.GetHashCode (); 
				var serializer = new BinaryFormatter ();
				using (var ms = new MemoryStream ())
				{
					serializer.Serialize (ms, encoding);
					ms.Position = 0;
					var clone = (Encoding) serializer.Deserialize (ms);
					Assert.AreEqual (encoding, clone);
				}
			}
		}
#endif

		[Test] // https://github.com/mono/mono/issues/11663
		public void EncodingIsBinaryCompatible ()
		{
			const string serializedEncoding = 
				"AAEAAAD/////AQAAAAAAAAAEAQAAABlTeXN0ZW0uVGV4dC5BU0NJSUVuY29kaW5nCQAAAAptX2NvZGVQYWdlCGRhdGFJdGVtD2VuY29kZXJGYWxsY"  +
				"mFjaw9kZWNvZGVyRmFsbGJhY2sTRW5jb2RpbmcrbV9jb2RlUGFnZRFFbmNvZGluZytkYXRhSXRlbRVFbmNvZGluZyttX2lzUmVhZE9ubHkYRW5jb2R" +
				"pbmcrZW5jb2RlckZhbGxiYWNrGEVuY29kaW5nK2RlY29kZXJGYWxsYmFjawADAwMAAwADAwglU3lzdGVtLkdsb2JhbGl6YXRpb24uQ29kZVBhZ2VEY" +
				"XRhSXRlbSZTeXN0ZW0uVGV4dC5FbmNvZGVyUmVwbGFjZW1lbnRGYWxsYmFjayZTeXN0ZW0uVGV4dC5EZWNvZGVyUmVwbGFjZW1lbnRGYWxsYmFjawg" +
				"lU3lzdGVtLkdsb2JhbGl6YXRpb24uQ29kZVBhZ2VEYXRhSXRlbQEmU3lzdGVtLlRleHQuRW5jb2RlclJlcGxhY2VtZW50RmFsbGJhY2smU3lzdGVtL" +
				"lRleHQuRGVjb2RlclJlcGxhY2VtZW50RmFsbGJhY2ufTgAACgkCAAAACQMAAACfTgAACgEJAgAAAAkDAAAABAIAAAAmU3lzdGVtLlRleHQuRW5jb2R" +
				"lclJlcGxhY2VtZW50RmFsbGJhY2sDAAAACnN0ckRlZmF1bHQbYklzTWljcm9zb2Z0QmVzdEZpdEZhbGxiYWNrK0VuY29kZXJGYWxsYmFjaytiSXNNa" +
				"WNyb3NvZnRCZXN0Rml0RmFsbGJhY2sBAAABAQYGAAAAAT8AAAQDAAAAJlN5c3RlbS5UZXh0LkRlY29kZXJSZXBsYWNlbWVudEZhbGxiYWNrAwAAAAp" +
				"zdHJEZWZhdWx0G2JJc01pY3Jvc29mdEJlc3RGaXRGYWxsYmFjaytEZWNvZGVyRmFsbGJhY2srYklzTWljcm9zb2Z0QmVzdEZpdEZhbGxiYWNrAQAAA" +
				"QEJBgAAAAAACw==";

			using (var ms = new MemoryStream (Convert.FromBase64String (serializedEncoding)))
			{
				var serializer = new BinaryFormatter ();
				var e = (Encoding) serializer.Deserialize (ms);
				Assert.IsTrue (e.EncoderFallback.GetHashCode () != 0);
				Assert.IsTrue (e.DecoderFallback.GetHashCode () != 0);
				Assert.IsTrue (e.GetDecoder ().GetHashCode () != 0);
				Assert.IsTrue (e.GetEncoder ().GetHashCode () != 0);
				Assert.IsTrue (e.GetHashCode () != 0);
			}
		}
	}
}
