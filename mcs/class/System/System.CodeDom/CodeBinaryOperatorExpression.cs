//
// System.CodeDom CodeBinaryOperatorExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeBinaryOperatorExpression
		: CodeExpression 
	{
		private CodeExpression left, right;
		private CodeBinaryOperatorType op;

		//
		// Constructors
		//
		public CodeBinaryOperatorExpression ()
		{
		}

		public CodeBinaryOperatorExpression (CodeExpression left,
						     CodeBinaryOperatorType op,
						     CodeExpression right)
		{
			this.left = left;
			this.op = op;
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

		public CodeBinaryOperatorType Operator {
			get {
				return op;
			}
			set {
				op = value;
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
	}
}
