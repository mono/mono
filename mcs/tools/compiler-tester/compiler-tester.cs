//
// compiler-tester.cs
//
// Author:
//   Marek Safar (marek.safar@gmail.com)
//

//
// Copyright (C) 2008, 2009 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Xml;
using System.Collections.Generic;

namespace TestRunner {

	interface ITester
	{
		string Output { get; }
		bool Invoke (string[] args);
		bool IsWarning (int warningNumber);
	}

	class ReflectionTester: ITester {
		MethodInfo ep;
		object[] method_arg;
		StringWriter output;
		int[] all_warnings;

		public ReflectionTester (Assembly a)
		{
			Type t = a.GetType ("Mono.CSharp.CompilerCallableEntryPoint");

			if (t == null)
				Console.Error.WriteLine ("null, huh?");

			ep = t.GetMethod ("InvokeCompiler", 
				BindingFlags.Static | BindingFlags.Public);
			if (ep == null)
				throw new MissingMethodException ("static InvokeCompiler");
			method_arg = new object [2];

			PropertyInfo pi = t.GetProperty ("AllWarningNumbers");
			all_warnings = (int[])pi.GetValue (null, null);
			Array.Sort (all_warnings);
		}

		public string Output {
			get {
				return output.GetStringBuilder ().ToString ();
			}
		}

		public bool Invoke(string[] args)
		{
			output = new StringWriter ();
			method_arg [0] = args;
			method_arg [1] = output;
			return (bool)ep.Invoke (null, method_arg);
		}

		public bool IsWarning (int warningNumber)
		{
			return Array.BinarySearch (all_warnings, warningNumber) >= 0;
		}
	}

#if !NET_2_1
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
			StringBuilder sb = new StringBuilder ("/nologo ");
			foreach (string s in args) {
				sb.Append (s);
				sb.Append (" ");
			}
			pi.Arguments = sb.ToString ();
			Process p = Process.Start (pi);
			output = p.StandardError.ReadToEnd ();
			if (output.Length == 0)
			    output = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();
			return p.ExitCode == 0;
		}

		public bool IsWarning (int warningNumber)
		{
			throw new NotImplementedException ();
		}
	}
