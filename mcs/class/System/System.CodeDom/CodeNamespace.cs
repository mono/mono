//
// System.CodeDom CodeNamespace Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeNamespace {
		CodeClassCollection classes;
		CodeNamespaceImportCollection imports;
		bool allowLateBound, requireVariableDeclaration;
		string name;
		object userData;

		public CodeNamespace  ()
		{
		}

		public CodeNamespace (string name)
		{
			this.name = name;
		}

		//
		// Properties
		//

		public bool AllowLateBound {
			get {
				return allowLateBound;
			}

			set {
				allowLateBound = value;
			}
		}

		public CodeClassCollection Classes {
			get {
				return classes;
			}

			set {
				classes = value;
			}
		}

		public CodeNamespaceImportCollection Imports {
			get {
				return imports;
			}

			set {
				imports = value;
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

		public bool RequireVariableDeclaration {
			get {
				return requireVariableDeclaration;
			}

			set {
				requireVariableDeclaration = value;
			}
		}

		public object UserData {
			get {
				return userData;
			}

			set {
				userData = value;
			}
		}
	}
}
