using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.Text;

namespace XmlNormalizer {
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class XmlNormalizer {
		class OptionLetterAttribute:Attribute{
			char _c;
			public OptionLetterAttribute(char c):base(){
				_c = c;
			}
			public override string ToString() {
				return _c.ToString();
			}
		}

		XmlDocument doc;
		bool _removeWhiteSpace;
		bool _sortAttributes;
		bool _removeAttributes;
		bool _removeNamespacesAndPrefixes;
		bool _removeText;
		bool _removeAll;
		bool _newLines;

		[OptionLetter('w')]
		[Description("remove white space")]
		public bool RemoveWhiteSpace {
			get {return _removeWhiteSpace;}
			set {_removeWhiteSpace=value;}
		}
		[OptionLetter('s')]
		[Description("sort attributes")]
		public bool SortAttributes {
			get {return _sortAttributes;}
			set {_sortAttributes=value;}
		}
		[OptionLetter('a')]
		[Description("remove attributes")]
		public bool RemoveAttributes {
			get {return _removeAttributes;}
			set {_removeAttributes=value;}
		}
		[OptionLetter('p')]
		[Description("remove namespaces and prefixes")]
		public bool RemoveNamespacesAndPrefixes {
			get {return _removeNamespacesAndPrefixes;}
			set {_removeNamespacesAndPrefixes=value;}
		}
		[OptionLetter('t')]
		[Description("remove text nodes")]
		public bool RemoveText {
			get {return _removeText;}
			set {_removeText=value;}
		}
		[OptionLetter('n')]
		[Description("remove all except element nodes")]
		public bool RemoveAll {
			get {return _removeAll;}
			set {_removeAll=value;}
		}
		[OptionLetter('x')]
		[Description("insert newlines before elements")]
		public bool NewLines {
			get {return _newLines;}
			set {_newLines=value;}
		}
		[OptionLetter('m')]
		[Description("minimal normalizing")]
		public bool MinimalNormalizing {
			get {return false;}
		}

		public XmlNormalizer ()
			:this ("") {
		}

		public XmlNormalizer (string options) {
			ParseOptions(options);
		}

		public void Process(TextReader rd) {
			doc=new XmlDocument();
			doc.PreserveWhitespace = true;

			string fileContents = rd.ReadToEnd();
			
			try {
				doc.LoadXml (fileContents);
			}
			catch (Exception x) {
				StringBuilder sb = new StringBuilder ();
				sb.Append ("<NormalizerRoot>");
				sb.Append (fileContents);
				sb.Append ("</NormalizerRoot>");
				doc.LoadXml (sb.ToString ());
			}

		
			if (RemoveText)
				RemoveWhiteSpace = true;

			if (RemoveAll)
				RemoveNamespacesAndPrefixes = true;

			XmlDocument newDoc = new XmlDocument();

			CopyNodes(newDoc, doc, newDoc);

			doc=newDoc;
		}

		void CopyNodes (XmlDocument newDoc, XmlNode fromParent, XmlNode toParent) {
			if (fromParent.HasChildNodes)
				foreach (XmlNode c in fromParent.ChildNodes)
					CopyNode (newDoc, c, toParent);

			if (fromParent.Attributes != null) {
				string [] keys = new string [fromParent.Attributes.Count];

				for (int i=0; i<fromParent.Attributes.Count; i++) {
					keys[i] = fromParent.Attributes[i].Name;
				}
				if (SortAttributes){ 
					Array.Sort(keys);
				}
				for (int i=0; i<keys.Length; i++) {
					CopyNode (newDoc, fromParent.Attributes[keys[i]], toParent);
				}
			}
		}

		void CopyNode (XmlDocument newDoc, XmlNode from, XmlNode toParent) {
			if (RemoveAll && from.NodeType != XmlNodeType.Element)
				return;

			XmlNode child = null;
			bool newLineNode = false;
			
			switch (from.NodeType) {
				case XmlNodeType.Element: 
					newLineNode = true;
					if (RemoveNamespacesAndPrefixes)
						child = newDoc.CreateElement (from.LocalName);
					else {
						XmlElement e = from as XmlElement;
						child = newDoc.CreateElement (e.Prefix, e.LocalName, e.NamespaceURI);
					}
					break;
				case XmlNodeType.Attribute: {
					if (RemoveAttributes)
						return;

					XmlAttribute fromAttr = from as XmlAttribute;
					if (!fromAttr.Specified)
						return;

					XmlAttribute a;

					if (RemoveNamespacesAndPrefixes)
						a = newDoc.CreateAttribute (fromAttr.LocalName);
					else
						a = newDoc.CreateAttribute (fromAttr.Prefix, fromAttr.LocalName, fromAttr.NamespaceURI);
					
					toParent.Attributes.Append(a);
					CopyNodes (newDoc, from, a);
					return;
				}
				case XmlNodeType.CDATA:
					newLineNode = true;
					child = newDoc.CreateCDataSection ((from as XmlCDataSection).Data);
					break;
				case XmlNodeType.Comment:
					if (RemoveWhiteSpace)
						return;
					newLineNode = true;
					child = newDoc.CreateComment ((from as XmlComment).Data);
					break;
				case XmlNodeType.ProcessingInstruction:
					newLineNode = true;
					XmlProcessingInstruction pi = from as XmlProcessingInstruction;
					child = newDoc.CreateProcessingInstruction (pi.Target, pi.Data);
					break;
				case XmlNodeType.DocumentType:
					newLineNode = true;
					toParent.AppendChild (from.CloneNode (true));
					return;
				case XmlNodeType.EntityReference:
					child = newDoc.CreateEntityReference ((from as XmlEntityReference).Name);
					break;
				case XmlNodeType.SignificantWhitespace:
					if (RemoveWhiteSpace)
						return;
					child = newDoc.CreateSignificantWhitespace (from.Value);
					break;
				case XmlNodeType.Text:
					if (RemoveText)
						return;
					newLineNode = true;
					child = newDoc.CreateTextNode (from.Value);
					break;
				case XmlNodeType.Whitespace:
					if (RemoveWhiteSpace)
						return;
					child = newDoc.CreateWhitespace (from.Value);
					break;
				case XmlNodeType.XmlDeclaration:
					newLineNode = true;
					XmlDeclaration d = from as XmlDeclaration;
					XmlDeclaration d1 = newDoc.CreateXmlDeclaration (d.Version, d.Encoding, d.Standalone);
					newDoc.InsertBefore(d1, newDoc.DocumentElement);
					return;
			}
			if (NewLines && newLineNode && toParent.NodeType != XmlNodeType.Attribute) {
				XmlSignificantWhitespace s = newDoc.CreateSignificantWhitespace("\r\n");
				toParent.AppendChild (s);
			}
			toParent.AppendChild(child);
			CopyNodes (newDoc, from, child);
		}

		public void ParseOptions (string options) {
			_removeWhiteSpace = false;
			_sortAttributes = false;
			_removeAttributes = false;
			_removeNamespacesAndPrefixes = false;
			_removeText = false;
			_removeAll = false;
			_newLines = false;
			foreach (PropertyInfo pi in typeof (XmlNormalizer).GetProperties()) {
				string option = pi.GetCustomAttributes(typeof(OptionLetterAttribute),true)[0].ToString();
				if (options.IndexOf(option) == -1)
					continue;
				pi.GetSetMethod().Invoke (this, new object [] {true});
			}
		}

		public static Hashtable GetOptions() {
			Hashtable h = new Hashtable();

			foreach (PropertyInfo pi in typeof (XmlNormalizer).GetProperties()) {
				string option = pi.GetCustomAttributes(typeof(OptionLetterAttribute),true)[0].ToString();
				string descr = (pi.GetCustomAttributes(typeof(DescriptionAttribute), true)[0] as DescriptionAttribute).Description;
				h[option] = descr;
			}
			return h;
		}

		public void Output(XmlWriter wr) {
			doc.WriteTo(wr);
		}

		public void Output(TextWriter wr) {
			Output (new XmlTextWriter (wr));
		}
		
		void ProcessFile (string inputfile, string outputfile) {
			StreamWriter wr = null;
			StreamReader rd = null;
			try {
				wr = new StreamWriter (outputfile);
				rd = new StreamReader (inputfile);
				ProcessFile (rd, wr);
			} catch (Exception) {
				if (wr != null)
					wr.Close ();
				if (rd != null)
					rd.Close ();
				wr = null;
				rd = null;
				File.Copy (inputfile, outputfile, true);
			} finally {
				if (wr != null)
					wr.Close ();
				if (rd != null)
					rd.Close ();
			}
		}

		void ProcessFile (TextReader input, TextWriter output) {
			XmlTextWriter xwr = new XmlTextWriter (output);
			
			Process (input);
			Output (xwr);
		}
		
		void ProcessDirectory (string inputdir, string outputdir) {
			if (!Directory.Exists (outputdir))
				Directory.CreateDirectory (outputdir);
			DirectoryInfo idi = new DirectoryInfo(inputdir);
			foreach (FileInfo fi in idi.GetFiles()) {
				string outputfile = Path.Combine(outputdir, fi.Name);
				ProcessFile (fi.FullName, outputfile);
			}
			foreach (DirectoryInfo di in idi.GetDirectories())
				ProcessDirectory (di.FullName, Path.Combine(outputdir, di.Name));
		}
#if !XML_NORMALIZER_NO_MAIN
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args) {
			if (args.Length < 2 || args[0].Length < 2 || args[0][0] != '-') {
				PrintUsage();
				return 1;
			}
			XmlNormalizer norm = new XmlNormalizer (args[0].Substring(1));
			if (File.Exists(args[1])) {
				if (args.Length != 2) {
					PrintUsage();
					return 1;
				}
				norm.ProcessFile(new StreamReader (args[1]), Console.Out);
			}
			else if (Directory.Exists (args[1])) {
				if (args.Length != 3) {
					PrintUsage();
					return 1;
				}
				norm.ProcessDirectory (args[1], args[2]);
			}
			else {
				Console.Error.WriteLine("Path not found: {0}", args[1]);
				return 2;
			}
			return 0;
		}
		static void PrintUsage () {
			Console.Error.WriteLine("Usage: xmlnorm -<flags> <inputfile>");
			Console.Error.WriteLine("Or: xmlnorm -<flags> <inputdir> <outputdir>");
			Console.Error.WriteLine("\tFlags:");
			foreach (DictionaryEntry de in XmlNormalizer.GetOptions())
				Console.Error.WriteLine ("\t{0}\t{1}", de.Key, de.Value);
		}
#endif
	}

}
