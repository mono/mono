// 
// System.Web.Services.Discovery.DiscoveryDocumentSearchPattern.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
//

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryDocumentSearchPattern : DiscoverySearchPattern {
		
		#region Fields

		private string pattern = "*.vsdisco";

		#endregion // Fields

		#region Constructors

		public DiscoveryDocumentSearchPattern () 
		{
		}
		
		#endregion // Constructors

		#region Properties

		public override string Pattern {
			get { return pattern; }
		}

		#endregion // Properties

		#region Methods

		public override DiscoveryReference GetDiscoveryReference (string filename)
		{
			DiscoveryDocumentReference refe = new DiscoveryDocumentReference ();
			refe.Url = filename;
			refe.Ref = filename;
			return refe;
		}

		#endregion // Methods
	}
}
