//
// System.Runtime.Remoting.Services.RemotingClientProxy.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// 2004 (C) Copyright, Novell, Inc.
//

using System;
using System.ComponentModel;

namespace System.Runtime.Remoting.Services
{
	public class RemotingClientProxy: Component
	{
		protected object _tp;
		protected Type _type;
		protected string _url;
		
		protected RemotingClientProxy()
		{
		}
		
		[MonoTODO]
		public bool AllowAutoRedirect 
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public object Cookies
		{
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string Domain
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public bool EnableCookies
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string Password
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string Path
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public bool PreAuthenticate
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public string ProxyName
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public int ProxyPort
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public int Timeout
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public string Url
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public string UserAgent
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public string Username
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		protected void ConfigureProxy (Type type, string url)
		{
		}
		
		[MonoTODO]
		protected void ConnectProxy()
		{
		}
	}
}
