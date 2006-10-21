//
// Decompiler.cs: Port to C# of Mozilla's Rhino decompiler.

/*
 * The contents of this file are subject to the Netscape Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/NPL/
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * The Original Code is Rhino code, released
 * May 6, 1999.
 *
 * The Initial Developer of the Original Code is Netscape
 * Communications Corporation.  Portions created by Netscape are
 * Copyright (C) 1997-1999 Netscape Communications Corporation. All
 * Rights Reserved.
 *
 * Contributor(s):
 * Mike Ang
 * Igor Bukanov
 * Mike McCabe
 *
 * Alternatively, the contents of this file may be used under the
 * terms of the GNU Public License (the "GPL"), in which case the
 * provisions of the GPL are applicable instead of those above.
 * If you wish to allow use of your version of this file only
 * under the terms of the GPL and not to allow others to use your
 * version of this file under the NPL, indicate your decision by
 * deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL.  If you do not delete
 * the provisions above, a recipient may use your version of this
 * file under either the NPL or the GPL.
 */

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

//
//  The following class save decompilation information about the source.
//  Source information is returned from the parser as a String
//  associated with function nodes and with the toplevel script.  When
//  saved in the constant pool of a class, this string will be UTF-8
//  encoded, and token values will occupy a single byte.
//
//  Source is saved (mostly) as token numbers.  The tokens saved pretty
//  much correspond to the token stream of a 'canonical' representation
//  of the input program, as directed by the parser.  (There were a few
//  cases where tokens could have been left out where decompiler could
//  easily reconstruct them, but I left them in for clarity).  (I also
//  looked adding source collection to TokenStream instead, where I
//  could have limited the changes to a few lines in getToken... but
//  this wouldn't have saved any space in the resulting source
//  representation, and would have meant that I'd have to duplicate
//  parser logic in the decompiler to disambiguate situations where
//  newlines are important.)  The function decompile expands the
//  tokens back into their string representations, using simple
//  lookahead to correct spacing and indentation.
//
//  Assignments are saved as two-token pairs (Token.ASSIGN, op). Number tokens
//  are stored inline, as a NUMBER token, a character representing the type, and
//  either 1 or 4 characters representing the bit-encoding of the number.  String
//  types NAME, STRING and OBJECT are currently stored as a token type,
//  followed by a character giving the length of the string (assumed to
//  be less than 2^16), followed by the characters of the string
//  inlined into the source string.  Changing this to some reference to
//  to the string in the compiled class' constant pool would probably
//  save a lot of space... but would require some method of deriving
//  the final constant pool entry from information available at parse
//  time.
//
// Author:
//	Cesar Lopez Nataren (cnataren@novell.com)
// 

using System;
using System.Text;

namespace Microsoft.JScript {

internal class Decompiler {
	//
	// Flag to indicate that the decompilation should omit the
	// function header and trailing brace.
	//
	internal const int ONLY_BODY_FLAG = 1 << 0;

	//
	// Flag to indicate that the decompilation generates toSource result.
	//
	internal const int TO_SOURCE_FLAG = 1 << 1;

	//
	// Decompilation property to specify initial ident value.
	//
	internal const int INITIAL_INDENT_PROP = 0;

	//
	// Decompilation property to specify default identation offset.
	//
	internal const int INDENT_GAP_PROP = 4;

	//
	// Decompilation property to specify identation offset for case labels.
	//
	internal const int CASE_GAP_PROP = 3;

	// Marker to denote the last RC of function so it can be distinguished from
	// the last RC of object literals in case of function expressions
	private static readonly int FUNCTION_END = Token.LAST_TOKEN + 1;

        private char [] sourceBuffer = new char [128];

	// Per script/function source buffer top: parent source does not include a
	// nested functions source and uses function index as a reference instead.
	private int sourceTop;

	// whether to do a debug print of the source information, when decompiling.
	private const bool printSource = false;

	internal string GetEncodedSource ()
	{
		return SourceToString (0);
	}

	internal int GetCurrentOffset ()
	{
		return sourceTop;
	}

