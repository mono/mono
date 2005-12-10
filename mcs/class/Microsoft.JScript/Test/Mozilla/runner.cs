using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace JSTestRunner {
	class Program {
		struct Config {
			public bool is_win32;
			public string preamble;
			public string test_file;
			public string fail_file;
			public bool full_run;
			public string test_dir;
			public string compile_cmd;
			public string compile_args;
			public string run_cmd;
			public string run_args;
		}

		static Config config;
		static string preamble;
		static string [] fail_tests;
		static string [] tests;

		static void Main (string [] args)
		{
			LoadConfig ();
			ReadFiles ();

			// Can specify test to run as first argument
			if (args.Length == 1) {
				string arg = args [0];
	
				if (arg == "--full-run" || arg == "-f")
					config.full_run = true;
				else
					tests = new string [] { args [0] };
			}

			foreach (string test in tests) {
				if (File.Exists (test))
					RunTest (test);
				else
					Console.WriteLine ("WARNING: Test `{0}' doesn't exist.", test);
			}
		}

		static void LoadConfig ()
		{
			IDictionary env = Environment.GetEnvironmentVariables ();
			config = new Config ();
			config.is_win32 = Environment.OSVersion.ToString ().IndexOf ("Windows") != -1;
			
			config.preamble = env ["MJS_TEST_PREAMBLE"] as string;
			if (config.preamble == null)
				config.preamble = "mjs.preamble";

			config.test_file = env ["MJS_TEST_FILE"] as string;
			if (config.test_file == null)
				config.test_file = "mjs-most.tests";

			config.fail_file = env ["MJS_FAIL_FILE"] as string;
			if (config.fail_file == null)
				config.fail_file = "mjs-most.fail";

			config.full_run = false;
			config.test_dir = env ["MJS_TEST_DIR"] as string;
			if (config.test_dir == null)
				config.test_dir = config.is_win32 ? env ["TEMP"] as string : "."; 

			config.compile_cmd = env ["MJS_COMPILE_CMD"] as string;
			if (config.compile_cmd == null)
				config.compile_cmd = config.is_win32 ? "bash" : "mjs";

			string compile_str = env ["MJS_COMPILE_ARGS"] as string;
			if (compile_str == null)
				compile_str = config.is_win32 ? "mjs {0}" : "{0}";
			config.compile_args = String.Format (compile_str, "jstest.js");

			config.run_cmd = env ["MJS_RUN_CMD"] as string;
			if (config.run_cmd == null)
				config.run_cmd = "mono";

			string run_str = env ["MJS_RUN_ARGS"] as string;
			if (run_str == null)
				run_str = "{0}";
			config.run_args = String.Format (run_str, "jstest.exe");
		}

		static void ReadFiles ()
		{
			StreamReader preamble_io = File.OpenText (config.preamble);
			try {
				preamble = preamble_io.ReadToEnd ();
			} finally {
				preamble_io.Close ();
			}

			StreamReader fail_io = File.OpenText (config.fail_file);
			try {
				fail_tests = FilterTextToLines (fail_io.ReadToEnd ());
			} finally {
				fail_io.Close ();
			}

			StreamReader test_io = File.OpenText (config.test_file);
			try {
				tests = FilterTextToLines (test_io.ReadToEnd ());
			} finally {
				test_io.Close ();
			}
		}

		// Filters whitespace and comment lines
		static string [] FilterTextToLines (string text)
		{
			ArrayList result = new ArrayList ();
			foreach (string line in Regex.Split (text, @"\r?\n")) {
				string t_line = line.Trim ();
				if ((t_line != "") && (t_line [0] != '#'))
					result.Add (t_line);
			}
			return (string []) result.ToArray (typeof (string));
		}

		class ReplaceEval {
			public StringBuilder eval_funs;
			int eval_fun_count = 0;

			public ReplaceEval ()
			{
				eval_funs = new StringBuilder ();
			}

			public string replace_eval (Match match)
			{
				string body = ProcessEscapes (match.Groups [2].Value);

				// Try to insert a return statement before a semicolon or closing curly brace
				string new_body = Regex.Replace (body, "^(.+)([;}])(.+)$", "$1$2 return $3", RegexOptions.Singleline);
				// No return inserted yet? Prepend to whole body.
				if (body == new_body)
					new_body = "return " + body;

				string fun_name = String.Format ("eval_fun_{0}", eval_fun_count++);
				eval_funs.AppendFormat ("function {0}() {{ {1} }}{2}", fun_name, new_body, Environment.NewLine);

				return fun_name + "()";
			}
		}

		static void RunTest (string test)
		{
			string target = Path.Combine (config.test_dir, "jstest.js");

			ReplaceEval repl_eval = new ReplaceEval ();
			MatchEvaluator replace_eval = new MatchEvaluator (repl_eval.replace_eval);

			StreamReader source_io = File.OpenText (test);
			string source;
			try {
				source = source_io.ReadToEnd ();
			} finally {
				source_io.Close ();
			}

			source = Regex.Replace (source, @"eval\s*\(([""'])(.+?[^\\]?)\1\s*\)", replace_eval).
				Replace ("new TestCase", "TestCase");
			string eval_funs = repl_eval.eval_funs.ToString ();

			StreamWriter target_io = File.CreateText (target);
			try {
				target_io.WriteLine (preamble);
				target_io.WriteLine (eval_funs);
				target_io.WriteLine (source);
			} finally {
				target_io.Close ();
			}

			// Luckily, we don't have to deal with multiple threads here...
			string old_dir = Environment.CurrentDirectory;
			Environment.CurrentDirectory = config.test_dir;
			{
				Console.WriteLine ();
				Console.WriteLine ("Running {0}...", test);

				ProcessStartInfo compiler_info = new ProcessStartInfo (config.compile_cmd, config.compile_args);
				compiler_info.CreateNoWindow = true;
				compiler_info.UseShellExecute = false;
				compiler_info.RedirectStandardOutput = true;
				compiler_info.RedirectStandardError = true;
				Process compiler = Process.Start (compiler_info);
				compiler.WaitForExit ();
				Console.Write (compiler.StandardOutput.ReadToEnd ());
				Console.Write (compiler.StandardError.ReadToEnd ());
				if (compiler.ExitCode != 0 || !File.Exists ("jstest.exe")) {
					Failed (test, String.Format ("compiler aborted with exit code {0}", compiler.ExitCode));
					goto done;
				}

				ProcessStartInfo runtime_info = new ProcessStartInfo (config.run_cmd, config.run_args);
				runtime_info.CreateNoWindow = true;
				runtime_info.UseShellExecute = false;
				runtime_info.RedirectStandardOutput = true;
				runtime_info.RedirectStandardError = true;
				Process runtime = Process.Start (runtime_info);
				// For some reason we need the timeout here even if the runtime does not need longer than it...
				runtime.WaitForExit (5000);

				Console.Write (runtime.StandardOutput.ReadToEnd ());
				Console.Write (runtime.StandardError.ReadToEnd ());
				if (runtime.ExitCode != 0)
					Failed (test, String.Format ("runtime aborted with exit code {0}", runtime.ExitCode));
				File.Delete ("jstest.exe");
			}

			done:
			Environment.CurrentDirectory = old_dir;
		}

		static string ProcessEscapeMatch (Match match)
		{
			string escape = match.Groups [1].Value;

			switch (escape) {
			case "n":
				return "\n";
			case "r":
				return "\r";
			case "'":
				return "'";
			case "\"":
				return "\"";
			case "\\":
				return "\\";
			default:
				return "\\" + escape;
			}
		}

		static string ProcessEscapes (String text)
		{
			MatchEvaluator process = new MatchEvaluator (ProcessEscapeMatch);
			return Regex.Replace (text, @"\\(.)", process);
		}

		static void Failed (string test, string reason)
		{			
			bool fail_expected = Array.IndexOf (fail_tests, test) != -1;
			Console.WriteLine ("Failed {0}: {1}!", test, reason);
			Console.WriteLine (); Console.WriteLine (); Console.WriteLine ();
			if (!fail_expected) {
				Console.WriteLine ("CRITICAL: Unexpected failure of {0}. Aborting run.", test);
				if (!config.full_run)
					Environment.Exit (1);
			}
		}
	}
}
