using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Net.WebSockets
{
	[TestFixture]
	public class ClientWebSocketTest
	{
		const string EchoServerUrl = "ws://corefx-net-http11.azurewebsites.net/WebSocket/EchoWebSocket.ashx";

		ClientWebSocket socket;
		MethodInfo headerSetMethod;
		int Port;

		[SetUp]
		public void Setup ()
		{
			socket = new ClientWebSocket ();
		}

		HttpListener _listener;
		HttpListener listener {
			get {
				if (_listener != null)
					return _listener;

				return NetworkHelpers.CreateAndStartHttpListener ("http://localhost:", out Port, "/");
			}
		}

		[TearDown]
		public void Teardown ()
		{
			if (_listener != null) {
				_listener.Stop ();
				_listener = null;
			}
			if (socket != null) {
				if (socket.State == WebSocketState.Open)
					socket.CloseAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (2000);
				socket.Dispose ();
			}
		}

		[Test]
		[Category ("NotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void ServerHandshakeReturnCrapStatusCodeTest ()
		{
			// On purpose, 
			#pragma warning disable 4014
			HandleHttpRequestAsync ((req, resp) => resp.StatusCode = 418);
			#pragma warning restore 4014
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri ("ws://localhost:" + Port), CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.Success, typeof (WebException));
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void ServerHandshakeReturnWrongUpgradeHeader ()
		{
			#pragma warning disable 4014
			HandleHttpRequestAsync ((req, resp) => {
					resp.StatusCode = 101;
					resp.Headers["Upgrade"] = "gtfo";
				});
			#pragma warning restore 4014
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri ("ws://localhost:" + Port), CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.Success);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void ServerHandshakeReturnWrongConnectionHeader ()
		{
			#pragma warning disable 4014
			HandleHttpRequestAsync ((req, resp) => {
					resp.StatusCode = 101;
					resp.Headers["Upgrade"] = "websocket";
					// Mono http request doesn't like the forcing, test still valid since the default connection header value is empty
					//ForceSetHeader (resp.Headers, "Connection", "Foo");
				});
			#pragma warning restore 4014
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri ("ws://localhost:" + Port), CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.Success);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		[Category ("MobileNotWorking")] // The test hangs when ran as part of the entire BCL test suite. Works when only this fixture is ran
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
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void CloseOutputAsyncTest ()
		{
			Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
			Assert.AreEqual (WebSocketState.Open, socket.State);

			Assert.IsTrue (socket.CloseOutputAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
			Assert.AreEqual (WebSocketState.CloseSent, socket.State);

			var resp = socket.ReceiveAsync (new ArraySegment<byte> (new byte[0]), CancellationToken.None).Result;
			Assert.AreEqual (WebSocketState.CloseReceived, socket.State);
			Assert.AreEqual (WebSocketMessageType.Close, resp.MessageType);
			Assert.AreEqual (WebSocketCloseStatus.NormalClosure, resp.CloseStatus);
			Assert.AreEqual (string.Empty, resp.CloseStatusDescription);
		}

		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void CloseAsyncTest ()
		{
			if (!socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000)) {
				Assert.Inconclusive (socket.State.ToString ());
				return;
			}

			Assert.AreEqual (WebSocketState.Open, socket.State);

			Assert.IsTrue (socket.CloseAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
			Assert.AreEqual (WebSocketState.Closed, socket.State);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void SendAsyncArgTest_NotConnected ()
		{
			socket.SendAsync (new ArraySegment<byte> (new byte[0]), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void SendAsyncArgTest_NoArray ()
		{
			Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
			socket.SendAsync (new ArraySegment<byte> (), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void ReceiveAsyncArgTest_NotConnected ()
		{
			socket.ReceiveAsync (new ArraySegment<byte> (new byte[0]), CancellationToken.None);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void ReceiveAsyncArgTest_NoArray ()
		{
			Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
			socket.ReceiveAsync (new ArraySegment<byte> (), CancellationToken.None);
		}

		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void ReceiveAsyncWrongState_Closed ()
		{
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.CloseAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.ReceiveAsync (new ArraySegment<byte> (new byte[0]), CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.InvalidState);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void SendAsyncWrongState_Closed ()
		{
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.CloseAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.SendAsync (new ArraySegment<byte> (new byte[0]), WebSocketMessageType.Text, true, CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.InvalidState);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
		public void SendAsyncWrongState_CloseSent ()
		{
			try {
				Assert.IsTrue (socket.ConnectAsync (new Uri (EchoServerUrl), CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.CloseOutputAsync (WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait (5000));
				Assert.IsTrue (socket.SendAsync (new ArraySegment<byte> (new byte[0]), WebSocketMessageType.Text, true, CancellationToken.None).Wait (5000));
			} catch (AggregateException e) {
				AssertWebSocketException (e, WebSocketError.InvalidState);
				return;
			}
			Assert.Fail ("Should have thrown");
		}

		[Test]
		[Category ("NotWorking")]  // FIXME: test relies on unimplemented HttpListenerContext.AcceptWebSocketAsync (), reenable it when the method is implemented
		public void SendAsyncEndOfMessageTest ()
		{
			var cancellationToken = new CancellationTokenSource (TimeSpan.FromSeconds (30)).Token;
			SendAsyncEndOfMessageTest (false, WebSocketMessageType.Text, cancellationToken).Wait (5000);
			SendAsyncEndOfMessageTest (true, WebSocketMessageType.Text, cancellationToken).Wait (5000);
			SendAsyncEndOfMessageTest (false, WebSocketMessageType.Binary, cancellationToken).Wait (5000);
			SendAsyncEndOfMessageTest (true, WebSocketMessageType.Binary, cancellationToken).Wait (5000);
		}

		public async Task SendAsyncEndOfMessageTest (bool expectedEndOfMessage, WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken)
		{
			using (var client = new ClientWebSocket ()) {
				// Configure the listener.
				var serverReceive = HandleHttpWebSocketRequestAsync<WebSocketReceiveResult> (async socket => await socket.ReceiveAsync (new ArraySegment<byte> (new byte[32]), cancellationToken), cancellationToken);

				// Connect to the listener and make the request.
				await client.ConnectAsync (new Uri ("ws://localhost:" + Port + "/"), cancellationToken);
				await client.SendAsync (new ArraySegment<byte> (Encoding.UTF8.GetBytes ("test")), webSocketMessageType, expectedEndOfMessage, cancellationToken);

				// Wait for the listener to handle the request and return its result.
				var result = await serverReceive;

				// Cleanup and check results.
				await client.CloseAsync (WebSocketCloseStatus.NormalClosure, "Finished", cancellationToken);
				Assert.AreEqual (expectedEndOfMessage, result.EndOfMessage, "EndOfMessage should be " + expectedEndOfMessage);
			}
		}

		async Task<T> HandleHttpWebSocketRequestAsync<T> (Func<WebSocket, Task<T>> action, CancellationToken cancellationToken)
		{
			var ctx = await this.listener.GetContextAsync ();
			var wsContext = await ctx.AcceptWebSocketAsync (null);
			var result = await action (wsContext.WebSocket);
			await wsContext.WebSocket.CloseOutputAsync (WebSocketCloseStatus.NormalClosure, "Finished", cancellationToken);
			return result;
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

