//
// System.CodeDom CodeConstructor Class implementation
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
	public class CodeConstructor
		: CodeMemberMethod 
	{
		private CodeExpressionCollection baseConstructorArgs;
		private CodeExpressionCollection chainedConstructorArgs;

		//
		// Constructors
		//
		public CodeConstructor()
		{
		}

		//
		// Properties
		//
		public CodeExpressionCollection BaseConstructorArgs {
			get {
				if ( baseConstructorArgs == null )
					baseConstructorArgs = new CodeExpressionCollection();

				return baseConstructorArgs;
			}
		}

		public CodeExpressionCollection ChainedConstructorArgs {
			get {
				if ( chainedConstructorArgs == null )
					chainedConstructorArgs = new CodeExpressionCollection();

				return chainedConstructorArgs;
			}
		}
	}
}
