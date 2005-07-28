//
// It is not used to provide our collation tailoring sources, but generates
// easy-to-read summary of LDML tailorings for ASCII-based developers (us).
//
// The actual tailoring source is mono-tailoring-source.txt.
//

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

	public class TailoringComparer : IComparer
	{
		public static TailoringComparer Instance =
			new TailoringComparer ();

		public int Compare (object o1, object o2)
		{
			Tailoring t1 = (Tailoring) o1;
			Tailoring t2 = (Tailoring) o2;
			return String.CompareOrdinal (t1.TargetString, t2.TargetString);
		}
	}

	class Tailoring
	{
		public readonly int Before;
		string targetString;

		ArrayList tailored = new ArrayList ();

		public Tailoring (string value, int before)
		{
			targetString = value;
			Before = before;
		}

		public string TargetString {
			get { return targetString; }
		}

		public IList Tailored {
			get { return tailored; }
		}

		// <x>
		public void Contraction (int level, string value, string additional)
		{
			targetString += additional;
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

		public void Serialize (TextWriter w)
		{
			w.Write ("	// Target: '{0}' {{", TargetString);
			foreach (char c in TargetString)
				w.Write ("{0:X04},", (int) c);
			w.WriteLine ("}} {0}",
				TargetString.Length > 1 ? "!" : "");
			if (Before != 0)
				w.WriteLine ("	// Before: {0}", Before);
			foreach (Mapping m in tailored) {
				w.Write ("	// {0}:'{1}' {{", m.Level, m.Value);
				foreach (char c in m.Value)
					w.Write ("{0:X04},", (int) c);
				w.WriteLine ("}");
			}
		}
	}

	public class TailoringStoreComparer : IComparer
	{
		public static TailoringStoreComparer Instance =
			new TailoringStoreComparer ();

		public int Compare (object o1, object o2)
		{
			TailoringStore t1 = (TailoringStore) o1;
			TailoringStore t2 = (TailoringStore) o2;
			return t1.Culture.LCID - t2.Culture.LCID;
		}
	}

	class TailoringStore
	{
		CultureInfo culture;
		ArrayList tailorings = new ArrayList ();
		string alias;
		bool frenchSort;

		public TailoringStore (string name)
		{
			culture = new CultureInfo (name);
		}

		public CultureInfo Culture {
			get { return culture; }
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

		public int Count {
			get { return tailorings.Count; }
		}

		public void Serialize (TextWriter w)
		{
			w.WriteLine ("// Culture: {0} ({1})", culture.LCID, culture.Name);
			if (FrenchSort)
				w.WriteLine ("// FrenchSort.");
			if (Alias != null)
				w.WriteLine ("// Alias: {0}", Alias);

			tailorings.Sort (TailoringComparer.Instance);

			foreach (Tailoring t in tailorings)
				t.Serialize (w);
		}
	}

	class CultureSpecificLdmlReader
	{
		ArrayList ignoredFiles = new ArrayList ();
		ArrayList tailorings = new ArrayList ();

		public static void Main (string [] args)
		{
			new CultureSpecificLdmlReader ().Run (args);
		}

		void Run (string [] args)
		{
			if (args.Length < 2) {
				Console.WriteLine ("specify arguments: path_to_ldml_files config_file");
				return;
			}
			string dirname = args [0];
			string configFileName = args [1];
			string config = null;
			using (StreamReader sr = new StreamReader (configFileName)) {
				config = sr.ReadToEnd ();
			}
			foreach (string configLine in config.Split ('\n')) {
				int idx = configLine.IndexOf ('#');
				string line = idx < 0 ? configLine : configLine.Substring (0, idx);
				if (line.StartsWith ("ignore: "))
					ignoredFiles.Add (line.Substring (8).Trim ());
			}

			XmlTextReader rng = new XmlTextReader ("ldml-limited.rng");
			RelaxngPattern p = RelaxngPattern.Read (rng);
			rng.Close ();

			foreach (FileInfo fi in new DirectoryInfo (dirname).GetFiles ("*.xml")) {
				if (ignoredFiles.Contains (fi.Name))
					continue; // skip
				XmlTextReader inst = null;
				try {
					inst = new XmlTextReader (fi.FullName);
					inst.XmlResolver = null;
					RelaxngValidatingReader rvr = new 
						RelaxngValidatingReader (inst, p);
					rvr.ReportDetails = true;
					XmlDocument doc = new XmlDocument ();
					doc.XmlResolver = null;
					doc.Load (rvr);
					TailoringStore ts = ProcessLdml (doc);
					if (ts != null)
						tailorings.Add (ts);
				} finally {
					if (inst != null)
						inst.Close ();
				}
			}

			tailorings.Sort (TailoringStoreComparer.Instance);

			using (TextWriter tw = new StreamWriter ("create-tailoring.out", false, System.Text.Encoding.UTF8)) {
				Serialize (tw);
			}
		}

		void Serialize (TextWriter output)
		{
			output.WriteLine ("static char [] tailorings = new char [] {");
			foreach (TailoringStore ts in tailorings)
				ts.Serialize (output);
			output.WriteLine ("};");

			int [] tailoringIndex = new int [0x80];
			int [] tailoringCount = new int [0x80];
			bool [] frenchSort = new bool [0x80];
			int current = 0;
			foreach (TailoringStore ts in tailorings) {
				int lcid = ts.Culture.LCID;
				tailoringIndex [lcid] = current;
				tailoringCount [lcid] = ts.Count;
				frenchSort [lcid] = ts.FrenchSort;
				current += ts.Count;
			}
			// process alias
			foreach (TailoringStore ts in tailorings) {
				if (ts.Alias == null)
					continue;
				int lcid = ts.Culture.LCID;
				int target = new CultureInfo (ts.Alias).LCID;
				tailoringIndex [lcid] = tailoringIndex [target];
				tailoringCount [lcid] = tailoringCount [target];
				frenchSort [lcid] = frenchSort [target];
			}

			output.WriteLine (@"
/*typedef*/ struct TailoringInfo {
public TailoringInfo (ushort lcid, uint idx, ushort count, bool french) { Lcid = lcid; TailoringIndex = idx; TailoringCount = count; FrenchSort = french; }
public readonly ushort Lcid;
/*guint32*/ public readonly uint TailoringIndex;
/*guint16*/ public readonly ushort TailoringCount;
/*gboolean*/ public readonly bool FrenchSort;
}/* TailoringInfo;*/");

			output.WriteLine ("static TailoringInfo [] tailoringIndexes = new TailoringInfo [] {");
			for (int i = 0; i < tailoringIndex.Length; i++)
				output.WriteLine ("new TailoringInfo ({0}, {1}, {2}, {3}), ",
					i,
					tailoringIndex [i],
					tailoringCount [i],
					frenchSort [i]);
			output.WriteLine ("};");

			output.Flush ();
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
//			Console.Error.WriteLine ("Processing " + lcid);

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

				switch (el.LocalName) {
				case "reset":
					switch (el.GetAttribute ("before")) {
					case "primary": before = 1; break;
					case "secondary": before = 2; break;
					}

					switch (el.FirstChild.LocalName) {
					case "last_primary_ignorable":
					case "last_secondary_ignorable":
						Console.Error.WriteLine ("WARNING: {0} is not supported for now.", el.FirstChild.LocalName);
						continue;
					}
					XmlElement cpElem = el.SelectSingleNode ("cp") as XmlElement;
					string v = "";
					if (cpElem != null)
						v = new string ((char) (int.Parse (
							cpElem.GetAttribute ("hex"),
							NumberStyles.HexNumber)), 1);
					else
						v = el.FirstChild.Value;
					t = new Tailoring (v, before);
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
					bool exists = contraction != null;
					contraction = el.LastChild.InnerText;
					t.Contraction (contLevel,
						el.FirstChild.InnerText,
						exists ? "" : contraction);
					break;
				default:
					throw new Exception ("Support this element: " + el.Name);
				}
			}
			return ts;
		}
	}
}
