// 
// System.Web.Services.Description.FaultBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class FaultBinding : MessageBinding {

		#region Fields

		ServiceDescriptionFormatExtensionCollection extensions;
		OperationBinding operationBinding;

		#endregion // Fields

		#region Constructors
		
		public FaultBinding ()
		{
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
			operationBinding = null;
		}
		
		#endregion // Constructors

		#region Properties

		public override ServiceDescriptionFormatExtensionCollection Extensions { 	
			get { return extensions; }
		}
	
		#endregion // Properties

		#region Methods

		internal void SetParent (OperationBinding operationBinding)
		{
			this.operationBinding = operationBinding;
		}

		#endregion
	}
}
