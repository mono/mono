//
// System.Web.UI.WebControls.BaseCompareValidator
//
// Authors:
//	Chris Toshok (toshok@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.Threading;
using System.Globalization;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class BaseCompareValidator : BaseValidator
	{
		protected BaseCompareValidator ()
		{
		}

		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			if (RenderUplevel) {
				if (Page != null) {
					RegisterExpandoAttribute (ClientID, "type", Type.ToString ());

					switch (Type) {
						case ValidationDataType.Date:
							DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
							string pattern = dateTimeFormat.ShortDatePattern;
							string dateorder = (pattern.StartsWith ("y", true, Helpers.InvariantCulture) ? "ymd" : (pattern.StartsWith ("m", true, Helpers.InvariantCulture) ? "mdy" : "dmy"));
							RegisterExpandoAttribute (ClientID, "dateorder", dateorder);
							RegisterExpandoAttribute (ClientID, "cutoffyear", dateTimeFormat.Calendar.TwoDigitYearMax.ToString ());
							break;
						case ValidationDataType.Currency:
							NumberFormatInfo numberFormat = CultureInfo.CurrentCulture.NumberFormat;
							RegisterExpandoAttribute (ClientID, "decimalchar", numberFormat.CurrencyDecimalSeparator, true);
							RegisterExpandoAttribute (ClientID, "groupchar", numberFormat.CurrencyGroupSeparator, true);
							RegisterExpandoAttribute (ClientID, "digits", numberFormat.CurrencyDecimalDigits.ToString());
							RegisterExpandoAttribute (ClientID, "groupsize", numberFormat.CurrencyGroupSizes [0].ToString ());
							break;
					}
				}
			}

			base.AddAttributesToRender (w);
		}

		public static bool CanConvert (string text,
					       ValidationDataType type)
		{
			object value;

			return Convert (text, type, out value);
		}

		protected static bool Convert (string text,
					       ValidationDataType type,
					       out object value)
		{
            		return BaseCompareValidator.Convert(text, type, false, out value);
		}

		protected static bool Compare (string left, string right, ValidationCompareOperator op, ValidationDataType type)
		{
            		return BaseCompareValidator.Compare(left, false, right, false, op, type);	
		}

		protected override bool DetermineRenderUplevel ()
		{
			/* presumably the CompareValidator client side
			 * code makes use of newer dom/js stuff than
			 * the rest of the validators.  but ours
			 * doesn't for the moment, so let's just use
			 * our present implementation
			 */
			return base.DetermineRenderUplevel();
		}

		protected static string GetDateElementOrder ()
		{
			// I hope there's a better way to implement this...
			string pattern = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
			StringBuilder order = new StringBuilder();
			bool seen_date = false;
			bool seen_year = false;
			bool seen_month = false;

			pattern = pattern.ToLower (Helpers.InvariantCulture);

			for (int i = 0; i < pattern.Length; i ++) {
				char c = pattern[ i ];
				if (c != 'm' && c != 'd' && c != 'y')
					continue;

				if (c == 'm') {
					if (!seen_month) order.Append ("m");
					seen_month = true;
				} else if (c == 'y') {
					if (!seen_year) order.Append ("y");
					seen_year = true;
				} else /* (c == 'd') */ {
					if (!seen_date) order.Append ("d");
					seen_date = true;
				}
			}

			return order.ToString ();
		}

		protected static int GetFullYear (int two_digit_year)
		{
			/* This is an implementation that matches the
			 * docs on msdn, but MS doesn't seem to go by
			 * their docs (at least in 1.0). */
			int cutoff = CutoffYear;
			int twodigitcutoff = cutoff % 100;

			if (two_digit_year <= twodigitcutoff)
				return cutoff - twodigitcutoff + two_digit_year;
			else
				return cutoff - twodigitcutoff - 100 + two_digit_year;
		}

		[DefaultValue (false)]
		[Themeable (false)]
		public bool CultureInvariantValues {
			get { return ViewState.GetBool ("CultureInvariantValues", false); }
			set { ViewState ["CultureInvariantValues"] = value; }
		}
		
        	protected static int CutoffYear {
			get { return CultureInfo.CurrentCulture.Calendar.TwoDigitYearMax; }
		}


		[DefaultValue(ValidationDataType.String)]
		[Themeable (false)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public ValidationDataType Type {
			get { return ViewState ["Type"] == null ? ValidationDataType.String : (ValidationDataType) ViewState ["Type"]; }
			set { ViewState ["Type"] = value; }
		}

		public static bool CanConvert (string text, ValidationDataType type, bool cultureInvariant)
		{
            		object value;
            		return Convert(text, type, cultureInvariant, out value);
		}

		protected static bool Compare (string left, 
					       bool cultureInvariantLeftText, 
					       string right, 
					       bool cultureInvariantRightText, 
					       ValidationCompareOperator op, 
					       ValidationDataType type)
		{
			object lo, ro;

			if (!Convert(left, type, cultureInvariantLeftText, out lo))
				return false;

			/* DataTypeCheck is a unary operator that only
			 * depends on the lhs */
			if (op == ValidationCompareOperator.DataTypeCheck)
				return true;

			/* pretty crackladen, but if we're unable to
			 * convert the rhs to @type, the comparison
			 * succeeds */
			if (!Convert(right, type, cultureInvariantRightText, out ro))
				return true;

			int comp = ((IComparable)lo).CompareTo((IComparable)ro);

			switch (op) {
				case ValidationCompareOperator.Equal:
					return comp == 0;
				case ValidationCompareOperator.NotEqual:
					return comp != 0;
				case ValidationCompareOperator.LessThan:
					return comp < 0;
				case ValidationCompareOperator.LessThanEqual:
					return comp <= 0;
				case ValidationCompareOperator.GreaterThan:
					return comp > 0;
				case ValidationCompareOperator.GreaterThanEqual:
					return comp >= 0;
				default:
					return false;
			}
		}

		protected static bool Convert (string text, ValidationDataType type, bool cultureInvariant,out object value)
		{
			try {
				switch (type) {
					case ValidationDataType.String:
						value = text;
						return value != null;

					case ValidationDataType.Integer:
						IFormatProvider intFormatProvider = (cultureInvariant) ? NumberFormatInfo.InvariantInfo :
						NumberFormatInfo.CurrentInfo;
						value = Int32.Parse(text, intFormatProvider);
						return true;

					case ValidationDataType.Double:
						IFormatProvider doubleFormatProvider = (cultureInvariant) ? NumberFormatInfo.InvariantInfo :
						NumberFormatInfo.CurrentInfo;
						value = Double.Parse(text, doubleFormatProvider);
						return true;

					case ValidationDataType.Date:
                        
						IFormatProvider dateFormatProvider = (cultureInvariant) ? DateTimeFormatInfo.InvariantInfo :
						DateTimeFormatInfo.CurrentInfo;

						value = DateTime.Parse(text, dateFormatProvider);
						return true;

					case ValidationDataType.Currency:
						IFormatProvider currencyFormatProvider = (cultureInvariant) ? NumberFormatInfo.InvariantInfo :
						NumberFormatInfo.CurrentInfo;
						value = Decimal.Parse(text, NumberStyles.Currency, currencyFormatProvider);
						return true;

					default:
						value = null;
						return false;
				}
			} catch {
				value = null;
				return false;
			}
		}
	}
}







