//	
// System.MissingFieldException.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class MissingFieldException : MissingMemberException
	{
		// Constructors
		public MissingFieldException ()
			: base (Locale.GetText ("Cannot find requested field."))
		{
		}

		public MissingFieldException (string message)
			: base (message)
		{
		}

		protected MissingFieldException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public MissingFieldException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public MissingFieldException (string className, string fieldName)
			: base (className, fieldName)
		{
		}

		public override string Message {
			get {
				if (ClassName == null)
					return base.Message;

				return "Field " + ClassName + "." + MemberName + " not found.";
			}
		}
	}
}
