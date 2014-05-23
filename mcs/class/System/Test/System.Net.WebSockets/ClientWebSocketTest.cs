#if NET_4_5
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;

using NUnit.Framework;


namespace MonoTests.System.Net.WebSockets
{
	[TestFixture]
	public class ClientWebSocketTest
	{
		const string EchoServerUrl = "ws://echo.websocket.org";
		const int Port = 42123;
		HttpListener listener;
		ClientWebSocket socket;
		MethodInfo headerSetMethod;

		[SetUp]
		public void Setup ()
		{
			listener = new HttpListener ();
			listener.Prefixes.Add ("http://localhost:" + Port + "/");
			listener.Start ();
			socket = new ClientWebSocket ();
		}

		[TearDown]
		public void Teardown ()
		{
			if (listener != null) {
				listener.Stop ();
				listener = null;
			}
			if (socket != null) {
				if (socket.State == WebSocketState.Open)
					socket.CloseAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (2000);
				socket.Dispose ();
				socket = null;
			}
		}

		[Test]
		public void ServerHandshakeReturnCrapStatusCodeTest ()
		{
			HandleHttpRequestAsync ((req, resp) => resp.StatusCode = 418);
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri ("ws://localhost:" + Port), CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.Success, typeof (WebException));
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		public void ServerHandshakeReturnWrongUpgradeHeader ()
		{
			HandleHttpRequestAsync ((req, resp) => {
					resp.StatusCode = 101;
					resp.Headers["Upgrade"] = "gtfo";
				});
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri ("ws://localhost:" + Port), CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.Success);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		public void ServerHandshakeReturnWrongConnectionHeader ()
		{
			HandleHttpRequestAsync ((req, resp) => {
					resp.StatusCode = 101;
					resp.Headers["Upgrade"] = "websocket";
					// Mono http request doesn't like the forcing, test still valid since the default connection header value is empty
					//ForceSetHeader (resp.Headers, "Connection", "Foo");
				});
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri ("ws://localhost:" + Port), CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.Success);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		public void EchoTest ()
		{
			const string Payload = "This is a websocket test";

			Assert.AreEqual (WebSocketState.None, socket.State);
			socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait ();
			Assert.AreEqual (WebSocketState.Open, socket.State);

			var sendBuffer = Encoding.ASCII.GetBytes (Payload);
			Assert.IsTrue (socket.SendAsync (new ArraySegment<byte> (sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None).Wait (5000));

			var receiveBuffer = new byte[Payload.Length];
			var resp = socket.ReceiveAsync (new ArraySegment<byte> (receiveBuffer), CancellationToken.None).Result;

			Assert.AreEqual (Payload.Length, resp.Count);
			Assert.IsTrue (resp.EndOfMessage);
			Assert.AreEqual (WebSocketMessageType.Text, resp.MessageType);
			Assert.AreEqual (Payload, Encoding.ASCII.GetString (receiveBuffer, 0, resp.Count));

			Assert.IsTrue (socket.CloseAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
			Assert.AreEqual (WebSocketState.Closed, socket.State);
		}

		[Test]
		public void CloseOutputAsyncTest ()
		{
			Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
			Assert.AreEqual (WebSocketState.Open, socket.State);

			Assert.IsTrue (socket.CloseOutputAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
			Assert.AreEqual (WebSocketState.CloseSent, socket.State);

			var resp = socket.ReceiveAsync (new ArraySegment<byte> (new byte[0]), CancellationToken.None).Result;
			Assert.AreEqual (WebSocketState.Closed, socket.State);
			Assert.AreEqual (WebSocketMessageType.Close, resp.MessageType);
			Assert.AreEqual (WebSocketCloseStatus.NormalClosure, resp.CloseStatus);
			Assert.AreEqual (string.Empty, resp.CloseStatusDescription);
		}

		[Test]
		public void CloseAsyncTest ()
		{
			Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
			Assert.AreEqual (WebSocketState.Open, socket.State);

			Assert.IsTrue (socket.CloseAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
			Assert.AreEqual (WebSocketState.Closed, socket.State);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void SendAsyncArgTest_NotConnected ()
		{
			socket.SendAsync (new ArraySegment<byte> (new byte[0]), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void SendAsyncArgTest_NoArray ()
		{
			Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
			socket.SendAsync (new ArraySegment<byte> (), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void ReceiveAsyncArgTest_NotConnected ()
		{
			socket.ReceiveAsync (new ArraySegment<byte> (new byte[0]), CancellationToken.None);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ReceiveAsyncArgTest_NoArray ()
		{
			Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
			socket.ReceiveAsync (new ArraySegment<byte> (), CancellationToken.None);
		}

		[Test]
		public void ReceiveAsyncWrongState_Closed ()
		{
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.CloseAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.ReceiveAsync (new ArraySegment<byte> (new byte[0]), CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.Success);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		public void SendAsyncWrongState_Closed ()
		{
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.CloseAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.SendAsync (new ArraySegment<byte> (new byte[0]), WebSocketMessageType.Text, true, CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.Success);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		public void SendAsyncWrongState_CloseSent ()
		{
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.CloseOutputAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.SendAsync (new ArraySegment<byte> (new byte[0]), WebSocketMessageType.Text, true, CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.Success);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		async Task HandleHttpRequestAsync (Action<HttpListenerRequest, HttpListenerResponse> handler)
		{
			var ctx = await listener.GetContextAsync ();
			handler (ctx.Request, ctx.Response);
			ctx.Response.Close ();
		}

		void AssertWebSocketException (AggregateException e, WebSocketError error, Type inner = null)
		{
			var wsEx = e.InnerException as WebSocketException;
			Console.WriteLine (e.InnerException.ToString ());
			Assert.IsNotNull (wsEx, "Not a websocketexception");
			Assert.AreEqual (error, wsEx.WebSocketErrorCode);
			if (inner != null) {
				Assert.IsNotNull (wsEx.InnerException);
				Assert.IsTrue (inner.IsInstanceOfType (wsEx.InnerException));
			}
		}

		void ForceSetHeader (WebHeaderCollection headers, string name, string value)
		{
			if (headerSetMethod == null)
				headerSetMethod = typeof (WebHeaderCollection).GetMethod ("AddValue", BindingFlags.NonPublic);
			headerSetMethod.Invoke (headers, new[] { name, value });
		}
	}
}

#endif
