// 
// System.Web.Services.Protocols.DiscoveryExceptionDictionary.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.Collections;

namespace System.Web.Services.Discovery 
{
	public sealed class DiscoveryExceptionDictionary : DictionaryBase 
	{
		#region Constructors

		public DiscoveryExceptionDictionary () 
		{
		}
		
		#endregion // Constructors

		#region Properties

		public Exception this[string url] {
			get { return (Exception) InnerHashtable [url]; }
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

		public void Add (string url, Exception value)
		{
			InnerHashtable.Add (url, value);
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
