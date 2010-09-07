//
// System.Web.Compilation.AspGenerator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Marek Habersack <mhabersack@novell.com>
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2004-2009 Novell, Inc (http://www.novell.com)
//

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.Util;

namespace System.Web.Compilation
{
	class BuilderLocation
	{
		public ControlBuilder Builder;
		public ILocation Location;

		public BuilderLocation (ControlBuilder builder, ILocation location)
		{
			this.Builder = builder;
			this.Location = new Location (location);
		}
	}

	class BuilderLocationStack : Stack
	{
		public override void Push (object o)
		{
			if (!(o is BuilderLocation))
				throw new InvalidOperationException ();

			base.Push (o);
		}
		
		public virtual void Push (ControlBuilder builder, ILocation location)
		{
			BuilderLocation bl = new BuilderLocation (builder, location);
			Push (bl);
		}

		public new BuilderLocation Peek ()
		{
			return (BuilderLocation) base.Peek ();
		}

		public new BuilderLocation Pop ()
		{
			return (BuilderLocation) base.Pop ();
		}

		public ControlBuilder Builder {
			get { return Peek ().Builder; }
		}
	}

	class ParserStack
	{
		Hashtable files;
		Stack parsers;
		AspParser current;

		public ParserStack ()
		{
			files = new Hashtable (); // may be this should be case sensitive for windows
			parsers = new Stack ();
		}
		
		public bool Push (AspParser parser)
		{
			if (files.Contains (parser.Filename))
				return false;

			files [parser.Filename] = true;
			parsers.Push (parser);
			current = parser;
			return true;
		}

		public AspParser Pop ()
		{
			if (parsers.Count == 0)
				return null;

			files.Remove (current.Filename);
			AspParser result = (AspParser) parsers.Pop ();
			if (parsers.Count > 0)
				current = (AspParser) parsers.Peek ();
			else
				current = null;

			return result;
		}

		public int Count {
			get { return parsers.Count; }
		}
		
		public AspParser Parser {
			get { return current; }
		}

		public string Filename {
			get { return current.Filename; }
		}
	}

	class TagStack
	{
		Stack tags;

		public TagStack ()
		{
			tags = new Stack ();
		}
		
		public void Push (string tagid)
		{
			tags.Push (tagid);
		}

		public string Pop ()
		{
			if (tags.Count == 0)
				return null;

			return (string) tags.Pop ();
		}

		public bool CompareTo (string tagid)
		{
			if (tags.Count == 0)
				return false;

			return 0 == String.Compare (tagid, (string) tags.Peek (), true, Helpers.InvariantCulture);
		}
		
		public int Count {
			get { return tags.Count; }
		}

		public string Current {
			get { return (string) tags.Peek (); }
		}
	}

	enum TextBlockType
	{
		Verbatim,
		Expression,
		Tag,
		Comment
	}
	
	sealed class TextBlock
	{
		public string Content;
		public readonly TextBlockType Type;
		public readonly int Length;
		
		public TextBlock (TextBlockType type, string content)
		{
			Content = content;
			Type = type;
			Length = content.Length;
		}

		public override string ToString ()
		{
			return this.GetType ().FullName + " [" + this.Type + "]";
		}
	}
	
	class AspGenerator
	{
		const int READ_BUFFER_SIZE = 8192;
		
		internal static Regex DirectiveRegex = new Regex (@"<%\s*@(\s*(?<attrname>\w[\w:]*(?=\W))(\s*(?<equal>=)\s*""(?<attrval>[^""]*)""|\s*(?<equal>=)\s*'(?<attrval>[^']*)'|\s*(?<equal>=)\s*(?<attrval>[^\s%>]*)|(?<equal>)(?<attrval>\s*?)))*\s*?%>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static readonly Regex runatServer = new Regex (@"<[\w:\.]+.*?runat=[""']?server[""']?.*(?:/>|>)",
							       RegexOptions.Compiled | RegexOptions.Singleline |
							       RegexOptions.Multiline | RegexOptions.IgnoreCase |
							       RegexOptions.CultureInvariant);
		
		static readonly Regex endOfTag = new Regex (@"</[\w:\.]+\s*?>",
							    RegexOptions.Compiled | RegexOptions.Singleline |
							    RegexOptions.Multiline | RegexOptions.IgnoreCase |
							    RegexOptions.CultureInvariant);
		
		static readonly Regex expressionRegex = new Regex (@"<%.*?%>",
								   RegexOptions.Compiled | RegexOptions.Singleline |
								   RegexOptions.Multiline | RegexOptions.IgnoreCase |
								   RegexOptions.CultureInvariant);

		static readonly Regex clientCommentRegex = new Regex (@"<!--(.|\s)*?-->",
								      RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase |
								      RegexOptions.CultureInvariant);
		
		ParserStack pstack;
		BuilderLocationStack stack;
		TemplateParser tparser;
		StringBuilder text;
		RootBuilder rootBuilder;
		bool inScript, javascript, ignore_text;
		ILocation location;
		bool isApplication;
		StringBuilder tagInnerText = new StringBuilder ();
		static IDictionary emptyHash = new Dictionary <string, object> ();
		bool inForm;
		bool useOtherTags;
		TagType lastTag;
		AspComponentFoundry componentFoundry;
		Stream inputStream;

		public AspGenerator (TemplateParser tparser, AspComponentFoundry componentFoundry) : this (tparser)
		{
			this.componentFoundry = componentFoundry;
		}
		
		public AspGenerator (TemplateParser tparser)
		{
			this.tparser = tparser;
			text = new StringBuilder ();
			stack = new BuilderLocationStack ();
			pstack = new ParserStack ();
		}

		public RootBuilder RootBuilder {
			get { return rootBuilder; }
		}

		public AspParser Parser {
			get { return pstack.Parser; }
		}
		
		public string Filename {
			get { return pstack.Filename; }
		}

		PageParserFilter PageParserFilter {
			get {
				if (tparser == null)
					return null;

				return tparser.PageParserFilter;
			}
		}

		// KLUDGE WARNING
		//
		// The kludge to determine the base type of the to-be-generated ASP.NET class is
		// very unfortunate but with our current parser it is, unfortunately, necessary. The
		// reason for reading the entire file into memory and parsing it with a regexp is
		// that we need to read the main directive (i.e. <%@Page %>, <%@Control %> etc),
		// pass it to the page parser filter if it exists, and finally read the inherits
		// attribute of the directive to get access to the base type of the class to be
		// generated. On that type we check whether it is decorated with the
		// FileLevelControlBuilder attribute and, if yes, use the indicated type as the
		// RootBuilder. This is necessary for the ASP.NET MVC views using the "generic"
		// inherits declaration to work properly. Our current parser is not able to parse
		// the input file out of sequence (i.e. directives first, then the rest) so we need
		// to do what we do below, alas.
		IDictionary GetDirectiveAttributesDictionary (string skipKeyName, CaptureCollection names, CaptureCollection values)
		{
			var ret = new Dictionary <string, object> (StringComparer.OrdinalIgnoreCase);

			int index = 0;
			string keyName;
			foreach (Capture c in names) {
				keyName = c.Value;
				if (String.Compare (skipKeyName, keyName, StringComparison.OrdinalIgnoreCase) == 0) {
					index++;
					continue;
				}
				
				ret.Add (c.Value, values [index++].Value);
			}

			return ret;
		}

		string GetDirectiveName (CaptureCollection names)
		{
			string val;
			foreach (Capture c in names) {
				val = c.Value;
				if (Directive.IsDirective (val))
					return val;
			}

			return tparser.DefaultDirectiveName;
		}

		int GetLineNumberForIndex (string fileContents, int index)
		{
			int line = 1;
			char c;
			bool foundCR = false;
			
			for (int pos = 0; pos < index; pos++) {
				c = fileContents [pos];
				if (c == '\n' || foundCR) {
					line++;
					foundCR = false;
				}
				
				foundCR = (c == '\r');
			}

			return line;
		}

		int GetNumberOfLinesForRange (string fileContents, int index, int length)
		{
			int lines = 0;
			int stop = index + length;
			char c;
			bool foundCR = false;
			
			for (int pos = index; pos < stop; pos++) {
				c = fileContents [pos];
				if (c == '\n' || foundCR) {
					lines++;
					foundCR = false;
				}

				foundCR = (c == '\r');
			}

			return lines;
		}
		
		Type GetInheritedType (string fileContents, string filename)
		{
			MatchCollection matches = DirectiveRegex.Matches (fileContents);
			if (matches == null || matches.Count == 0)
				return null;

			string wantedDirectiveName = tparser.DefaultDirectiveName.ToLower (Helpers.InvariantCulture);
			string directiveName;
			GroupCollection groups;
			CaptureCollection ccNames;
			
			foreach (Match match in matches) {
				groups = match.Groups;
				if (groups.Count < 6)
					continue;

				ccNames = groups [3].Captures;
				directiveName = GetDirectiveName (ccNames);
				if (String.IsNullOrEmpty (directiveName))
					continue;
				
				if (String.Compare (directiveName.ToLower (Helpers.InvariantCulture), wantedDirectiveName, StringComparison.Ordinal) != 0)
					continue;

				var loc = new Location (null);
				int index = match.Index;
				
				loc.Filename = filename;
				loc.BeginLine = GetLineNumberForIndex (fileContents, index);
				loc.EndLine = loc.BeginLine + GetNumberOfLinesForRange (fileContents, index, match.Length);
				
				tparser.Location = loc;
				tparser.allowedMainDirectives = 2;
				tparser.AddDirective (wantedDirectiveName, GetDirectiveAttributesDictionary (wantedDirectiveName, ccNames, groups [5].Captures));

				return tparser.BaseType;
			}
			
			return null;
		}

		string ReadFileContents (Stream inputStream, string filename)
		{
			string ret = null;
			
			if (inputStream != null) {
				if (inputStream.CanSeek) {
					long curPos = inputStream.Position;
					inputStream.Seek (0, SeekOrigin.Begin);

					Encoding enc = WebEncoding.FileEncoding;
					StringBuilder sb = new StringBuilder ();
					byte[] buffer = new byte [READ_BUFFER_SIZE];
					int nbytes;
					
					while ((nbytes = inputStream.Read (buffer, 0, READ_BUFFER_SIZE)) > 0)
						sb.Append (enc.GetString (buffer, 0, nbytes));
					inputStream.Seek (curPos, SeekOrigin.Begin);
					
					ret = sb.ToString ();
					sb.Length = 0;
					sb.Capacity = 0;
				} else {
					FileStream fs = inputStream as FileStream;
					if (fs != null) {
						string fname = fs.Name;
						try {
							if (File.Exists (fname))
								ret = File.ReadAllText (fname);
						} catch {
							// ignore
						}
					}
				}
			}

			if (ret == null && !String.IsNullOrEmpty (filename) && String.Compare (filename, "@@inner_string@@", StringComparison.Ordinal) != 0) {
				try {
					if (File.Exists (filename))
						ret = File.ReadAllText (filename);
				} catch {
					// ignore
				}
			}

			return ret;
		}
		
		Type GetRootBuilderType (Stream inputStream, string filename)
		{
			Type ret = null;
			string fileContents;

			if (tparser != null)
				fileContents = ReadFileContents (inputStream, filename);
			else
				fileContents = null;
			
			if (!String.IsNullOrEmpty (fileContents)) {
				Type inheritedType = GetInheritedType (fileContents, filename);
				fileContents = null;
				if (inheritedType != null) {
					FileLevelControlBuilderAttribute attr;
					
					try {
						object[] attrs = inheritedType.GetCustomAttributes (typeof (FileLevelControlBuilderAttribute), true);
						if (attrs != null && attrs.Length > 0)
							attr = attrs [0] as FileLevelControlBuilderAttribute;
						else
							attr = null;
					} catch {
						attr = null;
					}

					ret = attr != null ? attr.BuilderType : null;
				}
			}
			
			if (ret == null) {
				if (tparser is PageParser)
					return typeof (FileLevelPageControlBuilder);
				else if (tparser is UserControlParser)
					return typeof (FileLevelUserControlBuilder);
				else
					return typeof (RootBuilder);
			} else
				return ret;
		}
		
		void CreateRootBuilder (Stream inputStream, string filename)
		{
			if (rootBuilder != null)
				return;
			
			Type rootBuilderType = GetRootBuilderType (inputStream, filename);
			rootBuilder = Activator.CreateInstance (rootBuilderType) as RootBuilder;
			if (rootBuilder == null)
				throw new HttpException ("Cannot create an instance of file-level control builder.");
			rootBuilder.Init (tparser, null, null, null, null, null);
			if (componentFoundry != null)
				rootBuilder.Foundry = componentFoundry;
			
			stack.Push (rootBuilder, null);
			tparser.RootBuilder = rootBuilder;
		}
		
		BaseCompiler GetCompilerFromType ()
		{
			Type type = tparser.GetType ();
			if (type == typeof (PageParser))
				return new PageCompiler ((PageParser) tparser);

			if (type == typeof (ApplicationFileParser))
				return new GlobalAsaxCompiler ((ApplicationFileParser) tparser);

			if (type == typeof (UserControlParser))
				return new UserControlCompiler ((UserControlParser) tparser);

			if (type == typeof(MasterPageParser))
				return new MasterPageCompiler ((MasterPageParser) tparser);

			throw new Exception ("Got type: " + type);
		}

		void InitParser (TextReader reader, string filename)
		{
			AspParser parser = new AspParser (filename, reader);
			parser.Error += new ParseErrorHandler (ParseError);
			parser.TagParsed += new TagParsedHandler (TagParsed);
			parser.TextParsed += new TextParsedHandler (TextParsed);
			parser.ParsingComplete += new ParsingCompleteHandler (ParsingCompleted);
			tparser.AspGenerator = this;
			CreateRootBuilder (inputStream, filename);

			if (!pstack.Push (parser))
				throw new ParseException (Location, "Infinite recursion detected including file: " + filename);

			if (filename != "@@inner_string@@") {
				string arvp = Path.Combine (tparser.BaseVirtualDir, Path.GetFileName (filename));
				if (VirtualPathUtility.IsAbsolute (arvp))
					arvp = VirtualPathUtility.ToAppRelative (arvp);
				
				tparser.AddDependency (arvp);
			}
		}
		
		void InitParser (string filename)
		{
			StreamReader reader = new StreamReader (filename, WebEncoding.FileEncoding);
			InitParser (reader, filename);
		}

		void CheckForDuplicateIds (ControlBuilder root, Stack scopes)
		{
			if (root == null)
				return;
			
			if (scopes == null)
				scopes = new Stack ();			

			Dictionary <string, bool> ids;
			
			if (scopes.Count == 0 || root.IsNamingContainer) {
				ids = new Dictionary <string, bool> (StringComparer.Ordinal);
				scopes.Push (ids);
			} else {
				ids = scopes.Peek () as Dictionary <string, bool>;
			}
			
			if (ids == null)
				return;

			ControlBuilder cb;
			string id;
			ArrayList children = root.Children;
			if (children != null) {
				foreach (object o in children) {
					cb = o as ControlBuilder;
					if (cb == null)
						continue;

					id = cb.ID;
					if (id == null || id.Length == 0)
						continue;
				
					if (ids.ContainsKey (id))
						throw new ParseException (cb.Location, "Id '" + id + "' is already used by another control.");

					ids.Add (id, true);
					CheckForDuplicateIds (cb, scopes);
				}
			}
		}
		
		public void Parse (string file)
		{
			Parse (file, false);
		}
		
		public void Parse (TextReader reader, string filename, bool doInitParser)
		{
			try {
				isApplication = tparser.DefaultDirectiveName == "application";

				if (doInitParser)
					InitParser (reader, filename);

				pstack.Parser.Parse ();
				if (text.Length > 0)
					FlushText ();

				tparser.MD5Checksum = pstack.Parser.MD5Checksum;
				pstack.Pop ();

#if DEBUG
				PrintTree (RootBuilder, 0);
#endif

				if (stack.Count > 1 && pstack.Count == 0)
					throw new ParseException (stack.Builder.Location,
								  "Expecting </" + stack.Builder.TagName + "> " + stack.Builder);

				CheckForDuplicateIds (RootBuilder, null);
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		public void Parse (Stream stream, string filename, bool doInitParser)
		{
			inputStream = stream;
			Parse (new StreamReader (stream, WebEncoding.FileEncoding), filename, doInitParser);
		}
		
		public void Parse (string filename, bool doInitParser)
		{
			StreamReader reader = new StreamReader (filename, WebEncoding.FileEncoding);
			Parse (reader, filename, doInitParser);
		}

		public void Parse ()
		{
			string inputFile = tparser.InputFile;
			TextReader inputReader = tparser.Reader;

			try {			
				if (String.IsNullOrEmpty (inputFile)) {
					StreamReader sr = inputReader as StreamReader;
					if (sr != null) {
						FileStream fr = sr.BaseStream as FileStream;
						if (fr != null)
							inputFile = fr.Name;
					}

					if (String.IsNullOrEmpty (inputFile))
						inputFile = "@@inner_string@@";
				}

				if (inputReader != null) {
					Parse (inputReader, inputFile, true);
				} else {
					if (String.IsNullOrEmpty (inputFile))
						throw new HttpException ("Parser input file is empty, cannot continue.");
					inputFile = Path.GetFullPath (inputFile);
					InitParser (inputFile);
					Parse (inputFile);
				}
			} finally {
				if (inputReader != null)
					inputReader.Close ();
			}
		}

		internal static void AddTypeToCache (ArrayList dependencies, string inputFile, Type type)
		{
			if (type == null || inputFile == null || inputFile.Length == 0)
				return;

			if (dependencies != null && dependencies.Count > 0) {
				string [] deps = (string []) dependencies.ToArray (typeof (string));
				HttpContext ctx = HttpContext.Current;
				HttpRequest req = ctx != null ? ctx.Request : null;
				
				if (req == null)
					throw new HttpException ("No current context, cannot compile.");

				for (int i = 0; i < deps.Length; i++)
					deps [i] = req.MapPath (deps [i]);

				HttpRuntime.InternalCache.Insert ("@@Type" + inputFile, type, new CacheDependency (deps));
			} else
				HttpRuntime.InternalCache.Insert ("@@Type" + inputFile, type);
		}
		
		public Type GetCompiledType ()
		{
			Type type = (Type) HttpRuntime.InternalCache.Get ("@@Type" + tparser.InputFile);
			if (type != null) {
				return type;
			}

			Parse ();
			BaseCompiler compiler = GetCompilerFromType ();
			
			type = compiler.GetCompiledType ();
			AddTypeToCache (tparser.Dependencies, tparser.InputFile, type);
			return type;
		}

#if DEBUG
		static void PrintTree (ControlBuilder builder, int indent)
		{
			if (builder == null)
				return;

			string i = new string ('\t', indent);
			Console.Write (i);
			Console.WriteLine ("b: {0}; naming container: {1}; id: {2}; type: {3}; parent: {4}",
					   builder, builder.IsNamingContainer, builder.ID, builder.ControlType, builder.ParentBuilder);

			if (builder.Children != null)
			foreach (object o in builder.Children) {
				if (o is ControlBuilder)
					PrintTree ((ControlBuilder) o, indent++);
			}
		}
		
		static void PrintLocation (ILocation loc)
		{
			Console.WriteLine ("\tFile name: " + loc.Filename);
			Console.WriteLine ("\tBegin line: " + loc.BeginLine);
			Console.WriteLine ("\tEnd line: " + loc.EndLine);
			Console.WriteLine ("\tBegin column: " + loc.BeginColumn);
			Console.WriteLine ("\tEnd column: " + loc.EndColumn);
			Console.WriteLine ("\tPlainText: " + loc.PlainText);
			Console.WriteLine ();
		}
#endif

		void ParseError (ILocation location, string message)
		{
			throw new ParseException (location, message);
		}

		// KLUDGE WARNING!!
		//
		// The code below (ProcessTagsInAttributes, ParseAttributeTag) serves the purpose to work
		// around a limitation of the current asp.net parser which is unable to parse server
		// controls inside client tag attributes. Since the architecture of the current
		// parser does not allow for clean solution of this problem, hence the kludge
		// below. It will be gone as soon as the parser is rewritten.
		//
		// The kludge supports only self-closing tags inside attributes.
		//
		// KLUDGE WARNING!!
		bool ProcessTagsInAttributes (ILocation location, string tagid, TagAttributes attributes, TagType type)
		{
			if (attributes == null || attributes.Count == 0)
				return false;
			
			Match match;
			Group group;
			string value;
			bool retval = false;
			int index, length;
			StringBuilder sb = new StringBuilder ();

			sb.AppendFormat ("\t<{0}", tagid);
			foreach (string key in attributes.Keys) {
				value = attributes [key] as string;
				if (value == null || value.Length < 16) { // optimization
					sb.AppendFormat (" {0}=\"{1}\"", key, value);
					continue;
				}
				
				match = runatServer.Match (attributes [key] as string);
				if (!match.Success) {
					sb.AppendFormat (" {0}=\"{1}\"", key, value);
					continue;
				}
				if (sb.Length > 0) {
					TextParsed (location, sb.ToString ());
					sb.Length = 0;
				}
				
				retval = true;
				group = match.Groups [0];
				index = group.Index;
				length = group.Length;

				TextParsed (location, String.Format (" {0}=\"{1}", key, index > 0 ? value.Substring (0, index) : String.Empty));;
				FlushText ();
				ParseAttributeTag (group.Value, location);
				if (index + length < value.Length)
					TextParsed (location, value.Substring (index + length) + "\"");
				else
					TextParsed (location, "\"");
			}
			if (type == TagType.SelfClosing)
				sb.Append ("/>");
			else
				sb.Append (">");

			if (retval && sb.Length > 0)
				TextParsed (location, sb.ToString ());
			
			return retval;
		}

		void ParseAttributeTag (string code, ILocation location)
		{
			AspParser outerParser = location as AspParser;
			int positionOffset = outerParser != null ? outerParser.BeginPosition : 0;
			AspParser parser = new AspParser ("@@attribute_tag@@", new StringReader (code), location.BeginLine - 1, positionOffset, outerParser);
			parser.Error += new ParseErrorHandler (ParseError);
			parser.TagParsed += new TagParsedHandler (TagParsed);
			parser.TextParsed += new TextParsedHandler (TextParsed);
			parser.Parse ();
			if (text.Length > 0)
				FlushText ();
		}

		void ParsingCompleted ()
		{
			PageParserFilter pfilter = PageParserFilter;
			if (pfilter == null)
				return;

			pfilter.ParseComplete (RootBuilder);
		}

		void CheckIfIncludeFileIsSecure (string filePath)
		{
			if (filePath == null || filePath.Length == 0)
				return;
			
			// a bit slow, but fully portable
			string newdir = null;
			Exception exception = null;
			try {
				string origdir = Directory.GetCurrentDirectory ();
				Directory.SetCurrentDirectory (Path.GetDirectoryName (filePath));
				newdir = Directory.GetCurrentDirectory ();
				Directory.SetCurrentDirectory (origdir);
				if (newdir [newdir.Length - 1] != '/')
					newdir += "/";
			} catch (DirectoryNotFoundException) {
				return; // will be converted into 404
			} catch (FileNotFoundException) {
				return; // as above
			} catch (Exception ex) {
				// better safe than sorry
				exception = ex;
			}

			if (exception != null || !StrUtils.StartsWith (newdir, HttpRuntime.AppDomainAppPath))
				throw new ParseException (Location, "Files above the application's root directory cannot be included.");
		}

		string ChopOffTagStart (ILocation location, string content, string tagid)
		{
			string tagstart = '<' + tagid;
			if (content.StartsWith (tagstart)) {
				TextParsed (location, tagstart);
				content = content.Substring (tagstart.Length);
			}

			return content;
		}
		
		void TagParsed (ILocation location, TagType tagtype, string tagid, TagAttributes attributes)
		{
			bool tagIgnored;
			
			this.location = new Location (location);
			if (tparser != null)
				tparser.Location = location;

			if (text.Length != 0) {
				bool ignoreEmptyString = lastTag == TagType.CodeRender;
				FlushText (ignoreEmptyString);
			}
			
			if (0 == String.Compare (tagid, "script", true, Helpers.InvariantCulture)) {
				bool in_script = (inScript || ignore_text);
				if (in_script) {
					if (ProcessScript (tagtype, attributes))
						return;
				} else
					if (ProcessScript (tagtype, attributes))
						return;
			}

			lastTag = tagtype;
			switch (tagtype) {
			case TagType.Directive:
				if (tagid.Length == 0)
					tagid = tparser.DefaultDirectiveName;

				tparser.AddDirective (tagid, attributes.GetDictionary (null));
				break;
			case TagType.Tag:
				if (ProcessTag (location, tagid, attributes, tagtype, out tagIgnored)) {
					if (!tagIgnored)
						useOtherTags = true;
					break;
				}

				if (useOtherTags) {
					stack.Builder.EnsureOtherTags ();
					stack.Builder.OtherTags.Add (tagid);
				}

				{
					string plainText = location.PlainText;
					if (!ProcessTagsInAttributes (location, tagid, attributes, TagType.Tag))
						TextParsed (location, ChopOffTagStart (location, plainText, tagid));
				}
				break;
			case TagType.Close:
				bool notServer = (useOtherTags && TryRemoveTag (tagid, stack.Builder.OtherTags));
				if (!notServer && CloseControl (tagid))
					break;
				
				TextParsed (location, location.PlainText);
				break;
			case TagType.SelfClosing:
				int count = stack.Count;
				if (!ProcessTag (location, tagid, attributes, tagtype, out tagIgnored) && !tagIgnored) {
					string plainText = location.PlainText;
					if (!ProcessTagsInAttributes (location, tagid, attributes, TagType.SelfClosing))
						TextParsed (location, ChopOffTagStart (location, plainText, tagid));
				} else if (stack.Count != count) {
					CloseControl (tagid);
				}
				break;
			case TagType.DataBinding:
			case TagType.CodeRenderExpression:
			case TagType.CodeRender:
#if NET_4_0
			case TagType.CodeRenderEncode:
#endif
				if (isApplication)
					throw new ParseException (location, "Invalid content for application file.");
			
				ProcessCode (tagtype, tagid, location);
				break;
			case TagType.Include:
				if (isApplication)
					throw new ParseException (location, "Invalid content for application file.");
			
				string file = attributes ["virtual"] as string;
				bool isvirtual = (file != null);
				if (!isvirtual)
					file = attributes ["file"] as string;

				if (isvirtual) {
					bool parsed = false;
					VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;

					if (vpp.FileExists (file)) {
						VirtualFile vf = vpp.GetFile (file);
						if (vf != null) {
							Parse (vf.Open (), file, true);
							parsed = true;
						}
					}
					
					if (!parsed)
						Parse (tparser.MapPath (file), true);
				} else {
					string includeFilePath = GetIncludeFilePath (tparser.ParserDir, file);
					CheckIfIncludeFileIsSecure (includeFilePath);
					tparser.PushIncludeDir (Path.GetDirectoryName (includeFilePath));
					try {
						Parse (includeFilePath, true);
					} finally {
						tparser.PopIncludeDir ();
					}
				}
				
				break;
			default:
				break;
			}
			//PrintLocation (location);
		}

		static bool TryRemoveTag (string tagid, ArrayList otags)
		{
			if (otags == null || otags.Count == 0)
				return false;

			for (int idx = otags.Count - 1; idx >= 0; idx--) {
				string otagid = (string) otags [idx];
				if (0 == String.Compare (tagid, otagid, true, Helpers.InvariantCulture)) {
					do {
						otags.RemoveAt (idx);
					} while (otags.Count - 1 >= idx);
					return true;
				}
			}
			return false;
		}

		static string GetIncludeFilePath (string basedir, string filename)
		{
			if (Path.DirectorySeparatorChar == '/')
				filename = filename.Replace ("\\", "/");

			return Path.GetFullPath (Path.Combine (basedir, filename));
		}

		delegate bool CheckBlockEnd (string text);
		
		bool CheckTagEndNeeded (string text)
		{
			return !text.EndsWith ("/>");
		}
		
		List <TextBlock> FindRegexBlocks (Regex rxStart, Regex rxEnd, CheckBlockEnd checkEnd, IList blocks, TextBlockType typeForMatches, bool discardBlocks)
		{
			var ret = new List <TextBlock> ();
			foreach (TextBlock block in blocks) {
				if (block.Type != TextBlockType.Verbatim) {
					ret.Add (block);
					continue;
				}

				int lastIndex = 0, index;
				MatchCollection matches = rxStart.Matches (block.Content);
				bool foundMatches = matches.Count > 0;
				foreach (Match match in matches) {
					foundMatches = true;
					index = match.Index;
					if (lastIndex < index)
						ret.Add (new TextBlock (TextBlockType.Verbatim, block.Content.Substring (lastIndex, index - lastIndex)));

					string value = match.Value;
					if (rxEnd != null && checkEnd (value)) {
						int startFrom = index + value.Length;
						Match m = rxEnd.Match (block.Content, startFrom);
						if (m.Success)
							value += block.Content.Substring (startFrom, m.Index - startFrom) + m.Value;
					}

					if (!discardBlocks)
						ret.Add (new TextBlock (typeForMatches, value));
					lastIndex = index + value.Length;
				}

				if (lastIndex > 0 && lastIndex < block.Content.Length)
					ret.Add (new TextBlock (TextBlockType.Verbatim, block.Content.Substring (lastIndex)));

				if (!foundMatches)
					ret.Add (block);
			}

			return ret;
		}
		
		IList SplitTextIntoBlocks (string text)
		{
			var ret = new List <TextBlock> ();

			ret.Add (new TextBlock (TextBlockType.Verbatim, text));
			ret = FindRegexBlocks (clientCommentRegex, null, null, ret, TextBlockType.Comment, false);
			ret = FindRegexBlocks (runatServer, endOfTag, CheckTagEndNeeded, ret, TextBlockType.Tag, false);
			ret = FindRegexBlocks (expressionRegex, null, null, ret, TextBlockType.Expression, false);

			return ret;
		}

		void TextParsed (ILocation location, string text)
		{
			if (ignore_text)
				return;

			if (inScript) {
				this.text.Append (text);
				FlushText (true);
				return;
			}

			IList blocks = SplitTextIntoBlocks (text);
			foreach (TextBlock block in blocks) {
				switch (block.Type) {
					case TextBlockType.Verbatim:
						this.text.Append (block.Content);
						break;

					case TextBlockType.Expression:
						if (this.text.Length > 0)
							FlushText (true);
						CodeRenderParser r = new CodeRenderParser (block.Content, stack.Builder, location);
						r.AddChildren (this);
						break;

					case TextBlockType.Tag:
						ParseAttributeTag (block.Content, location);
						break;

					case TextBlockType.Comment: {
						this.text.Append ("<!--");
						FlushText (true);
						string blockToParse = block.Content.Substring (4, block.Length - 7);
						bool condEndif;
						if (blockToParse.EndsWith ("<![endif]")) {
							blockToParse = blockToParse.Substring (0, blockToParse.Length - 9);
							condEndif = true;
						} else
							condEndif = false;

						AspParser outerParser = location as AspParser;
						int positionOffset = outerParser != null ? outerParser.BeginPosition : 0;
						AspParser parser = new AspParser ("@@comment_code@@", new StringReader (blockToParse), location.BeginLine - 1, positionOffset, outerParser);
						parser.Error += new ParseErrorHandler (ParseError);
						parser.TagParsed += new TagParsedHandler (TagParsed);
						parser.TextParsed += new TextParsedHandler (TextParsed);
						parser.Parse ();
						if (condEndif)
							this.text.Append ("<![endif]");
						this.text.Append ("-->");
						FlushText (true);
						break;
					}
				}
			}
		}

		void FlushText ()
		{
			FlushText (false);
		}
		
		void FlushText (bool ignoreEmptyString)
		{
			string t = text.ToString ();
			text.Length = 0;

			if (ignoreEmptyString && t.Trim ().Length == 0)
				return;
			
			if (inScript) {
				PageParserFilter pfilter = PageParserFilter;
				if (pfilter != null && !pfilter.ProcessCodeConstruct (CodeConstructType.ScriptTag, t))
					return;

				tparser.Scripts.Add (new ServerSideScript (t, new System.Web.Compilation.Location (tparser.Location)));
				return;
			}

			if (tparser.DefaultDirectiveName == "application" && t.Trim () != "")
				throw new ParseException (location, "Content not valid for application file.");

			ControlBuilder current = stack.Builder;
			current.AppendLiteralString (t);
			if (current.NeedsTagInnerText ()) {
				tagInnerText.Append (t);
			}
		}

		bool BuilderHasOtherThan (Type type, ControlBuilder cb)
		{
			ArrayList al = cb.OtherTags;
			if (al != null && al.Count > 0)
				return true;
			
			al = cb.Children;
			if (al != null) {
				ControlBuilder tmp;
				
				foreach (object o in al) {
					if (o == null)
						continue;
					
					tmp = o as ControlBuilder;
					if (tmp == null) {
						string s = o as string;
						if (s != null && String.IsNullOrEmpty (s.Trim ()))
							continue;
						
						return true;
					}
					
					if (tmp is System.Web.UI.WebControls.ContentBuilderInternal)
						continue;
					
					if (tmp.ControlType != typeof (System.Web.UI.WebControls.Content))
						return true;
				}
			}

			return false;
		}
		
		bool OtherControlsAllowed (ControlBuilder cb)
		{
			if (cb == null)
				return true;
			
			if (!typeof (System.Web.UI.WebControls.Content).IsAssignableFrom (cb.ControlType))
				return true;

			if (BuilderHasOtherThan (typeof (System.Web.UI.WebControls.Content), RootBuilder))
				return false;
			
			return true;
		}

		public void AddControl (Type type, IDictionary attributes)
		{
			ControlBuilder parent = stack.Builder;
			ControlBuilder builder = ControlBuilder.CreateBuilderFromType (tparser, parent, type, null, null,
										       attributes, location.BeginLine,
										       location.Filename);
			if (builder != null)
				parent.AppendSubBuilder (builder);
		}
		
		bool ProcessTag (ILocation location, string tagid, TagAttributes atts, TagType tagtype, out bool ignored)
		{
			ignored = false;
			if (isApplication) {
				if (String.Compare (tagid, "object", true, Helpers.InvariantCulture) != 0)
					throw new ParseException (location, "Invalid tag for application file.");
			}

			ControlBuilder parent = stack.Builder;
			ControlBuilder builder = null;
			if (parent != null && parent.ControlType == typeof (HtmlTable) &&
			    (String.Compare (tagid, "thead", true, Helpers.InvariantCulture) == 0 ||
			     String.Compare (tagid, "tbody", true, Helpers.InvariantCulture) == 0)) {
				ignored = true;
				return true;
			}
				
			IDictionary htable = (atts != null) ? atts.GetDictionary (null) : emptyHash;
			if (stack.Count > 1) {
				try {
					builder = parent.CreateSubBuilder (tagid, htable, null, tparser, location);
				} catch (TypeLoadException e) {
					throw new ParseException (Location, "Type not found.", e);
				} catch (Exception e) {
					throw new ParseException (Location, e.Message, e);
				}
			}

			bool runatServer = atts != null && atts.IsRunAtServer ();
			if (builder == null && runatServer) {
				string id = htable ["id"] as string;
				if (id != null && !CodeGenerator.IsValidLanguageIndependentIdentifier (id))
					throw new ParseException (Location, "'" + id + "' is not a valid identifier");
					
				try {
					builder = RootBuilder.CreateSubBuilder (tagid, htable, null, tparser, location);
				} catch (TypeLoadException e) {
					throw new ParseException (Location, "Type not found.", e);
				} catch (HttpException e) {
					CompilationException inner = e.InnerException as CompilationException;
					if (inner != null)
						throw inner;
					
					throw new ParseException (Location, e.Message, e);
				} catch (Exception e) {
					throw new ParseException (Location, e.Message, e);
				}
			}
			
			if (builder == null)
				return false;

			// This is as good as we can do for now - if the parsed location contains
			// both expressions and code render blocks then we're out of luck...
			string plainText = location.PlainText;
			if (!runatServer && plainText.IndexOf ("<%$") == -1&& plainText.IndexOf ("<%") > -1)
				return false;

			PageParserFilter pfilter = PageParserFilter;
			if (pfilter != null && !pfilter.AllowControl (builder.ControlType, builder))
				throw new ParseException (Location, "Control type '" + builder.ControlType + "' not allowed.");
			
			if (!OtherControlsAllowed (builder))
				throw new ParseException (Location, "Only Content controls are allowed directly in a content page that contains Content controls.");
			
			builder.Location = location;
			builder.ID = htable ["id"] as string;
			if (typeof (HtmlForm).IsAssignableFrom (builder.ControlType)) {
				if (inForm)
					throw new ParseException (location, "Only one <form> allowed.");

				inForm = true;
			}

			if (builder.HasBody () && !(builder is ObjectTagBuilder)) {
				if (builder is TemplateBuilder) {
				//	push the id list
				}
				stack.Push (builder, location);
			} else {
				if (!isApplication && builder is ObjectTagBuilder) {
					ObjectTagBuilder ot = (ObjectTagBuilder) builder;
					if (ot.Scope != null && ot.Scope.Length > 0)
						throw new ParseException (location, "Scope not allowed here");

					if (tagtype == TagType.Tag) {
						stack.Push (builder, location);
						return true;
					}
				}
				
				parent.AppendSubBuilder (builder);
				builder.CloseControl ();
			}

			return true;
		}

		string ReadFile (string filename)
		{
			string realpath = tparser.MapPath (filename);
			using (StreamReader sr = new StreamReader (realpath, WebEncoding.FileEncoding)) {
				string content = sr.ReadToEnd ();
				return content;
			}
		}

		bool ProcessScript (TagType tagtype, TagAttributes attributes)
		{
			if (tagtype != TagType.Close) {
				if (attributes != null && attributes.IsRunAtServer ()) {
					string language = (string) attributes ["language"];
					if (language != null && language.Length > 0 && tparser.ImplicitLanguage)
						tparser.SetLanguage (language);
					CheckLanguage (language);
					string src = (string) attributes ["src"];
					if (src != null) {
						if (src.Length == 0)
							throw new ParseException (Parser,
								"src cannot be an empty string");

						string content = ReadFile (src);
						inScript = true;
						TextParsed (Parser, content);
						FlushText ();
						inScript = false;
						if (tagtype != TagType.SelfClosing) {
							ignore_text = true;
							Parser.VerbatimID = "script";
						}
					} else if (tagtype == TagType.Tag) {
						Parser.VerbatimID = "script";
						inScript = true;
					}

					return true;
				} else {
					if (tagtype != TagType.SelfClosing) {
						Parser.VerbatimID = "script";
						javascript = true;
					}
					string content = location.PlainText;
					/* HACK, HACK, HACK */
					if (content.StartsWith ("<script")) {
						TextParsed (location, "<script");
						content = content.Substring (7);
					}

					TextParsed (location, content);
					return true;
				}
			}

			bool result;
			if (inScript) {
				result = inScript;
				inScript = false;
			} else if (!ignore_text) {
				result = javascript;
				javascript = false;
				TextParsed (location, location.PlainText);
			} else {
				ignore_text = false;
				result = true;
			}

			return result;
		}

		bool CloseControl (string tagid)
		{
			ControlBuilder current = stack.Builder;
			string btag = current.OriginalTagName;
			if (String.Compare (btag, "tbody", true, Helpers.InvariantCulture) != 0 &&
			    String.Compare (tagid, "tbody", true, Helpers.InvariantCulture) == 0) {
				if (!current.ChildrenAsProperties) {
					try {
						TextParsed (location, location.PlainText);
						FlushText ();
					} catch {}
				}
				return true;
			}

			if (current.ControlType == typeof (HtmlTable) && String.Compare (tagid, "thead", true, Helpers.InvariantCulture) == 0)
				return true;
			
			if (0 != String.Compare (tagid, btag, true, Helpers.InvariantCulture))
				return false;

			// if (current is TemplateBuilder)
			//	pop from the id list
			if (current.NeedsTagInnerText ()) {
				try { 
					current.SetTagInnerText (tagInnerText.ToString ());
				} catch (Exception e) {
					throw new ParseException (current.Location, e.Message, e);
				}

				tagInnerText.Length = 0;
			}

			if (typeof (HtmlForm).IsAssignableFrom (current.ControlType)) {
				inForm = false;
			}

			current.CloseControl ();
			stack.Pop ();
			stack.Builder.AppendSubBuilder (current);
			return true;
		}

		CodeConstructType MapTagTypeToConstructType (TagType tagtype)
		{
			switch (tagtype) {
				case TagType.CodeRenderExpression:
					return CodeConstructType.ExpressionSnippet;

				case TagType.CodeRender:
#if NET_4_0
				case TagType.CodeRenderEncode:
#endif
					return CodeConstructType.CodeSnippet;

				case TagType.DataBinding:
					return CodeConstructType.DataBindingSnippet;

				default:
					throw new InvalidOperationException ("Unexpected tag type.");
			}
		}
		
		bool ProcessCode (TagType tagtype, string code, ILocation location)
		{
			PageParserFilter pfilter = PageParserFilter;
			// LAMESPEC:
			//
			// http://msdn.microsoft.com/en-us/library/system.web.ui.pageparserfilter.processcodeconstruct.aspx
			//
			// The above page says if false is returned then we should NOT process the
			// code further, wheras in reality it's the other way around. The
			// ProcessCodeConstruct return value means whether or not the filter
			// _processed_ the code.
			//
			if (pfilter != null && (!pfilter.AllowCode || pfilter.ProcessCodeConstruct (MapTagTypeToConstructType (tagtype), code)))
				return true;

			ControlBuilder b = null;
			if (tagtype == TagType.CodeRender)
				b = new CodeRenderBuilder (code, false, location);
			else if (tagtype == TagType.CodeRenderExpression)
				b = new CodeRenderBuilder (code, true, location);
			else if (tagtype == TagType.DataBinding)
				b = new DataBindingBuilder (code, location);
#if NET_4_0
			else if (tagtype == TagType.CodeRenderEncode)
				b = new CodeRenderBuilder (code, true, location, true);
#endif
			else
				throw new HttpException ("Should never happen");

			stack.Builder.AppendSubBuilder (b);
			return true;
		}

		public ILocation Location {
			get { return location; }
		}

		void CheckLanguage (string lang)
		{
			if (lang == null || lang == "")
				return;

			if (String.Compare (lang, tparser.Language, true, Helpers.InvariantCulture) == 0)
				return;

			CompilationSection section = (CompilationSection) WebConfigurationManager.GetWebApplicationSection ("system.web/compilation");
			if (section.Compilers[tparser.Language] != section.Compilers[lang])
				throw new ParseException (Location,
						String.Format ("Trying to mix language '{0}' and '{1}'.", 
								tparser.Language, lang));
		}

		// Used to get CodeRender tags in attribute values
		class CodeRenderParser
		{
			string str;
			ControlBuilder builder;
			AspGenerator generator;
			ILocation location;
			
			public CodeRenderParser (string str, ControlBuilder builder, ILocation location)
			{
				this.str = str;
				this.builder = builder;
				this.location = location;
			}

			public void AddChildren (AspGenerator generator)
			{
				this.generator = generator;
				int index = str.IndexOf ("<%");
				if (index > 0)
					DoParseExpressions (str);
				else
					DoParse (str);
			}

			void DoParseExpressions (string str)
			{
				int startIndex = 0, index = 0;
				Regex codeDirective = new Regex ("(<%(?!@)(?<code>(.|\\s)*?)%>)|(<[\\w:\\.]+.*?runat=[\"']?server[\"']?.*?/>)",
								 RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);
				Match match;
				int strLen = str.Length;
				
				while (index > -1 && startIndex < strLen) {
					match = codeDirective.Match (str, index);
					
					if (match.Success) {
						string value = match.Value;
						index = match.Index;
						if (index > startIndex)
							TextParsed (null, str.Substring (startIndex, index - startIndex));
						DoParse (value);
						index += value.Length;
						startIndex = index;
					} else
						break;

					if (index < strLen)
						index = str.IndexOf ('<', index);
					else
						break;
				}
				
				if (startIndex < strLen)
					TextParsed (null, str.Substring (startIndex));
			}
			
			void DoParse (string str)
			{
				AspParser outerParser = location as AspParser;
				int positionOffset = outerParser != null ? outerParser.BeginPosition : 0;
				AspParser parser = new AspParser ("@@code_render@@", new StringReader (str), location.BeginLine - 1, positionOffset, outerParser);
				parser.Error += new ParseErrorHandler (ParseError);
				parser.TagParsed += new TagParsedHandler (TagParsed);
				parser.TextParsed += new TextParsedHandler (TextParsed);
				parser.Parse ();
			}

			void TagParsed (ILocation location, TagType tagtype, string tagid, TagAttributes attributes)
			{
				switch (tagtype) {
					case TagType.CodeRender:
						builder.AppendSubBuilder (new CodeRenderBuilder (tagid, false, location));
						break;
						
					case TagType.CodeRenderExpression:
						builder.AppendSubBuilder (new CodeRenderBuilder (tagid, true, location));
						break;
#if NET_4_0
					case TagType.CodeRenderEncode:
						builder.AppendSubBuilder (new CodeRenderBuilder (tagid, true, location, true));
						break;
#endif
					case TagType.DataBinding:
						builder.AppendSubBuilder (new DataBindingBuilder (tagid, location));
						break;

					case TagType.Tag:
					case TagType.SelfClosing:
					case TagType.Close:
						if (generator != null)
							generator.TagParsed (location, tagtype, tagid, attributes);
						else
							goto default;
						break;
						
					default:
						string text = location.PlainText;
						if (text != null && text.Trim ().Length > 0)
							builder.AppendLiteralString (text);
						break;
				}
			}

			void TextParsed (ILocation location, string text)
			{
				builder.AppendLiteralString (text);
			}

			void ParseError (ILocation location, string message)
			{
				throw new ParseException (location, message);
			}
		}
	}
}

