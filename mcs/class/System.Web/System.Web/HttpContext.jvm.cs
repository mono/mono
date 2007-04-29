//
// System.Web.HttpContext.cs 
//
// Author:
//	Eyal Alaluf (eyala@mainsoft.com)
//

//
// Copyright (C) 2005 Mainsoft Co. (http://www.mainsoft.com)
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

using System.Collections;
using System.Configuration;
using System.Threading;
using javax.servlet.http;
using vmw.@internal.j2ee;

namespace System.Web {
	
	public sealed partial class HttpContext {
		static LocalDataStoreSlot _ContextSlot = Thread.GetNamedDataSlot ("Context");
		// No remoting support (CallContext) yet in Grasshopper
		[MonoInternalNote("Context - Use System.Remoting.Messaging.CallContext instead of Thread storage")]
		public static HttpContext Current
		{
			get { return (HttpContext) Thread.GetData (_ContextSlot); }
			set { Thread.SetData (_ContextSlot, value); }
		}

		public bool IsDebuggingEnabled { get { return false; } }

		internal object GetWorkerService(Type t)
		{
			IServiceProvider prv = WorkerRequest as IServiceProvider;
			return prv != null ? prv.GetService(t) : null;
		}

		internal HttpServlet Servlet {
			get { return (HttpServlet)GetWorkerService(typeof(HttpServlet)); }
		}

		internal HttpServletRequest ServletRequest {
			get { return (HttpServletRequest)GetWorkerService(typeof(HttpServletRequest)); }
		}

		internal HttpServletResponse ServletResponse {
			get { return (HttpServletResponse)GetWorkerService(typeof(HttpServletResponse)); }
		}

		HttpRuntime _httpRuntime = null;
		internal HttpRuntime HttpRuntimeInstance {
			get
			{
				if (_httpRuntime == null)
					_httpRuntime = (HttpRuntime) AppDomain.CurrentDomain.GetData ("HttpRuntime");
				return _httpRuntime;
			}
		}

		// Timeout is not supported in GH
		internal bool CheckIfTimeout (DateTime t)
		{
			return false;
		}

		internal bool TimeoutPossible
		{
			get { return true; }
		}

		internal bool IsPortletRequest { get { return ServletRequest is IPortletRequest; } }

		internal void BeginTimeoutPossible ()
		{
		}

		internal void EndTimeoutPossible ()
		{
		}
	}
}
