//
// System.CodeDom CodeArrayCreateExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom {
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeAssignStatement
		: CodeStatement 
	{
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
