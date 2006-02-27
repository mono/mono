// ILTokenizer.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Globalization;

namespace Mono.ILASM {

        public delegate void NewTokenEvent (object sender, NewTokenEventArgs args);

        public class NewTokenEventArgs : EventArgs {

                public readonly ILToken Token;

                public NewTokenEventArgs (ILToken token)
                {
                        Token = token;
                }
        }

        /// <summary>
        /// </summary>
        public class ILTokenizer : ITokenStream {

                private static readonly string idchars = "_$@?.`";

                private static Hashtable keywords;
                private static Hashtable directives;

                private ILToken lastToken;
                private ILReader reader;
                private StringHelper strBuilder;
                private NumberHelper numBuilder;
                private bool in_byte_array;
                
                public event NewTokenEvent NewTokenEvent;

                static ILTokenizer()
                {
                        keywords = ILTables.Keywords;
                        directives = ILTables.Directives;
                }

                /// <summary>
                /// </summary>
                /// <param name="reader"></param>
                public ILTokenizer (StreamReader reader)
                {
                        this.reader = new ILReader (reader);
                        strBuilder = new StringHelper (this);
                        numBuilder = new NumberHelper (this);
                        lastToken = ILToken.Invalid.Clone () as ILToken;
                }

                public ILReader Reader {
                        get {
                                return reader;
                        }
                }

		public Location Location {
			get {
				return reader.Location;
			}
		}

                public bool InByteArray {
                        get { return in_byte_array; }
                        set { in_byte_array = value; }
                }

                public ILToken GetNextToken ()
                {
                        if (lastToken == ILToken.EOF) return ILToken.EOF;

                        int ch;
                        int next;
                        ILToken res = ILToken.EOF.Clone () as ILToken;

                        
                        while ((ch = reader.Read ()) != -1) {

                                // Comments
                                if (ch == '/') {
                                        next = reader.Peek ();
                                        if (next == '/') {
                                                // double-slash comment, skip to the end of the line.
                                                for (reader.Read ();
                                                        next != -1 && next != '\n';
                                                        next = reader.Read ());
                                                continue;
                                        } else if (next == '*') {
                                                reader.Read ();
                                                for (next = reader.Read (); next != -1; next = reader.Read ()) {
                                                        if (next == '*' && reader.Peek () == '/') {
                                                                reader.Read ();
                                                                goto end;
                                                        }
                                                }
                                        end:
                                                continue;
                                        }
                                }

                                // HEXBYTES are flagged by the parser otherwise it is
                                // impossible to figure them out
                                if (in_byte_array) {
                                        string hx = String.Empty;

                                        if (Char.IsWhiteSpace ((char) ch))
                                                continue;

                                        if (ch == ')') {
                                                res = ILToken.CloseParens;
                                                break;
                                        }

                                        if (!is_hex (ch))
                                                throw new ILTokenizingException (reader.Location, ((char) ch).ToString ());
                                        hx += (char) ch;
                                        if (is_hex (reader.Peek ()))
                                                hx += (char) reader.Read ();
                                        else if (!Char.IsWhiteSpace ((char) reader.Peek ()) && reader.Peek () != ')')
                                                throw new ILTokenizingException (reader.Location,
                                                                ((char) reader.Peek ()).ToString ());
                                        res.token = Token.HEXBYTE;
                                        res.val = Byte.Parse (hx, NumberStyles.HexNumber);

                                        while (Char.IsWhiteSpace ((char) reader.Peek ()))
                                                reader.Read ();
                                        break;
                                }
                                
                                // Ellipsis
                                if (ch == '.' && reader.Peek () == '.') {
                                        reader.MarkLocation ();
                                        int ch2 = reader.Read ();
                                        if (reader.Peek () == '.') {
                                                res = ILToken.Ellipsis;
                                                reader.Read ();
                                                break;
                                        }
                                        reader.Unread (ch2);
                                        reader.RestoreLocation ();
                                }

                                if (ch == '.' || ch == '#') {
                                        next = reader.Peek ();
                                        if (ch == '.' && Char.IsDigit((char) next)) {
                                                numBuilder.Start (ch);
                                                reader.Unread (ch);
                                                numBuilder.Build ();
                                                if (numBuilder.ResultToken != ILToken.Invalid) {
                                                        res.CopyFrom (numBuilder.ResultToken);
                                                        break;
                                                }
                                        } else {
                                                if (strBuilder.Start (next) && strBuilder.TokenId == Token.ID) {
                                                        reader.MarkLocation ();
                                                        string dirBody = strBuilder.Build ();
                                                        string dir = new string ((char) ch, 1) + dirBody;
                                                        if (IsDirective (dir)) {
                                                                res = ILTables.Directives [dir] as ILToken;
                                                        } else {
                                                                reader.Unread (dirBody.ToCharArray ());
                                                                reader.RestoreLocation ();
                                                                res = ILToken.Dot;
                                                        }
                                                } else {
                                                        res = ILToken.Dot;
                                                }
                                                break;
                                        }
                                }

                                // Numbers && Hexbytes
                                if (numBuilder.Start (ch)) {
                                        if ((ch == '-') && !(Char.IsDigit ((char) reader.Peek ()))) {
                                                res = ILToken.Dash;
                                                break;
                                        } else {
                                                reader.Unread (ch);
                                                numBuilder.Build ();
                                                if (numBuilder.ResultToken != ILToken.Invalid) {
                                                        res.CopyFrom (numBuilder.ResultToken);
                                                        break;
                                                }
                                        }
                                }

                                // Punctuation
                                ILToken punct = ILToken.GetPunctuation (ch);
                                if (punct != null) {
                                        if (punct == ILToken.Colon && reader.Peek () == ':') {
                                                reader.Read ();
                                                res = ILToken.DoubleColon;
                                        } else {
                                                res = punct;
                                        }
                                        break;
                                }

                                // ID | QSTRING | SQSTRING | INSTR_* | KEYWORD
                                if (strBuilder.Start (ch)) {
                                        reader.Unread (ch);
                                        string val = strBuilder.Build ();
                                        if (strBuilder.TokenId == Token.ID) {
                                                ILToken opcode;
                                                next = reader.Peek ();
                                                if (next == '.') {
                                                        reader.MarkLocation ();
                                                        reader.Read ();
                                                        next = reader.Peek ();
                                                        if (IsIdChar ((char) next)) {
                                                                string opTail = BuildId ();
                                                                string full_str = String.Format ("{0}.{1}", val, opTail);
                                                                opcode = InstrTable.GetToken (full_str);

                                                                if (opcode == null) {
                                                                        if (strBuilder.TokenId != Token.ID) {
                                                                                reader.Unread (opTail.ToCharArray ());
										reader.Unread ('.');
                                                                                reader.RestoreLocation ();
                                                                                res.val = val;
                                                                        } else {
                                                                                res.token = Token.COMP_NAME;
                                                                                res.val = full_str;
                                                                        }
                                                                        break;
                                                                } else {
                                                                        res = opcode;
                                                                        break;
                                                                }

                                                        } else if (Char.IsWhiteSpace ((char) next)) {
								// Handle 'tail.' and 'unaligned.'
								opcode = InstrTable.GetToken (val + ".");
								if (opcode != null) {
									res = opcode;
									break;
								}
								// Let the parser handle the dot
								reader.Unread ('.');
                                                        }
                                                }
                                                opcode = InstrTable.GetToken (val);
                                                if (opcode != null) {
                                                        res = opcode;
                                                        break;
                                                }
                                                if (IsKeyword (val)) {
                                                        res = ILTables.Keywords [val] as ILToken;
                                                        break;
                                                }
                                        }

                                        res.token = strBuilder.TokenId;
                                        res.val = val;
                                        break;
                                }
                        }

                        OnNewToken (res);
                        lastToken.CopyFrom (res);
                        return res;
                }


