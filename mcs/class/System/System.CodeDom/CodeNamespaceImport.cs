//
// System.CodeDOM CodeNamespaceImport Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeNamespaceImport : CodeStatement {
		string nameSpace;
		
		public CodeNamespaceImport () {}

		public CodeNamespaceImport (string nameSpace)
		{
			this.nameSpace = nameSpace;
		}

		//
		// Properties
		//

		public string Namespace {
			get {
				return nameSpace;
			}

			set {
				nameSpace = value;
			}
		}
	}
}
