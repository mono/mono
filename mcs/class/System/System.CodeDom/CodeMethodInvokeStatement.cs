//
// System.CodeDom CodeMethodInvokeStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeMethodInvokeStatement : CodeStatement {
		string methodName;
		CodeExpression targetObject;
		CodeExpressionCollection parameters;
		CodeMethodInvokeExpression methodInvoke;
		
		//
		// Constructors
		//
		public CodeMethodInvokeStatement () {}

		public CodeMethodInvokeStatement (CodeMethodInvokeExpression methodInvoke)
		{
			this.methodInvoke = methodInvoke;
		}
		
		public CodeMethodInvokeStatement (CodeExpression targetObject, string methodName)
		{
			this.targetObject = targetObject;
			this.methodName = methodName;
		}

		public CodeMethodInvokeStatement (CodeExpression targetObject,
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

		public CodeMethodInvokeExpression MethodInvoke {
			get {
				return methodInvoke;
			}

			set {
				methodInvoke = value;
			}
		}
	}
}
