//
// System.CodeDom CodeVariableDeclarationStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeVariableDeclarationStatement : CodeStatement  {
		CodeExpression initExpression;
		string type, name;

		public CodeVariableDeclarationStatement () {}

		public CodeVariableDeclarationStatement (string type, string name)
		{
			this.type = type;
			this.name = name;
		}

		public CodeVariableDeclarationStatement (string type, string name,
							 CodeExpression initExpression)
		{
			this.type = type;
			this.name = name;
			this.initExpression = initExpression;
		}

		public CodeExpression InitExpression {
			get {
				return initExpression;
			}

			set {
				initExpression = value;
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
