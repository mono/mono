//
// System.CodeDOM CodeClassDelegate Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeClassDelegate : CodeClass {
		CodeParameterDeclarationExpressionCollection parameters;
		string returnType;
		string name;

		//
		// Constructors
		//
		public CodeClassDelegate ()
		{
		}

		public CodeClassDelegate (string name)
		{
			this.name = name;
		}

		//
		// Properties
		//
		public CodeParameterDeclarationExpressionCollection Parameters {
			get {
				return parameters;
			}

			set {
				parameters = value;
			}
		}

		public string ReturnType {
			get {
				return returnType;
			}

			set {
				returnType = value;
			}
		}
	}
}
