//
// System.CodeDom CodePrimitiveExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodePrimitiveExpression : CodeExpression {
		object value;
		
		public CodePrimitiveExpression () {}

		public CodePrimitiveExpression (Object value)
		{
			this.value = value;
		}

		public object Value {
			get {
				return value;
			}

			set {
				this.value = value;
			}
		}
	}
}

