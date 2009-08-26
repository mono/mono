//
// System.Web.Compilation.AspTokenizer
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
using System.Security.Cryptography;

namespace System.Web.Compilation
{
	class Token
	{
		public const int EOF 		= 0x0200000;
		public const int IDENTIFIER 	= 0x0200001;
		public const int DIRECTIVE 	= 0x0200002;
		public const int ATTVALUE	= 0x0200003;
		public const int TEXT	    	= 0x0200004;
		public const int DOUBLEDASH 	= 0x0200005;
		public const int CLOSING 	= 0x0200006;
	}

	class AspTokenizer
	{
#if NET_2_0
		const int CHECKSUM_BUF_SIZE = 8192;
#endif
		class PutBackItem
		{
			public readonly string Value;
			public readonly int Position;
			public readonly int CurrentToken;
			public readonly bool InTag;
			
			public PutBackItem (string value, int position, int currentToken, bool inTag)
			{
				Value = value;
				Position = position;
				CurrentToken = currentToken;
				InTag = inTag;
			}
		}
		
		static char [] lfcr = new char [] { '\n', '\r' };
		TextReader sr;
		int current_token;
		StringBuilder sb, odds;
		int col, line;
		int begcol, begline;
		int position;
		bool inTag;
		bool expectAttrValue;
		bool alternatingQuotes;
		bool hasPutBack;
		bool verbatim;
		bool have_value;
		bool have_unget;
		int unget_value;
		string val;
		Stack putBackBuffer;
#if NET_2_0
		MD5 checksum;
		char[] checksum_buf = new char [CHECKSUM_BUF_SIZE];
		int checksum_buf_pos = -1;
		
		public MD5 Checksum {
			get { return checksum; }
		}
#endif
		
		public AspTokenizer (TextReader reader)
		{
			this.sr = reader;
			sb = new StringBuilder ();
			odds= new StringBuilder();
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
			if (hasPutBack && !inTag)
				throw new HttpException ("put_back called twice!");
			
			hasPutBack = true;
			if (putBackBuffer == null)
				putBackBuffer = new Stack ();

			string val = Value;
			putBackBuffer.Push (new PutBackItem (val, position, current_token, inTag));
			position -= val.Length;
		}
		
		public int get_token ()
		{
			if (hasPutBack) {
				PutBackItem pbi;
				if (verbatim) {
					pbi = putBackBuffer.Pop () as PutBackItem;
					string value = pbi.Value;
					switch (value.Length) {
						case 0:
							// do nothing, CurrentToken will be used
							break;

						case 1:
							pbi = new PutBackItem (String.Empty, pbi.Position, (int)value [0], false);
							break;

						default:
							pbi = new PutBackItem (value, pbi.Position, (int)value [0], false);
							break;
					}		
				} else
					pbi = putBackBuffer.Pop () as PutBackItem;
				
				hasPutBack = putBackBuffer.Count > 0;
				position = pbi.Position;
				have_value = false;
				val = null;
				sb = new StringBuilder (pbi.Value);
				current_token = pbi.CurrentToken;
				inTag = pbi.InTag;
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

		void ungetc (int value)
		{
			have_unget = true;
			unget_value = value;

			// Only '/' passes through here now.
			// If we ever let \n here, update 'line'
			position--;
			col--;
		}

#if NET_2_0
		void TransformNextBlock (int count, bool final)
		{
			byte[] input = Encoding.UTF8.GetBytes (checksum_buf, 0, count);

			if (checksum == null)
				checksum = MD5.Create ();
			
			if (final)
				checksum.TransformFinalBlock (input, 0, input.Length);
			else
				checksum.TransformBlock (input, 0, input.Length, input, 0);
			input = null;
			
			checksum_buf_pos = -1;
		}
		
		void UpdateChecksum (int c)
		{
			bool final = c == -1;

			if (!final) {
				if (checksum_buf_pos + 1 >= CHECKSUM_BUF_SIZE)
					TransformNextBlock (checksum_buf_pos + 1, false);
				checksum_buf [++checksum_buf_pos] = (char)c;
			} else
				TransformNextBlock (checksum_buf_pos + 1, true);
		}
#endif
		int read_char ()
		{
			int c;
			if (have_unget) {
				c = unget_value;
				have_unget = false;
			} else {
				c = sr.Read ();
#if NET_2_0
				UpdateChecksum (c);
#endif
			}

			if (c == '\r' && sr.Peek () == '\n') {
				c = sr.Read ();
#if NET_2_0
				UpdateChecksum (c);
#endif
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
			alternatingQuotes = true;
			
			while ((c = sr.Peek ()) != -1) {
				if (c == '%' && last == '<') {
					inServerTag = true;
				} else if (inServerTag && c == '>' && last == '%') {
					inServerTag = false;
				} else if (!inServerTag) {
					if (!quoted && c == '/') {
						read_char ();
						c = sr.Peek ();
						if (c == -1) {
							c = '/';
						} else if (c == '>') {
							ungetc ('/');
							break;
						}
					} else if (!quoted && (c == '>' || Char.IsWhiteSpace ((char) c))) {
						break;
					} else if (quoted && c == quoteChar && last != '\\') {
						read_char ();
						break;
					}
				} else if (quoted && c == quoteChar) {
					alternatingQuotes = false;
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
			odds.Length=0;
			while ((c = read_char ()) != -1){
				if (verbatim){
					inTag = false;
					sb.Append  ((char) c);
					return c;
				}

				if (inTag && expectAttrValue && (c == '"' || c == '\''))
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
					if (odds.Length == 0 || odds.ToString ().IndexOfAny (lfcr) < 0) {
						sb.Append ((char) c);
						return c;
					}
					sb.Append ((char) c);
					continue;
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

				if (!Char.IsWhiteSpace ((char) c)) {
					sb.Append  ((char) c);
					return c;
				}
				// keep otherwise discarded characters in case we need.
				odds.Append((char) c);
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

		public string Odds {
			get {
				return odds.ToString();
			}
		}

		public bool InTag {
			get { return inTag; }
			set { inTag = value; }
		}

		// Hack for preventing confusion with VB comments (see bug #63451)
		public bool ExpectAttrValue {
			get { return expectAttrValue; }
			set { expectAttrValue = value; }
		}
		
		public bool AlternatingQuotes {
			get { return alternatingQuotes; }
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

