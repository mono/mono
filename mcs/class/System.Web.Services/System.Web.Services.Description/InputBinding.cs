// 
// System.Web.Services.Description.InputBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class InputBinding : MessageBinding {

		#region Fields

		ServiceDescriptionFormatExtensionCollection extensions;

		#endregion // Fields

		#region Constructors
		
		public InputBinding ()
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