#endif

	class TestCase : MarshalByRefObject
	{
		public readonly string FileName;
		public readonly string[] CompilerOptions;
		public readonly string[] Dependencies;

		public TestCase (string filename, string[] options, string[] deps)
		{
			this.FileName = filename;
			this.CompilerOptions = options;
			this.Dependencies = deps;
		}
	}

	class PositiveTestCase : TestCase
	{
		public class VerificationData : MarshalByRefObject
		{
			public class MethodData : MarshalByRefObject
			{
				public MethodData (MethodBase mi, int il_size)
				{
					this.Type = mi.DeclaringType.ToString ();
					this.MethodName = mi.ToString ();
					this.ILSize = il_size;
				}

				public MethodData (string type_name, string method_name, int il_size)
				{
					this.Type = type_name;
					this.MethodName = method_name;
					this.ILSize = il_size;
				}

				public string Type;
				public string MethodName;
				public int ILSize;
				public bool Checked;
			}

			ArrayList methods;
			public bool IsNewSet;

			public VerificationData (string test_file)
			{
#if NET_2_0
				this.test_file = test_file;
#endif				
			}

#if NET_2_0
			string test_file;

			public static VerificationData FromFile (string name, XmlReader r)
			{
				VerificationData tc = new VerificationData (name);
				ArrayList methods = new ArrayList ();
				r.Read ();
				while (r.ReadToNextSibling ("type")) {
					string type_name = r ["name"];
					r.Read ();
					while (r.ReadToNextSibling ("method")) {
						string m_name = r ["name"];

						r.ReadToDescendant ("size");
						int il_size = r.ReadElementContentAsInt ();
						methods.Add (new MethodData (type_name, m_name, il_size));
						r.Read ();
					}
					r.Read ();
				}

				tc.methods = methods;
				return tc;
			}

			public void WriteCodeInfoTo (XmlWriter w)
			{
				w.WriteStartElement ("test");
				w.WriteAttributeString ("name", test_file);

				string type = null;
				foreach (MethodData data in methods) {
					if (!data.Checked)
						continue;

					if (type != data.Type) {
						if (type != null)
							w.WriteEndElement ();

						type = data.Type;
						w.WriteStartElement ("type");
						w.WriteAttributeString ("name", type);
					}

					w.WriteStartElement ("method");
					w.WriteAttributeString ("name", data.MethodName);
					w.WriteStartElement ("size");
					w.WriteValue (data.ILSize);
					w.WriteEndElement ();
					w.WriteEndElement ();
				}

				if (type != null)
					w.WriteEndElement ();

				w.WriteEndElement ();
			}
#endif

			public MethodData FindMethodData (string method_name, string declaring_type)
			{
				if (methods == null)
					return null;

				foreach (MethodData md in methods) {
					if (md.MethodName == method_name && md.Type == declaring_type)
						return md;
				}

				return null;
			}

			public void AddNewMethod (MethodBase mb, int il_size)
			{
				if (methods == null)
					methods = new ArrayList ();

				MethodData md = new MethodData (mb, il_size);
				md.Checked = true;
				methods.Add (md);
			}
		}

		VerificationData verif_data;

		public PositiveTestCase (string filename, string [] options, string [] deps)
			: base (filename, options, deps)
		{
		}

		public void CreateNewTest ()
		{
			verif_data = new VerificationData (FileName);
			verif_data.IsNewSet = true;
		}

		public VerificationData VerificationProvider {
			set {
				verif_data = value;
			}
			get {
				return verif_data;
			}
		}
	}

	class Checker: MarshalByRefObject, IDisposable
	{
		protected ITester tester;
		protected int success;
		protected int total;
		protected int ignored;
		protected int syntax_errors;
		string issue_file;
		StreamWriter log_file;
		protected string[] extra_compiler_options;
		// protected string[] compiler_options;
		// protected string[] dependencies;

		protected ArrayList tests = new ArrayList ();
		protected Hashtable test_hash = new Hashtable ();
		protected ArrayList regression = new ArrayList ();
		protected ArrayList know_issues = new ArrayList ();
		protected ArrayList ignore_list = new ArrayList ();
		protected ArrayList no_error_list = new ArrayList ();
		
		protected bool verbose;
		protected bool safe_execution;
			
		int total_known_issues;

		protected Checker (ITester tester)
		{
			this.tester = tester;
		}

		public string IssueFile {
			set {
				this.issue_file = value;
				ReadWrongErrors (issue_file);
			}
		}
		
		public string LogFile {
			set {
				this.log_file = new StreamWriter (value, false);
			}
		}

		public bool Verbose {
			set {
				verbose = value;
			}
		}

		public bool SafeExecution {
			set {
				safe_execution = value;
			}
		}

		public string[] ExtraCompilerOptions {
			set {
				extra_compiler_options = value;
			}
		}

		protected virtual bool GetExtraOptions (string file, out string[] compiler_options,
							out string[] dependencies)
		{
			int row = 0;
			compiler_options = null;
			dependencies = null;
			try {
				using (StreamReader sr = new StreamReader (file)) {
					String line;
					while (row++ < 3 && (line = sr.ReadLine()) != null) {
						if (!AnalyzeTestFile (file, ref row, line, ref compiler_options,
								      ref dependencies))
							return false;
					}
				}
			} catch {
				return false;
			}
			return true;
		}

		protected virtual bool AnalyzeTestFile (string file, ref int row, string line,
							ref string[] compiler_options,
							ref string[] dependencies)
		{
			const string options = "// Compiler options:";
			const string depends = "// Dependencies:";

			if (row == 1) {
				compiler_options = null;
				dependencies = null;
			}

			int index = line.IndexOf (options);
			if (index != -1) {
				compiler_options = line.Substring (index + options.Length).Trim().Split (' ');
				for (int i = 0; i < compiler_options.Length; i++)
					compiler_options[i] = compiler_options[i].TrimStart ();
			}
			index = line.IndexOf (depends);
			if (index != -1) {
				dependencies = line.Substring (index + depends.Length).Trim().Split (' ');
				for (int i = 0; i < dependencies.Length; i++)
					dependencies[i] = dependencies[i].TrimStart ();
			}

			return true;
		}

		public bool Do (string filename)
		{
			if (test_hash.Contains (filename))
				return true;

			if (verbose)
				Log (filename + "...\t");

			if (ignore_list.Contains (filename)) {
				++ignored;
				LogFileLine (filename, "NOT TESTED");
				return false;
			}

			string[] compiler_options, dependencies;
			if (!GetExtraOptions (filename, out compiler_options, out dependencies)) {
				LogFileLine (filename, "ERROR");
				return false;
			}

			if (extra_compiler_options != null) {
				if (compiler_options == null)
					compiler_options = extra_compiler_options;
				else {
					string[] new_options = new string [compiler_options.Length + extra_compiler_options.Length];
					extra_compiler_options.CopyTo (new_options, 0);
					compiler_options.CopyTo (new_options, extra_compiler_options.Length);
					compiler_options = new_options;
				}
			}

			TestCase test = CreateTestCase (filename, compiler_options, dependencies);
			test_hash.Add (filename, test);

			++total;
			if (dependencies != null) {
				foreach (string dependency in dependencies) {
					if (!Do (dependency)) {
						LogFileLine (filename, "DEPENDENCY FAILED");
						return false;
					}
				}
			}

			tests.Add (test);

			return Check (test);
		}

		protected virtual bool Check (TestCase test)
		{
			string[] test_args;

			if (test.CompilerOptions != null) {
				test_args = new string [2 + test.CompilerOptions.Length];
				test.CompilerOptions.CopyTo (test_args, 0);
			} else {
				test_args = new string [2];
			}
			test_args [test_args.Length - 2] = test.FileName;
			test_args [test_args.Length - 1] = "-debug";

			return tester.Invoke (test_args);
		}

		protected virtual TestCase CreateTestCase (string filename, string [] options, string [] deps)
		{
			return new TestCase (filename, options, deps);
		}

		void ReadWrongErrors (string file)
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
			total_known_issues = know_issues.Count;
		}

		protected virtual void PrintSummary ()
		{
			LogLine ("Done" + Environment.NewLine);
			float rate = 0;
			if (total > 0)
				rate = (float) (success) / (float)total;
			LogLine ("{0} test cases passed ({1:0.##%})", success, rate);

			if (syntax_errors > 0)
				LogLine ("{0} test(s) ignored because of wrong syntax !", syntax_errors);
				
			if (ignored > 0)
				LogLine ("{0} test(s) ignored", ignored);
			
			if (total_known_issues - know_issues.Count > 0)
				LogLine ("{0} known issue(s)", total_known_issues - know_issues.Count);

			know_issues.AddRange (no_error_list);
			if (know_issues.Count > 0) {
				LogLine ("");
				LogLine (issue_file + " contains {0} already fixed issues. Please remove", know_issues.Count);
				foreach (string s in know_issues)
					LogLine (s);
			}
			if (regression.Count > 0) {
				LogLine ("");
				LogLine ("The latest changes caused regression in {0} file(s)", regression.Count);
				foreach (string s in regression)
					LogLine (s);
			}
		}

		public int ResultCode
		{
			get {
				return regression.Count == 0 ? 0 : 1;
			}
		}

		protected void Log (string msg, params object [] rest)
		{
			Console.Write (msg, rest);
			if (log_file != null)
				log_file.Write (msg, rest);
		}

		protected void LogLine (string msg)
		{
			Console.WriteLine (msg);
			if (log_file != null)
				log_file.WriteLine (msg);
		}

		protected void LogLine (string msg, params object [] rest)
		{
			Console.WriteLine (msg, rest);
			if (log_file != null)
				log_file.WriteLine (msg, rest);
		}
		
		public void LogFileLine (string file, string msg, params object [] args)
		{
			string s = verbose ? 
				string.Format (msg, args) :
				file + "...\t" + string.Format (msg, args); 

			Console.WriteLine (s);
			if (log_file != null)
				log_file.WriteLine (s);
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (log_file != null)
				log_file.Close ();
		}

		#endregion

		public virtual void Initialize ()
		{
		}

		public virtual void CleanUp ()
		{
			PrintSummary ();
		}
	}

	class PositiveChecker: Checker
	{
		readonly string files_folder;
		readonly static object[] default_args = new object[1] { new string[] {} };
		string doc_output;
		string verif_file;
		bool update_verif_file;
		Hashtable verif_data;

#if !NET_2_1
		ProcessStartInfo pi;
#endif
		readonly string mono;

		public enum TestResult {
			CompileError,
			ExecError,
			LoadError,
			XmlError,
			Success,
			ILError
		}

		public PositiveChecker (ITester tester, string verif_file):
			base (tester)
		{
			files_folder = Directory.GetCurrentDirectory ();
			this.verif_file = verif_file;

#if !NET_2_1
			pi = new ProcessStartInfo ();
			pi.CreateNoWindow = true;
			pi.WindowStyle = ProcessWindowStyle.Hidden;
			pi.RedirectStandardOutput = true;
			pi.RedirectStandardError = true;
			pi.UseShellExecute = false;

			mono = Environment.GetEnvironmentVariable ("MONO_RUNTIME");
			if (mono != null) {
				pi.FileName = mono;
			}
#endif
		}

		public bool UpdateVerificationDataFile {
			set {
				update_verif_file = value;
			}
			get {
				return update_verif_file;
			}
		}

		protected override bool GetExtraOptions(string file, out string[] compiler_options,
							out string[] dependencies) {
			if (!base.GetExtraOptions (file, out compiler_options, out dependencies))
				return false;

			doc_output = null;
			if (compiler_options == null)
				return true;

			foreach (string one_opt in compiler_options) {
				if (one_opt.StartsWith ("-doc:")) {
					doc_output = one_opt.Split (':', '/')[1];
				}
			}
			return true;
		}

		class DomainTester : MarshalByRefObject
		{
			public bool CheckILSize (PositiveTestCase test, PositiveChecker checker, string file)
			{
				Assembly assembly = Assembly.LoadFile (file);

				bool success = true;
				Type[] types = assembly.GetTypes ();
				foreach (Type t in types) {

					// Skip interfaces
					if (!t.IsClass && !t.IsValueType)
						continue;

					if (test.VerificationProvider == null) {
						if (!checker.UpdateVerificationDataFile)
							checker.LogFileLine (test.FileName, "Missing IL verification data");
						test.CreateNewTest ();
					}

					foreach (MemberInfo m in t.GetMembers (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
						MethodBase mi = m as MethodBase;
						if (mi == null)
							continue;

						if ((mi.Attributes & (MethodAttributes.PinvokeImpl)) != 0)
							continue;

						success &= CompareIL (mi, test, checker);
					}
				}

				return success;
			}

			bool CompareIL (MethodBase mi, PositiveTestCase test, PositiveChecker checker)
			{
				string m_name = mi.ToString ();
				string decl_type = mi.DeclaringType.ToString ();
				PositiveTestCase.VerificationData data_provider = test.VerificationProvider;

				PositiveTestCase.VerificationData.MethodData md = data_provider.FindMethodData (m_name, decl_type);
				if (md == null) {
					data_provider.AddNewMethod (mi, GetILSize (mi));
					if (!data_provider.IsNewSet) {
						checker.HandleFailure (test.FileName, PositiveChecker.TestResult.ILError, decl_type + ": " + m_name + " (new method?)");
						return false;
					}

					return true;
				}

				if (md.Checked) {
					checker.HandleFailure (test.FileName, PositiveChecker.TestResult.ILError, decl_type + ": " + m_name + " has a duplicate");
					return false;
				}

				md.Checked = true;

				int il_size = GetILSize (mi);
				if (md.ILSize == il_size)
					return true;

				if (md.ILSize > il_size) {
					checker.LogFileLine (test.FileName, "{0} (code size reduction {1} -> {2})", m_name, md.ILSize, il_size);
					md.ILSize = il_size;
					return true;
				}

				checker.HandleFailure (test.FileName, PositiveChecker.TestResult.ILError,
					string.Format ("{0} (code size {1} -> {2})", m_name, md.ILSize, il_size));

				md.ILSize = il_size;

				return false;
			}

			static int GetILSize (MethodBase mi)
			{
#if NET_2_0
				MethodBody body = mi.GetMethodBody ();
				if (body != null)
					return body.GetILAsByteArray ().Length;
#endif
				return 0;
			}

			bool ExecuteFile (MethodInfo entry_point, string filename)
			{
				TextWriter stdout = Console.Out;
				TextWriter stderr = Console.Error;
				Console.SetOut (TextWriter.Null);
				Console.SetError (TextWriter.Null);
				ParameterInfo[] pi = entry_point.GetParameters ();
				object[] args = pi.Length == 0 ? null : default_args;

				object result = null;
				try {
					try {
						result = entry_point.Invoke (null, args);
					} finally {
						Console.SetOut (stdout);
						Console.SetError (stderr);
					}
				} catch (Exception e) {
					throw new ApplicationException (e.ToString ());
				}

				if (result is int && (int) result != 0)
					throw new ApplicationException ("Wrong return code: " + result.ToString ());

				return true;
			}

			public bool Test (string file)
			{
				Assembly assembly = Assembly.LoadFile (file);
				return ExecuteFile (assembly.EntryPoint, file);
			}
		}

		protected override bool Check(TestCase test)
		{
			string filename = test.FileName;
			try {
				if (!base.Check (test)) {
					HandleFailure (filename, TestResult.CompileError, tester.Output);
					return false;
				}
			}
			catch (Exception e) {
				if (e.InnerException != null)
					e = e.InnerException;
				
				HandleFailure (filename, TestResult.CompileError, e.ToString ());
				return false;
			}

			// Test setup
			if (filename.EndsWith ("-lib.cs") || filename.EndsWith ("-mod.cs")) {
				if (verbose)
					LogFileLine (filename, "OK");
				--total;
				return true;
			}

			string file = Path.Combine (files_folder, Path.GetFileNameWithoutExtension (filename) + ".exe");

			// Enable .dll only tests (no execution required)
			if (!File.Exists(file)) {
				HandleFailure (filename, TestResult.Success, null);
				return true;
			}

			AppDomain domain = null;
#if !NET_2_1
			if (safe_execution) {
				// Create a new AppDomain, with the current directory as the base.
				AppDomainSetup setupInfo = new AppDomainSetup ();
				setupInfo.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
				setupInfo.LoaderOptimization = LoaderOptimization.SingleDomain;
				domain = AppDomain.CreateDomain (Path.GetFileNameWithoutExtension (file), null, setupInfo);
			}
#endif
			try {
				DomainTester tester;
				try {
#if !NET_2_1
					if (domain != null)
						tester = (DomainTester) domain.CreateInstanceAndUnwrap (typeof (PositiveChecker).Assembly.FullName, typeof (DomainTester).FullName);
					else
#endif
						tester = new DomainTester ();

					if (!tester.Test (file))
						return false;

				} catch (ApplicationException e) {
					HandleFailure (filename, TestResult.ExecError, e.Message);
					return false;
				} catch (Exception e) {
					HandleFailure (filename, TestResult.LoadError, e.ToString ());
					return false;
				}

				if (doc_output != null) {
					string ref_file = filename.Replace (".cs", "-ref.xml");
					try {
#if !NET_2_1
						XmlComparer.Compare (ref_file, doc_output);
#endif
					} catch (Exception e) {
						HandleFailure (filename, TestResult.XmlError, e.Message);
						return false;
					}
				} else {
					if (verif_file != null) {
						PositiveTestCase pt = (PositiveTestCase) test;
						pt.VerificationProvider = (PositiveTestCase.VerificationData) verif_data[filename];

						if (!tester.CheckILSize (pt, this, file))
							return false;
					}
				}
			} finally {
				if (domain != null)
					AppDomain.Unload (domain);
			}

			HandleFailure (filename, TestResult.Success, null);
			return true;
		}

		protected override TestCase CreateTestCase (string filename, string [] options, string [] deps)
		{
			return new PositiveTestCase (filename, options, deps);
		}

		public void HandleFailure (string file, TestResult status, string extra)
		{
			switch (status) {
				case TestResult.Success:
					success++;
					if (know_issues.Contains (file)) {
						LogFileLine (file, "FIXED ISSUE");
						return;
					}
					if (verbose)
						LogFileLine (file, "OK");
					return;

				case TestResult.CompileError:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Compilation error)");
						know_issues.Remove (file);
						return;
					}
					LogFileLine (file, "REGRESSION (SUCCESS -> COMPILATION ERROR)");
					break;

				case TestResult.ExecError:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Execution error)");
						know_issues.Remove (file);
						return;
					}
					LogFileLine (file, "REGRESSION (SUCCESS -> EXECUTION ERROR)");
					break;

				case TestResult.XmlError:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Xml comparision error)");
						know_issues.Remove (file);
						return;
					}
					LogFileLine (file, "REGRESSION (SUCCESS -> DOCUMENTATION ERROR)");
					break;

				case TestResult.LoadError:
					LogFileLine (file, "REGRESSION (SUCCESS -> LOAD ERROR)");
					break;

				case TestResult.ILError:
					if (!update_verif_file) {
						LogFileLine (file, "IL REGRESSION: " + extra);
					}
					extra = null;
					break;
			}

			if (extra != null)
				LogLine ("{0}", extra);

			if (!regression.Contains (file))
				regression.Add (file);
		}

		public override void Initialize ()
		{
			if (verif_file != null) {
#if NET_2_0
				LoadVerificationData (verif_file);
#else
				throw new NotSupportedException ();
#endif
			}

			base.Initialize ();
		}

		public override void CleanUp ()
		{
			base.CleanUp ();

			if (update_verif_file) {
#if NET_2_0
				UpdateVerificationData (verif_file);
#else
				throw new NotSupportedException ();
#endif
			}
		}

