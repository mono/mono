//
// System.CodeDom CodeSnippetExpression Class implementation
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
	public class CodeSnippetExpression
		: CodeExpression 
	{
		private string value;

		//
		// Constructors
		//
		public CodeSnippetExpression()
		{
		}

		public CodeSnippetExpression( string value )
		{
			Value = value;
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
