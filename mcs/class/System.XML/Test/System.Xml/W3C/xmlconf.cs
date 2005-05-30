using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace XmlConfTest {
	class CommandLineOptionAttribute:Attribute{
		char _short;
		string _long; //FIXME: use long form, too
		public CommandLineOptionAttribute (char a_short, string a_long):base() {
			_short = a_short;
			_long = a_long;
		}
			
		public CommandLineOptionAttribute (char a_short):this (a_short, null) {
		}

		public override string ToString() {
			return _short.ToString();
		}
	}

	class XmlConfTest {
		static int Main (string[] args)
		{
			if (!new XmlConfTest(args).Run ())
				return 1;
			else
				return 0;
		}

		static void PrintUsage () {
			Console.Error.WriteLine("Usage: xmlconf <flags>");
			Console.Error.WriteLine("\tFlags:");
			foreach (DictionaryEntry de in XmlConfTest.GetOptions())
				Console.Error.WriteLine ("\t{0}\t{1}", de.Key, de.Value);
		}

		public static Hashtable GetOptions() {
			Hashtable h = new Hashtable();

			foreach (FieldInfo i in typeof (XmlConfTest).GetFields()) {
				//FIXME: handle long options, too
				string option = "-" + i.GetCustomAttributes(typeof(CommandLineOptionAttribute),
					true)[0].ToString();
				string descr = (i.GetCustomAttributes(typeof(DescriptionAttribute),
					true)[0] as DescriptionAttribute).Description;
				h[option] = descr;
			}
			return h;
		}

		string [] _args;

		ArrayList slowTests = new ArrayList ();
		[CommandLineOptionAttribute ('s')]
		[Description ("do run slow tests (skipped by default)")]
		bool runSlow = false;

		public bool ParseOptions () {
			if (_args.Length < 1)
				return true;
			if(_args[0].Length < 2 || _args[0][0] != '-') {
				PrintUsage();
				return false;
			}
			string options = _args[0].Substring(1); //FIXME: handle long options
			foreach (FieldInfo i in typeof (XmlConfTest).GetFields (BindingFlags.NonPublic
				| BindingFlags.Instance)) {
				//FIXME: report if unknown options were passed
				object [] attrs = i.GetCustomAttributes(typeof(CommandLineOptionAttribute),true);
				if (attrs.Length == 0)
					continue;
				string option = attrs[0].ToString();
				if (options.IndexOf(option) == -1)
					continue;
				i.SetValue (this, true);
			}
			return true;
		}

		static void ReadStrings (ArrayList array, string filename)
		{
			if (File.Exists (filename))
				using (StreamReader reader = new StreamReader (filename)) {
					foreach (string s_ in reader.ReadToEnd ().Split ("\n".ToCharArray ())) {
						string s = s_.Trim ();
						if (s.Length > 0)
							array.Add (s);
					}
				}
		}

		XmlConfTest (string [] args)
		{
			_args = args;
			ReadStrings (slowTests, "slow.lst");
		}

		bool Run ()
		{
			bool res = true;
			if (!ParseOptions ())
				return false;

			XmlDocument catalog = new XmlDocument ();
			catalog.Load (@"xmlconf\xmlconf.xml");
			
			foreach (XmlElement test in catalog.SelectNodes ("//TEST")) {
				string testId = test.GetAttribute ("ID");
				if (!runSlow && slowTests.Contains (testId))
					continue;
				DateTime start = DateTime.Now;
				res &= PerformTest (test);	
				TimeSpan span = DateTime.Now - start;
				if (span.TotalSeconds > 1) {
					if (slowTests.Contains (testId))
						continue;
					using (StreamWriter wr = new StreamWriter("slow.lst", true))
						wr.WriteLine (testId);
				}
			}
			return res;
		}

		bool PerformTest (XmlElement test)
		{
			string type = test.GetAttribute ("TYPE");
			if (type == "error")
				return true; //save time

			Uri baseUri = new Uri (test.BaseURI);
			Uri testUri = new Uri (baseUri, test.GetAttribute ("URI"));
			bool validatingPassed;
			bool nonValidatingPassed;
			try {
				XmlTextReader trd = new XmlTextReader (testUri.ToString ());
				new XmlDocument ().Load (trd);
				nonValidatingPassed = true;
			}
			catch (Exception) {
				nonValidatingPassed = false;
			}

			try {
				XmlTextReader rd = new XmlTextReader (testUri.ToString ());
				XmlValidatingReader vrd = new XmlValidatingReader (rd);
				new XmlDocument ().Load (vrd);
				validatingPassed = true;
			}
			catch (Exception) {
				validatingPassed = false;
			}
			bool res = isOK (type, nonValidatingPassed, validatingPassed);
			Report (test, res, nonValidatingPassed, validatingPassed);
			return res;
		}

		bool isOK (string type, bool nonValidatingPassed, bool validatingPassed)
		{
			switch (type) {
			case "valid":
				return nonValidatingPassed && validatingPassed;
			case "invalid":
				return nonValidatingPassed && !validatingPassed;
			case "not-wf":
				return !nonValidatingPassed && !validatingPassed;
			case "error":
				return true;
			default:
				throw new ArgumentException ("Bad test type", "type");
			}
		}

		void Report (XmlElement test, bool isok, bool nonValidatingPassed, bool validatingPassed)
		{
			if (isok) {
				Console.Error.Write (".");
				return;
			}

			Console.Error.Write ("E");
		}
	}
}
