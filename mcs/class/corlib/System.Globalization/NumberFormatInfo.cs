//
// System.Globalization.NumberFormatInfo.cs
//
// Author:
//   Derek Holden (dholden@draper.com)
//   Bob Smith    (bob@thestuff.net)
//   Mohammad DAMT (mdamt@cdl2000.com)
//
// (C) Derek Holden
// (C) Bob Smith     http://www.thestuff.net
// (c) 2003, PT Cakram Datalingga Duaribu   http://www.cdl2000.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

//
// NumberFormatInfo. One can only assume it is the class gotten
// back from a GetFormat() method from an IFormatProvider / 
// IFormattable implementer. There are some discrepencies with the
// ECMA spec and the SDK docs, surprisingly. See my conversation
// with myself on it at:
// http://lists.ximian.com/archives/public/mono-list/2001-July/000794.html
// 
// Other than that this is totally ECMA compliant.
//

using System.Runtime.InteropServices;

namespace System.Globalization {

	[ComVisible (true)]
	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	public sealed class NumberFormatInfo : ICloneable, IFormatProvider {

/* Keep in sync with object-internals.h */
		
#pragma warning disable 649
		private bool isReadOnly;
		// used for temporary storage. Used in InitPatterns ()
		string decimalFormats;
		string currencyFormats;
		string percentFormats;
		string digitPattern = "#";
		string zeroPattern = "0";
		
		// Currency Related Format Info
		private int currencyDecimalDigits;
		private string currencyDecimalSeparator;
		private string currencyGroupSeparator;
		private int[] currencyGroupSizes;
		private int currencyNegativePattern;
		private int currencyPositivePattern;
		private string currencySymbol;

		private string nanSymbol;
		private string negativeInfinitySymbol;
		private string negativeSign;

		// Number Related Format Info
		private int numberDecimalDigits;
		private string numberDecimalSeparator;
		private string numberGroupSeparator;
		private int[] numberGroupSizes;
		private int numberNegativePattern;

		// Percent Related Format Info
		private int percentDecimalDigits;
		private string percentDecimalSeparator;
		private string percentGroupSeparator;
		private int[] percentGroupSizes;
		private int percentNegativePattern;
		private int percentPositivePattern;
		private string percentSymbol;

		private string perMilleSymbol;
		private string positiveInfinitySymbol;
		private string positiveSign;
#pragma warning restore 649
		
#pragma warning disable 169
		string ansiCurrencySymbol;	// TODO, MS.NET serializes this.
		int m_dataItem;	// Unused, but MS.NET serializes this.
		bool m_useUserOverride; // Unused, but MS.NET serializes this.
		bool validForParseAsNumber; // Unused, but MS.NET serializes this.
		bool validForParseAsCurrency; // Unused, but MS.NET serializes this.
#pragma warning restore 169
		
		string[] nativeDigits = invariantNativeDigits;
		int digitSubstitution = 1; // DigitShapes.None.

		static readonly string [] invariantNativeDigits = new string [] {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};

		internal NumberFormatInfo (int lcid, bool read_only)
		{
			isReadOnly = read_only;

			//FIXME: should add more LCID
			// CultureInfo uses this one also.
			if (lcid != 0x007F)
				lcid = 0x007F;

			// The Invariant Culture Info ID.
			if (lcid == 0x007f) {
				// Currency Related Format Info
				currencyDecimalDigits =       2;
				currencyDecimalSeparator =    ".";
				currencyGroupSeparator =      ",";
				currencyGroupSizes =          new int[1] { 3 };
				currencyNegativePattern =     0;
				currencyPositivePattern =     0;
				currencySymbol =              "\u00a4";
				
				nanSymbol =                   "NaN";
				negativeInfinitySymbol =      "-Infinity";
				negativeSign =                "-";
				
				// Number Related Format Info
				numberDecimalDigits =         2;
				numberDecimalSeparator =      ".";
				numberGroupSeparator =        ",";
				numberGroupSizes =            new int[1] { 3 };
				numberNegativePattern =       1;
				
				// Percent Related Format Info
				percentDecimalDigits =        2;
				percentDecimalSeparator =     ".";
				percentGroupSeparator =       ",";
				percentGroupSizes =           new int[1] { 3 };
				percentNegativePattern =      0;
				percentPositivePattern =      0;
				percentSymbol=                "%";
				
				perMilleSymbol =              "\u2030";
				positiveInfinitySymbol =      "Infinity";
				positiveSign =                "+";
			}
		}

		internal NumberFormatInfo (bool read_only) : this (0x007f, read_only)
		{
		}
		
		public NumberFormatInfo () : this (false)
		{
		}

		// this is called by mono/mono/metadata/locales.c
#pragma warning disable 169		
		void InitPatterns ()
		{
			string [] partOne, partTwo;
			string [] posNeg = decimalFormats.Split (new char [1] {';'}, 2);
			
			if (posNeg.Length == 2) {
				
				partOne = posNeg [0].Split (new char [1] {'.'}, 2);
											
				if (partOne.Length == 2) {
					// assumed same for both positive and negative
					// decimal digit side
					numberDecimalDigits = 0;					
					for (int i = 0; i < partOne [1].Length; i ++) {						
						if (partOne [1][i] == digitPattern [0]) {
							numberDecimalDigits ++;							
						} else
							break;						
					}

					// decimal grouping side
					partTwo = partOne [0].Split (',');
					if (partTwo.Length > 1) {
						numberGroupSizes = new int [partTwo.Length - 1];
						for (int i = 0; i < numberGroupSizes.Length; i ++) {
							string pat = partTwo [i + 1];
							numberGroupSizes [i] = pat.Length;
						}
					} else {
						numberGroupSizes = new int [1] { 0 };
					}

					if (posNeg [1].StartsWith ("(") && posNeg [1].EndsWith (")")) {
						numberNegativePattern = 0;
					} else if (posNeg [1].StartsWith ("- ")) {
						numberNegativePattern = 2;
					} else if (posNeg [1].StartsWith ("-")) {
						numberNegativePattern = 1;
					} else if (posNeg [1].EndsWith (" -")) {
						numberNegativePattern = 4;
					} else if (posNeg [1].EndsWith ("-")) {
						numberNegativePattern = 3;
					} else {
						numberNegativePattern = 1;
					}
				}
			}

			posNeg = currencyFormats.Split (new char [1] {';'}, 2);			
			if (posNeg.Length == 2) {
				partOne = posNeg [0].Split (new char [1] {'.'}, 2);
				
				if (partOne.Length == 2) {
					// assumed same for both positive and negative
					// decimal digit side
					currencyDecimalDigits = 0;
					for (int i = 0; i < partOne [1].Length; i ++) {
						if (partOne [1][i] == zeroPattern [0])
							currencyDecimalDigits ++;
						else
							break;
					}

					// decimal grouping side
					partTwo = partOne [0].Split (',');
					if (partTwo.Length > 1) {						
						currencyGroupSizes = new int [partTwo.Length - 1];
						for (int i = 0; i < currencyGroupSizes.Length; i ++) {
							string pat = partTwo [i + 1];
							currencyGroupSizes [i] = pat.Length;
						}
					} else {
						currencyGroupSizes = new int [1] { 0 };
					}

					if (posNeg [1].StartsWith ("(\u00a4 ") && posNeg [1].EndsWith (")")) {
						currencyNegativePattern = 14;
					} else if (posNeg [1].StartsWith ("(\u00a4") && posNeg [1].EndsWith (")")) {
						currencyNegativePattern = 0;
					} else if (posNeg [1].StartsWith ("\u00a4 ") && posNeg [1].EndsWith ("-")) {
						currencyNegativePattern = 11;
					} else if (posNeg [1].StartsWith ("\u00a4") && posNeg [1].EndsWith ("-")) {
						currencyNegativePattern = 3;
					} else if (posNeg [1].StartsWith ("(") && posNeg [1].EndsWith (" \u00a4")) {
						currencyNegativePattern = 15;
					} else if (posNeg [1].StartsWith ("(") && posNeg [1].EndsWith ("\u00a4")) {
						currencyNegativePattern = 4;
					} else if (posNeg [1].StartsWith ("-") && posNeg [1].EndsWith (" \u00a4")) {
						currencyNegativePattern = 8;
					} else if (posNeg [1].StartsWith ("-") && posNeg [1].EndsWith ("\u00a4")) {
						currencyNegativePattern = 5;
					} else if (posNeg [1].StartsWith ("-\u00a4 ")) {
						currencyNegativePattern = 9;
					} else if (posNeg [1].StartsWith ("-\u00a4")) {
						currencyNegativePattern = 1;
					} else if (posNeg [1].StartsWith ("\u00a4 -")) {
						currencyNegativePattern = 12;
					} else if (posNeg [1].StartsWith ("\u00a4-")) {
						currencyNegativePattern = 2;
					} else if (posNeg [1].EndsWith (" \u00a4-")) {
						currencyNegativePattern = 10;
					} else if (posNeg [1].EndsWith ("\u00a4-")) {
						currencyNegativePattern = 7;
					} else if (posNeg [1].EndsWith ("- \u00a4")) {
						currencyNegativePattern = 13;
					} else if (posNeg [1].EndsWith ("-\u00a4")) {
						currencyNegativePattern = 6;
					} else {
						currencyNegativePattern = 0;
					}
					
					if (posNeg [0].StartsWith ("\u00a4 ")) {
						currencyPositivePattern = 2;
					} else if (posNeg [0].StartsWith ("\u00a4")) {
						currencyPositivePattern = 0;
					} else if (posNeg [0].EndsWith (" \u00a4")) {
						currencyPositivePattern = 3;
					} else if (posNeg [0].EndsWith ("\u00a4")) {
						currencyPositivePattern = 1; 
					} else {
						currencyPositivePattern = 0;
					}
				}
			}

			// we don't have percentNegativePattern in CLDR so 
			// the percentNegativePattern are just guesses
			if (percentFormats.StartsWith ("%")) {
				percentPositivePattern = 2;
				percentNegativePattern = 2;
			} else if (percentFormats.EndsWith (" %")) {
				percentPositivePattern = 0;
				percentNegativePattern = 0;
			} else if (percentFormats.EndsWith ("%")) {
				percentPositivePattern = 1;
				percentNegativePattern = 1;
			} else {
				percentPositivePattern = 0;
				percentNegativePattern = 0;
			}

			partOne = percentFormats.Split (new char [1] {'.'}, 2);
			
			if (partOne.Length == 2) {
				// assumed same for both positive and negative
				// decimal digit side
				percentDecimalDigits = 0;
				for (int i = 0; i < partOne [1].Length; i ++) {
					if (partOne [1][i] == digitPattern [0])
						percentDecimalDigits ++;
					else
						break;
				}

				// percent grouping side
				partTwo = partOne [0].Split (',');
				if (partTwo.Length > 1) {
					percentGroupSizes = new int [partTwo.Length - 1];
					for (int i = 0; i < percentGroupSizes.Length; i ++) {
						string pat = partTwo [i + 1];
						percentGroupSizes [i] = pat.Length;
					}
				} else {
					percentGroupSizes = new int [1] { 0 };
				}
			}
			
		}
#pragma warning restore 169

		// =========== Currency Format Properties =========== //

		public int CurrencyDecimalDigits {
			get {
				return currencyDecimalDigits;
			}
			
			set {
				if (value < 0 || value > 99) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 99");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				currencyDecimalDigits = value;
			}
		}

		public string CurrencyDecimalSeparator {
			get {
				return currencyDecimalSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				currencyDecimalSeparator = value;
			}
		}


		public string CurrencyGroupSeparator {
			get {
				return currencyGroupSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				currencyGroupSeparator = value;
			}
		}

		public int[] CurrencyGroupSizes {
			get {
				return (int []) RawCurrencyGroupSizes.Clone ();
			}
			
			set {
				RawCurrencyGroupSizes = value;
			}
		}

		internal int[] RawCurrencyGroupSizes {
			get {
				return currencyGroupSizes;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				if (value.Length == 0) {
					currencyGroupSizes = EmptyArray<int>.Value;
					return;
				}
				
				// All elements except last need to be in range 1 - 9, last can be 0.
				int last = value.Length - 1;

				for (int i = 0; i < last; i++)
					if (value[i] < 1 || value[i] > 9)
						throw new ArgumentOutOfRangeException
						("One of the elements in the array specified is not between 1 and 9");

				if (value[last] < 0 || value[last] > 9)
					throw new ArgumentOutOfRangeException
					("Last element in the array specified is not between 0 and 9");
				
				currencyGroupSizes = (int[]) value.Clone();
			}
		}

		public int CurrencyNegativePattern {
			get {
				// See ECMA NumberFormatInfo page 8
				return currencyNegativePattern;
			}
			
			set {
				if (value < 0 || value > 15) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 15");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				currencyNegativePattern = value;
			}
		}

		public int CurrencyPositivePattern {
			get {
				// See ECMA NumberFormatInfo page 11 
				return currencyPositivePattern;
			}
			
			set {
				if (value < 0 || value > 3) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 3");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				currencyPositivePattern = value;
			}
		}

		public string CurrencySymbol {
			get {
				return currencySymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				currencySymbol = value;
			}
		}

		// =========== Static Read-Only Properties =========== //

		public static NumberFormatInfo CurrentInfo {
			get {
				NumberFormatInfo nfi = (NumberFormatInfo) System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat;
				nfi.isReadOnly = true;
				return nfi;
			}		       
		}

		public static NumberFormatInfo InvariantInfo {
			get {
				// This uses invariant info, which is same as in the constructor
				NumberFormatInfo nfi = new NumberFormatInfo (true);
				return nfi;
			}
		}

		public bool IsReadOnly {
			get {
				return isReadOnly;
			}
		}



		public string NaNSymbol {
			get {
				return nanSymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				nanSymbol = value;
			}
		}
		
#if !NET_2_1
		[MonoNotSupported ("We don't have native digit info")]
		[ComVisible (false)]
		public string [] NativeDigits {
			get { return nativeDigits; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value.Length != 10)
					throw new ArgumentException ("Argument array length must be 10");
				foreach (string s in value)
					if (String.IsNullOrEmpty (s))
						throw new ArgumentException ("Argument array contains one or more null strings");
				nativeDigits = value;
			}
		}

