//
// System.CodeDom CodeObjectCreateExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeObjectCreateExpression : CodeExpression {
		string createType;
		CodeExpressionCollection parameters;
		
		public CodeObjectCreateExpression () {}

		public CodeObjectCreateExpression (string createType)
		{
			this.createType = createType;
		}

		public CodeObjectCreateExpression (string createType, CodeExpression [] parameters)
		{
			this.createType = createType;
			this.parameters = new CodeExpressionCollection ();
			this.parameters.AddRange (parameters);
		}

		//
		// Properties
		//
		public string CreateType {
			get {
				return createType;
			}

			set {
				createType = value;
			}
		}

		public CodeExpressionCollection Parameters {
			get {
				return parameters;
			}

			set {
				parameters = value;
			}
		}
		
	}
}
