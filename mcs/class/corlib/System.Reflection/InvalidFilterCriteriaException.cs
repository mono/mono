//
// System.Reflection.InvalidFilterCriteriaException.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	public class InvalidFilterCriteriaException : ApplicationException
	{
		public InvalidFilterCriteriaException ()
			: base (Locale.GetText ("Filter Criteria is not valid."))
			{
			}
		public InvalidFilterCriteriaException (string message)
			: base (message)
		{
		}

		public InvalidFilterCriteriaException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public InvalidFilterCriteriaException (SerializationInfo info,
						       StreamingContext context)
			: base (info, context)
		{
		}
	}
}
