//
// System.CodeDom CodeBinaryOperatorExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeBinaryOperatorExpression : CodeExpression {

		CodeExpression left, right;
		CodeBinaryOperatorType oper;

		public enum CodeBinaryOperatorType {
			Add,
			Substract,
			Multiply,
			Divide,
			Modulus,
			Assign,
			IdentityInequality,
			IdentityEquality,
			ValueEquality,
			BitwiseOr,
			BitwiseAnd,
			BooleanOr,
			BooleanAnd,
			LessThan,
			LessThanOrEqual,
			GreatherThan,
			GreatherThanOrEqual,
		}
		
		//
		// Constructors
		//
		public CodeBinaryOperatorExpression ()
		{
		}


		public CodeBinaryOperatorExpression (CodeExpression left,
						     CodeBinaryOperatorType oper,
						     CodeExpression right)
		{
			this.left = left;
			this.oper = oper;
			this.right = right;
		}

		//
		// Properties
		//
		public CodeExpression Left {
			get {
				return left;
			}

			set {
				left = value;
			}
		}

		public CodeExpression Right {
			get {
				return right;
			}

			set {
				right = value;
			}
		}

		public CodeBinaryOperatorType Operator {
			get {
				return oper;
			}

			set {
				oper = value;
			}
		}
	}
}

