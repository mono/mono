//
// repl.cs: Support for using the compiler in interactive mode (read-eval-print loop)
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004, 2005, 2006, 2007, 2008 Novell, Inc
// Copyright 2011-2013 Xamarin Inc
//
//
// TODO:
//   Do not print results in Evaluate, do that elsewhere in preparation for Eval refactoring.
//   Driver.PartialReset should not reset the coretypes, nor the optional types, to avoid
//      computing that on every call.
//
using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using Mono.CSharp;

namespace Mono {

	public class Driver {
		public static string StartupEvalExpression;
		static int? attach;
		static string target_host;
		static int target_port;
		static string agent;
		static string [] script_args;

		public static string [] ScriptArgs => script_args;
		
		static int Main (string [] args)
		{
			if (!SplitDriverAndScriptArguments (ref args, out script_args))
				return 1;

			var cmd = new CommandLineParser (Console.Out);
			cmd.UnknownOptionHandler += HandleExtraArguments;

			// Enable unsafe code by default
			var settings = new CompilerSettings () {
				Unsafe = true
			};

			if (!cmd.ParseArguments (settings, args))
				return 1;

			var startup_files = new string [settings.SourceFiles.Count];
			int i = 0;
			foreach (var source in settings.SourceFiles)
				startup_files [i++] = source.OriginalFullPathName;
			settings.SourceFiles.Clear ();

			TextWriter agent_stderr = null;
			ReportPrinter printer;
			if (agent != null) {
				agent_stderr = new StringWriter ();
				printer = new StreamReportPrinter (agent_stderr);
			} else {
				printer = new ConsoleReportPrinter ();
			}

			var eval = new Evaluator (new CompilerContext (settings, printer));

			eval.InteractiveBaseClass = typeof (InteractiveBaseShell);
			eval.DescribeTypeExpressions = true;
			eval.WaitOnTask = true;

			CSharpShell shell;
#if !ON_DOTNET
			if (attach.HasValue) {
				shell = new ClientCSharpShell_v1 (eval, attach.Value);
			} else if (agent != null) {
				new CSharpAgent (eval, agent, agent_stderr).Run (startup_files);
				return 0;
			} else
#endif
			if (target_host != null) 
				shell = new ClientCSharpShell  (eval, target_host, target_port);
			else 
				shell = new CSharpShell (eval);

			return shell.Run (startup_files);
		}

		static bool SplitDriverAndScriptArguments (ref string [] driver_args, out string [] script_args)
		{
			// split command line arguments into two groups:
			// - anything before '--' or '-s' goes to the mcs driver, which may
			//   call back into the csharp driver for further processing
			// - anything after '--' or '-s' is made available to the REPL/script
			//   via the 'Args' global, similar to csi.
			// - if '-s' is used, the argument immediately following it will
			//   also be processed by the mcs driver (e.g. a source file)

			int driver_args_count = 0;
			int script_args_offset = 0;
			string script_file = null;

			while (driver_args_count < driver_args.Length && script_args_offset == 0) {
				switch (driver_args [driver_args_count]) {
				case "--":
					script_args_offset = driver_args_count + 1;
					break;
				case "-s":
					if (driver_args_count + 1 >= driver_args.Length) {
						script_args = null;
						Console.Error.WriteLine ("usage is: -s SCRIPT_FILE");
						return false;
					}
					driver_args_count++;
					script_file = driver_args [driver_args_count];
					script_args_offset = driver_args_count + 1;
					break;
				default:
					driver_args_count++;
					break;
				}
			}

			if (script_args_offset > 0) {
				int script_args_count = driver_args.Length - script_args_offset;
				script_args = new string [script_args_count];
				Array.Copy (driver_args, script_args_offset, script_args, 0, script_args_count);
			} else
				script_args = Array.Empty<string> ();

			Array.Resize (ref driver_args, driver_args_count);
			if (script_file != null)
				driver_args [driver_args_count - 1] = script_file;

			return true;
		}

