//
// System.CodeDom CodeMethodReturnStatement class implementation
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
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
	public class CodeMethodReturnStatement
		: CodeStatement
	{
		private CodeExpression expression;

		//
		// Constructors
		//
		public CodeMethodReturnStatement()
		{
		}
		
		public CodeMethodReturnStatement( CodeExpression expression )
		{
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
	}
}
