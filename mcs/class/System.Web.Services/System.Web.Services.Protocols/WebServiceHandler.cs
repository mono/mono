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
using System.Web;
using System.Web.Services;

namespace System.Web.Services.Protocols 
{
	internal class WebServiceHandler: IHttpHandler 
	{
		Type _type;
		HttpContext _context;

		
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

		public virtual void ProcessRequest (HttpContext context)
		{
		}
		
		protected object CreateServerInstance ()
		{
			object ws = Activator.CreateInstance (ServiceType);
			WebService wsi = ws as WebService;
			if (wsi != null) wsi.SetContext (_context);
			return ws;
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
