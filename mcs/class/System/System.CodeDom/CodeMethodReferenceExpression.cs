//
// System.CodeDom CodeMethodInvokeExpression Class implementation
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
	public class CodeMethodReferenceExpression
		: CodeExpression 
	{
		private string methodName;
		private CodeExpression targetObject;
		
		//
		// Constructors
		//
		public CodeMethodReferenceExpression ()
		{
		}

		public CodeMethodReferenceExpression (CodeExpression targetObject, 
						      string methodName)
		{
			this.targetObject = targetObject;
			this.methodName = methodName;
		}

		//
		// Properties
		//
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
