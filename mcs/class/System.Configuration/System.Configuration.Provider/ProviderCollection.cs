//
// System.Configuration.Provider.ProviderCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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

#if NET_2_0
using System.Collections;

namespace System.Configuration.Provider {
	public class ProviderCollection : ICollection
	{
		public ProviderCollection ()
		{
			lookup = new Hashtable (10, StringComparer.InvariantCultureIgnoreCase);
			values = new ArrayList ();
		}
	
		public virtual void Add (ProviderBase provider)
		{
			if (readOnly)
				throw new NotSupportedException ();
			
			if (provider == null || provider.Name == null)
				throw new ArgumentNullException ();
			
			int pos = values.Add (provider);
			try {
				lookup.Add (provider.Name, pos);
			} catch {
				values.RemoveAt (pos);
				throw;
			}
		}
		
		public void Clear ()
		{
			if (readOnly)
				throw new NotSupportedException ();
			values.Clear ();
			lookup.Clear ();
		}
				
		public void CopyTo (ProviderBase[] array, int index)
		{
			values.CopyTo (array, index);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			values.CopyTo (array, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return values.GetEnumerator();
		}
		
		public void Remove (string name)
		{
			if (readOnly)
				throw new NotSupportedException ();
			
			object position = lookup [name];
			
			if (position == null || !(position is int))
				throw new ArgumentException ();
			
			int pos = (int) position;
			if (pos >= values.Count)
				throw new ArgumentException ();
			
			values.RemoveAt (pos);
			lookup.Remove (name);
			
			ArrayList changed = new ArrayList ();
			foreach (DictionaryEntry de in lookup) {
					if ((int) de.Value <= pos)
						continue;
					changed.Add (de.Key);
			}
			
			foreach (string key in changed)
				lookup [key] = (int)lookup [key] - 1;
		}
		
		public void SetReadOnly ()
		{
			readOnly = true;
		}
		
		public int Count { get { return values.Count; } }
		public bool IsSynchronized { get { return false; } }
		public object SyncRoot { get { return this; } }
		
		public ProviderBase this [string name] { 
			get {
				object pos = lookup [name];
				if (pos == null) return null;
				
				return values [(int) pos] as ProviderBase;
			}
		}
		
		
		Hashtable lookup;
		bool readOnly;
		ArrayList values;
	 
	}
}
#endif
