//
// System.Configuration.Provider.ProviderCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;

namespace System.Configuration.Provider {
	public class ProviderCollection : ICollection
	{
		public ProviderCollection ()
		{
			lookup = new Hashtable (10, CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
			values = new ArrayList ();
		}
	
		public virtual void Add (IProvider provider)
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
				
		public void CopyTo (Array array, int index)
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
		
		public IProvider this [string name] { 
			get {
				object pos = lookup [name];
				if (pos == null) return null;
				
				return values [(int) pos] as IProvider;
			}
		}
		
		
		Hashtable lookup;
		bool readOnly;
		ArrayList values;
	 
	}
}
#endif
