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

		public DictionaryEntry (object key, object value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			this.key = key;
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
