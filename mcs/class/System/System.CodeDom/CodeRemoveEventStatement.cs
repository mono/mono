//
// System.CodeDom CodeRemoveEventStatement Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom 
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeRemoveEventStatement
		: CodeStatement 
	{
		private CodeEventReferenceExpression eventRef;
		private CodeExpression listener;

		//
		// Constructors
		//
		public CodeRemoveEventStatement ()
		{
		}

		public CodeRemoveEventStatement (CodeEventReferenceExpression eventRef,
						 CodeExpression listener)
		{
			this.eventRef = eventRef;
			this.listener = listener;
		}

		public CodeRemoveEventStatement (CodeExpression targetObject,
						 string eventName,
						 CodeExpression listener)
		{
			this.eventRef = new CodeEventReferenceExpression( targetObject,
									  eventName );
			this.listener = listener;
		}

		//
		// Properties
		//
		public CodeEventReferenceExpression Event {
			get {
				return eventRef;
			}
			set {
				eventRef = value;
			}
		}

		public CodeExpression Listener {
			get {
				return listener; 
			}
			set {
				listener = value;
			}
		}
	}
}

