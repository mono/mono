//
// System.CodeDom CodeTypeDelegate Class implementation
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
	public class CodeTypeDelegate
		: CodeTypeDeclaration
	{
		private	CodeParameterDeclarationExpressionCollection parameters;
		private CodeTypeReference returnType;

		//
		// Constructors
		//
		public CodeTypeDelegate()
		{
		}

		public CodeTypeDelegate( string name )
		{
			this.Name = name;
		}

		//
		// Properties
		//
		public CodeParameterDeclarationExpressionCollection Parameters {
			get {
				if ( parameters == null )
					parameters = new CodeParameterDeclarationExpressionCollection();
				return parameters;
			}
		}

		public CodeTypeReference ReturnType {
			get {
				return returnType;
			}
			set {
				returnType = value;
			}
		}
	}
}

