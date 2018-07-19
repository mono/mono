//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
	public class HttpFileCollectionWrapperTest
	{
		[Test]
		public void Get_String ()
		{
			var req = new HttpRequest ("default.aspx", "http://localhost/default.aspx", String.Empty);
			var files = req.Files;
			var wrapper = new HttpFileCollectionWrapper (files);

			Assert.IsNull (wrapper.Get ("DoesNotExist"), "#A1");
			Assert.IsNull (wrapper.Get (null), "#A2");
		}

		[Test]
		public void Get_Int ()
		{
			var req = new HttpRequest ("default.aspx", "http://localhost/default.aspx", String.Empty);
			var files = req.Files;
			var wrapper = new HttpFileCollectionWrapper (files);

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				wrapper.Get (0);
			}, "#A1");
		}

		[Test]
		public void Item_String ()
		{
			var req = new HttpRequest ("default.aspx", "http://localhost/default.aspx", String.Empty);
			var files = req.Files;
			var wrapper = new HttpFileCollectionWrapper (files);

			Assert.IsNull (wrapper ["DoesNotExist"], "#A1");
			Assert.IsNull (wrapper [null], "#A2");
		}

		[Test]
		public void Item_Int ()
		{
			var req = new HttpRequest ("default.aspx", "http://localhost/default.aspx", String.Empty);
			var files = req.Files;
			var wrapper = new HttpFileCollectionWrapper (files);

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				var f = wrapper [0];
			}, "#A1");
		}
	}
}
