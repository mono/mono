//
// System.CodeDOM CodeDelegateCreateExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeDelegateCreateExpression : CodeExpression {
		string delegateType, methodName;
		CodeExpression targetObject;

		//
		// Constructors
		//
		public CodeDelegateCreateExpression (string delegateType,
						     CodeExpression targetObject,
						     string methodName)
		{
			this.delegateType = delegateType;
			this.targetObject = targetObject;
			this.methodName = methodName;
		}

		public CodeDelegateCreateExpression ()
		{
		}

		//
		// Properties
		//

		string DelegateType {
			get {
				return delegateType;
			}

			set {
				delegateType = value;
			}
		}

		string MethodName {
			get {
				return methodName;
			}

			set {
				methodName = value;
			}
		}

		CodeExpression TargetObject {
			get {
				return targetObject;
			}

			set {
				targetObject = value;
			}
		}
	}
}
