/*
 * monoresgen: convert between the resource formats (.txt, .resources, .resx).
 *
 * Copyright (c) 2002 Ximian, Inc
 *
 * Authors:
 *	Paolo Molaro (lupus@ximian.com)
 *	Gonzalo Paniagua Javier (gonzalo@ximian.com)
 */

/*
 * TODO:
 * * escape/unescape in the .txt reader/writer to be able to roundtrip values with newlines
 *   (unlike the MS ResGen utility)
 */

using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Resources;
using System.Reflection;

class ResGen {

	static Assembly swf;
	static Type resxr;
	static Type resxw;

	/*
	 * We load the ResX format stuff on demand, since the classes are in 
	 * System.Windows.Forms (!!!) and we can't depend on that assembly in mono, yet.
	 */
	static void LoadResX () {
		if (swf != null)
			return;
		try {
			swf = Assembly.Load (Consts.AssemblySystem_Windows_Forms);
			resxr = swf.GetType ("System.Resources.ResXResourceReader");
			resxw = swf.GetType ("System.Resources.ResXResourceWriter");
		} catch (Exception e) {
			throw new Exception ("Cannot load support for ResX format: " + e.Message);
		}
	}

	static void Usage () {
		string Usage = @"Mono Resource Generator version 0.1
Usage:
		monoresgen source.ext [dest.ext]
		monoresgen /compile source.ext[,dest.resources] [...]

Convert a resource file from one format to another.
The currently supported formats are: '.txt' '.resources' '.resx' '.po'.
If the destination file is not specified, source.resources will be used.
The /compile option takes a list of .resX or .txt files to convert to
.resources files in one bulk operation, replacing .ext with .resources for
the output file name.
";
		Console.WriteLine( Usage );
	}
	
	static IResourceReader GetReader (Stream stream, string name) {
		string format = Path.GetExtension (name);
		switch (format.ToLower ()) {
		case ".po":
			return new PoResourceReader (stream);
		case ".txt":
		case ".text":
			return new TxtResourceReader (stream);
		case ".resources":
			return new ResourceReader (stream);
		case ".resx":
			LoadResX ();
			return (IResourceReader)Activator.CreateInstance (resxr, new object[] {stream});
		default:
			throw new Exception ("Unknown format in file " + name);
		}
	}
	
	static IResourceWriter GetWriter (Stream stream, string name) {
		string format = Path.GetExtension (name);
		switch (format.ToLower ()) {
		case ".po":
			return new PoResourceWriter (stream);
		case ".txt":
		case ".text":
			return new TxtResourceWriter (stream);
		case ".resources":
			return new ResourceWriter (stream);
		case ".resx":
			LoadResX ();
			return (IResourceWriter)Activator.CreateInstance (resxw, new object[] {stream});
		default:
			throw new Exception ("Unknown format in file " + name);
		}
	}
	
	static int CompileResourceFile(string sname, string dname ) {
		FileStream source, dest;
		IResourceReader reader;
		IResourceWriter writer;

		try {
			source = new FileStream (sname, FileMode.Open, FileAccess.Read);

			reader = GetReader (source, sname);

			dest = new FileStream (dname, FileMode.OpenOrCreate, FileAccess.Write);
			writer = GetWriter (dest, dname);

			int rescount = 0;
			foreach (DictionaryEntry e in reader) {
				rescount++;
				object val = e.Value;
				if (val is string)
					writer.AddResource ((string)e.Key, (string)e.Value);
				else
					writer.AddResource ((string)e.Key, e.Value);
			}
			Console.WriteLine( "Read in {0} resources from '{1}'", rescount, sname );

			reader.Close ();
			writer.Close ();
			Console.WriteLine("Writing resource file...  Done.");
		} catch (Exception e) {
			Console.WriteLine ("Error: {0}", e.Message);
			Exception inner = e.InnerException;
			if (inner != null)
				Console.WriteLine ("Inner exception: {0}", inner.Message);
			return 1;
		}
		return 0;
	}
	
