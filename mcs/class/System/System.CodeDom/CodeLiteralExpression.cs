//
// System.CodeDom CodeLiteralExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeLiteralExpression : CodeExpression {
		string val;

		//
		// Constructors
		//
		public CodeLiteralExpression ()
		{
		}

		public CodeLiteralExpression (string value)
		{
			val = value;
		}

		//
		// Properties
		//
		string Value {
			get {
				return val;
			}

			set {
				val = value;
			}
		}
	}
}