		[MonoNotSupported ("We don't have native digit info")]
		[ComVisible (false)]
		public DigitShapes DigitSubstitution {
			get { return (DigitShapes) digitSubstitution; }
			set { digitSubstitution = (int) value; }
		}
#endif
		
		public string NegativeInfinitySymbol {
			get {
				return negativeInfinitySymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				negativeInfinitySymbol = value;
			}
		}

		public string NegativeSign {
			get {
				return negativeSign;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				negativeSign = value;
			}
		}
		
		// =========== Number Format Properties =========== //

		public int NumberDecimalDigits {
			get {
				return numberDecimalDigits;
			}
			
			set {
				if (value < 0 || value > 99) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 99");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				numberDecimalDigits = value;
			}
		}		

		public string NumberDecimalSeparator {
			get {
				return numberDecimalSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				numberDecimalSeparator = value;
			}
		}


		public string NumberGroupSeparator {
			get {
				return numberGroupSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				numberGroupSeparator = value;
			}
		}

		public int[] NumberGroupSizes {
			get {
				return (int []) RawNumberGroupSizes.Clone ();
			}
			
			set {
				RawNumberGroupSizes = value;
			}
		}

		internal int[] RawNumberGroupSizes {
			get {
				return numberGroupSizes;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				if (value.Length == 0) {
					numberGroupSizes = EmptyArray<int>.Value;
					return;
				}
				// All elements except last need to be in range 1 - 9, last can be 0.
				int last = value.Length - 1;

				for (int i = 0; i < last; i++)
					if (value[i] < 1 || value[i] > 9)
						throw new ArgumentOutOfRangeException
						("One of the elements in the array specified is not between 1 and 9");

				if (value[last] < 0 || value[last] > 9)
					throw new ArgumentOutOfRangeException
					("Last element in the array specified is not between 0 and 9");
				
				numberGroupSizes = (int[]) value.Clone();
			}
		}

