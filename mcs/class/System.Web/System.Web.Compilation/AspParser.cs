//
// System.Web.Compilation.AspParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Marek Habersack <mhabersack@novell.com>
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004-2009 Novell, Inc (http://novell.com)
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
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Util;
using System.Security.Cryptography;

namespace System.Web.Compilation
{
	delegate void ParseErrorHandler (ILocation location, string message);
	delegate void TextParsedHandler (ILocation location, string text);
	delegate void TagParsedHandler (ILocation location, TagType tagtype, string id, TagAttributes attributes);
	delegate void ParsingCompleteHandler ();
	
	class AspParser : ILocation
	{
		static readonly object errorEvent = new object ();
		static readonly object tagParsedEvent = new object ();
		static readonly object textParsedEvent = new object ();
		static readonly object parsingCompleteEvent = new object();

		MD5 checksum;
		AspTokenizer tokenizer;
		int beginLine, endLine;
		int beginColumn, endColumn;
		int beginPosition, endPosition;
		string filename;
		string verbatimID;
		string fileText;
		StringReader fileReader;
		bool _internal;
		int _internalLineOffset;
		int _internalPositionOffset;
		AspParser outer;
		
		EventHandlerList events = new EventHandlerList ();
		
		public event ParseErrorHandler Error {
			add { events.AddHandler (errorEvent, value); }
			remove { events.RemoveHandler (errorEvent, value); }
		}
		
		public event TagParsedHandler TagParsed {
			add { events.AddHandler (tagParsedEvent, value); }
			remove { events.RemoveHandler (tagParsedEvent, value); }
		}
		
		public event TextParsedHandler TextParsed {
			add { events.AddHandler (textParsedEvent, value); }
			remove { events.RemoveHandler (textParsedEvent, value); }
		}

		public event ParsingCompleteHandler ParsingComplete {
			add { events.AddHandler (parsingCompleteEvent, value); }
			remove { events.RemoveHandler (parsingCompleteEvent, value); }
		}
		
		public AspParser (string filename, TextReader input)
		{
			this.filename = filename;
			this.fileText = input.ReadToEnd ();
			this.fileReader = new StringReader (this.fileText);
			this._internalLineOffset = 0;
			tokenizer = new AspTokenizer (this.fileReader);
		}

		public AspParser (string filename, TextReader input, int startLineOffset, int positionOffset, AspParser outer)
			: this (filename, input)
		{
			this._internal = true;
			this._internalLineOffset = startLineOffset;
			this._internalPositionOffset = positionOffset;
			this.outer = outer;
		}
		
		public byte[] MD5Checksum {
			get {
				if (checksum == null)
					return new byte[0];
				
				return checksum.Hash;
			}
		}

		public int BeginPosition {
			get { return beginPosition; }
		}

		public int EndPosition {
			get { return endPosition; }
		}
		
		public int BeginLine {
			get {
				if (_internal)
					return beginLine + _internalLineOffset;

				return beginLine;
			}
		}

		public int BeginColumn {
			get { return beginColumn; }
		}

		public int EndLine {
			get {
				if (_internal)
					return endLine + _internalLineOffset;
				return endLine;
			}
		}

		public int EndColumn {
			get { return endColumn; }
		}

		public string FileText {
			get {
				string ret = null;
				
				if (_internal && outer != null)
				 	ret = outer.FileText;
				
				if (ret == null && fileText != null)
					ret = fileText;
				
				return ret;
			}
		}
		
		public string PlainText {
			get {
				if (beginPosition >= endPosition || fileText == null)
					return null;

				string text = FileText;
				int start, len;
				
				if (_internal && outer != null) {
					start = beginPosition + _internalPositionOffset;
					len = (endPosition + _internalPositionOffset) - start;
				} else {
					start = beginPosition;
					len = endPosition - beginPosition;
				}
				
				if (text != null)
					return text.Substring (start, len);

				return null;
			}
		}

		public string Filename {
			get {
				if (_internal && outer != null)
					return outer.Filename;
				
				return filename;
			}
		}

		public string VerbatimID {
			set {
				tokenizer.Verbatim = true;
				verbatimID = value;
			}
		}

		bool Eat (int expected_token)
		{
			int token = tokenizer.get_token ();
			if (token != expected_token) {
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
			if (tokenizer == null) {
				OnError ("AspParser not initialized properly.");
				return;
			}
			
			int token;
			string id;
			TagAttributes attributes;
			TagType tagtype = TagType.Text;
			StringBuilder text =  new StringBuilder ();

			try {
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

					if (tokenizer.Value.Trim ().Length == 0 && tagtype == TagType.Directive) {
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
			} finally {
				if (fileReader != null) {
					fileReader.Close ();
					fileReader = null;
				}
				checksum = tokenizer.Checksum;
				tokenizer = null;
			}

			OnParsingComplete ();
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

			if (!StrUtils.StartsWith (str, "#include ", true))
				return false;

			str = str.Substring (9).Trim ();
			bool isfile = (StrUtils.StartsWith (str ,"file", true));
			if (!isfile && !StrUtils.StartsWith (str, "virtual", true))
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
					continue;
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
					if (Eat ('/') && Eat ('>')) {
						tagtype = TagType.SelfClosing;
					} else if (!Eat ('>')) {
						if (attributes.IsRunAtServer ()) {
							OnError ("The server tag is not well formed.");
							break;
						}
						tokenizer.Verbatim = true;
						attributes.Add (String.Empty, GetVerbatim (tokenizer.get_token (), ">") + ">");
						tokenizer.Verbatim = false;
					}
				}

				break;
			default:
				string idvalue = null;
				// This is to handle code like:
				//
				//  <asp:ListItem runat="server"> < </asp:ListItem>
				//
				if ((char)token == '<') {
					string odds = tokenizer.Odds;
					if (odds != null && odds.Length > 0 && Char.IsWhiteSpace (odds [0])) {
						tokenizer.put_back ();
						idvalue = odds;
					} else
						idvalue = tokenizer.Value;
				} else
					idvalue = tokenizer.Value;
				
				tagtype = TagType.Text;
				tokenizer.InTag = false;
				id = "<" + idvalue;
				break;
			}
		}

		TagAttributes GetAttributes ()
		{
			int token;
			TagAttributes attributes;
			string id;
			bool wellFormedForServer = true;

			attributes = new TagAttributes ();
			while ((token = tokenizer.get_token ()) != Token.EOF){
				if (token == '<' && Eat ('%')) {
					tokenizer.Verbatim = true;
					attributes.Add (String.Empty, "<%" + 
							GetVerbatim (tokenizer.get_token (), "%>") + "%>");
					tokenizer.Verbatim = false;
					tokenizer.InTag = true;
					continue;
				}
					
				if (token != Token.IDENTIFIER)
					break;

				id = tokenizer.Value;
				if (Eat ('=')){
					if (Eat (Token.ATTVALUE)){
						attributes.Add (id, tokenizer.Value);
						wellFormedForServer &= tokenizer.AlternatingQuotes;
					} else if (Eat ('<') && Eat ('%')) {
						tokenizer.Verbatim = true;
						attributes.Add (id, "<%" + 
								GetVerbatim (tokenizer.get_token (), "%>") + "%>");
						tokenizer.Verbatim = false;
						tokenizer.InTag = true;
					} else {
						OnError ("expected ATTVALUE");
						return null;
					}
				} else {
					attributes.Add (id, null);
				}
			}

			tokenizer.put_back ();

			if (attributes.IsRunAtServer () && !wellFormedForServer) {
				OnError ("The server tag is not well formed.");
				return null;
			}
			
			return attributes;
		}

		string GetVerbatim (int token, string end)
		{
			StringBuilder vb_text = new StringBuilder ();
			StringBuilder tmp = new StringBuilder ();
			int i = 0;

			if (tokenizer.Value.Length > 1){
				// May be we have a put_back token that is not a single character
				vb_text.Append (tokenizer.Value);
				token = tokenizer.get_token ();
			}

			end = end.ToLower (Helpers.InvariantCulture);
			int repeated = 0;
			for (int k = 0; k < end.Length; k++)
				if (end [0] == end [k])
					repeated++;
			
			while (token != Token.EOF){
				if (Char.ToLower ((char) token, Helpers.InvariantCulture) == end [i]){
					if (++i >= end.Length)
						break;
					tmp.Append ((char) token);
					token = tokenizer.get_token ();
					continue;
				} else if (i > 0) {
					if (repeated > 1 && i == repeated && (char) token == end [0]) {
						vb_text.Append ((char) token);
						token = tokenizer.get_token ();
						continue;
					}
					vb_text.Append (tmp.ToString ());
					tmp.Remove (0, tmp.Length);
					i = 0;
				}

				vb_text.Append ((char) token);
				token = tokenizer.get_token ();
			} 

			if (token == Token.EOF)
				OnError ("Expecting " + end + " and got EOF.");

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
			bool old = tokenizer.ExpectAttrValue;

			tokenizer.ExpectAttrValue = false;
			if (Eat ('@')){
				tokenizer.ExpectAttrValue = old;
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
				tokenizer.ExpectAttrValue = old;
				tokenizer.Verbatim = true;
				inside_tags = GetVerbatim (tokenizer.get_token (), "--%>");
				tokenizer.Verbatim = false;
				id = null;
				attributes = null;
				tagtype = TagType.ServerComment;
				return;
			}

			tokenizer.ExpectAttrValue = old;
			bool varname;
			bool databinding;
#if NET_4_0
			bool codeRenderEncode;
#endif
			varname = Eat ('=');
			databinding = !varname && Eat ('#');
#if NET_4_0
			codeRenderEncode = !databinding && !varname && Eat (':');
#endif
			string odds = tokenizer.Odds;
			
			tokenizer.Verbatim = true;
			inside_tags = GetVerbatim (tokenizer.get_token (), "%>");
			if (databinding && odds != null && odds.Length > 0) {
				databinding = false;

				// We encountered <% #something here %>, this should be passed
				// verbatim to the compiler
				inside_tags = '#' + inside_tags;
			}			

			tokenizer.Verbatim = false;
			id = inside_tags;
			attributes = null;
			if (databinding)
				tagtype = TagType.DataBinding;
			else if (varname)
				tagtype = TagType.CodeRenderExpression;
#if NET_4_0
			else if (codeRenderEncode)
				tagtype = TagType.CodeRenderEncode;
#endif
			else
				tagtype = TagType.CodeRender;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("AspParser {");
			if (filename != null && filename.Length > 0)
				sb.AppendFormat ("{0}:{1}.{2}", filename, beginLine, beginColumn);
			sb.Append ('}');

			return sb.ToString ();
		}
		
		void OnError (string msg)
		{
			ParseErrorHandler eh = events [errorEvent] as ParseErrorHandler;
			if (eh != null)
				eh (this, msg);
		}

		void OnTagParsed (TagType tagtype, string id, TagAttributes attributes)
		{
			TagParsedHandler eh = events [tagParsedEvent] as TagParsedHandler;
			if (eh != null)
				eh (this, tagtype, id, attributes);
		}

		void OnTextParsed (string text)
		{
			TextParsedHandler eh = events [textParsedEvent] as TextParsedHandler;
			if (eh != null)
				eh (this, text);
		}

		void OnParsingComplete ()
		{
			ParsingCompleteHandler eh = events [parsingCompleteEvent] as ParsingCompleteHandler;
			if (eh != null)
				eh ();
		}
	}
}

