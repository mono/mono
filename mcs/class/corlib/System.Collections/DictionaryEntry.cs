using System;

namespace System.Collections {

	[Serializable]
	public struct DictionaryEntry {
		private object key;
		private object val;

		public DictionaryEntry( object k, object value) {
			key = k;
			val = value;
		}
		
		public object Key {
			get {return key;}
			set {key = value;}
		}
		public object Value {
			get {return val;}
			set {val = value;}
		}
	}
}
