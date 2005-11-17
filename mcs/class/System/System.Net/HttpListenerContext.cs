//
// System.Net.HttpListenerContext
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;
namespace System.Net {
	public sealed class HttpListenerContext {
		HttpListenerRequest request;
		HttpListenerResponse response;
		IPrincipal user;
		HttpConnection cnc;
		string error;
		int err_status = 400;
		internal HttpListener Listener;

		internal HttpListenerContext (HttpConnection cnc)
		{
			this.cnc = cnc;
			request = new HttpListenerRequest (this);
			response = new HttpListenerResponse (this);
		}

		internal int ErrorStatus {
			get { return err_status; }
			set { err_status = value; }
		}

		internal string ErrorMessage {
			get { return error; }
			set { error = value; }
		}

		internal bool HaveError {
			get { return (error != null); }
		}

		internal HttpConnection Connection {
			get { return cnc; }
		}

		public HttpListenerRequest Request {
			get { return request; }
		}

		public HttpListenerResponse Response {
			get { return response; }
		}

		public IPrincipal User {
			get { return user; }
		}
	}
}
#endif

