// 
// System.Web.Services.Protocols.HttpPostClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Net;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class HttpPostClientProtocol : HttpSimpleClientProtocol {

		#region Constructors

		public HttpPostClientProtocol () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		protected override WebRequest GetWebRequest (Uri uri)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
