//
// LosFormatterTest.cs - Unit tests for System.Web.UI.LosFormatter
//
// Authors:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Gert Driesen
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
		static byte [] Empty = new byte [0];

		string Serialize (LosFormatter lf, object value)
		{
			StringWriter sw = new StringWriter ();
			lf.Serialize (sw, value);
			return sw.ToString ();
		}

		object Deserialize (LosFormatter lf, string serializedData)
		{
			return lf.Deserialize (serializedData);
		}

		string NoKeyRoundTrip (LosFormatter lf, string assertionMessage)
		{
			string serializedData = Serialize (lf, "Mono");
			Assert.AreEqual ("Mono", (string) Deserialize (lf, serializedData), assertionMessage);
			return serializedData;
		}

		[Test]
		public void Ctor_BoolByteArray ()
		{
			LosFormatter lf1 = new LosFormatter (false, (byte []) null);
			string expected = NoKeyRoundTrip (lf1, "false, null");

			LosFormatter lf2 = new LosFormatter (true, (byte []) null);
			Assert.AreEqual (expected, NoKeyRoundTrip (lf2, "true, null"), "2");

			LosFormatter lf3 = new LosFormatter (false, Empty);
			Assert.AreEqual (expected, NoKeyRoundTrip (lf3, "false, empty"), "3");

			// an empty key is still a key - a signature is appended
			LosFormatter lf4 = new LosFormatter (true, Empty);
			string signed = NoKeyRoundTrip (lf4, "true, empty");
			Assert.AreNotEqual (expected, signed, "4");

			byte [] data = Convert.FromBase64String (expected);
			byte [] signed_data = Convert.FromBase64String (signed);
			Assert.IsTrue (BitConverter.ToString (signed_data).StartsWith (BitConverter.ToString (data)), "4 / same data");
#if NET_4_0
			// 32 bytes == 256 bits -> match HMACSHA256 as default
			Assert.AreEqual (32, signed_data.Length - data.Length, "signature length");
#else
			// 20 bytes == 160 bits -> match HMACSHA1 as default
			Assert.AreEqual (20, signed_data.Length - data.Length, "signature length");
#endif
		}

		[Test]
		public void Ctor_BoolString ()
		{
			LosFormatter lf1 = new LosFormatter (false, (string) null);
			string expected = NoKeyRoundTrip (lf1, "false, null");

			LosFormatter lf2 = new LosFormatter (true, (string) null);
			Assert.AreEqual (expected, NoKeyRoundTrip (lf2, "true, null"), "2");

			LosFormatter lf3 = new LosFormatter (false, String.Empty);
			Assert.AreEqual (expected, NoKeyRoundTrip (lf3, "false, empty"), "3");

			// an empty string is not an empty key!
			LosFormatter lf4 = new LosFormatter (true, String.Empty);
			Assert.AreEqual (expected, NoKeyRoundTrip (lf4, "true, empty"), "4");

			byte [] key = new byte [32];
			LosFormatter lf5 = new LosFormatter (true, Convert.ToBase64String (key));
			string signed = NoKeyRoundTrip (lf5, "true, b64");
			Assert.AreNotEqual (expected, signed, "5");

			byte [] data = Convert.FromBase64String (expected);
			byte [] signed_data = Convert.FromBase64String (signed);
			Assert.IsTrue (BitConverter.ToString (signed_data).StartsWith (BitConverter.ToString (data)), "5 / same data");
#if NET_4_0
			// 32 bytes == 256 bits -> match HMACSHA256 as default
			Assert.AreEqual (32, signed_data.Length - data.Length, "signature length");
#else
			// 20 bytes == 160 bits -> match HMACSHA1 as default
			Assert.AreEqual (20, signed_data.Length - data.Length, "signature length");
#endif
		}

		string SerializeOverloads (LosFormatter lf, string message)
		{
			string stream_ser;
			using (MemoryStream ms = new MemoryStream ()) {
				lf.Serialize (ms, String.Empty);
				stream_ser = Convert.ToBase64String (ms.ToArray ());
			}

			string tw_ser;
			using (MemoryStream ms = new MemoryStream ()) {
				using (TextWriter tw = new StreamWriter (ms)) {
					lf.Serialize (tw, String.Empty);
				}
				tw_ser = Convert.ToBase64String (ms.ToArray ());
			}

			Assert.AreEqual (stream_ser, tw_ser, message);
			return stream_ser;
		}

		[Test]
		public void SerializeOverloads ()
		{
			LosFormatter lf1 = new LosFormatter (false, (string) null);
			string r1 = SerializeOverloads (lf1, "false, null");

			LosFormatter lf2 = new LosFormatter (true, (string) null);
			string r2 = SerializeOverloads (lf2, "true, null");
			Assert.AreEqual (r1, r2, "r1-r2");

			LosFormatter lf3 = new LosFormatter (false, String.Empty);
			string r3 = SerializeOverloads (lf3, "false, empty");
			Assert.AreEqual (r2, r3, "r2-r3");

			// an empty string is not an empty key!
			LosFormatter lf4 = new LosFormatter (true, String.Empty);
			string r4 = SerializeOverloads (lf4, "true, empty");
			Assert.AreEqual (r3, r4, "r3-r4");

			byte [] key = new byte [32];
			LosFormatter lf5 = new LosFormatter (true, Convert.ToBase64String (key));
			string r5 = SerializeOverloads (lf5, "false, b64");
			Assert.AreNotEqual (r4, r5, "r4-r5");
		}

#if NET_4_0
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void Deserialize_Stream_NonSeekable ()
		{
			string s1 = "Hello world";
			NonSeekableStream ns = new NonSeekableStream ();
			LosFormatter lf = new LosFormatter ();
			lf.Serialize (ns, s1);
		}
#else
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
#endif
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
