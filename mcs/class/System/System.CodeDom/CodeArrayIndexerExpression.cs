//
// System.CodeDom CodeArrayIndexerExpression Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom {

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeArrayIndexerExpression
		: CodeExpression 
	{
		private CodeExpressionCollection indices;
		private CodeExpression targetObject;

		//
		// Constructors
		//
		public CodeArrayIndexerExpression()
		{
		}

		public CodeArrayIndexerExpression( CodeExpression targetObject, params CodeExpression[] indices )
		{
			this.targetObject = targetObject;
			this.Indices.AddRange( indices );
		}

		//
		// Properties
		//
		public CodeExpressionCollection Indices {
			get {
				if ( indices == null )
					indices = new CodeExpressionCollection();
				return indices;
			}
		}

		public CodeExpression TargetObject {
			get {
				return targetObject;
			}
			set {
				targetObject = value;
			}
		}
	}
}
