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

		public TypeLoadException (SerializationInfo info,
					  StreamingContext context)
			: base (info, context)
		{
		}

		// Methods
		[MonoTODO]
		public override void GetObjectData (SerializationInfo info,
						    StreamingContext context)
		{
		}

		// Properties
		[MonoTODO]
		public override string Message
		{
			get { return null; }
		}

		[MonoTODO]
		public string TypeName
		{
			get { return null; }
		}
	}
}
