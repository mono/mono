//
// System.Web.Compilation.AspTokenizer
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

namespace System.Web.Compilation {
	
	class Token
	{
		public const int EOF 		= 0;
		public const int IDENTIFIER 	= 1000;
		public const int DIRECTIVE  	= 1001;
		public const int ATTVALUE   	= 1002;
		public const int TEXT	    	= 1003;
		public const int DOUBLEDASH 	= 1004;
		public const int CLOSING 	= 1005;
	}

	class AspTokenizer {
		private StreamReader sr;
		private int current_token;
		private StringBuilder sb;
		private int col, line;
		private bool inTag;
		private bool hasPutBack;
		private bool verbatim;
		private string filename;
		
		public AspTokenizer (string filename, Stream stream)
		{
			if (filename == null || stream == null)
				throw new ArgumentNullException ();

			this.sr = new StreamReader (stream);
			this.filename = filename;
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
				throw new ApplicationException ("put_back called twice!");
				
			hasPutBack = true;
		}
		
		public int get_token ()
		{
			if (hasPutBack){
				hasPutBack = false;
				return current_token;
			}

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

		private int read_char ()
		{
			int c = sr.Read ();

			if (c == '\r' && sr.Peek () == '\n')
				c = sr.Read ();

			if (c == '\n'){
				col = 0;
				line++;
			}
			else if (c != -1)
				col++;

			return c;
		}

		private int NextToken ()
		{
			int c;
			
			sb.Length = 0;
			while ((c = read_char ()) != -1){
				if (verbatim){
					inTag = false;
					sb.Append  ((char) c);
					return c;
				}

				if (inTag && (c == '"' || c == '\'')){
					int previous;
					int start = c;

					previous = 0;
					while ((c = sr.Peek ()) != -1) {
						if (c == start && previous != '\\'){
							read_char ();
							break;
						}
						sb.Append ((char) read_char ());
						previous = c;
					}
					
					return Token.ATTVALUE;
				}
				
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

				if (inTag && current_token == '=' && !Char.IsWhiteSpace ((char) c)){ 
					sb.Append ((char) c);
					while ((c = sr.Peek ()) != -1) {
						if (Char.IsWhiteSpace ((char) c) || c == '/' || c == '>')
							break;
						sb.Append ((char) read_char ());
					}

					return Token.ATTVALUE;
				}

				if (inTag && is_identifier_start_character ((char) c)){
					sb.Append ((char) c);
					while ((c = sr.Peek ()) != -1) {
						if (!is_identifier_part_character ((char) c) && c != ':')
							break;
						sb.Append ((char) read_char ());
					}

					if (current_token == '@' && Directive.IsDirectiveID (sb.ToString ()))
						return Token.DIRECTIVE;

					return Token.IDENTIFIER;
				}

				if (!Char.IsWhiteSpace ((char) c))
					return c;
			}

			return Token.EOF;
		}

		public string value {
			get { return sb.ToString (); }
		}

		public int Line {
			get {
				return line;
			}
		}

		public int Column {
			get {
				return col;
			}
		}

		public string Location {
			get { 
				string msg = filename;
				msg += " (" + line + ", " + col + "): " + sb.ToString ();
				return msg;
			}
		}

	}
}

