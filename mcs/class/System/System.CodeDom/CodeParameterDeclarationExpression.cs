//
// System.CodeDom CodeParameterDeclarationExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeParameterDeclarationExpression : CodeExpression {
		FieldDirection direction;
		CodeAttributeBlock customAttributes;
		string type, name;
		
		public CodeParameterDeclarationExpression ()
		{
		}

		public CodeParameterDeclarationExpression (string type, string name)
		{
			this.type = type;
			this.name = name;
		}

		public string Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}

		public string Name {
			get {
				return name;
			}

			set {
				name = value;
			}
		}

		public CodeAttributeBlock CustomAttributes {
			get {
				return customAttributes;
			}

			set {
				customAttributes = value;
			}
		}

		public FieldDirection Direction {
			get {
				return direction;
			}

			set {
				direction = value;
			}
		}
	}
}

