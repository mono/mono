// 
// System.Web.Services.Description.SoapBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class SoapBinding : ServiceDescriptionFormatExtension {

		#region Fields

		public const string HttpTransport = "http://schemas.xmlsoap.org/soap/http/";
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

		public SoapBindingStyle Style {
			get { return style; }
			set { style = value; }
		}

		public string Transport {
			get { return transport; }
			set { transport = value; }
		}
	
		#endregion // Properties
	}
}
