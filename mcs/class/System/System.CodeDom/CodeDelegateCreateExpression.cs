//
// System.CodeDom CodeDelegateCreateExpression Class implementation
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
	public class CodeDelegateCreateExpression
		: CodeExpression 
	{
		private CodeTypeReference delegateType;
		private string methodName;
		private CodeExpression targetObject;

		//
		// Constructors
		//
		public CodeDelegateCreateExpression ()
		{
		}

		public CodeDelegateCreateExpression (CodeTypeReference delegateType,
						     CodeExpression targetObject,
						     string methodName)
		{
			this.delegateType = delegateType;
			this.targetObject = targetObject;
			this.methodName = methodName;
		}


		//
		// Properties
		//
		public CodeTypeReference DelegateType {
			get {
				return delegateType;
			}
			set {
				delegateType = value;
			}
		}

		public string MethodName {
			get {
				return methodName;
			}
			set {
				methodName = value;
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
