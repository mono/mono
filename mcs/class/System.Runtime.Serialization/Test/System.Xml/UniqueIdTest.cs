//
// UniqueIdTest.cs
//
// Author:
//   Atsushi Enomoto <atsushi@ximian.com>
//   Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2006, 2009 Novell, Inc.  http://www.novell.com
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
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class UniqueIdTest
	{
		[Test]
		public void TestDefault ()
		{
			UniqueId id = new UniqueId ();
			Assert.IsTrue (id.IsGuid, "#1");

			Guid g = Guid.NewGuid ();

			UniqueId a = new UniqueId (g);
			UniqueId b = new UniqueId (g.ToByteArray ());

			Assert.AreEqual (a, b, "#2");
			Assert.AreEqual ("urn:uuid:", a.ToString ().Substring (0, 9), "#3");

			a = new UniqueId ("foo");
			Assert.AreEqual ("foo", a.ToString (), "#4");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ZeroLengthCtor ()
		{
			new UniqueId ("");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNull1 ()
		{
			new UniqueId ((string) null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_IdNull ()
		{
			new UniqueId (null, 0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Ctor_OffsetNegative ()
		{
			new UniqueId (new byte[0], -1);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void Ctor_OffsetTooLarge ()
		{
			new UniqueId (new byte[16], 16);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void Ctor_BufferTooSmall ()
		{
			new UniqueId (new byte[15], 0);
		}

		[Test]
		public void Ctor_Id ()
		{
			byte[] buf = Encoding.UTF8.GetBytes ("Hello!")
				.Concat (new Guid ().ToByteArray ()).ToArray ();
			var g = new UniqueId (buf, "Hello!".Length);
			Assert.AreEqual (new UniqueId (new Guid ()), g);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_CharsNull ()
		{
			new UniqueId (null, 0, 1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Ctor_Chars_OffsetNegative ()
		{
			new UniqueId (new char[2], -1, 1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Ctor_Chars_OffsetTooLarge ()
		{
			new UniqueId (new char[2], 2, 1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Ctor_Chars_CountNegative ()
		{
			new UniqueId (new char[2], 0, -1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Ctor_Chars_CountTooLarge ()
		{
			new UniqueId (new char[2], 1, 2);
		}

		[Test, ExpectedException (typeof (FormatException))]
		public void Ctor_Chars_CountZero ()
		{
			new UniqueId (new char[2], 0, 0);
		}

		[Test]
		public void Ctor_Chars ()
		{
			var a = new UniqueId ("Hello!".ToCharArray (), 0, 5);
			Assert.IsFalse (a.IsGuid);
			Assert.AreEqual ("Hello", a.ToString ());

			a = new UniqueId ();
			var b = new UniqueId (a.ToString ().ToCharArray (), 0, 45);
			Assert.IsTrue (b.IsGuid);
			Assert.AreEqual (a, b);

			string s = "foo" + a.ToString () + "bar";
			b = new UniqueId (s.ToCharArray (), 3, s.Length-6);
			Assert.IsTrue (b.IsGuid);
			Assert.AreEqual (a, b);

			a = new UniqueId (new Guid ());
			b = new UniqueId (a.ToString ().ToCharArray (), 0, 45);
			Assert.IsFalse (b.IsGuid);
			Assert.AreEqual (a, b);
		}

		[Test]
		public void CharArrayLength ()
		{
			var u = new UniqueId ("string");
			Assert.AreEqual (6, u.CharArrayLength);
			Assert.AreEqual (u.ToString().Length, u.CharArrayLength);

			u = new UniqueId (new Guid());
			Assert.AreEqual (45, u.CharArrayLength);
			Assert.AreEqual (u.ToString().Length, u.CharArrayLength);
		}

		[Test]
		public void Equals ()
		{
			var a = new UniqueId ("a");
			var b = new UniqueId (new Guid ());
			Assert.IsFalse (a.Equals (null));
			Assert.IsFalse (b.Equals (null));
			Assert.IsFalse (a.Equals (b));
			Assert.IsFalse (a == b);
			Assert.IsTrue (a != b);

			var c = new UniqueId ("a");
			Assert.IsTrue (a.Equals (c));
			Assert.IsTrue (a == c);
		}

		[Test]
		public new void GetHashCode ()
		{
			Assert.AreEqual ("a".GetHashCode (), new UniqueId ("a").GetHashCode ());
			// TODO: What does .NET do for UniqueId.GetHashCode() when UniqueId.IsGuid==true?
			// Assert.AreEqual (new Guid ().GetHashCode (), new UniqueId (new Guid ()).GetHashCode ());
		}

		[Test]
		public void IsGuid ()
		{
			var a = new UniqueId ("string");
			Assert.IsFalse (a.IsGuid);

			a = new UniqueId ();
			Assert.IsTrue (a.IsGuid);

			a = new UniqueId (new Guid ());
			Assert.IsFalse (a.IsGuid);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToCharArray_CharsNull ()
		{
			new UniqueId ("s").ToCharArray (null, 0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToCharArray_CharsTooSmall ()
		{
			new UniqueId ().ToCharArray (new char[15], 0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToCharArray_OffsetNegative ()
		{
			new UniqueId ("s").ToCharArray (new char[1], -1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToCharArray_OffsetOutOfRange ()
		{
			new UniqueId ("s").ToCharArray (new char[1], 2);
		}

		[Test]
		public void ToCharArray ()
		{
			char[] chars = new char[4];
			Assert.AreEqual (4, new UniqueId ("data").ToCharArray (chars, 0));
			Assert.AreEqual ("data".ToCharArray (), chars);

			chars = new char[45];
			Assert.AreEqual (45, new UniqueId (new Guid ()).ToCharArray (chars, 0));
			Assert.AreEqual (("urn:uuid:" + new Guid ().ToString ()).ToCharArray (), chars);
		}

		[Test]
		public new void ToString ()
		{
			Assert.AreEqual ("string", new UniqueId ("string").ToString ());
			Assert.AreEqual (
					"urn:uuid:00000000-0000-0000-0000-000000000000",
					new UniqueId (new Guid ()).ToString ());
			Guid g = Guid.NewGuid ();
			Assert.AreEqual ("urn:uuid:" + g.ToString (), new UniqueId (g).ToString ());
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void TryGetGuid_BufferNull ()
		{
			new UniqueId ().TryGetGuid (null, 0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TryGetGuid_BufferTooSmall ()
		{
			new UniqueId ().TryGetGuid (new byte[16], 1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TryGetGuid_OffsetNegative ()
		{
			new UniqueId ().TryGetGuid (new byte[16], -1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TryGetGuid_OffsetTooLarge ()
		{
			new UniqueId ().TryGetGuid (new byte[16], 16);
		}

		[Test]
		public void TryGetGuid ()
		{
			Guid g;
			Assert.IsFalse (new UniqueId ("string").TryGetGuid (out g));
			Assert.IsFalse (new UniqueId ("string").TryGetGuid (new byte [16], 0));
			Assert.IsFalse (new UniqueId (new Guid ()).TryGetGuid (out g));
			Assert.IsFalse (new UniqueId (new Guid ()).TryGetGuid (new byte [16], 0));

			g = Guid.NewGuid ();
			Guid g2;
			byte[] bg;
			Assert.IsTrue (new UniqueId (g).TryGetGuid (out g2));
			Assert.AreEqual (g, g2);
			Assert.IsTrue (new UniqueId (g).TryGetGuid (bg = new byte [17], 1));
			Assert.AreEqual (g, new Guid (bg.Skip (1).ToArray ()));
		}
	}
}
