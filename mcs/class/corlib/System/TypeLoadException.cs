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

                private string ClassName;
                private string AssemblyName;
                private string MessageArg;
                private string ResourceID;
                
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

		protected TypeLoadException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			if (info == null)
				throw new ArgumentNullException ("info is null.");

                        ClassName = info.GetString ("TypeLoadClassName");
                        AssemblyName = info.GetString ("TypeLoadAssemblyName");
                        MessageArg = info.GetString ("MessageArg");
                        ResourceID = info.GetString ("ResourceID");
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
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info is null.");

			base.GetObjectData (info, context);
                        info.AddValue ("TypeLoadClassName", ClassName, typeof (string)); 
                        info.AddValue ("TypeLoadAssemblyName", AssemblyName, typeof (string));
                        info.AddValue ("TypeLoadMessageArg", MessageArg, typeof (string));
                        info.AddValue ("TypeLoadResourceID", ResourceID);
		}
	}
}
