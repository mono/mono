//
// System.CodeDom CodeReturnStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeReturnStatement : CodeStatement {
		CodeExpression expression;
		
		public CodeReturnStatement ()
		{
		}

		public CodeReturnStatement (CodeExpression expression)
		{
			this.expression = expression;
		}

		//
		// Properties
		//
		CodeExpression Expression {
			get {
				return expression;
			}

			set {
				expression = value;
			}
		}
	}
}
