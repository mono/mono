//
// System.Runtime.Remoting.Services.RemotingService.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// 2004 (C) Copyright, Novell, Inc.
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
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Security.Principal;

namespace System.Runtime.Remoting.Services
{
	public class RemotingService: Component
	{
		public RemotingService ()
		{
		}
		
		[MonoTODO]
		public HttpApplicationState Application
		{
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public HttpContext Context
		{
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public HttpServerUtility Server
		{
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public HttpSessionState Session
		{
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public IPrincipal User
		{
			get { throw new NotImplementedException (); }
		}
	}
}
