//
// System.CodeDom CodeStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeStatement {

		CodeLinePragma codeLinePragma;
		object         userData;
		
		//
		// Constructors
		//
		public CodeStatement ()
		{
		}

		//
		// Properties
		//
		public CodeLinePragma LinePragma {
			get {
				return codeLinePragma;
			}

			set {
				codeLinePragma = value;
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
