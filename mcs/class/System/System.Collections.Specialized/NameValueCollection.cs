//
// System.Collections.Specialized.NameValueCollection.cs
//
// Author:
//   Gleb Novodran
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Text;

namespace System.Collections.Specialized{

	[Serializable]
	public class NameValueCollection : NameObjectCollectionBase
	{
		string[] cachedAllKeys = null;
		string[] cachedAll = null;

		//--------------------- Constructors -----------------------------

		/// <summary> SDK: Initializes a new instance of the NameValueCollection class that is empty,
		/// has the default initial capacity and uses the default case-insensitive hash code provider and the default case-insensitive comparer.
		/// </summary>
		public NameValueCollection() : base()
		{
		}
		
		/// <summary> SDK: Initializes a new instance of the NameValueCollection class that is empty, 
		/// has the specified initial capacity and uses the default case-insensitive hash code provider and the default case-insensitive comparer.
		///</summary>
		public NameValueCollection( int capacity ) : base(capacity)
		{
		}

		/// <summary> SDK: Copies the entries from the specified NameValueCollection to a new 
		/// NameValueCollection with the same initial capacity as the number of entries copied 
		/// and using the same case-insensitive hash code provider and the same case-insensitive 
		/// comparer as the source collection.
		/// </summary>
		/// TODO: uncomment constructor below after it will be possible to compile NameValueCollection and
		/// NameObjectCollectionBase to the same assembly 
		
		public NameValueCollection( NameValueCollection col ) : base(col.HashCodeProvider,col.Comparer)
		{
			if (col==null)
				throw new ArgumentNullException ("col");
			Add(col);
		}

		///<summary>SDK: Initializes a new instance of the NameValueCollection class that is empty, 
		///has the default initial capacity and uses the specified hash code provider and 
		///the specified comparer.</summary>
#if NET_2_0
		[Obsolete ("Use NameValueCollection(IEqualityComparer)")]
#endif
		public NameValueCollection( IHashCodeProvider hashProvider, IComparer comparer )
			: base(hashProvider, comparer)
		{
			
		}

		/// <summary>
		/// SDK: Copies the entries from the specified NameValueCollection to a new NameValueCollection
		/// with the specified initial capacity or the same initial capacity as the number of entries 
		/// copied, whichever is greater, and using the default case-insensitive hash code provider and 
		/// the default case-insensitive comparer.
		/// </summary>
		/// TODO: uncomment constructor below after it will be possible to compile NameValueCollection and
		/// NameObjectCollectionBase to the same assembly 
		
		public NameValueCollection( int capacity, NameValueCollection col )
			: base(capacity, col.HashCodeProvider, col.Comparer)
		{
			Add(col);			
		}

		/// <summary>
		/// SDK: Initializes a new instance of the NameValueCollection class that is serializable
		/// and uses the specified System.Runtime.Serialization.SerializationInfo and 
		/// System.Runtime.Serialization.StreamingContext.
		/// </summary>
		protected NameValueCollection( SerializationInfo info, StreamingContext context )
			:base(info, context)
		{
			
		}
		
		/// <summary>
		/// SDK: Initializes a new instance of the NameValueCollection class that is empty, 
		/// has the specified initial capacity and uses the specified hash code provider and 
		/// the specified comparer.
		/// </summary>
#if NET_2_0
		[Obsolete ("Use NameValueCollection(IEqualityComparer)")]
#endif
		public NameValueCollection( int capacity, IHashCodeProvider hashProvider, IComparer comparer )
			:base(capacity, hashProvider, comparer)
		{
			
		}

#if NET_2_0
		public NameValueCollection (IEqualityComparer equalityComparer)
			: base (equalityComparer)
		{
		}

		public NameValueCollection (int capacity, IEqualityComparer equalityComparer)
			: base (capacity, equalityComparer)
		{
		}
#endif

        //----------------------- Public Instance Properties -------------------------------

		
		///<summary> SDK:
		/// Gets all the keys in the NameValueCollection.
		/// The arrays returned by AllKeys are cached for better performance and are 
		/// automatically refreshed when the collection changes. A derived class can 
		/// invalidate the cached version by calling InvalidateCachedArrays, thereby 
		/// forcing the arrays to be recreated.
		/// </summary>
		public virtual string[] AllKeys 
		{
			get {
				if (cachedAllKeys==null)
					cachedAllKeys = BaseGetAllKeys();
				return this.cachedAllKeys;
			}
		}
		
		public string this[ int index ] 
		{
			get{
				return this.Get(index);
			}
		}
		
		public string this[ string name ] {
			get{
				return this.Get(name);
			}
			set{
				this.Set(name,value);
			}
		}
		
/////////////////////////////// Public Instance Methods //////////////////////////////
		
		/// <summary> SDK: Copies the entries in the specified NameValueCollection 
		/// to the current NameValueCollection.</summary>
		/// LAMESPEC: see description that comes this Add(string, string)
		
		public void Add (NameValueCollection c)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("Collection is read-only");
#if NET_2_0
			if (c == null)
				throw new ArgumentNullException ("c");
#endif
// make sense - but it's not the exception thrown
//				throw new ArgumentNullException ();
			
