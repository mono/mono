//
// System.ArgumentException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//
using System.Runtime.Serialization;
using System.Globalization;

namespace System {

	[Serializable]
	public class ArgumentException : SystemException {
		private string param_name;

		// Constructors
		public ArgumentException ()
			: base (Locale.GetText ("An invalid argument was specified."))
		{
		}

		public ArgumentException (string message)
			: base (message)
		{
		}

		public ArgumentException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public ArgumentException (string message, string param_name)
			: base (message)
		{
			this.param_name = param_name;
		}

		public ArgumentException (string message, string param_name, Exception inner)
			: base (message, inner)
		{
			this.param_name = param_name;
		}

		protected ArgumentException (SerializationInfo info, StreamingContext sc)
			: base (info, sc)
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
					return base_message + "\nParameter name: " + ParamName;
			}
		}
		
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("ParamName", param_name);
		}
	}
}
