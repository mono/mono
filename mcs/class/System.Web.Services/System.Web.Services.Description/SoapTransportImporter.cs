// 
// System.Web.Services.Description.SoapTransportImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;

namespace System.Web.Services.Description {
	public abstract class SoapTransportImporter {

		#region Fields

		static ArrayList transportImporters;
		SoapProtocolImporter importContext;

		#endregion // Fields

		#region Constructors
		
		static SoapTransportImporter ()
		{
			transportImporters = new ArrayList ();
			transportImporters.Add (new SoapHttpTransportImporter ());
		}
	
		protected SoapTransportImporter ()
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
		
		internal static SoapTransportImporter FindTransportImporter (string uri)
		{
			foreach (SoapHttpTransportImporter imp in transportImporters)
				if (imp.IsSupportedTransport (uri)) return imp;
			return null;
		}

		#endregion
	}
}
