//
// System.CodeDom CodeCastExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeCastExpression : CodeExpression {
		string targetType;
		CodeExpression expression;
		
		//
		// Constructors
		//
		public CodeCastExpression ()
		{
		}

		public CodeCastExpression (string targetType, CodeExpression expression)
		{
			this.targetType = targetType;
			this.expression = expression;
		}

		//
		// Properties
		//
		public CodeExpression Expression {
			get {
				return expression;
			}

			set {
				expression = value;
			}
		}

		public string TargetType {
			get {
				return targetType;
			}

			set {
				targetType = value;
			}
		}
	}
}
