//
// System.Web.Compilation.AspGenerator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System.IO;
using System.Text;
using System.Web.Caching;
using System.Web.UI;
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
		
		public AspParser Parser {
			get { return current; }
		}

		public string Filename {
			get { return current.Filename; }
		}
	}
	
	class AspGenerator
	{
		ParserStack pstack;
		BuilderLocationStack stack;
		TemplateParser tparser;
		StringBuilder text;
		RootBuilder rootBuilder;
		bool inScript, javascript;
		ILocation location;
		bool isApplication;
		StringBuilder tagInnerText = new StringBuilder ();
		static Hashtable emptyHash = new Hashtable ();

		public AspGenerator (TemplateParser tparser)
		{
			this.tparser = tparser;
			tparser.AddDependency (tparser.InputFile);
			text = new StringBuilder ();
			stack = new BuilderLocationStack ();
			rootBuilder = new RootBuilder (tparser);
			stack.Push (rootBuilder, null);
			tparser.RootBuilder = rootBuilder;
			pstack = new ParserStack ();
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

			throw new Exception ("Got type: " + type);
		}
		
		void InitParser (string filename)
		{
			StreamReader reader = new StreamReader (filename, WebEncoding.FileEncoding);
			AspParser parser = new AspParser (filename, reader);
			reader.Close ();
			parser.Error += new ParseErrorHandler (ParseError);
			parser.TagParsed += new TagParsedHandler (TagParsed);
			parser.TextParsed += new TextParsedHandler (TextParsed);
			if (!pstack.Push (parser))
				throw new ParseException (Location, "Infinite recursion detected including file: " + filename);
			tparser.AddDependency (filename);
		}

		void DoParse ()
		{
			pstack.Parser.Parse ();
			if (text.Length > 0)
				FlushText ();

			pstack.Pop ();
		}

		public Type GetCompiledType ()
		{
			Type type = (Type) HttpRuntime.Cache.Get ("@@Type" + tparser.InputFile);
			if (type != null) {
				return type;
			}

			isApplication = tparser.DefaultDirectiveName == "application";
			InitParser (Path.GetFullPath (tparser.InputFile));

			DoParse ();
#if DEBUG
			PrintTree (rootBuilder, 0);
#endif

			if (stack.Count > 1)
				throw new ParseException (stack.Builder.location,
						"Expecting </" + stack.Builder.TagName + ">" + stack.Builder);

			BaseCompiler compiler = GetCompilerFromType ();

			type = compiler.GetCompiledType ();
			CacheDependency cd = new CacheDependency ((string[])
							tparser.Dependencies.ToArray (typeof (string)));

			HttpRuntime.Cache.Insert ("@@Type" + tparser.InputFile, type, cd);
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
#endif
		
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
				FlushText ();

			if (0 == String.Compare (tagid, "script", true)) {
				if (ProcessScript (tagtype, attributes))
					return;
			}

			switch (tagtype) {
			case TagType.Directive:
				if (tagid == "")
					tagid = tparser.DefaultDirectiveName;

				tparser.AddDirective (tagid, attributes.GetDictionary (null));
				break;
			case TagType.Tag:
				if (!ProcessTag (tagid, attributes, tagtype))
					TextParsed (location, location.PlainText);
				break;
			case TagType.Close:
				if (!CloseControl (tagid))
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
					file = tparser.MapPath (file);
				} else {
					file = GetIncludeFilePath (tparser.BaseDir, file);
				}

				InitParser (file);
				DoParse ();
				break;
			default:
				break;
			}
			//PrintLocation (location);
		}

		static string GetIncludeFilePath (string basedir, string filename)
		{
			if (Path.DirectorySeparatorChar == '/')
				filename = filename.Replace ("\\", "/");

			return Path.GetFullPath (Path.Combine (basedir, filename));
		}
		
		void TextParsed (ILocation location, string text)
		{
			if (text.IndexOf ("<%") != -1 && !inScript) {
				if (this.text.Length > 0)
					FlushText ();
				CodeRenderParser r = new CodeRenderParser (text, stack.Builder);
				r.AddChildren ();
				return;
			}

			this.text.Append (text);
			//PrintLocation (location);
		}

		void FlushText ()
		{
			string t = text.ToString ();
			text.Length = 0;
			if (inScript) {
				// TODO: store location
				tparser.Scripts.Add (t);
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

		bool ProcessTag (string tagid, TagAttributes atts, TagType tagtype)
		{
			if ((atts == null || !atts.IsRunAtServer ()) && String.Compare (tagid, "tbody", true) == 0) {
				// MS completely ignores tbody or, if runat="server", fails when compiling
				if (stack.Count > 0)
					return stack.Builder.ChildrenAsProperties;

				return false;
			}

			if (isApplication) {
				if (String.Compare (tagid, "object", true) != 0)
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

			builder.location = location;
			builder.ID = htable ["id"] as string;
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

		bool ProcessScript (TagType tagtype, TagAttributes attributes)
		{
			if (tagtype != TagType.Close) {
				if (attributes != null && attributes.IsRunAtServer ()) {
					CheckLanguage ((string) attributes ["language"]);
					if (tagtype == TagType.Tag) {
						Parser.VerbatimID = "script";
						inScript = true;
					} //else if (tagtype == TagType.SelfClosing)
						// load script file here

					return true;
				} else {
					Parser.VerbatimID = "script";
					javascript = true;
					TextParsed (location, location.PlainText);
					return true;
				}
			}

			bool result;
			if (inScript) {
				result = inScript;
				inScript = false;
			} else {
				result = javascript;
				javascript = false;
				TextParsed (location, location.PlainText);
			}

			return result;
		}

		bool CloseControl (string tagid)
		{
			ControlBuilder current = stack.Builder;
			if (String.Compare (tagid, "tbody", true) == 0) {
				if (!current.ChildrenAsProperties) {
					try {
						TextParsed (location, location.PlainText);
						FlushText ();
					} catch {}
				}
				return true;
			}
			
			string btag = current.TagName;
			if (0 != String.Compare (tagid, btag, true))
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

			if (String.Compare (lang, tparser.Language, true) != 0) {
				throw new ParseException (Location,
						String.Format ("Trying to mix language '{0}' and '{1}'.", 
								tparser.Language, lang));
			}
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

