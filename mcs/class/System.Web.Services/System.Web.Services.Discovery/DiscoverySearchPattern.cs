// 
// System.Web.Services.Discovery.DiscoverySearchPattern.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

namespace System.Web.Services.Discovery {
	public abstract class DiscoverySearchPattern {

		#region Constructors

		protected DiscoverySearchPattern () {}
	
		#endregion // Constructors

		#region Properties

		public abstract string Pattern {
			get;
		}

		#endregion // Properties

		#region Methods

		public abstract DiscoveryReference GetDiscoveryReference (string filename);

		#endregion // Methods
	}
}
