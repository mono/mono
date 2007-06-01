//
// DateTimeFormatInfo.cs
//
// Authors:
//     Ben Maurer <bmaurer@andrew.cmu.edu>
//
// Copyright (C) 2005 Novell (http://www.novell.com)
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


using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;

namespace MonoTests.System.Globalization {

        [TestFixture]
        public class DateTimeFormatInfoTest : Assertion
        {
              
                [Test]
                public void GetAllDateTimePatterns_ret_diff_obj ()
                {
			// We need to return different objects for security reasons
			DateTimeFormatInfo dtfi = CultureInfo.InvariantCulture.DateTimeFormat;

			string [] one = dtfi.GetAllDateTimePatterns ();
			string [] two = dtfi.GetAllDateTimePatterns ();
			Assert (one != two);
                }

		[Test]
		public void EraName ()
		{
			CultureInfo en_US = new CultureInfo ("en-US");
			DateTimeFormatInfo dtfi = en_US.DateTimeFormat;
			AssertEquals ("#1", "AD", dtfi.GetAbbreviatedEraName (0));
			AssertEquals ("#2", "A.D.", dtfi.GetEraName (1));
			AssertEquals ("#3", 1, dtfi.GetEra ("A.D."));
			AssertEquals ("#4", 1, dtfi.GetEra ("AD"));
			AssertEquals ("#5", -1, dtfi.GetEra ("C.E"));
			AssertEquals ("#6", -1, dtfi.GetEra ("Common Era"));
		}
#if !TARGET_JVM
		[Test]
		public void Bug78569 ()
		{
			DateTime dt = DateTime.Now;
			CultureInfo ci = new CultureInfo ("en-GB");
			string s = dt.ToString (ci);
			DateTime dt2 = DateTime.Parse (s, ci);
			AssertEquals ("1", dt.Month, dt2.Month);
		}
#endif
        }
}


