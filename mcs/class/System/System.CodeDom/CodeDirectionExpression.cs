//
// System.CodeDom FieldDirection Enum implementation
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
	public class CodeDirectionExpression
		: CodeExpression
	{
		private FieldDirection direction;
		private CodeExpression expression;

		//
		// Constructors
		//
		public CodeDirectionExpression()
		{
		}

		public CodeDirectionExpression( FieldDirection direction, 
						CodeExpression expression )
		{
			this.direction = direction;
			this.expression = expression;
		}

		//
		// Properties
		//
		public FieldDirection Direction {
			get {
				return direction;
			}
			set {
				direction = value;
			}
		}

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
