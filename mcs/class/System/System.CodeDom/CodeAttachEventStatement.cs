//
// System.CodeDom CodeAttachEventStatement Class implementation
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
	public class CodeAttachEventStatement
		: CodeStatement 
	{
		private CodeEventReferenceExpression eventRef;
		private CodeExpression listener;
		
		//
		// Constructors
		//
		public CodeAttachEventStatement ()
		{
		}

		public CodeAttachEventStatement (CodeEventReferenceExpression eventRef,
						 CodeExpression listener)
		{
			this.eventRef = eventRef;
			this.listener = listener;
		}

		public CodeAttachEventStatement (CodeExpression targetObject,
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
