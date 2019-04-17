/**
 * \file UploadToSentry.cs
 * Support for reading verbose unmanaged crash dumps
 *
 * Author:
 *   Alexander Kyte (alkyte@microsoft.com)
 *
 * (C) 2018 Microsoft, Inc.
 *
 */

using System;

using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Collections.Generic;

using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

using System.Net;

[assembly: CLSCompliant (false)]

namespace UploadToSentry
{
	// Modeled after https://github.com/getsentry/raven-csharp/blob/develop/src/app/SharpRaven/Dsn.cs
	class Dsn {
		public readonly string PrivateKey;
		public readonly string PublicKey;
		public readonly string ProjectID;

		private readonly string path;
		private readonly int port;
		private readonly string sentryUriString;
		private readonly Uri uri;

		public Dsn (string dsn) {
			this.uri = new Uri(dsn);
			this.PrivateKey = GetPrivateKey(this.uri);
			this.PublicKey = GetPublicKey(this.uri) ?? throw new ArgumentException("A publicKey is required.", nameof(dsn));
			this.port = this.uri.Port;
			this.ProjectID = GetProjectID(this.uri);
			this.path = GetPath(this.uri);
			this.sentryUriString = String.Format("{0}://{1}:{2}{3}/api/{4}/store/", this.uri.Scheme, this.uri.DnsSafeHost, this.port, this.path, this.ProjectID);
		}

		private static string GetPath(Uri uri)
		{
			int lastSlash = uri.AbsolutePath.LastIndexOf("/", StringComparison.Ordinal);
			return uri.AbsolutePath.Substring(0, lastSlash);
		}

		private static string GetProjectID(Uri uri)
		{
			int lastSlash = uri.AbsoluteUri.LastIndexOf("/", StringComparison.Ordinal);
			return uri.AbsoluteUri.Substring(lastSlash + 1);
		}

		public Uri SentryUri {
			get {
				return new Uri (sentryUriString);
			}
		}

		private static string GetPrivateKey(Uri uri)
		{
			var parts = uri.UserInfo.Split(':');
			return parts.Length == 2 ? parts[1] : null;
		}

		private static string GetPublicKey(Uri uri)
		{
			var publicKey = uri.UserInfo.Split(':')[0];
			return publicKey != string.Empty ? publicKey : null;
		}
	}

	class CodeCollection
	{
		Dictionary<Tuple<string, uint>, Collection<SequencePoint>> Lookup;
		Dictionary<Tuple<string, uint>, Tuple<string, string, string>> Types;

		public void Add (string assembly, string klass, string function, string mvid, uint token, Collection<SequencePoint> seqs)
		{
			var key = new Tuple<string, uint>(mvid, token);
			Lookup[key] = seqs;
			Types[key] = new Tuple<string, string, string>(assembly, klass, function);
		}

		public CodeCollection(string [] assemblies)
		{
			Lookup = new Dictionary<Tuple<string, uint>, Collection<SequencePoint>>();
			Types = new Dictionary<Tuple<string, uint>, Tuple<string, string, string>>();

			foreach (string assembly in assemblies)
			{
				if (assembly.EndsWith(".dll") || assembly.EndsWith(".exe"))
				{
					// Console.WriteLine("Reading {0}", assembly);
					var readerParameters = new ReaderParameters { ReadSymbols = true, InMemory = true };
					AssemblyDefinition myLibrary = null;
					try
					{
						myLibrary = AssemblyDefinition.ReadAssembly(assembly, readerParameters);
						string mvid = myLibrary.MainModule.Mvid.ToString().ToUpper();
						Console.WriteLine("\t-- Success Parsing {0}: {1}", assembly, mvid);

						foreach (var ty in myLibrary.MainModule.Types)
						{
							for (int i = 0; i < ty.Methods.Count; i++)
							{
								string klass = ty.FullName;
								string function = ty.Methods[i].FullName;
								uint token = Convert.ToUInt32(ty.Methods[i].MetadataToken.ToInt32());
								this.Add(assembly, klass, function, mvid, token, ty.Methods[i].DebugInformation.SequencePoints);
							}
						}
					}
					catch (SymbolsNotFoundException)
					{
						// ignore assemblies without debug symbols
						continue;
					}
					catch (Exception e)
					{
						Console.WriteLine("\t-- Error Parsing {0}: {1}", assembly, e.Message);
					}
				}
			}
		}

