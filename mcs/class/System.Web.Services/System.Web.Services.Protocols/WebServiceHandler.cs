// 
// System.Web.Services.Protocols.WebServiceHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Reflection;
using System.Web.Services;

namespace System.Web.Services.Protocols 
{
	internal class WebServiceHandler: IHttpHandler 
	{
		Type _type;
		
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

		public virtual void ProcessRequest (HttpContext context)
		{
		}
		
		protected object CreateServerInstance ()
		{
			return Activator.CreateInstance (ServiceType);
		}

		[MonoTODO]
		protected IAsyncResult BeginCoreProcessRequest (AsyncCallback callback, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void CoreProcessRequest ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void EndCoreProcessRequest (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void WriteReturns (object[] returnValues)
		{
			//protocol.WriteReturns (returnValues, outputStream);
			throw new NotImplementedException ();
		}
	}
}
