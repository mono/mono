//
// System.MissingMemberException.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class MissingMemberException : MemberAccessException
	{
		// Fields
		protected string ClassName;
		protected string MemberName;
		protected byte[] Signature;

		public MissingMemberException ()
			: base (Locale.GetText ("Cannot find the requested class member."))
		{
		}

		public MissingMemberException (string message)
			: base (message)
		{
		}

		public MissingMemberException (string message, Exception inner)
			: base (message, inner)
		{
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
				else
					return "Member " + ClassName + "." + MemberName + " not found.";
			}
		}
	}
}
