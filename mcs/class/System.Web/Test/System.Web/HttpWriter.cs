//
// HttpWriter.cs - HttpWriter tests.
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpWriterTest {

		private HttpWriter writer;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			HttpContext context = new HttpContext (null);
			writer = (HttpWriter) context.Response.Output;
		}

		[Test]
		public void NullWrites ()
		{
			object null_object = null;
			string null_string = null;
			
			writer.Write (null_string);
			writer.Write (null_object);

			writer.WriteString (null, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NullBufferException ()
		{
			writer.Write (null, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WriteInvalidArg1 ()
		{
			char [] x = new char [] { 'a' };
			writer.Write (x, -1, 0);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WriteInvalidArg2 ()
		{
			char [] x = new char [] { 'a' };
			writer.Write (x, 0, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WriteStringInvalidArg1 ()
		{
			writer.WriteString ("hello", 0, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WriteStringInvalidArg2 ()
		{
			writer.WriteString ("hello", -1, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WriteStringInvalidArg3 ()
		{
			writer.WriteString ("hello", 0, 10);
		}
	}
}
