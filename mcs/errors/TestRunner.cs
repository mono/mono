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

		static ArrayList know_issues = new ArrayList ();
		static ArrayList regression = new ArrayList ();

		static int Main(string[] args) {
			if (args.Length != 3) {
				Console.WriteLine ("Usage: TestRunner test-dir compiler know-issues");
				return 1;
			}

			string test_directory = args [0];
			string mcs = args [1];
			string issue_file = args [2];

			string wrong_errors_file = Path.Combine (test_directory, issue_file);
			string[] files = Directory.GetFiles (test_directory, "cs*.cs");

			ReadWrongErrors (wrong_errors_file);
			ITester tester;
			try {
				tester = new ReflectionTester (Assembly.LoadFile (mcs));
			}
			catch (Exception) {
				Console.Error.WriteLine ("Switching to command line mode (compiler entry point was not found)");
				if (!File.Exists (mcs)) {
					Console.WriteLine ("ERROR: Tested compiler was not been found");
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
			    
				Console.Write (filename);

				string[] extra = GetExtraOptions (s);
				if (extra != null) {
					test_args = new string [1 + extra.Length];
					extra.CopyTo (test_args, 0);
				} else {
					test_args = new string [1];
				}
				test_args [test_args.Length - 1] = s;

				Console.Write ("...\t");
				try {

					bool result = tester.Invoke (test_args);
					if (result) {
						PrintFailed (filename);
						continue;
					}

					int end_char = filename.IndexOfAny (new char [] { '-', '.' } );
					string expected = filename.Substring (2, end_char - 2);
					if (CheckCompilerError (expected, tester.Output)) {
						success++;

						if (know_issues.Contains (s)) {
							Console.WriteLine ("FIXED ISSUE");
							continue;
						}
						Console.WriteLine ("OK");
						continue;
					}
					PrintFailed (filename);
					Console.WriteLine (tester.Output);
				}
				catch (Exception e) {
					PrintFailed (filename);
					Console.WriteLine (e.ToString ());
				}
			}

			Console.WriteLine ("Done" + Environment.NewLine);
			Console.WriteLine ("{0} correctly detected error cases ({1:.##%})", success, (float) (success) / (float)total);
			if (know_issues.Count > 0) {
				Console.WriteLine ();
				Console.WriteLine (issue_file + " contains already fixed issues. Please remove");
				foreach (string s in know_issues)
					Console.WriteLine (s);
			}
			if (regression.Count > 0) {
				Console.WriteLine ();
				Console.WriteLine ("The latest changes caused regression in {0} file(s)", regression.Count);
				foreach (string s in regression)
					Console.WriteLine (s);
			}

			return 0;
		}

		static void ReadWrongErrors (string file) {
			using (StreamReader sr = new StreamReader (file)) {
				String line;
				while ((line = sr.ReadLine()) != null) {
					if (line.StartsWith ("#"))
						continue;

					string file_name = line.Split (' ')[0];
					if (file_name.Length == 0)
						continue;
					know_issues.Add (file_name);
				}
			}
		}

		static void PrintFailed (string file) {
			if (know_issues.Contains (file)) {
				Console.WriteLine ("KNOW ISSUE");
				know_issues.Remove (file);
				return;
			}
			Console.WriteLine ("REGRESSION");
			regression.Add (file);
		}

		static bool CheckCompilerError (string expected, string buffer) {
			string tested_text = "error CS" + expected;
			StringReader sr = new StringReader (buffer);
			string line = sr.ReadLine ();
			int row = 0;
			while (line != null) {
				row++;
				if (line.IndexOf (tested_text) != -1) {
					//if (row > 1)
					//	Console.WriteLine ("WARNING: Not reported as primary error");
					return true;
				}
				line = sr.ReadLine ();
			}
			
			return false;
		}

		static string[] GetExtraOptions (string file) {
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
