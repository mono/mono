//
// System.CodeDom CodeMemberProperty Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeMemberProperty : CodeTypeMember {
		CodeParameterDeclarationExpressionCollection parameters;
		CodeStatementCollection getStatements, setStatements;
		bool hasGet, hasSet;
		string implementsType, type;
		bool   privateImplements;
		
		public CodeMemberProperty ()
		{
		}

		//
		// Properties
		//

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

		public CodeParameterDeclarationExpressionCollection Parameters {
			get {
				return parameters;
			}

			set {
				parameters = value;
			}
		}

		public CodeStatementCollection SetStatements {
			get {
				return setStatements;
			}

			set {
				setStatements = value;
			}
		}

		public CodeStatementCollection GetStatements {
			get {
				return getStatements;
			}

			set {
				getStatements = value;
			}
		}

		public string Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}

		public bool HasGet {
			get {
				return hasGet;
			}

			set {
				hasGet = value;
			}
		}

		public bool HasSet {
			get {
				return hasSet;
			}

			set {
				hasSet = value;
			}
		}
	}
}

