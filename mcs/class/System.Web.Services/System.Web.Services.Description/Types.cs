// 
// System.Web.Services.Description.Types.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPoint ("Extensions")]
	public sealed class Types : DocumentableItem {

		#region Fields

		ServiceDescriptionFormatExtensionCollection extensions;
		XmlSchemas schemas;

		#endregion // Fields

		#region Constructors
	
		public Types ()
		{
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			schemas = new XmlSchemas ();
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}

		[XmlElement ("schema", typeof (XmlSchema), Namespace = "http://www.w3.org/2001/XMLSchema")]
		public XmlSchemas Schemas {
			get { return schemas; }
		}

		#endregion // Properties
	}
}
