// 
// System.Web.Services.Description.MimeContentBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPrefix ("mime", "http://schemas.xmlsoap.org/wsdl/mime/")]
	[XmlFormatExtension ("content", "http://schemas.xmlsoap.org/wsdl/mime/", typeof (InputBinding), typeof (OutputBinding))]
	public sealed class MimeContentBinding : ServiceDescriptionFormatExtension {

		#region Fields

		public const string Namespace = "http://schemas.xmlsoap.org/wsdl/mime/";
		string part;
		string type;

		#endregion // Fields

		#region Constructors
		
		public MimeContentBinding ()
		{
			part = String.Empty;
			type = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("part", DataType = "NMTOKEN")]	
		public string Part {
			get { return part; }
			set { part = value; }
		}

		[XmlAttribute ("type")]
		public string Type {
			get { return type; }
			set { type = value; }
		}

		#endregion // Properties
	}
}
