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
			msg = Locale.GetText ("A type load exception has occured.");
		}

		public TypeLoadException (string message)
			: base (message)
		{
			msg = message;
		}

		public TypeLoadException (string message, Exception inner)
			: base (message, inner)
		{
			msg = message;
		}

		public TypeLoadException (SerializationInfo info,
					  StreamingContext context)
			: base (info, context)
		{
		}

		// Properties
		public override string Message
		{
			get { return msg; }
		}

		public string TypeName
		{
			get { return type; }
		}

		// Methods
		//
		// It seems like this object serializes more fields than those described.
		// Those fields are TypeLoadClassName, TypeLoadAssemblyName,
		// TypeLoadMessageArg and TypeLoadResourceID.
		//
		[MonoTODO] 
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info is null.");
			base.GetObjectData (info, context);
		}
	}
}
