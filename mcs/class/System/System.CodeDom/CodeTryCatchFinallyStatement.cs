//
// System.CodeDOM CodeTryCatchFinallyStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeTryCatchFinallyStatement : CodeStatement {
		CodeStatementCollection tryStatements, finallyStatements;
		CodeCatchClauseCollection catchClauses;
		
		public CodeTryCatchFinallyStatement () {}
		
		public CodeTryCatchFinallyStatement (CodeStatement []   tryStatements,
						     CodeCatchClause [] catchClauses)
		{
			this.tryStatements = new CodeStatementCollection ();
			this.catchClauses = new CodeCatchClauseCollection ();

			this.tryStatements.AddRange (tryStatements);
			this.catchClauses.AddRange (catchClauses);
		}

		public CodeTryCatchFinallyStatement (CodeStatement []   tryStatements,
						     CodeCatchClause [] catchClauses,
						     CodeStatement []   finallyStatements)
		{
			this.tryStatements = new CodeStatementCollection ();
			this.catchClauses = new CodeCatchClauseCollection ();
			this.finallyStatements = new CodeStatementCollection ();

			this.tryStatements.AddRange (tryStatements);
			this.catchClauses.AddRange (catchClauses);
			this.finallyStatements.AddRange (finallyStatements);
		}

		public CodeStatementCollection FinallyStatements{
			get {
				return finallyStatements;
			}

			set {
				finallyStatements = value;
			}
		}

		public CodeStatementCollection TryStatements {
			get {
				return tryStatements;
			}

			set {
				tryStatements = value;
			}
		}
		public CodeCatchClauseCollection CatchClauses {
			get {
				return catchClauses;
			}

			set {
				catchClauses = value;
			}
		}
	}
}
