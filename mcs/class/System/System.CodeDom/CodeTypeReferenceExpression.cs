//
// System.CodeDom CodeTypeReferenceExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeTypeReferenceExpression : CodeExpression {
		string type;
		
		public CodeTypeReferenceExpression () {}

		public CodeTypeReferenceExpression (string type)
		{
			this.type = type;
		}

		public string Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}
	}
}
