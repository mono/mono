//
// System.Runtime.Remoting.Messaging.Header.cs
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//

using System.Collections;

namespace System.Runtime.Remoting.Messaging {

	[Serializable]
	public class Header {
		public Header (string name, object value) :
			this (name, value, true)
		{
		}

		public Header (string name, object value, bool must_understand) :
			this (name, value, must_understand, null)
		{
		}

		public Header (string name, object value, bool must_understand, string header_namespace) {
			this.Name = name;
			this.Value = value;
			this.MustUnderstand = must_understand;
			this.HeaderNamespace = header_namespace;
		}

		// fields

		public string HeaderNamespace;

		public bool MustUnderstand;

		public string Name;

		public object Value;
	}
}
