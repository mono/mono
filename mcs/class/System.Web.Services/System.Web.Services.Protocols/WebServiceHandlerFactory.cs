// 
// System.Web.Services.Protocols.WebServiceHandlerFactory.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class WebServiceHandlerFactory : IHttpHandlerFactory {

		#region Constructors

		[MonoTODO]
		public WebServiceHandlerFactory () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public IHttpHandler GetHandler (HttpContext context, string verb, string url, string filePath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ReleaseHandler (IHttpHandler handler)
		{
			throw new NotImplementedException ();
		}
			
		#endregion // Methods
	}
}
