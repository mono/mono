//
// System.Web.Configuration.MachineKeyValidationConverterTest.cs - Unit tests
// for System.Web.Configuration.MachineKeyValidationConverter.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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

#if NET_2_0

using System;
using System.Web.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Web.Configuration {
	[TestFixture]
	public class MachineKeyValidationConverterTest
	{
		[Test]
		public void CanConvertFrom ()
		{
			MachineKeyValidationConverter cv = new MachineKeyValidationConverter ();

			Assert.IsTrue (cv.CanConvertFrom (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (object)), "A4");
		}

		[Test]
		public void CanConvertTo ()
		{
			MachineKeyValidationConverter cv = new MachineKeyValidationConverter ();

			Assert.IsTrue (cv.CanConvertTo (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (object)), "A4");
		}

		[Test]
		public void ConvertFrom ()
		{
			MachineKeyValidationConverter cv = new MachineKeyValidationConverter ();
			object o;

			o = cv.ConvertFrom (null, null, "MD5");
			Assert.AreEqual (typeof (MachineKeyValidation), o.GetType(), "A1");
			Assert.AreEqual ("MD5", o.ToString(), "A2");
			o = cv.ConvertFrom (null, null, "AES");
			Assert.AreEqual ("AES", o.ToString(), "A3");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertFrom_TypeError ()
		{
			MachineKeyValidationConverter cv = new MachineKeyValidationConverter ();
			object o;

			o = cv.ConvertFrom (null, null, 6);
			Assert.IsNull (o, "A1");
		}

		[Test]
		public void ConvertTo ()
		{
			MachineKeyValidationConverter cv = new MachineKeyValidationConverter ();

			Assert.AreEqual ("MD5", cv.ConvertTo (null, null, MachineKeyValidation.MD5, typeof (string)), "A1");
			Assert.AreEqual ("SHA1", cv.ConvertTo (null, null, MachineKeyValidation.SHA1, typeof (string)), "A2");
			Assert.AreEqual ("3DES", cv.ConvertTo (null, null, MachineKeyValidation.TripleDES, typeof (string)), "A3");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConvertTo_NullError ()
		{
			MachineKeyValidationConverter cv = new MachineKeyValidationConverter ();

			Assert.AreEqual ("", cv.ConvertTo (null, null, null, typeof (string)), "A1");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertTo_TypeError1 ()
		{
			MachineKeyValidationConverter cv = new MachineKeyValidationConverter ();

			Assert.AreEqual ("6", cv.ConvertTo (null, null, 6, typeof (string)), "A1");
		}

		[Test]
		public void ConvertTo_TypeError2 ()
		{
			MachineKeyValidationConverter cv = new MachineKeyValidationConverter ();

			Assert.AreEqual ("MD5", cv.ConvertTo (null, null, MachineKeyValidation.MD5, typeof (int)), "A1");
			Assert.AreEqual ("MD5", cv.ConvertTo (null, null, MachineKeyValidation.MD5, null), "A2");
		}

	}
}

#endif
