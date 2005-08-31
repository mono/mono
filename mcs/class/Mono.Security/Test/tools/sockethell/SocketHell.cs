using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

using Mono.Security.Authenticode;
using Mono.Security.Protocol.Tls;
using NUnit.Framework;

namespace Coversant.SoapBox.Base.Test
{
	[TestFixture]
	public class SocketHell
	{
		//this is used for shutting down sockets, so mono doesn't race out of control
		//see http://bugzilla.ximian.com/show_bug.cgi?id=75826
		//it doesn't always work with higher loads, but usually 
		//fixes this issue with a small number of sockets like we use here
		public static object GlobalSocketLock = new object();
		public static int GlobalSocketLockWait = 500;

		//Wrap BeginRead or BeginWrite calls to either Ssl Stream with a BeginInvoke?
		public static bool FakeAsyncReadWithDelegate = true;
		public static bool FakeAsyncWriteWithDelegate = false;

		private string CertFile = "socketpong.cer";
		private string PvkFile = "socketpong.pvk";
		private string PvkPassword = "";
		private int SendIterations = 100;
		private int BytesPerIteration = 8192;
		private int ReadBufferSize = 8192;
		private int TotalClients = 20;
		private int TestTimeoutMs = 60000;
		private bool ClientInitiatedClose = false;
		private ManualResetEvent _testComplete;
		private int _clientsComplete;
		private Exception _firstAsyncException;

