// 
// System.Web.Services.Description.OutputBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
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

		public override ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}
	
		#endregion // Properties
	}
}
