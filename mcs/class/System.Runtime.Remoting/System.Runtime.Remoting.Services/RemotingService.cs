//
// System.Runtime.Remoting.Services.RemotingService.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// 2004 (C) Copyright, Novell, Inc.
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