	static int Main (string[] args) {
		string sname = "", dname = ""; 
		if ((int) args.Length < 1 || args[0] == "-h" || args[0] == "-?" || args[0] == "/h" || args[0] == "/?") {
			  Usage();
			  return 1;
		}		
		if (args[0] == "/compile" || args[0] == "-compile") {
			for ( int i=1; i< args.Length; i++ ) {				
				if ( args[i].IndexOf(",") != -1 ){
					string[] pair =  args[i].Split(',');
					sname = pair[0]; 
					dname = pair[1];
					if (dname == ""){
						Console.WriteLine(@"error: You must specify an input & outfile file name like this:");
						Console.WriteLine("inFile.txt,outFile.resources." );
						Console.WriteLine("You passed in '{0}'.", args[i] );
						return 1;
					}
				} else {
					sname = args[i]; 
					dname = Path.ChangeExtension (sname, "resources");
				}
				int ret = CompileResourceFile( sname, dname );
				if (ret != 0 ) {
					return ret;
				}
			}
			return 0;
		
		}
		else if (args.Length == 1) {
			sname = args [0];
			dname = Path.ChangeExtension (sname, "resources");
		} else if (args.Length != 2) {
			Usage ();
			return 1;
		} else {
			sname = args [0];
			dname = args [1];			
		}		
		return CompileResourceFile( sname, dname );
	}
}

class TxtResourceWriter : IResourceWriter {
	StreamWriter s;
	
	public TxtResourceWriter (Stream stream) {
		s = new StreamWriter (stream);
	}
	
	public void AddResource (string name, byte[] value) {
		throw new Exception ("Binary data not valid in a text resource file");
	}
	
	public void AddResource (string name, object value) {
		if (value is string) {
			AddResource (name, (string)value);
			return;
		}
		throw new Exception ("Objects not valid in a text resource file");
	}
	
	/* FIXME: handle newlines */
	public void AddResource (string name, string value) {
		s.WriteLine ("{0}={1}", name, value);
	}
	
	public void Close () {
		s.Close ();
	}
	
	public void Dispose () {}
	
	public void Generate () {}
}

class TxtResourceReader : IResourceReader {
	Hashtable data;
	Stream s;
	
	public TxtResourceReader (Stream stream) {
		data = new Hashtable ();
		s = stream;
		Load ();
	}
	
	public virtual void Close () {
	}
	
	public IDictionaryEnumerator GetEnumerator() {
		return data.GetEnumerator ();
	}
	
	void Load () {
		StreamReader reader = new StreamReader (s);
		string line, key, val;
		int epos, line_num = 0;
		while ((line = reader.ReadLine ()) != null) {
			line_num++;
			line = line.Trim ();
			if (line.Length == 0 || line [0] == '#' ||
			    line [0] == ';')
				continue;
			epos = line.IndexOf ('=');
			if (epos < 0) 
				throw new Exception ("Invalid format at line " + line_num);
			key = line.Substring (0, epos);
			val = line.Substring (epos + 1);
			key = key.Trim ();
			val = val.Trim ();
			if (key.Length == 0) 
				throw new Exception ("Key is empty at line " + line_num);
			data.Add (key, val);
		}
	}
	
	IEnumerator IEnumerable.GetEnumerator () {
		return ((IResourceReader) this).GetEnumerator();
	}

	void IDisposable.Dispose () {}
}

class PoResourceReader : IResourceReader {
	Hashtable data;
	Stream s;
	int line_num;
	
	public PoResourceReader (Stream stream)
	{
		data = new Hashtable ();
		s = stream;
		Load ();
	}
	
	public virtual void Close ()
	{
		s.Close ();
	}
	
	public IDictionaryEnumerator GetEnumerator()
	{
		return data.GetEnumerator ();
	}
	
	string GetValue (string line)
	{
		int begin = line.IndexOf ('"');
		if (begin == -1)
			throw new FormatException (String.Format ("No begin quote at line {0}: {1}", line_num, line));

		int end = line.LastIndexOf ('"');
		if (end == -1)
			throw new FormatException (String.Format ("No closing quote at line {0}: {1}", line_num, line));

		return line.Substring (begin + 1, end - begin - 1);
	}
	
