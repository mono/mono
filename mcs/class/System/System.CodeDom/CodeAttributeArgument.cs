//
// System.CodeDom CodeAttributeArgument Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeAttributeArgument {
		string name;
		CodeExpression val;
		
		//
		// Constructors
		//
		public CodeAttributeArgument ()
		{
		}

		public CodeAttributeArgument (CodeExpression value)
		{
		}

		public CodeAttributeArgument (string name, CodeExpression val)
		{
			this.name = name;
			this.val = val;
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
				return val;
			}

			set {
				val = value;
			}
		}

		
	}

}
