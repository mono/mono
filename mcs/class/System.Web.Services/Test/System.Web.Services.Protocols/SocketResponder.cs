using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public delegate string SocketRequestHandler ();

public class SocketResponder : IDisposable
{
	private TcpListener tcpListener;
	private readonly IPEndPoint _localEndPoint;
	private Thread listenThread;
	private SocketRequestHandler _requestHandler;

	public SocketResponder (IPEndPoint localEP, SocketRequestHandler requestHandler)
	{
		_localEndPoint = localEP;
		_requestHandler = requestHandler;
	}

	public IPEndPoint LocalEndPoint
	{
		get { return _localEndPoint; }
	}

	public void Dispose ()
	{
		Stop ();
	}

	public void Start ()
	{
		tcpListener = new TcpListener (LocalEndPoint);
		tcpListener.Start ();
		listenThread = new Thread (new ThreadStart (Listen));
		listenThread.Start ();
	}

	public void Stop ()
	{
		if (tcpListener != null)
			tcpListener.Stop ();

		try {
			if (listenThread != null && listenThread.ThreadState == ThreadState.Running) {
				listenThread.Abort ();
			}
		} catch {
		}
	}

	private void Listen ()
	{
		Socket socket = tcpListener.AcceptSocket ();

		string content = _requestHandler ();

		MemoryStream outputStream = new MemoryStream ();
		StreamWriter sw = new StreamWriter (outputStream, Encoding.UTF8);
		sw.WriteLine ("HTTP/1.1 200 OK");
		sw.WriteLine ("Content-Type: text/xml");
		sw.WriteLine ("Content-Length: " + content.Length.ToString (CultureInfo.InvariantCulture));
		sw.WriteLine ();
		sw.Write (content);
		sw.Flush ();

		outputStream.Position = 0;

		using (StreamReader sr = new StreamReader (outputStream)) {
			byte [] buffer = Encoding.UTF8.GetBytes (sr.ReadToEnd ());
			socket.Send (buffer);
		}
	}
}
