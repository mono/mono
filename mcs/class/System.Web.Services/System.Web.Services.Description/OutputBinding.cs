// 
// System.Web.Services.Description.OutputBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPoint ("Extensions")]
	public sealed class OutputBinding : MessageBinding {

		#region Fields

		ServiceDescriptionFormatExtensionCollection extensions;

		#endregion // Fields

		#region Constructors
		
		public OutputBinding ()
		{
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public override ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}
	
		#endregion // Properties
	}
}
