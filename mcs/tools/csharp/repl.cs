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
using Mono.CSharp;

using Mono.Attach;

namespace Mono {

	public static class CSharpShell {
		static bool isatty = true;
		
		static Mono.Terminal.LineEditor editor;
		static bool dumb;

		static void ConsoleInterrupt (object sender, ConsoleCancelEventArgs a)
		{
			// Do not about our program
			a.Cancel = true;

			Mono.CSharp.Evaluator.Interrupt ();
		}
		
		static void SetupConsole ()
		{
			string term = Environment.GetEnvironmentVariable ("TERM");
			dumb = term == "dumb" || term == null || isatty == false;
			
			editor = new Mono.Terminal.LineEditor ("csharp", 300);
			Console.CancelKeyPress += ConsoleInterrupt;
		}

		static string GetLine (bool primary)
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

		static void InitializeUsing ()
		{
			Evaluate ("using System; using System.Linq; using System.Collections.Generic; using System.Collections;");
		}

		static void InitTerminal ()
		{
			isatty = UnixUtils.isatty (0) && UnixUtils.isatty (1);

			// Work around, since Console is not accounting for
			// cursor position when writing to Stderr.  It also
			// has the undesirable side effect of making
			// errors plain, with no coloring.
			Report.Stderr = Console.Out;
			SetupConsole ();

			if (isatty)
				Console.WriteLine ("Mono C# Shell, type \"help;\" for help\n\nEnter statements below.");

		}

		static void LoadStartupFiles ()
		{
			string dir = Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
				"csharp");
			if (!Directory.Exists (dir))
				return;

			foreach (string file in Directory.GetFiles (dir)){
				string l = file.ToLower ();
				
				if (l.EndsWith (".cs")){
					try {
						using (StreamReader r = File.OpenText (file)){
							ReadEvalPrintLoopWith (p => r.ReadLine ());
						}
					} catch {
					}
				} else if (l.EndsWith (".dll")){
					Evaluator.LoadAssembly (file);
				}
			}
		}

		static void ReadEvalPrintLoopWith (ReadLiner readline)
		{
			string expr = null;
			while (true){
				string input = readline (expr == null);
				if (input == null)
					return;

				if (input == "")
					continue;

				expr = expr == null ? input : expr + "\n" + input;
				
				expr = Evaluate (expr);
			} 
		}

		static public int ReadEvalPrintLoop ()
		{
			InitTerminal ();

			InitializeUsing ();

			LoadStartupFiles ();
			ReadEvalPrintLoopWith (GetLine);

			return 0;
		}

		static string Evaluate (string input)
		{
			bool result_set;
			object result;

			try {
				input = Evaluator.Evaluate (input, out result, out result_set);

				if (result_set){
					PrettyPrint (result);
					Console.WriteLine ();
				}
			} catch (Exception e){
				Console.WriteLine (e);
			}
			
			return input;
		}

		static void p (string s)
		{
			Console.Write (s);
		}

		static string EscapeString (string s)
		{
			return s.Replace ("\"", "\\\"");
		}
		
		static void PrettyPrint (object result)
		{
			if (result == null){
				p ("null");
				return;
			}
			
			if (result is Array){
				Array a = (Array) result;
				
				p ("{ ");
				int top = a.GetUpperBound (0);
				for (int i = a.GetLowerBound (0); i <= top; i++){
					PrettyPrint (a.GetValue (i));
					if (i != top)
						p (", ");
				}
				p (" }");
			} else if (result is bool){
				if ((bool) result)
					p ("true");
				else
					p ("false");
			} else if (result is string){
				p (String.Format ("\"{0}\"", EscapeString ((string)result)));
			} else if (result is IDictionary){
				IDictionary dict = (IDictionary) result;
				int top = dict.Count, count = 0;
				
				p ("{");
				foreach (DictionaryEntry entry in dict){
					count++;
					p ("{ ");
					PrettyPrint (entry.Key);
					p (", ");
					PrettyPrint (entry.Value);
					if (count != top)
						p (" }, ");
					else
						p (" }");
				}
				p ("}");
			} else if (result is IEnumerable) {
				int i = 0;
				p ("{ ");
				foreach (object item in (IEnumerable) result) {
					if (i++ != 0)
						p (", ");

					PrettyPrint (item);
				}
				p (" }");
			} else {
				p (result.ToString ());
			}
		}

		static int Main (string [] args)
		{
			if (args.Length > 0 && args [0] == "--attach") {
				new AttachedCSharpShell (Int32.Parse (args [1]));
				return 0;
			} else if (args.Length > 0 && args [0].StartsWith ("--agent")) {
				new CSharpAgent (args [0]);
				return 0;
			}

			try {
				Evaluator.Init (args);
			} catch {
				return 1;
			}
			
			return ReadEvalPrintLoop ();
		}
	}
}

/*
 * A shell connected to a CSharpAgent running in a remote process.
 * FIXME:
 * - using NOT.EXISTS works, but leads to an error later when mcs tries to search
 *   that namespace.
 * - it would be nice to provide some kind of autocompletion even in remote mode.
 * - maybe add 'class_name' and 'method_name' arguments to LoadAgent.
 */
class AttachedCSharpShell {

