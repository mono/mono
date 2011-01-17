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

using Microsoft.Web.Infrastructure.DynamicValidationHelper;
using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Security;
using System.Web;

namespace Microsoft.Web.Infrastructure.DynamicValidationHelper
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	public static class ValidationUtility
	{
		[SecuritySafeCritical]
		public static void EnableDynamicValidation (HttpContext context)
		{
			// No-op on Mono. We just split form/cookie/query string validation in two
			// parts and the code in GetUnvalidatedCollections below accesses the first,
			// unvalidated part.
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
			
			// This is *very* simplified. We should probably create a wrapper class
			// which would iterate over the collections on demand
			formGetter = () => {
				HttpRequest req = context != null ? context.Request : null;
				if (req == null)
					return null;
				return req.FormUnvalidated;
			};

			queryStringGetter = () => {
				HttpRequest req = context != null ? context.Request : null;
				if (req == null)
					return null;
				return req.QueryStringUnvalidated;
			};
		}
	}
}
