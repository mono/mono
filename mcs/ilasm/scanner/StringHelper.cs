// StringHelper.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.Text;

namespace Mono.ILASM {

	/// <summary>
	/// </summary>
	internal class StringHelper : StringHelperBase {

		private static readonly string startIdChars = "#$@_";
		private static readonly string idChars = "_$@?`";

		/// <summary>
		/// </summary>
		/// <param name="host"></param>
		public StringHelper (ILTokenizer host) : base (host)
		{
		}


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override bool Start (char ch)
		{
			mode = Token.UNKNOWN;

			if (Char.IsLetter (ch) || startIdChars.IndexOf (ch) != -1) {
				mode = Token.ID;
			} else if (ch == '\'') {
				mode = Token.SQSTRING;
			} else if (ch == '"') {
				mode = Token.QSTRING;
			}

			return (mode != Token.UNKNOWN);
		}


		private static bool IsIdChar (int c)
		{
			char ch = (char) c;
			return (Char.IsLetterOrDigit(ch) || idChars.IndexOf (ch) != -1);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string Build ()
		{
			if (mode == Token.UNKNOWN) return String.Empty;
			int ch = 0;

			ILReader reader = host.Reader;

			StringBuilder idsb = new StringBuilder ();
			if (mode == Token.SQSTRING || mode == Token.QSTRING) {
				int term = (mode == Token.SQSTRING) ? '\'' : '"';
				reader.Read (); // skip quote
				for (ch = reader.Read (); ch != -1; ch = reader.Read ()) {
					if (ch == term) {
						break;
					}

					if (ch == '\\') {
						ch = reader.Read ();

						/*
						 * Long string can be broken across multiple lines
						 * by using '\' as the last char in line.
						 * Any white space chars between '\' and the first
						 * char on the next line are ignored.
						 */
						if (ch == '\n') {
							reader.SkipWhitespace ();
							continue;
						}

						int escaped = Escape (reader, ch);
						if (escaped == -1) {
                                                        reader.Unread (ch);
                                                        ch = '\\';
                                                } else {
                                                        ch = escaped;
						}
					}

					idsb.Append((char)ch);
				}
			} else { // ID
				while ((ch = reader.Read ()) != -1) {
					if (IsIdChar (ch)) {
						idsb.Append ((char) ch);
					} else {
						reader.Unread (ch);
						break;
					}
				}
			}
			return idsb.ToString ();
		}




		/// <summary>
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		public static int Escape (ILReader reader, int ch)
		{
			int res = -1;

			if (ch >= '0' && ch <='7') {
				StringBuilder octal = new StringBuilder ();
				octal.Append ((char)ch);
				int possibleOctalChar = reader.Peek ();
				if (possibleOctalChar >= '0' && possibleOctalChar <='7') {
					octal.Append ((char)reader.Read ());
					possibleOctalChar = reader.Peek ();
					if (possibleOctalChar >= '0' && possibleOctalChar <='7')
						octal.Append ((char)reader.Read ());
				}
				res = Convert.ToInt32(octal.ToString (), 8);
			} else {
				int id = "abfnrtv\"'\\".IndexOf ((char)ch);
				if (id != -1) {
					res = "\a\b\f\n\r\t\v\"'\\" [id];
				}
			}

			return res;
		}

	}


}

