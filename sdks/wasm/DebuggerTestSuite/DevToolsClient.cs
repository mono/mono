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
		TaskCompletionSource<bool> side_exit = new TaskCompletionSource<bool> ();
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
			logger.LogDebug ("ReadOne: ENTER");
			try{
			byte [] buff = new byte [4000];
			var mem = new MemoryStream ();
			while (true) {

				if (socket.State != WebSocketState.Open) {
					logger.LogError ($"DevToolsProxy: Socket is no longer open.");
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
			} catch (Exception ex) { logger.LogError ($"ReadOne: {ex.Message}"); throw; }
		}

		protected void Send (byte [] bytes, CancellationToken token)
		{
			var t = queue.Send (bytes, token);
			if (t != null)
				new_write_available.SetResult (t);
		}

		async Task MarkCompleteAfterward (Func<CancellationToken, Task> send, CancellationToken token)
		{
			try {
					await send(token);
					side_exit.SetResult (true);
			} catch (Exception e) {
					logger.LogDebug ($"MarkCompleteForward: send() failed with: {e}");
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

			logger.LogDebug ("starting the main loop");

			queue = new DevToolsQueue (this.socket);
			var our_cts = new CancellationTokenSource ();
			var x = CancellationTokenSource.CreateLinkedTokenSource (our_cts.Token, token);

			var pending_ops = new List<Task> {
				ReadOne (x.Token),
				new_write_available.Task,
				client_initiated_close.Task,

				side_exit.Task,
				MarkCompleteAfterward (send, x.Token)
			};

			Console.WriteLine ($"- start the loop");
			ThreadPool.GetMaxThreads (out var wt, out var cpt);
			Console.WriteLine ($"max: {wt}, {cpt}");

			var new_ops = new List<Task> (pending_ops.Count * 2);
			try {
				while (!x.IsCancellationRequested) {
					logger.LogDebug ($"-- Let's wait for {pending_ops.Count} ops, pending_writes: {queue.pending.Count}");

					// for (int i = 0; i < pending_ops.Count; i ++)
						// Console.WriteLine ($"\t[{i}] = {pending_ops [i].Status} ({pending_ops [i]}");

					await Task.WhenAny (pending_ops);

					// new_ops.Clear ();
					for (int i = 0; i < pending_ops.Count; i ++) {
						var task = pending_ops [i];
						// Console.WriteLine ($"\t\tWoke: [{i}] = {pending_ops [i].Status}");
						if (task.IsFaulted) {
							logger.LogError ($"- task [{i}] has faulted. Exception: {task.Exception.Message}");
							throw task.Exception;
						}

						if (task.IsCanceled) {
							logger.LogTrace ($"********* Task is cancelled.. throw? or just exit??");
							//break;
							return false;
						}

						if (task.Status != TaskStatus.RanToCompletion) {
							new_ops.Insert (i, task);
							continue;
						}

						// Console.WriteLine ($"-- Got {i}'th task, status: {task.Status}");
						if (task == pending_ops [0]) {
							var msg = ((Task<string>)task).Result;
							// Console.WriteLine ($"\t* adding ReadOne at [0], Status: {task.Status}");//, msg: {msg.Truncate (100)}");
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
						} else if (task == side_exit.Task) {
							// Test ended normally. We don't care about the result of `side_exit`
							logger.LogTrace ($"- side_exit returned..");
							await side_exit.Task;
							return true;
						} else {
							var tsk = queue.Pump (our_cts.Token);
							if (tsk != null)
								new_ops.Add (tsk);
						}
					}

					var tmp = pending_ops;
					pending_ops = new_ops;
					new_ops = tmp;
					new_ops.Clear ();
				}
			} catch (Exception e) {
				// Console.WriteLine ($"DevToolsClient::ConnectWithMainLoops: Exception {e.Message}");
				logger.LogError ("error", $"DevToolsClient::ConnectWithMainLoops: Exception {e.Message}");
				throw;
			} finally {
				logger.LogDebug ($"-- DevToolsClient::ConnectWithMainLoops returning");
				logger.LogDebug ($"-- DevToolsclient::ConnectWithMainLoops x.IsCancellationRequested: {x.IsCancellationRequested}, our_cts: {our_cts.IsCancellationRequested}");
				for (int i = 0; i < pending_ops.Count; i ++)
					logger.LogDebug ($"\t[{i}] = {pending_ops [i].Status} ({pending_ops [i]}");
				ThreadPool.GetAvailableThreads (out var workerThreads, out var completionPortThreads);
				logger.LogDebug ($"Threadpool: pending: {ThreadPool.PendingWorkItemCount}, AvailThreads: (worker: {workerThreads}, cports: {completionPortThreads})");
				logger.LogDebug ($"GC: overall: :{GC.GetTotalAllocatedBytes () / 1024 / 1024}, totalmem: {GC.GetTotalMemory (false) / 1024 / 1024}");

				var minfo = GC.GetGCMemoryInfo ();
				Console.WriteLine ($"GCInfo: heapSize: {minfo.HeapSizeBytes / 1024 /1024}, frag: {minfo.FragmentedBytes/1024/1024}, memload: {minfo.MemoryLoadBytes/1024/1024}, highmemload: {minfo.HighMemoryLoadThresholdBytes/1024/1024}");

				if (!our_cts.IsCancellationRequested)
					our_cts.Cancel ();
			}

			logger.LogTrace ($"---- GOT cancellation request.. returning -----");
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
