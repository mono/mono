// 
// System.Web.Services.Description.SoapExtensionImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.CodeDom;

namespace System.Web.Services.Description {
	public abstract class SoapExtensionImporter {

		#region Fields

		SoapProtocolImporter importContext;
		
		#endregion // Fields

		#region Constructors
	
		protected SoapExtensionImporter ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public SoapProtocolImporter ImportContext {
			get { return importContext; }
			set { importContext = value; }
		}

		#endregion // Properties

		#region Methods

		public abstract void ImportMethod (CodeAttributeDeclarationCollection metadata);

		#endregion
	}
}
