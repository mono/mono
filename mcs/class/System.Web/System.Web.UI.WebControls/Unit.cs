
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
/**
 * Namespace: System.Web.UI.WebControls
 * Struct:    Unit
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Globalization;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[TypeConverter(typeof(UnitConverter))]
	[Serializable]
	public struct Unit
	{
		public static readonly Unit Empty = new Unit();

		private static int Min = -32768;
		private static int Max = +32767;

		private UnitType type;
		private double   val;

		public static Unit Parse(string s)
		{
			return new Unit(s);
		}

		public static Unit Parse(string s, CultureInfo culture)
		{
			return new Unit(s, culture);
		}

		public static Unit Percentage(double n)
		{
			return new Unit (n, UnitType.Percentage);
		}

		public static Unit Pixel(int n)
		{
			return new Unit (n, UnitType.Pixel);
		}

		public static Unit Point(int n)
		{
			return new Unit(n, UnitType.Point);
		}

		public static bool operator ==(Unit left, Unit right)
		{
			return (left.type == right.type && left.val == right.val);
		}

		public static bool operator !=(Unit left, Unit right)
		{
			return !(left == right);
		}

		public static implicit operator Unit(int n)
		{
			return new Unit(n);
		}

		public Unit(double value)
		{
			if(value < Min || value > Max)
			{
				throw new ArgumentOutOfRangeException();
			}
			val = value;
			type = UnitType.Pixel;
		}

		public Unit(int value)
		{
			if(value < Min || value > Max)
			{
				throw new ArgumentOutOfRangeException();
			}
			val = value;
			type = UnitType.Pixel;
		}

		public Unit(string value): this(value, CultureInfo.CurrentCulture)
		{
		}

		public Unit(double value, UnitType type)
		{
			if(value < Min || value > Max)
			{
				throw new ArgumentOutOfRangeException();
			}
			val = value;
			this.type = type;
		}

		public Unit(string value, CultureInfo culture): this(value, culture, UnitType.Pixel)
		{
		}

		internal Unit(string value, CultureInfo culture, UnitType defType)
		{
			string valueTrim;
			if (value == null || (valueTrim = value.Trim ()).Length == 0) {
				val = 0;
				type = (UnitType)0;
				return;
			}

			if (culture == null)
				culture = CultureInfo.CurrentCulture;

			string strVal = valueTrim.ToLower ();
			int length = strVal.Length;
			char c;
			int start = -1;
			for (int i = 0; i < length; i++) {
				c = strVal [i];
				if( (c >= '0' && c <= '9') || (c == '-' || c == '.' || c == ',') )
					start = i;
			}
			
			if (start == -1)
				throw new ArgumentException("No digits in 'value'");
			
			start++;
			if (start < length) {
				type = GetTypeFromString (strVal.Substring (start).Trim ());
				val  = 0;
			} else {
				type = defType;
			}

			try {
				string numbers = strVal.Substring (0, start);
				if (type == UnitType.Pixel)
					val = (double) Int32.Parse (numbers, culture);
				else
					val = (double) Single.Parse (numbers, culture);
			} catch (Exception) {
				throw new FormatException ("Error parsing " + value);
			}

			if (val < Min || val > Max)
				throw new ArgumentOutOfRangeException ();
		}

		private static UnitType GetTypeFromString(string s)
		{
			if(s == null || s.Length == 0)
				return UnitType.Pixel;
			switch(s)
			{
				case "px":
					return UnitType.Pixel;
				case "pt":
					return UnitType.Point;
				case "pc":
					return UnitType.Pica;
				case "in":
					return UnitType.Inch;
				case "mm":
					return UnitType.Mm;
				case "cm":
					return UnitType.Cm;
				case "%":
					return UnitType.Percentage;
				case "em":
					return UnitType.Em;
				case "ex":
					return UnitType.Ex;
				default:
					return UnitType.Pixel;
			}
		}

		private string GetStringFromPixel(UnitType ut)
		{
			switch(ut)
			{
				case UnitType.Pixel:
					return "px";
				case UnitType.Point:
					return "pt";
				case UnitType.Pica:
					return "pc";
				case UnitType.Inch:
					return "in";
				case UnitType.Mm:
					return "mm";
				case UnitType.Cm:
					return "cm";
				case UnitType.Percentage:
					return "%";
				case UnitType.Em:
					return "em";
				case UnitType.Ex:
					return "ex";
				default:
					return String.Empty;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return (type == 0);
			}
		}

		public UnitType Type
		{
			get
			{
				if(IsEmpty)
					return UnitType.Pixel;
				return type;
			}
		}

		public double Value
		{
			get
			{
				return val;
			}
		}

		public override bool Equals(object obj)
		{
			if(obj != null && obj is Unit)
			{
				Unit that = (Unit)obj;
				return ( this.type == that.type && this.val == that.val );
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ( (type.GetHashCode() << 2) | (val.GetHashCode()) );
		}

		public override string ToString()
		{
			if(IsEmpty)
				return String.Empty;
			return ( val.ToString() + GetStringFromPixel(type) );
		}

		public string ToString(CultureInfo culture)
		{
			if(IsEmpty)
				return String.Empty;
			return ( val.ToString(culture.NumberFormat) + GetStringFromPixel(type) );
		}
	}
}
