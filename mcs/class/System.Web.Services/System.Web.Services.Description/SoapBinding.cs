// 
// System.Web.Services.Description.SoapBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPrefix ("soap", "http://schemas.xmlsoap.org/wsdl/soap/")]
	[XmlFormatExtension ("binding", "http://schemas.xmlsoap.org/wsdl/soap/", typeof (Binding))]
	public sealed class SoapBinding : ServiceDescriptionFormatExtension {

		#region Fields

		public const string HttpTransport = "http://schemas.xmlsoap.org/soap/http";
		public const string Namespace = "http://schemas.xmlsoap.org/wsdl/soap/";

		SoapBindingStyle style;
		string transport;

		#endregion // Fields

		#region Constructors
		
		public SoapBinding ()
		{
			style = SoapBindingStyle.Document;
			transport = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		// LAMESPEC: .NET says that the default value is SoapBindingStyle.Document but
		// reflection shows this attribute is SoapBindingStyle.Default

		[DefaultValue (SoapBindingStyle.Default)]
		[XmlAttribute ("style")]
		public SoapBindingStyle Style {
			get { return style; }
			set { style = value; }
		}

		[XmlAttribute ("transport")]
		public string Transport {
			get { return transport; }
			set { transport = value; }
		}
	
		#endregion // Properties
	}
}
