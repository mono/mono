//
// System.CodeDom CodeFieldReferenceExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {

	[Serializable]
	public class CodeFieldReferenceExpression : CodeExpression {
		CodeExpression targetObject;
		string fieldName;
		FieldDirection direction;

		public enum FieldDirection {
			In,
			Out,
			Ref
		}
			
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
		public FieldDirection Direction {
			get {
				return direction;
			}

			set {
				direction = value;
			}
		}

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
