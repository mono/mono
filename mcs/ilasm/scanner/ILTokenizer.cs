// ILTokenizer.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.IO;
using System.Collections;

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

		private static Hashtable opcodes;
		private static Hashtable keywords;
		private static Hashtable directives;

		private ILToken lastToken;
		private ILReader reader;
		private StringHelper strBuilder;
		private NumberHelper numBuilder;

		public event NewTokenEvent NewTokenEvent;

		static ILTokenizer()
		{
			opcodes = ILTables.Opcodes;
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



		/// <summary>
		/// </summary>
		public ILReader Reader {
			get {
				return reader;
			}
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public ILToken GetNextToken ()
		{
			if (lastToken == ILToken.EOF) return ILToken.EOF;

			int ch;
			int next;
			ILToken res = ILToken.EOF.Clone () as ILToken;

			while ((ch = reader.Read ()) != -1) {

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
						reader.MarkLocation ();
						if (strBuilder.Start (next) && strBuilder.TokenId == Token.ID) {
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
						for (reader.Read ();
						     next != -1 && next != '*' && reader.Peek () != '/';
						     next = reader.Read ());
						reader.Read ();
						continue;
					}
				}


				// Numbers
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
						next = reader.Peek ();
						if (next == '.') {
							reader.MarkLocation ();
							ch = reader.Read ();
							next = reader.Peek ();
							if (Char.IsLetterOrDigit ((char) next)) {
								string opTail = reader.ReadToWhitespace ();
								
								if (!IsOpcode (val + "." + opTail)) {
									reader.Unread (opTail.ToCharArray ());
									reader.RestoreLocation ();
									reader.Unread (ch);
								}
							}
						}
						if (IsOpcode (val)) {
							res = InstrTable.GetToken (val);
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
