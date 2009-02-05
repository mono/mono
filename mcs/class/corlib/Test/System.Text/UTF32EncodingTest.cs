#if NET_2_0
using System;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class UTF32EncodingTest
	{
		[Test] // GetByteCount (Char [])
		[Category ("NotDotNet")] // A1/B1 return 24 on MS
		public void GetByteCount1 ()
		{
			char [] chars = new char[] { 'z', 'a', '\u0306',
				'\u01FD', '\u03B2', '\uD8FF', '\uDCFF' };

			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.AreEqual (28, le.GetByteCount (chars), "#A1");
			Assert.AreEqual (0, le.GetByteCount (new char [0]), "#A2");

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.AreEqual (28, be.GetByteCount (chars), "#B1");
			Assert.AreEqual (0, be.GetByteCount (new char [0]), "#B2");
		}

		[Test] // GetByteCount (Char [])
		public void GetByteCount1_Chars_Null ()
		{
			UTF32Encoding enc = new UTF32Encoding ();
			try {
				enc.GetByteCount ((Char []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("chars", ex.ParamName, "#5");
			}
		}

		[Test] // GetByteCount (String)
		[Category ("NotDotNet")] // A1/B1 return 24 on MS
		public void GetByteCount2 ()
		{
			string s = "za\u0306\u01FD\u03B2\uD8FF\uDCFF";

			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.AreEqual (28, le.GetByteCount (s), "#A1");
			Assert.AreEqual (0, le.GetByteCount (string.Empty), "#A2");

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.AreEqual (28, be.GetByteCount (s), "#B1");
			Assert.AreEqual (0, be.GetByteCount (string.Empty), "#B2");
		}

		[Test] // GetByteCount (String)
		public void GetByteCount2_S_Null ()
		{
			UTF32Encoding enc = new UTF32Encoding ();
			try {
				enc.GetByteCount ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("s", ex.ParamName, "#5");
			}
		}

		[Test] // GetByteCount (Char *)
		public unsafe void GetByteCount3 ()
		{
			char [] chars = new char[] { 'z', 'a', '\u0306',
				'\u01FD', '\u03B2', '\uD8FF', '\uDCFF' };

			fixed (char* cp = chars) {
				UTF32Encoding le = new UTF32Encoding (false, true);
				Assert.AreEqual (12, le.GetByteCount (cp, 3), "#A1");
				Assert.AreEqual (4, le.GetByteCount (cp, 1), "#A2");
				Assert.AreEqual (0, le.GetByteCount (cp, 0), "#A3");
				Assert.AreEqual (24, le.GetByteCount (cp, 6), "#A4");
				//Assert.AreEqual (24, le.GetByteCount (cp, 7), "#A5");

				UTF32Encoding be = new UTF32Encoding (true, true);
				Assert.AreEqual (12, be.GetByteCount (cp, 3), "#B1");
				Assert.AreEqual (4, be.GetByteCount (cp, 1), "#B2");
				Assert.AreEqual (0, be.GetByteCount (cp, 0), "#B3");
				Assert.AreEqual (24, be.GetByteCount (cp, 6), "#B4");
				//Assert.AreEqual (24, be.GetByteCount (cp, 7), "#B5");
			}
		}

		[Test] // GetByteCount (Char *)
		public unsafe void GetByteCount3_Chars_Null ()
		{
			UTF32Encoding enc = new UTF32Encoding ();
			try {
				enc.GetByteCount ((char *) null, 1);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("chars", ex.ParamName, "#5");
			}
		}

		[Test] // GetByteCount (Char [], Int32, Int32)
		public void GetByteCount4 ()
		{
			char [] chars = new char[] { 'z', 'a', '\u0306',
				'\u01FD', '\u03B2', '\uD8FF', '\uDCFF' };

			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.AreEqual (12, le.GetByteCount (chars, 0, 3), "#A1");
			Assert.AreEqual (16, le.GetByteCount (chars, 2, 4), "#A2");
			Assert.AreEqual (4, le.GetByteCount (chars, 4, 1), "#A3");
			Assert.AreEqual (4, le.GetByteCount (chars, 6, 1), "#A4");
			Assert.AreEqual (0, le.GetByteCount (chars, 6, 0), "#A5");
			Assert.AreEqual (24, le.GetByteCount (chars, 0, 6), "#A6");
			//Assert.AreEqual (24, le.GetByteCount (chars, 0, 7), "#A7");

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.AreEqual (12, be.GetByteCount (chars, 0, 3), "#B1");
			Assert.AreEqual (16, be.GetByteCount (chars, 2, 4), "#B2");
			Assert.AreEqual (4, be.GetByteCount (chars, 4, 1), "#B3");
			Assert.AreEqual (4, be.GetByteCount (chars, 6, 1), "#B4");
			Assert.AreEqual (0, be.GetByteCount (chars, 6, 0), "#B5");
			Assert.AreEqual (24, be.GetByteCount (chars, 0, 6), "#B6");
			//Assert.AreEqual (24, be.GetByteCount (chars, 0, 6), "#B7");
		}

		[Test] // GetByteCount (Char [], Int32, Int32)
		public void GetByteCount4_Chars_Null ()
		{
			UTF32Encoding enc = new UTF32Encoding ();
			try {
				enc.GetByteCount ((Char []) null, 0, 1);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("chars", ex.ParamName, "#5");
			}
		}

		[Test] // GetByteCount (Char [], Int32, Int32)
		public void GetByteCount4_Count_Negative ()
		{
			char [] chars = new char[] { 'z', 'a', '\u0306',
				'\u01FD', '\u03B2', '\uD8FF', '\uDCFF' };

			UTF32Encoding enc = new UTF32Encoding ();
			try {
				enc.GetByteCount (chars, 1, -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("count", ex.ParamName, "#5");
			}
		}

		[Test] // GetByteCount (Char [], Int32, Int32)
		public void GetByteCount4_Count_Overflow ()
		{
			char [] chars = new char[] { 'z', 'a', '\u0306',
				'\u01FD', '\u03B2', '\uD8FF', '\uDCFF' };

			UTF32Encoding enc = new UTF32Encoding ();
			try {
				enc.GetByteCount (chars, 6, 2);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index and count must refer to a location
				// within the buffer
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				//Assert.AreEqual ("chars", ex.ParamName, "#5");
			}
		}

		[Test] // GetByteCount (Char [], Int32, Int32)
		public void GetByteCount4_Index_Negative ()
		{
			char [] chars = new char[] { 'z', 'a', '\u0306',
				'\u01FD', '\u03B2', '\uD8FF', '\uDCFF' };

			UTF32Encoding enc = new UTF32Encoding ();
			try {
				enc.GetByteCount (chars, -1, 1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Non-negative number required
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("index", ex.ParamName, "#5");
			}
		}

		[Test] // GetByteCount (Char [], Int32, Int32)
		public void GetByteCount4_Index_Overflow ()
		{
			char [] chars = new char[] { 'z', 'a', '\u0306',
				'\u01FD', '\u03B2', '\uD8FF', '\uDCFF' };

			UTF32Encoding enc = new UTF32Encoding ();
			try {
				enc.GetByteCount (chars, 7, 1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// Index and count must refer to a location
				// within the buffer
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				//Assert.AreEqual ("chars", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetPreamble ()
		{
			byte[] lePreamble = new UTF32Encoding(false, true).GetPreamble();
			Assert.AreEqual (new byte [] { 0xff, 0xfe, 0, 0 }, lePreamble, "#1");

			byte[] bePreamble = new UTF32Encoding(true, true).GetPreamble();
			Assert.AreEqual (new byte [] { 0, 0, 0xfe, 0xff }, bePreamble, "#2");
		}

		[Test]
		public void IsBrowserDisplay ()
		{
			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.IsFalse (le.IsBrowserDisplay, "#1");

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.IsFalse (be.IsBrowserDisplay, "#2");
		}

		[Test]
		public void IsBrowserSave ()
		{
			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.IsFalse (le.IsBrowserSave);

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.IsFalse (be.IsBrowserSave, "#2");
		}

		[Test]
		public void IsMailNewsDisplay ()
		{
			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.IsFalse (le.IsMailNewsDisplay);

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.IsFalse (be.IsMailNewsDisplay, "#2");
		}

		[Test]
		public void IsMailNewsSave ()
		{
			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.IsFalse (le.IsMailNewsSave);

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.IsFalse (be.IsMailNewsSave, "#2");
		}
	}
}
#endif
