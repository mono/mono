//
// System.CodeDom CodeFieldReferenceExpression Class implementation
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
	public class CodeFieldReferenceExpression
		: CodeExpression 
	{
		private CodeExpression targetObject;
		private string fieldName;

		//
		// Constructors
		//
		public CodeFieldReferenceExpression ()
		{
		}
		
		public CodeFieldReferenceExpression (CodeExpression targetObject,
						     string fieldName)
		{
			this.targetObject = targetObject;
			this.fieldName = fieldName;
		}

		//
		// Properties
		//
		public string FieldName {
			get {
				return fieldName;
			}
			set {
				fieldName = value;
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
