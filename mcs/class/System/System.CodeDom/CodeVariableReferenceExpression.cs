//
// System.CodeDom CodeVariableReferenceExpression Class implementation
//
// Author:
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
	public class CodeVariableReferenceExpression
		: CodeExpression 
	{
		private string variableName;

		//
		// Constructors
		//
		public CodeVariableReferenceExpression() 
		{
		}

		public CodeVariableReferenceExpression( string variableName )
		{
			this.variableName = variableName;
		}

		//
		// Properties
		//
		public string VariableName {
			get {
				return variableName;
			}
			set {
				variableName = value;
			}
		}
	}
}
