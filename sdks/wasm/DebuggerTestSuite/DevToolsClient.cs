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
		TaskCompletionSource<bool> client_initiated_close = new TaskCompletionSource<bool> ();
		TaskCompletionSource<Task> new_write_available = new TaskCompletionSource<Task> ();
		DevToolsQueue queue;
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

		async Task<string> ReadOne (CancellationToken token)
		{
			byte [] buff = new byte [4000];
			var mem = new MemoryStream ();
			while (true) {

				if (socket.State != WebSocketState.Open) {
					Log ("error", $"DevToolsProxy: Socket is no longer open.");
					client_initiated_close.TrySetResult (true);
					return null;
				}

				// Console.WriteLine ($"ReadOne: waiting on ReceiveAsync");
				var result = await socket.ReceiveAsync (new ArraySegment<byte> (buff), token);
				// Console.WriteLine ($"ReadOne: back from waiting");
				if (result.MessageType == WebSocketMessageType.Close) {
					client_initiated_close.TrySetResult (true);
					return null;
				}

				mem.Write (buff, 0, result.Count);

				if (result.EndOfMessage) {
					// Console.WriteLine ($"\tReadOne: return the msg");
					return Encoding.UTF8.GetString (mem.GetBuffer (), 0, (int)mem.Length);
				}
			}
		}

		protected void Send (byte [] bytes, CancellationToken token)
		{
			var t = queue.Send (bytes, token);
			if (t != null)
				new_write_available.SetResult (t);
		}

		protected async Task<bool> ConnectWithMainLoops(
			Uri uri,
			Func<string, CancellationToken, Task> receive,
			Action<Exception> error,
			Func<CancellationToken, Task> init,
			CancellationToken token) {
			logger.LogDebug ("connecting to {0}", uri);
			this.socket = new ClientWebSocket ();
			this.socket.Options.KeepAliveInterval = Timeout.InfiniteTimeSpan;

			await this.socket.ConnectAsync (uri, token);

			logger.LogDebug ("starting the main loop");

			queue = new DevToolsQueue (this.socket);
			var our_cts = new CancellationTokenSource ();
			var x = CancellationTokenSource.CreateLinkedTokenSource (our_cts.Token, token);

			var pending_ops = new List<Task> {
				ReadOne (token),
				new_write_available.Task,
				client_initiated_close.Task,
				init (token)
			};

			Console.WriteLine ($"- start the loop");

			try {
				while (!x.IsCancellationRequested) {
					// Console.WriteLine ($"-- Let's wait for {pending_ops.Count} ops, pending_writes: {queue.pending.Count}");

					// for (int i = 0; i < pending_ops.Count; i ++)
					// 	Console.WriteLine ($"[{i}] = {pending_ops [i]}");

					await Task.WhenAny (pending_ops);
					var new_ops = new List<Task> (pending_ops.Count);

					for (int i = 0; i < pending_ops.Count; i ++) {
						var task = pending_ops [i];
						if (task.IsFaulted) {
							await task;
							throw new InvalidOperationException ("Should not have reached here");
						}

						if (task.Status != TaskStatus.RanToCompletion) {
							new_ops.Insert (i, task);
							continue;
						}

						if (task == pending_ops [0]) {
							var msg = ((Task<string>)task).Result;
							// Console.WriteLine ($"\t* adding ReadOne at [0]");
							new_ops.Insert (0, ReadOne (our_cts.Token));

							if (msg != null) {
								var newTask = receive (msg, our_cts.Token);
								if (newTask != null) {
									// Console.WriteLine ($"\t\t*added task for receive");
									new_ops.Add (newTask);
								}
							}
						} else if (task == pending_ops [1]) {
							// FIXME: check if the task is already done
							new_write_available = new TaskCompletionSource<Task> ();
							new_ops.Insert (1, new_write_available.Task);

							var write_task = ((Task<Task>)task).Result; // queue up a write
							new_ops.Add (write_task);
						} else if (task == pending_ops [2]) {
							// client_initiated_close
							if (task.IsFaulted)
								throw task.Exception;

							our_cts.Cancel ();
							return false;
						} else {
							var tsk = queue.Pump (our_cts.Token);
							if (tsk != null)
								new_ops.Add (tsk);
						}
					}

					pending_ops = new_ops;
				}
			} catch (Exception e) {
				Console.WriteLine ($"DevToolsClient::ConnectWithMainLoops: Exception {e.Message}");
				Log ("error", $"DevToolsClient::ConnectWithMainLoops: Exception {e.Message}");
				error (e);
				throw;
			} finally {
				if (!our_cts.IsCancellationRequested)
					our_cts.Cancel ();
			}

			Console.WriteLine ($"---- GOT cancellation request.. returning -----");
			return false;
		}

		protected virtual void Log (string priority, string msg)
		{
			//
		}
	}

	static class StringExtensions {
		public static string Truncate (this string str, int len)
		{
			return str?.Substring (0, Math.Min (len, str.Length));
		}

	}
}