		[Test]
		public void NetworkStreamPong_10()
		{
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 30000;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(false, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void ClientCloseNetworkStreamPong_10()
		{
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 30000;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = true;
			SocketPong(false, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void NetworkStreamPong_100()
		{
			TotalClients = 100;
			SendIterations = 100;
			TestTimeoutMs = 300000;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(false, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void NetworkStreamPong_100_1000()
		{
			TotalClients = 100;
			SendIterations = 1000;
			TestTimeoutMs = 0;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(false, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void TlsStreamPong_10()
		{
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 60000;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(true, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void ClientCloseTlsStreamPong_10()
		{
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 60000;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = true;
			SocketPong(true, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void TlsStreamPong_100()
		{
			TotalClients = 100;
			SendIterations = 100;
			TestTimeoutMs = 0;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(true, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void TlsStreamPong_100_1000()
		{
			TotalClients = 100;
			SendIterations = 1000;
			TestTimeoutMs = 0;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(true, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void FakeAsyncReadTlsStreamPong_10()
		{
			
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 60000;
			FakeAsyncReadWithDelegate = true;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(true, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void ClientCloseFakeAsyncReadTlsStreamPong_10()
		{

			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 60000;
			FakeAsyncReadWithDelegate = true;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = true;
			SocketPong(true, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void FakeAsyncReadTlsStreamPong_10_1000()
		{

			TotalClients = 10;
			SendIterations = 1000;
			TestTimeoutMs = 0;
			FakeAsyncReadWithDelegate = true;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(true, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void FakeAsyncReadWriteTlsStreamPong_10()
		{
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 60000;
			FakeAsyncReadWithDelegate = true;
			FakeAsyncWriteWithDelegate = true;
			ClientInitiatedClose = false;
			SocketPong(true, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void FakeAsyncWriteTlsStreamPong_10()
		{
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 60000;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = true;
			ClientInitiatedClose = false;
			SocketPong(true, BytesPerIteration, ReadBufferSize);
		}

		[Test]
		public void SmallerBufferNetworkStreamPong_10()
		{
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 30000;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(false, BytesPerIteration, Convert.ToInt32(BytesPerIteration / 2));
		}

		[Test]
		public void SmallerBufferTlsStreamPong_10()
		{
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 60000;
			FakeAsyncReadWithDelegate = false;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(true, BytesPerIteration, Convert.ToInt32(BytesPerIteration / 2));
		}

		[Test]
		public void SmallerBufferFakeAsyncReadTlsStreamPong_10()
		{
			TotalClients = 10;
			SendIterations = 100;
			TestTimeoutMs = 60000;
			FakeAsyncReadWithDelegate = true;
			FakeAsyncWriteWithDelegate = false;
			ClientInitiatedClose = false;
			SocketPong(true, BytesPerIteration, Convert.ToInt32(BytesPerIteration / 2));
		}

		private void SocketPong(bool useTls, int bytesPerIteration, int readBufferSize)
		{
			_clientsComplete = 0;
			_firstAsyncException = null;
			_testComplete = new ManualResetEvent(false);

			ArrayList clients = new ArrayList();
			SocketPongServer server = new SocketPongServer(useTls, readBufferSize, CertFile, PvkFile, PvkPassword);
			server.ExceptionOccurred += new AsyncTestClassBase.ExceptionOccurredEventHandler(this.ExceptionCallback);

			server.Start();

			try
			{
				for (int i = 0; i < TotalClients; i++)
				{
					if (_testComplete.WaitOne(0, false))
						break;

					SocketPingClient client = new SocketPingClient(useTls, server.LocalEndPoint, SendIterations, bytesPerIteration, readBufferSize);
					client.ExceptionOccurred += new AsyncTestClassBase.ExceptionOccurredEventHandler(this.ExceptionCallback);
					client.TestComplete += new SocketPingClient.TestCompleteEventHandler(this.TestCompleteCallback);
					client.Start();
				}

				if (TestTimeoutMs <= 0)
					_testComplete.WaitOne();
				else
				{
					if (!_testComplete.WaitOne(TestTimeoutMs, false))
						Assert.Fail("Tests timed out");
				}

				if (null != _firstAsyncException)
					Assert.Fail(_firstAsyncException.ToString());
			}
			finally
			{
				if (ClientInitiatedClose)
					CloseClients(clients);

				server.ExceptionOccurred -= new AsyncTestClassBase.ExceptionOccurredEventHandler(this.ExceptionCallback);
				server.Stop();

				if (!ClientInitiatedClose)
					CloseClients(clients);
				
			}
		}

		private void CloseClients(ArrayList clients)
		{
			foreach (SocketPingClient client in clients)
			{
				client.ExceptionOccurred -= new AsyncTestClassBase.ExceptionOccurredEventHandler(this.ExceptionCallback);
				client.TestComplete -= new SocketPingClient.TestCompleteEventHandler(this.TestCompleteCallback);
				client.Stop();
			}
		}

		private void ExceptionCallback(Exception ex)
		{
			if (_testComplete.WaitOne(0, false))
				return;

			lock (this)
			{
				if (null == _firstAsyncException)
				{
					_firstAsyncException = ex;
					_testComplete.Set();
				}
			}
		}

		private void TestCompleteCallback(object sender)
		{
			if (_testComplete.WaitOne(0, false))
				return;

			lock (this)
			{
				_clientsComplete++;

				if (_clientsComplete == TotalClients)
					_testComplete.Set();
			}
		}
	}

	public class SocketPingClient : TlsEnabledSocketBase
	{
		int _bytesPerIteration;
		int _iterations;
		int _currentIteration;
		IPEndPoint _server;
		int _totalBytesRead;
		int _expectedBytes;
		Random _byteGenerator;

		public delegate void TestCompleteEventHandler(object sender);
		public event TestCompleteEventHandler TestComplete;

		public SocketPingClient(bool useTls, IPEndPoint server, int iterations, int bytesPerIteration, int readBufferSize):
			base(useTls, readBufferSize)
		{
			_server = server;
			_iterations = iterations;
			_bytesPerIteration = bytesPerIteration;
			_byteGenerator = new Random();
			_expectedBytes = (_iterations * _bytesPerIteration);
			_currentIteration = 0;
		}

		protected override void OnStart()
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(_server);

			this.InitializeNetworkStream(socket);

			if (this.UseTls)
			{
				SslClientStream secureStream = new SslClientStream(base.NetworkStream, "localhost", true, Mono.Security.Protocol.Tls.SecurityProtocolType.Tls);
				secureStream.ServerCertValidationDelegate = new CertificateValidationCallback(CertValidationCallback);
				base.SecureStream = secureStream;
			}

			SendMoreData();

			byte[] readBuffer = new byte[ReadBufferSize];
			this.BeginRead(readBuffer, 0, readBuffer.Length, new AsyncCallback(BeginReadCallback), readBuffer);
		}

		private void SendMoreData()
		{
			_currentIteration++;

			if (_currentIteration > _iterations)
				return;

			byte[] buff = new byte[_bytesPerIteration];
			_byteGenerator.NextBytes(buff);

//			WTrace.TraceInfo("Socket Hell", this.GetType(), "Writing iteration {0} of {1}.", _currentIteration, _iterations);

			this.BeginWrite(buff, 0, buff.Length, new AsyncCallback(BeginWriteCallback), null);
		}

		private void BeginWriteCallback(IAsyncResult asyncResult)
		{
			if (_stop)
				return;

			try
			{
				this.EndWrite(asyncResult);
				SendMoreData();
			}
			catch (Exception ex)
			{
				OnExceptionOccurred(ex);
			}
		}

		private void BeginReadCallback(IAsyncResult asyncResult)
		{
			if (_stop)
				return;

			try
			{
				int bytesRead = this.EndRead(asyncResult);

				bool done = false;
				_totalBytesRead += bytesRead;

				done = (_totalBytesRead == _expectedBytes);

//				WTrace.TraceVerbose("Socket Hell", this.GetType(), "Read {0} of {1} bytes. Done? {2}", _totalBytesRead, _expectedBytes, done);

				if (done)
				{
					OnTestComplete(this);
				}
				else
				{
					byte[] readBuffer = (byte[])asyncResult.AsyncState;
					this.BeginRead(readBuffer, 0, readBuffer.Length, new AsyncCallback(BeginReadCallback), readBuffer);
				}
			}
			catch (Exception ex)
			{
				OnExceptionOccurred(ex);
			}
		}

		protected void OnTestComplete(object sender)
		{
			try
			{
				if (null != TestComplete)
					TestComplete(sender);
			}
			catch (Exception ex) 
			{
				OnExceptionOccurred(ex);
			}
		}

		private bool CertValidationCallback(X509Certificate certificate, int[] certificateErrors)
		{
			return true;
		}

	}

	public class SocketPongClient: TlsEnabledSocketBase
	{
		private byte[] _readBuffer;

		private AsymmetricAlgorithm _privateKey;

		public SocketPongClient(Socket socket, bool useTls, int readBufferSize, string certFile, string pvkFile, string pvkPassword):
			base(useTls, readBufferSize, socket)
		{
			if (useTls)
			{
				_privateKey = PrivateKey.CreateFromFile(pvkFile, pvkPassword).RSA;
				
				SslServerStream secureStream = new SslServerStream(
					base.NetworkStream,
					X509Certificate.CreateFromCertFile(certFile),
					false,
					true,
					Mono.Security.Protocol.Tls.SecurityProtocolType.Tls);

				secureStream.PrivateKeyCertSelectionDelegate = new PrivateKeySelectionCallback(PrivateKeyCertSelectionCallback);

				base.SecureStream = secureStream;
			}
		}

		private AsymmetricAlgorithm PrivateKeyCertSelectionCallback(X509Certificate certificate, string targetHost)
		{
			return _privateKey;
		}

		protected override void OnStart()
		{
			_readBuffer = new byte[ReadBufferSize];

			this.BeginRead(_readBuffer, 0, _readBuffer.Length, new AsyncCallback(BeginReadCallback), null);
		}

		private void BeginReadCallback(IAsyncResult asyncResult)
		{
			if (_stop)
				return;

			try
			{
				int bytesRead = this.EndRead(asyncResult);

				byte[] sendBuffer = new byte[_readBuffer.Length];
				_readBuffer.CopyTo(sendBuffer, 0);

				if (_stop)
					return;

//				WTrace.TraceVerbose("Socket Hell", this.GetType(), "Echoing");

				this.BeginWrite(sendBuffer, 0, bytesRead, new AsyncCallback(NullBeginWriteCallback), null);

				if (_stop)
					return;

//				WTrace.TraceVerbose("Socket Hell", this.GetType(), "Reading");

				this.BeginRead(_readBuffer, 0, _readBuffer.Length, new AsyncCallback(BeginReadCallback), null);
			}
			catch (Exception ex)
			{
				OnExceptionOccurred(ex);
			}
		}

	}

	public abstract class TlsEnabledSocketBase : AsyncTestClassBase
	{
		private bool _useTls;
		private NetworkStream _networkStream;
		private Stream _secureStream;
		private int _readBufferSize;

		protected TlsEnabledSocketBase(bool useTls, int readBufferSize)
		{
			_useTls = useTls;
			_readBufferSize = readBufferSize;
		}

		protected TlsEnabledSocketBase(bool useTls, int readBufferSize, Socket connectedSocket)
			: this(useTls, readBufferSize)
		{
			_useTls = useTls;
			InitializeNetworkStream(connectedSocket);
		}

		protected virtual void InitializeNetworkStream(Socket connectedSocket)
		{
			_networkStream = new NetworkStream(connectedSocket, FileAccess.ReadWrite, true);
		}

		protected int ReadBufferSize
		{
			get { return _readBufferSize; }
		}
		
		protected bool UseTls
		{
			get { return _useTls; }
			set { _useTls = value; }
		}

		protected Stream SecureStream
		{
			get { return _secureStream; }
			set { _secureStream = value; }
		}

		protected NetworkStream NetworkStream
		{
			get { return _networkStream; }
			set { _networkStream = value; }
		}

		protected Stream CurrentStream
		{
			get
			{
				if (null != _secureStream)
					return _secureStream;
				else
					return _networkStream;
			}
		}

		private delegate int MonoBeginReadDelegate(byte[] buffer, int offest, int count);
		private MonoBeginReadDelegate _currentReadOperation;

		private delegate void MonoBeginWriteDelegate(byte[] buffer, int offest, int count);
		private MonoBeginWriteDelegate _currentWriteOperation;

		protected IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (UseTls && SocketHell.FakeAsyncReadWithDelegate)
			{

				_currentReadOperation = new MonoBeginReadDelegate(this.CurrentStream.Read);

				return _currentReadOperation.BeginInvoke(buffer, offset, count, callback, state);
			}
			else
			{
				return this.CurrentStream.BeginRead(buffer, offset, count, callback, state);
			}
		}

		protected int EndRead(IAsyncResult asyncResult)
		{
			if (UseTls && SocketHell.FakeAsyncReadWithDelegate)
			{
				int result;
				try
				{
					result = _currentReadOperation.EndInvoke(asyncResult);
				}
				finally
				{
					_currentReadOperation = null;
				}

				return result;
			}
			else
			{
				return this.CurrentStream.EndRead(asyncResult);
			}
		}

		protected IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (UseTls && SocketHell.FakeAsyncWriteWithDelegate)
			{
				_currentWriteOperation = new MonoBeginWriteDelegate(this.CurrentStream.Write);
				return _currentWriteOperation.BeginInvoke(buffer, offset, count, callback, state);
			}
			else
			{
				return this.CurrentStream.BeginWrite(buffer, offset, count, callback, state);
			}
		}

		protected void EndWrite(IAsyncResult asyncResult)
		{
			if (UseTls && SocketHell.FakeAsyncWriteWithDelegate)
			{
				try
				{
					_currentWriteOperation.EndInvoke(asyncResult);
				}
				finally
				{
					_currentWriteOperation = null;
				}
			}
			else
			{
				this.CurrentStream.EndWrite(asyncResult);
			}
		}

		protected override void OnStop()
		{
			lock (SocketHell.GlobalSocketLock)
			{
				//this sleep helps to stop the CPU race, but it isn't perfect
				System.Threading.Thread.Sleep(SocketHell.GlobalSocketLockWait);

				try
				{
					if (null != this.CurrentStream)
						this.CurrentStream.Close();

					_secureStream = null;
					_networkStream = null;
				}
				catch (Exception ex)
				{
					OnExceptionOccurred(ex);
				}
			}
		}

		protected void NullBeginWriteCallback(IAsyncResult asyncResult)
		{
			if (_stop)
				return;

			try
			{
				this.EndWrite(asyncResult);
			}
			catch (Exception ex)
			{
				OnExceptionOccurred(ex);
			}
		}

		protected void NullBeginReadCallback(IAsyncResult asyncResult)
		{
			if (_stop)
				return;

			try
			{
				this.EndRead(asyncResult);
			}
			catch (Exception ex)
			{
				OnExceptionOccurred(ex);
			}
		}
	}

	public class SocketPongServer : AsyncTestClassBase
	{
		private bool _useTls;

		private string _certFile;
		private string _pvkFile;
		private string _pvkPassword;

		private Socket _listener;

		private int _readBufferSize;

		private ArrayList _connectedClients;

		public SocketPongServer(bool useTls, int readBufferSize, string certFile, string pvkFile, string pvkPassword): base()
		{
			_useTls = useTls;
			_readBufferSize = readBufferSize;
			_certFile = certFile;
			_pvkFile = pvkFile;
			_pvkPassword = pvkPassword;

			_connectedClients = new ArrayList();
		}

		protected override void OnStart()
		{
			_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
			_listener.Listen(int.MaxValue);
			_listener.BeginAccept(new AsyncCallback(BeginAcceptCallback), null);
		}

		private void BeginAcceptCallback(IAsyncResult asyncResult)
		{
			if (_stop)
				return;

			try
			{
				Socket accepted = _listener.EndAccept(asyncResult);
				
				if (_stop)
					return;

				SocketPongClient newClient = new SocketPongClient(accepted, _useTls, _readBufferSize, _certFile, _pvkFile, _pvkPassword);
				_connectedClients.Add(newClient);
				newClient.ExceptionOccurred += new ExceptionOccurredEventHandler(OnExceptionOccurred);
				newClient.Start();

				if (_stop)
					return;

				_listener.BeginAccept(new AsyncCallback(BeginAcceptCallback), null);
			}
			catch (Exception ex)
			{
				OnExceptionOccurred(ex);
			}
		}

		protected override void OnStop()
		{
			lock (SocketHell.GlobalSocketLock)
			{
				//this sleep helps to stop the CPU race, but it isn't perfect
				System.Threading.Thread.Sleep(SocketHell.GlobalSocketLockWait);

				if (null != _listener)
					_listener.Close();

				_listener = null;
			}

			foreach (SocketPongClient client in _connectedClients)
			{
				//absorb shutdown exceptions for each client as to not effect the test suite as a whole
				try
				{
					client.Stop();
					client.ExceptionOccurred -= new ExceptionOccurredEventHandler(OnExceptionOccurred);
				}
				catch (Exception ex) 
				{
					OnExceptionOccurred(ex);
				}
			}
		}
		
		public IPEndPoint LocalEndPoint
		{
			get
			{
				return (IPEndPoint)_listener.LocalEndPoint;
			}
		}

	}

	public class AsyncTestClassBase
	{
		protected volatile bool _stop;

		public delegate void ExceptionOccurredEventHandler(Exception ex);
		public event ExceptionOccurredEventHandler ExceptionOccurred;

		protected virtual void OnExceptionOccurred(Exception ex)
		{
			if (_stop)
				return;

			if (ex is NullReferenceException || ex is ObjectDisposedException)
			{
//				WTrace.TraceInfo("Socket Hell", this.GetType(), "Ignoring NullReferenceException or ObjectDisposedException");
				return;
			}

			if (ex is IOException)
			{
				if (null != ex.InnerException)
				{
					if (ex.InnerException is SocketException)
					{
						SocketException socketEx = ex.InnerException as SocketException;
						if (10054 == socketEx.ErrorCode)
						{
//							WTrace.TraceInfo("Socket Hell", this.GetType(), "Ignoring \"Forcibly Closed\" socket exception.");
							return;
						}
					}
				}
			}

			try
			{
//				WTrace.TraceError("Socket Hell", this.GetType(), "Async Exception!\n{0}", ex.ToString());

				if (null != ExceptionOccurred)
					ExceptionOccurred(ex);
			}
			catch { }

			//let the test shut us down instead.
			//try
			//{
			//    Stop();
			//}
			//catch { }
		}

		public void Start()
		{
			_stop = false;

			lock (this)
			{
				OnStart();
			}
		}

		protected virtual void OnStart() { }

		public void Stop()
		{
			_stop = true;

			lock (this)
			{
				OnStop();
			}
		}

		protected virtual void OnStop() { }

	}
}
