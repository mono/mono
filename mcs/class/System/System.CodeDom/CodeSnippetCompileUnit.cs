//
// System.CodeDom CodeSnippetCompileUnit Class implementation
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
	public class CodeSnippetCompileUnit
		: CodeCompileUnit
	{
		private CodeLinePragma linePragma;
		private string value;

		//
		// Constructors
		//
		public CodeSnippetCompileUnit( string value )
		{
			this.value = value;
		}

		//
		// Properties
		//
		public CodeLinePragma LinePragma {
			get {
				return linePragma;
			}
			set {
				linePragma = value;
			}
		}

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
