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

		[MonoTODO("Not sure what to do about sourceTextNames yet")]
		public static CompilerError[] Compile(string[] sourceTexts,
			string[] sourceTextNames, string target, string[] imports,
   			IDictionary options)
		{
			VerifyArgs (sourceTexts, sourceTextNames, target);
			
			ArrayList error_list = new ArrayList ();
			Process mcs = new Process ();
			StreamReader mcs_output;
			string error_line;

			mcs.StartInfo.FileName = "mcs";
			mcs.StartInfo.Arguments = BuildArgs (sourceTexts, 
				sourceTextNames, target, imports, options);
			mcs.StartInfo.CreateNoWindow = true;
			mcs.StartInfo.UseShellExecute = false;
			mcs.StartInfo.RedirectStandardOutput = true;

			try {
				mcs.Start ();
				mcs_output = mcs.StandardOutput;
				mcs.WaitForExit ();
			} finally {
				mcs.Close ();
			}
			
			error_line = mcs_output.ReadLine ();
			while (null != error_line) {
				CompilerError error = CreateErrorFromString (error_line);
				if (null != error)
					error_list.Add (error);
				error_line = mcs_output.ReadLine ();
			}
			
			return (CompilerError[])error_list.ToArray (typeof(CompilerError));
		}
		
		//
		// Private Methods
		//

		/// <summary>
		///   Converts an error string into a CompilerError object
		///   Return null if the line was not an error string
		/// </summary>
		private static CompilerError CreateErrorFromString(string error_string) 
		{
			CompilerError error = new CompilerError();
			Regex reg = new Regex (@"^((?<file>.*\.cs)\((?<line>\d*)(,(?<column>\d*))?\)\s){0,}(?<level>\w+)\sCS(?<number>\d*):\s(?<message>.*)", 
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

		private static string BuildArgs(string[] sourceTexts,
			string[] sourceTextNames, string target, string[] imports,
   			IDictionary options)
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
			
			foreach (string source in sourceTexts)
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

