// 
// System.Web.Services.Description.Types.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml.Serialization;

namespace System.Web.Services.Description {
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

		public ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}

		public XmlSchemas Schemas {
			get { return schemas; }
		}

		#endregion // Properties
	}
}
