//
// System.CodeDom CodeCommentStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001, 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeCommentStatement
		: CodeStatement
	{
		private CodeComment comment;
		
		//
		// Constructors
		//
		public CodeCommentStatement ()
		{
		}

		public CodeCommentStatement (CodeComment comment)
		{
			this.comment = comment;
		}

		public CodeCommentStatement (string text)
		{
			this.comment = new CodeComment( text );
		}
		
		public CodeCommentStatement (string text, bool docComment)
		{
			this.comment = new CodeComment( text, docComment );
		}

		//
		// Properties
		//
		public CodeComment Comment {
			get {
				return comment;
			}
			set {
				comment = value;
			}
		}
	}
}
