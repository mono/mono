// 
// System.Web.Services.Protocols.DiscoveryClientReferenceCollection.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.Collections;

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryClientReferenceCollection : DictionaryBase {

		#region Constructors

		[MonoTODO]
		public DiscoveryClientReferenceCollection () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public DiscoveryReference this[string url] {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}
		
		public ICollection Keys {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		public ICollection Values {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Add (DiscoveryReference value)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Add (string url, DiscoveryReference value)
		{
                        throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (string url)
		{
                        throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Remove (string url)
		{
                        throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
