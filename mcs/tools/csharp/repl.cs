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
using Mono.CSharp;

namespace Mono {

	public static class CSharpShell {
		static bool isatty = true;
		
		static Mono.Terminal.LineEditor editor;
		static bool dumb;
		static Thread invoke_thread;

		static void ConsoleInterrupt (object sender, ConsoleCancelEventArgs a)
		{
			// Do not about our program
			a.Cancel = true;

			Mono.CSharp.CSharpEvaluator.Interrupt ();
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
					CSharpEvaluator.LoadAssembly (file);
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
				input = CSharpEvaluator.Evaluate (input, out result, out result_set);

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
			try {
				CSharpEvaluator.Init (args);
			} catch {
				return 1;
			}
			
			return ReadEvalPrintLoop ();
		}
	}

}
