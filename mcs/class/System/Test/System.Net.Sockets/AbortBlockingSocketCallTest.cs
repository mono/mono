using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using System.IO;

using MonoTests.Helpers;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class AbortBlockingSocketCallTest
	{

		void StartBlockingAcceptCall ()
		{
			TcpListener listener = null;
			try
			{
				listener = NetworkHelpers.CreateAndStartTcpListener (out int port);
				Socket socket = listener.AcceptSocket ();
				socket.Close ();
			}
			finally
			{
				if (listener != null)
					listener.Stop ();
			}
		}

		[Test]
		public void AbortBlockingAcceptCall ()
		{
			Thread listenerThread = new Thread (StartBlockingAcceptCall);
			listenerThread.Start ();
			Thread.Sleep (2000);

			listenerThread.Abort ();
			listenerThread.Join ();
		}

	}
}

