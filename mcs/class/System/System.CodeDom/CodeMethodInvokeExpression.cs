//
// System.CodeDom CodeMethodInvokeExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeMethodInvokeExpression : CodeExpression {
		string methodName;
		CodeExpression targetObject;
		CodeExpressionCollection parameters;
		
		//
		// Constructors
		//
		public CodeMethodInvokeExpression () {}

		public CodeMethodInvokeExpression (CodeExpression targetObject, string methodName)
		{
			this.targetObject = targetObject;
			this.methodName = methodName;
		}

		public CodeMethodInvokeExpression (CodeExpression targetObject,
						   string methodName,
						   CodeExpression [] parameters)
		{
			this.targetObject = targetObject;
			this.methodName = methodName;
			this.parameters = new CodeExpressionCollection ();
			this.parameters.AddRange (parameters);
		}

		public string MethodName {
			get {
				return methodName;
			}

			set {
				methodName = value;
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
