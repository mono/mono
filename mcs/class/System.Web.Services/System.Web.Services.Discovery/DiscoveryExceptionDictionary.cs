// 
// System.Web.Services.Protocols.DiscoveryExceptionDictionary.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.Collections;

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryExceptionDictionary : DictionaryBase {

		#region Constructors

		[MonoTODO]
		public DiscoveryExceptionDictionary () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public Exception this[string url] {
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
		public void Add (string url, Exception value)
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
