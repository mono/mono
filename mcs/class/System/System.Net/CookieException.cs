//
// System.Net.CookieException.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public class CookieException : FormatException, ISerializable
	{

		// Constructors
		public CookieException () : base ()
		{
		}
		
		internal CookieException (string msg) : base (msg) 
		{
		}
		
		internal CookieException (string msg, Exception e) : base (msg, e)
		{
		}

		protected CookieException (SerializationInfo info, StreamingContext context)
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
	
