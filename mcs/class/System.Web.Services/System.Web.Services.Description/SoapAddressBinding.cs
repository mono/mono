// 
// System.Web.Services.Description.SoapAddressBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("address", "http://schemas.xmlsoap.org/wsdl/soap/", typeof (Port))]
	public sealed class SoapAddressBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string location;

		#endregion // Fields

		#region Constructors
		
		public SoapAddressBinding ()
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
