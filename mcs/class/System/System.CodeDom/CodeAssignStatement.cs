//
// System.CodeDom CodeArrayCreateExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeAssignStatement : CodeStatement {

		CodeExpression left, right;

		//
		// Constructors
		//
		public CodeAssignStatement ()
		{
		}

		public CodeAssignStatement (CodeExpression left, CodeExpression right)
		{
			this.left = left;
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
	}
}
