//
// System.CodeDom CodeSnippetTypeMember Class implementation
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
	public class CodeSnippetTypeMember
		: CodeTypeMember
	{
		private string text;

		//
		// Constructors
		//
		public CodeSnippetTypeMember()
		{
		}
		
		public CodeSnippetTypeMember( string text )
		{
			this.text = text;
		}

		//
		// Properties
		//
		public string Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}
	}
}
