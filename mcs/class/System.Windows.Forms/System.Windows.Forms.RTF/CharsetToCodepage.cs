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
// Copyright (c) 2020 Karl Scowen
//
// Authors:
//  Karl Scowen		<contact@scowencomputers.co.nz>
//
//


namespace System.Windows.Forms.RTF {
	internal static class CharsetToCodepage {
		public static int Translate(CharsetType charset)
		{
			switch (charset) {
				case CharsetType.General:
				case CharsetType.Arabic_Traditional:
				case CharsetType.Arabic_user:
				case CharsetType.Hebrew_user:
				case CharsetType.Mac: // Technically wrong, because "mac" should actually be quite a few with their own code pages...
				default:
					return System.Text.Encoding.Default.CodePage;
				case CharsetType.ANSI:
					return 1252;
				case CharsetType.Symbol:
					return 42;
				case CharsetType.Shift_Jis:
					return 932;
				case CharsetType.Hangul:
					return 949;
				case CharsetType.Johab:
					return 1361;
				case CharsetType.GB2312:
					return 936;
				case CharsetType.Big5:
					return 950;
				case CharsetType.Greek:
					return 1253;
				case CharsetType.Turkish:
					return 1254;
				case CharsetType.Vietnamese:
					return 1258;
				case CharsetType.Hebrew:
					return 1255;
				case CharsetType.Arabic:
					return 1256;
				case CharsetType.Baltic:
					return 1257;
				case CharsetType.Russian:
					return 1251;
				case CharsetType.Thai:
					return 874;
				case CharsetType.Eastern_European:
					return 1250;
				case CharsetType.PC_437:
					return 437;
				case CharsetType.OEM:
					return 850;
			}
		}
	}
}