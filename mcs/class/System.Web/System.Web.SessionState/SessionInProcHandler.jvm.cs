//
// System.Web.SessionState.SesionInProchandler.jvm.cs
//
// Authors:
//  Ilya Kharmatsky (ilyak@mainsoft.com)
//  Alon Gazit
//  Pavel Sandler
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.IO;
using System.Collections;
using System.Web;
using System.Web.Hosting;

namespace System.Web.SessionState
{

	class SessionInProcHandler : ISessionHandler
	{

		SessionConfig config;

		public void Dispose () { }

		public void Init (SessionStateModule module, HttpApplication context, SessionConfig config)
		{
			this.config = config;
		}

		public void UpdateHandler (HttpContext context, SessionStateModule module) { }

		public HttpSessionState UpdateContext (HttpContext context, SessionStateModule module,
							bool required, bool read_only, ref bool isNew)
		{

			if (!required)
				return null;

			ServletWorkerRequest worker = (ServletWorkerRequest)context.Request.WorkerRequest;
			javax.servlet.http.HttpSession javaSession = worker.ServletRequest.getSession(false);
			HttpSessionState state;
			if (javaSession != null) 
			{
				state = (HttpSessionState) javaSession.getAttribute("GH_SESSION_STATE");
 			    return state;
			}
			
			// We create a new session.
			javaSession = worker.ServletRequest.getSession(true);
			string sessionID = javaSession.getId();
			state = new HttpSessionState (sessionID, // unique identifier
					new SessionDictionary(), // dictionary
					HttpApplicationFactory.ApplicationState.SessionObjects,
					config.Timeout, //lifetime before death.
					true, //new session
					false, // is cookieless
					SessionStateMode.J2ee,
					read_only); //readonly
					
			javaSession.setAttribute("GH_SESSION_STATE", state);
			
			isNew = true;
			return state;
		}
	}
}

