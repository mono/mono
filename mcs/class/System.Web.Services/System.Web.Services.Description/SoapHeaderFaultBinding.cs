// 
// System.Web.Services.Description.SoapHeaderFaultBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Web.Services.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("headerfault", "http://schemas.xmlsoap.org/wsdl/soap/", typeof (InputBinding), typeof (OutputBinding))]
	public sealed class SoapHeaderFaultBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string encoding;
		XmlQualifiedName message;
		string ns;
		string part;
		SoapBindingUse use;

		#endregion // Fields

		#region Constructors
	
		[MonoTODO]	
		public SoapHeaderFaultBinding ()
		{
			encoding = String.Empty;
			message = XmlQualifiedName.Empty;
			ns = String.Empty;
			part = String.Empty;
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

		[XmlAttribute ("message")]
		public XmlQualifiedName Message {
			get { return message; }
			set { message = value; }
		}
	
		[DefaultValue ("")]
		[XmlAttribute ("namespace")]	
		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		[XmlAttribute ("part", DataType = "NMTOKEN")]
		public string Part {
			get { return part; }
			set { part = value; }
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
