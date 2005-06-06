using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace XmlConfTest {
	class XmlConfTest {
		
		#region Command Line Options Handling

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

			public string Long {
				get {
					return _long;
				}
			}
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
		#endregion

		string [] _args;

		#region statistics fields
		int totalCount = 0;
		int performedCount = 0;
		int passedCount = 0;
		int failedCount = 0;
		int regressionsCount = 0; //failures not listed in knownFailures.lst
		int fixedCount = 0; //tested known to fail that passed
		#endregion

		#region test list fields
		ArrayList slowTests = new ArrayList ();
		ArrayList igroredTests = new ArrayList ();
		ArrayList knownFailures = new ArrayList ();
		ArrayList fixmeList = new ArrayList ();
		ArrayList netFailures = new ArrayList ();
		StreamWriter failedList;
		StreamWriter fixedList;
		StreamWriter slowNewList;
		#endregion

		#region command line option fields
		[CommandLineOption ('s')]
		[Description ("do run slow tests (skipped by default)")]
		bool runSlow = false;

		[CommandLineOption ('i')]
		[Description ("do run tests being ignored by default")]
		bool runIgnored = false;
		#endregion

		static int Main (string[] args)
		{
			if (!new XmlConfTest(args).Run ())
				return 1;
			else
				return 0;
		}

		#region ReadStrings ()
		static void ReadStrings (ArrayList array, string filename)
		{
			if (!File.Exists (filename))
				return;

			using (StreamReader reader = new StreamReader (filename)) {
				foreach (string s_ in reader.ReadToEnd ().Split ("\n".ToCharArray ())) {
					string s = s_.Trim ();
					if (s.Length > 0)
						array.Add (s);
				}
			}
		}
		#endregion

		XmlConfTest (string [] args)
		{
			_args = args;
			failedList = new StreamWriter ("failed.lst", false);
			fixedList = new StreamWriter ("fixed.lst", false);
			slowNewList = new StreamWriter ("slow-new.lst", false);
			ReadStrings (slowTests, "slow.lst");
			ReadStrings (igroredTests, "ignored.lst");
			ReadStrings (knownFailures, "knownFailures.lst");
			ReadStrings (fixmeList, "fixme.lst");
			ReadStrings (netFailures, "net-failed.lst");
		}

		bool Run ()
		{
			bool res = true;
			if (!ParseOptions ())
				return false;

			XmlDocument catalog = new XmlDocument ();
			catalog.Load ("xmlconf/xmlconf.xml");
			
			foreach (XmlElement test in catalog.SelectNodes ("//TEST")) {
				++totalCount;

				string testId = test.GetAttribute ("ID");
				
				if (!runSlow && slowTests.Contains (testId)) {
					continue;
				}

				if (!runIgnored && igroredTests.Contains (testId)) {
					continue;
				}

				DateTime start = DateTime.Now;
				if (!PerformTest (test))
					res = false;
				TimeSpan span = DateTime.Now - start;
				if (span.TotalSeconds > 1) {
					if (slowTests.Contains (testId))
						continue;
					slowNewList.WriteLine (testId);
					slowNewList.Flush ();
				}
			}

			Console.Error.WriteLine ("\n\n*********");
			Console.Error.WriteLine ("Total:{0}", totalCount);
			Console.Error.WriteLine ("Performed:{0}", performedCount);
			Console.Error.WriteLine ("Passed:{0}", passedCount);
			Console.Error.WriteLine ("Failed:{0}", failedCount);
			Console.Error.WriteLine ("Regressions:{0}", regressionsCount);
			Console.Error.WriteLine ("Fixed:{0}\n", fixedCount);

			if (fixedCount > 0)
				Console.Error.WriteLine (@"

ATTENTION!
Delete the fixed tests (those listed in fixed.lst) from
knownFailures.lst or fixme.lst, or we might miss
regressions in the future.");

			if (regressionsCount > 0)
				Console.Error.WriteLine (@"

ERROR!!! New regressions!
If you see this message for the first time, your last changes had
introduced new bugs! Before you commit, consider one of the following:

1. Find and fix the bugs, so tests will pass again.
2. Open new bugs in bugzilla and temporily add the tests to fixme.lst
3. Write to devlist and confirm adding the new tests to knownFailures.lst");

			return res;
		}

		bool PerformTest (XmlElement test)
		{
			++performedCount;

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
			
			return Report (test, res, nonValidatingPassed, validatingPassed);
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
				return true; //readers can optionally accept or reject errors
			default:
				throw new ArgumentException ("Bad test type", "type");
			}
		}

		bool Report (XmlElement test, bool isok, bool nonValidatingPassed, bool validatingPassed)
		{
			string testId = test.GetAttribute ("ID");

			if (isok) {
				++passedCount;
				if (fixmeList.Contains (testId) || knownFailures.Contains (testId)) {
					++fixedCount;
					fixedList.WriteLine (testId);
					fixedList.Flush ();
					Console.Error.Write ("!");
					return true;
				}
				if (netFailures.Contains (testId)) {
					Console.Error.Write (",");
					return true;
				}

				Console.Error.Write (".");
				return true;
			}

			++failedCount;

			if (netFailures.Contains (testId)) {
				Console.Error.Write ("K");
				return true;
			}
			if (knownFailures.Contains (testId)) {
				Console.Error.Write ("k");
				return true;
			}
			if (fixmeList.Contains (testId)) {
				Console.Error.Write ("f");
				return true;
			}

			++regressionsCount;
			Console.Error.Write ("E");
			failedList.Write ("*** Test failed:\t{0}\ttype:{1}\tnonValidatingPassed:{2},validatingPassed:{3}\t",
				testId, test.GetAttribute ("TYPE"), nonValidatingPassed, validatingPassed);
			failedList.WriteLine (test.InnerXml);
			failedList.Flush ();
			return false;
		}
	}
}
