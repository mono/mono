//
// System.CodeDom CodeMemberField Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeMemberField : CodeClassMember {
		CodeExpression initExpression;
		string type, name;
		
		public CodeMemberField ()
		{
		}

		public CodeMemberField (string type, string name)
		{
			this.type = type;
			this.name = name;
		}

		//
		// Properties
		//
		public CodeExpression InitExpression {
			get {
				return initExpression;
			}

			set {
				initExpression = value;
			}
		}

		public string Type {
			get {
				return type;
			}

			set {
				type = name;
			}
		}
	}
}
