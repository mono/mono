//
// System.CodeDOM CodeConstructor Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeConstructor : CodeMemberMethod {
		CodeExpressionCollection baseConstructorArgs;
		CodeExpressionCollection chainedConstructorArgs;
		
		//
		// Constructors
		//
		public CodeConstructor ()
		{
		}

		public CodeExpressionCollection BaseConstructorArgs {
			get {
				return baseConstructorArgs;
			}

			set {
				baseConstructorArgs = value;
			}
		}

		public CodeExpressionCollection ChainedConstructorArgs {
			get {
				return chainedConstructorArgs;
			}

			set {
				chainedConstructorArgs = value;
			}
		}
	}
}
