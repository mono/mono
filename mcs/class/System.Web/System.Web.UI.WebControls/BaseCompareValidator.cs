//
// System.Web.UI.WebControls.BaseCompareValidator
//
// Authors:
//	Chris Toshok (toshok@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class BaseCompareValidator : BaseValidator {

		ValidationDataType type;

#if NET_2_0
		protected
#else
		public
#endif
		BaseCompareValidator ()
		{
			type = ValidationDataType.String;
		}

		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			if (RenderUplevel) {
				w.AddAttribute ("datatype", type.ToString());
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
			try {
				switch (type) {
				case ValidationDataType.String:
					value = text;
					return value != null;
				case ValidationDataType.Integer:
					value = Int32.Parse (text);
					return true;
				case ValidationDataType.Double:
					value = Double.Parse(text);
					return true;
				case ValidationDataType.Date:
					value = DateTime.Parse(text);
					return true;
				case ValidationDataType.Currency:
					value = Decimal.Parse(text, NumberStyles.Currency);
					return true;
				default:
					value = null;
					return false;
				}
			}
			catch {
				value = null;
				return false;
			}

		}

		protected static bool Compare (string left,
					       string right,
					       ValidationCompareOperator op,
					       ValidationDataType type)
		{
			object lo, ro;

			if (!Convert (left, type, out lo))
				return false;

			/* DataTypeCheck is a unary operator that only
			 * depends on the lhs */
			if (op == ValidationCompareOperator.DataTypeCheck)
				return true;

			/* pretty crackladen, but if we're unable to
			 * convert the rhs to @type, the comparison
			 * succeeds */
			if (!Convert (right, type, out ro))
				return true;

			int comp = ((IComparable)lo).CompareTo ((IComparable)ro);

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

			pattern = pattern.ToLower ();

			for (int i = 0; i < pattern.Length; i ++) {
				char c = pattern[ i ];
				if (c != 'm' && c != 'd' && c != 'y')
					continue;

				if (c == 'm') {
					if (!seen_month) order.Append ("m");
					seen_month = true;
				}
				else if (c == 'y') {
					if (!seen_year) order.Append ("y");
					seen_year = true;
				}
				else /* (c == 'd') */ {
					if (!seen_date) order.Append ("d");
					seen_date = true;
				}
			}

			return order.ToString ();
		}

		protected static int GetFullYear (int two_digit_year)
		{
#if NET_2_0
			/* This is an implementation that matches the
			 * docs on msdn, but MS doesn't seem to go by
			 * their docs (at least in 1.0). */
			int cutoff = CutoffYear;
			int twodigitcutoff = cutoff % 100;

			if (two_digit_year <= twodigitcutoff) {
				return cutoff - twodigitcutoff + two_digit_year;
			}
			else {
				return cutoff - twodigitcutoff - 100 + two_digit_year;
			}
#else
			/* This is the broken implementation in 1.0 */
			int cutoff = CutoffYear;
			int twodigitcutoff = cutoff % 100;

			return cutoff - twodigitcutoff + two_digit_year;
#endif
		}

#if NET_2_0
		[MonoTODO]
		[DefaultValue (false)]
		[Themeable (false)]
		public bool CultureInvariantValues {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif

		protected static int CutoffYear {
			get {
				return CultureInfo.CurrentCulture.Calendar.TwoDigitYearMax;
			}
		}

		[DefaultValue(ValidationDataType.String)]
#if NET_2_0
		[Themeable (false)]
#endif
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public ValidationDataType Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}

#if NET_2_0
		[MonoTODO]
		public static bool CanConvert (string text, 
					       ValidationDataType type, 
					       bool cultureInvariant)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool Compare (string leftText, 
					       bool cultureInvariantLeftText, 
					       string rightText, 
					       bool cultureInvariantRightText, 
					       ValidationCompareOperator op, 
					       ValidationDataType type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static bool Convert (string text,
					       ValidationDataType type,
					       bool cultureInvariant,
					       out object value)
		{
			throw new NotImplementedException ();
		}
#endif
	}

}

