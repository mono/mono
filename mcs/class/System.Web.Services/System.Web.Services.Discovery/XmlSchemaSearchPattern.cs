// 
// System.Web.Services.Discovery.XmlSchemaSearchPattern.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

namespace System.Web.Services.Discovery {
	public sealed class XmlSchemaSearchPattern : DiscoverySearchPattern {
		
		#region Fields
		
		private string pattern = "*.xsd";

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public XmlSchemaSearchPattern () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public override string Pattern {
			get { return pattern; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override DiscoveryReference GetDiscoveryReference (string filename)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
