//
// report.cs: report errors and warnings.
//
// Author: Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;

namespace Mono.CSharp {

	/// <summary>
	///   This class is used to report errors and warnings t te user.
	/// </summary>
	public class Report {
		static int errors;
		static int warnings;

		// whether errors are fatal (they throw an exception), useful
		// for debugging the compiler
		static bool fatal;

		// whether we consider warnings to be errors.
		static bool warnings_are_errors;
		
		//
		// If the error code is reported on the given line,
		// then the process exits with a unique error code.
		//
		// Used for the test suite to excercise the error codes
		//
		static int probe_error = 0;
		static int probe_line = 0;

		//
		// Keeps track of the warnings that we are ignoring
		//
		static Hashtable warning_ignore_table;
		
		static void Check (int code)
		{
			if (code ==  probe_error){
				Environment.Exit (123);
			}
		}
		
		static public void RealError (string msg)
		{
			errors++;
			Console.WriteLine (msg);

			if (fatal)
				throw new Exception (msg);
		}
		       
		static public void Error (int code, Location l, string text)
		{
			string msg = l.Name + "(" + l.Row + 
				"): error CS"+code+": " + text;

			RealError (msg);
			Check (code);
		}

		static public void Warning (int code, Location l, string text)
		{
			if (warning_ignore_table != null){
				if (warning_ignore_table.Contains (code))
					return;
			}
			
			if (warnings_are_errors)
				Error (code, l, text);
			else {
				string row;
				
				if (Location.IsNull (l))
					row = "";
				else
					row = l.Row.ToString ();
				
				Console.WriteLine (l.Name + "(" + row + "): warning CS"+code+": " + text);
				warnings++;
				Check (code);
			}
		}
		
		static public void Warning (int code, string text)
		{
			Warning (code, Location.Null, text);
		}

		static public void Error (int code, string text)
		{
			string msg = "error CS"+code+": "+text;

			RealError (msg);
			Check (code);
		}

		static public void Message (Message m)
		{
			if (m is ErrorMessage)
				Error (m.code, m.text);
			else
				Warning (m.code, m.text);
		}

		static public void SetIgnoreWarning (int code)
		{
			if (warning_ignore_table == null)
				warning_ignore_table = new Hashtable ();

			warning_ignore_table [code] = true;
		}
		
		static public void SetProbe (int code, int line)
		{
			probe_error = code;
			probe_line = line;
		}

		static public int ProbeCode {
			get {
				return probe_error;
			}
		}
		
		static public int Errors {
			get {
				return errors;
			}
		}

		static public int Warnings {
			get {
				return warnings;
			}
		}

		static public bool Fatal {
			set {
				fatal = true;
			}

			get {
				return fatal;
			}
		}

		static public bool WarningsAreErrors {
			set {
				warnings_are_errors = true;
			}
		}
	}

	public class Message {
		public int code;
		public string text;
		
		public Message (int code, string text)
		{
			this.code = code;
			this.text = text;
		}
	}

	public class WarningMessage : Message {
		public WarningMessage (int code, string text) : base (code, text)
		{
		}
	}

	public class ErrorMessage : Message {
		public ErrorMessage (int code, string text) : base (code, text)
		{
		}

		//
		// For compatibility reasons with old code.
		//
		public static void report_error (string error)
		{
			Console.Write ("ERROR: ");
			Console.WriteLine (error);
		}
	}
}


