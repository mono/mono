using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using System.Windows.Forms.RTF;
using System.IO;

namespace TextTestClass {
	public class Test {
		static Test	test;
		int		skip_width;
		int		skip_count;
		private string rtf_string = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n\\viewkind4\\uc1\\pard\\f0\\fs17 testing 123testiong\\par\r\n}";
		private string rtf_string2 =	"{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fswiss\\fcharset0 Arial;}{\\f1\\fmodern\\fprq1\\fcharset0 Courier;}{\\f2\\fswiss\\fprq2\\fcharset0 Arial;}}\r\n" + 
			"{\\colortbl ;\\red255\\green0\\blue0;\\red0\\green0\\blue0;}\r\n" + 
			"{\\*\\generator Msftedit 5.41.15.1507;}\\viewkind4\\uc1\\pard\\f0\\fs20 I am in Arial 10pt\\par\r\n" + 
			"\\fs24 I am in Arial 12pt\\par\r\n" +
			"\\f1 I am in Courier 12pt\\par\r\n" + 
			"\\cf1 I am in Courier 12pt Red\\par\r\n" + 
			"\\cf2\\f2\\fs20 I am in Arial 10pt\\par\r\n" +
			"\\b I am in Arial 10pt Italic\\cf0\\b0\\f0\\par\r\n" +
			"}";
		private string rtf_string3 = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fswiss\\fcharset0 Arial;}{" + 
			"\\f1\\fmodern\\fprq1\\fcharset0 Courier;}{\\f2\\fswiss\\fprq2\\fcharset0 Arial;}{\\f3\\fni" +
			"l\\fcharset0 Impact;}{\\f4\\fnil\\fcharset0 Arial Unicode MS;}{\\f5\\fnil\\fcharset136 Arial Unicode MS;}{\\f6\\fnil\\fcharset0 MS" +
			" Shell Dlg;}}" +
			"{\\colortbl ;\\red255\\green0\\blue0;\\red0\\green0\\blue0;}" +
			"{\\*\\generator Msftedit 5.41.15.1507;}\\viewkind4\\uc1\\pard\\f0\\fs20 I am in Arial 1" +
			"0pt\\par" +
			"\\fs24 I am in Arial 12pt\\par" +
			"\\f1 I am in Courier 12pt\\par" +
			"\\cf1 I am in Courier 12pt Red\\par" +
			"\\cf2\\f2\\fs20 I am in Arial 10pt\\par" +
			"\\b I am in Arial 10pt Bold\\par" +
			"\\i I am in Arial 10pt Bold Italic\\par" +
			"\\ul I am in Arial 10pt Bold Italic Underline\\par" +
			"\\ulnone\\b0\\i0\\strike I am in Arial 10pt Strikethrough\\par" +
			"\\cf0\\strike0\\f3\\fs23 Some cyrilic character: \\u1034?\\par" +
			"And 5 CJK characters: \\f4\\fs21\\u23854?\\u23854?\\u23854?\\u23854?\\u23854?\\f5\\fs17\\par" + 
			"Some special chars:\\par" +
			"\\tab Tilde: ~\\par" +
			"\\tab Questionmark:?\\par" +
			"\\tab Yen: \\f5\\u165?\\f6\\fs17\\par" +
			"\\tab Umlaut: \\'e4\\par" +
			"\\f0\\fs20\\par" +
			"}";

		TextMap text;

		public Test() {
			MemoryStream	stream;
			RTF		rtf;
			byte[]		buffer;

			text = new TextMap();
			TextMap.SetupStandardTable(text.Table);

			buffer = new byte[rtf_string.Length];
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = (byte)rtf_string[i];
			}
			stream = new MemoryStream(buffer);
			rtf = new RTF(stream);

			skip_width = 0;
			skip_count = 0;

			rtf.ClassCallback[TokenClass.Text] = new ClassDelegate(HandleText);
			rtf.ClassCallback[TokenClass.Control] = new ClassDelegate(HandleControl);

			rtf.Read();

			stream.Close();
		}

		void HandleControl(RTF rtf) {
			switch(rtf.Major) {
				case Major.Unicode: {
					switch(rtf.Minor) {
						case Minor.UnicodeCharBytes: {
							skip_width = rtf.Param;
							break;
						}

						case Minor.UnicodeChar: {
							Console.Write("[Unicode {0:X4}]", rtf.Param);
							skip_count += skip_width;
							break;
						}
					}
					break;
				}

				case Major.Destination: {
					Console.Write("[Got Destination control {0}]", rtf.Minor);
					rtf.SkipGroup();
					break;
				}

				case Major.CharAttr: {
					switch(rtf.Minor) {
						case Minor.ForeColor: {
							System.Windows.Forms.RTF.Color	color;
							int	num;

							color = System.Windows.Forms.RTF.Color.GetColor(rtf, rtf.Param);
							if (color != null) {
								if (color.Red == -1 && color.Green == -1 && color.Blue == -1) {
									Console.Write("[Default Color]");
								} else {
									Console.Write("[Color {0} [{1:X2}{2:X2}{3:X}]]", rtf.Param, color.Red, color.Green, color.Blue);
								}
							}
							break;
						}

						case Minor.FontSize: {
							Console.Write("[Fontsize {0}]", rtf.Param);
							break;
						}

						case Minor.FontNum: {
							System.Windows.Forms.RTF.Font	font;

							font = System.Windows.Forms.RTF.Font.GetFont(rtf, rtf.Param);
							if (font != null) {
								Console.Write("[Font {0} [{1}]]", rtf.Param, font.Name);
							}
							break;
						}

						case Minor.Plain: {
							Console.Write("[Normal]");
							break;
						}

						case Minor.Bold: {
							if (rtf.Param == RTF.NoParam) {
								Console.Write("[Bold]");
							} else {
								Console.Write("[NoBold]");
							}
							break;
						}

						case Minor.Italic: {
							if (rtf.Param == RTF.NoParam) {
								Console.Write("[Italic]");
							} else {
								Console.Write("[NoItalic]");
							}
							break;
						}

						case Minor.StrikeThru: {
							if (rtf.Param == RTF.NoParam) {
								Console.Write("[StrikeThru]");
							} else {
								Console.Write("[NoStrikeThru]");
							}
							break;
						}

						case Minor.Underline: {
							if (rtf.Param == RTF.NoParam) {
								Console.Write("[Underline]");
							} else {
								Console.Write("[NoUnderline]");
							}
							break;
						}

						case Minor.NoUnderline: {
							Console.Write("[NoUnderline]");
							break;
						}
					}
					break;
				}

				case Major.SpecialChar: {
					Console.Write("[Got SpecialChar control {0}]", rtf.Minor);
					SpecialChar(rtf);
					break;
				}
			}
		}

		void SpecialChar(RTF rtf) {
			switch(rtf.Minor) {
				case Minor.Page:
				case Minor.Sect:
				case Minor.Row:
				case Minor.Line:
				case Minor.Par: {
					Console.Write("\n");
					break;
				}

				case Minor.Cell: {
					Console.Write(" ");
					break;
				}

				case Minor.NoBrkSpace: {
					Console.Write(" ");
					break;
				}

				case Minor.Tab: {
					Console.Write("\t");
					break;
				}

				case Minor.NoBrkHyphen: {
					Console.Write("-");
					break;
				}

				case Minor.Bullet: {
					Console.Write("*");
					break;
				}

				case Minor.EmDash: {
					Console.Write("");
					break;
				}

				case Minor.EnDash: {
					Console.Write("");
					break;
				}

				case Minor.LQuote: {
					Console.Write("");
					break;
				}

				case Minor.RQuote: {
					Console.Write("");
					break;
				}

				case Minor.LDblQuote: {
					Console.Write("");
					break;
				}

				case Minor.RDblQuote: {
					Console.Write("");
					break;
				}

				default: {
					rtf.SkipGroup();
					break;
				}
			}
		}


		void HandleText(RTF rtf) {
			if (skip_count > 0) {
				skip_count--;
				return;
			}
			if ((StandardCharCode)rtf.Minor != StandardCharCode.nothing) {
				Console.Write("{0}", text[(StandardCharCode)rtf.Minor]);
			} else {
				if ((int)rtf.Major > 31 && (int)rtf.Major < 128) {
					Console.Write("{0}", (char)rtf.Major);
				} else {
					Console.Write("[Literal:0x{0:X2}]", (int)rtf.Major);
				}
			}
		}

		public static void Main() {
			test = new Test();
		}
	}
}
