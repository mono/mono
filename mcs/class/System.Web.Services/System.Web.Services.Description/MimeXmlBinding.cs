// 
// System.Web.Services.Description.MimeXmlBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("mimeXml", "http://schemas.xmlsoap.org/wsdl/mime/", typeof (MimePart), typeof (InputBinding), typeof (OutputBinding))]
	public sealed class MimeXmlBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string part;

		#endregion // Fields

		#region Constructors
		
		public MimeXmlBinding ()
		{
			part = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("part", DataType = "NMTOKEN")]
		public string Part {
			get { return part; }
			set { part = value; }
		}
		
		#endregion // Properties
	}
}
