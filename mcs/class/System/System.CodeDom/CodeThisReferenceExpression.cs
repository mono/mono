//
// System.CodeDom CodeParameterDeclarationExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeThisReferenceExpression
		: CodeExpression 
	{

		//
		// Constructors
		//
		public CodeThisReferenceExpression()
		{
		}
	}
}
