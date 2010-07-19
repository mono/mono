//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://novell.com)
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
using System.ComponentModel;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class MultilineStringConverterTest
	{
		[Test]
		public void GetPropertiesSupported ()
		{
			var cvt = new MultilineStringConverter ();

			Assert.IsFalse (cvt.GetPropertiesSupported (), "#A1-1");
			Assert.IsFalse (cvt.GetPropertiesSupported (null), "#A1-2");
		}

		[Test]
		public void GetProperties ()
		{
			var cvt = new MultilineStringConverter ();

			Assert.IsNull (cvt.GetProperties (null, null, null), "#A1-1");
			Assert.IsNull (cvt.GetProperties (null, "string", null), "#A1-2");
			Assert.IsNull (cvt.GetProperties (null, "string", new Attribute[] {}), "#A1-1");
		}

		[Test]
		public void ConvertTo ()
		{
			var cvt = new MultilineStringConverter ();

			AssertThrows<ArgumentNullException> (() => {
				cvt.ConvertTo (null, null, "string", null);
			}, "#A1-1");

			AssertThrows<NotSupportedException> (() => {
				cvt.ConvertTo (null, null, "string", typeof (int));
			}, "#A1-2");

			AssertThrows<NotSupportedException> (() => {
				cvt.ConvertTo (null, null, "string", typeof (double));
			}, "#A1-3");

			object result = cvt.ConvertTo (null, null, "string", typeof (string));
			Assert.IsNotNull (result, "#A2-1");
			Assert.IsTrue (result.GetType () == typeof (string), "#A2-2");
			Assert.AreEqual ("(Text)", (string) result, "#A2-3");

			string orig = @"This
is
a
multiline
string";
			result = cvt.ConvertTo (null, null, orig, typeof (string));
			Assert.IsNotNull (result, "#A3-1");
			Assert.IsTrue (result.GetType () == typeof (string), "#A3-2");
			Assert.AreEqual ("(Text)", (string) result, "#A3-3");

			result = cvt.ConvertTo (null, null, 1234, typeof (string));
			Assert.IsNotNull (result, "#A4-1");
			Assert.IsTrue (result.GetType () == typeof (string), "#A4-2");
			Assert.AreEqual ("1234", (string) result, "#A4-3");
		}

		void AssertThrows <ExType> (Action code, string format, params object [] parms) where ExType : Exception
		{
			if (code == null)
				throw new ArgumentNullException ("code");

			bool failed = false;
			try {
				code ();
				failed = true;
			} catch (ExType) {
			} catch (Exception) {
				failed = true;
			}

			if (failed)
				Assert.Fail (format, parms);
		}
	}
}
