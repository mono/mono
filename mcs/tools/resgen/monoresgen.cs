/*
 * resgen: convert between the resource formats (.txt, .resources, .resx).
 *
 * Copyright (c) 2002 Ximian, Inc
 *
 * Authors:
 *	Paolo Molaro (lupus@ximian.com)
 *	Gonzalo Paniagua Javier (gonzalo@ximian.com)
 */

using System;
using System.Globalization;
using System.Text;
using System.IO;
using System.Collections;
using System.Resources;
using System.Reflection;
using System.Xml;

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

		string Usage = @"Mono Resource Generator version " + Consts.MonoVersion +
		    @"
Usage:
		resgen source.ext [dest.ext]
		resgen [options] /compile source.ext[,dest.resources] [...]";
		Usage += @"

Convert a resource file from one format to another.
The currently supported formats are: '.txt' '.resources' '.resx' '.po'.
If the destination file is not specified, source.resources will be used.";

		Usage += @"

Options:
-compile, /compile
	takes a list of .resX or .txt files to convert to .resources files
	in one bulk operation, replacing .ext with .resources for the 
	output file name (if not set).
-usesourcepath, /useSourcePath
	to resolve relative file paths, use the directory of the resource 
	file as current directory.";
		Usage += @"
";
		Console.WriteLine( Usage );
	}
	
	static IResourceReader GetReader (Stream stream, string name, bool useSourcePath) {
		string format = Path.GetExtension (name);
		switch (format.ToLower (System.Globalization.CultureInfo.InvariantCulture)) {
		case ".po":
			return new PoResourceReader (stream);
		case ".txt":
		case ".text":
			return new TxtResourceReader (stream);
		case ".resources":
			return new ResourceReader (stream);
		case ".resx":
			LoadResX ();
			IResourceReader reader = (IResourceReader) Activator.CreateInstance (
				resxr, new object[] {stream});
			if (useSourcePath) { // only possible on 2.0 profile, or higher
				PropertyInfo p = reader.GetType ().GetProperty ("BasePath",
					BindingFlags.Public | BindingFlags.Instance);
				if (p != null && p.CanWrite) {
					p.SetValue (reader, Path.GetDirectoryName (name), null);
				}
			}
			return reader;
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
	
	static int CompileResourceFile (string sname, string dname, bool useSourcePath) {
		FileStream source = null;
		FileStream dest = null;
		IResourceReader reader = null;
		IResourceWriter writer = null;

		try {
			source = new FileStream (sname, FileMode.Open, FileAccess.Read);
			reader = GetReader (source, sname, useSourcePath);

			dest = new FileStream (dname, FileMode.Create, FileAccess.Write);
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

			// under 2.0 ResXResourceReader can wrap an exception into an XmlException
			// and this hides some helpful message from the original exception
			XmlException xex = (inner as XmlException);
			if (xex != null) {
				// message is identical to the inner exception (from MWF ResXResourceReader)
				Console.WriteLine ("Position: Line {0}, Column {1}.", xex.LineNumber, xex.LinePosition);
				inner = inner.InnerException;
			}

			if (inner is TargetInvocationException && inner.InnerException != null)
				inner = inner.InnerException;
			if (inner != null)
				Console.WriteLine ("Inner exception: {0}", inner.Message);

			if (reader != null)
				reader.Dispose ();
			if (source != null)
				source.Close ();
			if (writer != null)
				writer.Dispose ();
			if (dest != null)
				dest.Close ();

			// since we're not first reading all entries in source, we may get a
			// read failure after we're started writing to the destination file
			// and leave behind a broken resources file, so remove it here
			try {
				File.Delete (dname);
			} catch {
			}
			return 1;
		}
		return 0;
	}
	
	static int Main (string[] args) {
		bool compileMultiple = false;
		bool useSourcePath = false;
		ArrayList inputFiles = new ArrayList ();

		for (int i = 0; i < args.Length; i++) {
			switch (args [i].ToLower ()) {
			case "-h":
			case "/h":
			case "-?":
			case "/?":
				Usage ();
				return 1;
			case "/compile":
			case "-compile":
				if (inputFiles.Count > 0) {
					// the /compile option should be specified before any files
					Usage ();
					return 1;
				}
				compileMultiple = true;
				break;

			case "/usesourcepath":
			case "-usesourcepath":
				if (compileMultiple) {
					// the /usesourcepath option should not appear after the
					// /compile switch on the command-line
					Console.WriteLine ("ResGen : error RG0000: Invalid "
						+ "command line syntax.  Switch: \"/compile\"  Bad value: "
						+ args [i] + ".  Use ResGen /? for usage information.");
					return 1;
				}
				useSourcePath = true;
				break;

			default:
				if (!IsFileArgument (args [i])) {
					Usage ();
					return 1;
				}

				ResourceInfo resInf = new ResourceInfo ();
				if (compileMultiple) {
					string [] pair = args [i].Split (',');
					switch (pair.Length) {
					case 1:
						resInf.InputFile = Path.GetFullPath (pair [0]);
						resInf.OutputFile = Path.ChangeExtension (resInf.InputFile,
							"resources");
						break;
					case 2:
						if (pair [1].Length == 0) {
							Console.WriteLine (@"error: You must specify an input & outfile file name like this:");
							Console.WriteLine ("inFile.txt,outFile.resources.");
							Console.WriteLine ("You passed in '{0}'.", args [i]);
							return 1;
						}
						resInf.InputFile = Path.GetFullPath (pair [0]);
						resInf.OutputFile = Path.GetFullPath (pair [1]);
						break;
					default:
						Usage ();
						return 1;
					}
				} else {
					if ((i + 1) < args.Length) {
						resInf.InputFile = Path.GetFullPath (args [i]);
						// move to next arg, since we assume that one holds
						// the name of the output file
						i++;
						resInf.OutputFile = Path.GetFullPath (args [i]);
					} else {
						resInf.InputFile = Path.GetFullPath (args [i]);
						resInf.OutputFile = Path.ChangeExtension (resInf.InputFile,
							"resources");
					}
				}
				inputFiles.Add (resInf);
				break;
			}
		}

		if (inputFiles.Count == 0) {
			Usage ();
			return 1;
		}

		foreach (ResourceInfo res in inputFiles) {
			int ret = CompileResourceFile (res.InputFile, res.OutputFile, useSourcePath);
			if (ret != 0 )
				return ret;
		}
		return 0;
	}

	private static bool RunningOnUnix {
		get {
			// check for Unix platforms - see FAQ for more details
			// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
			int platform = (int) Environment.OSVersion.Platform;
			return ((platform == 4) || (platform == 128) || (platform == 6));
		}
	}

	private static bool IsFileArgument (string arg)
	{
		if ((arg [0] != '-') && (arg [0] != '/'))
			return true;

		// cope with absolute filenames for resx files on unix, as
		// they also match the option pattern
		//
		// `/home/test.resx' is considered as a resx file, however
		// '/test.resx' is considered as error
		return (RunningOnUnix && arg.Length > 2 && arg.IndexOf ('/', 2) != -1);
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
	
	public void AddResource (string name, string value) {
		s.WriteLine ("{0}={1}", name, Escape (value));
	}

	// \n -> \\n ...
	static string Escape (string value)
	{
		StringBuilder b = new StringBuilder ();
		for (int i = 0; i < value.Length; i++) {
			switch (value [i]) {
			case '\n':
				b.Append ("\\n");
				break;
			case '\r':
				b.Append ("\\r");
				break;
			case '\t':
				b.Append ("\\t");
				break;
			case '\\':
				b.Append ("\\\\");
				break;
			default:
				b.Append (value [i]);
				break;
			}
		}
		return b.ToString ();
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

			val = Unescape (val);
			if (val == null)
				throw new Exception (String.Format ("Unsupported escape character in value of key '{0}'.", key));


			data.Add (key, val);
		}
	}

	// \\n -> \n ...
	static string Unescape (string value)
	{
		StringBuilder b = new StringBuilder ();

		for (int i = 0; i < value.Length; i++) {
			if (value [i] == '\\') {
				if (i == value.Length - 1)
					return null;

				i++;
				switch (value [i]) {
				case 'n':
					b.Append ('\n');
					break;
				case 'r':
					b.Append ('\r');
					break;
				case 't':
					b.Append ('\t');
					break;
				case 'u':
					int ch = int.Parse (value.Substring (++i, 4), NumberStyles.HexNumber);
					b.Append (char.ConvertFromUtf32 (ch));
					i += 3;
					break;
				case '\\':
					b.Append ('\\');
					break;
				default:
					return null;
				}

			} else {
				b.Append (value [i]);
			}
		}

		return b.ToString ();
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
		s.WriteLine ("\"X-Generator: Mono resgen 0.1\\n\"");
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

class ResourceInfo
{
	public string InputFile;
	public string OutputFile;
}
