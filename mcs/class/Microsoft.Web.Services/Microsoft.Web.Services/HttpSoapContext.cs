//
// HttpSoapContext.cs: Http Soap Contexts
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services {

	public sealed class HttpSoapContext {

		// must have an internal one or a public one will be created (winchurn)
		internal HttpSoapContext () {}

		[MonoTODO("ASP.NET related")]
		public static SoapContext RequestContext { 
			get { return null; }
		}

		[MonoTODO("ASP.NET related")]
		public static SoapContext ResponseContext { 
			get { return null; }
		}
	}
}
