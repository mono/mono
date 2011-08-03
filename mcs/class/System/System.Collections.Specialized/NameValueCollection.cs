//
// System.Collections.Specialized.NameValueCollection.cs
//
// Author:
//   Gleb Novodran
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2011 Novell (http://www.novell.com)
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

		public NameValueCollection () : base ()
		{
		}
		
		public NameValueCollection (int capacity) : base (capacity)
		{
		}
		
		public NameValueCollection (NameValueCollection col) : base (( col == null ) ? null : col.EqualityComparer ,
									     (col == null) ? null : col.Comparer, 
									     (col == null) ? null : col.HashCodeProvider)
		{
			if (col==null)
				throw new ArgumentNullException ("col");		
			Add(col);
		}

		[Obsolete ("Use NameValueCollection (IEqualityComparer)")]
		public NameValueCollection (IHashCodeProvider hashProvider, IComparer comparer)
			: base (hashProvider, comparer)
		{
			
		}

		public NameValueCollection (int capacity, NameValueCollection col)
			: base (capacity, (col == null) ? null : col.HashCodeProvider, 
					(col == null) ? null : col.Comparer)
		{
			Add (col);			
		}

		protected NameValueCollection (SerializationInfo info, StreamingContext context)
			:base (info, context)
		{
			
		}
		
		[Obsolete ("Use NameValueCollection (IEqualityComparer)")]
		public NameValueCollection (int capacity, IHashCodeProvider hashProvider, IComparer comparer)
			:base (capacity, hashProvider, comparer)
		{
			
		}

		public NameValueCollection (IEqualityComparer equalityComparer)
			: base (equalityComparer)
		{
		}

		public NameValueCollection (int capacity, IEqualityComparer equalityComparer)
			: base (capacity, equalityComparer)
		{
		}

		public virtual string[] AllKeys 
		{
			get {
				if (cachedAllKeys == null)
					cachedAllKeys = BaseGetAllKeys ();
				return this.cachedAllKeys;
			}
		}
		
		public string this [int index] 
		{
			get{
				return this.Get (index);
			}
		}
		
		public string this [string name] {
			get{
				return this.Get (name);
			}
			set{
				this.Set (name,value);
			}
		}
		
		public void Add (NameValueCollection c)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("Collection is read-only");
			if (c == null)
				throw new ArgumentNullException ("c");

// make sense - but it's not the exception thrown
//				throw new ArgumentNullException ();
			
			InvalidateCachedArrays ();
			int max = c.Count;
			for (int i=0; i < max; i++){
				string key = c.GetKey (i);
				string[] values = c.GetValues (i);

				if (values != null && values.Length > 0) {
					foreach (string value in values)
						Add (key, value);
				} else
					Add (key, null);
			}
		}

		/// in SDK doc: If the same value already exists under the same key in the collection, 
		/// it just adds one more value in other words after
		/// <code>
		/// NameValueCollection nvc;
		/// nvc.Add ("LAZY","BASTARD")
		/// nvc.Add ("LAZY","BASTARD")
		/// </code>
		/// nvc.Get ("LAZY") will be "BASTARD,BASTARD" instead of "BASTARD"

		public virtual void Add (string name, string val)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("Collection is read-only");
			
			InvalidateCachedArrays ();
			ArrayList values = (ArrayList)BaseGet (name);
			if (values == null){
				values = new ArrayList ();
				if (val != null)
					values.Add (val);
				BaseAdd (name, values);
			}
			else {
				if (val != null)
					values.Add (val);
			}

		}

		public virtual void Clear ()
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("Collection is read-only");
			InvalidateCachedArrays ();
			BaseClear ();
		}

		public void CopyTo (Array dest, int index)
		{
			if (dest == null)
				throw new ArgumentNullException ("dest", "Null argument - dest");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "index is less than 0");
			if (dest.Rank > 1)
				throw new ArgumentException ("dest", "multidim");

			if (cachedAll == null)
				RefreshCachedAll ();
			try {
				cachedAll.CopyTo (dest, index);
		        } catch (ArrayTypeMismatchException) {
		        	throw new InvalidCastException();
		        }
		}

		private void RefreshCachedAll ()
		{
			this.cachedAll = null;
			int max = this.Count;
			cachedAll = new string [max];
			for (int i = 0; i < max; i++)
				cachedAll [i] = this.Get (i);
		}
		
		public virtual string Get (int index)
		{
			ArrayList values = (ArrayList)BaseGet (index);
			// if index is out of range BaseGet throws an ArgumentOutOfRangeException

			return AsSingleString (values);
		}
		
		public virtual string Get (string name)
		{
			ArrayList values = (ArrayList)BaseGet (name);
			return AsSingleString (values);
		}

		private static string AsSingleString (ArrayList values)
		{
			const char separator = ',';
			
			if (values == null)
				return null;
			int max = values.Count;
			
			switch (max) {
			case 0:
				return null;
			case 1:
				return (string)values [0];
			case 2:
				return String.Concat ((string)values [0], separator, (string)values [1]);
			default:
				int len = max;
				for (int i = 0; i < max; i++)
					len += ((string)values [i]).Length;
				StringBuilder sb = new StringBuilder ((string)values [0], len);
				for (int i = 1; i < max; i++){
					sb.Append (separator);
					sb.Append (values [i]);
				}

				return sb.ToString ();
			}
		}
		
		
		public virtual string GetKey (int index)
		{
			return BaseGetKey (index);
		}
		
		
		public virtual string[] GetValues (int index)
		{
			ArrayList values = (ArrayList)BaseGet (index);
			
			return AsStringArray (values);
		}
		
		
		public virtual string[] GetValues (string name)
		{
			ArrayList values = (ArrayList)BaseGet (name);
			
			return AsStringArray (values);
		}
		
		private static string[] AsStringArray (ArrayList values)
		{
			if (values == null)
				return null;
			int max = values.Count;//get_Count ();
			if (max == 0)
				return null;
			
			string[] valArray = new string[max];
			values.CopyTo (valArray);
			return valArray;
		}
		
		public bool HasKeys ()
		{
			return BaseHasKeys ();
		}
		
		public virtual void Remove (string name)
		{
			if (this.IsReadOnly)
				throw new NotSupportedException ("Collection is read-only");
			InvalidateCachedArrays ();
			BaseRemove (name);
			
		}
		
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
		
		protected void InvalidateCachedArrays ()
		{
			cachedAllKeys = null;
			cachedAll = null;
		}

	}
}