#if NET_2_0
		void LoadVerificationData (string file)
		{
			LogLine ("Loading verification data from `{0}' ...", file);

			using (XmlReader r = XmlReader.Create (file)) {
				r.ReadStartElement ("tests");
				verif_data = new Hashtable ();

				while (r.Read ()) {
					if (r.Name != "test")
						continue;

					string name = r.GetAttribute ("name");
					PositiveTestCase.VerificationData tc = PositiveTestCase.VerificationData.FromFile (name, r);
					verif_data.Add (name, tc);
				}
			}
		}

		void UpdateVerificationData (string file)
		{
			LogLine ("Updating verification data `{0}' ...", file);

			XmlWriterSettings s = new XmlWriterSettings ();
			s.Indent = true;
			using (XmlWriter w = XmlWriter.Create (new StreamWriter (file, false, Encoding.UTF8), s)) {
				w.WriteStartDocument ();
				w.WriteComment ("This file contains expected IL and metadata produced by compiler for each test");
				w.WriteStartElement ("tests");
				foreach (PositiveTestCase tc in tests) {
					if (tc.VerificationProvider != null)
						tc.VerificationProvider.WriteCodeInfoTo (w);
				}
				w.WriteEndElement ();
			}
		}
#endif
	}

	class NegativeChecker: Checker
	{
		string expected_message;
		string error_message;
		bool check_msg;
		bool check_error_line;
		bool is_warning;
		IDictionary wrong_warning;

		protected enum CompilerError {
			Expected,
			Wrong,
			Missing,
			WrongMessage,
			MissingLocation,
			Duplicate
		}

		public NegativeChecker (ITester tester, bool check_msg):
			base (tester)
		{
			this.check_msg = check_msg;
			wrong_warning = new Hashtable ();
		}

		protected override bool AnalyzeTestFile (string file, ref int row, string line,
							ref string[] compiler_options,
							ref string[] dependencies)
		{
			if (row == 1) {
				expected_message = null;

				int index = line.IndexOf (':');
				if (index == -1 || index > 15) {
					LogFileLine (file, "IGNORING: Wrong test file syntax (missing error mesage text)");
					++syntax_errors;
					base.AnalyzeTestFile (file, ref row, line, ref compiler_options,
							      ref dependencies);
					return false;
				}

				expected_message = line.Substring (index + 1).Trim ();
			}

			if (row == 2) {
				string filtered = line.Replace(" ", "");

				// Some error tests require to have different error text for different runtimes.
				if (filtered.StartsWith ("//GMCS")) {
					row = 1;
#if !NET_2_0
					return true;
#else
					return AnalyzeTestFile(file, ref row, line, ref compiler_options, ref dependencies);
#endif
				}

				check_error_line = !filtered.StartsWith ("//Line:0");

				if (!filtered.StartsWith ("//Line:")) {
					LogFileLine (file, "IGNORING: Wrong test syntax (following line after an error messsage must have `// Line: xx' syntax");
					++syntax_errors;
					return false;
				}
			}

			if (!base.AnalyzeTestFile (file, ref row, line, ref compiler_options, ref dependencies))
				return false;

			is_warning = false;
			if (compiler_options != null) {
				foreach (string s in compiler_options) {
					if (s.StartsWith ("-warnaserror") || s.StartsWith ("/warnaserror"))
						is_warning = true;
				}
			}
			return true;
		}


		protected override bool Check (TestCase test)
		{
			string filename = test.FileName;

			int start_char = 0;
			while (Char.IsLetter (filename, start_char))
				++start_char;

			int end_char = filename.IndexOfAny (new char [] { '-', '.' } );
			string expected = filename.Substring (start_char, end_char - start_char);

			try {
				if (base.Check (test)) {
					HandleFailure (filename, CompilerError.Missing);
					return false;
				}
			}
			catch (Exception e) {
				HandleFailure (filename, CompilerError.Missing);
				if (e.InnerException != null)
					e = e.InnerException;
				
				Log (e.ToString ());
				return false;
			}

			int err_id = int.Parse (expected, System.Globalization.CultureInfo.InvariantCulture);
			if (tester.IsWarning (err_id)) {
				if (!is_warning)
					wrong_warning [err_id] = true;
			} else {
				if (is_warning)
					wrong_warning [err_id] = false;
			}

			CompilerError result_code = GetCompilerError (expected, tester.Output);
			if (HandleFailure (filename, result_code)) {
				success++;
				return true;
			}

			if (result_code == CompilerError.Wrong)
				LogLine (tester.Output);

			return false;
		}

		CompilerError GetCompilerError (string expected, string buffer)
		{
			const string error_prefix = "CS";
			const string ignored_error = "error CS5001";
			string tested_text = "error " + error_prefix + expected;
			StringReader sr = new StringReader (buffer);
			string line = sr.ReadLine ();
			ArrayList ld = new ArrayList ();
			CompilerError result = CompilerError.Missing;
			while (line != null) {
				if (ld.Contains (line) && result == CompilerError.Expected) {
					if (line.IndexOf ("Location of the symbol related to previous") == -1)
						return CompilerError.Duplicate;
				}
				ld.Add (line);

				if (result != CompilerError.Expected) {
					if (line.IndexOf (tested_text) != -1) {
						if (check_msg) {
							int first = line.IndexOf (':');
							int second = line.IndexOf (':', first + 1);
							if (line.IndexOf ("Warning as Error: ", first, StringComparison.Ordinal) > 0) {
								if (check_error_line) {
									second = line.IndexOf (':', second + 1);
								}
							} else if (second == -1 || !check_error_line) {
								second = first;
							}

							string msg = line.Substring (second + 1).TrimEnd ('.').Trim ();
							if (msg != expected_message && msg != expected_message.Replace ('`', '\'')) {
								error_message = msg;
								return CompilerError.WrongMessage;
							}

							if (check_error_line && line.IndexOf (".cs(") == -1)
								return CompilerError.MissingLocation;
						}
						result = CompilerError.Expected;
					} else if (line.IndexOf (error_prefix) != -1 &&
						line.IndexOf (ignored_error) == -1)
						result = CompilerError.Wrong;
				}

				line = sr.ReadLine ();
			}
			
			return result;
		}

		bool HandleFailure (string file, CompilerError status)
		{
			switch (status) {
				case CompilerError.Expected:
					if (know_issues.Contains (file) || no_error_list.Contains (file)) {
						LogFileLine (file, "FIXED ISSUE");
						return true;
					}
				
					if (verbose)
						LogFileLine (file, "OK");
					return true;

				case CompilerError.Wrong:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Wrong error reported)");
						know_issues.Remove (file);
						return false;
					}
					if (no_error_list.Contains (file)) {
						LogFileLine (file, "REGRESSION (NO ERROR -> WRONG ERROR CODE)");
						no_error_list.Remove (file);
					}
					else {
						LogFileLine (file, "REGRESSION (CORRECT ERROR -> WRONG ERROR CODE)");
					}
					break;

				case CompilerError.WrongMessage:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Wrong error message reported)");
						know_issues.Remove (file);
						return false;
					}
					if (no_error_list.Contains (file)) {
						LogFileLine (file, "REGRESSION (NO ERROR -> WRONG ERROR MESSAGE)");
						no_error_list.Remove (file);
					}
					else {
						LogFileLine (file, "REGRESSION (CORRECT ERROR -> WRONG ERROR MESSAGE)");
						LogLine ("Exp: {0}", expected_message);
						LogLine ("Was: {0}", error_message);
					}
					break;

				case CompilerError.Missing:
					if (no_error_list.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (No error reported)");
						no_error_list.Remove (file);
						return false;
					}

					if (know_issues.Contains (file)) {
						LogFileLine (file, "REGRESSION (WRONG ERROR -> NO ERROR)");
						know_issues.Remove (file);
					}
					else {
						LogFileLine (file, "REGRESSION (CORRECT ERROR -> NO ERROR)");
					}

					break;

				case CompilerError.MissingLocation:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Missing error location)");
						know_issues.Remove (file);
						return false;
					}
					if (no_error_list.Contains (file)) {
						LogFileLine (file, "REGRESSION (NO ERROR -> MISSING ERROR LOCATION)");
						no_error_list.Remove (file);
					}
					else {
						LogFileLine (file, "REGRESSION (CORRECT ERROR -> MISSING ERROR LOCATION)");
					}
					break;

				case CompilerError.Duplicate:
					// Will become an error soon
					LogFileLine (file, "WARNING: EXACTLY SAME ERROR HAS BEEN ISSUED MULTIPLE TIMES");
					return true;
			}

			regression.Add (file);
			return false;
		}

		protected override void PrintSummary()
		{
			base.PrintSummary ();

			if (wrong_warning.Count > 0) {
				LogLine ("");
				LogLine ("List of incorectly defined warnings (they should be either defined in the compiler as a warning or a test-case has redundant `warnaserror' option)");
				LogLine ("");
				foreach (DictionaryEntry de in wrong_warning)
					LogLine ("CS{0:0000} : {1}", de.Key, (bool)de.Value ? "incorrect warning definition" : "missing warning definition");
			}
		}

	}

	class Tester {

		static int Main(string[] args)
		{
			string temp;

			if (GetOption ("help", args, false, out temp)) {
				Usage ();
				return 0;
			}

			string compiler;
			if (!GetOption ("compiler", args, true, out compiler)) {
				Usage ();
				return 1;
			}

			ITester tester;
			try {
				Console.WriteLine ("Loading " + compiler + " ...");
				tester = new ReflectionTester (Assembly.LoadFile (compiler));
			}
			catch (Exception) {
#if NET_2_1
				throw;
#else
				Console.Error.WriteLine ("Switching to command line mode (compiler entry point was not found)");
				if (!File.Exists (compiler)) {
					Console.Error.WriteLine ("ERROR: Tested compiler was not found");
					return 1;
				}
				tester = new ProcessTester (compiler);
#endif
			}

			string mode;
			if (!GetOption ("mode", args, true, out mode)) {
				Usage ();
				return 1;
			}

			Checker checker;
			bool positive;
			switch (mode) {
				case "neg":
					checker = new NegativeChecker (tester, true);
					positive = false;
					break;
				case "pos":
					string iltest;
					GetOption ("il", args, false, out iltest);
					checker = new PositiveChecker (tester, iltest);
					positive = true;

					if (iltest != null && GetOption ("update-il", args, false, out temp)) {
						((PositiveChecker) checker).UpdateVerificationDataFile = true;
					}

					break;
				default:
					Console.Error.WriteLine ("Invalid -mode argument");
					return 1;
			}


			if (GetOption ("issues", args, true, out temp))
				checker.IssueFile = temp;
			if (GetOption ("log", args, true, out temp))
				checker.LogFile = temp;
			if (GetOption ("verbose", args, false, out temp))
				checker.Verbose = true;
			if (GetOption ("safe-execution", args, false, out temp))
				checker.SafeExecution = true;
			if (GetOption ("compiler-options", args, true, out temp)) {
				string[] extra = temp.Split (' ');
				checker.ExtraCompilerOptions = extra;
			}

			string test_pattern;
			if (!GetOption ("files", args, true, out test_pattern)) {
				Usage ();
				return 1;
			}

			var files = new List<string> ();
			switch (test_pattern) {
			case "v1":
				files.AddRange (Directory.GetFiles (".", positive ? "test*.cs" : "cs*.cs"));
				break;
			case "v2":
				files.AddRange (Directory.GetFiles (".", positive ? "gtest*.cs" : "gcs*.cs"));
				goto case "v1";
			case "v4":
				files.AddRange (Directory.GetFiles (".", positive ? "dtest*.cs" : "dcs*.cs"));
				goto case "v2";
			default:
				files.AddRange (Directory.GetFiles (".", test_pattern));
				break;
			}

			if (files.Count == 0) {
				Console.Error.WriteLine ("No files matching `{0}' found", test_pattern);
				return 2;
			}

			checker.Initialize ();
/*
			files.Sort ((a, b) => {
				if (a.EndsWith ("-lib.cs", StringComparison.Ordinal)) {
					if (!b.EndsWith ("-lib.cs", StringComparison.Ordinal))
						return -1;
				} else if (b.EndsWith ("-lib.cs", StringComparison.Ordinal)) {
					if (!a.EndsWith ("-lib.cs", StringComparison.Ordinal))
						return 1;
				}

				return a.CompareTo (b);
			});
*/
			foreach (string s in files) {
				string filename = Path.GetFileName (s);
				if (Char.IsUpper (filename, 0)) { // Windows hack
					continue;
				}

				if (filename.EndsWith ("-p2.cs"))
					continue;
			    
				checker.Do (filename);
			}

			checker.CleanUp ();

			checker.Dispose ();

			return checker.ResultCode;
		}

		static bool GetOption (string opt, string[] args, bool req_arg, out string value)
		{
			opt = "-" + opt;
			foreach (string a in args) {
				if (a.StartsWith (opt)) {
					int sep = a.IndexOf (':');
					if (sep > 0) {
						value = a.Substring (sep + 1);
					} else {
						value = null;
						if (req_arg) {
							Console.Error.WriteLine ("Missing argument in option " + opt);
							return false;
						}
					}

					return true;
				}
			}

			value = null;
			return false;
		}

		static void Usage ()
		{
			Console.WriteLine (
				"Mono compiler tester, (C) 2009 Novell, Inc.\n" +
				"compiler-tester -mode:[pos|neg] -compiler:FILE -files:file-list [options]\n" +
				"   \n" +
				"   -compiler:FILE   The file which will be used to compiler tests\n" +
				"   -compiler-options:OPTIONS  Add global compiler options\n" +
				"   -il:IL-FILE      XML file with expected IL details for each test\n" +
				"   -issues:FILE     The list of expected failures\n" +
				"   -log:FILE        Writes any output also to the file\n" +
				"   -help            Lists all options\n" +
				"   -mode:[pos|neg]  Specifies compiler test mode\n" +
				"   -safe-execution  Runs compiled executables in separate app-domain\n" +
				"   -update-il       Updates IL-FILE to match compiler output\n" +
				"   -verbose         Prints more details during testing\n"
				);
		}
	}
}
