//
// System.CodeDom CodeThrowExceptionStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeThrowExceptionStatement : CodeStatement {
		CodeExpression toThrow;
		
		public CodeThrowExceptionStatement () {}
		public CodeThrowExceptionStatement (CodeExpression toThrow)
		{
			this.toThrow = toThrow;
		}

		public CodeExpression ToThrow {
			get {
				return toThrow;
			}

			set {
				toThrow = value;
			}
		}
	}
}