	internal int MarkFunctionStart (int functionType)
	{
		int savedOffset = GetCurrentOffset ();
		AddToken (Token.FUNCTION);
		Append ((char) functionType);
		return savedOffset;
	}

	internal int MarkFunctionEnd (int functionStart)
	{
		int offset = GetCurrentOffset ();
		Append ((char) FUNCTION_END);
		return offset;
	}

	internal void AddToken (int token)
	{
		if (!(0 <= token && token <= Token.LAST_TOKEN))
			throw new Exception ("Illegal argument");

		Append ((char) token);
	}

	internal void AddEOL (int token)
	{
		if (!(0 <= token && token <= Token.LAST_TOKEN))
			throw new Exception ("Illegal argument");

		if (sourceTop > 0 && sourceBuffer [sourceTop - 1] != Token.SEMI)
			Append ((char) token);
		Append ((char) Token.EOL);
	}

	internal void AddName (string str)
	{
		AddToken (Token.NAME);
		AppendString (str);
	}

	internal void AddString (string str)
	{
		AddToken (Token.STRING);
		AppendString (str);
	}

	internal void AddRegexp (string regexp, string flags)
	{
		AddToken (Token.REGEXP);
		AppendString ('/' + regexp + '/' + flags);
	}

	internal void AddAssignOp (int op)
	{
		if (!(0 <= op && op <= Token.LAST_TOKEN))
			throw new Exception ("Illegal argument");

		Append ((char) Token.ASSIGNOP);
		Append ((char) op);
	}

	internal void AddNumber (double n)
	{
		AddToken (Token.NUMBER);

		// encode the number in the source stream.
		// Save as NUMBER type (char | char char char char)
		// where type is
		// 'D' - double, 'S' - short, 'J' - long.
		//
		// We need to retain float vs. integer type info to keep the
		// behavior of liveconnect type-guessing the same after
		// decompilation.  (Liveconnect tries to present 1.0 to Java
		// as a float/double)
		// OPT: This is no longer true. We could compress the format.

		// This may not be the most space-efficient encoding;
		// the chars created below may take up to 3 bytes in
		// constant pool UTF-8 encoding, so a Double could take
		// up to 12 bytes.

		long lbits = (long) n;
		if (lbits != n) {
			// if it's floating point, save as a Double bit pattern.
			// (12/15/97 our scanner only returns Double for f.p.)
			lbits = BitConverter.DoubleToInt64Bits (n);
			
			Append ('D');
			Append ((char) (lbits >> 48));
			Append ((char) (lbits >> 32));
			Append ((char) (lbits >> 16));
			Append ((char) lbits);
		} else {
			// we can ignore negative values, bc they're already prefixed
			// by NEG

#if false
			if (lbits < 0)
				;
#endif
               
			// will it fit in a char?
			// this gives a short encoding for integer values up to 2^16.
			if (lbits <= Char.MaxValue) {
				Append ('S');
				Append ((char) lbits);
			} else { // Integral, but won't fit in a char. Store as a long.
				Append ('J');
				Append ((char) (lbits >> 48));
				Append ((char) (lbits >> 32));
				Append ((char) (lbits >> 16));
				Append ((char) lbits);
			}
		}
	}

	private void AppendString (string str)
	{
		int L = str.Length;
		int lengthEncodingSize = 1;
		if (L >= 0x8000)
			lengthEncodingSize = 2;

		int nextTop = sourceTop + lengthEncodingSize + L;
		if (nextTop > sourceBuffer.Length)
			IncreaseSourceCapacity (nextTop);

		if (L >= 0x8000) {
			// Use 2 chars to encode strings exceeding 32K, were the highest
			// bit in the first char indicates presence of the next byte
			sourceBuffer [sourceTop] = (char) (0x8000 | BitwiseBinary.UnsignedRightShift (L, 16));
			++sourceTop;
		}
		sourceBuffer [sourceTop] = (char) L;
		++sourceTop;
		str.CopyTo (0, sourceBuffer, sourceTop, L);
		sourceTop = nextTop;
	}

