using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace System.IO
{
	public class PipeException : IOException
	{
		public PipeException ()
		{
		}

		public PipeException (string message): base (message)
		{
		}

		protected PipeException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public PipeException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public PipeException (string message, int errorCode)
			: base (message, errorCode)
		{
		}

		public virtual int ErrorCode {
			get {
				// we re-use the HResult for the error code here.
				return HResult;
			}
		}
	}
}

