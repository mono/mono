//
// System.CodeDom CodeCatchClaus Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeCatchClause {

		CodeParameterDeclarationExpression condition;
		CodeStatementCollection statements;
		
		public CodeCatchClause ()
		{
			this.statements = new CodeStatementCollection ();
		}

		public CodeCatchClause (CodeParameterDeclarationExpression condition,
					CodeStatement [] statements)
		{
			this.condition = condition;
			this.statements = new CodeStatementCollection ();
			this.statements.AddRange (statements);
		}

		public CodeStatementCollection Statements {
			get {
				return statements;
			}
		}

		public CodeParameterDeclarationExpression Condition {
			get {
				return condition;
			}

			set {
				condition = value;
			}
		}
	}
}
