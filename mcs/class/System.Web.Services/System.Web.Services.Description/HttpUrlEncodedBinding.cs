// 
// System.Web.Services.Description.HttpUrlEncodedBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("urlEncoded", "http://schemas.xmlsoap.org/wsdl/http/", typeof (InputBinding))]
	public sealed class HttpUrlEncodedBinding : ServiceDescriptionFormatExtension {

		#region Constructors
		
		public HttpUrlEncodedBinding ()
		{
		}
		
		#endregion // Constructors
	}
}
