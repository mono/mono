//
// DictionaryEntry.cs
//
// Authors:
// 	Paolo Molaro  (lupus@ximian.com)
// 	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Ben Maurer

using System;

namespace System.Collections {

	[Serializable]
	public struct DictionaryEntry {
		private object key;
		private object val;

		public DictionaryEntry (object k, object value) {
			if (k == null)
				throw new ArgumentNullException ("k");
			
			key = k;
			val = value;
		}
		
		public object Key {
			get {return key;}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				key = value;
			}
		}
		public object Value {
			get {return val;}
			set {val = value;}
		}
	}
}
