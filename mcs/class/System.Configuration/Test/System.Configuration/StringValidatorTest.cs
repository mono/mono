//
// System.Configuration.StringValidatorTest.cs - Unit tests
// for System.Configuration.StringValidator.
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
using System.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class StringValidatorTest
	{
		[Test]
		public void CanValidate ()
		{
			StringValidator v = new StringValidator (5);

			Assert.IsTrue (v.CanValidate (typeof (string)));
			Assert.IsFalse (v.CanValidate (typeof (int)));
			Assert.IsFalse (v.CanValidate (typeof (object)));
		}

		[Test]
		public void NullZero ()
		{
			StringValidator v = new StringValidator (0);

			v.Validate (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Null ()
		{
			StringValidator v = new StringValidator (1);

			v.Validate (null);
		}

		[Test]
		public void MinLenth ()
		{
			StringValidator v = new StringValidator (5);

			v.Validate ("123456789");
			v.Validate ("1234567");
			v.Validate ("12345");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MinLength_fail ()
		{
			StringValidator v = new StringValidator (5);

			v.Validate ("1234");
		}

		[Test]
		public void Bounded ()
		{
			StringValidator v = new StringValidator (5, 7);

			v.Validate ("12345");
			v.Validate ("123456");
			v.Validate ("123457");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Bounded_fail1()
		{
			StringValidator v = new StringValidator (5, 7);

			v.Validate ("1234");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Bounded_fail2()
		{
			StringValidator v = new StringValidator (5, 7);

			v.Validate ("12345678");
		}

		[Test]
		public void Invalid ()
		{
			StringValidator v = new StringValidator (5, 7, "890");

			v.Validate ("123456");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Invalid_fail ()
		{
			StringValidator v = new StringValidator (5, 7, "345");

			v.Validate ("123456");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Invalid_fail_length ()
		{
			StringValidator v = new StringValidator (5, 7, "890");

			v.Validate ("12345678");
		}
	}
}

#endif
