//
// System.CodeDom CodeArgumentReferenceExpression Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom {

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeArgumentReferenceExpression 
		: CodeExpression
	{
		private string parameterName;

		//
		// Constructors
		//
		public CodeArgumentReferenceExpression()
		{
		}

		public CodeArgumentReferenceExpression( string name )
		{
			this.parameterName = name;
		}

		//
		// Properties
		//
		public string ParameterName {
			get {
				return parameterName;
			}
			set {
				parameterName = value;
			}
		}
	}
}