	void Load ()
	{
		StreamReader reader = new StreamReader (s);
		string line;
		string msgid = null;
		string msgstr = null;
		bool ignoreNext = false;

		while ((line = reader.ReadLine ()) != null) {
			line_num++;
			line = line.Trim ();
			if (line.Length == 0)
				continue;
				
			if (line [0] == '#') {
				if (line.Length == 1 || line [1] != ',')
					continue;

				if (line.IndexOf ("fuzzy") != -1) {
					ignoreNext = true;
					if (msgid != null) {
						if (msgstr == null)
							throw new FormatException ("Error. Line: " + line_num);
						data.Add (msgid, msgstr);
						msgid = null;
						msgstr = null;
					}
				}
				continue;
			}
			
			if (line.StartsWith ("msgid ")) {
				if (msgid == null && msgstr != null)
					throw new FormatException ("Found 2 consecutive msgid. Line: " + line_num);

				if (msgstr != null) {
					if (!ignoreNext)
						data.Add (msgid, msgstr);

					ignoreNext = false;
					msgid = null;
					msgstr = null;
				}

				msgid = GetValue (line);
				continue;
			}

			if (line.StartsWith ("msgstr ")) {
				if (msgid == null)
					throw new FormatException ("msgstr with no msgid. Line: " + line_num);

				msgstr = GetValue (line);
				continue;
			}

			if (line [0] == '"') {
				if (msgid == null || msgstr == null)
					throw new FormatException ("Invalid format. Line: " + line_num);

				msgstr += GetValue (line);
				continue;
			}

			throw new FormatException ("Unexpected data. Line: " + line_num);
		}

		if (msgid != null) {
			if (msgstr == null)
				throw new FormatException ("Expecting msgstr. Line: " + line_num);

			if (!ignoreNext)
				data.Add (msgid, msgstr);
		}
	}
	
	IEnumerator IEnumerable.GetEnumerator ()
	{
		return GetEnumerator();
	}

	void IDisposable.Dispose ()
	{
		if (data != null)
			data = null;

		if (s != null) {
			s.Close ();
			s = null;
		}
	}
}

class PoResourceWriter : IResourceWriter
{
	TextWriter s;
	bool headerWritten;
	
	public PoResourceWriter (Stream stream)
	{
		s = new StreamWriter (stream);
	}
	
	public void AddResource (string name, byte [] value)
	{
		throw new InvalidOperationException ("Binary data not valid in a po resource file");
	}
	
	public void AddResource (string name, object value)
	{
		if (value is string) {
			AddResource (name, (string) value);
			return;
		}
		throw new InvalidOperationException ("Objects not valid in a po resource file");
	}

	StringBuilder ebuilder = new StringBuilder ();
	
	public string Escape (string ns)
	{
		ebuilder.Length = 0;

		foreach (char c in ns){
			switch (c){
			case '"':
			case '\\':
				ebuilder.Append ('\\');
				ebuilder.Append (c);
				break;
			case '\a':
				ebuilder.Append ("\\a");
				break;
			case '\n':
				ebuilder.Append ("\\n");
				break;
			case '\r':
				ebuilder.Append ("\\r");
				break;
			default:
				ebuilder.Append (c);
				break;
			}
		}
		return ebuilder.ToString ();
	}
	
	public void AddResource (string name, string value)
	{
		if (!headerWritten) {
			headerWritten = true;
			WriteHeader ();
		}
		
		s.WriteLine ("msgid \"{0}\"", Escape (name));
		s.WriteLine ("msgstr \"{0}\"", Escape (value));
		s.WriteLine ("");
	}
	
	void WriteHeader ()
	{
		s.WriteLine ("msgid \"\"");
		s.WriteLine ("msgstr \"\"");
		s.WriteLine ("\"MIME-Version: 1.0\\n\"");
		s.WriteLine ("\"Content-Type: text/plain; charset=UTF-8\\n\"");
		s.WriteLine ("\"Content-Transfer-Encoding: 8bit\\n\"");
		s.WriteLine ("\"X-Generator: monoresgen 0.1\\n\"");
		s.WriteLine ("#\"Project-Id-Version: FILLME\\n\"");
		s.WriteLine ("#\"POT-Creation-Date: yyyy-MM-dd HH:MM+zzzz\\n\"");
		s.WriteLine ("#\"PO-Revision-Date: yyyy-MM-dd HH:MM+zzzz\\n\"");
		s.WriteLine ("#\"Last-Translator: FILLME\\n\"");
		s.WriteLine ("#\"Language-Team: FILLME\\n\"");
		s.WriteLine ("#\"Report-Msgid-Bugs-To: \\n\"");
		s.WriteLine ();
	}

	public void Close ()
	{
		s.Close ();
	}
	
	public void Dispose () { }
	
	public void Generate () {}
}

