using System;
using System.Runtime.Serialization;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Exceptions
{
	public class InternalLoggerException : Exception
	{
		public InternalLoggerException ()
			: this ("Build aborted")
		{
		}
		
		public InternalLoggerException (string message)
			: base (message)
		{
		}
		
		public InternalLoggerException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
		
		internal InternalLoggerException (string message, Exception innerException, BuildEventArgs buildEventArgs, string errorCode, string helpKeyword, bool initializationException)
			: base (message, innerException)
		{
			BuildEventArgs = buildEventArgs;
			ErrorCode = errorCode;
			HelpKeyword = helpKeyword;
			InitializationException = initializationException;
		}
		
		internal InternalLoggerException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			BuildEventArgs = (BuildEventArgs) info.GetValue ("buildEventArgs", typeof (BuildEventArgs));
			ErrorCode = info.GetString ("errorCode");
			HelpKeyword = info.GetString ("helpKeyword");
			InitializationException = info.GetBoolean ("initializationException");
		}
		
		public BuildEventArgs BuildEventArgs { get; private set; }
		public string ErrorCode { get; private set; }
		public string HelpKeyword { get; private set; }
		public bool InitializationException { get; private set; }
		
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("buildEventArgs", BuildEventArgs);
			info.AddValue ("errorCode", ErrorCode);
			info.AddValue ("helpKeyword", HelpKeyword);
			info.AddValue ("initializationException", InitializationException);
		}
	}
}

