using System;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Derivative;

public class Test
{
	static char SEP = Path.DirectorySeparatorChar;

	public static void Main ()
	{
Console.WriteLine ("Started:  " + DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss.fff"));
		RunTest ();
Console.WriteLine ("Finished: " + DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss.fff"));
	}

	static void RunTest ()
	{
		foreach (DirectoryInfo di in
			new DirectoryInfo (@"relax-ng").GetDirectories ()) {

/*
if (di.Name == "056") // baseURI
	continue;
if (di.Name == "102") // invalid URI fragment
	continue;
if (di.Name == "208") // infinite loop!!
	continue;
if (di.Name == "210") // infinite loop!!
	continue;
*/

			XmlTextReader xtr = null;
			FileInfo fi = new FileInfo (di.FullName + "/i.rng");
			// Invalid grammar case:
			if (fi.Exists) {
				xtr = new XmlTextReader (fi.FullName);
				try {
					RelaxngPattern.Read (xtr).Compile ();
					Console.WriteLine ("Expected error: " + di.Name);
				} catch (RelaxngException ex) {
				} catch (XmlException ex) {
				} catch (ArgumentNullException ex) {
				} catch (UriFormatException ex) {
				} catch (Exception ex) {
					Console.WriteLine ("Unexpected error type : " + di.Name + " : " + ex.Message);
				} finally {
					xtr.Close ();
				}
				continue;
			}

			// Valid grammar case:
			xtr = new XmlTextReader (di.FullName + "/c.rng");
			RelaxngPattern p = null;
			try {
				p = RelaxngPattern.Read (xtr);
				p.Compile ();
				} catch (Exception ex) {
					Console.WriteLine ("Invalidated grammar: " + di.Name + " : " + ex.Message);
					continue;
			} finally {
				xtr.Close ();
			}


			// Instance validation
			foreach (FileInfo inst in di.GetFiles ("*.xml")) {
				try {
					RelaxngValidatingReader vr = new RelaxngValidatingReader (new XmlTextReader (inst.FullName), p);
					while (!vr.EOF)
						vr.Read ();
					if (inst.Name.IndexOf ("i.") >= 0)
						Console.WriteLine ("Incorrectly validated instance: " + di.Name + "/" + inst.Name);
				} catch (RelaxngException ex) {
					if (inst.Name.IndexOf ("i.") >= 0)
						continue;
					Console.WriteLine ("Invalidated instance: " + di.Name + "/" + inst.Name + " : " + ex.Message);
				}
			}
		}
	}
}
