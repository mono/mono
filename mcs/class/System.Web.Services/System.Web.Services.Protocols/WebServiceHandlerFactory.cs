// 
// System.Web.Services.Protocols.WebServiceHandlerFactory.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//	 Dave Bettin (dave@opendotnet.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Web.UI;


namespace System.Web.Services.Protocols {
	public class WebServiceHandlerFactory : IHttpHandlerFactory {

		#region Constructors

		public WebServiceHandlerFactory () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public IHttpHandler GetHandler (HttpContext context, string verb, string url, string filePath)
		{
			Type type = WebServiceParser.GetCompiledType(filePath, context);
			throw new NotImplementedException ();
		}


		public void ReleaseHandler (IHttpHandler handler)
		{
		}
			
		#endregion // Methods
	}
}
