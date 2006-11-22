// 
// System.Web.Services.Protocols.WebServiceHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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
using System.Reflection;
using System.Web;
using System.Web.Services;
using System.Web.SessionState;

namespace System.Web.Services.Protocols 
{
	internal class WebServiceHandler: IHttpHandler 
	{
		Type _type;
		HttpContext _context;
		HttpSessionState session;

		
		public WebServiceHandler (Type type)
		{
			_type = type;
		}

		public Type ServiceType
		{
			get { return _type; }
		}
		
		public virtual bool IsReusable 
		{
			get { return false; }
		}

		protected HttpContext Context {
			set { _context = value; }
		}

		protected HttpSessionState Session {
			set { this.session = value; }
		}

		internal virtual MethodStubInfo GetRequestMethod (HttpContext context)
		{
			return null;
		}
		
		public virtual void ProcessRequest (HttpContext context)
		{
		}
		
		protected object CreateServerInstance ()
		{
			return Activator.CreateInstance (ServiceType);
		}
	}
}

