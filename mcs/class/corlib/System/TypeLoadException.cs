//
// System.TypeLoadException.cs
//
// Authors:
//   Sean MacIsaac (macisaac@ximian.com)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class TypeLoadException : SystemException
	{
		const int Result = unchecked ((int)0x80131522);
		
		// Fields
		private string msg;
		private string type;

		// Constructors
		public TypeLoadException ()
			: base (Locale.GetText ("A type load exception has occurred."))
		{
			HResult = Result;
		}

		public TypeLoadException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public TypeLoadException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		protected TypeLoadException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			type = info.GetString ("TypeLoadClassName");
		}

		// Properties
		public override string Message {
			get {
				if (type == null)
					return base.Message;

				if (msg == null)
					msg = "Cannot load type '" + type + "'";

				return msg;
			}
		}

		public string TypeName {
			get { 
				if (type == null)
					return "";

				return type;
			}
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			base.GetObjectData (info, context);
			info.AddValue ("TypeLoadClassName", type, typeof (string)); 
			info.AddValue ("TypeLoadAssemblyName", "", typeof (string)); 
			info.AddValue ("TypeLoadMessageArg", "", typeof (string)); 
			info.AddValue ("TypeLoadResourceID", 0, typeof (int)); 
		}
	}
}
