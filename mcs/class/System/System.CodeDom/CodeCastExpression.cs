//
// System.CodeDom CodeCastExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom 
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeCastExpression
		: CodeExpression 
	{
		private CodeTypeReference targetType;
		private CodeExpression expression;
		
		//
		// Constructors
		//
		public CodeCastExpression ()
		{
		}

		public CodeCastExpression (CodeTypeReference targetType, CodeExpression expression)
		{
			this.targetType = targetType;
			this.expression = expression;
		}

		public CodeCastExpression (string targetType, CodeExpression expression)
		{
			this.targetType = new CodeTypeReference( targetType );
			this.expression = expression;
		}

		public CodeCastExpression (Type targetType, CodeExpression expression)
		{
			this.targetType = new CodeTypeReference( targetType );
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

		public CodeTypeReference TargetType {
			get {
				return targetType;
			}
			set {
				targetType = value;
			}
		}
	}
}
