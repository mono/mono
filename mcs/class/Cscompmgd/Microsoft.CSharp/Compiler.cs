// Microsoft.CSharp.Compiler
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.
//

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Microsoft.CSharp {

	public class Compiler {
		
		public Compiler()
		{
		}

		[MonoTODO("Have not implemented bugreports")]
		public static CompilerError[] Compile(string[] sourceTexts,
			string[] sourceTextNames, string target, string[] imports,
   			IDictionary options)
		{
			VerifyArgs (sourceTexts, sourceTextNames, target);
			
			string[] temp_cs_files;
			CompilerError[] errors;

			temp_cs_files = CreateCsFiles (sourceTexts, sourceTextNames);
			
			try {
				errors = CompileFiles (temp_cs_files, target, imports, options);
			} catch {
				throw;
			} finally {
				foreach (string temp_file in temp_cs_files) {
					FileInfo file = new FileInfo (temp_file);
					file.Delete ();
				}
			}
			
			return errors;
		}
		
		//
		// Private Methods
		//

		private static CompilerError[] CompileFiles (string[] cs_files,
			string target, string[] imports, IDictionary options) 
		{
			ArrayList error_list = new ArrayList ();
			Process mcs = new Process ();
			string mcs_output;
			string[] mcs_output_lines;

			mcs.StartInfo.FileName = "mcs";
			mcs.StartInfo.Arguments = BuildArgs (cs_files, 
				target, imports, options);
			mcs.StartInfo.CreateNoWindow = true;
			mcs.StartInfo.UseShellExecute = false;
			mcs.StartInfo.RedirectStandardOutput = true;

			try {
				mcs.Start ();
				mcs_output = mcs.StandardOutput.ReadToEnd();
				mcs.WaitForExit ();
			} finally {
				mcs.Close ();
			}
			
			mcs_output_lines = mcs_output.Split (
				System.Environment.NewLine.ToCharArray ());
			foreach (string error_line in mcs_output_lines) {
				CompilerError error = CreateErrorFromString (error_line);
				if (null != error)
					error_list.Add (error);	
			}
			
			return (CompilerError[])error_list.ToArray (typeof(CompilerError));
		}

		/// <summary>
		///   Converts an error string into a CompilerError object
		///   Return null if the line was not an error string
		/// </summary>
		private static CompilerError CreateErrorFromString(string error_string) 
		{
			CompilerError error = new CompilerError();
			Regex reg = new Regex (@"^((?<file>.*)\((?<line>\d*)(,(?<column>\d*))?\)\s){0,}(?<level>\w+)\sCS(?<number>\d*):\s(?<message>.*)", 
			RegexOptions.Compiled | RegexOptions.ExplicitCapture);

			Match match = reg.Match (error_string);
			
			if (!match.Success)
				return null;
			
			if (String.Empty != match.Result ("${file}"))
				error.SourceFile = match.Result ("${file}");
			if (String.Empty != match.Result ("${line}"))
				error.SourceLine = Int32.Parse (match.Result ("${line}"));
			if (String.Empty != match.Result ("${column}"))
				error.SourceColumn = Int32.Parse (match.Result ("${column}"));
			error.ErrorLevel = (ErrorLevel)Enum.Parse (typeof(ErrorLevel),
				match.Result ("${level}"), true);
			error.ErrorNumber = Int32.Parse (match.Result ("${number}"));
			error.ErrorMessage = match.Result ("${message}");
			
			return error;
		}

		private static string[] CreateCsFiles (string[] source_text, string[] source_name) 
		{
			ArrayList temp_file_list = new ArrayList ();

			for (int i=0; i<source_text.Length; i++) {
				string temp_path = Path.GetTempFileName ();
				StreamWriter writer = null;
				try {
					writer = new StreamWriter (temp_path);
					writer.WriteLine (String.Format ("#line 1 \"{0}\"", 
						source_name[i]));
					writer.Write (source_text[i]);
				} catch {
				} finally {
					if (writer != null)
						writer.Close ();
				}
				temp_file_list.Add (temp_path);
			}
		
			return (string[])temp_file_list.ToArray (typeof(string));	
		}

		private static string BuildArgs(string[] source_files,
			string target, string[] imports, IDictionary options)
		{
			StringBuilder args = new StringBuilder ();

			args.AppendFormat ("/out:{0} ", target);
			
			if (null != imports) {
				foreach (string import in imports)
					args.AppendFormat ("/r:{0} ", import);
			}
			
			if (null != options) {
				foreach (object option in options.Keys) {
					object value = options[option];
					args.AppendFormat ("{0} ", OptionString (option,value));
				}
			}
			
			foreach (string source in source_files)
				args.AppendFormat ("{0} ", source);

			return args.ToString ();
		}

		private static string OptionString(object option, object value)
		{
			if (null != value)
				return String.Format ("/{0}:{1}", option, value);
			
			return String.Format("/{0}", option);
		}

		private static void VerifyArgs (string[] sourceTexts,
			string[] sourceTextNames, string target)
		{
			if (null == sourceTexts)
				throw new ArgumentNullException ("sourceTexts");
			if (null == sourceTextNames)
				throw new ArgumentNullException ("sourceTextNames");
			if (null == target)
				throw new ArgumentNullException ("target");

			if (sourceTexts.Length <= 0 || sourceTextNames.Length <= 0)
				throw new IndexOutOfRangeException ();
		}

	}

}

