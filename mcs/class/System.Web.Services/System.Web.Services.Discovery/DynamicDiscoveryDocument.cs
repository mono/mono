// 
// System.Web.Services.Discovery.DynamicDiscoveryDocument.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.IO;

namespace System.Web.Services.Discovery {
	public sealed class DynamicDiscoveryDocument {

		#region Fields
		
		public const string Namespace = "urn:schemas-dynamicdiscovery:disco.2000-03-17";
		
		#endregion // Fields
		
		#region Constructors

		[MonoTODO]
		public DynamicDiscoveryDocument () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties
		
		public ExcludePathInfo[] ExcludePaths {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static DynamicDiscoveryDocument Load (Stream stream)
		{
                        throw new NotImplementedException ();
		}

		[MonoTODO]
		public	 void Write (Stream stream)
		{
                        throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
