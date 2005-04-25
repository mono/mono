//
// LdmlReader.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Parses LDML basic text files.
//
// TODO: store parsed LDML data into some kind of structures.
//

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Mono.Globalization.Unicode
{
	public class LdmlException : ApplicationException
	{
		public LdmlException (string msg)
			: base (msg)
		{
		}

		public LdmlException (string msg, int line, int column)
			: base (String.Format ("{0} ({1}, {2})", msg, line, column))
		{
		}
	}

	public class LdmlConst
	{
		public const int Reset = -1;
		public const int Secondary = -2;
		public const int Thirtiary = -3;
		public const int Quarternary = -4;
		public const int Identical = -5;
		public const int Before1 = -6;
		public const int Before2 = -7;
		public const int Before3 = -8;
		public const int LastSecondaryIgnorable = -9;
		public const int LastRegular = -10;
		public const int ThreeDots = -11;
	}

	public class LdmlReader
	{
		Ldml ldml = new Ldml ();
		TextReader reader;
		int line = 1;
		int column;
		char [] nameBuffer = new char [128];

		public static void Main (string [] args)
		{
			string dirname = null;

			foreach (string arg in args) {
				// handle options here
				dirname = arg;
			}
			if (dirname == null) {
				Console.WriteLine ("pass directory name for collation *.txt files");
				return;
			}

			foreach (FileInfo fi in new DirectoryInfo (dirname).GetFiles ("*.txt"))
				if (fi.Name != "root.txt")
					new LdmlReader (fi.FullName).ReadCulture ();
		}

		public LdmlReader (string filename)
		{
			Console.WriteLine ("Filename: " + filename);
			reader = new StreamReader (filename, Encoding.UTF8);
		}

		Exception Error (string msg)
		{
			throw new LdmlException (msg, line, column);
		}

		int PeekChar ()
		{
			return reader.Peek ();
		}

		int ReadChar ()
		{
			int i = reader.Read ();
			if (i >= 0) {
				if (i == '\n') {
					line++;
					column = 0;
				}
				column++;
			}
			return i;
		}

		string ReadLine ()
		{
			line++;
			return reader.ReadLine ();
		}

		void Expect (char c)
		{
			int ch;
			if (c != (ch = ReadChar ()))
				throw Error (String.Format ("Expected '{0}' (#x{1:x}) but was '{2}' (#x{3:x}).", c, (int) c, (char) ch, ch));
		}

		void Expect (string s)
		{
			foreach (char c in s)
				Expect (c);
		}

		void SkipIgnorables ()
		{
			SkipComments ();
			SkipWhitespace ();
		}

		void SkipWhitespace ()
		{
			int ch;
			while ((ch = PeekChar ()) >= 0 && Char.IsWhiteSpace ((char) ch))
				ReadChar ();
		}

		string ReadName ()
		{
			SkipIgnorables ();
			int ch;
			if ((ch = PeekChar ()) < 0 || !IsFirstNameChar ((char) ch))
				throw Error ("Name was expected");
			int i = 0;
			for (; (ch = PeekChar ()) > 0 && (ch == '_' || IsNameChar ((char) ch)); i++)
					nameBuffer [i] = (char) ReadChar ();
			if (i == nameBuffer.Length)
				throw Error ("Name too long.");
			return new string (nameBuffer, 0, i);
		}

		void ReadCulture ()
		{
			SkipComments ();
			SkipIgnorables ();
			ldml.Language = ReadName ();
Console.WriteLine (" Culture name is " + ldml.Language);
			SkipIgnorables ();
			Expect ('{');
			SkipIgnorables ();
			while (PeekChar () != '}')
				ReadCultureContent ();
			ReadChar ();
			// successfully read.
		}

		void ReadCultureContent ()
		{
			string name = ReadName ();
			switch (name) {
			case "collations":
				ReadCollations ();
				break;
			case "Version":
			case "___":
				SkipSection ();
				break;
			default:
Console.WriteLine ("** " + name);
				SkipSection ();
Console.WriteLine ("   ... done");
				break;
			}
			SkipIgnorables ();
		}

		void ReadCollations ()
		{
			if (PeekChar () == ':') {
				ReadChar ();
				Expect ("alias");
				SkipWhitespace ();
				Expect ('{');
				Expect ('"');
				SkipWhitespace ();
				ldml.Alias = ReadName ();
				while (PeekChar () != '"' && PeekChar () >= 0)
					ReadChar ();
				Expect ('"');
				Expect ('}');
				return;
			}
			Expect ('{');
			SkipIgnorables ();
			while (PeekChar () != '}') {
				string name = ReadName ();
				switch (name) {
				case "standard":
					Expect ('{');
					ReadStandardCollation ();
					Expect ('}');
					break;
				case "big5han":
					Expect ('{');
					ReadBig5HanCollation ();
					Expect ('}');
					break;
				default:
Console.WriteLine ("****** " + name);
					SkipSection ();
Console.WriteLine ("    ... done.");
					break;
				}
				SkipWhitespace ();
			}
			SkipIgnorables ();
			Expect ('}');
		}

		void ReadStandardCollation ()
		{
			SkipIgnorables ();
			int ch;
			while ((ch = PeekChar ()) >= 0 && IsNameChar ((char) ch)) {
				string name = ReadName ();
				switch (name) {
				case "Sequence":
					ReadSequence ();
					break;
				default:
Console.WriteLine ("**** " + name);
					SkipSection ();
Console.WriteLine ("    ... done.");
					break;
				}
				SkipWhitespace ();
			}
		}

		void ReadBig5HanCollation ()
		{
			SkipIgnorables ();
			int ch;
			while ((ch = PeekChar ()) >= 0 && IsNameChar ((char) ch)) {
				string name = ReadName ();
				switch (name) {
				case "Sequence":
//					ReadBig5Sequence ();
					ReadSequence ();
					break;
				default:
Console.WriteLine ("******** " + name);
					SkipSection ();
Console.WriteLine ("    ... done.");
					break;
				}
				SkipWhitespace ();
			}
		}

		bool IsFirstNameChar (char c)
		{
			return Char.IsLetter (c) || c == '_' || c == '-';
		}

		bool IsNameChar (char c)
		{
			return IsFirstNameChar (c) || Char.IsDigit (c);
		}

		void AddValue (int v)
		{
			ldml.AddValue (v);
		}

		void ReadSequence ()
		{
			SkipIgnorables ();
			Expect ('{');
			bool loop = true;
			bool quoted = false;
			while (loop) {
				SkipIgnorables ();
				if (PeekChar () < 0)
					throw Error ("Unterminated sequence section.");
				if (PeekChar () == '}')
					break;
				if (!quoted) {
					Expect ('"');
					quoted = true;
				}
				SkipIgnorables ();
				switch ((char) PeekChar ()) {
				case '"':
					ReadChar ();
					quoted = !quoted;
					break;
				case '&':
					ReadChar ();
					if (PeekChar () != '[') {
						AddValue (LdmlConst.Reset); // reset
						break;
					}
					ReadChar ();
					SkipIgnorables ();
					string name = ReadName ();
					switch (name) {
					case "before":
						SkipIgnorables ();
						int x = ReadChar ();
						switch (x) {
						case '1':
							AddValue (LdmlConst.Before1);
							break;
						case '2':
							AddValue (LdmlConst.Before2);
							break;
						case '3':
							AddValue (LdmlConst.Before3);
							break;
						default:
							throw Error ("Not supported before : " + (char) x);
						}
						break;
					case "last":
						SkipWhitespace ();
						name = ReadName ();
						switch (name) {
						case "secondary":
							SkipWhitespace ();
							Expect ("ignorable");
							AddValue (LdmlConst.LastSecondaryIgnorable);
							break;
						case "regular":
							AddValue (LdmlConst.LastRegular);
							break;
						default:
							throw Error ("Not supported last : " + name);
						}
						SkipWhitespace ();
						break;
					default:
						throw Error ("To be supported: " + name);
					}
					Expect (']');
					break;
				case '<':
					int i = 0;
					for (; PeekChar () == '<'; i++)
						ReadChar ();
					switch (i) {
					case 2:
						AddValue (LdmlConst.Secondary);
						break;
					case 3:
						AddValue (LdmlConst.Thirtiary);
						break;
					case 4:
						AddValue (LdmlConst.Quarternary);
						break;
					}
					break;
				case '=':
					AddValue (LdmlConst.Identical);
					ReadChar ();
					break;
				case '\'':
					ReadChar ();
					if (PeekChar () == '\'') {
						ReadChar ();
						AddValue ('\'');
						break;
					}
					// '\uXXXX'
					else if (PeekChar () == '\\') {
						ReadChar ();
						if (ReadChar () == 'u') {
							int v = 0;
							while (true) {
								int ch = PeekChar ();
								if ('0' <= ch && ch <= '9')
									v = v * 16 + ch - '0';
								else if ('A' <= ch && ch <= 'F')
									v = v * 16 + ch - 'A';
								else if ('a' <= ch && ch <= 'f')
									v = v * 16 + ch - 'a';
								else
									break;
								ReadChar ();
							}
							AddValue ((char) v);
						}
						else
							AddValue ('}');
					}
					else if (PeekChar () == '.') {
						ReadChar ();
						if (PeekChar () == '.') {
							Expect ("..");
							AddValue (LdmlConst.ThreeDots);
						}
						else
							AddValue ('.');
					}
					else
						AddValue ((char) ReadChar ());
					Expect ('\'');
					break;
				default:
					AddValue ((char) ReadChar ());
					break;
				}
			}
			Expect ('}');
		}

		void SkipSection ()
		{
			SkipIgnorables ();
			Expect ('{');
			bool quoted = false;
			for (int count = 1; count > 0;) {
				if (PeekChar () < 0)
					throw Error ("Unterminated brace section.");
				switch (ReadChar ()) {
				case '\'':
					count += quoted ? -1 : 1;
					quoted = !quoted;
					break;
				case '{':
					if (!quoted)
						count++;
					break;
				case '}':
					if (!quoted)
						count--;
					break;
				}
			}
		}

		void SkipComments ()
		{
			for (SkipWhitespace ();
				PeekChar () == '/';
				SkipWhitespace ()) {
				ReadChar ();
				if (PeekChar () == '/') {
					ReadLine ();
					continue;
				}
				if (PeekChar () != '*')
					break; // FIXME: it is unsafe (but won't happen)
				ReadChar ();
				bool loop = true;
				while (loop) {
					if (PeekChar () < 0)
						throw Error ("Invalid LDML basic document");
					// can't ReadChar() at second step,
					// since * might be sequential.
					if (ReadChar () == '*' && PeekChar () == '/')
						break;
				}
				ReadChar (); // '/'
			}
		}
	}

	public class Ldml
	{
		public string Language;
		public string [] ValidSubLocales;

		// Store only standard collation (except for Chinese which
		// has "big5han" for the standard collation).

		// There is only on "rules"

		public string Alias;

		int [] rules = new int [200];
		int ruleLength;

		// it could be transient (might result in different instance in the end).
		public int [] Rules {
			get { return rules; }
		}

		public int RuleLength {
			get { return ruleLength; }
		}

		public void AddValue (int value)
		{
			if (ruleLength == rules.Length) {
				int [] tmp = new int [ruleLength * 2];
				Array.Copy (rules, tmp, ruleLength);
				rules = tmp;
			}
		}
	}
}
