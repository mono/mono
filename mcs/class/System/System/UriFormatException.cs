//
// System.UriFormatException.cs
//
// Author:
//   Scott Sanders (scott@stonecobra.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Scott Sanders
// (C) 2002 Ximian, Inc.
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System {
	[Serializable]
	[MonoTODO] // Doc says this class doesn't implement ISerializable
	public class UriFormatException : FormatException, ISerializable
	{

		// Constructors
		public UriFormatException ()
			: base (Locale.GetText ("Invalid URI format"))
		{
		}

		public UriFormatException (string message)
			: base (message)
		{
		}

		protected UriFormatException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{			
		}

		// Methods
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}
	}
}
	
