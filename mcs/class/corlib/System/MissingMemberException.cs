//
// System.MissingMemberException.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class MissingMemberException : MemberAccessException
	{
		const int Result = unchecked ((int)0x80131512);

		// Fields
		protected string ClassName;
		protected string MemberName;
		protected byte[] Signature;

		public MissingMemberException ()
			: base (Locale.GetText ("Cannot find the requested class member."))
		{
			HResult = Result;
		}

		public MissingMemberException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public MissingMemberException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		protected MissingMemberException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			ClassName = info.GetString ("MMClassName");
			MemberName = info.GetString ("MMMemberName");
			Signature = (byte[]) info.GetValue ("MMSignature", Signature.GetType ());
		}

		public MissingMemberException (string className, string memberName)
		{
			ClassName = className;
			MemberName = memberName;
			HResult = Result;
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("MMClassName", ClassName);
			info.AddValue ("MMMemberName", MemberName);
			info.AddValue ("MMSignature", Signature);
		}

		public override string Message {
			get {
				if (ClassName == null)
					return base.Message;

				String msg = Locale.GetText ("Member {0}.{1} not found.");
				return String.Format (msg, ClassName, MemberName);
			}
		}
	}
}
