// 
// System.Web.Services.Description.HttpUrlReplacementBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("urlReplacement", "http://schemas.xmlsoap.org/wsdl/http/", typeof (InputBinding))]
	public sealed class HttpUrlReplacementBinding : ServiceDescriptionFormatExtension {

		#region Constructors
		
		public HttpUrlReplacementBinding ()
		{
		}
		
		#endregion // Constructors
	}
}
