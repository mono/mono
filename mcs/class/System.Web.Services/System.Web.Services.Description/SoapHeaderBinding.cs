// 
// System.Web.Services.Description.SoapHeaderBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("header", "http://schemas.xmlsoap.org/wsdl/soap/", typeof (InputBinding), typeof (OutputBinding))]
	public class SoapHeaderBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string encoding;
		bool mapToProperty;
		XmlQualifiedName message;
		string ns;
		string part;
		SoapBindingUse use;

		#endregion // Fields

		#region Constructors
	
		public SoapHeaderBinding ()
		{
			encoding = String.Empty;
			mapToProperty = false;
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

		[XmlIgnore]
		public bool MapToProperty {	
			get { return mapToProperty; }
			set { mapToProperty = value; }
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

#if NET_1_1
		[MonoTODO]
		public SoapHeaderFaultBinding Fault 
		{
			get { return null; }
			set { ; }
		}
#endif

		#endregion // Properties
	}
}
