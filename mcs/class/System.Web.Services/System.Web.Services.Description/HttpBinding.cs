// 
// System.Web.Services.Description.HttpBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("binding", "http://schemas.xmlsoap.org/wsdl/http/", typeof (Binding))]
	[XmlFormatExtensionPrefix ("http", "http://schemas.xmlsoap.org/wsld/http/")]
	public sealed class HttpBinding : ServiceDescriptionFormatExtension {

		#region Fields

		public const string Namespace = "http://schemas.xmlsoap.org/wsdl/http/";
		string verb;

		#endregion // Fields

		#region Constructors
		
		public HttpBinding ()
		{
			verb = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("verb", DataType = "NMTOKEN")]
		public string Verb { 	
			get { return verb; }
			set { verb = value; }
		}
	
		#endregion // Properties
	}
}
