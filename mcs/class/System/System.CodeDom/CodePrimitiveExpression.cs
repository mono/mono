//
// System.CodeDom CodePrimitiveExpression Class implementation
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
	public class CodePrimitiveExpression
		: CodeExpression 
	{
		private object value;

		//
		// Constructors
		//
		public CodePrimitiveExpression ()
		{
		}

		public CodePrimitiveExpression (Object value)
		{
			this.value = value;
		}

		//
		// Properties
		//
		public object Value {
			get {
				return this.value;
			}
			set {
				this.value = value;
			}
		}
	}
}