	private void Append (char c)
	{
		if (sourceTop == sourceBuffer.Length)
			IncreaseSourceCapacity (sourceTop + 1);

		sourceBuffer [sourceTop] = c;
		++sourceTop;
	}

	private void IncreaseSourceCapacity (int minimalCapacity)
	{
		// Call this only when capacity increase is must
		if (minimalCapacity <= sourceBuffer.Length)
			Console.WriteLine ("Warning: capacity increase is must");

		int newCapacity = sourceBuffer.Length * 2;
		if (newCapacity < minimalCapacity)
			newCapacity = minimalCapacity;

		char [] tmp = new char [newCapacity];
		Array.Copy (sourceBuffer, 0, tmp, 0, sourceTop);
		sourceBuffer = tmp;
	}

	internal string SourceToString (int offset)
	{
		if (offset < 0 || sourceTop < offset)
			throw new Exception ("SourceToString: offset < 0 || sourceTop < offset");
		return new string (sourceBuffer, offset, sourceTop - offset);
	}

	///
	/// <remarks>
	/// Decompile the source information associated with this js
	/// function/script back into a string.  For the most part, this
	/// just means translating tokens back to their string
	/// representations; there's a little bit of lookahead logic to
	/// decide the proper spacing/indentation.  Most of the work in
	/// mapping the original source to the prettyprinted decompiled
	/// version is done by the parser.
	/// </remarks>
	///
	/// <param name="source"> encoded source tree presentation </param>
	///
	/// <param name="flags"> flags to select output format </param>
	///
	///

	internal static string Decompile (string source, int flags)
	{
		return Decompile (source, flags, INITIAL_INDENT_PROP);
	}

