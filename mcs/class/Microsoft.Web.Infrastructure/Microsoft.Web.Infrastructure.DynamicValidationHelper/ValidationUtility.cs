// 
// ValidationUtility.cs
//  
// Author:
//       Marek Habersack <grendel@twistedcode.net>
// 
// Copyright (c) 2011 Novell, Inc (http://novell.com/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Security;
using System.Web;
using System.Web.Util;

namespace Microsoft.Web.Infrastructure.DynamicValidationHelper
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	public static class ValidationUtility
	{
		private const string UNVALIDATED_DATA_KEY = "__MWI_UNVALIDATED_DATA_KEY";

		[SecuritySafeCritical]
		public static void EnableDynamicValidation (HttpContext context)
		{
			HttpRequest req = context != null ? context.Request : null;
			if (req == null)
				return;

			// We might get called more than once.. (weird, isnt it?)
			if (context.Items [UNVALIDATED_DATA_KEY] != null)
				return;

			// Store unvalidated values at context so we can access them later.
			context.Items [UNVALIDATED_DATA_KEY] = new object [] { req.FormUnvalidated, req.QueryStringUnvalidated };

			// Just to be safe, make sure it's on
			req.ValidateInput ();
			req.SetFormCollection (new LazyWebROCollection (RequestValidationSource.Form, req.FormUnvalidated), true);
			req.SetQueryStringCollection (new LazyWebROCollection (RequestValidationSource.QueryString, req.QueryStringUnvalidated), true);
		}

		[SecuritySafeCritical]
		public static bool? IsValidationEnabled (HttpContext context)
		{
			HttpRequest req = context != null ? context.Request : null;
			if (req == null)
				return true;

			return req.InputValidationEnabled;
		}

		[SecuritySafeCritical]
		public static void GetUnvalidatedCollections (HttpContext context, out Func <NameValueCollection> formGetter, out Func <NameValueCollection> queryStringGetter)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			formGetter = null;
			queryStringGetter = null;
			
			formGetter = () => {
				HttpRequest req = context != null ? context.Request : null;
				if (req == null)
					return null;
				return GetUnvalidatedCollection (context, 0) ?? req.FormUnvalidated;
			};

			queryStringGetter = () => {
				HttpRequest req = context != null ? context.Request : null;
				if (req == null)
					return null;
				return GetUnvalidatedCollection (context, 1) ?? req.QueryStringUnvalidated;
			};
		}

		private static NameValueCollection GetUnvalidatedCollection (HttpContext context, int offset)
		{
			if (context.Items [UNVALIDATED_DATA_KEY] == null)
				return null;

			var data = context.Items [UNVALIDATED_DATA_KEY] as object [];
			return data [offset] as NameValueCollection;
		}
	}
}
