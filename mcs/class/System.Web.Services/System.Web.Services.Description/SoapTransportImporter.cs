// 
// System.Web.Services.Description.SoapTransportImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public abstract class SoapTransportImporter {

		#region Fields

		SoapProtocolImporter importContext;

		#endregion // Fields

		#region Constructors
	
		public SoapTransportImporter ()
		{
			importContext = null;
		}
		
		#endregion // Constructors

		#region Properties

		public SoapProtocolImporter ImportContext {
			get { return importContext; }
			set { importContext = value; }
		}

		#endregion // Properties

		#region Methods

		public abstract void ImportClass ();
		public abstract bool IsSupportedTransport (string transport);

		#endregion
	}
}