	public AttachedCSharpShell (int pid) {
		/* Create a server socket we listen on whose address is passed to the agent */
		TcpListener listener = new TcpListener (new IPEndPoint (IPAddress.Loopback, 0));
		listener.Start ();

		string agent_assembly = typeof (AttachedCSharpShell).Assembly.Location;
		string agent_arg = "--agent:" + ((IPEndPoint)listener.Server.LocalEndPoint).Port;

		VirtualMachine vm = new VirtualMachine (pid);
		vm.Attach (agent_assembly, agent_arg);

		/* Wait for the client to connect */
		TcpClient client = listener.AcceptTcpClient ();
		NetworkStream s = client.GetStream ();
		StreamReader sr = new StreamReader (s);
		StreamWriter sw = new StreamWriter (s);

		Console.WriteLine ("Connected.");

		InitTerminal ();

		sw.WriteLine ("using System; using System.Linq; using System.Collections.Generic; using System.Collections;");
		sw.Flush ();
		/* Read result */
		while (true) {
			string line = sr.ReadLine ();
			if (line == "<END>")
				break;
		}

		//LoadStartupFiles ();

		string expr = "";
		bool eof = false;
		while (!eof) {
			string input = GetLine (expr == "");
			if (input == null)
				break;

			if (input == "")
				continue;

			sw.WriteLine (input);
			sw.Flush ();

			/* Read the (possible) error messages */
			while (true) {
				string line = sr.ReadLine ();
				if (line == null) {
					eof = true;
					break;
				}
				if (line == "<RESULT>")
					break;
				else
					// FIXME: Colorize
					Console.WriteLine (line);
			}
			/* Read the result */
			while (true) {
				string line = sr.ReadLine ();
				if (line == null) {
					eof = true;
					break;
				}
				if (line == "<INPUT>")
					break;
				else
					Console.WriteLine (line);
			}
			/* Read the (possible) incomplete input */
			expr = "";
			while (true) {
				string line = sr.ReadLine ();
				if (line == null) {
					eof = true;
					break;
				}
				if (line == "<END>")
					break;
				else
					expr += line;
			}
		}
	}

	static bool isatty = true;
		
	static Mono.Terminal.LineEditor editor;
	static bool dumb;

	static void ConsoleInterrupt (object sender, ConsoleCancelEventArgs a)
	{
		// Do not about our program
		a.Cancel = true;

		Mono.CSharp.Evaluator.Interrupt ();
    }
		
	static void SetupConsole ()
	{
		string term = Environment.GetEnvironmentVariable ("TERM");
		dumb = term == "dumb" || term == null || isatty == false;
			
		editor = new Mono.Terminal.LineEditor ("csharp", 300);
		Console.CancelKeyPress += ConsoleInterrupt;
    }

	static string GetLine (bool primary)
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

	static void InitTerminal ()
	{
		isatty = UnixUtils.isatty (0) && UnixUtils.isatty (1);

		SetupConsole ();

		if (isatty)
			Console.WriteLine ("Mono C# Shell, type \"help;\" for help\n\nEnter statements below.");
    }
}

namespace Mono.Management
{
	interface IVirtualMachine {
		void LoadAgent (string filename, string args);
	}
}

/*
 * This is the agent loaded into the target process when using --attach.
 */
class CSharpAgent
{
	public CSharpAgent (String arg) {
		new Thread (new ParameterizedThreadStart (Run)).Start (arg);
	}

	public void Run (object o) {
		string arg = (string)o;
		int port = Int32.Parse (arg.Substring (arg.IndexOf (":") + 1));

		Console.WriteLine ("csharp-agent: started, connecting to localhost:" + port);

		TcpClient client = new TcpClient ("127.0.0.1", port);
		Console.WriteLine ("csharp-agent: connected.");

		NetworkStream s = client.GetStream ();

		try {
			Evaluator.Init (new string [0]);
		} catch {
			return;
		}

		try {
			// Add all assemblies loaded later
			AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoaded;

			// Add all currently loaded assemblies
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies ())
				Evaluator.ReferenceAssembly (a);

			RunRepl (s);
		} finally {
			AppDomain.CurrentDomain.AssemblyLoad -= AssemblyLoaded;
			client.Close ();
			Console.WriteLine ("csharp-agent: disconnected.");			
		}
	}

	static void AssemblyLoaded (object sender, AssemblyLoadEventArgs e) {
		Evaluator.ReferenceAssembly (e.LoadedAssembly);
	}

	public void RunRepl (NetworkStream s) {
		StreamReader r = new StreamReader (s);
		StreamWriter w = new StreamWriter (s);
		string input = null;

		Report.Stderr = w;

		while (true) {
			try {
				string line = r.ReadLine ();

				bool result_set;
				object result;

				if (input == null)
					input = line;
				else
					input = input + "\n" + line;

				// This will print any error messages to w
				input = Evaluator.Evaluate (input, out result, out result_set);

				// FIXME: Emit XML

				// This separates the result from the possible error messages
				w.WriteLine ("<RESULT>");
				if (result_set) {
					if (result == null)
						w.Write ("null");
					else
						w.Write (result.ToString ());
					w.WriteLine ();
				}
				// FIXME: This might occur in the output as well.
				w.WriteLine ("<INPUT>");
				/* The rest of the input */
				w.WriteLine (input);
				w.WriteLine ("<END>");
				w.Flush ();
			} catch (IOException) {
				break;
			} catch (Exception e){
				Console.WriteLine (e);
			}
		}
	}
}