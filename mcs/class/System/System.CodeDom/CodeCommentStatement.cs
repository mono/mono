//
// System.CodeDom CodeCommentStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeCommentStatement : CodeStatement {
		string text;
		
		//
		// Constructors
		//
		public CodeCommentStatement ()
		{
		}

		public CodeCommentStatement (string text)
		{
			this.text = text;
		}

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

			
