//
// System.MissingMemberException.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System {

	[Serializable]
	public class MissingMemberException : MemberAccessException {

		// Fields
		protected string ClassName;
		protected string MemberName;
		protected byte[] Signature;
		protected string msg;		   

		// Constructors
		public MissingMemberException ()
			: base (Locale.GetText ("A missing member exception has occurred."))
		{
			msg = Locale.GetText ("A missing member exception has occured.");
		}

		public MissingMemberException (string message)
			: base (message)
		{
			msg = message;
		}

		protected MissingMemberException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			ClassName = info.GetString ("MMClassName");
			MemberName = info.GetString ("MMMemberName");
			Signature = (byte[]) info.GetValue ("MMSignature", Signature.GetType ());
		}
		
		public MissingMemberException (string message, Exception inner)
			: base (message, inner)
		{
			msg = message;
		}

		public MissingMemberException (string className, string memberName)
		{
			ClassName = className;
			MemberName = memberName;
		}

		// Properties
		public override string Message {
			   get { return msg; }
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("MMClassName", ClassName);
			info.AddValue ("MMMemberName", MemberName);
			info.AddValue ("MMSignature", Signature);
		}
	}
}
