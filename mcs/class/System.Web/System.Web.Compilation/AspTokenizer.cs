//
// System.Web.Compilation.AspTokenizer
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

namespace System.Web.Compilation
{
	class Token
	{
		public const int EOF 		= 0;
		public const int IDENTIFIER 	= 1000;
		public const int DIRECTIVE 	= 1001;
		public const int ATTVALUE   	= 1002;
		public const int TEXT	    	= 1003;
		public const int DOUBLEDASH 	= 1004;
		public const int CLOSING 	= 1005;
	}

	class AspTokenizer
	{
		TextReader sr;
		int current_token;
		StringBuilder sb;
		int col, line;
		int begcol, begline;
		int position;
		bool inTag;
		bool hasPutBack;
		bool verbatim;
		bool have_value;
		string val;
		
		public AspTokenizer (TextReader reader)
		{
			this.sr = reader;
			sb = new StringBuilder ();
			col = line = 1;
			hasPutBack = inTag = false;
		}

		public bool Verbatim
		{
			get { return verbatim; }
			set { verbatim = value; }
		}

		public void put_back ()
		{
			if (hasPutBack)
				throw new HttpException ("put_back called twice!");
			
			hasPutBack = true;
			position -= Value.Length;
		}
		
		public int get_token ()
		{
			if (hasPutBack){
				hasPutBack = false;
				position += Value.Length;
				return current_token;
			}

			begline = line;
			begcol = col;
			have_value = false;
			current_token = NextToken ();
			return current_token;
		}

		bool is_identifier_start_character (char c)
		{
			return (Char.IsLetter (c) || c == '_' );
		}

		bool is_identifier_part_character (char c)
		{
			return (Char.IsLetterOrDigit (c) || c == '_' || c == '-');
		}

		int read_char ()
		{
			int c = sr.Read ();

			if (c == '\r' && sr.Peek () == '\n') {
				c = sr.Read ();
				position++;
			}

			if (c == '\n'){
				col = -1;
				line++;
			}

			if (c != -1) {
				col++;
				position++;
			}

			return c;
		}

		int ReadAttValue (int start)
		{
			int quoteChar = 0;
			bool quoted = false;

			if (start == '"' || start == '\'') {
				quoteChar = start;
				quoted = true;
			} else {
				sb.Append ((char) start);
			}

			int c;
			int last = 0;
			bool inServerTag = false;
			
			while ((c = sr.Peek ()) != -1) {
				if (c == '%' && last == '<') {
					inServerTag = true;
				} else if (inServerTag && c == '>' && last == '%') {
					inServerTag = false;
				} else if (!inServerTag) {
					if (!quoted && (c == '/' || c == '>' || Char.IsWhiteSpace ((char) c))) {
						break;
					} else if (quoted && c == quoteChar && last != '\\') {
						read_char ();
						break;
					}
				}

				sb.Append ((char) c);
				read_char ();
				last = c;
			}

			return Token.ATTVALUE;
		}

		int NextToken ()
		{
			int c;
			
			sb.Length = 0;
			while ((c = read_char ()) != -1){
				if (verbatim){
					inTag = false;
					sb.Append  ((char) c);
					return c;
				}

				if (inTag && (c == '"' || c == '\''))
					return ReadAttValue (c);
				
				if (c == '<'){
					inTag = true;
					sb.Append ((char) c);
					return c;
				}

				if (c == '>'){
					inTag = false;
					sb.Append ((char) c);
					return c;
				}

				if (current_token == '<' && "%/!".IndexOf ((char) c) != -1){
					sb.Append ((char) c);
					return c;
				}

				if (inTag && current_token == '%' && "@#=".IndexOf ((char) c) != -1){
					sb.Append ((char) c);
					return c;
				}

				if (inTag && c == '-' && sr.Peek () == '-'){
					sb.Append ("--");
					read_char ();
					return Token.DOUBLEDASH;
				}

				if (!inTag){
					sb.Append ((char) c);
					while ((c = sr.Peek ()) != -1 && c != '<')
						sb.Append ((char) read_char ());

					return (c != -1 || sb.Length > 0) ? Token.TEXT : Token.EOF;
				}

				if (inTag && current_token == '=' && !Char.IsWhiteSpace ((char) c))
					return ReadAttValue (c);

				if (inTag && is_identifier_start_character ((char) c)){
					sb.Append ((char) c);
					while ((c = sr.Peek ()) != -1) {
						if (!is_identifier_part_character ((char) c) && c != ':')
							break;
						sb.Append ((char) read_char ());
					}

					if (current_token == '@' && Directive.IsDirective (sb.ToString ()))
						return Token.DIRECTIVE;
					
					return Token.IDENTIFIER;
				}

				if (!Char.IsWhiteSpace ((char) c))
					return c;
			}

			return Token.EOF;
		}

		public string Value {
			get {
				if (have_value)
					return val;

				have_value = true;
				val = sb.ToString ();
				return val;
			}
		}

		public int BeginLine {
			get { return begline; }
		}

		public int BeginColumn {
			get { return begcol; }
		}

		public int EndLine {
			get { return line; }
		}

		public int EndColumn {
			get { return col; }
		}

		public int Position {
			get { return position; }
		}
	}
}

