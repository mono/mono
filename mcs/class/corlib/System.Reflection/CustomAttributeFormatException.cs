//
// System.Reflection.CustomAttributeFormatException.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc. http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	public class CustomAttributeFormatException : FormatException
	{
		// Constructors
		public CustomAttributeFormatException ()
			: base (Locale.GetText ("The Binary format of the custom attribute is invalid."))
			{
			}
		public CustomAttributeFormatException (string message)
			: base (message)
			{
			}

		public CustomAttributeFormatException (string message, Exception inner)
			: base (message, inner)
			{
			}

		protected CustomAttributeFormatException (SerializationInfo info,
						       StreamingContext context)
			{
			}
	}       
}

