// 
// System.Web.Services.Discovery.DiscoveryClientDocumentCollection.cs
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
	public sealed class DiscoveryClientDocumentCollection : DictionaryBase {

		#region Constructors 

		public DiscoveryClientDocumentCollection () 
			: base ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public object this [string url] {
			get { return InnerHashtable [url]; }
			set { 
				if (url == null)
					throw new ArgumentNullException ();
				InnerHashtable [url] = value; 
			}
		}
		
		public ICollection Keys {
			get { return InnerHashtable.Keys; }
		}
		
		public ICollection Values {
			get { return InnerHashtable.Values; }
		}
		
		#endregion // Properties

		#region Methods

		public void Add (string url, object value)
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
