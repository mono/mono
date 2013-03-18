//
// DateTimeFormatEntry.cs
//
// Authors:
//  Jackson Harper (jackson@ximian.com)
//	Marek Safar  <marek.safar@gmail.com>
//
// (C) 2004, Novell, Inc (http://www.novell.com)
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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

using System.Text;
using System.Globalization;

namespace Mono.Tools.LocaleBuilder
{
	public class NumberFormatEntry : Entry
	{
		public string CurrencyDecimalDigits;
		public string CurrencyDecimalSeparator = ",";
		public string CurrencyGroupSeparator = ",";
		public string[] CurrencyGroupSizes = new string[Constants.GROUP_SIZE];
		public string CurrencyNegativePattern;
		public string CurrencyPositivePattern;
		public string CurrencySymbol;
		public string NaNSymbol;
		public string NegativeSign = "-";
		public int NumberDecimalDigits;
		public string NumberDecimalSeparator = ",";
		public string NumberGroupSeparator = ",";
		public string[] NumberGroupSizes = new string[Constants.GROUP_SIZE];
		public string NumberNegativePattern;
		public int PercentDecimalDigits;
		public string PercentDecimalSeparator = ",";
		public string PercentGroupSeparator = ",";
		public string[] PercentGroupSizes = new string[Constants.GROUP_SIZE];
		public string PercentNegativePattern;
		public string PercentPositivePattern;
		public string PercentSymbol = "%";
		public string PerMilleSymbol = "‰";
		public string InfinitySymbol = "Infinity";
		public string PositiveSign = "+";
		public DigitShapes DigitSubstitution = DigitShapes.None;
		public string[] NativeDigits = new string[10] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

		public int Row;

		public string NegativeInfinitySymbol
		{
			get
			{
				if (InfinitySymbol.StartsWith (PositiveSign))
					return NegativeSign + InfinitySymbol.Substring (1, InfinitySymbol.Length - 1);
	
				return NegativeSign + InfinitySymbol;
			}
		}

		public string PositiveInfinitySymbol
		{
			get
			{
				return InfinitySymbol;
			}
		}

		public void AppendTableRow (StringBuilder builder)
		{
			builder.Append ("\t{");

			builder.Append (EncodeStringIdx (CurrencyDecimalSeparator) + ", ");
			builder.Append (EncodeStringIdx (CurrencyGroupSeparator) + ", ");
			builder.Append (EncodeStringIdx (PercentDecimalSeparator) + ", ");
			builder.Append (EncodeStringIdx (PercentGroupSeparator) + ", ");
			builder.Append (EncodeStringIdx (NumberDecimalSeparator) + ", ");
			builder.Append (EncodeStringIdx (NumberGroupSeparator) + ", ");

			builder.Append (EncodeStringIdx (CurrencySymbol) + ", ");
			builder.Append (EncodeStringIdx (PercentSymbol) + ", ");
			builder.Append (EncodeStringIdx (NaNSymbol) + ", ");
			builder.Append (EncodeStringIdx (PerMilleSymbol) + ", ");
			builder.Append (EncodeStringIdx (NegativeInfinitySymbol) + ", ");
			builder.Append (EncodeStringIdx (PositiveInfinitySymbol) + ", ");

			builder.Append (EncodeStringIdx (NegativeSign) + ", ");
			builder.Append (EncodeStringIdx (PositiveSign) + ", ");

			builder.Append (CurrencyNegativePattern + ", ");
			builder.Append (CurrencyPositivePattern + ", ");
			builder.Append (PercentNegativePattern + ", ");
			builder.Append (PercentPositivePattern + ", ");
			builder.Append (NumberNegativePattern + ", ");

			builder.Append (CurrencyDecimalDigits + ", ");
			builder.Append (PercentDecimalDigits + ", ");
			builder.Append (NumberDecimalDigits + ", ");

			AppendGroupSizes (builder, CurrencyGroupSizes);
			builder.Append (", ");
			AppendGroupSizes (builder, PercentGroupSizes);
			builder.Append (", ");
			AppendGroupSizes (builder, NumberGroupSizes);

			builder.Append ('}');
		}

		static void AppendGroupSizes (StringBuilder builder, string[] gs)
		{
			builder.Append ('{');
			for (int i = 0; i < gs.Length; i++) {
				if (i > 0)
					builder.Append (", ");

				if (gs[i] == null)
					builder.Append (-1);
				else
					builder.Append (gs[i]);
			}

			builder.Append ('}');
		}

		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			AppendTableRow (builder);
			return builder.ToString ();
		}
	}
}

