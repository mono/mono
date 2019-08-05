using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace WsProxy {
	public class FirefoxMonoProxyServer 
	{
		int portProxy;
		int portBrowser;

		public FirefoxMonoProxyServer (int portProxy, int portBrowser)
		{
			this.portBrowser = portBrowser;
			this.portProxy = portProxy;
		}

		public void Run ()
		{
			var _server = new TcpListener (IPAddress.Parse ("127.0.0.1"), portProxy);
			_server.Start ();
			while (true) {
				// wait for client connection
				TcpClient newClient = _server.AcceptTcpClient ();

				// client found.
				// create a thread to handle communication
				var monoProxy = new FirefoxMonoProxy (portBrowser);
				Thread t = new Thread (new ParameterizedThreadStart (monoProxy.Run));
				t.Start (newClient);
			}
		}

			
	}
	public class FirefoxMonoProxy : MonoProxy {
		string actorName;
		string readedIDE;
		string readedBrowser;
		int local_breakpoint_id;
		bool isPausedOnMonoBreakpoint;
		int portBrowser;

		public FirefoxMonoProxy (int portBrowser)
		{
			this.portBrowser = portBrowser;
		}
		public void Run (object obj)
		{
			TcpClient client = (TcpClient)obj;
			NetworkStream ide = client.GetStream ();

			TcpClient client2 = new TcpClient ();
			client2.Connect ("127.0.0.1", portBrowser);
			NetworkStream Browser = client2.GetStream ();

			while (true) {
				Byte [] bytes = new Byte [100000];
				int bytesRec = 0;
				if (Browser.DataAvailable || (readedBrowser != null && readedBrowser.Length > 0)) {
					if (Browser.DataAvailable)
						bytesRec = Browser.Read (bytes, 0, bytes.Length);
					if (readedBrowser != null && readedBrowser.Length > 0) {
						var newStr = readedBrowser + Encoding.ASCII.GetString (bytes, 0, bytesRec);
						bytesRec = bytesRec + readedBrowser.Length;
						readedBrowser = "";
						bytes = Encoding.ASCII.GetBytes (newStr);
					}
					//Console.WriteLine ("Recebido do Browser = {0}", Encoding.ASCII.GetString (bytes, 0, bytesRec));
					ParseCommandFromBrowser (bytes, bytesRec, Browser, ide);
				}
				if (ide.DataAvailable || (readedIDE != null && readedIDE.Length > 0)) {
					if (ide.DataAvailable)
						bytesRec = ide.Read (bytes, 0, bytes.Length);

					if (readedIDE != null && readedIDE.Length > 0) {
						var newStr = readedIDE + Encoding.ASCII.GetString (bytes, 0, bytesRec);
						bytesRec = bytesRec + readedIDE.Length;
						readedIDE = "";
						bytes = Encoding.ASCII.GetBytes (newStr);
					}
					//Console.WriteLine ("Recebido do ide = {0}", Encoding.ASCII.GetString (bytes, 0, bytesRec));
					ParseCommandFromIde (bytes, bytesRec, Browser, ide);
				}
			}
		}
		void ParseCommandFromIde (Byte [] bytes, int bytesRec, NetworkStream Browser, NetworkStream ide)
		{
			string str = Encoding.ASCII.GetString (bytes, 0, bytesRec);
			int posColon = str.IndexOf (":");
			while (posColon > 0) {
				string size = str.Substring (0, posColon);
				if (str.Length < posColon + 1 + Convert.ToInt32 (size)) {
					readedIDE = str;

					goto Next;
				}
				string message = str.Substring (posColon + 1, Convert.ToInt32 (size));
				var details = JObject.Parse (message);
				if (details ["type"] != null && details ["type"].ToString ().Equals ("startListeners")) {
					//SendCommandToBrowser(details["to"].ToString(), "evaluateJS", Browser, ide);
					actorName = details ["to"].ToString ();
				}
				if (details ["type"] != null && details ["type"].ToString ().Equals ("resume") && details ["resumeLimit"].Type == JTokenType.Null) {
					//Sending request { "to":"server1.conn110.child1/consoleActor2","type":"evaluateJSAsync","text":"alert(\"oi\")","frameActor":"server1.conn110.child1/frame23"}
					SendCommandToBrowser (actorName, "evaluateJS", "MONO.mono_wasm_get_loaded_files()", Browser, ide);
					Byte [] bytes2 = new Byte [100000];
					int bytesRec2 = Browser.Read (bytes2, 0, bytes2.Length);
					ParseSourceFiles (bytes2, bytesRec2);
				}
				if (details ["type"] != null && details ["type"].ToString ().Equals ("resume") && details ["resumeLimit"].Type != JTokenType.Null && details ["resumeLimit"] ["type"].ToString ().Equals ("next")) {
					SendNextToBrowser (details, Browser, ide);
					goto Next;
				}
				if (details ["type"] != null && details ["type"].ToString ().Equals ("source") && details ["to"].ToString ().StartsWith ("dotnet://")) {
					SendFileToIde (details ["to"].ToString (), Browser, ide);
					goto Next;
				}
				if (details ["type"] != null && details ["type"].ToString ().Equals ("getBreakableLines") && details ["to"].ToString ().StartsWith ("dotnet://")) {
					SendBreakableLinesToIde (details ["to"].ToString (), Browser, ide);
					goto Next;
				}
				if (details ["type"] != null && details ["type"].ToString ().Equals ("getBreakpointPositionsCompressed") && details ["to"].ToString ().StartsWith ("dotnet://")) {
					SendBreakpointPositionsCompressed (details ["to"].ToString (), Browser, ide, details ["query"] ["start"] ["line"].ToString ());
					goto Next;
				}
				if (details ["type"] != null && details ["type"].ToString ().Equals ("setBreakpoint") && details ["location"] ["sourceUrl"].ToString ().EndsWith (".cs")) {
					SendBreakpointToMono (details, Browser, ide);
					goto Next;
				}
				if (details ["type"] != null && details ["type"].ToString ().Equals ("frames") && isPausedOnMonoBreakpoint) {
					GetFramesAndSendToIde (details, Browser, ide, details ["to"].ToString ());
					goto Next;
				}
				if (details ["type"] != null && details ["type"].ToString ().Equals ("getEnvironment") && details ["to"].ToString ().StartsWith ("dotnet")) {
					SendEnvironmentToIDE (details, Browser, ide);
					goto Next;
				}
				if (message.Length > 0) {
					var msg = $"{message.Length.ToString()}:{message}";
					Browser.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
				}

			Next:
				str = str.Substring (posColon + Convert.ToInt32 (size) + 1);
				posColon = str.IndexOf (":");

			}
		}

		void SendNextToBrowser (JObject details, NetworkStream Browser, NetworkStream ide)
		{
			SendCommandToBrowser (actorName, "evaluateJS", string.Format (MonoCommands.START_SINGLE_STEPPING, (int)StepKind.Over), Browser, ide);
			var message = "{ \"type\":\"resume\",\"resumeLimit\":null,\"rewind\":false,\"to\":\"" + details ["to"].ToString () + "\"}";
			var msg = $"{message.Length}:{message}";
			Browser.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
		}

		void SendEnvironmentToIDE (JObject details, NetworkStream Browser, NetworkStream ide)
		{
			var scope = current_callstack.FirstOrDefault (s => s.Id == int.Parse (details ["to"].ToString ().Substring ("dotnet:scope:".Length)));
			var vars = scope.Method.GetLiveVarsAt (scope.Location.CliLocation.Offset);


			var var_ids = string.Join (",", vars.Select (v => v.Index));

			SendCommandToBrowser (actorName, "evaluateJS", string.Format (MonoCommands.GET_SCOPE_VARIABLES, scope.Id, var_ids), Browser, ide);
			Byte [] bytes = new Byte [100000];
			int bytesRec = Browser.Read (bytes, 0, bytes.Length);
			string varsM = getFirstMessage (Encoding.ASCII.GetString (bytes, 0, bytesRec));
			var varsObj = JObject.Parse (varsM);
			var env = JObject.FromObject (new {
				actor = details ["to"].ToString () + scope.Method.Name,
				type = "function",
				scopeKind = "function",
				function = JObject.FromObject (new {
					type = "object",
					actor = details ["to"].ToString () + scope.Method.Name + "1",
					extensible = true,
					frozen = false,
					ownPropertyLength = vars.Length,
					name = scope.Method.Name,
					displayName = scope.Method.Name
				}),
				bindings = JObject.FromObject (new {
					arguments = new JArray (),

					variables = new JObject ()
				}),
				from = details ["to"].ToString ()
			});
			((JObject)env ["function"]).Add ("class", "Function");
			((JObject)env ["function"]).Add ("sealed", false);

			for (int i = 0; i < varsObj ["result"] ["preview"] ["length"].Value<int> (); i++) {
				var value = GetVariableValue (Browser, ide, varsObj ["result"] ["preview"] ["items"] [i] ["actor"].ToString ());
				if (value != null) {
					//System.Console.WriteLine (vars [i].Name + "=" + value.ToString ());
					((JObject)env ["bindings"] ["variables"]).Add (vars [i].Name, JObject.FromObject (new {
						value = value.ToString ()
					}));
				}


			}
			var msg = env.ToString ();
			msg = $"{msg.Length}:{msg}";
			//System.Console.WriteLine (msg);
			ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
			readedBrowser = getRestOfMessage (Encoding.ASCII.GetString (bytes, 0, bytesRec));
		}

		JToken GetVariableValue (NetworkStream Browser, NetworkStream ide, string actor)
		{
			Byte [] bytes = new Byte [100000];
			SendCommandToBrowser (actor, "prototypeAndProperties", "", Browser, ide);
		ReadAgain2:
			var bytesRec = Browser.Read (bytes, 0, bytes.Length);
			//System.Console.WriteLine (Encoding.ASCII.GetString (bytes, 0, bytesRec));
			var var = getFirstMessage (Encoding.ASCII.GetString (bytes, 0, bytesRec));
			var varObj = JObject.Parse (var);
			if (varObj ["ownProperties"] == null)
				goto ReadAgain2;
			return (varObj ["ownProperties"]? ["value"]? ["value"]? ["preview"]? ["ownProperties"]? ["value"]? ["value"]);
		}

		void GetFramesAndSendToIde (global::System.Object details, NetworkStream Browser, NetworkStream ide, string from)
		{
			SendCommandToBrowser (actorName, "evaluateJS", "MONO.mono_wasm_get_call_stack()", Browser, ide);
			Byte [] bytes = new Byte [100000];
		ReadAgain:
			int bytesRec = Browser.Read (bytes, 0, bytes.Length);
			//System.Console.WriteLine (Encoding.ASCII.GetString (bytes, 0, bytesRec));
			string callStack = getFirstMessage (Encoding.ASCII.GetString (bytes, 0, bytesRec));
			//System.Console.WriteLine ("CallStack:" + callStack);
			var call_stack = JObject.Parse (callStack);
			var actor = call_stack ["result"]? ["preview"]? ["ownProperties"]? ["frames"]? ["value"]? ["actor"].ToString ();
			if (actor == null)
				goto ReadAgain;

			SendCommandToBrowser (actor, "prototypeAndProperties", "", Browser, ide);
		ReadAgain2:
			bytesRec = Browser.Read (bytes, 0, bytes.Length);
			//System.Console.WriteLine (Encoding.ASCII.GetString (bytes, 0, bytesRec));
			callStack = getFirstMessage (Encoding.ASCII.GetString (bytes, 0, bytesRec));
			call_stack = JObject.Parse (callStack);
			var mono_frame = call_stack ["ownProperties"]? ["0"]? ["value"]? ["preview"]? ["ownProperties"];
			if (mono_frame == null)
				goto ReadAgain2;
			var il_pos = mono_frame ["il_pos"] ["value"].Value<int> ();
			var method_token = mono_frame ["method_token"] ["value"].Value<int> ();
			var assembly_name = mono_frame ["assembly_name"] ["value"].Value<string> ();
			var framesList = new List<Frame> ();
			var asm = store.GetAssemblyByName (assembly_name);
			var method = asm.GetMethodByToken (method_token);

			//TODO: do a loop to read all callstack

			var location = method?.GetLocationByIl (il_pos);
			int frame_id = 0;
			framesList.Add (new Frame (method, location, frame_id));


			var frames = JObject.FromObject (new {
				frames = new JArray (),
				from = from
			});

			var frame = JObject.FromObject (new {
				actor = $"dotnet:scope:{frame_id}",
				type = "call",
				displayName = method.Name,
				where = JObject.FromObject (new {
					actor = location.Id.ToString (),
					line = location.Line,
					column = location.Column
				})
			});
			((JArray)frames ["frames"]).Add (frame);
			++frame_id;
			current_callstack = framesList;
			var msg = frames.ToString ();
			msg = $"{msg.Length}:{msg}";
			//System.Console.WriteLine (msg);
			ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
			readedBrowser = getRestOfMessage (Encoding.ASCII.GetString (bytes, 0, bytesRec));
		}

		void SendBreakpointToMono (JObject details, NetworkStream Browser, NetworkStream ide)
		{
			if (store == null)
				return;
			string msg = ":{ \"from\":\"" + details ["to"].ToString () + "\"}";
			msg = (msg.Length - 1).ToString () + msg;
			//System.Console.WriteLine (msg);
			ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);


			BreakPointRequest req = BreakPointRequest.ParseFirefox (details, store);
			var bp_loc = store.FindBestBreakpoint (req);
			if (bp_loc == null)
				return;
			var bp = new Breakpoint (bp_loc, local_breakpoint_id++, BreakPointState.Pending);
			var asm_name = bp.Location.CliLocation.Method.Assembly.Name;
			var method_token = bp.Location.CliLocation.Method.Token;
			var il_offset = bp.Location.CliLocation.Offset;

			var expression = string.Format ("MONO.mono_wasm_set_breakpoint(\"{0}\", {1}, {2})", asm_name, method_token, il_offset);
			SendCommandToBrowser (actorName, "evaluateJS", expression, Browser, ide);
		}

		void SendBreakpointPositionsCompressed (string script_id, NetworkStream Browser, NetworkStream ide, string line)
		{
			string msg = ":{ \"positions\":{ \"" + line + "\":[0]},\"from\":\"" + script_id + "\"}";
			msg = (msg.Length - 1).ToString () + msg;
			//System.Console.WriteLine (msg);
			ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
		}
		void SendBreakableLinesToIde (string script_id, NetworkStream Browser, NetworkStream ide)
		{
			Byte [] bytes = new Byte [100000];
			var id = new SourceId (script_id);
			var doc = store.GetFileById (id);
			string breakableLines = ":{\"lines\":[";
			bool isFirst = true;
			foreach (var m in doc.Methods) {

				foreach (var sp in m.methodDef.DebugInformation.SequencePoints) {
					if (sp.IsHidden)
						continue;
					if (!isFirst) {
						breakableLines += ",";
					} else
						isFirst = false;
					breakableLines += sp.StartLine.ToString ();
				}
			}
		Next:
			breakableLines += "],\"from\":\"";
			breakableLines += script_id;
			breakableLines += "\"}";
			var msg = (breakableLines.Length - 1).ToString () + breakableLines;
			ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
			//var bytesRec = ide.Read(bytes, 0, bytes.Length);
			//System.Console.WriteLine("O que achou do source:" + Encoding.ASCII.GetString(bytes, 0, bytesRec));
			//107:{"lines":[15,16,17,19,20,22,23,25,27,29,30,31,32,33,35,36,38,39],"from":"server1.conn155.child28/source26"}
		}
		async Task SendFileToIde (string script_id, NetworkStream Browser, NetworkStream ide)
		{
			var id = new SourceId (script_id);
			var src_file = store.GetFileById (id);

			var res = new StringWriter ();
			//res.WriteLine ($"//{id}");

			try {
				var uri = new Uri (src_file.Url);
				if (uri.IsFile && File.Exists (uri.LocalPath)) {
					using (var f = new StreamReader (File.Open (src_file.SourceUri.LocalPath, FileMode.Open))) {
						await res.WriteAsync (await f.ReadToEndAsync ());
					}
					//
					var o = JObject.FromObject (new {
						source = res.ToString (),
						contentType = "text/javascript",
						from = script_id
					});
					var str2 = o.ToString ();
					var msg = $"{str2.Length}:{str2}";
					ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
				} else if (src_file.SourceLinkUri != null) {
					var doc = await new WebClient ().DownloadStringTaskAsync (src_file.SourceLinkUri);
					await res.WriteAsync (doc);

					var o = JObject.FromObject (new {
						source = res.ToString (),
						contentType = "text/javascript",
						from = script_id
					});

					var str2 = o.ToString ();
					var msg = $"{str2.Length}:{str2}";
					ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
				} else {
					var o = JObject.FromObject (new {
						source = $"// Unable to find document {src_file.SourceUri}",
						contentType = "text/javascript",
						from = script_id
					});
					var str2 = o.ToString ();
					var msg = $"{str2.Length}:{str2}";
					ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
				}
			} catch (Exception e) {
				var o = JObject.FromObject (new {
					source = $"// Unable to read document ({e.Message})\n" +
								$"Local path: {src_file?.SourceUri}\n" +
								$"SourceLink path: {src_file?.SourceLinkUri}\n",
					contentType = "text/javascript",
					from = script_id
				});
				var str2 = o.ToString ();
				var msg = $"{str2.Length}:{str2}";
				ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
			}
		}
		void SendCommandToBrowser (string to, string type, string expr, NetworkStream Browser, NetworkStream ide)
		{

			var o = JObject.FromObject (new {
				to = to,
				type = type,
				text = expr
			});
			var str = o.ToString ();
			var msg = $"{str.Length}:{str}";
			Browser.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);

		}

		void ParseSourceFiles (Byte [] bytes, int bytesRec)
		{
			string files = getFirstMessage (Encoding.ASCII.GetString (bytes, 0, bytesRec));
			//System.Console.WriteLine ("PDBs:" + files);
			var loaded_pdbs = JObject.Parse (files);
			var the_value = loaded_pdbs ["result"]? ["preview"]? ["items"];
			var the_pdbs = the_value?.ToObject<string []> ();
			if (the_pdbs != null)
				store = new DebugStore (the_pdbs);
			readedBrowser = getRestOfMessage (Encoding.ASCII.GetString (bytes, 0, bytesRec));
		}
		string getFirstMessage (string str)
		{
			var posColon = str.IndexOf (":");
			string size = str.Substring (0, posColon);
			string message = str.Substring (posColon + 1, Convert.ToInt32 (size));
			return message;
		}
		string getRestOfMessage (string str)
		{
			var posColon = str.IndexOf (":");
			string size = str.Substring (0, posColon);
			string message = str.Substring (posColon + 1 + Convert.ToInt32 (size));
			return message;
		}
		void ParseCommandFromBrowser (Byte [] bytes, int bytesRec, NetworkStream Browser, NetworkStream ide)
		{
			string str = Encoding.ASCII.GetString (bytes, 0, bytesRec);
			int posColon = str.IndexOf (":");
			while (posColon > 0) {
				string size = str.Substring (0, posColon);
				if (str.Length < posColon + 1 + Convert.ToInt32 (size)) {
					readedBrowser = str;
					return;
				}
				string message = str.Substring (posColon + 1, Convert.ToInt32 (size));
				var details = JObject.Parse (message);
				if (details ["sources"] != null && store != null) {
					foreach (var s in store.AllSources ()) {
						var obj = JObject.FromObject (new {
							actor = s.SourceId.ToString (),
							extensionName = "cs",
							url = s.Url,
							isBlackBoxed = false,
							introductionType = "scriptElement"
						});

						((JArray)details ["sources"]).Add (obj);
					}

					var str2 = details.ToString ();
					var msg = $"{str2.Length}:{str2}";
					ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
					goto Next;
				}
				if (details ["type"] != null && details ["type"].ToString ().Equals ("paused") && details ["frame"] != null && details ["frame"] ["displayName"].ToString ().Equals ("_mono_wasm_fire_bp"))
					isPausedOnMonoBreakpoint = true;
				if (message.Length > 0) {
					var msg = message.Length.ToString ();
					msg += ":";
					msg += message;
					ide.Write (Encoding.ASCII.GetBytes (msg), 0, msg.Length);
				}
			Next:
				str = str.Substring (posColon + Convert.ToInt32 (size) + 1);
				posColon = str.IndexOf (":");
			}
		}
	}
}
