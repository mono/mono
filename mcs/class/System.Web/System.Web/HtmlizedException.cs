//
// System.Web.HtmlizedException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.IO;
using System.Text;

namespace System.Web
{
	internal abstract class HtmlizedException : Exception
	{
		protected HtmlizedException ()
		{
		}

		protected HtmlizedException (string message)
			: base (message)
		{
		}

		protected HtmlizedException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public abstract string Title { get; }
		public abstract string Description { get; }
		public abstract string ErrorMessage { get; }
		public abstract string FileName { get; }
		public abstract StringReader SourceError { get; }
		public abstract int SourceErrorLine { get; }
		public abstract TextReader SourceFile { get; }

		public bool HaveSourceError {
			get { return SourceError != null; }
		}

		public bool HaveSourceFile {
			get { return SourceFile != null; }
		}

		internal static string GetErrorLines (TextReader reader, int line, out int errorLine)
		{
			int firstLine = (line > 2) ? (line - 2) : line;
			int lastLine = (line >= 0) ? (firstLine + 2) : Int32.MaxValue;
			errorLine = (line > 2) ? line : 1;
			int current = 0;
			string s;

			while ((s = reader.ReadLine ()) != null && current != firstLine)
				current++;

			if (s == null)
				return "Cannot read error line.";

			StringBuilder builder = new StringBuilder ();
			do {
				builder.Append (s + '\n');
				current++;
			} while (current < lastLine && (s = reader.ReadLine ()) != null);

			return builder.ToString ();
		}
	}
}

