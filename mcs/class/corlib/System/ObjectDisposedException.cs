//
// System.ObjectDisposedException.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ObjectDisposedException : InvalidOperationException
	{
		// Does not override the HResult from InvalidOperationException

		private string obj_name;
		private string msg;

		// Constructors
		public ObjectDisposedException (string objectName)
			: base (Locale.GetText ("The object was used after being disposed."))
		{
			obj_name = objectName;
			msg = Locale.GetText ("The object was used after being disposed.");
		}

		public ObjectDisposedException (string objectName, string message) 
			: base (message)
		{
			obj_name = objectName;
			msg = message;
		}

		protected ObjectDisposedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			obj_name = info.GetString ("ObjectName");
		}

		// Properties
		public override string Message {
			get { return msg; }
		}

		public string ObjectName {
			get { return obj_name; }
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("ObjectName", obj_name);
		}
	}
}
