//
// System.CodeDom CodeVariableDeclarationStatement Class implementation
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
	public class CodeVariableDeclarationStatement
		: CodeStatement  
	{
		private CodeExpression initExpression;
		private CodeTypeReference type;
		private string name;

		//
		// Constructors
		//
		public CodeVariableDeclarationStatement () 
		{
		}

		public CodeVariableDeclarationStatement( CodeTypeReference type, string name )
		{
			this.type = type;
			this.name = name;
		}
		
		public CodeVariableDeclarationStatement( string type, string name )
		{
			this.type = new CodeTypeReference( type );
			this.name = name;
		}

		public CodeVariableDeclarationStatement( Type type, string name )
		{
			this.type = new CodeTypeReference( type );
			this.name = name;
		}

		public CodeVariableDeclarationStatement( CodeTypeReference type, 
							 string name,
							 CodeExpression initExpression )
		{
			this.type = type;
			this.name = name;
			this.initExpression = initExpression;
		}

		public CodeVariableDeclarationStatement( string type,
							 string name, 
							 CodeExpression initExpression )
		{
			this.type = new CodeTypeReference( type );
			this.name = name;
			this.initExpression = initExpression;
		}

		public CodeVariableDeclarationStatement( Type type, 
							 string name, 
							 CodeExpression initExpression )
		{
			this.type = new CodeTypeReference( type );
			this.name = name;
			this.initExpression = initExpression;
		}


		//
		// Properties
		//
		public CodeExpression InitExpression {
			get {
				return initExpression;
			}
			set {
				initExpression = value;
			}
		}

		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

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
