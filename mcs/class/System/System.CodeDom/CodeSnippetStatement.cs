//
// System.CodeDom CodeSnippetStatement Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeSnippetStatement
		: CodeStatement 
	{
		private string value;

		//
		// Constructors
		//
		public CodeSnippetStatement()
		{
		}

		public CodeSnippetStatement( string value )
		{
			this.value = value;
		}
		
		//
		// Properties
		//
		public string Value {
			get {
				return this.value;
			}
			set {
				this.value = value;
			}
		}
	}
}