	internal static string Decompile (string source, int flags, int indentCount)
	{
		int length = source.Length;

		if (length == 0)
			return "";

		int indent = indentCount * INDENT_GAP_PROP;
		if (indent < 0)
			throw new Exception ("Illegal argument");

		int indentGap = INDENT_GAP_PROP;

		if (indentGap < 0)
			throw new Exception ("Illegal argument");

		int caseGap = CASE_GAP_PROP;
		if (caseGap < 0)
			throw new Exception ("Illegal argument");

		StringBuilder result = new StringBuilder ();
		bool justFunctionBody = (0 != (flags & Decompiler.ONLY_BODY_FLAG));
		bool toSource = (0 != (flags & Decompiler.TO_SOURCE_FLAG));

		// Spew tokens in source, for debugging.
		// as TYPE number char
		if (printSource) {
			Console.WriteLine ("length:" + length);
			for (int j = 0; j < length; ++j) {
				// Note that tokenToName will fail unless Context.printTrees
				// is true.
				string tokenname = null;
				if (Token.PrintNames)
					tokenname = Token.Name (source [j], true);

			        if (tokenname == null)
					tokenname = "---";

				string pad = tokenname.Length > 7
					? "\t"
					: "\t\t";
				Console.WriteLine (tokenname
					 + pad + (int) source [j]
				         + "\t'" + EscapeString (source.Substring (j, 1))
					 + "'");
			}
		        Console.WriteLine ();
		}

		int braceNesting = 0;
		bool afterFirstEOL = false;
		int i = 0;
		int topFunctionType;
		if (source [i] == Token.SCRIPT) {
			++i;
			topFunctionType = -1;
		} else {
			topFunctionType = source [i + 1];
		}

		if (!toSource) {
			// add an initial newline to exactly match js.
			result.Append ('\n');
			for (int j = 0; j < indent; j++)
				result.Append (' ');
		} else {
			if (topFunctionType == (int) FunctionType.Expression) {
				result.Append ('(');
			}
		}

		int t;

		while (i < length) {
			t = source [i];

			if (t == Token.NAME || t == Token.REGEXP) {
				// re-wrapped in '/'s in parser...
				i = PrintSourceString (source, i + 1, false, result);
				continue;
			}  else if (t == Token.STRING) {
				i = PrintSourceString (source, i + 1, true, result);
				continue;
			} else if (t == Token.NUMBER) {
				i = PrintSourceNumber (source, i + 1, result);
				continue;
			} else if (t == Token.TRUE)
				result.Append ("true");
			else if (t == Token.FALSE)
				result.Append ("false");
			else if (t == Token.NULL)
				result.Append ("null");
			else if (t == Token.THIS)
				result.Append ("this");
			else if (t == Token.FUNCTION) {
				++i; // skip function type
				result.Append ("function ");
			} else if (t == FUNCTION_END) {
				// Do nothing
			} else if (t == Token.COMMA)
				result.Append (", ");
			else if (t == Token.LC) {
				++braceNesting;
				if (Token.EOL == GetNext (source, length, i))
					indent += indentGap;
				result.Append ('{');
			} else if (t == Token.RC) {
				--braceNesting;
				/* don't print the closing RC if it closes the
				 * toplevel function and we're called from
				 * decompileFunctionBody.
				 */
				if (!(justFunctionBody && braceNesting == 0)) {
					result.Append ('}');
					int tt = GetNext (source, length, i);
					if (tt == Token.EOL || tt == FUNCTION_END)
						indent -= indentGap;
					else if (tt == Token.WHILE || tt == Token.ELSE) {
						indent -= indentGap;
						result.Append (' ');
					}
				}
			} else if (t == Token.LP)
				result.Append ('(');
			else if (t == Token.RP) {
				result.Append (')');
				if (Token.LC == GetNext (source, length, i))
					result.Append (' ');
			} else if (t == Token.LB)
				result.Append ('[');
			else if (t == Token.RB)
				result.Append (']');
			else if (t == Token.EOL) {
				if (!toSource) {
					bool newLine = true;
					if (!afterFirstEOL) {
						afterFirstEOL = true;
						if (justFunctionBody) {
							/* throw away just added 'function name(...) {'
							 * and restore the original indent
							 */
							result.Length = 0;
							indent -= indentGap;
							newLine = false;
						}
					}
					if (newLine)
						result.Append ('\n');

					/* add indent if any tokens remain,
					 * less setback if next token is
					 * a label, case or default.
					 */
					if (i + 1 < length) {
						int less = 0;
						int nextToken = source [i + 1];
						if (nextToken == Token.CASE
							|| nextToken == Token.DEFAULT) {
							less = indentGap - caseGap;
						} else if (nextToken == Token.RC) {
							less = indentGap;
						}

					/* elaborate check against label... skip past a
					 * following inlined NAME and look for a COLON.
					 */
					else if (nextToken == Token.NAME) {
							int afterName = GetSourceStringEnd (source, i + 2);
							if (source [afterName] == Token.COLON)
								less = indentGap;
						}

						for (; less < indent; less++)
							result.Append (' ');
					}
				}
			} else if (t == Token.DOT)
				result.Append ('.');
			else if (t == Token.NEW)
				result.Append ("new ");
			else if (t == Token.DELPROP)
				result.Append ("delete ");
			else if (t == Token.IF)
				result.Append ("if ");
			else if (t == Token.ELSE)
				result.Append ("else ");
			else if (t == Token.FOR)
				result.Append ("for ");
			else if (t == Token.IN)
				result.Append (" in ");
			else if (t == Token.WITH)
				result.Append ("with ");
			else if (t == Token.WHILE)
				result.Append ("while ");
			else if (t == Token.DO)
				result.Append ("do ");
			else if (t == Token.TRY)
				result.Append ("try ");
			else if (t == Token.CATCH)
				result.Append ("catch ");
			else if (t == Token.FINALLY)
				result.Append ("finally ");
			else if (t == Token.THROW)
				result.Append ("throw ");
			else if (t == Token.SWITCH)
				result.Append ("switch ");
			else if (t == Token.BREAK) {
				result.Append ("break");
				if (Token.NAME == GetNext (source, length, i))
					result.Append (' ');
			} else if (t == Token.CONTINUE) {
				result.Append ("continue");
				if (Token.NAME == GetNext (source, length, i))
					result.Append (' ');
			} else if (t == Token.CASE)
				result.Append ("case ");
			else if (t == Token.DEFAULT)
				result.Append ("default");
			else if (t == Token.RETURN) {
				result.Append ("return");
				if (Token.SEMI != GetNext (source, length, i))
					result.Append (' ');
			} else if (t == Token.VAR)
				result.Append ("var ");
			else if (t == Token.SEMI) {
				result.Append (';');
				if (Token.EOL != GetNext (source, length, i)) {
					// separators in FOR
					result.Append (' ');
				}
			} else if (t == Token.ASSIGN)
				result.Append (" = ");
			else if (t == Token.ASSIGNOP) {
				i++;
				int op = source [i];
				
				if (op == Token.ADD)
					result.Append (" += ");
				else if (op == Token.SUB)
					result.Append (" -= ");
				else if (op == Token.MUL)
					result.Append (" *= ");
				else if (op == Token.DIV)
					result.Append (" /= ");
				else if (op == Token.MOD)
					result.Append (" %= ");
				else if (op == Token.BITOR)
					result.Append (" |= ");
				else if (op == Token.BITXOR)
					result.Append (" ^= ");
				else if (op == Token.BITAND)
					result.Append (" &= ");
				else if (op == Token.LSH)
					result.Append (" <<= ");
				else if (op == Token.RSH)
					result.Append (" >>= ");
				else if (op == Token.URSH)
					result.Append (" >>>= ");
			} else if (t == Token.HOOK)
				result.Append (" ? ");
			else if (t == Token.OBJECTLIT) {
				// pun OBJECTLIT to mean colon in objlit property
				// initialization.
				// This needs to be distinct from COLON in the general case
				// to distinguish from the colon in a ternary... which needs
				// different spacing.
				result.Append (':');
			} else if (t == Token.COLON) {
				if (Token.EOL == GetNext(source, length, i))
					// it's the end of a label
					result.Append (':');
				else
					// it's the middle part of a ternary
					result.Append (" : ");
			} else if (t == Token.OR)
				result.Append (" || ");
			else if (t == Token.AND)
				result.Append (" && ");
			else if (t == Token.BITOR)
				result.Append (" | ");
			else if (t == Token.BITXOR)
				result.Append (" ^ ");
			else if (t == Token.BITAND)
				result.Append (" & ");
			else if (t == Token.SHEQ)
				result.Append (" === ");
			else if (t == Token.SHNE)
				result.Append (" !== ");
			else if (t == Token.EQ)
				result.Append (" == ");
			else if (t == Token.NE)
				result.Append (" != ");
			else if (t == Token.LE)
				result.Append (" <= ");
			else if (t == Token.LT)
				result.Append (" < ");
			else if (t == Token.GE)
				result.Append (" >= ");
			else if (t == Token.GT)
				result.Append (" > ");
			else if (t == Token.INSTANCEOF)
				result.Append (" instanceof ");
			else if (t == Token.LSH)
				result.Append (" << ");
			else if (t == Token.RSH)
				result.Append (" >> ");
			else if (t == Token.URSH)
				result.Append (" >>> ");
			else if (t == Token.TYPEOF)
				result.Append ("typeof ");
			else if (t == Token.VOID)
				result.Append ("void ");
			else if (t == Token.NOT)
				result.Append ('!');
			else if (t == Token.BITNOT)
				result.Append ('~');
			else if (t == Token.POS)
				result.Append ('+');
			else if (t == Token.NEG)
				result.Append ('-');
			else if (t == Token.INC)
				result.Append ("++");
			else if (t == Token.DEC)
				result.Append ("--");
			else if (t == Token.ADD)
				result.Append (" + ");
			else if (t == Token.SUB)
				result.Append (" - ");
			else if (t == Token.MUL)
				result.Append (" * ");
			else if (t == Token.DIV)
				result.Append (" / ");
			else if (t == Token.MOD)
				result.Append (" % ");
			else {
				// If we don't know how to decompile it, raise an exception.
				throw new Exception ("If we don't know how to decompile it, raise an exception.");
			}
			++i;
		}

		if (!toSource) {
			// add that trailing newline if it's an outermost function.
			if (!justFunctionBody)
				result.Append ('\n');
		} else {
			if (topFunctionType == (int) FunctionType.Expression)
				result.Append (')');
		}
		return result.ToString ();
	}

