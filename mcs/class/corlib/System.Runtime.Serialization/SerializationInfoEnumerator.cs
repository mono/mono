//
// System.Runtime.Serialization.SerializationEnumerator.cs
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//
// Stub file. Fix when SerializationInfo is implemented.
//

using System.Collections;

namespace System.Runtime.Serialization {

	[MonoTODO]
	public sealed class SerializationInfoEnumerator : IEnumerator {
		public SerializationEntry Current {
			get { return new SerializationEntry (); }
		}

		public string Name {
			get { return null; }
		}

		public Type ObjectType {
			get { return null; }
		}

		public object Value {
			get { return null; }
		}

		// IEnumerator

		object IEnumerator.Current {
			get { return null; }
		}

		public bool MoveNext () {
			return false;
		}

		public void Reset () {
		}

		// internal

		internal SerializationInfoEnumerator (SerializationInfo info) {
			this.info = info;
		}

		// private
	
		private SerializationInfo info;
	}
}
