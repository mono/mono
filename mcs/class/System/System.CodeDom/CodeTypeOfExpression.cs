//
// System.CodeDom CodeTypeOfExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeTypeOfExpression : CodeExpression {
		string type;
			
		public CodeTypeOfExpression () {}

		public CodeTypeOfExpression (string type)
		{
			this.type = type;
		}

		public string Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}
	}
}
