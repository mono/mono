//
// System.Collections.Generic.IDictionary
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if GENERICS
using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic {
	[CLSCompliant(false)]
	[ComVisible(false)]
	public interface IDictionary<K,V> : ICollection<KeyValuePair<K,V>> {
		void Add (K key, V value);
		void Clear ();
		bool ContainsKey (K key);
		bool Remove (K key);
		bool IsFixedSize { get; }
		bool IsReadOnly { get; }
		V this[K key] { get; set; }
		ICollection<KeyValuePair<K,V>> Keys { get; }
		ICollection<KeyValuePair<K,V>> Values { get; }
	}
}
#endif
