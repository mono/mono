using System;
using System.Runtime.Serialization;

namespace Microsoft.Build.Exceptions
{
	public class BuildAbortedException : Exception
	{
		public BuildAbortedException ()
			: this ("Build aborted")
		{
		}
		
		public BuildAbortedException (string message)
			: base (message)
		{
		}
		
		public BuildAbortedException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
		protected BuildAbortedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			ErrorCode = info.GetString ("errorCode");
		}
		
		internal BuildAbortedException (string message, string errorCode)
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

