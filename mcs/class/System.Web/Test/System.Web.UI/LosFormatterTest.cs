//
// LosFormatterTest.cs - Unit tests for System.Web.UI.LosFormatter
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
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
using System.Web.UI;

using NUnit.Framework;

namespace MonoTests.System.Web.UI
{
	[TestFixture]
	public class LosFormatterTest
	{
		[Test] // bug #411115
		public void Deserialize_Stream_NonSeekable ()
		{
			string s1 = "Hello world";
			NonSeekableStream ns = new NonSeekableStream ();
			LosFormatter lf = new LosFormatter ();
			lf.Serialize (ns, s1);
			ns.Reset ();
			string s2 = lf.Deserialize (ns) as string;
			Assert.AreEqual (s1, s2);
		}

		[Test] // bug #324526
		public void Serialize ()
		{
			string s = "Hello world";
			LosFormatter lf = new LosFormatter ();
			StringWriter sw = new StringWriter ();
			lf.Serialize (sw, s);
			string s1 = sw.ToString ();
			Assert.IsNotNull (s1, "#1");
			string s2 = lf.Deserialize (s1) as string;
			Assert.IsNotNull (s2, "#2");
			Assert.AreEqual (s, s2, "#3");
		}

		[Test]
		[Category ("NotWorking")]
		public void Serialize_Output ()
		{
			string s = "Hello world";
			LosFormatter lf = new LosFormatter ();
			StringWriter sw = new StringWriter ();
			lf.Serialize (sw, s);
			string s1 = sw.ToString ();
#if NET_2_0
			Assert.AreEqual ("/wEFC0hlbGxvIHdvcmxk", s1, "#1");
#else
			Assert.AreEqual ("SGVsbG8gd29ybGQ=", s1, "#1");
#endif
			string s2 = lf.Deserialize (s1) as string;
			Assert.IsNotNull (s2, "#2");
			Assert.AreEqual (s, s2, "#3");
		}

		[Test]
		[Category ("NotDotNet")] // MS throws NullReferenceException
		public void Serialize_Output_Null ()
		{
			LosFormatter lf = new LosFormatter ();
			try {
				lf.Serialize ((TextWriter) null, "test");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("output", ex.ParamName, "#6");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Serialize_Stream ()
		{
			string s = "Hello world";
			LosFormatter lf = new LosFormatter ();
			MemoryStream ms = new MemoryStream ();
			lf.Serialize (ms, s);
			string s1 = Encoding.UTF8.GetString (ms.GetBuffer (), 0, (int) ms.Length);
#if NET_2_0
			Assert.AreEqual ("/wEFC0hlbGxvIHdvcmxk", s1, "#1");
#else
			Assert.AreEqual ("SGVsbG8gd29ybGQ=", s1, "#1");
#endif
			string s2 = lf.Deserialize (s1) as string;
			Assert.IsNotNull (s2, "#2");
			Assert.AreEqual (s, s2, "#3");
		}

		[Test]
		public void Serialize_Stream_Null ()
		{
			LosFormatter lf = new LosFormatter ();
			try {
				lf.Serialize ((Stream) null, "test");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("stream", ex.ParamName, "#6");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Serialize_Value_Null ()
		{
			LosFormatter lf = new LosFormatter ();
			MemoryStream ms = new MemoryStream ();
			lf.Serialize (ms, null);
			string s1 = Encoding.UTF8.GetString (ms.GetBuffer (), 0, (int) ms.Length);
#if NET_2_0
			Assert.AreEqual ("/wFk", s1, "#1");
#else
			Assert.AreEqual (string.Empty, s1, "#1");
#endif

			StringWriter sw = new StringWriter ();
			lf.Serialize (sw, null);
			string s2 = sw.ToString ();
#if NET_2_0
			Assert.AreEqual ("/wFk", s1, "#2");
#else
			Assert.AreEqual (string.Empty, s1, "#2");
#endif
		}

		class NonSeekableStream : MemoryStream
		{
			private bool canSeek;

			public override bool CanSeek {
				get { return canSeek; }
			}

			public override long Length {
				get {
					if (!CanSeek)
						throw new NotSupportedException ();
					return base.Length;
				}
			}

			public override long Position {
				get{
					if (!CanSeek)
						throw new NotSupportedException ();
					return base.Position;
				}
				set {
					base.Position = value;
				}
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				if (!CanSeek)
					throw new NotSupportedException ();
				return base.Seek (offset, origin);
			}

			public void Reset ()
			{
				canSeek = true;
				Position = 0;
				canSeek = false;
			}
		}
	}
}
