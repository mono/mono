//
// System.ObsoleteAttribute.cs
//
// Author:
//   Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Enum | AttributeTargets.Constructor |
		AttributeTargets.Method | AttributeTargets.Property |
		AttributeTargets.Field | AttributeTargets.Event |
		AttributeTargets.Interface | AttributeTargets.Delegate)]
	[Serializable]
	public sealed class ObsoleteAttribute : Attribute
	{
		private string message;
		private bool isError = false;

		//	 Constructors
		public ObsoleteAttribute ()
			: base ()
		{
		}

		public ObsoleteAttribute (string message)
		{
			this.message = message;
		}

		public ObsoleteAttribute (string message, bool error)
		{
			this.message = message;
			this.isError = error;
		}

		// Properties
		public string Message {
			get { return message; }
		}

		public bool IsError {
			get { return isError; }
		}
	}
}
