//
// System.CodeDom CodeMemberMethod Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeMemberMethod : CodeTypeMember {
		CodeParameterDeclarationExpressionCollection parameters;
		CodeStatementCollection statements;
		string implementsType;
		string returnType;
		bool   privateImplements;
		
		public CodeMemberMethod ()
		{
		}

		public string ImplementsType {
			get {
				return implementsType;
			}

			set {
				implementsType = value;
			}
		}

		public bool PrivateImplements {
			get {
				return privateImplements;
			}

			set {
				privateImplements = value;
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

		public CodeParameterDeclarationExpressionCollection Parameters {
			get {
				return parameters;
			}

			set {
				parameters = value;
			}
		}

		public CodeStatementCollection Statements {
			get {
				return statements;
			}

			set {
				statements = value;
			}
		}
	}
}
