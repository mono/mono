// 
// System.Web.Services.Discovery.XmlSchemaSearchPattern.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
//

namespace System.Web.Services.Discovery {
	public sealed class XmlSchemaSearchPattern : DiscoverySearchPattern {
		
		#region Fields
		
		private string pattern = "*.xsd";

		#endregion // Fields

		#region Constructors

		public XmlSchemaSearchPattern () 
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
			SchemaReference refe = new SchemaReference ();
			refe.Url = filename;
			refe.Ref = filename;
			return refe;
		}

		#endregion // Methods
	}
}
