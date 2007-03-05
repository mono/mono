//
// System.Web.UI.WebControls.Unit.cs
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//   Ben Maurer (bmaurer@ximian.com).
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	[TypeConverter(typeof (UnitConverter))]
#if NET_2_0
	[Serializable]
#else
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
	public struct Unit {
		UnitType type;
		double value;
		public static readonly Unit Empty;
		
		public Unit (double value, UnitType type)
		{
			if (value < -32768 || value > 32767)
				throw new ArgumentOutOfRangeException ("value");

			this.type = type;
			if (type == UnitType.Pixel)
				this.value = (int) value;
			else
				this.value = value;
		}

		public Unit (double value) : this (value, UnitType.Pixel)
		{
		}
		
		public Unit (int value) : this ((double) value, UnitType.Pixel)
		{
		}

		internal Unit (string value, char sep)
		{
			if (value == null || value == String.Empty){
				type = (UnitType) 0;
				this.value = 0.0;
				return;
			}

			// KLUDGE: the parser below should be rewritten
			if (value [0] == sep)
				value = "0" + value;
			
			int count = value.Length;
			int i = 0;
			int sign = 1;
			
			while (i < count && Char.IsWhiteSpace (value [i]))
				i++;
			if (value [i] == '-'){
				sign = -1;
				i++;
				if (!Char.IsDigit (value [i]))
					throw new ArgumentOutOfRangeException ("value");
			} else if (!Char.IsDigit (value [i])) {
				throw new FormatException ();
			}

			double dv = 0;
			for (; i < count; i++){
				char c = value [i];
				if (!Char.IsDigit (c))
					break;
				dv = dv * 10 + ((int) c) - ((int) '0');
			}
			dv *= sign;
			this.value = dv;
			dv = 0;
			if (i < count && value [i] == sep){
				i++;
				double factor = .1;
				for (; i < count; i++){
					char c = value [i];
					if (!Char.IsDigit (c))
						break;
					dv = dv + (((int) c) - ((int) '0')) * factor;
					factor = factor *.1;
				}
				this.value += dv;
			}
			
			while (i < count && Char.IsWhiteSpace (value [i]))
				i++;

			if (i == count){
				type = UnitType.Pixel;
				return;
			}

			if (value [i] == '%'){
				type = UnitType.Percentage;
				i++;
				while (i < count && Char.IsWhiteSpace (value [i]))
					i++;
				if (i != count)
					throw new ArgumentOutOfRangeException ("value");
				return;
			}
			
			int j = i;
			while (j < count && Char.IsLetter (value [j]))
				j++;
			string code = value.Substring (i, j-i);
			switch (code.ToLower (CultureInfo.InvariantCulture)){
			case "in": type = UnitType.Inch; break;
			case "cm": type = UnitType.Cm; break;
			case "mm": type = UnitType.Mm; break;
			case "pt": type = UnitType.Point; break;
			case "pc": type = UnitType.Pica; break;
			case "em": type = UnitType.Em; break;
			case "ex": type = UnitType.Ex; break;
			case "px":
				type = UnitType.Pixel;
				if (dv != 0)
					throw new FormatException ("Pixel units do not allow floating point values");
				break;
			default:
				throw new ArgumentOutOfRangeException ("value");
			}

			while (j < count && Char.IsWhiteSpace (value [j]))
				j++;
			if (j != count)
				throw new ArgumentOutOfRangeException ("value");
		}

		
		public Unit (string value) : this (value, '.')
		{
		}

		public Unit (string value, CultureInfo culture) : this (value, culture.NumberFormat.NumberDecimalSeparator [0])
		{
		}

		internal Unit (string value, CultureInfo culture, UnitType t) : this (value, '.')
		{
		}
		
		public bool IsEmpty {
			get {
				return type == 0;
			}
		}

		public UnitType Type {
			get {
				if (type == 0)
					return UnitType.Pixel;
				return type;
			}
		}

		public double Value {
			get {
				return value;
			}
		}
		
		public static Unit Parse (string s)
		{
			return new Unit (s);
		}

		public static System.Web.UI.WebControls.Unit Parse (string s, System.Globalization.CultureInfo culture)
		{
			return new Unit (s, culture);
		}
		

		public static Unit Percentage (double n)
		{
			return new Unit (n, UnitType.Percentage);
		}
		
		public static Unit Pixel (int n)
		{
			return new Unit (n);
		}
		
		public static Unit Point (int n)
		{
			return new Unit (n, UnitType.Point);
		}
				
		public override bool Equals (object obj)
		{
			if (obj is Unit){
				Unit other = (Unit) obj;
				return (other.type == type && other.value == value);
			}
			return false;
		}
		
		public override int GetHashCode ()
		{
			return Type.GetHashCode () ^ Value.GetHashCode ();
		}
		
		public static bool operator == (Unit left, Unit right)
		{
			return left.Type == right.Type && left.Value == right.Value;
		}

		public static bool operator != (Unit left, Unit right)
		{
			return left.Type != right.Type || left.Value != right.Value;
		}
		
		public static implicit operator Unit (int n)
		{
			return new Unit (n);
		}

		string GetExtension ()
		{
			switch (type){
			case UnitType.Pixel: return "px";
			case UnitType.Point: return "pt";
			case UnitType.Pica: return "pc";
			case UnitType.Inch: return "in";
			case UnitType.Mm: return "mm";
			case UnitType.Cm: return "cm";
			case UnitType.Percentage: return "%";
			case UnitType.Em: return "em";
			case UnitType.Ex: return "ex";
			default: return "";
			}
		}

		public string ToString (CultureInfo culture)
		{
			if (type == 0)
				return "";
			
			string ex = GetExtension ();
			
			return String.Format (culture, "{0}{1}", value, ex);
		}
			
		public override string ToString ()
		{
			return ToString (CultureInfo.InvariantCulture);
		}

#if NET_2_0
		public string ToString (IFormatProvider provider)
		{
			if (type == 0)
				return "";

			string ex = GetExtension ();

			return String.Format (provider, "{0}{1}", value, ex);
		}
#endif
	}

}
