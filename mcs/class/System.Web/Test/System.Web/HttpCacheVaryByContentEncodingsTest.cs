//
// System.Web.HttpCacheVaryByContentEncodings
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
//

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
using System.Web;
using NUnit.Framework;

namespace MonoTests.System.Web 
{
	[TestFixture]
	public class HttpCacheVaryByContentEncodingsTest
	{
		[Test]
		public void Indexer ()
		{
			HttpResponse response = new HttpResponse (Console.Out);
			HttpCacheVaryByContentEncodings encs = response.Cache.VaryByContentEncodings;

			encs ["gzip"] = true;
			encs ["bzip2"] = false;

			Assert.IsTrue (encs ["gzip"], "gzip == true");
			Assert.IsFalse (encs ["bzip2"], "bzip2 == false");

			bool exceptionCaught = false;
			try {
				encs [null] = true;
			} catch (ArgumentNullException) {
				exceptionCaught = true;
			}
			Assert.IsTrue (exceptionCaught, "ArgumentNullException on this [null] setter");

			exceptionCaught = false;
			try {
				bool t = encs [null];
			} catch (ArgumentNullException) {
				exceptionCaught = true;
			}
			Assert.IsTrue (exceptionCaught, "ArgumentNullException on this [null] getter");
		}
	}
}
