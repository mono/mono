//
// System.CodeDom CodeTypeOfExpression Class implementation
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
	public class CodeTypeOfExpression
		: CodeExpression 
	{
		private CodeTypeReference type;
			
		//
		// Constructors
		//
		public CodeTypeOfExpression () 
		{
		}

		public CodeTypeOfExpression (CodeTypeReference type)
		{
			this.type = type;
		}

		public CodeTypeOfExpression (string type)
		{
			this.type = new CodeTypeReference( type );
		}
		
		public CodeTypeOfExpression (Type type)
		{
			this.type = new CodeTypeReference( type );
		}

		//
		// Properties
		//
		public CodeTypeReference Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}
	}
}
