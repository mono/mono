//
// System.CodeDom CodeFieldReferenceExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeIndexerExpression : CodeExpression {
		CodeExpression targetObject;
		CodeExpression index;

		//
		// Constructors
		//
		public CodeIndexerExpression ()
		{
		}
		
		public CodeIndexerExpression (CodeExpression targetObject, CodeExpression index)
		{
			this.index = index;
			this.targetObject = targetObject;
		}

		//
		// Properties
		//
	}
}