	private static int GetNext (string source, int length, int i)
	{
		return (i + 1 < length) ? source [i + 1] : Token.EOF;
	}

	private static int GetSourceStringEnd (string source, int offset)
	{
		return PrintSourceString (source, offset, false, null);
	}

	private static int PrintSourceString (string source, int offset, bool asQuotedString, StringBuilder sb)
	{
		int length = source [offset];
		++offset;
		if ((0x8000 & length) != 0) {
			length = ((0x7FFF & length) << 16) | source [offset];
			++offset;
		}
		if (sb != null) {
			string str = source.Substring (offset, length);
			if (!asQuotedString) {
				sb.Append (str);
			} else {
				sb.Append ('"');
				sb.Append (EscapeString (str));
				sb.Append ('"');
			}
		}
		return offset + length;
	}

	private static int PrintSourceNumber (string source, int offset, StringBuilder sb)
	{
		double number = 0.0;
		char type = source [offset];
		++offset;
		if (type == 'S') {
			if (sb != null) {
				int ival = source [offset];
				number = ival;
			}
			++offset;
		} else if (type == 'J' || type == 'D') {
			if (sb != null) {
				long lbits;
				lbits = (long)source [offset] << 48;
				lbits |= (long)source [offset + 1] << 32;
				lbits |= (long)source [offset + 2] << 16;
				lbits |= (long)source [offset + 3];
				if (type == 'J') {
					number = lbits;
				} else {
					number = BitConverter.Int64BitsToDouble (lbits);
				}
			}
			offset += 4;
		} else {
			// Bad source
			throw new Exception("Runtime exception");
		}
		if (sb != null) {
			string str = Convert.ToString (number);
			sb.Append (str);
			bool forceDouble = offset < source.Length && source [offset] == Token.DOT;
			if (forceDouble && str.IndexOf ('.') == -1)
				sb.Append (".0");
		}
		return offset;
	}

