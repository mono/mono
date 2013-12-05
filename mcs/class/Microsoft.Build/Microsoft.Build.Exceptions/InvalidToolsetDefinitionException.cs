using System;
using System.Runtime.Serialization;

namespace Microsoft.Build.Exceptions
{
	public class InvalidToolsetDefinitionException : Exception
	{
		public InvalidToolsetDefinitionException ()
			: this ("Invalid toolset definition")
		{
		}
		
		public InvalidToolsetDefinitionException (string message)
			: base (message)
		{
		}
		
		public InvalidToolsetDefinitionException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
		protected InvalidToolsetDefinitionException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			ErrorCode = info.GetString ("errorCode");
		}
		
		internal InvalidToolsetDefinitionException (string message, string errorCode)
			: base (message + " error code: " + errorCode)
		{
			ErrorCode = errorCode;
		}
		
		public string ErrorCode { get; private set; }
		
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("errorCode", ErrorCode);
		}
	}
}
