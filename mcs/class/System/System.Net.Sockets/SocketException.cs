//
// System.Net.Sockets.NetworkStream.cs
//
// Author:
//   Andrew Sutton
//
// (C) Andrew Sutton
//

namespace System.Net.Sockets
{
	// <remarks>
	//    A socket exception. Does this REALLY need to
	//    be derived from Win32Exception? It seems a
	//    little lame.
	//
	//    This needs some work...
	// </remarks>
	public class SocketException : Win32Exception
	{
		protected int		error_code;
		protected int		native_code;
		protected string	help_link;
		protected string	message;

		public SocketException ()
		{
			error_code = 0;
		}

		public SocketException (int error)
		{
			error_code = error;
		}

		public SocketException (SerializationInfo info, StreamingContext cxt)
		{
		}

		public override int ErrorCode
		{
			get { return error_code; }
		}

		public override string HelpLink
		{
			get { return help_link; }
			set { help_link = value; }
		}

		public override Exception InnerException
		{
			get { return inner_exception };
		}

		public override string Message
		{
			get { return message };
		}

		public override int NativeErrorCode
		{
			get { return native_code; }
		}

		public override string Source
		{
			get { return source; }
		}

		public override string StackTrace
		{
			get { return stack_trace; }
		}

		public override MethodBase TargetSite
		{
			get { return target_site; }
		}

		protected override int HResult
		{
			get { return hresult; }
			set { hresult = value; }
		}

		public override int GetHashCode()
		{
			return native_code;
		}

		public override void GetObjectData( SerializationInfo info, StreamingContext cxt )
		{
			// TODO: fill this in
		}

		public override string ToString()
		{
			return error_code + " " + help_link;
		}
	}
}