			InvalidateCachedArrays ();
			int max = c.Count;
			for (int i=0; i < max; i++){
				string key = c.GetKey (i);
				ArrayList new_values = (ArrayList) c.BaseGet (i);
				ArrayList values = (ArrayList) BaseGet (key);
				if (values != null && new_values != null)
					values.AddRange (new_values);
				else if (new_values != null)
					values = new ArrayList (new_values);
				BaseSet (key, values);
			}
		}

		
		/// <summary> SDK: Adds an entry with the specified name and value to the 
		/// NameValueCollection. </summary>
		/// 
		/// in SDK doc: If the same value already exists under the same key in the collection, 
		/// it just adds one more value in other words after
		/// <code>
		/// NameValueCollection nvc;
		/// nvc.Add("LAZY","BASTARD")
		/// nvc.Add("LAZY","BASTARD")
		/// </code>
		/// nvc.Get("LAZY") will be "BASTARD,BASTARD" instead of "BASTARD"

		public virtual void Add( string name, string val )
		{
			
			if (this.IsReadOnly)
				throw new NotSupportedException("Collection is read-only");
			
			InvalidateCachedArrays();
			ArrayList values = (ArrayList)BaseGet(name);
			if (values==null){
				values = new ArrayList();
				if (val!=null)
					values.Add(val);
				BaseAdd(name,values);
			}
			else {
				if (val!=null)
					values.Add(val);
			}

		}

		/// <summary> SDK: Invalidates the cached arrays and removes all entries from 
		/// the NameValueCollection.</summary>
#if NET_2_0
		public virtual void Clear ()
#else
		public void Clear ()
#endif
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("Collection is read-only");
			InvalidateCachedArrays();
			BaseClear();
		}

		/// <summary> SDK: Copies the entire NameValueCollection to a compatible one-dimensional Array,
		/// starting at the specified index of the target array.</summary>

		public void CopyTo( Array dest, int index )
		{
			if (dest==null)
				throw new ArgumentNullException("dest", "Null argument - dest");
			if (index<0)
				throw new ArgumentOutOfRangeException("index", "index is less than 0");
			if (dest.Rank > 1)
				throw new ArgumentException ("dest", "multidim");

			if (cachedAll==null)
				RefreshCachedAll();
			cachedAll.CopyTo(dest, index);
		}

		private void RefreshCachedAll()
		{
			this.cachedAll=null;
			int max = this.Count;
			cachedAll = new string[max];
			for(int i=0;i<max;i++){
				cachedAll[i] = this.Get(i);
			}
			
		}
		
		/// <summary> SDK: Gets the values at the specified index of the NameValueCollection combined
		/// into one comma-separated list.</summary>
		
		public virtual string Get( int index )
		{
			ArrayList values = (ArrayList)BaseGet(index);
			// if index is out of range BaseGet throws an ArgumentOutOfRangeException

			return AsSingleString(values);
			
		}
		
		/**
		 * SDK: Gets the values associated with the specified key from the NameValueCollection
		 * combined into one comma-separated list.
		 */
		public virtual string Get( string name )
		{
			ArrayList values = (ArrayList)BaseGet(name);
/*			if (values==null) 
				Console.WriteLine("BaseGet returned null");*/
			return AsSingleString(values);
// -------------------------------------------------------------

		}
		/// <summary></summary>
		[MonoTODO]
		private static string AsSingleString(ArrayList values)
		{
			const char separator = ',';
			
			if (values==null)
				return null;
			int max = values.Count;
			
			if (max==0)
				return null;
			//TODO: reimplement this
			StringBuilder sb = new StringBuilder((string)values[0]);
			for (int i=1; i<max; i++){
				sb.Append(separator);
				sb.Append(values[i]);
			}

			return sb.ToString();			
		}
		
		
		/// <summary>SDK: Gets the key at the specified index of the NameValueCollection.</summary>
		public virtual string GetKey( int index )
		{
			return BaseGetKey(index);
		}
		
		
		/// <summary>SDK: Gets the values at the specified index of the NameValueCollection.</summary>
		 
		public virtual string[] GetValues( int index )
		{
			ArrayList values = (ArrayList)BaseGet(index);
			
			return AsStringArray(values);
		}
		
		
		public virtual string[] GetValues( string name )
		{
			ArrayList values = (ArrayList)BaseGet(name);
			
			return AsStringArray(values);
		}
		
		private static string[] AsStringArray(ArrayList values)
		{
			if (values == null)
				return null;
			int max = values.Count;//get_Count();
			if (max==0)
				return null;
			
			string[] valArray =new string[max];
			values.CopyTo(valArray);
			return valArray;
		}
		
		
		/// <summary>
		/// SDK: Gets a value indicating whether the NameValueCollection contains keys that
		/// are not a null reference 
		/// </summary>

		public bool HasKeys()
		{
			return BaseHasKeys();
		}
		
		public virtual void Remove( string name )
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("Collection is read-only");
			InvalidateCachedArrays();
			BaseRemove(name);
			
		}
		
		/// <summary>
		/// Sets the value of an entry in the NameValueCollection.
		/// </summary>
		public virtual void Set (string name, string value)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("Collection is read-only");

			InvalidateCachedArrays ();
			
			ArrayList values = new ArrayList ();
			if (value != null) {
				values.Add (value);
				BaseSet (name,values);
			}
			else {
				// remove all entries
				BaseSet (name, null);
			}
		}
		

//---------------------- Protected Instance Methods ----------------------	

		protected void InvalidateCachedArrays()
		{
			cachedAllKeys = null;
			cachedAll = null;
		}

	}
}
