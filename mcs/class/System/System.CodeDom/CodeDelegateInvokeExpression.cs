//
// System.CodeDom CodeDelegateInvokeExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeDelegateInvokeExpression : CodeExpression {
		CodeExpressionCollection parameters;
		CodeExpression targetObject;
		
		//
		// Constructors
		//
		public CodeDelegateInvokeExpression ()
		{
		}
		
		public CodeDelegateInvokeExpression (CodeExpression targetObject,
						     CodeExpression [] parameters)
		{
			this.targetObject = targetObject;
			this.parameters = new CodeExpressionCollection ();
			this.parameters.AddRange (parameters);
		}

		public CodeDelegateInvokeExpression (CodeExpression targetObject)
		{
			this.targetObject = targetObject;
		}

		//
		// Properties
		//
		public CodeExpression TargetObject {
			get {
				return targetObject;
			}

			set {
				targetObject = value;
			}
		}

		public CodeExpressionCollection Parameters {
			get {
				return parameters;
			}

			set {
				parameters = value;
			}
		}
	}
}