		static int HandleExtraArguments (string [] args, int pos)
		{
			switch (args [pos]) {
			case "-e":
				if (pos + 1 < args.Length) {
					StartupEvalExpression = args[pos + 1];
					return pos + 1;
				}
				break;
			case "--attach":
				if (pos + 1 < args.Length) {
					attach = Int32.Parse (args[1]);
					return pos + 1;
				}
				break;
			default:
				if (args [pos].StartsWith ("--server=")){
					var hostport = args [pos].Substring (9);
					int p = hostport.IndexOf (':');
					if (p == -1){
						target_host = hostport;
						target_port = 10000;
					} else {
						target_host = hostport.Substring (0,p);
						if (!int.TryParse (hostport.Substring (p), out target_port)){
							Console.Error.WriteLine ("Usage is: --server[=host[:port]");
							Environment.Exit (1);
						}
					}
					return pos + 1;
				}
				if (args [pos].StartsWith ("--client")){
					target_host = "localhost";
					target_port = 10000;
					return pos + 1;
				}
				if (args [pos].StartsWith ("--agent:")) {
					agent = args[pos];
					return pos + 1;
				} else {
					return -1;
				}
			}
			return -1;
		}
		
	}

	public class InteractiveBaseShell : InteractiveBase {
		static bool tab_at_start_completes;
		
		static InteractiveBaseShell ()
		{
			tab_at_start_completes = false;
		}

		internal static Mono.Terminal.LineEditor Editor;
		
		public static bool TabAtStartCompletes {
			get {
				return tab_at_start_completes;
			}

			set {
				tab_at_start_completes = value;
				if (Editor != null)
					Editor.TabAtStartCompletes = value;
			}
		}

		public static new string help {
			get {
				return InteractiveBase.help +
					"  TabAtStartCompletes      - Whether tab will complete even on empty lines\n" +
					"  Args                     - Any command line arguments passed to csharp\n" +
					"                             after the '--' (stop processing) argument";
			}
		}

		public static string [] Args => Driver.ScriptArgs;
	}
	
	public class CSharpShell {
		static bool isatty = true, is_unix = false;
		protected string [] startup_files;
		
		Mono.Terminal.LineEditor editor;
		bool dumb;
		readonly Evaluator evaluator;

		public CSharpShell (Evaluator evaluator)
		{
			this.evaluator = evaluator;
		}

		protected virtual void ConsoleInterrupt (object sender, ConsoleCancelEventArgs a)
		{
			// Do not about our program
			a.Cancel = true;

			evaluator.Interrupt ();
		}
		
		void SetupConsole ()
		{
			if (is_unix){
				string term = Environment.GetEnvironmentVariable ("TERM");
				dumb = term == "dumb" || term == null || isatty == false;
			} else
				dumb = false;
			
			editor = new Mono.Terminal.LineEditor ("csharp", 300) {
				HeuristicsMode = "csharp"
			};
			InteractiveBaseShell.Editor = editor;

			editor.AutoCompleteEvent += delegate (string s, int pos){
				string prefix = null;

				string complete = s.Substring (0, pos);
				
				string [] completions = evaluator.GetCompletions (complete, out prefix);
				
				return new Mono.Terminal.LineEditor.Completion (prefix, completions);
			};
			
#if false
			//
			// This is a sample of how completions sould be implemented.
			//
			editor.AutoCompleteEvent += delegate (string s, int pos){

				// Single match: "Substring": Sub-string
				if (s.EndsWith ("Sub")){
					return new string [] { "string" };
				}

				// Multiple matches: "ToString" and "ToLower"
				if (s.EndsWith ("T")){
					return new string [] { "ToString", "ToLower" };
				}
				return null;
			};
#endif
			
			Console.CancelKeyPress += ConsoleInterrupt;
		}

		string GetLine (bool primary)
		{
			string prompt = primary ? InteractiveBase.Prompt : InteractiveBase.ContinuationPrompt;

			if (dumb){
				if (isatty)
					Console.Write (prompt);

				return Console.ReadLine ();
			} else {
				return editor.Edit (prompt, "");
			}
		}

		delegate string ReadLiner (bool primary);

		void InitializeUsing ()
		{
			Evaluate ("using System; using System.Linq; using System.Collections.Generic; using System.Collections;");
		}

		void InitTerminal (bool show_banner)
		{
			int p = (int) Environment.OSVersion.Platform;
			is_unix = (p == 4) || (p == 128);

			isatty = !Console.IsInputRedirected && !Console.IsOutputRedirected;

			// Work around, since Console is not accounting for
			// cursor position when writing to Stderr.  It also
			// has the undesirable side effect of making
			// errors plain, with no coloring.
//			Report.Stderr = Console.Out;
			SetupConsole ();

			if (isatty && show_banner)
				Console.WriteLine ("Mono C# Shell, type \"help;\" for help\n\nEnter statements below.");

		}

