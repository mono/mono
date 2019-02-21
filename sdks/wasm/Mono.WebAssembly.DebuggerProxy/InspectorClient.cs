using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace WsProxy {
	public class InspectorClient : WsClient {
		List<(int, TaskCompletionSource<Result>)> pending_cmds = new List<(int, TaskCompletionSource<Result>)> ();
		Func<string, JObject, CancellationToken, Task> onEvent;
		int next_cmd_id;

		Task HandleMessage (string msg, CancellationToken token)
		{
			var res = JObject.Parse (msg);
			if (res ["id"] == null)
				DumpProtocol (string.Format("Event method: {0} params: {1}", res ["method"], res ["params"]));
			else
				DumpProtocol (string.Format ("Response id: {0} res: {1}", res ["id"], res));

			if (res ["id"] == null)
				return onEvent (res ["method"].Value<string> (), res ["params"] as JObject, token);
			var id = res ["id"].Value<int> ();
			var idx = pending_cmds.FindIndex (e => e.Item1 == id);
			var item = pending_cmds [idx];
			pending_cmds.RemoveAt (idx);
			item.Item2.SetResult (Result.FromJson (res));
			return null;
		}

		public async Task Connect(
			Uri uri,
			Func<string, JObject, CancellationToken, Task> onEvent,
			Func<CancellationToken, Task> send,
			CancellationToken token) {

			this.onEvent = onEvent;
			await ConnectWithMainLoops (uri, HandleMessage, send, token);
		}

		public Task<Result> SendCommand (string method, JObject args, CancellationToken token)
		{
			int id = ++next_cmd_id;
			if (args == null)
				args = new JObject ();

			var o = JObject.FromObject (new {
				id = id,
				method = method,
				@params = args
			});

			var tcs = new TaskCompletionSource<Result> ();
			pending_cmds.Add ((id, tcs));

			var str = o.ToString ();
			System.Console.WriteLine ($"Command: id: {id} method: {method} params: {args}");

			var bytes = Encoding.UTF8.GetBytes (str);
			Send (bytes, token);
			return tcs.Task;
		}

		protected virtual void DumpProtocol (string msg){
			// Console.WriteLine (msg);
			//XXX make logging not stupid
		}
	}
} 
