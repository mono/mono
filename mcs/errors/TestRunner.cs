using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Collections;

namespace TestRunner {

	interface ITester
	{
		string Output { get; }
		bool Invoke (string[] args);
	}

	class ReflectionTester: ITester {
		MethodInfo ep;
		object[] method_arg;
		StringWriter output;

		public ReflectionTester (Assembly a)
		{
			ep = a.GetType ("Mono.CSharp.CompilerCallableEntryPoint").GetMethod ("InvokeCompiler", 
				BindingFlags.Static | BindingFlags.Public);
			if (ep == null)
				throw new MissingMethodException ("static InvokeCompiler");
			method_arg = new object [1];
		}

		public string Output {
			get {
				return output.GetStringBuilder ().ToString ();
			}
		}

		public bool Invoke(string[] args)
		{
			TextWriter old_writer = Console.Error;
			output = new StringWriter ();
			Console.SetError (output);
			method_arg [0] = args;
			try {
				return (bool)ep.Invoke (null, method_arg);
			}
			finally {
				Console.SetError (old_writer);
			}
		}
	}

	class ProcessTester: ITester
	{
		ProcessStartInfo pi;
		string output;

		public ProcessTester (string p_path)
		{
			pi = new ProcessStartInfo ();
			pi.FileName = p_path;
			pi.CreateNoWindow = true;
			pi.WindowStyle = ProcessWindowStyle.Hidden;
			pi.RedirectStandardOutput = true;
			pi.RedirectStandardError = true;
			pi.UseShellExecute = false;
		}

		public string Output {
			get {
				return output;
			}
		}

