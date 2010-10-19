//
// ServiceHostParser.cs
//
// Author:
//	Ankit Jain	(jankit@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;

namespace System.ServiceModel.Channels {

	class ServiceHostParser
	{
		string file;
		string url;
		string type_name;
		string language;
		string factory;
		bool debug;
		bool got_default_directive;
		string program; // If program == null, we have to get the requested 'type_name' from the assemblies in bin
		ArrayList assemblies;
		HttpContext context;

		public ServiceHostParser (string file, string url, HttpContext context)
		{
			this.file = file;
			this.url = url;
			assemblies = new ArrayList ();
			assemblies.Add ("System.ServiceModel");
			this.context = context;
			CompilationSection section = (CompilationSection) context.GetSection ("system.web/compilation");
			language = section.DefaultLanguage;
		}

		public HttpContext Context {
			get { return context; }
		}

		public string Filename {
			get { return file; }
		}

		public string TypeName {
			get { return type_name; }
		}

		public bool Debug {
			get { return debug; }
		}

		public string Program {
			get { return program; }
		}

		public ArrayList Assemblies {
			get { return assemblies; }
		}

		public string Factory {
			get { return factory; }
		}

		public string Language {
			get { return language; }
		}

		public void Parse ()
		{
			using (StreamReader reader = new StreamReader (file)) {
				string line;
				bool directive_found = false;
				StringBuilder content = new StringBuilder ();
				StringBuilder directiveBuffer = null;

				while ((line = reader.ReadLine ()) != null) {
					string trimmed = line.Trim ();
					if (!directive_found && trimmed.Length == 0)
						continue;

					if (trimmed.StartsWith ("<%@")) {
						int directiveEnd = trimmed.IndexOf ("%>");

						if (directiveEnd == -1) {
							if (directiveBuffer == null)
								directiveBuffer = new StringBuilder ();
							directiveBuffer.Append (line);
							continue;
						}

						ParseDirective (trimmed);
						directive_found = true;
						continue;
					} else if (!directive_found && directiveBuffer != null) {
						int directiveEnd = trimmed.IndexOf ("%>");
						if (directiveEnd == -1) {
							directiveBuffer.Append (trimmed);
							continue;
						}

						directiveEnd += 2;
						int tlen = trimmed.Length;
						if (tlen > directiveEnd)
							content.Append (trimmed.Substring (directiveEnd) + "\n");

						directiveBuffer.Append (trimmed.Substring (0, directiveEnd));
						ParseDirective (directiveBuffer.ToString ());
						directive_found = true;
						directiveBuffer = null;
						continue;
					}

					content.Append (line + "\n");
					content.Append (reader.ReadToEnd ());
				}
				
				if (!got_default_directive)
					throw new Exception ("No @ServiceHost directive found");

				this.program = content.ToString ().Trim ();
				if (this.program.Trim ().Length == 0)
					this.program = null;
			}

			if (String.IsNullOrEmpty (Language))
				throw new Exception ("Language not specified.");
		}

		void ParseDirective (string line)
		{
			StringDictionary attributes = Split (line);

			//Directive
			if (String.Compare (attributes ["directive"], "ServiceHost", true) == 0) {
				got_default_directive = true;

				if (!attributes.ContainsKey ("SERVICE"))
					throw new Exception ("Service attribute not present in @ServiceHost directive.");
				else
					type_name = attributes ["SERVICE"];

				if (attributes.ContainsKey ("LANGUAGE"))
					language = attributes ["LANGUAGE"];

				if (attributes.ContainsKey ("FACTORY"))
					factory = attributes ["FACTORY"];

				if (attributes.ContainsKey ("DEBUG")) {
					if (String.Compare (attributes ["DEBUG"], "TRUE", true) == 0)
						debug = true;
					else if (String.Compare (attributes ["DEBUG"], "FALSE", true) == 0)
						debug = false;
					else
						throw new Exception (String.Format (
							"Invalid value for debug attribute : '{0}'", attributes ["DEBUG"]));
				}


				//FIXME: Other attributes, 
				return;
			}
			//FIXME: Other directives? Documentation doesn't mention any other

			throw new Exception (String.Format ("Cannot handle directive : '{0}'", attributes ["directive"]));
		}

		StringDictionary Split (string line)
		{
			line.Trim ();
			int end_pos = line.LastIndexOf ("%>");
			if (end_pos < 0)
				throw new Exception ("Directive must end with '%>'");

			StringDictionary table = new StringDictionary ();
			string content = line.Substring (3, end_pos - 3).Trim ();
			if (content.Length == 0)
				throw new Exception ("No directive found");

			int len = content.Length;
			int pos = 0;
			
			while (pos < len && content [pos] != ' ')
				pos ++;

			if (pos >= len) {
				table ["directive"] = content;
				return table;
			}

			table ["directive"] = content.Substring (0, pos);

			content = content.Substring (pos);

			len = content.Length;
			pos = 0;
			while (pos < len) {
				//skip spaces	
				while (content [pos] == ' ' && pos < len)
					pos ++;

				int eq_pos = content.IndexOf ('=', pos);
				string key = content.Substring (pos, eq_pos - pos).Trim ();

				pos = eq_pos + 1;
				int start_quote = content.IndexOf ('"', pos);
				int end_quote = content.IndexOf ('"', start_quote + 1);

				string val = content.Substring (start_quote + 1, end_quote - start_quote - 1).Trim ();

				pos = end_quote + 1;
				table [key.ToUpper ()] = val;
			}

			return table;
		}

	}
}
