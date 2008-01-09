//
// Mainsoft.Web.Hosting.BaseExternalContext.cs
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2008 Mainsoft Co. (http://www.mainsoft.com)
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
using System.Text;
using javax.faces.context;
using System.Web;
using java.util;

namespace Mainsoft.Web.Hosting
{
	public abstract partial class BaseExternalContext : ExternalContext
	{
		readonly HttpContext _httpContext;
		readonly string _executionFilePath;

		private Map _requestParameterMap;
		private Map _requestParameterValuesMap;

		protected BaseExternalContext (HttpContext httpContext, string executionFilePath) {
			_httpContext = httpContext;
			_executionFilePath = executionFilePath;
		}

		protected HttpContext Context {
			get { return _httpContext; }
		}

		public override Map getRequestParameterMap () {
			return _requestParameterMap ?? (_requestParameterMap = new RequestParameterMap (Context.Request.Form));
		}

		public override Iterator getRequestParameterNames () {
			return new IEnumeratorIteratorImpl (Context.Request.Form.Keys.GetEnumerator ());
		}

		public override Map getRequestParameterValuesMap () {
			return _requestParameterValuesMap ?? (_requestParameterValuesMap = new RequestParameterValuesMap (Context.Request.Form));
		}

		public override string getRequestPathInfo () {
			return _executionFilePath.Substring (getRequestContextPath ().Length);
		}
	}
}
