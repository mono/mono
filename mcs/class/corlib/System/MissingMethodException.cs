//
// System.MissingMethodException.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class MissingMethodException : MissingMemberException
	{
		public MissingMethodException ()
			: base (Locale.GetText ("Cannot find the requested method."))
		{
		}

		public MissingMethodException (string message)
			: base (message)
		{
		}

		protected MissingMethodException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public MissingMethodException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public MissingMethodException (string className, string methodName)
			: base (className, methodName)
		{
		}

		public override string Message {
			get {
				if (ClassName == null)
					return base.Message;
				else
					return "Method " + ClassName + "." + MemberName + " not found.";
			}
		}
	}
}