		public bool Invoke(string[] args)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (string s in args) {
				sb.Append (s);
				sb.Append (" ");
			}
			pi.Arguments = sb.ToString ();
			Process p = Process.Start (pi);
			output = p.StandardError.ReadToEnd ();
			p.WaitForExit ();
			return p.ExitCode == 0;
		}
	}

	class Tester {

		enum CompilerError
		{
			Expected,
			Wrong,
			Missing
		}

		static ArrayList know_issues = new ArrayList ();
		static ArrayList ignore_list = new ArrayList ();
		static ArrayList no_error_list = new ArrayList ();
		static ArrayList regression = new ArrayList ();

		static StreamWriter log_file;

		static void Log (string msg, params object [] rest)
		{
			Console.Write (msg, rest);
			log_file.Write (msg, rest);
		}

		static void LogLine ()
		{
			Console.WriteLine ();
			log_file.WriteLine ();
		}

		static void LogLine (string msg, params object [] rest)
		{
			Console.WriteLine (msg, rest);
			log_file.WriteLine (msg, rest);
		}

		static int Main(string[] args) {
			if (args.Length != 4) {
				Console.Error.WriteLine ("Usage: TestRunner (test-pattern|compiler-name) compiler know-issues log-file");
				return 1;
			}

			string test_pattern = args [0];
			string mcs = args [1];
			string issue_file = args [2];
			string log_fname = args [3];

			log_file = new StreamWriter (log_fname, false);

			// THIS IS BUG #73763 workaround
			if (test_pattern == "gmcs")
				test_pattern = "*cs*.cs";
			else
				test_pattern = "cs*.cs";

			string wrong_errors_file = issue_file;
			string[] files = Directory.GetFiles (".", test_pattern);

			ReadWrongErrors (wrong_errors_file);
			ITester tester;
			try {
				tester = new ReflectionTester (Assembly.LoadFile (mcs));
			}
			catch (Exception) {
				Console.Error.WriteLine ("Switching to command line mode (compiler entry point was not found)");
				if (!File.Exists (mcs)) {
					Console.Error.WriteLine ("ERROR: Tested compiler was not found");
					return 1;
				}
				tester = new ProcessTester (mcs);
			}

			string[] test_args;
			int success = 0;
			int total = files.Length;
			foreach (string s in files) {
				string filename = Path.GetFileName (s);
				if (filename.StartsWith ("CS")) { // Windows hack
					total--;
					continue;
				}
			    
				Log (filename);

				string[] extra = GetExtraOptions (s);
				if (extra != null) {
					test_args = new string [1 + extra.Length];
					extra.CopyTo (test_args, 0);
				} else {
					test_args = new string [1];
				}
				test_args [test_args.Length - 1] = s;

				Log ("...\t");

				if (ignore_list.Contains (filename)) {
					LogLine ("NOT TESTED");
					total--;
					continue;
				}

				try {
					int start_char = 0;
					while (Char.IsLetter (filename, start_char))
						++start_char;

					int end_char = filename.IndexOfAny (new char [] { '-', '.' } );
					string expected = filename.Substring (start_char, end_char - start_char);

					bool result = tester.Invoke (test_args);
					if (result) {
						HandleFailure (filename, CompilerError.Missing);
						continue;
					}

					CompilerError result_code = GetCompilerError (expected, tester.Output);
					if (HandleFailure (filename, result_code)) {
						success++;
					} else {
						LogLine (tester.Output);
					}
				}
				catch (Exception e) {
					HandleFailure (filename, CompilerError.Missing);
					Log (e.ToString ());
				}
			}

			LogLine ("Done" + Environment.NewLine);
			LogLine ("{0} correctly detected error cases ({1:.##%})", success, (float) (success) / (float)total);

			know_issues.AddRange (no_error_list);
			if (know_issues.Count > 0) {
				LogLine ();
				LogLine (issue_file + " contains {0} already fixed issues. Please remove", know_issues.Count);
				foreach (string s in know_issues)
					LogLine (s);
			}
			if (regression.Count > 0) {
				LogLine ();
				LogLine ("The latest changes caused regression in {0} file(s)", regression.Count);
				foreach (string s in regression)
					LogLine (s);
			}

			log_file.Close ();

			return regression.Count == 0 ? 0 : 1;
		}

		static void ReadWrongErrors (string file)
		{
			const string ignored = "IGNORE";
			const string no_error = "NO ERROR";

			using (StreamReader sr = new StreamReader (file)) {
				string line;
				while ((line = sr.ReadLine()) != null) {
					if (line.StartsWith ("#"))
						continue;

					ArrayList active_cont = know_issues;

					if (line.IndexOf (ignored) > 0)
						active_cont = ignore_list;
					else if (line.IndexOf (no_error) > 0)
						active_cont = no_error_list;

					string file_name = line.Split (' ')[0];
					if (file_name.Length == 0)
						continue;

					active_cont.Add (file_name);
				}
			}
		}

		static bool HandleFailure (string file, CompilerError status)
		{
			switch (status) {
				case CompilerError.Expected:
					if (know_issues.Contains (file) || no_error_list.Contains (file)) {
						LogLine ("FIXED ISSUE");
						return true;
					}
					LogLine ("OK");
					return true;

				case CompilerError.Wrong:
					if (know_issues.Contains (file)) {
						LogLine ("KNOWN ISSUE");
						know_issues.Remove (file);
						return false;
					}
					if (no_error_list.Contains (file)) {
						LogLine ("REGRESSION (NO ERROR -> WRONG ERROR)");
						no_error_list.Remove (file);
					}
					else {
						LogLine ("REGRESSION (CORRECT ERROR -> WRONG ERROR)");
					}

					break;

				case CompilerError.Missing:
					if (no_error_list.Contains (file)) {
						LogLine ("KNOW ISSUE");
						no_error_list.Remove (file);
						return false;
					}

					if (know_issues.Contains (file)) {
						LogLine ("REGRESSION (WRONG ERROR -> NO ERROR)");
						know_issues.Remove (file);
					}
					else {
						LogLine ("REGRESSION (CORRECT ERROR -> NO ERROR)");
					}

					break;
			}

			regression.Add (file);
			return false;
		}

		static CompilerError GetCompilerError (string expected, string buffer)
		{
			const string error_prefix = "CS";
			const string ignored_error = "error CS5001";
			string tested_text = "error " + error_prefix + expected;
			StringReader sr = new StringReader (buffer);
			string line = sr.ReadLine ();
			bool any_error = false;
			while (line != null) {

				if (line.IndexOf (tested_text) != -1)
					return CompilerError.Expected;

				if (line.IndexOf (error_prefix) != -1 &&
					line.IndexOf (ignored_error) == -1)
					any_error = true;

				line = sr.ReadLine ();
			}
			
			return any_error ? CompilerError.Wrong : CompilerError.Missing;
		}

		static string[] GetExtraOptions (string file)
		{
			const string options = "// Compiler options:";

			int row = 0;
			using (StreamReader sr = new StreamReader (file)) {
				String line;
				while (row++ < 3 && (line = sr.ReadLine()) != null) {
					int index = line.IndexOf (options);
					if (index != -1) {
						string[] o = line.Substring (index + options.Length).Split (' ');
						for (int i = 0; i < o.Length; i++)
							o [i] = o[i].TrimStart ();
						return o;
					}				
				}
			}
			return null;
		}			
	}
}
