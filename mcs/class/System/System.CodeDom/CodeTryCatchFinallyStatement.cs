//
// System.CodeDom CodeTryCatchFinallyStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeTryCatchFinallyStatement
		: CodeStatement 
	{
		private CodeStatementCollection tryStatements, finallyStatements;
		private CodeCatchClauseCollection catchClauses;
		
		//
		// Constructors
		//
		public CodeTryCatchFinallyStatement ()
		{
		}
		
		public CodeTryCatchFinallyStatement (CodeStatement [] tryStatements,
						     CodeCatchClause [] catchClauses)
		{
			TryStatements.AddRange( tryStatements );
			CatchClauses.AddRange( catchClauses );
		}

		public CodeTryCatchFinallyStatement (CodeStatement [] tryStatements,
						     CodeCatchClause [] catchClauses,
						     CodeStatement [] finallyStatements)
		{
			TryStatements.AddRange( tryStatements );
			CatchClauses.AddRange( catchClauses );
			FinallyStatements.AddRange( finallyStatements );
		}

		//
		// Properties
		//
		public CodeStatementCollection FinallyStatements{
			get {
				if ( finallyStatements == null )
					finallyStatements = new CodeStatementCollection();
				return finallyStatements;
			}
		}

		public CodeStatementCollection TryStatements {
			get {
				if ( tryStatements == null )
					tryStatements = new CodeStatementCollection();
				return tryStatements;
			}
		}
		public CodeCatchClauseCollection CatchClauses {
			get {
				if ( catchClauses == null )
					catchClauses = new CodeCatchClauseCollection();
				return catchClauses;
			}
		}
	}
}
