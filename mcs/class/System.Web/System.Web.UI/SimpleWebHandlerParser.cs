//
// System.Web.UI.SimpleWebHandlerParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	public abstract class SimpleWebHandlerParser
	{
		HttpContext context;
		string vPath;
		string physPath;
		string className;
		string codeBehind;
		bool debug;
		string language;
		string program;
		ArrayList elements;

		protected SimpleWebHandlerParser (HttpContext context, string virtualPath, string physicalPath)
		{
			this.context = context;
			this.vPath = virtualPath;
			this.physPath = physicalPath;
			GetDirectiveAndContent ();
		}

		private void GetDirectiveAndContent ()
		{
			StreamReader reader = new StreamReader (File.OpenRead (physPath));
			string line;
			bool directiveFound = false;
			StringBuilder content = new StringBuilder ();

			while ((line = reader.ReadLine ()) != null) {
				string trimmed = line.Trim ();
				if (!directiveFound && trimmed != String.Empty)
					continue;
				
				if (!directiveFound) {
					ParseDirective (trimmed);
					directiveFound = true;
					continue;
				}

				content.Append (line + "\n");
				content.Append (reader.ReadToEnd ());
			}

			this.program = content.ToString ();
			reader.Close ();
		}

		void ParseError (string msg, int line, int col)
		{
			string exc = String.Format ("parse error: {0}", msg);
			throw new HttpException (exc);
		}

	/*	void TagParsed (Tag tag, int line, int col)
		{
			elements.Add (tag);
		}*/

		void TextParsed (string msg, int line, int col)
		{
			//elements.Add (msg);
		}

		private void ParseDirective (string line)
		{
			/*byte [] bytes = WebEncoding.Encoding.GetBytes (line);
			//AspTokenizer tok = new AspTokenizer (physPath, new MemoryStream (bytes));
			//AspParser parser = new AspParser (tok);

			elements = new ArrayList ();
			parser.Error += new ParseErrorHandler (ParseError);
			parser.TagParsed += new TagParsedHandler (TagParsed);
			parser.TextParsed += new TextParsedHandler (TextParsed);

			parser.Parse ();

			if (elements.Count != 1)
				throw new ApplicationException ("Error looking for WebService directive.");

			Directive directive = elements [0] as Directive;
			if (directive == null)
				throw new ApplicationException ("Error looking for WebService directive.");

			if (0 != String.Compare (directive.TagID, DefaultDirectiveName, false))
				throw new ApplicationException ("Expecting @WebService. Got: " +
								directive.TagID);

			
			TagAttributes ta = directive.Attributes;
			className = ta ["class"] as string;
			if (className == null)
				throw new ApplicationException ("No Class attribute found.");
			
			string d = ta ["debug"] as string;
			if (d != null)
				debug = Convert.ToBoolean (d);

			language = ta ["language"] as string;
			if (language != null) {
				if (0 != String.Compare (language, "C#", false))
					throw new ApplicationException ("Only C# language is supported.");
			}

			codeBehind = ta ["codebehind"] as string;
			if (codeBehind != null) {
				string ext = Path.GetExtension (codeBehind);
				if (0 != String.Compare (ext, "cs", false) &&
				    0 != String.Compare (ext, "dll", false))
					throw new ApplicationException ("Unknown file type in CodeBehind.");

			}
			*/
		}

		protected abstract string DefaultDirectiveName { get; }

		internal HttpContext Context
		{
			get {
				return context;
			}
		}

		internal string VirtualPath
		{
			get {
				return vPath;
			}
		}

		internal string PhysicalPath
		{
			get {
				return physPath;
			}
		}

		internal string ClassName
		{
			get {
				return className;
			}
			
			set {
				className = value;
			}
		}

		internal string CodeBehind
		{
			get {
				return codeBehind;
			}
			
			set {
				codeBehind = value;
			}
		}

		internal bool Debug
		{
			get {
				return debug;
			}
			
			set {
				debug = value;
			}
		}

		internal string Language
		{
			get {
				return language;
			}
			
			set {
				language = value;
			}
		}

		internal string Program
		{
			get {
				return program;
			}
			
			set {
				program = value;
			}
		}
	}
}

