//
// System.CodeDom CodeObjectCreateExpression Class implementation
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
	public class CodeObjectCreateExpression
		: CodeExpression 
	{
		private CodeTypeReference createType;
		private CodeExpressionCollection parameters;
		
		//
		// Constructors
		//
		public CodeObjectCreateExpression () 
		{
		}

		public CodeObjectCreateExpression (CodeTypeReference createType, 
						   params CodeExpression [] parameters)
		{
			this.createType = createType;
			this.Parameters.AddRange( parameters );
		}

		public CodeObjectCreateExpression (string createType, 
						   params CodeExpression [] parameters)
		{
			this.createType = new CodeTypeReference( createType );
			this.Parameters.AddRange( parameters );
		}

		public CodeObjectCreateExpression (Type createType, 
						   params CodeExpression [] parameters)
		{
			this.createType = new CodeTypeReference( createType );
			this.Parameters.AddRange( parameters );
		}

		//
		// Properties
		//
		public CodeTypeReference CreateType {
			get {
				return createType;
			}
			set {
				createType = value;
			}
		}

		public CodeExpressionCollection Parameters {
			get {
				if ( parameters == null )
					parameters = new CodeExpressionCollection();
				return parameters;
			}
		}
	}
}
