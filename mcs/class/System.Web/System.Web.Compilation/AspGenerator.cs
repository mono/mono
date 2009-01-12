//
// System.Web.Compilation.AspGenerator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2004,2006 Novell, Inc (http://www.novell.com)
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
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text;
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
			this.Location = location;
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

			return 0 == String.Compare (tagid, (string) tags.Peek (), true, CultureInfo.InvariantCulture);
		}
		
		public int Count {
			get { return tags.Count; }
		}

		public string Current {
			get { return (string) tags.Peek (); }
		}
	}

	class AspGenerator
	{
		ParserStack pstack;
		BuilderLocationStack stack;
		TemplateParser tparser;
		StringBuilder text;
		RootBuilder rootBuilder;
		bool inScript, javascript, ignore_text;
		ILocation location;
		bool isApplication;
		StringBuilder tagInnerText = new StringBuilder ();
		static Hashtable emptyHash = new Hashtable ();
		bool inForm;
		bool useOtherTags;
		TagType lastTag;
		
		public AspGenerator (TemplateParser tparser)
		{
			this.tparser = tparser;
			text = new StringBuilder ();
			stack = new BuilderLocationStack ();
			rootBuilder = new RootBuilder (tparser);
			stack.Push (rootBuilder, null);
			tparser.RootBuilder = rootBuilder;
			pstack = new ParserStack ();
		}

		public RootBuilder RootBuilder {
			get { return tparser.RootBuilder; }
		}

		public AspParser Parser {
			get { return pstack.Parser; }
		}
		
		public string Filename {
			get { return pstack.Filename; }
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
#if NET_2_0
			if (type == typeof(MasterPageParser))
				return new MasterPageCompiler ((MasterPageParser) tparser);
#endif

			throw new Exception ("Got type: " + type);
		}

		void InitParser (TextReader reader, string filename)
		{
			AspParser parser = new AspParser (filename, reader);
			parser.Error += new ParseErrorHandler (ParseError);
			parser.TagParsed += new TagParsedHandler (TagParsed);
			parser.TextParsed += new TextParsedHandler (TextParsed);
			if (!pstack.Push (parser))
				throw new ParseException (Location, "Infinite recursion detected including file: " + filename);

			if (filename != "@@inner_string@@") {
				string arvp = Path.Combine (tparser.BaseVirtualDir, Path.GetFileName (filename));
				if (VirtualPathUtility.IsAbsolute (arvp))
					arvp = VirtualPathUtility.ToAppRelative (arvp);
				
				tparser.AddDependency (arvp);
			}
#if NET_2_0
			tparser.MD5Checksum = parser.MD5Checksum;
#endif
		}
		
#if NET_2_0
		void InitParser (string filename)
		{
			StreamReader reader = new StreamReader (filename, WebEncoding.FileEncoding);
			InitParser (reader, filename);
		}
#endif
		
		public void Parse (string file)
		{
#if ONLY_1_1
			Parse (file, true);
#else
			Parse (file, false);
#endif
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

				pstack.Pop ();

#if DEBUG
				PrintTree (rootBuilder, 0);
#endif

				if (stack.Count > 1 && pstack.Count == 0)
					throw new ParseException (stack.Builder.location,
								  "Expecting </" + stack.Builder.TagName + "> " + stack.Builder);
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		public void Parse (Stream stream, string filename, bool doInitParser)
		{
			Parse (new StreamReader (stream, WebEncoding.FileEncoding), filename, doInitParser);
		}
		
		public void Parse (string filename, bool doInitParser)
		{
			StreamReader reader = new StreamReader (filename, WebEncoding.FileEncoding);
			Parse (reader, filename, doInitParser);
		}

		public void Parse ()
		{
#if NET_2_0
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
#else
			Parse (Path.GetFullPath (tparser.InputFile));
#endif
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
			Console.WriteLine ("b: {0} id: {1} type: {2} parent: {3}",
					   builder, builder.ID, builder.ControlType, builder.parentBuilder);

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

		void TagParsed (ILocation location, TagType tagtype, string tagid, TagAttributes attributes)
		{
			this.location = new Location (location);
			if (tparser != null)
				tparser.Location = location;

			if (text.Length != 0)
				FlushText (lastTag == TagType.CodeRender);

			if (0 == String.Compare (tagid, "script", true, CultureInfo.InvariantCulture)) {
				bool in_script = (inScript || ignore_text);
				if (in_script || (tagtype != TagType.Close && attributes != null)) {
					if ((in_script || attributes.IsRunAtServer ()) && ProcessScript (tagtype, attributes))
						return;
				}
			}

			lastTag = tagtype;
			switch (tagtype) {
			case TagType.Directive:
				if (tagid == "")
					tagid = tparser.DefaultDirectiveName;

				tparser.AddDirective (tagid, attributes.GetDictionary (null));
				break;
			case TagType.Tag:
				if (ProcessTag (tagid, attributes, tagtype)) {
					useOtherTags = true;
					break;
				}

				if (useOtherTags) {
					stack.Builder.EnsureOtherTags ();
					stack.Builder.OtherTags.Add (tagid);
				}

				TextParsed (location, location.PlainText);
				break;
			case TagType.Close:
				bool notServer = (useOtherTags && TryRemoveTag (tagid, stack.Builder.OtherTags));
				if (!notServer && CloseControl (tagid))
					break;
				
				TextParsed (location, location.PlainText);
				break;
			case TagType.SelfClosing:
				int count = stack.Count;
				if (!ProcessTag (tagid, attributes, tagtype)) {
					TextParsed (location, location.PlainText);
				} else if (stack.Count != count) {
					CloseControl (tagid);
				}
				break;
			case TagType.DataBinding:
				goto case TagType.CodeRender;
			case TagType.CodeRenderExpression:
				goto case TagType.CodeRender;
			case TagType.CodeRender:
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
#if NET_2_0
					VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;

					if (vpp.FileExists (file)) {
						VirtualFile vf = vpp.GetFile (file);
						if (vf != null) {
							Parse (vf.Open (), file, true);
							parsed = true;
						}
					}
#endif
					
					if (!parsed)
						Parse (tparser.MapPath (file), true);
				} else {
					string includeFilePath = GetIncludeFilePath (tparser.ParserDir, file);
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
				if (0 == String.Compare (tagid, otagid, true, CultureInfo.InvariantCulture)) {
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
		
		void TextParsed (ILocation location, string text)
		{
			if (ignore_text)
				return;

			if (text.IndexOf ("<%") != -1 && !inScript) {
				if (this.text.Length > 0)
					FlushText (true);
				CodeRenderParser r = new CodeRenderParser (text, stack.Builder);
				r.AddChildren ();
				return;
			}
			
			this.text.Append (text);
			//PrintLocation (location);
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

#if NET_2_0
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

			if (BuilderHasOtherThan (typeof (System.Web.UI.WebControls.Content), rootBuilder))
				return false;
			
			return true;
		}
#endif
		
		bool ProcessTag (string tagid, TagAttributes atts, TagType tagtype)
		{
			if (isApplication) {
				if (String.Compare (tagid, "object", true, CultureInfo.InvariantCulture) != 0)
					throw new ParseException (location, "Invalid tag for application file.");
			}

			ControlBuilder parent = stack.Builder;
			ControlBuilder builder = null;
			Hashtable htable = (atts != null) ? atts.GetDictionary (null) : emptyHash;
			if (stack.Count > 1) {
				try {
					builder = parent.CreateSubBuilder (tagid, htable, null, tparser, location);
				} catch (TypeLoadException e) {
					throw new ParseException (Location, "Type not found.", e);
				} catch (Exception e) {
					throw new ParseException (Location, e.Message, e);
				}
			}

			if (builder == null && atts != null && atts.IsRunAtServer ()) {
				string id = htable ["id"] as string;
				if (id != null && !CodeGenerator.IsValidLanguageIndependentIdentifier (id))
					throw new ParseException (Location, "'" + id + "' is not a valid identifier");
					
				try {
					builder = rootBuilder.CreateSubBuilder (tagid, htable, null, tparser, location);
				} catch (TypeLoadException e) {
					throw new ParseException (Location, "Type not found.", e);
				} catch (Exception e) {
					throw new ParseException (Location, e.Message, e);
				}
			}
			
			if (builder == null)
				return false;

#if NET_2_0
			if (!OtherControlsAllowed (builder))
				throw new ParseException (Location, "Only Content controls are allowed directly in a content page that contains Content controls.");
#endif
			
			builder.location = location;
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
					if (ot.Scope != null && ot.Scope != "")
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
						if (src == "")
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
					TextParsed (location, location.PlainText);
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
			if (String.Compare (btag, "tbody", true, CultureInfo.InvariantCulture) != 0 &&
			    String.Compare (tagid, "tbody", true, CultureInfo.InvariantCulture) == 0) {
				if (!current.ChildrenAsProperties) {
					try {
						TextParsed (location, location.PlainText);
						FlushText ();
					} catch {}
				}
				return true;
			}
			
			if (0 != String.Compare (tagid, btag, true, CultureInfo.InvariantCulture))
				return false;

			// if (current is TemplateBuilder)
			//	pop from the id list
			if (current.NeedsTagInnerText ()) {
				try { 
					current.SetTagInnerText (tagInnerText.ToString ());
				} catch (Exception e) {
					throw new ParseException (current.location, e.Message, e);
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

		bool ProcessCode (TagType tagtype, string code, ILocation location)
		{
			ControlBuilder b = null;
			if (tagtype == TagType.CodeRender)
				b = new CodeRenderBuilder (code, false, location);
			else if (tagtype == TagType.CodeRenderExpression)
				b = new CodeRenderBuilder (code, true, location);
			else if (tagtype == TagType.DataBinding)
				b = new DataBindingBuilder (code, location);
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

			if (String.Compare (lang, tparser.Language, true, CultureInfo.InvariantCulture) == 0)
				return;

#if NET_2_0
			CompilationSection section = (CompilationSection) WebConfigurationManager.GetSection ("system.web/compilation");
			if (section.Compilers[tparser.Language] != section.Compilers[lang])
#else
			CompilationConfiguration cfg = CompilationConfiguration.GetInstance (HttpContext.Current); 
			if (!cfg.Compilers.CompareLanguages (tparser.Language, lang))
#endif
				throw new ParseException (Location,
						String.Format ("Trying to mix language '{0}' and '{1}'.", 
								tparser.Language, lang));
		}

		// Used to get CodeRender tags in attribute values
		class CodeRenderParser
		{
			string str;
			ControlBuilder builder;

			public CodeRenderParser (string str, ControlBuilder builder)
			{
				this.str = str;
				this.builder = builder;
			}

			public void AddChildren ()
			{
				int index = str.IndexOf ("<%");
				if (index > 0) {
					TextParsed (null, str.Substring (0, index));
					str = str.Substring (index);
				}

				AspParser parser = new AspParser ("@@inner_string@@", new StringReader (str));
				parser.Error += new ParseErrorHandler (ParseError);
				parser.TagParsed += new TagParsedHandler (TagParsed);
				parser.TextParsed += new TextParsedHandler (TextParsed);
				parser.Parse ();
			}

			void TagParsed (ILocation location, TagType tagtype, string tagid, TagAttributes attributes)
			{
				if (tagtype == TagType.CodeRender)
					builder.AppendSubBuilder (new CodeRenderBuilder (tagid, false, location));
				else if (tagtype == TagType.CodeRenderExpression)
					builder.AppendSubBuilder (new CodeRenderBuilder (tagid, true, location));
				else if (tagtype == TagType.DataBinding)
					builder.AppendSubBuilder (new DataBindingBuilder (tagid, location));
				else
					builder.AppendLiteralString (location.PlainText);
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

