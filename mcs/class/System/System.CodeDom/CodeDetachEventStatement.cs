//
// System.CodeDOM CodeDetachEventStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDOM {

	public class CodeDetachEventStatement : CodeStatement {
		string eventName;
		CodeExpression targetObject, newListener;
		
		//
		// Constructors
		//
		public CodeDetachEventStatement ()
		{
		}

		public CodeDetachEventStatement (CodeExpression targetObject,
						 string eventName,
						 CodeExpression newListener)
		{
			this.targetObject = targetObject;
			this.eventName = eventName;
			this.newListener = newListener;
		}

		public string EventName {
			get {
				return eventName;
			}

			set {
				eventName = value;
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

		public CodeExpression NewListener {
			get {
				return newListener;
			}

			set {
				newListener = value;
			}
		}
	}
}
