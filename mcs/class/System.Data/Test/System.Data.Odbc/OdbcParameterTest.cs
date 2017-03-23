//
// OdbcParameterTest.cs - NUnit Test Cases for testing the
// OdbcParameter class
//
// Author:
//      Sureshkumar T (TSureshkumar@novell.com)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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

#if !NO_ODBC

using System;
using System.Text;
using System.Data;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
{

        [TestFixture]
        public class OdbcParameterTest
        {
                [Test]
                public void OdbcTypeTest ()
                {
			OdbcParameter param = new OdbcParameter ();
			Assert.AreEqual (OdbcType.NVarChar, param.OdbcType, "#1");

			// change test
			param.OdbcType = OdbcType.Int;
			Assert.AreEqual (OdbcType.Int, param.OdbcType, "#2");

			param = new OdbcParameter ("test", 10);
			Assert.AreEqual (OdbcType.NVarChar, param.OdbcType, "#3");
			param.OdbcType = OdbcType.Real;
			Assert.AreEqual (OdbcType.Real, param.OdbcType, "#4");
			Assert.AreEqual (10, param.Value, "#5");

			param = new OdbcParameter ("test", OdbcType.NText);
			Assert.AreEqual (null, param.Value, "#6");
			Assert.AreEqual (OdbcType.NText, param.OdbcType, "#7");

			param = new OdbcParameter ("test", OdbcType.Binary);
			Assert.AreEqual (null, param.Value, "#8");
			Assert.AreEqual (OdbcType.Binary, param.OdbcType, "#9");
                }

        }
}

#endif