// 
// System.Web.Services.Protocols.DiscoveryClientReferenceCollection.cs
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
	public sealed class DiscoveryClientReferenceCollection : DictionaryBase {

		#region Constructors

		public DiscoveryClientReferenceCollection () 
			: base ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public DiscoveryReference this [string url] {
			get { return (DiscoveryReference) InnerHashtable [url]; }
			set { InnerHashtable [url] = value; }
		}
		
		public ICollection Keys {
			get { return InnerHashtable.Keys; }
		}
		
		public ICollection Values {
			get { return InnerHashtable.Values; }
		}
		
		#endregion // Properties

		#region Methods

		public void Add (DiscoveryReference value)
		{
			Add (value.Url, value);
		}
		
		public void Add (string url, DiscoveryReference value)
		{
			InnerHashtable [url] = value;
		}

		public bool Contains (string url)
		{
			return InnerHashtable.Contains (url);
		}
		
		public void Remove (string url)
		{
			InnerHashtable.Remove (url);
		}

		#endregion // Methods
	}
}
