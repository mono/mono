//
// System.CodeDOM CodeLiteralStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeLiteralStatement : CodeStatement {
		string value;

		//
		// Constructors
		//
		public CodeLiteralStatement (string value)
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
	}
}