	internal static string EscapeString (string s)
	{
		return EscapeString (s, '"');
	}

	/**
	 * For escaping strings printed by object and array literals; not quite
	 * the same as 'escape.'
	 */
	internal static string EscapeString (string s, char escapeQuote)
	{
		if (!(escapeQuote == '"' || escapeQuote == '\''))
			throw new Exception ("Debug");

		StringBuilder sb = null;

		for(int i = 0, L = s.Length; i != L; ++i) {
			int c = s [i];

			if (' ' <= c && c <= '~' && c != escapeQuote && c != '\\') {
				// an ordinary print character (like C isprint()) and not "
				// or \ .
				if (sb != null)
					sb.Append ((char) c);

				continue;
			}
			if (sb == null) {
				sb = new StringBuilder (L + 3);
				sb.Append (s);
				sb.Length = i;
			}

			int escape = -1;
			switch (c) {
			case '\b':  escape = 'b';  break;
			case '\f':  escape = 'f';  break;
			case '\n':  escape = 'n';  break;
			case '\r':  escape = 'r';  break;
			case '\t':  escape = 't';  break;
			case 0xb:   escape = 'v';  break; // Java lacks \v.
			case ' ':   escape = ' ';  break;
			case '\\':  escape = '\\'; break;
			}
			if (escape >= 0) {
				// an \escaped sort of character
				sb.Append ('\\');
				sb.Append ((char) escape);
			} else if (c == escapeQuote) {
				sb.Append ('\\');
				sb.Append (escapeQuote);
			} else {
				int hexSize;
				if (c < 256) {
					// 2-digit hex
					sb.Append ("\\x");
					hexSize = 2;
				} else {
					// Unicode.
					sb.Append ("\\u");
					hexSize = 4;
				}
				// append hexadecimal form of c left-padded with 0
				for (int shift = (hexSize - 1) * 4; shift >= 0; shift -= 4) {
					int digit = 0xf & (c >> shift);
					int hc = (digit < 10) ? '0' + digit : 'a' - 10 + digit;
					sb.Append ((char) hc);
				}
			}
		}
		return (sb == null) ? s : sb.ToString ();
	}
}
}
