// 
// System.Web.Services.Description.SoapBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.Web.Services.Configuration;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPrefix ("soap", "http://schemas.xmlsoap.org/wsdl/soap/")]
	[XmlFormatExtensionPrefix ("soapenc", "http://schemas.xmlsoap.org/soap/encoding/")]
	[XmlFormatExtension ("binding", "http://schemas.xmlsoap.org/wsdl/soap/", typeof (Binding))]
	public class SoapBinding : ServiceDescriptionFormatExtension {

		#region Fields

		public const string HttpTransport = "http://schemas.xmlsoap.org/soap/http";
		public const string Namespace = "http://schemas.xmlsoap.org/wsdl/soap/";

		SoapBindingStyle style;
		string transport;

#if NET_2_0
		static XmlSchema schema;
#endif

		#endregion // Fields

		#region Constructors
		
		public SoapBinding ()
		{
			style = SoapBindingStyle.Document;
			transport = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

#if NET_2_0
		public static XmlSchema Schema {
			get {
				if (schema == null) {
					schema = XmlSchema.Read (typeof (SoapBinding).Assembly.GetManifestResourceStream ("wsdl-1.1-soap.xsd"), null);
				}
				return schema;
			}
		}
#endif

		// LAMESPEC: .NET says that the default value is SoapBindingStyle.Document but
		// reflection shows this attribute is SoapBindingStyle.Default

		[DefaultValue (SoapBindingStyle.Document)]
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
