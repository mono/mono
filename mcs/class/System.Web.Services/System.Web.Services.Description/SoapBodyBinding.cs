// 
// System.Web.Services.Description.SoapBodyBinding.cs
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
	[XmlFormatExtension ("body", "http://schemas.xmlsoap.org/wsdl/soap/", typeof (InputBinding), typeof (OutputBinding), typeof (MimePart))]
	[XmlFormatExtensionPrefixAttribute ("soap", "http://schemas.xmlsoap.org/wsdl/soap/")]
	[XmlFormatExtensionPrefixAttribute ("soapenc", "http://schemas.xmlsoap.org/soap/encoding/")]
	public class SoapBodyBinding : ServiceDescriptionFormatExtension {

		#region Fields
		
		string encoding;
		string ns;
		string[] parts;
		string partsString;
		SoapBindingUse use;

		#endregion // Fields

		#region Constructors
		
		public SoapBodyBinding ()
		{
			encoding = String.Empty;
			ns = String.Empty;
			parts = null;
			partsString = null;
			use = SoapBindingUse.Default;
		}
		
		#endregion // Constructors

		#region Properties

		[DefaultValue ("")]
		[XmlAttribute ("encodingStyle")]
		public string Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		[DefaultValue ("")]
		[XmlAttribute ("namespace")]
		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		[XmlIgnore]
		public string[] Parts {
			get { return parts; }
			set {
				parts = value;
				if (value == null)
					partsString = null;
				else
					partsString = String.Join(" ", value);
			}
		}

		[XmlAttribute ("parts", DataType = "NMTOKENS")]
		public string PartsString {
			get { return partsString; }
			set {
				partsString = value;
				if (value == null)
					parts = null;
				else
					parts = value.Split(' ');
			}
		}

		[DefaultValue (SoapBindingUse.Default)]
		[XmlAttribute ("use")]
		public SoapBindingUse Use {
			get { return use; }
			set { use = value; }
		}

		#endregion // Properties
	}
}
