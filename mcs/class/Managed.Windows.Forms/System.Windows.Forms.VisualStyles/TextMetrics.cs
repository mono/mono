//
// TextMetrics.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

namespace System.Windows.Forms.VisualStyles
{
	public struct TextMetrics
	{
		#region Private Variables
		private int ascent;
		private int average_char_width;
		private char break_char;
		private TextMetricsCharacterSet char_set;
		private char default_char;
		private int descent;
		private int digitized_aspect_x;
		private int digitized_aspect_y;
		private int external_leading;
		private char first_char;
		private int height;
		private int internal_leading;
		private bool italic;
		private char last_char;
		private int max_char_width;
		private int overhang;
		private TextMetricsPitchAndFamilyValues pitch_and_family;
		private bool struck_out;
		private bool underlined;
		private int weight;
		#endregion

		#region Public Properties
		public int Ascent {
			get { return this.ascent; }
			set { this.ascent = value; }
		}

		public int AverageCharWidth {
			get { return this.average_char_width; }
			set { this.average_char_width = value; }
		}

		public char BreakChar {
			get { return this.break_char; }
			set { this.break_char = value; }
		}

		public TextMetricsCharacterSet CharSet {
			get { return this.char_set; }
			set { this.char_set = value; }
		}

		public char DefaultChar {
			get { return this.default_char; }
			set { this.default_char = value; }
		}

		public int Descent {
			get { return this.descent; }
			set { this.descent = value; }
		}

		public int DigitizedAspectX {
			get { return this.digitized_aspect_x; }
			set { this.digitized_aspect_x = value; }
		}

		public int DigitizedAspectY {
			get { return this.digitized_aspect_y; }
			set { this.digitized_aspect_y = value; }
		}

		public int ExternalLeading {
			get { return this.external_leading; }
			set { this.external_leading = value; }
		}

		public char FirstChar {
			get { return this.first_char; }
			set { this.first_char = value; }
		}

		public int Height {
			get { return this.height; }
			set { this.height = value; }
		}

		public int InternalLeading {
			get { return this.internal_leading; }
			set { this.internal_leading = value; }
		}

		public bool Italic {
			get { return this.italic; }
			set { this.italic = value; }
		}

		public char LastChar {
			get { return this.last_char; }
			set { this.last_char = value; }
		}

		public int MaxCharWidth {
			get { return this.max_char_width; }
			set { this.max_char_width = value; }
		}

		public int Overhang {
			get { return this.overhang; }
			set { this.overhang = value; }
		}

		public TextMetricsPitchAndFamilyValues PitchAndFamily {
			get { return this.pitch_and_family; }
			set { this.pitch_and_family = value; }
		}

		public bool StruckOut {
			get { return this.struck_out; }
			set { this.struck_out = value; }
		}

		public bool Underlined {
			get { return this.underlined; }
			set { this.underlined = value; }
		}

		public int Weight {
			get { return this.weight; }
			set { this.weight = value; }
		}
		#endregion
	}
}