//
// System.CodeDom CodeComment Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeComment
		: CodeObject
	{
		private bool docComment;
		private string text;
		
		//
		// Constructors
		//
		public CodeComment()
		{
		}

		public CodeComment( string text )
		{
			this.text = text;
		}

		public CodeComment( string text, bool docComment )
		{
			this.text = text;
			this.docComment = docComment;
		}

		//
		// Properties
		//
		public bool DocComment {
			get {
				return docComment;
			}
			set {
				docComment = value;
			}
		}
		
		public string Text {
			get {
				if (text == null)
					return String.Empty;
				return text;
			}
			set {
				text = value;
			}
		}
	}
}
