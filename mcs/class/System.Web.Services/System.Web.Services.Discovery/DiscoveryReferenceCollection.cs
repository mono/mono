// 
// System.Web.Services.Protocols.DiscoveryReferenceCollection.cs
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
	public sealed class DiscoveryReferenceCollection : CollectionBase {

		#region Constructors

		public DiscoveryReferenceCollection () 
			: base ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public DiscoveryReference this [int i] {
			get { 
				if (i < 0 || i >= Count)
					throw new ArgumentOutOfRangeException ();
				return (DiscoveryReference) InnerList [i]; 
			}
			set {
				if (i < 0 || i >= Count)
					throw new ArgumentOutOfRangeException ();
				InnerList [i] = value;
			}
		}
		
		#endregion // Properties

		#region Methods

		public int Add (DiscoveryReference value)
		{
			return InnerList.Add (value);
		}

		public bool Contains (DiscoveryReference value)
		{
			return InnerList.Contains (value);
		}
		
		public void Remove (DiscoveryReference value)
		{
			InnerList.Remove (value);
		}

		#endregion // Methods
	}
}
