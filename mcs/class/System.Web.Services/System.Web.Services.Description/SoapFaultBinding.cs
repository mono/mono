// 
// System.Web.Services.Description.SoapFaultBinding.cs
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
	[XmlFormatExtension ("fault", "http://schemas.xmlsoap.org/wsdl/soap/", typeof (FaultBinding))]
	public sealed class SoapFaultBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string encoding;
		string ns;
		SoapBindingUse use;

		#endregion // Fields

		#region Constructors
		
		public SoapFaultBinding ()
		{
			encoding = String.Empty;
			ns = String.Empty;
			use = SoapBindingUse.Default;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("encodingStyle")]
		public string Encoding {
			get { return encoding; }
			set { encoding = value; }
		}
	
		[XmlAttribute ("namespace")]	
		public string Namespace {
			get { return ns; }
			set { ns = value; }
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
