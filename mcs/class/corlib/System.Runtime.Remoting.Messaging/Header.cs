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
			this.name = name;
			this.value = value;
			this.must_understand = must_understand;
			this.header_namespace = header_namespace;
		}

		// fields

		public string HeaderNamespace;

		public bool MustUnderstand;

		public string Name;

		public object Value;

		// private

		private string name;
		private object value;
		private bool must_understand;
		private string header_namespace;
	}
}
