// 
// System.Web.Services.Discovery.DiscoveryRequestHandler.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.Web;

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryRequestHandler : IHttpHandler {
		
		#region Fields

		private bool isReusable = true;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public DiscoveryRequestHandler () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public bool IsReusable {
			get { return isReusable; }			
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void ProcessRequest (HttpContext context)
		{
                        throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
