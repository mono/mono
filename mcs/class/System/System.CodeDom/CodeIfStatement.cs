//
// System.CodeDOM CodeIfStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeIfStatement : CodeStatement {

		CodeExpression condition;
		CodeStatementCollection trueStatements;
		CodeStatementCollection falseStatements;
		
		//
		// Constructors
		//
		public CodeIfStatement ()
		{
			trueStatements = new CodeStatementCollection ();
			falseStatements = new CodeStatementCollection ();
		}
		
		public CodeIfStatement (CodeExpression condition, CodeStatement [] trueStatements)
		{
			this.condition = condition;
			this.trueStatements = new CodeStatementCollection ();
			this.trueStatements.AddRange (trueStatements);
			this.falseStatements = new CodeStatementCollection ();
		}

		public CodeIfStatement (CodeExpression condition,
					CodeStatement [] trueStatements,
					CodeStatement [] falseStatements)
		{
			this.condition = condition;

			this.trueStatements = new CodeStatementCollection ();
			this.trueStatements.AddRange (trueStatements);

			this.falseStatements = new CodeStatementCollection ();
			this.falseStatements.AddRange (falseStatements);
		}

		//
		// Properties
		//
		public CodeExpression Condition {
			get {
				return condition;
			}

			set {
				condition = value;
			}
		}

		public CodeStatementCollection FalseStatements {
			get {
				return falseStatements;
			}
		}
		
		public CodeStatementCollection TrueStatements {
			get {
				return trueStatements;
			}
		}
	}
}
