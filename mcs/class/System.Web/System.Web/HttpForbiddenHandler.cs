//
// System.Web.HttpForbiddenHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

namespace System.Web
{
	class HttpForbiddenHandler : IHttpHandler
	{
		public void ProcessRequest (HttpContext context)
		{
			HttpRequest req = context != null ? context.Request : null;
			string path = req != null ? req.Path : null;
			string description = "The type of page you have requested is not served because it has been explicitly forbidden. The extension '" +
				(path == null ? String.Empty : VirtualPathUtility.GetExtension (path)) +
				"' may be incorrect. Please review the URL below and make sure that it is spelled correctly.";
				
			throw new HttpException (403,
						 "This type of page is not served.",
						 req != null ? HttpUtility.HtmlEncode (req.Path) : null,
						 description);
		}

		public bool IsReusable
		{
			get {
				return true;
			}
		}
	}
}

