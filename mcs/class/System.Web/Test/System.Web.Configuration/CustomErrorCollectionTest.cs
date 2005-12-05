//
// CustomErrorCollectionTest.cs 
//	- unit tests for System.Web.Configuration.CustomErrorCollection
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

using NUnit.Framework;

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class CustomErrorCollectionTest
	{
		[Test]
		public void GetKey ()
		{
			CustomErrorCollection col = new CustomErrorCollection ();

			col.Add (new CustomError (404, "http://404-error.com/"));

			Assert.AreEqual ("404", col.GetKey (0), "A1");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void GetKey_OutOfRange ()
		{
			CustomErrorCollection col = new CustomErrorCollection ();
			Assert.IsNull (col.GetKey(0), "A1");
		}

		[Test]
		public void Set ()
		{
			CustomErrorCollection col = new CustomErrorCollection ();

			col.Add (new CustomError (404, "http://404-error.com/"));
			col.Add (new CustomError (403, "http://403-error.com/"));
			col.Add (new CustomError (999, "http://403-error.com/"));

			col.Set (new CustomError (403, "http://403-error2.com/"));

			Assert.AreEqual (3, col.Count, "A1");
			Assert.AreEqual (3, col.AllKeys.Length, "A2");

			Assert.AreEqual ("http://403-error2.com/", col[1].Redirect, "A3");
		}

		[Test]
		public void SetToDuplicate ()
		{
			CustomErrorCollection col = new CustomErrorCollection ();

			col.Add (new CustomError (404, "http://404-error.com/"));
			col.Add (new CustomError (403, "http://403-error.com/"));
			col.Add (new CustomError (999, "http://403-error.com/"));

			/* override the 999 entry with a duplicate 403 */
			col[2] = new CustomError (403, "http://403-error2.com/");

			Assert.AreEqual (3, col.Count, "A1");
			Assert.AreEqual (3, col.AllKeys.Length, "A2");

			Assert.AreEqual (403, col[1].StatusCode, "A3");
			Assert.AreEqual ("http://403-error.com/", col[1].Redirect, "A4");
			Assert.AreEqual (403, col[2].StatusCode, "A5");
			Assert.AreEqual ("http://403-error2.com/", col[2].Redirect, "A6");
		}

		[Test]
		public void DuplicateKeyInAdd ()
		{
			CustomErrorCollection col = new CustomErrorCollection ();

			col.Add (new CustomError (404, "http://404-error.com/"));
			col.Add (new CustomError (404, "http://403-error.com/"));

			Assert.AreEqual (1, col.Count, "A1");
			Assert.AreEqual (1, col.AllKeys.Length, "A2");

			Assert.AreEqual ("http://403-error.com/", col[0].Redirect, "A3");
		}

		[Test]
		public void AllKeys ()
		{
			CustomErrorCollection col = new CustomErrorCollection ();

			col.Add (new CustomError (404, "http://404-error.com/"));
			col.Add (new CustomError (403, "http://403-error.com/"));
			col.Add (new CustomError (999, "http://403-error.com/"));

			Assert.AreEqual (3, col.AllKeys.Length, "A1");
			Assert.AreEqual ("404", col.AllKeys[0], "A2");
			Assert.AreEqual ("403", col.AllKeys[1], "A3");
			Assert.AreEqual ("999", col.AllKeys[2], "A4");
		}
	}
}

#endif