		public int NumberNegativePattern {
			get {
				// See ECMA NumberFormatInfo page 27
				return numberNegativePattern;
			}
			
			set {
				if (value < 0 || value > 4) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 15");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				numberNegativePattern = value;
			}
		}

		// =========== Percent Format Properties =========== //

		public int PercentDecimalDigits {
			get {
				return percentDecimalDigits;
			}
			
			set {
				if (value < 0 || value > 99) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 99");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				percentDecimalDigits = value;
			}
		}

		public string PercentDecimalSeparator {
			get {
				return percentDecimalSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				percentDecimalSeparator = value;
			}
		}


		public string PercentGroupSeparator {
			get {
				return percentGroupSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				percentGroupSeparator = value;
			}
		}

		public int[] PercentGroupSizes {
			get {
				return (int []) RawPercentGroupSizes.Clone ();
			}
			
			set {
				RawPercentGroupSizes = value;
			}
		}

		internal int[] RawPercentGroupSizes {
			get {
				return percentGroupSizes;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				if (this == CultureInfo.CurrentCulture.NumberFormat)
					throw new Exception ("HERE the value was modified");
				
				if (value.Length == 0) {
					percentGroupSizes = EmptyArray<int>.Value;
					return;
				}

				// All elements except last need to be in range 1 - 9, last can be 0.
				int last = value.Length - 1;

				for (int i = 0; i < last; i++)
					if (value[i] < 1 || value[i] > 9)
						throw new ArgumentOutOfRangeException
						("One of the elements in the array specified is not between 1 and 9");

				if (value[last] < 0 || value[last] > 9)
					throw new ArgumentOutOfRangeException
					("Last element in the array specified is not between 0 and 9");
				
				percentGroupSizes = (int[]) value.Clone();
			}
		}