		void ExecuteSources (IEnumerable<string> sources, bool ignore_errors)
		{
			foreach (string file in sources){
				try {
					try {
						bool first = true;
			
						using (System.IO.StreamReader r = System.IO.File.OpenText (file)){
							ReadEvalPrintLoopWith (p => {
								var line = r.ReadLine ();
								if (first){
									if (line.StartsWith ("#!"))
										line = r.ReadLine ();
									first = false;
								}
								return line;
							});
						}
					} catch (FileNotFoundException){
						Console.Error.WriteLine ("cs2001: Source file `{0}' not found", file);
						return;
					}
				} catch {
					if (!ignore_errors)
						throw;
				}
			}
		}
		
		protected virtual void LoadStartupFiles ()
		{
			string dir = Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
				"csharp");
			if (!Directory.Exists (dir))
				return;

			List<string> sources = new List<string> ();
			List<string> libraries = new List<string> ();
			
			foreach (string file in System.IO.Directory.GetFiles (dir)){
				string l = file.ToLower ();
				
				if (l.EndsWith (".cs"))
					sources.Add (file);
				else if (l.EndsWith (".dll"))
					libraries.Add (file);
			}

			foreach (string file in libraries)
				evaluator.LoadAssembly (file);

			ExecuteSources (sources, true);
		}

		void ReadEvalPrintLoopWith (ReadLiner readline)
		{
			string expr = null;
			while (!InteractiveBase.QuitRequested){
				string input = readline (expr == null);
				if (input == null)
					return;

				if (input == "")
					continue;

				expr = expr == null ? input : expr + "\n" + input;
				
				expr = Evaluate (expr);
			}
		}

		public int ReadEvalPrintLoop ()
		{
			if (startup_files != null && startup_files.Length == 0)
				InitTerminal (startup_files.Length == 0 && Driver.StartupEvalExpression == null);

			InitializeUsing ();

			LoadStartupFiles ();

			if (startup_files != null && startup_files.Length != 0) {
				ExecuteSources (startup_files, false);
			} else {
				if (Driver.StartupEvalExpression != null){
					ReadEvalPrintLoopWith (p => {
						var ret = Driver.StartupEvalExpression;
						Driver.StartupEvalExpression = null;
						return ret;
						});
				} else {
					ReadEvalPrintLoopWith (GetLine);
				}
				
				editor.SaveHistory ();
			}

			Console.CancelKeyPress -= ConsoleInterrupt;
			
			return 0;
		}

		protected virtual string Evaluate (string input)
		{
			bool result_set;
			object result;

			try {
				input = evaluator.Evaluate (input, out result, out result_set);

				if (result_set){
					PrettyPrint (Console.Out, result);
					Console.WriteLine ();
				}
			} catch (Exception e){
				Console.WriteLine (e);
				return null;
			}
			
			return input;
		}

		static void p (TextWriter output, string s)
		{
			output.Write (s);
		}

		static void EscapeString (TextWriter output, string s)
		{
			foreach (var c in s){
				if (c >= 32){
					output.Write (c);
					continue;
				}
				switch (c){
				case '\"':
					output.Write ("\\\""); break;
				case '\a':
					output.Write ("\\a"); break;
				case '\b':
					output.Write ("\\b"); break;
				case '\n':
					output.Write ("\n");
					break;
				
				case '\v':
					output.Write ("\\v");
					break;
				
				case '\r':
					output.Write ("\\r");
					break;
				
				case '\f':
					output.Write ("\\f");
					break;
				
				case '\t':
					output.Write ("\\t");
					break;

				default:
					output.Write ("\\x{0:x}", (int) c);
					break;
				}
			}
		}
		
		static void EscapeChar (TextWriter output, char c)
		{
			if (c == '\''){
				output.Write ("'\\''");
				return;
			}
			if (c >= 32){
				output.Write ("'{0}'", c);
				return;
			}
			switch (c){
			case '\a':
				output.Write ("'\\a'");
				break;

			case '\b':
				output.Write ("'\\b'");
				break;
				
			case '\n':
				output.Write ("'\\n'");
				break;
				
			case '\v':
				output.Write ("'\\v'");
				break;
				
			case '\r':
				output.Write ("'\\r'");
				break;
				
			case '\f':
				output.Write ("'\\f'");
				break;
				
			case '\t':
				output.Write ("'\\t");
				break;

			default:
				output.Write ("'\\x{0:x}'", (int) c);
				break;
			}
		}

