//
// System.Net.ProtocolViolationException.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public class ProtocolViolationException : InvalidOperationException, ISerializable
	{

		// Constructors
		public ProtocolViolationException () : base ()
		{
		}
		
		public ProtocolViolationException (string message) : base (message)
		{
		}

		protected ProtocolViolationException (SerializationInfo info, StreamingContext context)
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
	
