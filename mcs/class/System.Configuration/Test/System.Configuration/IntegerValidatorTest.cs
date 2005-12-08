//
// System.Configuration.IntegerValidatorTest.cs - Unit tests
// for System.Configuration.IntegerValidator.
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
	public class IntegerValidatorTest
	{
		[Test]
		public void CanValidate ()
		{
			IntegerValidator v = new IntegerValidator (1, 1);

			Assert.IsFalse (v.CanValidate (typeof (TimeSpan)), "A1");
			Assert.IsTrue (v.CanValidate (typeof (int)), "A2");
			Assert.IsFalse (v.CanValidate (typeof (long)), "A3");
		}

		[Test]
		public void Validate_inRange ()
		{
			IntegerValidator v = new IntegerValidator (5000, 10000);
			v.Validate (7000);
		}

		[Test]
		public void Validate_Inclusive ()
		{
			IntegerValidator v = new IntegerValidator (5000, 10000, false);
			v.Validate (5000);
			v.Validate (10000);
		}

		[Test]
		public void Validate_Exclusive ()
		{
			IntegerValidator v = new IntegerValidator (5000, 10000, true);
			v.Validate (1000);
			v.Validate (15000);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Validate_Exclusive_fail1 ()
		{
			IntegerValidator v = new IntegerValidator (5000, 10000, true);
			v.Validate (5000);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Validate_Exclusive_fail2 ()
		{
			IntegerValidator v = new IntegerValidator (5000, 10000, true);
			v.Validate (10000);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Validate_Exclusive_fail3 ()
		{
			IntegerValidator v = new IntegerValidator (5000, 10000, true);
			v.Validate (7000);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Validate_Resolution ()
		{
			IntegerValidator v = new IntegerValidator (20000,
								   50000,
								   false,
								   3000);

			v.Validate (40000);
		}
	}
}

#endif
