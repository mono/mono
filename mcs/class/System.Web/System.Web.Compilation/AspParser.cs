//
// System.Web.Compilation.AspParser
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
using System.IO;
using System.Text;

namespace System.Web.Compilation
{
	delegate void ParseErrorHandler (ILocation location, string message);
	delegate void TextParsedHandler (ILocation location, string text);
	delegate void TagParsedHandler (ILocation location, TagType tagtype, string id, TagAttributes attributes);

	class AspParser : ILocation
	{
		AspTokenizer tokenizer;
		int beginLine, endLine;
		int beginColumn, endColumn;
		int beginPosition, endPosition;
		string filename;
		string fileText;
		string verbatimID;

		public AspParser (string filename, TextReader input)
		{
			this.filename = filename;
			fileText = input.ReadToEnd ();
			StringReader reader = new StringReader (fileText);
			tokenizer = new AspTokenizer (reader);
		}

		public int BeginLine {
			get { return beginLine; }
		}

		public int BeginColumn {
			get { return beginColumn; }
		}

		public int EndLine {
			get { return endLine; }
		}

		public int EndColumn {
			get { return endColumn; }
		}

		public string PlainText {
			get {
				if (beginPosition >= endPosition)
					return null;

				return fileText.Substring (beginPosition, endPosition - beginPosition);
			}
		}

		public string Filename {
			get { return filename; }
		}

		public string VerbatimID {
			set {
				tokenizer.Verbatim = true;
				verbatimID = value.ToUpper ();
			}
		}
		
		bool Eat (int expected_token)
		{
			if (tokenizer.get_token () != expected_token) {
				tokenizer.put_back ();
				return false;
			}

			endLine = tokenizer.EndLine;
			endColumn = tokenizer.EndColumn;
			return true;
		}

		void BeginElement ()
		{
			beginLine = tokenizer.BeginLine;
			beginColumn = tokenizer.BeginColumn;
			beginPosition = tokenizer.Position - 1;
		}

		void EndElement ()
		{
			endLine = tokenizer.EndLine;
			endColumn = tokenizer.EndColumn;
			endPosition = tokenizer.Position;
		}

		public void Parse ()
		{
			int token;
			string id;
			TagAttributes attributes;
			TagType tagtype = TagType.Text;
			StringBuilder text =  new StringBuilder ();

			while ((token = tokenizer.get_token ()) != Token.EOF) {
				BeginElement ();

				if (tokenizer.Verbatim){
					string end_verbatim = "</" + verbatimID + ">";
					string verbatim_text = GetVerbatim (token, end_verbatim);

					if (verbatim_text == null)
						OnError ("Unexpected EOF processing " + verbatimID);

					tokenizer.Verbatim = false;

					EndElement ();
					endPosition -= end_verbatim.Length;
					OnTextParsed (verbatim_text);
					beginPosition = endPosition;
					endPosition += end_verbatim.Length;
					OnTagParsed (TagType.Close, verbatimID, null);
					continue;
				}
				
				if (token == '<') {
					GetTag (out tagtype, out id, out attributes);
					EndElement ();
					if (tagtype == TagType.ServerComment)
						continue;

					if (tagtype == TagType.Text)
						OnTextParsed (id);
					else
						OnTagParsed (tagtype, id, attributes);

					continue;
				}

				if (tokenizer.Value.Trim () == "" && tagtype == TagType.Directive) {
					continue;
				}

				text.Length = 0;
				do {
					text.Append (tokenizer.Value);
					token = tokenizer.get_token ();
				} while (token != '<' && token != Token.EOF);

				tokenizer.put_back ();
				EndElement ();
				OnTextParsed (text.ToString ());
			}
		}

		bool GetInclude (string str, out string pathType, out string filename)
		{
			pathType = null;
			filename = null;
			str = str.Substring (2).Trim ();
			int len = str.Length;
			int lastQuote = str.LastIndexOf ('"');
			if (len < 10 || lastQuote != len - 1)
				return false;

			if (!str.ToLower ().StartsWith ("#include "))
				return false;

			str = str.Substring (9).Trim ();
			bool isfile = (str.ToLower ().StartsWith ("file"));
			if (!isfile && !str.ToLower ().StartsWith ("virtual"))
				return false;

			pathType = (isfile) ? "file" : "virtual";
			if (str.Length < pathType.Length + 3)
				return false;

			str = str.Substring (pathType.Length).Trim ();
			if (str.Length < 3 || str [0] != '=')
				return false;

			int index = 1;
			for (; index < str.Length; index++) {
				if (Char.IsWhiteSpace (str [index]))
					index++;
				else if (str [index] == '"')
					break;
			}

			if (index == str.Length || index == lastQuote)
				return false;

			str = str.Substring (index);
			if (str.Length == 2) { // only quotes
				OnError ("Empty file name.");
				return false;
			}

			filename = str.Trim ().Substring (index, str.Length - 2);
			if (filename.LastIndexOf  ('"') != -1)
				return false; // file=""" -> no error

			return true;
		}

