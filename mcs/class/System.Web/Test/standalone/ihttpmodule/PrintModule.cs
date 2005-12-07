//
// PrintModule
//
// Author:
//	Gonzalo Paniagua (gonzalo@ximian.com)
//    
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace Mono.Http.Modules
{
	public class PrintModule : IHttpModule
	{
		class X {
			string msg;

			public X (string message)
			{
				this.msg = message;
			}

			public void Hook (object sender, EventArgs args)
			{
				HttpApplication app = (HttpApplication) sender;
				Console.WriteLine ("{0}:{1}:{2}", app.GetHashCode (), app.Context.Request.Url, msg);
			}
		}

		public PrintModule ()
		{
			Console.WriteLine ("Instance created");
		}

		public void Init (HttpApplication app)
		{
			app.PreSendRequestHeaders += new EventHandler (new X ("PreSendRequestHeaders").Hook);
			app.PreSendRequestContent += new EventHandler (new X ("PreSendRequestContent").Hook);
			app.AcquireRequestState += new EventHandler (new X ("AcquireRequestState").Hook);
			app.AuthenticateRequest += new EventHandler (new X ("AuthenticateRequest").Hook);
			app.AuthorizeRequest += new EventHandler (new X ("AuthorizeRequest").Hook);
			app.BeginRequest += new EventHandler (new X ("BeginRequest").Hook);
			app.EndRequest += new EventHandler (new X ("EndRequest").Hook);
			app.PostRequestHandlerExecute += new EventHandler (new X ("PostRequestHandlerExecute").Hook);
			app.PreRequestHandlerExecute += new EventHandler (new X ("PreRequestHandlerExecute").Hook);
			app.ReleaseRequestState += new EventHandler (new X ("ReleaseRequestState").Hook);
			app.ResolveRequestCache += new EventHandler (new X ("ResolveRequestCache").Hook);
			app.UpdateRequestCache += new EventHandler (new X ("UpdateRequestCache").Hook);
#if NET_2_0
			app.PostAuthenticateRequest += new EventHandler (new X ("PostAuthenticateRequest").Hook);
			app.PostAuthorizeRequest += new EventHandler (new X ("PostAuthorizeRequest").Hook);
			app.PostResolveRequestCache += new EventHandler (new X ("PostResolveRequestCache").Hook);
			app.PostMapRequestHandler += new EventHandler (new X ("PostMapRequestHandler").Hook);
			app.PostAcquireRequestState += new EventHandler (new X ("PostAcquireRequestState").Hook);
			app.PostReleaseRequestState += new EventHandler (new X ("PostReleaseRequestState").Hook);
			app.PostUpdateRequestCache += new EventHandler (new X ("PostUpdateRequestCache").Hook);
#endif
		}

		public void Dispose ()
		{
		}
	}
}

