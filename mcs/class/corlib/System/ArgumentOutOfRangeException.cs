//
// System.ArgumentOutOfRangeException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ArgumentOutOfRangeException : ArgumentException
	{
		private object actual_value;

		// Constructors
		public ArgumentOutOfRangeException ()
			: base (Locale.GetText ("Argument is out of range."))
		{
		}

		public ArgumentOutOfRangeException (string paramName)
			: base (Locale.GetText ("Argument is out of range."), paramName)
		{
		}

		public ArgumentOutOfRangeException (string paramName, string message)
			: base (message, paramName)
		{
		}

		public ArgumentOutOfRangeException (string paramName, object actualValue, string message)
			: base (message, paramName)
		{
			this.actual_value = actualValue;
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
