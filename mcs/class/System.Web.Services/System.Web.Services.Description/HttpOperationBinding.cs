// 
// System.Web.Services.Description.HttpOperationBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("operation", "http://schemas.xmlsoap.org/wsdl/http/", typeof (OperationBinding))]
	public sealed class HttpOperationBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string location;

		#endregion // Fields

		#region Constructors
		
		public HttpOperationBinding ()
		{
			location = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("location")]
		public string Location { 	
			get { return location; }
			set { location = value; }
		}
	
		#endregion // Properties
	}
}
