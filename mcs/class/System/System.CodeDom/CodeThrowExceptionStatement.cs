//
// System.CodeDom CodeThrowExceptionStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeThrowExceptionStatement
		: CodeStatement 
	{
		private CodeExpression toThrow;
		
		//
		// Constructors
		//
		public CodeThrowExceptionStatement ()
		{
		}

		public CodeThrowExceptionStatement (CodeExpression toThrow)
		{
			this.toThrow = toThrow;
		}

		//
		// Properties
		//
		public CodeExpression ToThrow {
			get {
				return toThrow;
			}
			set {
				toThrow = value;
			}
		}
	}
}
