// 
// System.Web.Services.Protocols.DiscoverySearchPattern.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

namespace System.Web.Services.Discovery {
	public abstract class DiscoverySearchPattern {

		#region Constructors

		[MonoTODO]
		protected DiscoverySearchPattern () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public abstract string Pattern {
			get;
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public abstract DiscoveryReference GetDiscoveryReference (string filename);

		#endregion // Methods
	}
}
