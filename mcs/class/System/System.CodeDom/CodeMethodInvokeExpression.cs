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
	public class CodeMethodInvokeExpression
		: CodeExpression 
	{
		private CodeMethodReferenceExpression method;
		private CodeExpressionCollection parameters;
		
		//
		// Constructors
		//
		public CodeMethodInvokeExpression () 
		{
		}

		public CodeMethodInvokeExpression (CodeMethodReferenceExpression method, params CodeExpression[] parameters)
		{
			this.method = method;
			this.Parameters.AddRange( parameters );
		}

		public CodeMethodInvokeExpression (CodeExpression targetObject,
						   string methodName,
						   params CodeExpression [] parameters)
		{
			this.method = new CodeMethodReferenceExpression( targetObject, methodName );
			this.Parameters.AddRange (parameters);
		}

		//
		// Properties
		//
		public CodeMethodReferenceExpression Method {
			get {
				return method;
			}
			set {
				method = value;
			}
		}

		public CodeExpressionCollection Parameters {
			get {
				if ( parameters == null )
					parameters = new CodeExpressionCollection();
				return parameters;
			}
		}
	}
}
