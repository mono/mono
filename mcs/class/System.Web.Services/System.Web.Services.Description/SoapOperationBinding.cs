// 
// System.Web.Services.Description.SoapOperationBinding.cs
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
	[XmlFormatExtension ("operation", "http://schema.xmlsoap.org/wsdl/soap/", typeof (OperationBinding))]
	public sealed class SoapOperationBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string soapAction;
		SoapBindingStyle style;

		#endregion // Fields

		#region Constructors
	
		public SoapOperationBinding ()
		{
			soapAction = String.Empty;
			style = SoapBindingStyle.Document;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("soapAction")]
		public string SoapAction {
			get { return soapAction; }
			set { soapAction = value; }
		}

		// LAMESPEC: .NET Documentation says that the default value for this property is
		// SoapBindingStyle.Document (see constructor), but reflection shows that this 
		// attribute value is SoapBindingStyle.Default

		[DefaultValue (SoapBindingStyle.Default)]
		[XmlAttribute ("style")]
		public SoapBindingStyle Style {
			get { return style; }
			set { style = value; }
		}

		#endregion // Properties
	}
}
