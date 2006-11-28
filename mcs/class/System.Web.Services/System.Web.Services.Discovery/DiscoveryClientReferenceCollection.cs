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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
