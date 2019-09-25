using System.Net.Sockets;
using System.Threading;

namespace System.Net.Internals
{
	public static class MartinTest
	{
		public static void Run ()
		{
			var sa = new SocketAddress (IPAddress.Loopback, 80);
			Console.Error.WriteLine ($"SA: {sa} {sa.Family} {sa.GetIPEndPoint ()}");

			SafeCloseSocket handle;
			SocketError errorCode = SocketPal.CreateSocket(sa.Family, SocketType.Stream, ProtocolType.Tcp, out handle);

			Console.Error.WriteLine ($"SOCKET: {handle} {handle.IsNonBlocking} {errorCode}");

			handle.IsNonBlocking = true;

			errorCode = handle.AsyncContext.ConnectAsync (sa.Buffer, sa.Size, CompletionCallback);

//			errorCode = SocketPal.Connect(handle, sa.Buffer, sa.Size);
			Console.Error.WriteLine ($"CONNECT DONE: {errorCode}");

			Thread.Sleep (TimeSpan.FromSeconds (3));

			Console.Error.WriteLine ($"DONE SLEEPING!");

			handle.Dispose ();

			Console.Error.WriteLine ($"DONE!");

			void CompletionCallback (SocketError result)
			{
				Console.Error.WriteLine ($"COMPLETION CALLBACK: {result}");
			}
		}
	}
}