		public JObject Find (string mvid, uint token, uint goal)
		{
			var method_idx = new Tuple<string, uint>(mvid, token);
			if (!Lookup.ContainsKey(method_idx))
				return null;

			var seqs = Lookup[method_idx];

			var accum = new JObject();
			foreach (var seq in seqs)
			{
				if (goal != seq.Offset)
					continue;

				accum.Add (new JProperty("lineno", seq.StartLine));
				accum.Add (new JProperty("filename", seq.Document.Url));
				break;
			}

			var typ = Types[method_idx];
			var assembly = typ.Item1;
			var klass = typ.Item2;
			accum.Add (new JProperty("module", String.Format("{0} {1}", assembly, klass)));
			accum.Add (new JProperty("function", typ.Item3));

			return accum;
		}
	}


	class Uploader
	{
		CodeCollection codebase;

		public JObject Format_0_0_3 (string fileName, JObject payload, string hash) 
		{
			var event_id = new JProperty("event_id", Guid.NewGuid().ToString("n"));
			var timestamp = new JProperty("timestamp", DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
			var exc_objs = new List<JObject> ();
			var thread_objs = new List<JObject> ();

			var stackTraces = payload["threads"] as JArray;

			var path = Path.GetDirectoryName (fileName); // Best differentiator in-tree for files is where they are run from

			foreach (var st in stackTraces) {
				var thread_id = st["native_thread_id"]?.ToString();

				var unmanaged_frames = new List<JObject>(); 
				var managed_frames = new List<JObject>();
				var exceptions = new List<JObject>();

				var thread_name = st["thread_name"]?.ToString ();
				if (thread_name == null || thread_name?.Length == 0)
					thread_name = "Unnamed thread";

				var payload_unmanaged_frames = st["unmanaged_frames"] as JArray;
				for (int fr=0; payload_unmanaged_frames != null && fr < payload_unmanaged_frames.Count; fr++)
				{
					var frame = payload_unmanaged_frames [fr] as JObject;
					var native_address = frame["native_address"];
					var unmanaged_name = frame["unmanaged_name"] != null ? frame["unmanaged_name"].ToString() : "";

					var fn_filename = new JProperty("filename", "");
					var function = new JProperty("function", unmanaged_name);
					var module = new JProperty("module", "mono-sgen");
					var vars = new JProperty("vars", new JObject(new JProperty ("native_address", native_address)));
					var blob = new JObject(fn_filename, function, module, vars);

					unmanaged_frames.Add (blob);
				}

				var payload_managed_frames = st["managed_frames"] as JArray;
				for (int fr = 0; payload_managed_frames != null && fr < payload_managed_frames.Count; fr++)
				{
					var frame = payload_managed_frames [fr] as JObject;
					if (frame["is_managed"] != null && frame["is_managed"].ToString ().ToUpper () == "TRUE")
					{
						var guid_val = frame["guid"].ToString ();
						var token_val = Convert.ToUInt32(frame["token"].ToString (), 16);
						var offset_val = Convert.ToUInt32(frame["il_offset"].ToString (), 16);

						var output_frame = codebase.Find (guid_val, token_val, offset_val);
						if (output_frame == null)
							continue;

						var guid  = new JProperty("guid", guid_val);
						var token =  new JProperty("token", token_val);
						var il_offset = new JProperty("il_offset", offset_val);
					   
						output_frame.Add (new JProperty("vars", new JObject(guid, token, il_offset)));

						managed_frames.Add(output_frame);
					} else {
						var native_address = frame["native_address"];
						var unmanaged_name = frame["unmanaged_name"] != null ? frame["unmanaged_name"].ToString() : "";

						var fn_filename = new JProperty("filename", "mono-sgen");
						var function = new JProperty("function", unmanaged_name);
						var module = new JProperty("module", "");
						var vars = new JProperty("vars", frame);
						var blob = new JObject(fn_filename, function, module, vars);

						managed_frames.Add (blob);
					}
				}

				var payload_exceptions = st["exceptions"] as JArray;
				for (int exc=0; payload_exceptions != null && exc < payload_exceptions.Count; exc++)
				{
					var exception_obj = payload_exceptions [exc] as JObject;
					var exc_managed_frames = new List<JObject>();
					var payload_exception_frames = exception_obj ["managed_frames"] as JArray;
					for (int fr = 0; payload_exception_frames != null && fr < payload_exception_frames.Count; fr++)
					{
						var frame = payload_exception_frames [fr] as JObject;
						if (frame["is_managed"] != null && frame["is_managed"].ToString ().ToUpper () == "TRUE")
						{
							var guid_val = frame["guid"].ToString ();
							var token_val = Convert.ToUInt32(frame["token"].ToString (), 16);
							var offset_val = Convert.ToUInt32(frame["il_offset"].ToString (), 16);

							var output_frame = codebase.Find (guid_val, token_val, offset_val);
							if (output_frame == null)
								continue;

							var guid = new JProperty("guid", guid_val);
							var token =  new JProperty("token", token_val);
							var il_offset = new JProperty("il_offset", offset_val);
						   
							output_frame.Add (new JProperty("vars", new JObject(guid, token, il_offset)));

							exc_managed_frames.Add(output_frame);
						} else {
							var native_address = frame["native_address"];
							var unmanaged_name = frame["unmanaged_name"] != null ? frame["unmanaged_name"].ToString() : "";

							var fn_filename = new JProperty("filename", "mono-sgen");
							var function = new JProperty("function", unmanaged_name);
							var module = new JProperty("module", "");
							var vars = new JProperty("vars", frame);
							var blob = new JObject(fn_filename, function, module, vars);

							exc_managed_frames.Add (blob);
						}
					}

					var type = new JProperty ("type", exception_obj ["type"].ToString ());
					var managed_st = new JProperty("frames", new JArray(exc_managed_frames.ToArray()));
					var exc_blob = new JObject(type, managed_st);

					exceptions.Add (exc_blob);
				}

				if (unmanaged_frames.Count > 0) {
					var unmanaged_st = new JObject(new JProperty("frames", new JArray(unmanaged_frames.ToArray ())));
					var id = String.Format ("{0}_unmanaged", st ["native_thread_id"]);
					var active = new JProperty ("active", "true");

					if (st["crashed"]?.ToString ().ToUpper () == "TRUE") {
						var unmanaged_thread = new JObject (active, new JProperty ("crashed", "true"), new JProperty ("name", String.Format ("{0} unmanaged", thread_name)), new JProperty ("id", id));
						var unmanaged_exc = new JObject(new JProperty("module", String.Format("{0}_managed_frames", thread_id)),
							new JProperty("type", path),
							new JProperty("value", ""),
							new JProperty("stacktrace", unmanaged_st), new JProperty("thread_id", id));

						thread_objs.Add(unmanaged_thread);
						exc_objs.Add (unmanaged_exc);
					} else {
						var unmanaged_thread = new JObject (active, new JProperty ("name", String.Format ("{0} Unmanaged", thread_name)), 
							new JProperty ("id", id), new JProperty ("stacktrace", unmanaged_st));
						thread_objs.Add(unmanaged_thread);
					}
				}

				if (managed_frames.Count > 0) {
					var managed_st = new JObject(new JProperty("frames", new JArray(managed_frames.ToArray())));
					// If we are the crashing thread, set the exception object to the
					// managed stacktrace and the thread object to the managed thread
					//
					// If we aren't, add the thread + st to 
					var id = String.Format ("{0}_managed", st["native_thread_id"]);
					var active = new JProperty ("active", "true");

					if (unmanaged_frames.Count == 0 && st["crashed"]?.ToString ().ToUpper () == "TRUE") {
						var managed_thread = new JObject (active, new JProperty ("crashed", "true"), new JProperty ("name", String.Format ("{0} managed", thread_name)), new JProperty ("id", id));
						var managed_exc = new JObject(new JProperty("module", String.Format("{0}_managed_frames", thread_id)),
							new JProperty("type", path),
							new JProperty("value", ""),
							new JProperty("stacktrace", managed_st), new JProperty("thread_id", id));

						thread_objs.Add(managed_thread);
						exc_objs.Add (managed_exc);
					} else {
						var managed_thread = new JObject (active, new JProperty ("name", String.Format ("{0} managed", thread_name)), 
							new JProperty ("id", id), new JProperty ("stacktrace", managed_st));
						thread_objs.Add(managed_thread);
					}
				}

				bool first_exception = true;
				foreach (var exc_input in exceptions) {
					// The 0.0.3 managed exception only has one thread, so we want it to be active and set crashed to true.
					var active = new JProperty ("active", first_exception ? "true" : "false");
					first_exception = false;

					var name = "Caught Managed Exception";

					var exception_thread = new JObject (active, new JProperty ("crashed", "true"), new JProperty ("name", name), new JProperty ("id", name), new JProperty ("exception", exc_input));
					var exception_exc = new JObject(new JProperty("module", name),
							new JProperty("type", path),
							new JProperty("value", ""),
							new JProperty("stacktrace", exc_input ["frames"]));

					thread_objs.Add (exception_thread);
					exc_objs.Add (exception_exc);
				}
			}

			var exception = new JProperty("exception", new JObject (new JProperty ("values", new JArray(exc_objs.ToArray ()))));
			var threads = new JProperty("threads", new JObject (new JProperty ("values", new JArray(thread_objs.ToArray ()))));
			// Bake in the whole blob
			var embedded = new JProperty("extra", payload);
			var fingerprint = new JProperty ("fingerprint", new JArray (new JValue (hash)));

			var sentry_message = new JObject (timestamp, event_id, exception, embedded, threads, fingerprint);

			return sentry_message;
		}

		public JObject Format_0_0_2 (string fileName, JObject payload, string hash) 
		{
			var event_id = new JProperty("event_id", Guid.NewGuid().ToString("n"));
			var timestamp = new JProperty("timestamp", DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
			var exc_objs = new List<JObject> ();
			var thread_objs = new List<JObject> ();

			var stackTraces = payload["threads"] as JArray;

			var path = Path.GetDirectoryName (fileName); // Best differentiator in-tree for files is where they are run from

			for (int i=0; i < stackTraces.Count; i++){
				var thread_id = stackTraces[i]["native_thread_id"]?.ToString();

				var unmanaged_frames = new List<JObject>(); 
				var managed_frames = new List<JObject>();

				var thread_name = stackTraces [i]["thread_name"].ToString ();
				if (thread_name == null || thread_name.Length == 0)
					thread_name = "Unnamed thread";

				var payload_unmanaged_frames = stackTraces[i]["unmanaged_frames"] as JArray;
				for (int fr=0; payload_unmanaged_frames != null && fr < payload_unmanaged_frames.Count; fr++)
				{
					var frame = payload_unmanaged_frames [fr] as JObject;
					var native_address = frame["native_address"];
					var unmanaged_name = frame["unmanaged_name"] != null ? frame["unmanaged_name"].ToString() : "";

					var fn_filename = new JProperty("filename", "");
					var function = new JProperty("function", unmanaged_name);
					var module = new JProperty("module", "mono-sgen");
					var vars = new JProperty("vars", new JObject(new JProperty ("native_address", native_address)));
					var blob = new JObject(fn_filename, function, module, vars);

					unmanaged_frames.Add (blob);
				}

				var payload_managed_frames = stackTraces[i]["managed_frames"] as JArray;
				for (int fr = 0; payload_managed_frames != null && fr < payload_managed_frames.Count; fr++)
				{
					var frame = payload_managed_frames [fr] as JObject;
					if (frame["is_managed"] != null && frame["is_managed"].ToString ().ToUpper () == "TRUE")
					{
						var guid_val = frame["guid"].ToString ();
						var token_val = Convert.ToUInt32(frame["token"].ToString (), 16);
						var offset_val = Convert.ToUInt32(frame["il_offset"].ToString (), 16);

						var output_frame = codebase.Find (guid_val, token_val, offset_val);
						if (output_frame == null)
							continue;

						var guid  = new JProperty("guid", guid_val);
						var token =  new JProperty("token", token_val);
						var il_offset = new JProperty("il_offset", offset_val);
					   
						output_frame.Add (new JProperty("vars", new JObject(guid, token, il_offset)));

						managed_frames.Add(output_frame);
					} else {
						var native_address = frame["native_address"];
						var unmanaged_name = frame["unmanaged_name"] != null ? frame["unmanaged_name"].ToString() : "";

						var fn_filename = new JProperty("filename", "mono-sgen");
						var function = new JProperty("function", unmanaged_name);
						var module = new JProperty("module", "");
						var vars = new JProperty("vars", frame);
						var blob = new JObject(fn_filename, function, module, vars);

						managed_frames.Add (blob);
					}
				}

				if (unmanaged_frames.Count > 0) {
					var unmanaged_st = new JObject(new JProperty("frames", new JArray(unmanaged_frames.ToArray ())));
					var id = String.Format ("{0}_unmanaged", stackTraces[i]["native_thread_id"]);
					var active = new JProperty ("active", "true");

					if (stackTraces[i]["crashed"].ToString ().ToUpper () == "TRUE") {
						var unmanaged_thread = new JObject (active, new JProperty ("crashed", "true"), new JProperty ("name", String.Format ("{0} unmanaged", thread_name)), new JProperty ("id", id));
						var unmanaged_exc = new JObject(new JProperty("module", String.Format("{0}_managed_frames", thread_id)),
							new JProperty("type", path),
							new JProperty("value", ""),
							new JProperty("stacktrace", unmanaged_st), new JProperty("thread_id", id));

						thread_objs.Add(unmanaged_thread);
						exc_objs.Add (unmanaged_exc);
					} else {
						var unmanaged_thread = new JObject (active, new JProperty ("name", String.Format ("{0} Unmanaged", thread_name)), 
							new JProperty ("id", id), new JProperty ("stacktrace", unmanaged_st));
						thread_objs.Add(unmanaged_thread);
					}
				}

				if (managed_frames.Count > 0) {
					var managed_st = new JObject(new JProperty("frames", new JArray(managed_frames.ToArray())));
					// If we are the crashing thread, set the exception object to the
					// managed stacktrace and the thread object to the managed thread
					//
					// If we aren't, add the thread + st to 
					var id = String.Format ("{0}_managed", stackTraces[i]["native_thread_id"]);
					var active = new JProperty ("active", "true");

					if (unmanaged_frames.Count == 0 && stackTraces[i]["crashed"].ToString ().ToUpper () == "TRUE") {
						var managed_thread = new JObject (active, new JProperty ("crashed", "true"), new JProperty ("name", String.Format ("{0} managed", thread_name)), new JProperty ("id", id));
						var managed_exc = new JObject(new JProperty("module", String.Format("{0}_managed_frames", thread_id)),
							new JProperty("type", path),
							new JProperty("value", ""),
							new JProperty("stacktrace", managed_st), new JProperty("thread_id", id));

						thread_objs.Add(managed_thread);
						exc_objs.Add (managed_exc);
					} else {
						var managed_thread = new JObject (active, new JProperty ("name", String.Format ("{0} managed", thread_name)), 
							new JProperty ("id", id), new JProperty ("stacktrace", managed_st));
						thread_objs.Add(managed_thread);
					}
				}
			}

			var exception = new JProperty("exception", new JObject (new JProperty ("values", new JArray(exc_objs.ToArray ()))));
			var threads = new JProperty("threads", new JObject (new JProperty ("values", new JArray(thread_objs.ToArray ()))));
			// Bake in the whole blob
			var embedded = new JProperty("extra", payload);
			var fingerprint = new JProperty ("fingerprint", new JArray (new JValue (hash)));

			var sentry_message = new JObject (timestamp, event_id, exception, embedded, threads, fingerprint);

			return sentry_message;
		}

		public void SendMessage (JObject sentry_message, Dsn url)
		{
			// Console.WriteLine ("Sending {0}", sentry_message.ToString ());

			var request = (HttpWebRequest) WebRequest.Create (url.SentryUri);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.UserAgent = "MonoSentryUploader/1.0.0.0";

			var sentryVersion = 7;
			var time = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			var key = url.PrivateKey != null ? ", sentry_secret=" + url.PrivateKey : null;
			var header = String.Format("Sentry sentry_version={0}" +
			", sentry_client={1}, sentry_timestamp={2}, sentry_key={3}{4}",
			sentryVersion, request.UserAgent, time, url.PublicKey, key);
			request.Headers ["X-Sentry-Auth"] = header;

			byte[] byteArray = Encoding.UTF8.GetBytes(sentry_message.ToString ());
			request.ContentLength = byteArray.Length;

			Stream dataStream = request.GetRequestStream();
			dataStream.Write(byteArray, 0, byteArray.Length);
			dataStream.Close();

			try {
				WebResponse response = request.GetResponse ();
				// Display the status.  
				// Console.WriteLine(((HttpWebResponse)response).StatusDescription);

				StreamReader reader = new StreamReader(response.GetResponseStream());
				// Read the content.  
				string responseFromServer = reader.ReadToEnd();
				// Display the content.  
				Console.WriteLine("\t-- HTTP POST Success {0}", responseFromServer);
				// Clean up the streams.  
			} catch (WebException ex) {
				Console.WriteLine("\t-- HTTP POST Error:", ex.Response.Headers [""]);
				for (int i=0; i < ex.Response.Headers.Count; i++)
					Console.WriteLine("\t\t {0} : {1}", ex.Response.Headers.Keys [i], ex.Response.Headers [i]);
			}
		}

		public void Upload (string filePath, string os_tag, Dsn url)
		{
			if (!File.Exists(filePath))
				throw new Exception(String.Format("Json file not found {0}", filePath));

			var dump = File.ReadAllText(filePath);
			//var message = new SentryMessage(dump);
			// var blob = new SentryEvent(message);
			var payload = JObject.Parse(dump);

			// Try to extract a test name
			var fileName = Path.GetFileName (filePath);
			var extract = Regex.Match(fileName, @"mono_crash\.([A-Za-z0-9]+)\.(\d)\.json");
			if (!extract.Success)
				throw new Exception ("File name does not match correct format");

			var groups = extract.Groups;
			var hash = groups[1].Value;
			// var increment = groups[2].Value;

			var version_string = payload["protocol_version"].ToString();
			JObject sentry_message = null;
			if (version_string == "0.0.4") {
				// Same for now
				sentry_message = Format_0_0_3 (filePath, payload, hash);
			} else if (version_string == "0.0.3") {
				sentry_message = Format_0_0_3 (filePath, payload, hash);
			} else if (version_string == "0.0.2") {
				sentry_message = Format_0_0_2 (filePath, payload, hash);
			} else {
				Console.WriteLine ("ERROR: Crash reporting version mismatch");
				return;
			}

			// sent to url via post?
			// Console.WriteLine (sentry_message);

			SendMessage (sentry_message, url);
		}

		public Uploader (CodeCollection assemblies)
		{
			this.codebase = assemblies;
		}

		static string[] GetAssemblies (string fileRoot) {
			var dlls = Directory.GetFiles (fileRoot, "*.dll", SearchOption.AllDirectories);
			var exes = Directory.GetFiles (fileRoot, "*.exe", SearchOption.AllDirectories);

			return dlls.Concat (exes).ToArray ();
		}

		static string[] GetFiles (string fileRoot) {
			return Directory.GetFiles (fileRoot, "mono_crash.*.json", SearchOption.AllDirectories);
		}

		public static void Main (string[] args)
		{
			var url = System.Environment.GetEnvironmentVariable ("MONO_SENTRY_URL");
			if (url == null) {
				Console.WriteLine ("MONO_SENTRY_URL missing");
				return;
			}

			var fileRoot = System.Environment.GetEnvironmentVariable ("MONO_SENTRY_ROOT");
			if (fileRoot == null) {
				Console.WriteLine ("MONO_SENTRY_ROOT missing");
				return;
			}

			var os_tag = System.Environment.GetEnvironmentVariable ("MONO_SENTRY_OS");
			if (os_tag == null) {
				Console.WriteLine ("MONO_SENTRY_OS missing");
				return;
			}

			var dsn = new Dsn(url);

			var files = GetFiles (fileRoot);

			if (files.Length == 0)
				return;

			// Find all of the assemblies in tree that could have made the crash dump
			var assemblies = GetAssemblies (fileRoot);
			var codebase = new CodeCollection (assemblies);

			foreach (var file in files) {
				Console.WriteLine ($"Processing {file} ...");
				var state = new Uploader (codebase);
				state.Upload (file, os_tag, dsn);
			}
		}
	}
}
