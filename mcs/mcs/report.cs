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
		
		public void Error (int code, string text)
		{
			Console.WriteLine ("Error CS"+code+": "+text);
			errors++;
		}

		public void Warning (int code, string text)
		{
			Console.WriteLine ("Warning CS"+code+": "+text);
			warnings++;
		}

		public void Message (Message m)
		{
			if (m is Error)
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

	public class Warning : Message {
		public Warning (int code, string text) : base (code, text)
		{
		}
	}

	public class Error : Message {
		public Error (int code, string text) : base (code, text)
		{
		}
	}
}


