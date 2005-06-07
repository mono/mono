using System;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Xml;
using Commons.Xml.Relaxng;

namespace Mono.Globalization.Unicode
{


	class Mapping
	{
		public readonly int Level;
		public readonly string Value;

		public Mapping (int level, string value)
		{
			Level = level;
			Value = value;
		}
	}

	class Tailoring
	{
		public readonly char Target;
		public readonly int Before;
		string targetString;

		ArrayList tailored = new ArrayList ();

		public Tailoring (char c, int before)
		{
			Target = c;
			Before = before;
		}

		public string TargetString {
			get { return targetString; }
		}

		public IList Tailored {
			get { return tailored; }
		}

		public void Contraction (int level, string value, string additional)
		{
			targetString = Target + additional;
			tailored.Add (new Mapping (level, value));
		}

		// <p> <s> <t> <q> <i>
		public void Add (int level, string value)
		{
			tailored.Add (new Mapping (level, value));
		}

		// <pc> <sc> <tc> <qc> <ic>
		public void AddRange (int level, string value)
		{
			foreach (char c in value)
				tailored.Add (new Mapping (level, new string (c, 1)));
		}
	}

	class TailoringStore
	{
		int lcid;
		ArrayList tailorings = new ArrayList ();
		string alias;
		bool frenchSort;

		public TailoringStore (string name)
		{
			lcid = new CultureInfo (name).LCID;
		}

		public bool FrenchSort {
			get { return frenchSort; }
			set { frenchSort = value; }
		}

		public string Alias {
			get { return alias; }
			set { alias = value; }
		}

		public void Add (Tailoring t)
		{
			tailorings.Add (t);
		}
	}

	class CultureSpecificLdmlReader
	{
		ArrayList tailorings = new ArrayList ();

		public static void Main (string [] args)
		{
			new CultureSpecificLdmlReader ().Run (args);
		}

		void Run (string [] args)
		{
			string dirname = args [0];
			XmlTextReader rng = new XmlTextReader ("ldml-limited.rng");
			RelaxngPattern p = RelaxngPattern.Read (rng);
			rng.Close ();

			foreach (FileInfo fi in new DirectoryInfo (dirname).GetFiles ("*.xml")) {
				switch (fi.Name) {
				case "dz.xml": // too fragile draft
				case "ja.xml": // will use modified one instead
				case "ko.xml": // will use modified one instead
				case "root.xml":
					continue; // skip
				}
				if (fi.Name.StartsWith ("zh"))
					continue; // will use modified ones instead
				XmlTextReader inst = null;
				try {
					inst = new XmlTextReader (fi.FullName);
					RelaxngValidatingReader rvr = new 
						RelaxngValidatingReader (inst, p);
					rvr.ReportDetails = true;
					XmlDocument doc = new XmlDocument ();
					doc.Load (rvr);
					TailoringStore ts = ProcessLdml (doc);
					if (ts != null)
						tailorings.Add (ts);
				} finally {
					if (inst != null)
						inst.Close ();
				}
			}
		}

		TailoringStore ProcessLdml (XmlDocument doc)
		{
			XmlElement langElem = doc.SelectSingleNode (
				"/ldml/identity/language") as XmlElement;
			string lang = langElem.GetAttribute ("type");
			XmlElement terr = doc.SelectSingleNode (
				"/ldml/identity/territory") as XmlElement;
			string lcid = lang + (terr != null ?
				"-" + terr.GetAttribute ("type") : null);
			TailoringStore ts = null;
			try {
				ts = new TailoringStore (lcid);
			} catch (ArgumentException) {
				Console.Error.WriteLine ("WARNING: culture " + lcid + " is not supported in the runtime.");
				return null;
			}
			Console.Error.WriteLine ("Processing " + lcid);

			XmlNode vn = doc.SelectSingleNode ("/ldml/collations/alias/@source");
			if (vn != null) {
				ts.Alias = vn.Value;
				return ts;
			}

			XmlElement collation = doc.SelectSingleNode ("/ldml/collations/collation[@type='standard']") as XmlElement;
			XmlElement settings = collation.SelectSingleNode ("settings") as XmlElement;
			if (settings != null)
				ts.FrenchSort = settings.GetAttribute ("backwards") == "on";

			Tailoring t = null;
			int before = 0;
			string contraction = null;

			foreach (XmlNode n in collation.SelectNodes ("rules/*")) {
				XmlElement el = n as XmlElement;
				if (el == null)
					continue;
				foreach (XmlAttribute a in el.Attributes) {
					switch (a.LocalName) {
					case "before":
						switch (el.LocalName) {
						case "reset":
							before = a.Value == "primary" ? 1 : a.Value == "secondary" ? 2 : 6;
							continue;
						}
						break;
					}
					throw new Exception ("Support this attribute: " + el.Attributes [0].Name);
				}

				switch (el.LocalName) {
				case "reset":
					switch (el.FirstChild.LocalName) {
					case "last_primary_ignorable":
					case "last_secondary_ignorable":
						Console.Error.WriteLine ("WARNING: {0} is not supported for now.", el.FirstChild.LocalName);
						continue;
					}
					XmlElement cpElem = el.SelectSingleNode ("cp") as XmlElement;
					char cp = char.MinValue;
					if (cpElem != null)
						cp = (char) (int.Parse (
							cpElem.GetAttribute ("hex"),
							NumberStyles.HexNumber));
					else
						cp = el.FirstChild.Value [0];
					t = new Tailoring (cp, before);
					before = 0;
					contraction = null;
					ts.Add (t);
					break;
				case "p":
				case "pc":
					t.Add (1, el.InnerText);
					break;
				case "s":
				case "sc":
					t.Add (2, el.InnerText);
					break;
				case "t":
				case "tc":
					t.Add (3, el.InnerText);
					break;
				case "q":
				case "qc":
					t.Add (4, el.InnerText);
					break;
				case "i":
				case "ic":
					t.Add (5, el.InnerText);
					break;
				case "x":
					int contLevel = 0;
					switch (el.FirstChild.LocalName) {
					case "s":
						contLevel = 2; break;
					case "t":
						contLevel = 3; break;
					default:
						throw new Exception ("Not expected first child of 'x': " + el.Name);
					}
					if (contraction != null && el.LastChild.InnerText != contraction)
						throw new Exception ("When there are sequential 'x' elements for single tailoring, those 'extend' text must be identical.");
					contraction = el.LastChild.InnerText;
					t.Contraction (contLevel,
						el.FirstChild.InnerText,
						contraction);
					break;
				default:
					throw new Exception ("Support this element: " + el.Name);
				}
			}
			return ts;
		}
	}
}
