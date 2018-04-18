// Authors:
//      Marek Habersack <mhabersack@novell.com>
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Util;

using NUnit.Framework;

namespace MonoTests.System.Web.Util
{	
	class TestRequestValidator : RequestValidator
	{
		public bool DoIsValidRequestString (HttpContext context, string value, RequestValidationSource requestValidationSource,
						string collectionKey, out int validationFailureIndex)
		{
			return IsValidRequestString (context, value, requestValidationSource, collectionKey, out validationFailureIndex);
		}
	}

	[TestFixture]
	public class RequestValidatorTest
	{
		class FakeHttpWorkerRequest : BaseFakeHttpWorkerRequest
		{
			Uri uri;

			public FakeHttpWorkerRequest (string url)
			{
				uri = new Uri (url, UriKind.Absolute);
			}

			public override string GetQueryString ()
			{
				return uri.Query;
			}

			public override string GetUriPath ()
			{
				return uri.AbsolutePath;
			}
		}

		[Test]
		public void Current ()
		{
			Assert.IsNotNull (RequestValidator.Current, "#A1");
			Assert.AreEqual (typeof (RequestValidator).AssemblyQualifiedName, RequestValidator.Current.GetType ().AssemblyQualifiedName, "#A2");
			Assert.Throws<ArgumentNullException> (() => {
				RequestValidator.Current = null;
			}, "#A3");

			RequestValidator.Current = new TestRequestValidator ();
			Assert.IsNotNull (RequestValidator.Current, "#B1");
			Assert.AreEqual (typeof (TestRequestValidator).AssemblyQualifiedName, RequestValidator.Current.GetType ().AssemblyQualifiedName, "#B2");
		}

		[Test]
		public void IsValidRequestString ()
		{
			var rv = new TestRequestValidator ();
			int validationFailureIndex;
			var fr = new FakeHttpWorkerRequest ("http://localhost/default.aspx?key=invalid%value");
			var ctx = new HttpContext (fr);

			Assert.IsFalse (rv.DoIsValidRequestString (null, "<script>invalid%value", RequestValidationSource.QueryString, "key", out validationFailureIndex), "#A1");
			Assert.IsFalse (rv.DoIsValidRequestString (ctx, "<script>invalid%value", RequestValidationSource.QueryString, "key", out validationFailureIndex), "#A2");
		}
	}
}
