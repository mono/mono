// 
// System.Web.Services.Protocols.ContractSearchPattern.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
//

namespace System.Web.Services.Discovery {
	public sealed class ContractSearchPattern : DiscoverySearchPattern {

		#region Fields

		private string pattern = "*.asmx";

		#endregion // Fields

		#region Constructors

		public ContractSearchPattern () 
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
			ContractReference refe = new ContractReference ();
			refe.Url = filename;
			refe.Ref = filename;
			refe.DocRef = filename;
			return refe;
		}

		#endregion // Methods
	}
}
