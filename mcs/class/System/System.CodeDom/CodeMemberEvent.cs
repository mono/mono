//
// System.CodeDom CodeMemberEvent Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeMemberEvent : CodeClassMember {
		string implementsType, type;
		bool   privateImplements;
		
		public CodeMemberEvent ()
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

		public string Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}
	}
}