		// Some types (System.Json.JsonPrimitive) implement
		// IEnumerator and yet, throw an exception when we
		// try to use them, helper function to check for that
		// condition
		static internal bool WorksAsEnumerable (object obj)
		{
			IEnumerable enumerable = obj as IEnumerable;
			if (enumerable != null){
				try {
					enumerable.GetEnumerator ();
					return true;
				} catch {
					// nothing, we return false below
				}
			}
			return false;
		}
		
		internal static void PrettyPrint (TextWriter output, object result)
		{
			if (result == null){
				p (output, "null");
				return;
			}
			
			if (result is Array){
				Array a = (Array) result;
				
				p (output, "{ ");
				int top = a.GetUpperBound (0);
				for (int i = a.GetLowerBound (0); i <= top; i++){
					PrettyPrint (output, a.GetValue (i));
					if (i != top)
						p (output, ", ");
				}
				p (output, " }");
			} else if (result is bool){
				if ((bool) result)
					p (output, "true");
				else
					p (output, "false");
			} else if (result is string){
				p (output, "\"");
				EscapeString (output, (string)result);
				p (output, "\"");
			} else if (result is IDictionary){
				IDictionary dict = (IDictionary) result;
				int top = dict.Count, count = 0;
				
				p (output, "{");
				foreach (DictionaryEntry entry in dict){
					count++;
					p (output, "{ ");
					PrettyPrint (output, entry.Key);
					p (output, ", ");
					PrettyPrint (output, entry.Value);
					if (count != top)
						p (output, " }, ");
					else
						p (output, " }");
				}
				p (output, "}");
			} else if (WorksAsEnumerable (result)) {
				int i = 0;
				p (output, "{ ");
				foreach (object item in (IEnumerable) result) {
					if (i++ != 0)
						p (output, ", ");

					PrettyPrint (output, item);
				}
				p (output, " }");
			} else if (result is char) {
				EscapeChar (output, (char) result);
			} else {
				p (output, result.ToString ());
			}
		}

		public virtual int Run (string [] startup_files)
		{
			this.startup_files = startup_files;
			return ReadEvalPrintLoop ();
		}
		
	}

	//
	// Stream helper extension methods
	//
	public static class StreamHelper {
		static DataConverter converter = DataConverter.LittleEndian;
		
		static void GetBuffer (this Stream stream, byte [] b)
		{
			int n, offset = 0;
			int len = b.Length;

			do {
				n = stream.Read (b, offset, len);
				if (n == 0)
					throw new IOException ("End reached");

				offset += n;
				len -= n;
			} while (len > 0);
		}

		public static int GetInt (this Stream stream)
		{
			byte [] b = new byte [4];
			stream.GetBuffer (b);
			return converter.GetInt32 (b, 0);
		}

		public static string GetString (this Stream stream)
		{
			int len = stream.GetInt ();
			if (len == 0)
				return "";

			byte [] b = new byte [len];
			stream.GetBuffer (b);

			return Encoding.UTF8.GetString (b);
		}

		public static void WriteInt (this Stream stream, int n)
		{
			byte [] bytes = converter.GetBytes (n);
			stream.Write (bytes, 0, bytes.Length);
		}
	
		public static void WriteString (this Stream stream, string s)
		{
			stream.WriteInt (s.Length);
			byte [] bytes = Encoding.UTF8.GetBytes (s);
			stream.Write (bytes, 0, bytes.Length);
		}
	}
	
	public enum AgentStatus : byte {
		// Received partial input, complete
		PARTIAL_INPUT  = 1,
	
		// The result was set, expect the string with the result
		RESULT_SET     = 2,
	
		// No result was set, complete
		RESULT_NOT_SET = 3,
	
		// Errors and warnings string follows
		ERROR          = 4,

		// Stdout
		STDOUT         = 5,
	}

	class ClientCSharpShell : CSharpShell {
		string target_host;
		int target_port;
		
		public ClientCSharpShell (Evaluator evaluator, string target_host, int target_port) : base (evaluator)
		{
			this.target_port = target_port;
			this.target_host = target_host;
		}

		T ConnectServer<T> (Func<NetworkStream,T> callback, Action<Exception> error)
		{
			try {
				var client = new TcpClient (target_host, target_port);
				var ns = client.GetStream ();
				T ret = callback (ns);
				ns.Flush ();
				ns.Close ();
				client.Close ();
				return ret;
			} catch (Exception e){
				error (e);
				return default(T);
			}
		}
		
		protected override string Evaluate (string input)
		{
			return ConnectServer<string> ((ns)=> {
				try {
					ns.WriteString ("EVALTXT");
					ns.WriteString (input);

					while (true) {
						AgentStatus s = (AgentStatus) ns.ReadByte ();
					
						switch (s){
						case AgentStatus.PARTIAL_INPUT:
							return input;
						
						case AgentStatus.ERROR:
							string err = ns.GetString ();
							Console.Error.WriteLine (err);
							break;

						case AgentStatus.STDOUT:
							string stdout = ns.GetString ();
							Console.WriteLine (stdout);
							break;
						
						case AgentStatus.RESULT_NOT_SET:
							return null;
						
						case AgentStatus.RESULT_SET:
							string res = ns.GetString ();
							Console.WriteLine (res);
							return null;
						}
					}
				} catch (Exception e){
					Console.Error.WriteLine ("Error evaluating expression, exception: {0}", e);
				}
				return null;
			}, (e) => {
				Console.Error.WriteLine ("Error communicating with server {0}", e);
			});
		}
		
		public override int Run (string [] startup_files)
		{
			// The difference is that we do not call Evaluator.Init, that is done on the target
			this.startup_files = startup_files;
			return ReadEvalPrintLoop ();
		}
	
		protected override void ConsoleInterrupt (object sender, ConsoleCancelEventArgs a)
		{
			ConnectServer<int> ((ns)=> {
				ns.WriteString ("INTERRUPT");
				return 0;
			}, (e) => { });
		}
			
	}

