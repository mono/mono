// 
// System.Web.Services.Description.MimePart.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
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
	
		public ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}

		#endregion // Properties
	}
}
