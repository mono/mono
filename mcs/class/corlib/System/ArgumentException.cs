//
// System.ArgumentException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ArgumentException : SystemException
	{
		const int Result = unchecked ((int)0x80070057);

		private string param_name;

		// Constructors
		public ArgumentException ()
			: base (Locale.GetText ("An invalid argument was specified."))
		{
			HResult = Result;
		}

		public ArgumentException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public ArgumentException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		public ArgumentException (string message, string paramName)
			: base (message)
		{
			this.param_name = paramName;
			HResult = Result;
		}

		public ArgumentException (string message, string paramName, Exception innerException)
			: base (message, innerException)
		{
			this.param_name = paramName;
			HResult = Result;
		}

		protected ArgumentException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			param_name = info.GetString ("ParamName");
		}

		// Properties
		public virtual string ParamName {
			get {
				return param_name;
			}
		}

		public override string Message {
			get {
				string base_message = base.Message;
				if (base_message == null)
					base_message = Locale.GetText ("An invalid argument was specified.");

				if (param_name == null)
					return base_message;
				else
					return base_message + Environment.NewLine + Locale.GetText ("Parameter name: ") + param_name;
			}
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("ParamName", param_name);
		}
	}
}