#if !ON_DOTNET
	//
	// A shell connected to a CSharpAgent running in a remote process.
	//  - maybe add 'class_name' and 'method_name' arguments to LoadAgent.
	//  - Support Gtk and Winforms main loops if detected, this should
	//    probably be done as a separate agent in a separate place.
	//
	class ClientCSharpShell_v1 : CSharpShell {
		NetworkStream ns, interrupt_stream;
		
		public ClientCSharpShell_v1 (Evaluator evaluator, int pid)
			: base (evaluator)
		{
			// Create a server socket we listen on whose address is passed to the agent
			TcpListener listener = new TcpListener (new IPEndPoint (IPAddress.Loopback, 0));
			listener.Start ();
			TcpListener interrupt_listener = new TcpListener (new IPEndPoint (IPAddress.Loopback, 0));
			interrupt_listener.Start ();
	
			string agent_assembly = typeof (ClientCSharpShell).Assembly.Location;
			string agent_arg = String.Format ("--agent:{0}:{1}" ,
							  ((IPEndPoint)listener.Server.LocalEndPoint).Port,
							  ((IPEndPoint)interrupt_listener.Server.LocalEndPoint).Port);
	
			var vm = new Attach.VirtualMachine (pid);
			vm.Attach (agent_assembly, agent_arg);
	
			/* Wait for the client to connect */
			TcpClient client = listener.AcceptTcpClient ();
			ns = client.GetStream ();
			TcpClient interrupt_client = interrupt_listener.AcceptTcpClient ();
			interrupt_stream = interrupt_client.GetStream ();
	
			Console.WriteLine ("Connected.");
		}

		//
		// A remote version of Evaluate
		//
		protected override string Evaluate (string input)
		{
			ns.WriteString (input);
			while (true) {
				AgentStatus s = (AgentStatus) ns.ReadByte ();
	
				switch (s){
				case AgentStatus.PARTIAL_INPUT:
					return input;
	
				case AgentStatus.ERROR:
					string err = ns.GetString ();
					Console.Error.WriteLine (err);
					break;
	
				case AgentStatus.RESULT_NOT_SET:
					return null;
	
				case AgentStatus.RESULT_SET:
					string res = ns.GetString ();
					Console.WriteLine (res);
					return null;
				}
			}
		}
		
		public override int Run (string [] startup_files)
		{
			// The difference is that we do not call Evaluator.Init, that is done on the target
			this.startup_files = startup_files;
			return ReadEvalPrintLoop ();
		}
	
		protected override void ConsoleInterrupt (object sender, ConsoleCancelEventArgs a)
		{
			// Do not about our program
			a.Cancel = true;
	
			interrupt_stream.WriteByte (0);
			int c = interrupt_stream.ReadByte ();
			if (c != -1)
				Console.WriteLine ("Execution interrupted");
		}
			
	}

	//
	// This is the agent loaded into the target process when using --attach.
	//
	class CSharpAgent
	{
		NetworkStream interrupt_stream;
		readonly Evaluator evaluator;
		TextWriter stderr;
		
		public CSharpAgent (Evaluator evaluator, String arg, TextWriter stderr)
		{
			this.evaluator = evaluator;
			this.stderr = stderr;
			new Thread (new ParameterizedThreadStart (Run)).Start (arg);
		}

		public void InterruptListener ()
		{
			while (true){
				int b = interrupt_stream.ReadByte();
				if (b == -1)
					return;
				evaluator.Interrupt ();
				interrupt_stream.WriteByte (0);
			}
		}
		
		public void Run (object o)
		{
			string arg = (string)o;
			string ports = arg.Substring (8);
			int sp = ports.IndexOf (':');
			int port = Int32.Parse (ports.Substring (0, sp));
			int interrupt_port = Int32.Parse (ports.Substring (sp+1));
	
			Console.WriteLine ("csharp-agent: started, connecting to localhost:" + port);
	
			TcpClient client = new TcpClient ("127.0.0.1", port);
			TcpClient interrupt_client = new TcpClient ("127.0.0.1", interrupt_port);
			Console.WriteLine ("csharp-agent: connected.");
	
			NetworkStream s = client.GetStream ();
			interrupt_stream = interrupt_client.GetStream ();
			new Thread (InterruptListener).Start ();

			try {
				// Add all assemblies loaded later
				AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoaded;
	
				// Add all currently loaded assemblies
				foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies ()) {
					// Some assemblies seem to be already loaded, and loading them again causes 'defined multiple times' errors
					if (a.GetName ().Name != "mscorlib" && a.GetName ().Name != "System.Core" && a.GetName ().Name != "System")
						evaluator.ReferenceAssembly (a);
				}
	
				RunRepl (s);
			} finally {
				AppDomain.CurrentDomain.AssemblyLoad -= AssemblyLoaded;
				client.Close ();
				interrupt_client.Close ();
				Console.WriteLine ("csharp-agent: disconnected.");			
			}
		}
	
		void AssemblyLoaded (object sender, AssemblyLoadEventArgs e)
		{
			evaluator.ReferenceAssembly (e.LoadedAssembly);
		}
	
		public void RunRepl (NetworkStream s)
		{
			string input = null;

			while (!InteractiveBase.QuitRequested) {
				try {
					string error_string;
					StringWriter error_output = (StringWriter)stderr;

					string line = s.GetString ();
	
					bool result_set;
					object result;
	
					if (input == null)
						input = line;
					else
						input = input + "\n" + line;
	
					try {
						input = evaluator.Evaluate (input, out result, out result_set);
					} catch (Exception e) {
						s.WriteByte ((byte) AgentStatus.ERROR);
						s.WriteString (e.ToString ());
						s.WriteByte ((byte) AgentStatus.RESULT_NOT_SET);
						continue;
					}
					
					if (input != null){
						s.WriteByte ((byte) AgentStatus.PARTIAL_INPUT);
						continue;
					}
	
					// Send warnings and errors back
					error_string = error_output.ToString ();
					if (error_string.Length != 0){
						s.WriteByte ((byte) AgentStatus.ERROR);
						s.WriteString (error_output.ToString ());
						error_output.GetStringBuilder ().Clear ();
					}
	
					if (result_set){
						s.WriteByte ((byte) AgentStatus.RESULT_SET);
						StringWriter sr = new StringWriter ();
						CSharpShell.PrettyPrint (sr, result);
						s.WriteString (sr.ToString ());
					} else {
						s.WriteByte ((byte) AgentStatus.RESULT_NOT_SET);
					}
				} catch (IOException) {
					break;
				} catch (Exception e){
					Console.WriteLine (e);
				}
			}
		}
	}

	public class UnixUtils {
		[System.Runtime.InteropServices.DllImport ("libc", EntryPoint="isatty")]
		extern static int _isatty (int fd);
			
		public static bool isatty (int fd)
		{
			try {
				return _isatty (fd) == 1;
			} catch {
				return false;
			}
		}
	}
#endif
}
	
namespace Mono.Management
{
	interface IVirtualMachine {
		void LoadAgent (string filename, string args);
	}
}

