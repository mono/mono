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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//  Karl Scowen		<contact@scowencomputers.co.nz>
//

// COMPLETE

#undef RTF_DEBUG

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace System.Windows.Forms.RTF {
	internal class RTF {
		#region	Local Variables
		internal const char	EOF = unchecked((char)-1);
		internal const int	NoParam = -1000000;
		internal const int	DefaultEncodingCodePage = 1252;

		private TokenClass	rtf_class;
		private Major		major;
		private Minor		minor;
		private int		param;
		private string encoded_text;
		private Encoding encoding;
		private int encoding_code_page = DefaultEncodingCodePage;
		private StringBuilder	text_buffer;
		private Picture picture;
		private int		line_num;
		private int		line_pos;

		private char		pushed_char;
		//private StringBuilder	pushed_text_buffer;
		private TokenClass	pushed_class;
		private Major		pushed_major;
		private Minor		pushed_minor;
		private int		pushed_param;

		private char		prev_char;
		private bool		bump_line;
		private Stack		charset_stack;

		private Style		styles;
		private Color		colors;
		private Font		fonts;

		private StreamReader	source;

		private static Hashtable	key_table;
		private static KeyStruct[]	Keys = KeysInit.Init();

		private DestinationCallback	destination_callbacks;
		private ClassCallback		class_callbacks;
		#endregion	// Local Variables

		#region Constructors
		static RTF() {
			key_table = new Hashtable(Keys.Length);
			for (int i = 0; i < Keys.Length; i++) {
				key_table[Keys[i].Symbol] = Keys[i];
			}
		}

		public RTF(Stream stream) {
			source = new StreamReader(stream);

			text_buffer = new StringBuilder(1024);
			//pushed_text_buffer = new StringBuilder(1024);

			rtf_class = TokenClass.None;
			pushed_class = TokenClass.None;
			pushed_char = unchecked((char)-1);

			line_num = 0;
			line_pos = 0;
			prev_char = unchecked((char)-1);
			bump_line = false;
			charset_stack = new Stack();

			destination_callbacks = new DestinationCallback();
			class_callbacks = new ClassCallback();

			destination_callbacks [Minor.OptDest] = new DestinationDelegate (HandleOptDest);
			destination_callbacks[Minor.FontTbl] = new DestinationDelegate(ReadFontTbl);
			destination_callbacks[Minor.ColorTbl] = new DestinationDelegate(ReadColorTbl);
			destination_callbacks[Minor.StyleSheet] = new DestinationDelegate(ReadStyleSheet);
			destination_callbacks[Minor.Info] = new DestinationDelegate(ReadInfoGroup);
			destination_callbacks[Minor.Pict] = new DestinationDelegate(ReadPictGroup);
			destination_callbacks[Minor.Object] = new DestinationDelegate(ReadObjGroup);
		}
		#endregion	// Constructors

		#region Properties
		public TokenClass TokenClass {
			get {
				return this.rtf_class;
			}

			set {
				this.rtf_class = value;
			}
		}

		public Major Major {
			get {
				return this.major;
			}

			set {
				this.major = value;
			}
		}

		public Minor Minor {
			get {
				return this.minor;
			}

			set {
				this.minor = value;
			}
		}

		public int Param {
			get {
				return this.param;
			}

			set {
				this.param = value;
			}
		}

		public string Text {
			get {
				return this.text_buffer.ToString();
			}

			set {
				if (value == null) {
					this.text_buffer.Length = 0;
				} else {
					this.text_buffer = new StringBuilder(value);
				}
			}
		}

		public string EncodedText {
			get { return encoded_text; }
		}

		public Picture Picture {
			get { return picture; }
			set { picture = value; }
		}

		public Color Colors {
			get {
				return colors;
			}

			set {
				colors = value;
			}
		}

		public Style Styles {
			get {
				return styles;
			}

			set {
				styles = value;
			}
		}

		public Font Fonts {
			get {
				return fonts;
			}

			set {
				fonts = value;
			}
		}

		public ClassCallback ClassCallback {
			get {
				return class_callbacks;
			}

			set {
				class_callbacks = value;
			}
		}

		public DestinationCallback DestinationCallback {
			get {
				return destination_callbacks;
			}

			set {
				destination_callbacks = value;
			}
		}

		public int LineNumber {
			get {
				return line_num;
			}
		}

		public int LinePos {
			get {
				return line_pos;
			}
		}
		#endregion	// Properties

		#region Methods
		/// <summary>Set the default font for documents without font table</summary>
		public void DefaultFont(string name) {
			Font font;

			font = new Font(this);
			font.Num = 0;
			font.Name = name;
		}

		/// <summary>Read the next character from the input - skip any crlf</summary>
		private char GetChar ()
		{
			return GetChar (true);
		}

		/// <summary>Read the next character from the input</summary>
		private char GetChar(bool skipCrLf) 
		{
			int	c;
			bool	old_bump_line;

SkipCRLF:
			if ((c = source.Read()) != -1) {
				this.text_buffer.Append((char) c);
			}

			if (this.prev_char == EOF) {
				this.bump_line = true;
			}

			old_bump_line = bump_line;
			bump_line = false;

			if (skipCrLf) {
				if (c == '\r') {
					bump_line = true;
					text_buffer.Length--;
					goto SkipCRLF;
				} else if (c == '\n') {
					bump_line = true;
					if (this.prev_char == '\r') {
						old_bump_line = false;
					}

					text_buffer.Length--;
					goto SkipCRLF;
				}
			}

			this.line_pos ++;
			if (old_bump_line) {
				this.line_num++;
				this.line_pos = 1;
			}

			this.prev_char = (char) c;
			return (char) c;
		}

		/// <summary>Parse the RTF stream</summary>
		public void Read() {
			while (GetToken() != TokenClass.EOF) {
				RouteToken();
			}
		}

		/// <summary>Route a token</summary>
		public void RouteToken() {

			if (CheckCM(TokenClass.Control, Major.Destination)) {
				DestinationDelegate d;

				d = destination_callbacks[minor];
				if (d != null) {
					d(this);
				}
			}

			// Invoke class callback if there is one
			ClassDelegate c;

			c = class_callbacks[rtf_class];
			if (c != null) {
				c(this);
			}
			
		}

		/// <summary>Skip to the end of the next group to start or current group we are in</summary>
		public void SkipGroup() {
			int	level;

			level = 1;

			while (GetToken() != TokenClass.EOF) {
				if (rtf_class == TokenClass.Group) {
					if (this.major == Major.BeginGroup) {
						level++;
					} else if (this.major == Major.EndGroup) {
						level--;
						if (level < 1) {
							break;
						}
					}
				}
			}
		}

		/// <summary>Return the next token in the stream</summary>
		public TokenClass GetToken() {
			if (pushed_class != TokenClass.None) {
				this.rtf_class = this.pushed_class;
				this.major = this.pushed_major;
				this.minor = this.pushed_minor;
				this.param = this.pushed_param;
				this.pushed_class = TokenClass.None;
				return this.rtf_class;
			}

			GetToken2();

			if (this.rtf_class == TokenClass.Text) {
				if (encoding == null)
					encoding = Encoding.GetEncoding (encoding_code_page);
				encoded_text = new String (encoding.GetChars (new byte [] { (byte) this.major }));
			} else if (CheckCMM (TokenClass.Control, Major.Unicode, Minor.UnicodeAnsiCodepage)) {
				encoding_code_page = param;

				// fallback to the default one in case we have an invalid value
				if (encoding_code_page < 0 || encoding_code_page > 65535)
					encoding_code_page = DefaultEncodingCodePage;

				encoding = null;
			} else if (CheckCMM(TokenClass.Control, Major.CharAttr, Minor.FontNum)) {
				Font	fp;

				fp = Font.GetFont(this.fonts, this.param);

				if (fp != null) {
					if (fp.Codepage != 0) {
						if (fp.Codepage != encoding_code_page) {
							encoding_code_page = fp.Codepage;
							encoding = null;
						}
					} else {
						var cp = CharsetToCodepage.Translate (fp.Charset);
						if (cp != 0 && cp != encoding_code_page) {
							encoding_code_page = cp;
							encoding = null;
						}
					}
				}
			} else if (this.rtf_class == TokenClass.Group) {
				switch(this.major) {
					case Major.BeginGroup: {
						charset_stack.Push(encoding_code_page);
						break;
					}

					case Major.EndGroup: {
						if (charset_stack.Count > 0) {
							encoding_code_page = (int)this.charset_stack.Pop();
						} else {
							encoding_code_page = DefaultEncodingCodePage;
						}
						if (encoding != null && encoding.CodePage != encoding_code_page)
							encoding = null;
						break;
					}
				}
			}

			return this.rtf_class;
		}

		private void GetToken2() {
			char	c;
			int	sign;

			this.rtf_class = TokenClass.Unknown;
			this.param = NoParam;

			this.text_buffer.Length = 0;

			if (this.pushed_char != EOF) {
				c = this.pushed_char;
				this.text_buffer.Append(c);
				this.pushed_char = EOF;
			} else if ((c = GetChar()) == EOF) {
				this.rtf_class = TokenClass.EOF;
				return;
			}

			if (c == '{') {
				this.rtf_class = TokenClass.Group;
				this.major = Major.BeginGroup;
				return;
			}

			if (c == '}') {
				this.rtf_class = TokenClass.Group;
				this.major = Major.EndGroup;
				return;
			}

			if (c != '\\') {
				if (c != '\t') {
					this.rtf_class = TokenClass.Text;
					this.major = (Major)c;	// FIXME - typing?
					return;
				} else {
					this.rtf_class = TokenClass.Control;
					this.major = Major.SpecialChar;
					this.minor = Minor.Tab;
					return;
				}
			}

			if ((c = GetChar()) == EOF) {
				// Not so good
				return;
			}

			if (!Char.IsLetter(c)) {
				if (c == '\'') {
					char c2;

					if ((c = GetChar()) == EOF) {
						return;
					}

					if ((c2 = GetChar()) == EOF) {
						return;
					}

					this.rtf_class = TokenClass.Text;
					this.major = (Major)((Char)((Convert.ToByte(c.ToString(), 16) * 16 + Convert.ToByte(c2.ToString(), 16))));
					return;
				}

				// Escaped char
				if (c == ':' || c == '{' || c == '}' || c == '\\') {
					this.rtf_class = TokenClass.Text;
					this.major = (Major)c;
					return;
				}

				Lookup(this.text_buffer.ToString());
				return;
			}

			while (Char.IsLetter(c)) {
				if ((c = GetChar(false)) == EOF) {
					break;
				}
			}

			if (c != EOF) {
				this.text_buffer.Length--;
			}

			Lookup(this.text_buffer.ToString());

			if (c != EOF) {
				this.text_buffer.Append(c);
			}

			sign = 1;
			if (c == '-') {
				sign = -1;
				c = GetChar();
			}

			if (c != EOF && Char.IsDigit(c) && minor != Minor.PngBlip) {
				this.param = 0;
				while (Char.IsDigit(c)) {
					this.param = this.param * 10 + Convert.ToByte(c) - 48;
					if ((c = GetChar()) == EOF) {
						break;
					}
				}
				this.param *= sign;
			}

			if (c != EOF) {
				if (c != ' ' && c != '\r' && c != '\n') {
					this.pushed_char = c;
				}
				this.text_buffer.Length--;
			}
		}

		public void SetToken(TokenClass cl, Major maj, Minor min, int par, string text) {
			this.rtf_class = cl;
			this.major = maj;
			this.minor = min;
			this.param = par;
			if (par == NoParam) {
				this.text_buffer = new StringBuilder(text);
			} else {
				this.text_buffer = new StringBuilder(text + par.ToString());
			}
		}

		public void UngetToken() {
			if (this.pushed_class != TokenClass.None) {
				throw new RTFException(this, "Cannot unget more than one token");
			}

			if (this.rtf_class == TokenClass.None) {
				throw new RTFException(this, "No token to unget");
			}

			this.pushed_class = this.rtf_class;
			this.pushed_major = this.major;
			this.pushed_minor = this.minor;
			this.pushed_param = this.param;
			//this.pushed_text_buffer = new StringBuilder(this.text_buffer.ToString());
		}

		public TokenClass PeekToken() {
			GetToken();
			UngetToken();
			return rtf_class;
		}

		public void Lookup(string token) {
			Object		obj;
			KeyStruct	key;

			obj = key_table[token.Substring(1)];
			if (obj == null) {
				rtf_class = TokenClass.Unknown;
				major = (Major) -1;
				minor = (Minor) -1;
				return;
			}

			key = (KeyStruct)obj;
			this.rtf_class = TokenClass.Control;
			this.major = key.Major;
			this.minor = key.Minor;
		}

		public bool CheckCM(TokenClass rtf_class, Major major) {
			if ((this.rtf_class == rtf_class) && (this.major == major)) {
				return true;
			}

			return false;
		}

		public bool CheckCMM(TokenClass rtf_class, Major major, Minor minor) {
			if ((this.rtf_class == rtf_class) && (this.major == major) && (this.minor == minor)) {
				return true;
			}

			return false;
		}

		public bool CheckMM(Major major, Minor minor) {
			if ((this.major == major) && (this.minor == minor)) {
				return true;
			}

			return false;
		}
		#endregion	// Methods

		#region Default Delegates

		private void HandleOptDest (RTF rtf)
		{
			int group_levels = 1;

			while (true) {
				GetToken ();

				// Here is where we should handle recognised optional
				// destinations.
				//
				// Handle a picture group 
				//
				if (rtf.CheckCMM (TokenClass.Control, Major.Destination, Minor.Pict)) {
					ReadPictGroup (rtf);
					return;
				}

				if (rtf.CheckCM (TokenClass.Group, Major.EndGroup)) {
					if ((--group_levels) == 0) {
						break;
					}
				}

				if (rtf.CheckCM (TokenClass.Group, Major.BeginGroup)) {
					group_levels++;
				}
			}
		}

		private void ReadFontTbl(RTF rtf) {
			int	old;
			Font	font;

			old = -1;
			font = null;

			while (true) {
				rtf.GetToken();

				if (rtf.CheckCM(TokenClass.Group, Major.EndGroup)) {
					break;
				}

				if (old < 0) {
					if (rtf.CheckCMM(TokenClass.Control, Major.CharAttr, Minor.FontNum)) {
						old = 1;
					} else if (rtf.CheckCM(TokenClass.Group, Major.BeginGroup)) {
						old = 0;
					} else {
						throw new RTFException(rtf, "Cannot determine format");
					}
				}

				if (old == 0) {
					if (!rtf.CheckCM(TokenClass.Group, Major.BeginGroup)) {
						throw new RTFException(rtf, "missing \"{\"");
					}
					rtf.GetToken();
				}

				font = new Font(rtf);

				int depth = 0;
				string untaggedName = null;

				while ((rtf.rtf_class != TokenClass.EOF) && (!rtf.CheckCM(TokenClass.Text, (Major)';')) && depth >= 0) {
					if (rtf.rtf_class == TokenClass.Control) {
						switch(rtf.major) {
							case Major.FontFamily: {
								font.Family = (int)rtf.minor;
								break;
							}

							case Major.CharAttr: {
								switch(rtf.minor) {
									case Minor.FontNum: {
										font.Num = rtf.param;
										break;
									}

									default: {
										#if RTF_DEBUG
											Console.WriteLine("Got unhandled Control.CharAttr.Minor: " + rtf.minor);
										#endif
										break;
									}
								}
								break;
							}

							case Major.FontAttr: {
								switch (rtf.minor) {
									case Minor.FontCharSet: {
										font.Charset = (CharsetType)rtf.param;
										break;
									}

									case Minor.FontPitch: {
										font.Pitch = rtf.param;
										break;
									}

									case Minor.FontCodePage: {
										font.Codepage = rtf.param;
										break;
									}

									case Minor.FTypeNil:
									case Minor.FTypeTrueType: {
										font.Type = rtf.param;
										break;
									}
									default: {
										#if RTF_DEBUG
											Console.WriteLine("Got unhandled Control.FontAttr.Minor: " + rtf.minor);
										#endif
										break;
									}
								}
								break;
							}

							case Major.Destination: {
								switch (rtf.minor) {
									case Minor.FontName:
										untaggedName = ReadFontName (rtf);
										break;

									case Minor.FontAltName:
										font.AltName = ReadFontName (rtf);
										break;

									default: {
										#if RTF_DEBUG
											Console.WriteLine ("Got unhandled Control.Destination.Minor: " + rtf.minor);
										#endif
										break;
									}
								}
								break;
							}

							default: {
								#if RTF_DEBUG
									Console.WriteLine("ReadFontTbl: Unknown Control token " + rtf.major);
								#endif
								break;
							}
						}
					} else if (rtf.CheckCM(TokenClass.Group, Major.BeginGroup)) {
						depth++;
					} else if (rtf.CheckCM(TokenClass.Group, Major.EndGroup)) {
						depth--;
					} else if (rtf.rtf_class == TokenClass.Text)
					{
						font.Name = ReadFontName (rtf);
						continue;
#if RTF_DEBUG
					} else {
						Console.WriteLine("ReadFontTbl: Unknown token " + rtf.text_buffer);
#endif
					}

					rtf.GetToken();
				}

				if (untaggedName != null)
					font.Name = untaggedName;

				if (old == 0) {
					rtf.GetToken();

					if (!rtf.CheckCM(TokenClass.Group, Major.EndGroup)) {
						throw new RTFException(rtf, "Missing \"}\"");
					}
				}
			}

			if (font == null) {
				throw new RTFException(rtf, "No font created");
			}

			if (font.Num == -1) {
				throw new RTFException(rtf, "Missing font number");
			}

			rtf.RouteToken();
		}

		private static String ReadFontName(RTF rtf)
		{
			StringBuilder sb = new StringBuilder ();

			while (rtf.rtf_class != TokenClass.EOF && rtf.rtf_class != TokenClass.Text)
				rtf.GetToken ();

			while ((rtf.rtf_class != TokenClass.EOF) && (!rtf.CheckCM (TokenClass.Text, (Major)';')) && (!rtf.CheckCM(TokenClass.Group, Major.EndGroup)) &&
				(!rtf.CheckCM (TokenClass.Group, Major.BeginGroup))) {
				sb.Append ((char)rtf.major);
				rtf.GetToken ();
			}

			if (rtf.CheckCM (TokenClass.Group, Major.EndGroup)) {
				rtf.UngetToken();
			}

			return sb.ToString ();
		}

		private void ReadColorTbl(RTF rtf) {
			Color	color;
			int	num;

			num = 0;

			while (true) {
				rtf.GetToken();

				if (rtf.CheckCM(TokenClass.Group, Major.EndGroup)) {
					break;
				}

				color = new Color(rtf);
				color.Num = num++;

				while (rtf.CheckCM(TokenClass.Control, Major.ColorName)) {
					switch (rtf.minor) {
						case Minor.Red: {
							color.Red = rtf.param;
							break;
						}

						case Minor.Green: {
							color.Green = rtf.param;
							break;
						}

						case Minor.Blue: {
							color.Blue = rtf.param;
							break;
						}
					}

					rtf.GetToken();
				}
				if (!rtf.CheckCM(TokenClass.Text, (Major)';')) {
					throw new RTFException(rtf, "Malformed color entry");
				}
			}
			rtf.RouteToken();
		}

		private void ReadStyleSheet(RTF rtf) {
			Style		style;
			StringBuilder	sb;

			sb = new StringBuilder();

			while (true) {
				rtf.GetToken();

				if (rtf.CheckCM(TokenClass.Group, Major.EndGroup)) {
					break;
				}

				style = new Style(rtf);

				if (!rtf.CheckCM(TokenClass.Group, Major.BeginGroup)) {
					throw new RTFException(rtf, "Missing \"{\"");
				}

				while (true) {
					rtf.GetToken();

					if ((rtf.rtf_class == TokenClass.EOF) || rtf.CheckCM(TokenClass.Text, (Major)';')) {
						break;
					}

					if (rtf.rtf_class == TokenClass.Control) {
						if (rtf.CheckMM(Major.ParAttr, Minor.StyleNum)) {
							style.Num = rtf.param;
							style.Type = StyleType.Paragraph;
							continue;
						}
						if (rtf.CheckMM(Major.CharAttr, Minor.CharStyleNum)) {
							style.Num = rtf.param;
							style.Type = StyleType.Character;
							continue;
						}
						if (rtf.CheckMM(Major.StyleAttr, Minor.SectStyleNum)) {
							style.Num = rtf.param;
							style.Type = StyleType.Section;
							continue;
						}
						if (rtf.CheckMM(Major.StyleAttr, Minor.BasedOn)) {
							style.BasedOn = rtf.param;
							continue;
						}
						if (rtf.CheckMM(Major.StyleAttr, Minor.Additive)) {
							style.Additive = true;
							continue;
						}
						if (rtf.CheckMM(Major.StyleAttr, Minor.Next)) {
							style.NextPar = rtf.param;
							continue;
						}

						new StyleElement(style, rtf.rtf_class, rtf.major, rtf.minor, rtf.param, rtf.text_buffer.ToString());
					} else if (rtf.CheckCM(TokenClass.Group, Major.BeginGroup)) {
						// This passes over "{\*\keycode ... }, among other things
						rtf.SkipGroup();
					} else if (rtf.rtf_class == TokenClass.Text) {
						while (rtf.rtf_class == TokenClass.Text) {
							if (rtf.major == (Major)';') {
								rtf.UngetToken();
								break;
							}

							sb.Append((char)rtf.major);
							rtf.GetToken();
						}

						style.Name = sb.ToString();
#if RTF_DEBUG
					} else {
						Console.WriteLine("ReadStyleSheet: Ignored token " + rtf.text_buffer);
#endif
					}
				}
				rtf.GetToken();

				if (!rtf.CheckCM(TokenClass.Group, Major.EndGroup)) {
					throw new RTFException(rtf, "Missing EndGroup (\"}\"");
				}

				// Sanity checks
				if (style.Name == null) {
					throw new RTFException(rtf, "Style must have name");
				}

				if (style.Num < 0) {
					if (!sb.ToString().StartsWith("Normal") && !sb.ToString().StartsWith("Standard")) {
						throw new RTFException(rtf, "Missing style number");
					}

					style.Num = Style.NormalStyleNum;
				}

				if (style.NextPar == -1) {
					style.NextPar = style.Num;
				}
			}

			rtf.RouteToken();
		}

		private void ReadInfoGroup(RTF rtf) {
			rtf.SkipGroup();
			rtf.RouteToken();
		}

		private void ReadPictGroup(RTF rtf)
		{
			bool read_image_data = false;
			int groupDepth = 0;
			Picture picture = new Picture ();
			while (true) {
				rtf.GetToken ();

				if (rtf.CheckCM (TokenClass.Group, Major.BeginGroup))
					groupDepth++;

				if (rtf.CheckCM (TokenClass.Group, Major.EndGroup))
					groupDepth--;

				if (groupDepth < 0)
					break;

				switch (minor) {
				case Minor.PngBlip:
				case Minor.JpegBlip:
				case Minor.WinMetafile:
				case Minor.EnhancedMetafile:
					picture.ImageType = minor;
					read_image_data = true;
					continue;
				case Minor.PicWid:
					continue;
				case Minor.PicHt:
					continue;
				case Minor.PicGoalWid:
					picture.SetWidthFromTwips (param);
					continue;
				case Minor.PicGoalHt:
					picture.SetHeightFromTwips (param);
					continue;
				}

				if (read_image_data && rtf.rtf_class == TokenClass.Text) {

					picture.Data.Seek (0, SeekOrigin.Begin);

					//char c = (char) rtf.major;

					uint digitValue1;
					uint digitValue2;
					char hexDigit1 = (char) rtf.major;
					char hexDigit2;

					while (true) {

						while (hexDigit1 == '\n' || hexDigit1 == '\r') {
							hexDigit1 = (char) source.Peek ();
							if (hexDigit1 == '}')
								break;
							hexDigit1 = (char) source.Read ();
						}
						
						hexDigit2 = (char) source.Peek ();
						if (hexDigit2 == '}')
							break;
						hexDigit2 = (char) source.Read ();
						while (hexDigit2 == '\n' || hexDigit2 == '\r') {
							hexDigit2 = (char) source.Peek ();
							if (hexDigit2 == '}')
								break;
							hexDigit2 = (char) source.Read ();
						}

						if (Char.IsDigit (hexDigit1))
							digitValue1 = (uint) (hexDigit1 - '0');
						else if (Char.IsLower (hexDigit1))
							digitValue1 = (uint) (hexDigit1 - 'a' + 10);
						else if (Char.IsUpper (hexDigit1))
							digitValue1 = (uint) (hexDigit1 - 'A' + 10);
						else if (hexDigit1 == '\n' || hexDigit1 == '\r')
							continue;
						else
							break;

						if (Char.IsDigit (hexDigit2))
							digitValue2 = (uint) (hexDigit2 - '0');
						else if (Char.IsLower (hexDigit2))
							digitValue2 = (uint) (hexDigit2 - 'a' + 10);
						else if (Char.IsUpper (hexDigit2))
							digitValue2 = (uint) (hexDigit2 - 'A' + 10);
						else if (hexDigit2 == '\n' || hexDigit2 == '\r')
							continue;
						else 
							break;

						picture.Data.WriteByte ((byte) checked (digitValue1 * 16 + digitValue2));

						// We get the first hex digit at the end, since in the very first
						// iteration we use rtf.major as the first hex digit
						hexDigit1 = (char) source.Peek ();
						if (hexDigit1 == '}')
							break;
						hexDigit1 = (char) source.Read ();
					}

					
					read_image_data = false;
					break;
				}
			}

			if (picture.ImageType != Minor.Undefined && !read_image_data) {
				this.picture = picture;
				SetToken (TokenClass.Control, Major.PictAttr, picture.ImageType, 0, String.Empty);
			}
		}

		private void ReadObjGroup (RTF rtf)
		{
			int level;

			level = 1;

			while (GetToken () != TokenClass.EOF && this.minor != Minor.ObjResult) {
				if (rtf_class == TokenClass.Group) {
					if (this.major == Major.BeginGroup) {
						level++;
					} else if (this.major == Major.EndGroup) {
						level--;
						if (level < 1) {
							break;
						}
					}
				}
			}

			if (level >= 1) {
				GetToken ();

				if (rtf_class == TokenClass.Group)
					GetToken ();
				rtf.RouteToken ();
			}
		}
		#endregion	// Default Delegates
	}
	}
