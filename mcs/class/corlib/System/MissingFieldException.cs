//	
// System.MissingFieldException.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class MissingFieldException : MissingMemberException
	{
		const int Result = unchecked ((int)0x80131511);

		// Constructors
		public MissingFieldException ()
			: base (Locale.GetText ("Cannot find requested field."))
		{
			HResult = Result;
		}

		public MissingFieldException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected MissingFieldException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public MissingFieldException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		public MissingFieldException (string className, string fieldName)
			: base (className, fieldName)
		{
			HResult = Result;
		}

		public override string Message {
			get {
				if (ClassName == null)
					return base.Message;

				String msg = Locale.GetText ("Field {0}.{1} not found.");
				return String.Format (msg, ClassName, MemberName);
			}
		}
	}
}
