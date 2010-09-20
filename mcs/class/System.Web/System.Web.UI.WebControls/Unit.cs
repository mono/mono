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
using System.Web.Util;

namespace System.Web.UI.WebControls {

	[TypeConverter(typeof (UnitConverter))]
#if NET_2_0
	[Serializable]
#else
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
	public struct Unit {
		enum ParsingStage
		{
			Trim,
			SignOrSep,
			DigitOrSep,
			DigitOrUnit,
			Unit
		}
		
		UnitType type;
		double value;
		bool valueSet;
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
			valueSet = true;
		}

		public Unit (double value) : this (value, UnitType.Pixel)
		{
		}
		
		public Unit (int value) : this ((double) value, UnitType.Pixel)
		{
		}

		internal Unit (string input, char sep)
		{
			if (input == null || input == String.Empty){
				type = (UnitType) 0;
				value = 0.0;
				return;
			}

			value = 0.0;
			double dv = 0, factor = .1;
			int i = 0;
			int count = input.Length;
			int sign = 1, unitStart = -1, unitLen = 0, wsCount = 0;
			char c;
			ParsingStage ps = ParsingStage.Trim;
			bool done = false, haveSep = false, haveDigits = false, isWhiteSpace;

			while (!done && i < count) {
				c = input [i];

				switch (ps) {
					case ParsingStage.Trim:
						if (Char.IsWhiteSpace (c)) {
							i++;
							continue;
						}
						ps = ParsingStage.SignOrSep;
						continue;

					case ParsingStage.SignOrSep:
						wsCount = 0;
						if (c == '-') {
							sign = -1;
							i++;
							ps = ParsingStage.DigitOrSep;
							continue;
						}

						if (c == sep) {
							i++;
							haveSep = true;
							ps = ParsingStage.DigitOrUnit;
							dv = 0;
							continue;
						}

						if (Char.IsDigit (c)) {
							ps = ParsingStage.DigitOrSep;
							continue;
						}
	  
						throw new FormatException ();

					case ParsingStage.DigitOrSep:
						if (Char.IsDigit (c)) {
							dv = dv * 10 + ((int) c) - ((int)'0');
							i++;
							haveDigits = true;
							continue;
						}

						if (c == sep) {
							if (wsCount > 0)
								throw new ArgumentOutOfRangeException ("input");
	    
							i++;
							haveSep = true;
							value = dv * sign;
							dv = 0;
							ps = ParsingStage.DigitOrUnit;
							continue;
						}

						isWhiteSpace = Char.IsWhiteSpace (c);
						if (isWhiteSpace || c == '%' || Char.IsLetter (c)) {
							if (isWhiteSpace) {
								if (!haveDigits)
									throw new ArgumentOutOfRangeException ("input");
								wsCount++;
								i++;
								continue;
							}

							value = dv * sign;
							dv = 0;
							unitStart = i;
	    
							if (haveSep) {
								haveDigits = false;
								ps = ParsingStage.DigitOrUnit;
							} else
								ps = ParsingStage.Unit;
							wsCount = 0;
							continue;
						}
	  
						throw new FormatException ();
	  
					case ParsingStage.DigitOrUnit:
						if (c == '%') {
							unitStart = i;
							unitLen = 1;
							done = true;
							continue;
						}

						isWhiteSpace = Char.IsWhiteSpace (c);
						if (isWhiteSpace || Char.IsLetter (c)) {
							if (isWhiteSpace) {
								wsCount++;
								i++;
								continue;
							}
	    
							ps = ParsingStage.Unit;
							unitStart = i;
							continue;
						}

						if (Char.IsDigit (c)) {
							if (wsCount > 0)
								throw new ArgumentOutOfRangeException ();
	    
							dv = dv + (((int) c) - ((int) '0')) * factor;
							factor = factor *.1;
							i++;
							continue;
						}
	  
						throw new FormatException ();

					case ParsingStage.Unit:
						if (c == '%' || Char.IsLetter (c)) {
							i++;
							unitLen++;
							continue;
						}

						if (unitLen == 0 && Char.IsWhiteSpace (c)) {
							i++;
							unitStart++;
							continue;
						}
	  
						done = true;
						break;
				}
			}

			value += dv * sign;
			if (unitStart >= 0) {
				int unitTail = unitStart + unitLen;
				if (unitTail < count) {
					for (int j = unitTail; j < count; j++) {
						if (!Char.IsWhiteSpace (input [j]))
							throw new ArgumentOutOfRangeException ("input");
					}
				}

				if (unitLen == 1 && input [unitStart] == '%')
					type = UnitType.Percentage;
				else {
					switch (input.Substring (unitStart, unitLen).ToLower (Helpers.InvariantCulture)) {
						case "in": type = UnitType.Inch; break;
						case "cm": type = UnitType.Cm; break;
						case "mm": type = UnitType.Mm; break;
						case "pt": type = UnitType.Point; break;
						case "pc": type = UnitType.Pica; break;
						case "em": type = UnitType.Em; break;
						case "ex": type = UnitType.Ex; break;
						case "px":
							type = UnitType.Pixel;
							break;
						default:
							throw new ArgumentOutOfRangeException ("value");
					}
				}
			} else
				type = UnitType.Pixel;

			if (haveSep && type == UnitType.Pixel)
				throw new FormatException ("Pixel units do not allow floating point values");
			valueSet = true;
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
				return (other.type == type && other.value == value && valueSet == other.valueSet);
			}
			return false;
		}
		
		public override int GetHashCode ()
		{
			return Type.GetHashCode () ^ Value.GetHashCode ();
		}
		
		public static bool operator == (Unit left, Unit right)
		{
			return left.Type == right.Type && left.Value == right.Value && left.valueSet == right.valueSet;
		}

		public static bool operator != (Unit left, Unit right)
		{
			return left.Type != right.Type || left.Value != right.Value || left.valueSet != right.valueSet;
		}
		
		public static implicit operator Unit (int n)
		{
			return new Unit (n);
		}

		internal static string GetExtension (UnitType type)
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
				default: return String.Empty;
			}
		}

		public string ToString (CultureInfo culture)
		{
			if (type == 0)
				return String.Empty;
			
			string ex = GetExtension (type);
			
			return value.ToString (culture) + ex;
		}
			
		public override string ToString ()
		{
			return ToString (Helpers.InvariantCulture);
		}

#if NET_2_0
		public string ToString (IFormatProvider provider)
		{
			if (type == 0)
				return String.Empty;

			string ex = GetExtension (type);

			return value.ToString (provider) + ex;
		}
#endif
	}

}
