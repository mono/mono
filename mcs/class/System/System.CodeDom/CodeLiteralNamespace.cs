//
// System.CodeDOM CodeLiteralNamespace Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeLiteralNamespace : CodeNamespace {
		CodeLinePragma linePragma;
		string value;

		//
		// Constructors
		//
		public CodeLiteralNamespace (string value)
		{ 
			this.value = value;
		}

		//
		// Properties
		//
		string Value {
			get {
				return value;
			}

			set {
				this.value = value;
			}
		}

		CodeLinePragma LinePragma {
			get {
				return linePragma;
			}

			set {
				linePragma = value;
			}
		}
	}
}

