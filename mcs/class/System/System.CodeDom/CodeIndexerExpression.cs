//
// System.CodeDom CodeFieldReferenceExpression Class implementation
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
	public class CodeIndexerExpression
		: CodeExpression 
	{
		private CodeExpression targetObject;
		private CodeExpressionCollection indices;

		//
		// Constructors
		//
		public CodeIndexerExpression ()
		{
		}
		
		public CodeIndexerExpression (CodeExpression targetObject, params CodeExpression[] indices)
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