		public int PercentNegativePattern {
			get {
				// See ECMA NumberFormatInfo page 8
				return percentNegativePattern;
			}
			
			set {
				if (value < 0 || value > 2) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 15");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				percentNegativePattern = value;
			}
		}

		public int PercentPositivePattern {
			get {
				// See ECMA NumberFormatInfo page 11 
				return percentPositivePattern;
			}
			
			set {
				if (value < 0 || value > 2) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 3");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				percentPositivePattern = value;
			}
		}

		public string PercentSymbol {
			get {
				return percentSymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				percentSymbol = value;
			}
		}

		public string PerMilleSymbol {
			get {
				return perMilleSymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				perMilleSymbol = value;
			}
		}

		public string PositiveInfinitySymbol {
			get {
				return positiveInfinitySymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				positiveInfinitySymbol = value;
			}
		}

		public string PositiveSign {
			get {
				return positiveSign;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (isReadOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				positiveSign = value;
			}
		}

		public object GetFormat (Type formatType) 
		{
			return (formatType == typeof (NumberFormatInfo)) ? this : null;
		}
		
		public object Clone () 
		{
			NumberFormatInfo clone = (NumberFormatInfo) MemberwiseClone();
			// clone is not read only
			clone.isReadOnly = false;
			return clone;
		}

		public static NumberFormatInfo ReadOnly (NumberFormatInfo nfi)
		{
			NumberFormatInfo copy = (NumberFormatInfo)nfi.Clone();
			copy.isReadOnly = true;
			return copy;
		}			

		public static NumberFormatInfo GetInstance(IFormatProvider formatProvider)
		{
			if (formatProvider != null) {
				NumberFormatInfo nfi;
				nfi = (NumberFormatInfo)formatProvider.GetFormat(typeof(NumberFormatInfo));
				if (nfi != null)
					return nfi;
			}
			
			return CurrentInfo;
		}
	}
}
