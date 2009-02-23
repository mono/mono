using System;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Derivative;

public class Test
{
	static char SEP = Path.DirectorySeparatorChar;
	static bool skip_error = true;

	public static void Main (string [] args)
	{
		if (args.Length > 0 && args [0] == "--skip-error")
			skip_error = true;

Console.WriteLine ("Started:  " + DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss.fff"));
		RunTest ();
Console.WriteLine ("Finished: " + DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss.fff"));
	}

	static void RunTest ()
	{
		foreach (DirectoryInfo di in new DirectoryInfo (@"relax-ng").GetDirectories ()) {
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
					if (skip_error)
						vr.InvalidNodeFound += RelaxngValidatingReader.IgnoreError;
					while (!vr.EOF)
						vr.Read ();
					if (inst.Name.IndexOf ("i.") >= 0 && !skip_error)
						Console.WriteLine ("Incorrectly validated instance: " + di.Name + "/" + inst.Name);
				} catch (RelaxngException ex) {
					string path = di.Name + "/" + inst.Name;
					if (skip_error)
						Console.WriteLine ("Failed to skip error : " + path + ex.Message);
					if (inst.Name.IndexOf ("i.") >= 0)
						continue;
					Console.WriteLine ("Invalidated instance: " + path + " : " + ex.Message);
				}
			}
		}
	}
}
