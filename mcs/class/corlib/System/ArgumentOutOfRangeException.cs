//
// System.ArgumentOutOfRangeException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System {

	[Serializable]
	public class ArgumentOutOfRangeException : ArgumentException {
		private object actual_value;

		// Constructors
		public ArgumentOutOfRangeException ()
			: base (Locale.GetText ("Argument is out of range"))
		{
		}

		public ArgumentOutOfRangeException (string param_name)
			: base (Locale.GetText ("Argument is out of range"), param_name)
		{
		}

		public ArgumentOutOfRangeException (string param_name, string message)
			: base (message, param_name)
		{
		}

		public ArgumentOutOfRangeException (string param_name, object actual_value, string message)
			: base (message, param_name)
		{
			this.actual_value = actual_value;
		}

		protected ArgumentOutOfRangeException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			actual_value = info.GetString ("ActualValue");
		}
 
		// Properties
		public virtual object ActualValue {
			get {
				return actual_value;
			}
		}

		public override string Message {
			get {
				string basemsg = base.Message;
				if (actual_value == null)
					return basemsg;
				return basemsg + '\n' + actual_value;
			}
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("ActualValue", actual_value);
		}
		
	}
}
