//
// System.CodeDOM CodeAttachEventStatement Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
namespace System.CodeDOM {

	public class CodeAttachEventStatement : CodeStatement {
		CodeExpression targetObject;
		string eventName;
		CodeExpression newListener;
		
		public CodeAttachEventStatement ()
		{
		}

		public CodeAttachEventStatement (CodeExpression targetObject,
						 string eventName,
						 CodeExpression newListener)
		{
			this.targetObject = targetObject;
			this.eventName = eventName;
			this.newListener = newListener;
		}

		//
		// Properties
		//
		public string EventName {
			get {
				return eventName;
			}

			set {
				eventName = value;
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
