//
// System.Runtime.Remoting.Services.RemotingClientProxy.cs
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
#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Runtime.Remoting.Services
{
#if NET_2_0
	[ComVisible (true)]
#endif
	public abstract class RemotingClientProxy: Component
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