		void GetTag (out TagType tagtype, out string id, out TagAttributes attributes)
		{
			int token = tokenizer.get_token ();

			tagtype = TagType.ServerComment;
			id = null;
			attributes = null;
			switch (token){
			case '%':
				GetServerTag (out tagtype, out id, out attributes);
				break;
			case '/':
				if (!Eat (Token.IDENTIFIER))
					OnError ("expecting TAGNAME");

				id = tokenizer.Value;
				if (!Eat ('>'))
					OnError ("expecting '>'. Got '" + id + "'");

				tagtype = TagType.Close;
				break;
			case '!':
				bool double_dash = Eat (Token.DOUBLEDASH);
				if (double_dash)
					tokenizer.put_back ();

				tokenizer.Verbatim = true;
				string end = double_dash ? "-->" : ">";
				string comment = GetVerbatim (tokenizer.get_token (), end);
				tokenizer.Verbatim = false;
				if (comment == null)
					OnError ("Unfinished HTML comment/DTD");

				string pathType, filename;
				if (double_dash && GetInclude (comment, out pathType, out filename)) {
					tagtype = TagType.Include;
					attributes = new TagAttributes ();
					attributes.Add (pathType, filename);
				} else {
					tagtype = TagType.Text;
					id = "<!" + comment + end;
				}
				break;
			case Token.IDENTIFIER:
				if (this.filename == "@@inner_string@@") {
					// Actually not tag but "xxx < yyy" stuff in inner_string!
					tagtype = TagType.Text;
					tokenizer.InTag = false;
					id = "<" + tokenizer.Odds + tokenizer.Value;
				} else {
					id = tokenizer.Value;
					try {
						attributes = GetAttributes ();
					} catch (Exception e) {
						OnError (e.Message);
						break;
					}
					
					tagtype = TagType.Tag;
					if (Eat ('/') && Eat ('>'))
						tagtype = TagType.SelfClosing;
					else if (!Eat ('>'))
						OnError ("expecting '>'. Got '" + tokenizer.Value + "'");
				}

				break;
			default:
				tagtype = TagType.Text;
				tokenizer.InTag = false;
				id = "<" + tokenizer.Value;
				break;
			}
		}

		TagAttributes GetAttributes ()
		{
			int token;
			TagAttributes attributes;
			string id;

			attributes = new TagAttributes ();
			while ((token = tokenizer.get_token ()) != Token.EOF){
				if (token != Token.IDENTIFIER)
					break;
				id = tokenizer.Value;
				if (Eat ('=')){
					if (Eat (Token.ATTVALUE)){
						attributes.Add (id, tokenizer.Value);
					} else if (Eat ('<') && Eat ('%')) {
						attributes.Add (id, "<%" +
								GetVerbatim (tokenizer.get_token (), "%>"));
					} else {
						OnError ("expected ATTVALUE");
						return null;
					}
					
				} else {
					attributes.Add (id, null);
				}
			}

			tokenizer.put_back ();
			return attributes;
		}

		string GetVerbatim (int token, string end)
		{
			StringBuilder vb_text = new StringBuilder ();
			int i = 0;

			if (tokenizer.Value.Length > 1){
				// May be we have a put_back token that is not a single character
				vb_text.Append (tokenizer.Value);
				token = tokenizer.get_token ();
			}

			while (token != Token.EOF){
				if (Char.ToUpper ((char) token) == end [i]){
					if (++i >= end.Length)
						break;
					token = tokenizer.get_token ();
					continue;
				} else if (i > 0) {
					for (int j = 0; j < i; j++)
						vb_text.Append (end [j]);
					i = 0;
				}

				vb_text.Append ((char) token);
				token = tokenizer.get_token ();
			} 

			return RemoveComments (vb_text.ToString ());
		}

		string RemoveComments (string text)
		{
			int end;
			int start = text.IndexOf ("<%--");

			while (start != -1) {
				end = text.IndexOf ("--%>");
				if (end == -1 || end <= start + 1)
					break;

				text = text.Remove (start, end - start + 4);
				start = text.IndexOf ("<%--");
			}

			return text;
		}

		void GetServerTag (out TagType tagtype, out string id, out TagAttributes attributes)
		{
			string inside_tags;

			if (Eat ('@')){
				tagtype = TagType.Directive;
				id = "";
				if (Eat (Token.DIRECTIVE))
					id = tokenizer.Value;

				attributes = GetAttributes ();
				if (!Eat ('%') || !Eat ('>'))
					OnError ("expecting '%>'");

				return;
			}
			
			if (Eat (Token.DOUBLEDASH)) {
				tokenizer.Verbatim = true;
				inside_tags = GetVerbatim (tokenizer.get_token (), "--%>");
				tokenizer.Verbatim = false;
				id = null;
				attributes = null;
				tagtype = TagType.ServerComment;
				return;
			}

			bool varname;
			bool databinding;
			varname = Eat ('=');
			databinding = !varname && Eat ('#');

			tokenizer.Verbatim = true;
			inside_tags = GetVerbatim (tokenizer.get_token (), "%>");
			tokenizer.Verbatim = false;
			id = inside_tags;
			attributes = null;
			tagtype = (databinding ? TagType.DataBinding :
				  (varname ? TagType.CodeRenderExpression : TagType.CodeRender));
		}

		public event ParseErrorHandler Error;
		public event TagParsedHandler TagParsed;
		public event TextParsedHandler TextParsed;

		void OnError (string msg)
		{
			if (Error != null)
				Error (this, msg);
		}

		void OnTagParsed (TagType tagtype, string id, TagAttributes attributes)
		{
			if (TagParsed != null)
				TagParsed (this, tagtype, id, attributes);
		}

		void OnTextParsed (string text)
		{
			if (TextParsed != null)
				TextParsed (this, text);
		}
	}

}

