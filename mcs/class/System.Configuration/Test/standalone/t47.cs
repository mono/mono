#define TRACE

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace MonoTest.Configuration
{
	class Program
	{
		static void Main ()
		{
			Trace.Write ("OK");

			string basedir = AppDomain.CurrentDomain.BaseDirectory;
			string output = Path.Combine (basedir, "log.tmp");

			try {
				using (StreamReader sr = new StreamReader (Path.Combine (basedir, "log.tmp"), Encoding.UTF8)) {
					string text = sr.ReadToEnd ();
					Console.WriteLine (text);
				}
			} finally {
				File.Delete (output);
			}
		}
	}

	public class CustomFileListener : TraceListener
	{
		private string Prefix {
			get {
				return Attributes ["prefix"];
			}
		}

		private string OutputFile {
			get {
				string basedir = AppDomain.CurrentDomain.BaseDirectory;
				return Path.Combine (basedir, Attributes ["output"]);
			}
		}

		protected override string [] GetSupportedAttributes ()
		{
			return new string [] { "output", "prefix", "suffix" };
		}

		public override void Write (string message)
		{
			using (StreamWriter sw = new StreamWriter (OutputFile, true, Encoding.UTF8)) {
				if (Prefix != null)
					sw.Write (Prefix);
				sw.Write (message);
				sw.Write (Attributes.Count.ToString (CultureInfo.InvariantCulture));
			}
		}

		public override void WriteLine (string message)
		{
			using (StreamWriter sw = new StreamWriter (OutputFile, true, Encoding.UTF8)) {
				if (Prefix != null)
					sw.Write (Prefix);
				sw.WriteLine (message);
			}
		}
	}
}
