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
}
