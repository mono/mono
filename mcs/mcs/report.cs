//
// report.cs: report errors and warnings.
//
// Author: Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc. (http://www.ximian.com)
//

using System;

namespace CIR {

	public class Report {
		int errors;
		int warnings;
		bool fatal;

		public void RealError (string msg)
		{
			errors++;
			Console.WriteLine (msg);

			if (fatal)
				throw new Exception (msg);
		}
		       
		public void Error (int code, Location l, string text)
		{
			string msg = l.Name + "(" + l.Row + "," + l.Col +
				"): Error CS"+code+": " + text;

			RealError (msg);
		}

		public void Warning (int code, Location l, string text)
		{
			Console.WriteLine (l.Name + "(" + l.Row + "," + l.Col +
					   "): Warning CS"+code+": " + text);
			warnings++;
		}
		
		public void Error (int code, string text)
		{
			string msg = "Error CS"+code+": "+text;

			RealError (msg);
		}

		public void Warning (int code, string text)
		{
			Console.WriteLine ("Warning CS"+code+": "+text);
			warnings++;
		}

		public void Message (Message m)
		{
			if (m is ErrorMessage)
				Error (m.code, m.text);
			else
				Warning (m.code, m.text);
		}
			
		public int Errors {
			get {
				return errors;
			}
		}

		public int Warnings {
			get {
				return warnings;
			}
		}

		public bool Fatal {
			set {
				fatal = true;
			}

			get {
				return fatal;
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


