//
// System.CodeDom CodeLiteralClassMember Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeLiteralClassMember : CodeClassMember {
		string text;

		//
		// Constructors
		//
		public CodeLiteralClassMember ()
		{
		}

		public CodeLiteralClassMember (string text)
		{
			this.text = text;
		}

		//
		// Properties
		//
		string Text {
			get {
				return text;
			}

			set {
				text = value;
			}
		}
	}
}
