
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
 * Class:     BaseCompareValidator
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public abstract class BaseCompareValidator: BaseValidator
	{
		protected BaseCompareValidator(): base()
		{
		}

		public static bool CanConvert(string text, ValidationDataType type)
		{
			object o = null;
			return Convert(text, type, out o);
		}

		[DefaultValue(ValidationDataType.String)]
		[WebCategory("Behavior")]
		[WebSysDescription("RangeValidator_Type")]
		public ValidationDataType Type
		{
			get
			{
				object o = ViewState["Type"];
				if(o!=null)
					return (ValidationDataType)o;
				return ValidationDataType.String;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(ValidationDataType), value))
					throw new ArgumentException();
				ViewState["Type"] = value;
			}
		}

		protected static int CutoffYear
		{
			get
			{
				return DateTimeFormatInfo.CurrentInfo.Calendar.TwoDigitYearMax;
			}
		}

		protected static int GetFullYear(int shortYear)
		{
			int century = DateTime.Today.Year - (DateTime.Today.Year % 100);
			if(century < CutoffYear)
			{
				return (shortYear + century);
			}
			return (shortYear + century - 100);
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(RenderUplevel)
			{
				writer.AddAttribute("type", PropertyConverter.EnumToString(typeof(ValidationDataType), Type));
				NumberFormatInfo currInfo = NumberFormatInfo.CurrentInfo;
				if(Type == ValidationDataType.Double)
				{
					writer.AddAttribute("decimalchar", currInfo.NumberDecimalSeparator);
					return;
				}
				if(Type == ValidationDataType.Currency)
				{
					writer.AddAttribute("decimalchar", currInfo.CurrencyDecimalSeparator);
					string grpSep = currInfo.CurrencyGroupSeparator;
					if(grpSep[0] == 0xA0)
					{
						grpSep = " ";
					}
					writer.AddAttribute("groupchar", grpSep);
					writer.AddAttribute("digits", currInfo.CurrencyDecimalDigits.ToString(NumberFormatInfo.InvariantInfo));
					return;
				}
				if(Type == ValidationDataType.Date)
				{
					writer.AddAttribute("cutoffyear", CutoffYear.ToString());
					writer.AddAttribute("century", ( DateTime.Today.Year - (DateTime.Today.Year % 100) ).ToString());
					return;
				}
			}
		}

		protected override bool DetermineRenderUplevel()
		{
			if(Type == ValidationDataType.Date && DateTimeFormatInfo.CurrentInfo.Calendar.GetType() != typeof(GregorianCalendar))
			{
				return false;
			}
			return base.DetermineRenderUplevel();
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		protected static bool Compare(string leftText, string rightText, ValidationCompareOperator op, ValidationDataType type)
		{
			object left = null, right = null;
			if(!Convert(leftText, type, out left))
			{
				return false;
			}
			if(op == ValidationCompareOperator.DataTypeCheck)
			{
				return true;
			}
			if(!Convert(rightText, type, out right))
			{
				return true;
			}
			int compareResult = 0;
			switch(type)
			{
				case ValidationDataType.String:
					compareResult = ((String)left).CompareTo(right);
					break;
				case ValidationDataType.Integer:
					compareResult = ((int)left).CompareTo(right);
					break;
				case ValidationDataType.Double:
					compareResult = ((Double)left).CompareTo(right);
					break;
				case ValidationDataType.Date:
					compareResult = ((DateTime)left).CompareTo(right);
					break;
				case ValidationDataType.Currency:
					compareResult = ((Decimal)left).CompareTo(right);
					break;
			}
			switch(op)
			{
				case ValidationCompareOperator.Equal:
					return (compareResult == 0);
				case ValidationCompareOperator.NotEqual:
					return (compareResult != 0);
				case ValidationCompareOperator.GreaterThan:
					return (compareResult > 0);
				case ValidationCompareOperator.GreaterThanEqual:
					return (compareResult >= 0);
				case ValidationCompareOperator.LessThan:
					return (compareResult < 0);
				case ValidationCompareOperator.LessThanEqual:
					return (compareResult <= 0);
			}
			return false;
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		protected static string GetDateElementOrder()
		{
			string pattern = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

			//TODO: What are the various possibilities?
			// I can think of only y*/M*/d*, d*/M*/y*, M*/d*/y*
			if(pattern.IndexOf('y') < pattern.IndexOf('M'))
			{
				return "ymd";
			}
			if(pattern.IndexOf('M') < pattern.IndexOf('d'))
			{
				return "mdy";
			}
			return "dmy";
		}

		static bool ConvertDate (string text, ValidationDataType type, ref object convertedValue)
		{
			//Console.WriteLine (DateTimeFormatInfo.CurrentInfo.Calendar.GetType ());
			// FIXME: sometime, somehow, the condition is true even when GetType () says
			// it's a GregorianCalendar.
			if (DateTimeFormatInfo.CurrentInfo.Calendar.GetType () != typeof (GregorianCalendar)) {
				convertedValue = DateTime.Parse (text);
				return true;
			}

			string order = GetDateElementOrder ();
			int date = 0, mth = 0, year = 0;
			string dateStr = null;
			string mthStr = null;
			string yearStr = null;
			Match match = Regex.Match (text, @"^\s*((\d{4})|(\d{2}))([\.\/-])(\d{1,2})\4(\d{1,2})\s*$");
			if (match.Success || order == "ymd") {
				dateStr = match.Groups [6].Value;
				mthStr = match.Groups [5].Value;
				if (match.Groups [2].Success)
					yearStr = match.Groups [2].Value;
				else
					yearStr = match.Groups [3].Value;
			} else {
				match = Regex.Match(text, @"^\s*(\d{1,2})([\.\/-])(\d{1,2})\2((\d{4}|\d{2}))\s*$");
				if (!match.Success)
					return false;

				if (order == "dmy") {
					dateStr = match.Groups [1].Value;
					mthStr  = match.Groups [3].Value;
					if (match.Groups [5].Success)
						yearStr = match.Groups [5].Value;
					else
						yearStr = match.Groups [6].Value;
				} else if (order == "mdy") {
					dateStr = match.Groups [3].Value;
					mthStr  = match.Groups [1].Value;
					if (match.Groups [5].Success)
						yearStr = match.Groups [5].Value;
					else
						yearStr = match.Groups [6].Value;
				}
			}

			if (dateStr == null || mthStr == null || yearStr == null) {
				return false;
			}

			CultureInfo inv = CultureInfo.InvariantCulture;
			date = Int32.Parse (dateStr, inv);
			mth  = Int32.Parse (mthStr, inv);
			year = Int32.Parse (yearStr, inv);
			year = (year < 100 ? GetFullYear (year) : year);
			if (date != 0 && mth != 0 && year != 0) {
				convertedValue = new DateTime  (year, mth, date);
				return true;
			}

			return false;
		}

		static bool ConvertDouble (string text, ValidationDataType type, ref object convertedValue)
		{
			Match match = Regex.Match (text, @"^\s*([-\+])?(\d+)?(\" +
						   NumberFormatInfo.CurrentInfo.NumberDecimalSeparator +
						   @"(\d+))?\s*$");

			if (!match.Success)
				return false;

			string sign     = (match.Groups [1].Success ? match.Groups [1].Value : "+");
			string decPart  = (match.Groups [2].Success ? match.Groups [2].Value : "0");
			string mantissa = (match.Groups [4].Success ? match.Groups [4].Value : "0");
			string num = sign + decPart + "." + mantissa;
			convertedValue  = Double.Parse (num, CultureInfo.InvariantCulture);
			return true;
		}

		static bool ConvertCurrency (string text, ValidationDataType type, ref object convertedValue)
		{
			string decSep = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
			string grpSep = NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator;
			int decDig = NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits;
			if (grpSep [0] == 0xA0)
				grpSep = " ";

			string [] patternArray = new string [5];
			patternArray [0] = "^\\s*([-\\+])?(((\\d+)\\";
			patternArray [1] = grpSep;
			patternArray [2] = @")*)(\d+)";
			if (decDig > 0) {
				string [] decPattern = new string [5];
				decPattern [0] = "(\\";
				decPattern [1] = decSep;
				decPattern [2] = @"(\d{1,";
				decPattern [3] = decDig.ToString (NumberFormatInfo.InvariantInfo);
				decPattern [4] = @"}))";
				patternArray [3] = String.Concat (decPattern);

			} else {
				patternArray [3] = String.Empty;
			}

			patternArray [4] = @"?\s*$";
			Match match = Regex.Match (text, String.Concat (patternArray));
			if (!match.Success)
				return false;

			StringBuilder sb = new StringBuilder ();
			sb.Append (match.Groups [1]);
			CaptureCollection cc = match.Groups [4].Captures;
			foreach (IEnumerable current in cc)
				sb.Append ((Capture) current);

			sb.Append (match.Groups [5]);
			if (decDig > 0) {
				sb.Append (".");
				sb.Append (match.Groups [7]);
			}

			convertedValue = Decimal.Parse (sb.ToString (), CultureInfo.InvariantCulture);
			return true;
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		protected static bool Convert (string text, ValidationDataType type, out object convertedValue)
		{
			CultureInfo inv = CultureInfo.InvariantCulture;
			convertedValue = null;
			try {
				switch(type) {
				case ValidationDataType.String:
					convertedValue = text;
					break;
				case ValidationDataType.Integer:
					convertedValue = Int32.Parse (text, inv);
					break;
				case ValidationDataType.Double:
					return ConvertDouble (text, type, ref convertedValue);
				case ValidationDataType.Date:
					return ConvertDate (text, type, ref convertedValue);
				case  ValidationDataType.Currency:
					return ConvertCurrency (text, type, ref convertedValue);
				}
			} catch (Exception) {
				convertedValue = null;
			}

			return (convertedValue != null);
		}
	}
}
