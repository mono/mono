//
// System.CodeDom CodeDelegateInvokeStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodeDelegateInvokeStatement : CodeStatement {
		CodeStatementCollection parameters;
		CodeStatement targetObject;
		CodeDelegateInvokeExpression delegateInvoke;
		
		//
		// Constructors
		//
		public CodeDelegateInvokeStatement ()
		{
		}

		public CodeDelegateInvokeStatement (CodeStatement targetObject)
		{
			this.targetObject = targetObject;
		}

		public CodeDelegateInvokeStatement (CodeDelegateInvokeExpression delegateInvoke)
		{
			this.delegateInvoke = delegateInvoke;
		}
		
		public CodeDelegateInvokeStatement (CodeStatement targetObject,
						     CodeStatement [] parameters)
		{
			this.targetObject = targetObject;
			this.parameters = new CodeStatementCollection ();
			this.parameters.AddRange (parameters);
		}

		//
		// Properties
		//
		public CodeDelegateInvokeExpression DelegateInvoke {
			get {
				return delegateInvoke;
			}	

			set {
				delegateInvoke = value;
			}
		}
	}
}
