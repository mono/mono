//
// System.CodeDom CodeExpressionStatement Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeExpressionStatement
		: CodeStatement
	{
		private CodeExpression expression;

		//
		// Constructors
		//
		public CodeExpressionStatement()
		{
		}

		public CodeExpressionStatement(CodeExpression expression)
		{
			Expression = expression;
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
	}
}
