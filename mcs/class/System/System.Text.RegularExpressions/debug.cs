//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	debug.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

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

namespace System.Text.RegularExpressions {

	class Disassembler {
		public static void DisassemblePattern (ushort[] image) {
			DisassembleBlock (image, 0, 0);
		}
	
		public static void DisassembleBlock (ushort[] image, int pc, int depth) {
			OpCode op;
			OpFlags flags;

			for (;;) {
				if (pc >= image.Length)
					return;
			
				PatternCompiler.DecodeOp (image[pc], out op, out flags);
				Console.Write (FormatAddress (pc) + ": ");		// address
				Console.Write (new string (' ', depth * 2));		// indent
				Console.Write (DisassembleOp (image, pc));		// instruction
				Console.WriteLine ();

				int skip;
				switch (op) {
				case OpCode.False: case OpCode.True: case OpCode.Until:
					skip = 1;
					break;

				case OpCode.Character: case OpCode.Category: case OpCode.Position:
				case OpCode.Open: case OpCode.Close: case OpCode.Reference:
				case OpCode.Sub: case OpCode.Branch: case OpCode.Jump: case OpCode.In:
					skip = 2;
					break;

				case OpCode.Balance: case OpCode.IfDefined: case OpCode.Range:
				case OpCode.Test: case OpCode.Anchor:
					skip = 3;
					break;

				case OpCode.Repeat: case OpCode.FastRepeat: case OpCode.Info:
					skip = 4;
					break;

				case OpCode.String: skip = image[pc + 1] + 2; break;
				case OpCode.Set: skip = image[pc + 2] + 3; break;

				default:
					skip = 1;
					break;
				}

				pc += skip;
			}
		}

		public static string DisassembleOp (ushort[] image, int pc) {
			OpCode op;
			OpFlags flags;

			PatternCompiler.DecodeOp (image[pc], out op, out flags);
			string str = op.ToString ();
			if (flags != 0)
				str += "[" + flags.ToString ("f") + "]";

			switch (op) {
			case OpCode.False: case OpCode.True: case OpCode.Until:
			default:
				break;

			case OpCode.Info:
				str += " " + image[pc + 1];
				str += " (" + image[pc + 2] + ", " + image[pc + 3] + ")";
				break;
			
			case OpCode.Character:
				str += " '" + FormatChar ((char)image[pc + 1]) + "'";
				break;

			case OpCode.Category:
				str += " /" + (Category)image[pc + 1];
				break;
			
			case OpCode.Range:
				str += " '" + FormatChar ((char)image[pc + 1]) + "', ";
				str += " '" + FormatChar ((char)image[pc + 2]) + "'";
				break;

			case OpCode.Set:
				str += " " + FormatSet (image, pc + 1);
				break;

			case OpCode.String:
				str += " '" + ReadString (image, pc + 1) + "'";
				break;

			case OpCode.Position:
				str += " /" + (Position)image[pc + 1];
				break;

			case OpCode.Open: case OpCode.Close: case OpCode.Reference:
				str += " " + image[pc + 1];
				break;

			case OpCode.Balance:
				str += " " + image[pc + 1] + " " + image[pc + 2];
				break;

			case OpCode.IfDefined: case OpCode.Anchor:
				str += " :" + FormatAddress (pc + image[pc + 1]);
				str += " " + image[pc + 2];
				break;
			
			case OpCode.Sub: case OpCode.Branch: case OpCode.Jump:
			case OpCode.In:
				str += " :" + FormatAddress (pc + image[pc + 1]);
				break;

			case OpCode.Test:
				str += " :" + FormatAddress (pc + image[pc + 1]);
				str += ", :" + FormatAddress (pc + image[pc + 2]);
				break;

			case OpCode.Repeat: case OpCode.FastRepeat:
				str += " :" + FormatAddress (pc + image[pc + 1]);
				str += " (" + image[pc + 2] + ", ";
				if (image[pc + 3] == 0xffff)
					str += "Inf";
				else
					str += image[pc + 3];
				str += ")";
				break;

			}

			return str;
		}

		// private static members
	
		private static string ReadString (ushort[] image, int pc) {
			int len = image[pc];
			char[] chars = new char[len];

			for (int i = 0; i < len; ++ i)
				chars[i] = (char)image[pc + i + 1];

			return new string (chars);
		}

		private static string FormatAddress (int pc) {
			return pc.ToString ("x4");
		}

		private static string FormatSet (ushort[] image, int pc) {
			int lo = image[pc ++];
			int hi = (image[pc ++] << 4) - 1;

			string str = "[";

			bool hot = false;
			char a = (char)0, b;
			for (int i = 0; i <= hi; ++ i) {
				bool m = (image[pc + (i >> 4)] & (1 << (i & 0xf))) != 0;

				if (m & !hot) {				// start of range
					a = (char)(lo + i);
					hot = true;
				}
				else if (hot & (!m || i == hi)) {	// end of range
					b = (char)(lo + i - 1);

					str += FormatChar (a);
					if (b != a)
						str += "-" + FormatChar (b);
					
					hot = false;
				}
			}

			str += "]";
			return str;
		}

		private static string FormatChar (char c) {
			if (c == '-' || c == ']')
				return "\\" + c;

			if (Char.IsLetterOrDigit (c) || Char.IsSymbol (c))
				return c.ToString ();
			
			if (Char.IsControl (c)) {
				return "^" + (char)('@' + c);
			}

			return "\\u" + ((int)c).ToString ("x4");
		}
	}
}
