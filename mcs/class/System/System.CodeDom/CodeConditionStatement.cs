//
// System.CodeDom CodeConditionStatement Class implementation
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
	public class CodeConditionStatement
		: CodeStatement
	{
		private CodeExpression condition;
		private CodeStatementCollection trueStatements;
		private CodeStatementCollection falseStatements;

		//
		// Constructors
		//
		public CodeConditionStatement()
		{
		}

		public CodeConditionStatement( CodeExpression condition, 
					       params CodeStatement[] trueStatements )
		{
			this.condition = condition;
			this.TrueStatements.AddRange( trueStatements );
		}

		public CodeConditionStatement( CodeExpression condition,
					       CodeStatement[] trueStatements,
					       CodeStatement[] falseStatements )
		{
			this.condition = condition;
			this.TrueStatements.AddRange( trueStatements );
			this.FalseStatements.AddRange( falseStatements );
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
				if ( falseStatements == null )
					falseStatements = 
						new CodeStatementCollection();
				return falseStatements;
			}
		}

		public CodeStatementCollection TrueStatements {
			get {
				if ( trueStatements == null )
					trueStatements = 
						new CodeStatementCollection();
				return trueStatements;
			}
		}
 	}
}
