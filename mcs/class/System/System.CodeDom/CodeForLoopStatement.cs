//
// System.CodeDom CodeForLoopStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeForLoopStatement : CodeExpression {
		CodeStatement initStatement, incrementStatement;
		CodeExpression testExpression;
		CodeStatementCollection statements;

		//
		// Constructors
		//
		public CodeForLoopStatement ()
		{
			statements = new CodeStatementCollection ();
		}

		public CodeForLoopStatement (CodeStatement initStatement,
					     CodeExpression testExpression,
					     CodeStatement incrementStatement,
					     CodeStatement [] statements)
		{
			this.initStatement = initStatement;
			this.testExpression = testExpression;
			this.incrementStatement = incrementStatement;
			this.statements = new CodeStatementCollection ();
			this.statements.AddRange (statements);
		}

		//
		// Properties
		//
		
		public CodeStatement InitStatement {
			get {
				return initStatement;
			}

			set {
				initStatement = value;
			}
		}

		public CodeStatement IncrementStatement {
			get {
				return incrementStatement;
			}

			set {
				incrementStatement = value;
			}
		}

		public CodeStatementCollection Statements {
			get {
				return statements;
			}
		}

		public CodeExpression TestExpression {
			get {
				return testExpression;
			}

			set {
				testExpression = value;
			}
		}
	}
}

