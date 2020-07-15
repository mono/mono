using System;
using System.Threading.Tasks;

using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WebAssembly.Net.Debugging {
	internal class DevToolsClient: IDisposable {
		ClientWebSocket socket;
		List<Task> pending_ops = new List<Task> ();
		TaskCompletionSource<bool> side_exit = new TaskCompletionSource<bool> ();
		List<byte []> pending_writes = new List<byte []> ();
		Task current_write;
		readonly ILogger logger;

		public DevToolsClient (ILogger logger) {
			this.logger = logger;
		}

		~DevToolsClient() {
			Dispose(false);
		}

		public void Dispose() {
			Dispose(true);
		}

		public async Task Close (CancellationToken cancellationToken)
		{
			if (socket.State == WebSocketState.Open)
				await socket.CloseOutputAsync (WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
		}

		protected virtual void Dispose (bool disposing) {
			if (disposing)
				socket.Dispose ();
		}

		Task Pump (Task task, CancellationToken token)
		{
			if (task != current_write)
				return null;
			current_write = null;

			pending_writes.RemoveAt (0);

			if (pending_writes.Count > 0) {
				current_write = socket.SendAsync (new ArraySegment<byte> (pending_writes [0]), WebSocketMessageType.Text, true, token);
				return current_write;
			}
			return null;
		}

		async Task<string> ReadOne (CancellationToken token)
		{
			byte [] buff = new byte [4000];
			var mem = new MemoryStream ();
			while (true) {
				var result = await this.socket.ReceiveAsync (new ArraySegment<byte> (buff), token);
				if (result.MessageType == WebSocketMessageType.Close) {
					return null;
				}

				if (result.EndOfMessage) {
					mem.Write (buff, 0, result.Count);
					return Encoding.UTF8.GetString (mem.GetBuffer (), 0, (int)mem.Length);
				} else {
					mem.Write (buff, 0, result.Count);
				}
			}
		}

		protected void Send (byte [] bytes, CancellationToken token)
		{
			pending_writes.Add (bytes);
			if (pending_writes.Count == 1) {
				if (current_write != null)
					throw new Exception ("Internal state is bad. current_write must be null if there are no pending writes");

				current_write = socket.SendAsync (new ArraySegment<byte> (bytes), WebSocketMessageType.Text, true, token);
				pending_ops.Add (current_write);
			}
		}

		async Task MarkCompleteAfterward (Func<CancellationToken, Task> send, CancellationToken token)
		{
			try {
				await send(token);
				side_exit.SetResult (true);
			} catch (Exception e) {
				side_exit.SetException (e);
			}
		}

		protected async Task<bool> ConnectWithMainLoops(
			Uri uri,
			Func<string, CancellationToken, Task> receive,
			Func<CancellationToken, Task> send,
			CancellationToken token) {

			logger.LogDebug ("connecting to {0}", uri);
			this.socket = new ClientWebSocket ();
			this.socket.Options.KeepAliveInterval = Timeout.InfiniteTimeSpan;

			await this.socket.ConnectAsync (uri, token);
			pending_ops.Add (ReadOne (token));
			pending_ops.Add (side_exit.Task);
			pending_ops.Add (MarkCompleteAfterward (send, token));

			while (!token.IsCancellationRequested) {
				await Task.WhenAny (pending_ops);

				var new_ops = new List<Task> (pending_ops.Count);
				// Clear out all the completed tasks
				for (int i = 0; i < pending_ops.Count; i ++) {
					var task = pending_ops [i];

					if (task.IsFaulted) {
						await task;
						throw new InvalidOperationException ("Should not have reached here");
					}

					if (task.Status != TaskStatus.RanToCompletion) {
						new_ops.Add (task);
						continue;
					}

					if (i == 0) { //pending_ops[0] is for message reading
						var msg = ((Task<string>)task).Result;
						new_ops.Add (ReadOne (token));
						Task tsk = receive (msg, token);
						if (tsk != null)
							new_ops.Add (tsk);
					} else if (task == side_exit.Task) {
						var res = ((Task<bool>)task).Result;
						//it might not throw if exiting successfull
						return res;
					} else { //must be a background task
						var tsk = Pump (task, token);
						if (tsk != null)
							new_ops.Add (tsk);
					}
				}

				pending_ops = new_ops;
			}

			return false;
		}

		protected virtual void Log (string priority, string msg)
		{
			//
		}
	}
}
