//
// System.CodeDom CodePropertyReferenceExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	public class CodePropertyReferenceExpression : CodeExpression {
		CodeExpressionCollection parameters;
		CodeExpression targetObject;
		string propertyName;
		
		public CodePropertyReferenceExpression () {}

		public CodePropertyReferenceExpression (CodeExpression targetObject,
							string propertyName)
		{
			this.targetObject = targetObject;
			this.propertyName = propertyName;
		}

		//
		// Properties
		//
		public CodeExpressionCollection Parameter {
			get {
				return parameters;
			}

			set {
				parameters = value;
			}
		}

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
