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
		const int Result = unchecked ((int)0x80131513);

		public MissingMethodException ()
			: base (Locale.GetText ("Cannot find the requested method."))
		{
			HResult = Result;
		}

		public MissingMethodException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected MissingMethodException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public MissingMethodException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		public MissingMethodException (string className, string methodName)
			: base (className, methodName)
		{
			HResult = Result;
		}

		public override string Message {
			get {
				if (ClassName == null)
					return base.Message;

				String msg = Locale.GetText ("Method {0}.{1} not found.");
				return String.Format (msg, ClassName, MemberName);
			}
		}
	}
}