                /// <summary>
                /// </summary>
                public ILToken NextToken {
                        get {
                                return GetNextToken ();
                        }
                }


                /// <summary>
                /// </summary>
                public ILToken LastToken {
                        get {
                                return lastToken;
                        }
                }

                bool is_hex (int e)
                {
                        return (e >= '0' && e <= '9') || (e >= 'A' && e <= 'F') || (e >= 'a' && e <= 'f');
                }

                private static bool IsIdStartChar (char ch)
                {
                        return (Char.IsLetter (ch) || (idchars.IndexOf (ch) != -1));
                }


                private static bool IsIdChar (char ch)
                {
                        return (Char.IsLetterOrDigit (ch) || (idchars.IndexOf (ch) != -1));
                }

                /// <summary>
                /// </summary>
                /// <param name="name"></param>
                /// <returns></returns>
                public static bool IsOpcode (string name)
                {
                        return InstrTable.IsInstr (name);
                }


                /// <summary>
                /// </summary>
                /// <param name="name"></param>
                /// <returns></returns>
                public static bool IsDirective (string name)
                {
                        char ch = name [0];
                        bool res = (ch == '.' || ch == '#');

                        if (res) {
                                res = directives.Contains (name);
                        }

                        return res;
                }

                private string BuildId ()
                {
                        StringBuilder idsb = new StringBuilder ();
                        int ch, last;

                        last = -1;
                        while ((ch = reader.Read ()) != -1) {
                                if (IsIdChar ((char) ch) || ch == '.') {
                                        idsb.Append ((char) ch);
                                } else {
                                        reader.Unread (ch);
                                        // Never end an id on a DOT
                                        if (last == '.') {
                                                reader.Unread (last);
                                                idsb.Length -= 1;
                                        }        
                                        break;
                                }
                                last = ch;
                        }

                        return idsb.ToString ();
                }

                /// <summary>
                /// </summary>
                /// <param name="name"></param>
                /// <returns></returns>
                public static bool IsKeyword (string name)
                {
                        return keywords.Contains (name);
                }

                private void OnNewToken (ILToken token)
                {
                        if (NewTokenEvent != null)
                                NewTokenEvent (this, new NewTokenEventArgs (token));
                }

        }
}
