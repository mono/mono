//
// System.Configuration.CallbackValidatorTest.cs - Unit tests
// for System.Configuration.CallbackValidator.
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
	public class CallbackValidatorTest
	{
		[Test]
		public void CanValidate ()
		{
			CallbackValidator v = new CallbackValidator (typeof (int), success);

			Assert.IsFalse (v.CanValidate (typeof (string)));
			Assert.IsTrue (v.CanValidate (typeof (int)));
			Assert.IsFalse (v.CanValidate (typeof (object)));
		}

		public void NullCallback ()
		{
			CallbackValidator v = new CallbackValidator (typeof (int), null);
		}

		public void NullType ()
		{
			CallbackValidator v = new CallbackValidator (null, success);
		}

		bool hit_success;
		bool hit_failure;

		void success (object o)
		{
			hit_success = true;
		}

		void failure (object o)
		{
			hit_failure = true;
			throw new Exception ();
		}

		[Test]
		public void TestSuccess ()
		{
			hit_success = false;
			CallbackValidator v = new CallbackValidator (typeof (int), success);
			v.Validate (5);

			Assert.IsTrue (hit_success, "A1");
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void TestFailure1 ()
		{
			CallbackValidator v = new CallbackValidator (typeof (int), failure);
			v.Validate (5);
		}

		[Test]
		public void TestFailure2 ()
		{
			hit_failure = false;
			CallbackValidator v = new CallbackValidator (typeof (int), failure);
			try {
				v.Validate (5);
			}
			catch { }
			finally {
				Assert.IsTrue (hit_failure, "A1");
			}
		}
	}

}

#endif
