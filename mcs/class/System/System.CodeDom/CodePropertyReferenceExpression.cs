//
// System.CodeDom CodePropertyReferenceExpression Class implementation
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
	public class CodePropertyReferenceExpression
		: CodeExpression 
	{
		private CodeExpression targetObject;
		private string propertyName;
		
		//
		// Constructors
		//
		public CodePropertyReferenceExpression () 
		{
		}

		public CodePropertyReferenceExpression (CodeExpression targetObject,
							string propertyName)
		{
			this.targetObject = targetObject;
			this.propertyName = propertyName;
		}

		//
		// Properties
		//
		public string PropertyName {
			get {
				return propertyName;
			}
			set {
				propertyName = value;
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
