// 
// System.Web.Services.Description.MimePart.cs
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
	public sealed class MimePart : ServiceDescriptionFormatExtension {

		#region Fields

		ServiceDescriptionFormatExtensionCollection extensions;

		#endregion // Fields

		#region Constructors
		
		public MimePart ()
		{
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]	
		public ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}

		#endregion // Properties
	}
}
