// 
// System.Web.Services.Protocols.DiscoveryClientResultCollection.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Dave Bettin, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryClientResultCollection : CollectionBase {

		#region Constructors

		public DiscoveryClientResultCollection () 
			: base ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public DiscoveryClientResult this [int i] {
			get { 
				if (i < 0 || i >= Count)
					throw new ArgumentOutOfRangeException (); 
				return (DiscoveryClientResult) InnerList [i]; 
			}	
			set { 
				if (i < 0 || i >= Count)
					throw new ArgumentOutOfRangeException (); 
				InnerList [i] = value; 
			}
		}				
		
		#endregion // Properties

		#region Methods

		public int Add (DiscoveryClientResult value)
		{
			return InnerList.Add (value);
		}

		public bool Contains (DiscoveryClientResult value)
		{
			return InnerList.Contains (value);
		}
		
		public void Remove (DiscoveryClientResult value)
		{
			InnerList.Remove (value);
		}

		#endregion // Methods
	}
}
