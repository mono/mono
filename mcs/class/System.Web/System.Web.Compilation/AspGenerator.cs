//
// System.Web.Compilation.AspGenerator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Web.UI;

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

	class AspGenerator
	{
		string filename;
		AspParser parser;
		BuilderLocationStack stack;
		TemplateParser tparser;
		StringBuilder text;
		RootBuilder rootBuilder;
		bool inScript;
		ILocation location;
		static Hashtable emptyHash = new Hashtable ();

		public AspGenerator (TemplateParser tparser)
		{
			this.tparser = tparser;
			this.filename = Path.GetFullPath (tparser.InputFile);
			tparser.AddDependency (tparser.InputFile);
			text = new StringBuilder ();
			stack = new BuilderLocationStack ();
			rootBuilder = new RootBuilder (tparser);
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

			throw new Exception ("Got type: " + type);
		}
		
		public Type GetCompiledType ()
		{
			//FIXME: use the encoding of the file or the one specified in the machine.config/web.config file.
			StreamReader reader = new StreamReader (filename, Encoding.Default);
			parser = new AspParser (filename, reader);
			parser.Error += new ParseErrorHandler (ParseError);
			parser.TagParsed += new TagParsedHandler (TagParsed);
			parser.TextParsed += new TextParsedHandler (TextParsed);

			parser.Parse ();
			if (text.Length > 0)
				FlushText ();

#if DEBUG
			PrintTree (rootBuilder, 0);
#endif

			if (stack.Count > 1)
				throw new ParseException (stack.Builder.location,
						"Expecting </" + stack.Builder.TagName + ">" + stack.Builder);

			BaseCompiler compiler = GetCompilerFromType ();

			return compiler.GetCompiledType ();
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
				if (!ProcessTag (tagid, attributes))
					TextParsed (location, location.PlainText);
				break;
			case TagType.Close:
				if (!CloseControl (tagid))
					TextParsed (location, location.PlainText);
				break;
			case TagType.SelfClosing:
				int count = stack.Count;
				if (!ProcessTag (tagid, attributes)) {
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
				ProcessCode (tagtype, tagid, location);
				break;
			case TagType.Include:
				string file = attributes ["virtual"] as string;
				bool isvirtual = (file != null);
				if (!isvirtual)
					file = attributes ["file"] as string;

				TextParsed (location, tparser.ProcessInclude (isvirtual, file));
				break;
			default:
				break;
			}
			//PrintLocation (location);
		}

		void TextParsed (ILocation location, string text)
		{
			if (text.IndexOf ("<%") != -1) {
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

			stack.Builder.AppendLiteralString (t);
		}

		bool ProcessTag (string tagid, TagAttributes atts)
		{
			ControlBuilder parent = stack.Builder;
			ControlBuilder builder = null;
			BuilderLocation bl = null;
			Hashtable htable = (atts != null) ? atts.GetDictionary (null) : emptyHash;
			if (stack.Count > 1)
				builder = parent.CreateSubBuilder (tagid, htable, null, tparser, location);

			if (builder == null && atts != null && atts.IsRunAtServer ())
				builder = rootBuilder.CreateSubBuilder (tagid, htable, null, tparser, location);
			
			if (builder == null)
				return false;

			builder.location = location;
			builder.ID = htable ["id"] as string;
			if (builder.HasBody ()) {
				if (builder is TemplateBuilder) {
				//	push the id list
				}
				stack.Push (builder, location);
			} else {
				// FIXME:ObjectTags...
				parent.AppendSubBuilder (builder);
				builder.CloseControl ();
			}

			return true;
		}

		bool ProcessScript (TagType tagtype, TagAttributes attributes)
		{
			if (tagtype != TagType.Close && attributes != null && attributes.IsRunAtServer ()) {
				if (tagtype == TagType.Tag) {
					parser.VerbatimID = "script";
					inScript = true;
				} //else if (tagtype == TagType.SelfClosing)
					// load script file here

				return true;
			}

			bool result = inScript;
			inScript = false;

			return result;
		}

		bool CloseControl (string tagid)
		{
			ControlBuilder current = stack.Builder;
			string btag = current.TagName;
			if (0 != String.Compare (tagid, btag, true))
				return false;

			// if (current is TemplateBuilder)
			//	pop from the id list
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

