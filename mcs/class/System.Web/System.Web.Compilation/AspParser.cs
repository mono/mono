//
// System.Web.Compilation.AspParser
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

namespace System.Web.Compilation
{
	class AspParser {
		private AspTokenizer tokenizer;
		private ArrayList elements; // List of processed elements in the HTML page.

		private void error ()
		{
			throw new HttpException ("Error parsing: " + tokenizer.location);
		}

		private void error (string msg)
		{
			throw new Exception ("Error: "+ msg + "\n" + tokenizer.location);
		}

		public AspParser (AspTokenizer tokenizer)
		{
			this.tokenizer = tokenizer;
			elements = new ArrayList ();
		}

		public AspParser (string filename, Stream input) : 
			this (new AspTokenizer (filename, input))
		{
		}

		public ArrayList Elements
		{
			get { return elements; }
		}

		private bool Eat (int expected_token)
		{
			if (tokenizer.get_token () != expected_token) {
				tokenizer.put_back ();
				return false;
			}
			return true;
		}

		private void AddPlainText (string newText)
		{
			if (elements.Count > 0){
				Element element = (Element) elements [elements.Count - 1];
				if (element is PlainText){
					((PlainText) element).Append (newText);
					return;
				}
			}
			elements.Add (new PlainText (newText));
		}
		
		public void Parse ()
		{
			int token;
			Element element;
			Tag tag_element;
			string tag = "";

			while ((token = tokenizer.get_token ()) != Token.EOF){
				if (tokenizer.Verbatim){
					string end_verbatim = "</" + tag + ">";
					string verbatim_text = GetVerbatim (token, end_verbatim);

					if (verbatim_text == null)
						error ("Unexpected EOF processing " + tag);

					AddPlainText (verbatim_text);
					elements.Add (new CloseTag (tag));
					tokenizer.Verbatim = false;
				}
				else if (token == '<') {
					element = GetTag ();
					if (element == null)
						error ();

					if (element is ServerComment)
						continue;

					if (!(element is Tag)){
						AddPlainText (((PlainText) element).Text);
						continue;
					}

					elements.Add (element);

					tag_element = element as Tag;
					tag = tag_element.TagID.ToUpper ();
					if (!tag_element.SelfClosing && (tag == "SCRIPT" || tag == "PRE"))
						tokenizer.Verbatim = true;
				}
				else {
					StringBuilder text =  new StringBuilder ();
					do {
						text.Append (tokenizer.value);
						token = tokenizer.get_token ();
					} while (token != '<' && token != Token.EOF);
					tokenizer.put_back ();
					AddPlainText (text.ToString ());
				}
			}
		}

		private Element GetTag ()
		{
			int token = tokenizer.get_token ();
			string id;

			switch (token){
			case '%':
				return GetServerTag ();
			case '/':
				if (!Eat (Token.IDENTIFIER))
					error ("expecting TAGNAME");
				id = tokenizer.value;
				if (!Eat ('>'))
					error ("expecting '>'");
				return new CloseTag (id);
			case '!':
				bool double_dash = Eat (Token.DOUBLEDASH);
				if (double_dash)
					tokenizer.put_back ();

				tokenizer.Verbatim = true;
				string end = double_dash ? "-->" : ">";
				string comment = GetVerbatim (tokenizer.get_token (), end);
				tokenizer.Verbatim = false;
				if (comment == null)
					error ("Unfinished HTML comment/DTD");

				return new PlainText ("<!" + comment + end);
			case Token.IDENTIFIER:
				id = tokenizer.value;
				Tag tag = new Tag (id, GetAttributes (), Eat ('/'));
				if (!Eat ('>'))
					error ("expecting '>'");
				return tag;
			default:
				return null;
			}
		}

		private TagAttributes GetAttributes ()
		{
			int token;
			TagAttributes attributes;
			string id;

			attributes = new TagAttributes ();
			while ((token = tokenizer.get_token ())  != Token.EOF){
				if (token != Token.IDENTIFIER)
					break;
				id = tokenizer.value;
				if (Eat ('=')){
					if (Eat (Token.ATTVALUE)){
						attributes.Add (id, tokenizer.value);
					} else {
						//TODO: support data binding syntax without quotes
						error ("expected ATTVALUE");
						return null;
					}
					
				} else {
					attributes.Add (id, null);
				}
			}

			tokenizer.put_back ();
			if (attributes.Count == 0)
				return null;

			return attributes;
		}

		private string GetVerbatim (int token, string end)
		{
			StringBuilder vb_text = new StringBuilder ();
			int i = 0;

			if (tokenizer.value.Length > 1){
				// May be we have a put_back token that is not a single character
				vb_text.Append (tokenizer.value);
				token = tokenizer.get_token ();
			}

			while (token != Token.EOF){
				if (Char.ToUpper ((char) token) == end [i]){
					if (++i >= end.Length)
						break;
					token = tokenizer.get_token ();
					continue;
				}
				else {
					for (int j = 0; j < i; j++)
						vb_text.Append (end [j]);
				}

				i = 0;
				vb_text.Append ((char) token);
				token = tokenizer.get_token ();
			} 

			if (token == Token.EOF)
				return null;

			return RemoveComments (vb_text.ToString ());
		}

		private string RemoveComments (string text)
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

		private Element GetServerTag ()
		{
			string id;
			string inside_tags;
			TagAttributes attributes;

			if (Eat ('@')){
				id = (Eat (Token.DIRECTIVE) ? tokenizer.value : "Page");
				attributes = GetAttributes ();
				if (!Eat ('%') || !Eat ('>'))
					error ("expecting '%>'");

				return new Directive (id, attributes);
			} else if (Eat (Token.DOUBLEDASH)) {
				tokenizer.Verbatim = true;
				inside_tags = GetVerbatim (tokenizer.get_token (), "--%>");
				tokenizer.Verbatim = false;
				return new ServerComment ("<%--" + inside_tags + "--%>");
			}

			bool varname;
			bool databinding;
			varname = Eat ('=');
			databinding = !varname && Eat ('#');

			tokenizer.Verbatim = true;
			inside_tags = GetVerbatim (tokenizer.get_token (), "%>");
			tokenizer.Verbatim = false;
			if (databinding)
				return new DataBindingTag (inside_tags);

			return new CodeRenderTag (varname, inside_tags);
		}

	}

}

