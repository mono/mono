//
// System.CodeDom CodeIterationStatement Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeIterationStatement
		: CodeStatement
	{
		private CodeStatement incrementStatement;
		private CodeStatement initStatement;
		private CodeStatementCollection statements;
		private CodeExpression testExpression;
		
		//
		// Constructors
		//
		public CodeIterationStatement()
		{
		}

		public CodeIterationStatement( CodeStatement initStatement, 
					       CodeExpression testExpression,
					       CodeStatement incrementStatement,
					       params CodeStatement[] statements )
		{
			this.initStatement = initStatement;
			this.testExpression = testExpression;
			this.incrementStatement = incrementStatement;
			this.Statements.AddRange( statements );
		}

		//
		// Properties
		//
		public CodeStatement IncrementStatement {
			get {
				return incrementStatement;
			}
			set {
				incrementStatement = value;
			}
		}

		public CodeStatement InitStatement {
			get {
				return initStatement;
			}
			set {
				initStatement = value;
			}
		}

		public CodeStatementCollection Statements {
			get {
				if ( statements == null )
					statements = new CodeStatementCollection();
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
