//
// System.CodeDom CodeDelegateInvokeExpression Class implementation
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
	public class CodeDelegateInvokeExpression
		: CodeExpression 
	{
		private CodeExpressionCollection parameters;
		private CodeExpression targetObject;
		
		//
		// Constructors
		//
		public CodeDelegateInvokeExpression ()
		{
		}
		
		public CodeDelegateInvokeExpression (CodeExpression targetObject)
		{
			this.targetObject = targetObject;
		}

		public CodeDelegateInvokeExpression (CodeExpression targetObject,
						     params CodeExpression [] parameters)
		{
			this.targetObject = targetObject;
			this.Parameters.AddRange( parameters );
		}


		//
		// Properties
		//
		public CodeExpressionCollection Parameters {
			get {
				if ( parameters == null )
					parameters = new CodeExpressionCollection();
				return parameters;
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
