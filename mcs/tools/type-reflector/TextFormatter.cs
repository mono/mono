//
// TextFormatter.cs: Formats text for display output
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	internal class TextFormatter {
		private int leftMargin = 0;
		private int rightMargin = 80;
		private int subsequentIndent = 0;

		public int LeftMargin {
			get {return leftMargin;}
			set {leftMargin = value;}
		}

		public int RightMargin {
			get {return rightMargin;}
			set {rightMargin = value;}
		}

		public int SubsequentIndent {
			get {return subsequentIndent;}
			set {subsequentIndent = value;}
		}

		public TextFormatter ()
			: this (0)
		{
		}

		public TextFormatter (int leftMargin)
			: this (leftMargin, 80)
		{
		}

		public TextFormatter (int leftMargin, int rightMargin)
			: this (leftMargin, rightMargin, 0)
		{
		}

		public TextFormatter (int leftMargin, int rightMargin, int subsequentIndent)
		{
			this.leftMargin = leftMargin;
			this.rightMargin = rightMargin;
			this.subsequentIndent = subsequentIndent;
		}

		private void WrapText (string text, int width, IList lines)
		{
			if (text.Length <= width) {
				lines.Add (text);
				return;
			}

			while (text.Length > width) {
				int b = width;
				if (!Char.IsWhiteSpace(text[b])) {
					b = text.LastIndexOf (' ', b);
					if (b == -1)
						// couldn't find an earlier word break
						b = width;
				}
				lines.Add (text.Substring (0, b));
				text = text.Substring (b).Trim();
			}
			lines.Add (text);
		}

		private ICollection WrapText (string text, int width)
		{
			ArrayList lines = new ArrayList ();
			string[] paragraphs = text.Split(new char[] {'\n'});
			foreach (string p in paragraphs)
				WrapText (p, width, lines);
			return lines;
		}

		public string Group (string text)
		{
			// should be "- followingIndent", but need an extra space for the '\n'.
			int width = (RightMargin - LeftMargin) - (SubsequentIndent+1);
			StringBuilder sb = new StringBuilder ();
			string format1 = "{1}";
			string formatN = "{0,-" + (LeftMargin + SubsequentIndent) + "}{1}";
			ICollection c = WrapText (text, width);
			int last = c.Count;
			string format = format1;
			foreach (string s in c) {
				string line = String.Format (format, "", s);
				sb.Append (line);
				if (--last != 0) {
					format = formatN;
					sb.Append ("\n");
				}
			}
			return sb.ToString();
		}

		private static int MaxDepth (string[][] args)
		{
			int max = 0;
			foreach (string[] arg in args) {
				if (arg.Length > max)
					max = arg.Length;
			}
			return max;
		}

		public static string Format (string format, params string[][] args)
		{
			int count = MaxDepth (args);
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i != count; ++i) {
				ArrayList row = new ArrayList ();
				foreach (string[] arg in args) {
					if (arg.Length > i)
						row.Add (arg[i]);
					else
						row.Add ("");
				}
				sb.Append (string.Format (format, row.ToArray()));
			}
			return sb.ToString();
		}
	}
}

