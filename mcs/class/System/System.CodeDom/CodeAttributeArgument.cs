//
// System.CodeDom CodeAttributeArgument Class implementation
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
	public class CodeAttributeArgument
	{
		private string name;
		private CodeExpression value;
		
		//
		// Constructors
		//
		public CodeAttributeArgument ()
		{
		}

		public CodeAttributeArgument (CodeExpression value)
		{
			this.value = value;
		}

		public CodeAttributeArgument (string name, CodeExpression value)
		{
			this.name = name;
			this.value = value;
		}

		//
		// Properties
		//
		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public CodeExpression Value {
			get {
				return this.value;
			}
			set {
				this.value = value;
			}
		}
	}
}
