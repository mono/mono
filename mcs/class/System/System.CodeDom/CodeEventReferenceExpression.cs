//
// System.CodeDom CodeEventReferenceExpression class implementation
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
	public class CodeEventReferenceExpression
		: CodeExpression
	{
		private string eventName;
		private CodeExpression targetObject;

		//
		// Constructors
		//
		public CodeEventReferenceExpression()
		{
		}

		public CodeEventReferenceExpression( CodeExpression targetObject, string eventName )
		{
			this.targetObject = targetObject;
			this.eventName = eventName;
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
