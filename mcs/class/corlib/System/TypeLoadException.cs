//
// System.TypeLoadException
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System {

	[Serializable]
	public class TypeLoadException : SystemException {

		// Fields
		private string msg;
		private string type;

                // Constructors
		public TypeLoadException ()
			: base (Locale.GetText ("A type load exception has occurred."))
		{
		}

		public TypeLoadException (string message)
			: base (message)
		{
		}

		public TypeLoadException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected TypeLoadException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			if (info == null)
				throw new ArgumentNullException ("info is null.");

			type = info.GetString ("type");
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
				throw new ArgumentNullException ("info is null.");

			base.GetObjectData (info, context);
			info.AddValue ("type", type, typeof (string)); 
		}
	}
}
