//
// Tests for System.Web.UI.WebControls.BaseCompareValidator 
//
// Author:
//	Chris Toshok (toshok@novell.com)
//      Yoni Klain   (Yonik@mainsoft.com)
//

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

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.UI.WebControls
{
	class BaseCompareValidatorPoker : BaseCompareValidator {
		public BaseCompareValidatorPoker ()
		{
			TrackViewState (); 
		}
		
		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}

		public void CheckControlProperties ()
		{
			ControlPropertiesValid ();
		}

		protected override bool EvaluateIsValid ()
		{
			return true;
		}

		public new bool Compare(string leftText,
					string rightText,
					ValidationCompareOperator op,
					ValidationDataType type)
		{
			return BaseCompareValidator.Compare (leftText,
							     rightText,
							     op, type);
		}

		public new bool Convert (string text,
					 ValidationDataType type,
					 out object value)
		{
			return BaseCompareValidator.Convert (text, type, out value);
		}

		public new bool CanConvert(string text,
					   ValidationDataType type)
		{
			return BaseCompareValidator.CanConvert (text, type);
		}

		public new string GetDateElementOrder ()
		{
			return BaseCompareValidator.GetDateElementOrder();
		}

		public new int GetFullYear (int two_digit_year)
		{
			return BaseCompareValidator.GetFullYear (two_digit_year);
		}

		public int GetCutoffYear ()
		{
			return BaseCompareValidator.CutoffYear;
		}
	}
	
	[TestFixture]
	public class BaseCompareValidatorTest : ValidatorTest
	{

		[Test]
		public void DefaultProperties ()
		{
			BaseCompareValidatorPoker p = new BaseCompareValidatorPoker ();
			
			Assert.AreEqual (ValidationDataType.String, p.Type, "CultureInvariantValues");
#if NET_2_0
			Assert.AreEqual (false, p.CultureInvariantValues, "CultureInvariantValues");
#endif 
		}

		[Test]
		public void AssignProperties ()
		{
			BaseCompareValidatorPoker p = new BaseCompareValidatorPoker ();
			
			p.Type = ValidationDataType.Double;
			Assert.AreEqual (ValidationDataType.Double, p.Type, "CultureInvariantValues");
#if NET_2_0
			p.CultureInvariantValues = true;
			Assert.AreEqual (true, p.CultureInvariantValues, "CultureInvariantValues");
#endif
		}

		[Test]
		public void ViewState ()
		{
			BaseCompareValidatorPoker p = new BaseCompareValidatorPoker ();

			p.Type = ValidationDataType.Double;
#if NET_2_0
			p.CultureInvariantValues = true;
#endif

			BaseCompareValidatorPoker copy = new BaseCompareValidatorPoker ();
			copy.LoadState (p.SaveState ());

			Assert.AreEqual (ValidationDataType.Double, copy.Type, "A1");
#if NET_2_0
			Assert.AreEqual (true, copy.CultureInvariantValues, "A1");
#endif
		}

		[Test]
		public void CanConvert ()
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;

			try {
				CultureInfo ci = CultureInfo.GetCultureInfo ("en-US");
				Thread.CurrentThread.CurrentCulture = ci;
				Thread.CurrentThread.CurrentUICulture = ci;
				RunCanConvertTests ();
			} finally {
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}

		void RunCanConvertTests ()
		{
			BaseCompareValidatorPoker p = new BaseCompareValidatorPoker ();

			/* an integer constant */
			Assert.IsTrue (p.CanConvert ("10", ValidationDataType.String), "B1");
			Assert.IsTrue (p.CanConvert ("10", ValidationDataType.Integer), "B2");
			Assert.IsTrue (p.CanConvert ("10", ValidationDataType.Double), "B3");
			Assert.IsFalse (p.CanConvert ("10", ValidationDataType.Date), "B4");
			Assert.IsTrue (p.CanConvert ("10", ValidationDataType.Currency), "B5");

			/* a double constant */
			Assert.IsTrue (p.CanConvert ("10.5", ValidationDataType.String), "B6");
			Assert.IsFalse (p.CanConvert ("10.5", ValidationDataType.Integer), "B7");
			Assert.IsTrue (p.CanConvert ("10.5", ValidationDataType.Double), "B8");
// find a way to do this in a Culture independent way
//			Assert.IsFalse (p.CanConvert ("10.5", ValidationDataType.Date), "B9");
			Assert.IsTrue (p.CanConvert ("10.5", ValidationDataType.Currency), "B10");

			/* a string constant */
			Assert.IsTrue (p.CanConvert ("hi", ValidationDataType.String), "B11");
			Assert.IsFalse (p.CanConvert ("hi", ValidationDataType.Integer), "B12");
			Assert.IsFalse (p.CanConvert ("hi", ValidationDataType.Double), "B13");
			Assert.IsFalse (p.CanConvert ("hi", ValidationDataType.Date), "B14");
			Assert.IsFalse (p.CanConvert ("hi", ValidationDataType.Currency), "B15");

			/* a currency constant? */
			Assert.IsTrue (p.CanConvert ("10.50", ValidationDataType.String), "B16");
			Assert.IsFalse (p.CanConvert ("10.50", ValidationDataType.Integer), "B17");
			Assert.IsTrue (p.CanConvert ("10.50", ValidationDataType.Double), "B18");
			Assert.IsFalse (p.CanConvert ("10.50", ValidationDataType.Date), "B19");
			Assert.IsTrue (p.CanConvert ("10.50", ValidationDataType.Currency), "B20");

			/* a date constant */
			DateTime dt = new DateTime (2005, 7, 19);
			string dt_str = dt.ToString("d");
			Assert.IsTrue  (p.CanConvert (dt_str, ValidationDataType.String), "B21");
			Assert.IsFalse (p.CanConvert (dt_str, ValidationDataType.Integer), "B22");
			Assert.IsFalse (p.CanConvert (dt_str, ValidationDataType.Double), "B23");
			Assert.IsTrue  (p.CanConvert (dt_str, ValidationDataType.Date), "B24");
			Assert.IsFalse (p.CanConvert (dt_str, ValidationDataType.Currency), "B25");

			/* null? */
			Assert.IsFalse (p.CanConvert (null, ValidationDataType.String), "B26");
			Assert.IsFalse (p.CanConvert (null, ValidationDataType.Integer), "B27");
			Assert.IsFalse (p.CanConvert (null, ValidationDataType.Double), "B28");
			Assert.IsFalse (p.CanConvert (null, ValidationDataType.Date), "B29");
			Assert.IsFalse (p.CanConvert (null, ValidationDataType.Currency), "B30");
		}

		[Test]
		public void Convert ()
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;

			try {
				CultureInfo ci = CultureInfo.GetCultureInfo ("en-US");
				Thread.CurrentThread.CurrentCulture = ci;
				Thread.CurrentThread.CurrentUICulture = ci;
				RunConvertTests ();
			} finally {
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}

		void RunConvertTests ()
		{
			BaseCompareValidatorPoker p = new BaseCompareValidatorPoker ();
			object result;

			/* an integer constant */
			Assert.IsTrue (p.Convert ("10", ValidationDataType.String, out result), "C1");
			Assert.AreEqual ("10", result, "C2");
			Assert.IsTrue (p.Convert ("10", ValidationDataType.Integer, out result), "C3");
			Assert.AreEqual (10, result, "C4");
			Assert.IsTrue (p.Convert ("10", ValidationDataType.Double, out result), "C5");
			Assert.AreEqual (10.0d, result, "C6");
			Assert.IsFalse (p.Convert ("10", ValidationDataType.Date, out result), "C7");
			Assert.IsNull (result, "C8");
			Assert.IsTrue (p.Convert ("10", ValidationDataType.Currency, out result), "C9");
			Assert.AreEqual (new Decimal (10.0f), result, "C10");

			/* a double constant */
			Assert.IsTrue (p.Convert ("10.5", ValidationDataType.String, out result), "C11");
			Assert.AreEqual ("10.5", result, "C12");
			Assert.IsFalse (p.Convert ("10.5", ValidationDataType.Integer, out result), "C13");
			Assert.IsNull (result, "C14");
			Assert.IsTrue (p.Convert ("10.5", ValidationDataType.Double, out result), "C15");
			Assert.AreEqual (10.5d, result, "C16");
// find a way to do this in a Culture independent way
//			Assert.IsFalse (p.Convert ("10.5", ValidationDataType.Date, out result), "C17");
//			Assert.IsNull (result, "C18");
			Assert.IsTrue (p.Convert ("10.5", ValidationDataType.Currency, out result), "C19");
			Assert.AreEqual (new Decimal (10.5f), result, "C20");

			/* a string constant */
			Assert.IsTrue (p.Convert ("hi", ValidationDataType.String, out result), "C21");
			Assert.AreEqual ("hi", result, "C22");
			Assert.IsFalse (p.Convert ("hi", ValidationDataType.Integer, out result), "C23");
			Assert.IsNull (result, "C24");
			Assert.IsFalse (p.Convert ("hi", ValidationDataType.Double, out result), "C25");
			Assert.IsNull (result, "C26");
			Assert.IsFalse (p.Convert ("hi", ValidationDataType.Date, out result), "C27");
			Assert.IsNull (result, "C28");
			Assert.IsFalse (p.Convert ("hi", ValidationDataType.Currency, out result), "C29");
			Assert.IsNull (result, "C30");

			/* a date constant */
			DateTime dt = new DateTime (2005, 7, 19);
			string dt_str = dt.ToString("d");
			Assert.IsTrue  (p.Convert (dt_str, ValidationDataType.String, out result), "C31");
			Assert.AreEqual (dt_str, result, "C32");
			Assert.IsFalse (p.Convert (dt_str, ValidationDataType.Integer, out result), "C33");
			Assert.IsNull (result, "C34");
			Assert.IsFalse (p.Convert (dt_str, ValidationDataType.Double, out result), "C35");
			Assert.IsNull (result, "C36");
			Assert.IsTrue  (p.Convert (dt_str, ValidationDataType.Date, out result), "C37");
			Assert.AreEqual (DateTime.Parse (dt_str), result, "C38");
			Assert.IsFalse (p.Convert (dt_str, ValidationDataType.Currency, out result), "C39");
			Assert.IsNull (result, "C40");

			/* a currency constant? */
			Assert.IsTrue (p.Convert ("10.50", ValidationDataType.String, out result), "C41");
			Assert.AreEqual ("10.50", result, "C42");
			Assert.IsFalse (p.Convert ("10.50", ValidationDataType.Integer, out result), "C43");
			Assert.IsNull (result, "C44");
			Assert.IsTrue (p.Convert ("10.50", ValidationDataType.Double, out result), "C45");
			Assert.AreEqual (10.50d, result, "C46");
			Assert.IsFalse (p.Convert ("10.50", ValidationDataType.Date, out result), "C47");
			Assert.IsNull (result, "C48");
			Assert.IsTrue (p.Convert ("10.50", ValidationDataType.Currency, out result), "C49");
			Assert.AreEqual (Decimal.Parse ("10.50"), result, "C50");
		}

		[Test]
		public void Compare ()
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;

			try {
				CultureInfo ci = CultureInfo.GetCultureInfo ("en-US");
				Thread.CurrentThread.CurrentCulture = ci;
				Thread.CurrentThread.CurrentUICulture = ci;
				RunCompareTests ();
			} finally {
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}

		void RunCompareTests ()
		{
			BaseCompareValidatorPoker p = new BaseCompareValidatorPoker ();

			/* integer comparisons */
			/* equal */
			Assert.IsTrue  (p.Compare ("10", "10", ValidationCompareOperator.Equal,            ValidationDataType.Integer), "D1");
			Assert.IsFalse (p.Compare ("10", "10", ValidationCompareOperator.NotEqual,         ValidationDataType.Integer), "D2");
			Assert.IsFalse (p.Compare ("10", "10", ValidationCompareOperator.LessThan,         ValidationDataType.Integer), "D3");
			Assert.IsTrue  (p.Compare ("10", "10", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Integer), "D4");
			Assert.IsFalse (p.Compare ("10", "10", ValidationCompareOperator.GreaterThan,      ValidationDataType.Integer), "D5");
			Assert.IsTrue  (p.Compare ("10", "10", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Integer), "D6");
			/* less than */
			Assert.IsFalse (p.Compare ( "5", "10", ValidationCompareOperator.Equal,            ValidationDataType.Integer), "D7");
			Assert.IsTrue  (p.Compare ( "5", "10", ValidationCompareOperator.NotEqual,         ValidationDataType.Integer), "D8");
			Assert.IsTrue  (p.Compare ( "5", "10", ValidationCompareOperator.LessThan,         ValidationDataType.Integer), "D9");
			Assert.IsTrue  (p.Compare ( "5", "10", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Integer), "D10");
			Assert.IsFalse (p.Compare ( "5", "10", ValidationCompareOperator.GreaterThan,      ValidationDataType.Integer), "D11");
			Assert.IsFalse (p.Compare ( "5", "10", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Integer), "D12");
			/* greater than */
			Assert.IsFalse (p.Compare ("10",  "5", ValidationCompareOperator.Equal,            ValidationDataType.Integer), "D13");
			Assert.IsTrue  (p.Compare ("10",  "5", ValidationCompareOperator.NotEqual,         ValidationDataType.Integer), "D14");
			Assert.IsFalse (p.Compare ("10",  "5", ValidationCompareOperator.LessThan,         ValidationDataType.Integer), "D15");
			Assert.IsFalse (p.Compare ("10",  "5", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Integer), "D16");
			Assert.IsTrue  (p.Compare ("10",  "5", ValidationCompareOperator.GreaterThan,      ValidationDataType.Integer), "D17");
			Assert.IsTrue  (p.Compare ("10",  "5", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Integer), "D18");
			/* error conditions */
			Assert.IsFalse (p.Compare ("hi",  "5", ValidationCompareOperator.Equal,            ValidationDataType.Integer), "D13");
			Assert.IsFalse (p.Compare ("hi",  "5", ValidationCompareOperator.NotEqual,         ValidationDataType.Integer), "D14");
			Assert.IsFalse (p.Compare ("hi",  "5", ValidationCompareOperator.LessThan,         ValidationDataType.Integer), "D15");
			Assert.IsFalse (p.Compare ("hi",  "5", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Integer), "D16");
			Assert.IsFalse (p.Compare ("hi",  "5", ValidationCompareOperator.GreaterThan,      ValidationDataType.Integer), "D17");
			Assert.IsFalse (p.Compare ("hi",  "5", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Integer), "D18");
			Assert.IsFalse (p.Compare (null,  "5", ValidationCompareOperator.Equal,            ValidationDataType.Integer), "D19");

			Assert.IsTrue  (p.Compare ( "5", "hi", ValidationCompareOperator.Equal,            ValidationDataType.Integer), "D20");
			Assert.IsTrue (p.Compare ( "5", "hi", ValidationCompareOperator.NotEqual,         ValidationDataType.Integer), "D21");
			Assert.IsTrue (p.Compare ( "5", "hi", ValidationCompareOperator.LessThan,         ValidationDataType.Integer), "D22");
			Assert.IsTrue (p.Compare ( "5", "hi", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Integer), "D23");
			Assert.IsTrue (p.Compare ( "5", "hi", ValidationCompareOperator.GreaterThan,      ValidationDataType.Integer), "D24");
			Assert.IsTrue (p.Compare ( "5", "hi", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Integer), "D25");
			Assert.IsTrue (p.Compare ( "5", null, ValidationCompareOperator.Equal,            ValidationDataType.Integer), "D26");
			Assert.IsFalse (p.Compare ( "hi", "hi", ValidationCompareOperator.Equal,            ValidationDataType.Integer), "D111");
			Assert.IsFalse (p.Compare ( null, null, ValidationCompareOperator.Equal,            ValidationDataType.Integer), "D112");

			/* double comparisons */
			/* equal */
			Assert.IsTrue  (p.Compare ("10.5", "10.5", ValidationCompareOperator.Equal,            ValidationDataType.Double), "D27");
			Assert.IsFalse (p.Compare ("10.5", "10.5", ValidationCompareOperator.NotEqual,         ValidationDataType.Double), "D28");
			Assert.IsFalse (p.Compare ("10.5", "10.5", ValidationCompareOperator.LessThan,         ValidationDataType.Double), "D29");
			Assert.IsTrue  (p.Compare ("10.5", "10.5", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Double), "D30");
			Assert.IsFalse (p.Compare ("10.5", "10.5", ValidationCompareOperator.GreaterThan,      ValidationDataType.Double), "D31");
			Assert.IsTrue  (p.Compare ("10.5", "10.5", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Double), "D32");
			/* less than */
			Assert.IsFalse (p.Compare ( "5.5", "10.5", ValidationCompareOperator.Equal,            ValidationDataType.Double), "D33");
			Assert.IsTrue  (p.Compare ( "5.5", "10.5", ValidationCompareOperator.NotEqual,         ValidationDataType.Double), "D34");
			Assert.IsTrue  (p.Compare ( "5.5", "10.5", ValidationCompareOperator.LessThan,         ValidationDataType.Double), "D35");
			Assert.IsTrue  (p.Compare ( "5.5", "10.5", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Double), "D36");
			Assert.IsFalse (p.Compare ( "5.5", "10.5", ValidationCompareOperator.GreaterThan,      ValidationDataType.Double), "D37");
			Assert.IsFalse (p.Compare ( "5.5", "10.5", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Double), "D38");
			/* greater than */
			Assert.IsFalse (p.Compare ("10.5",  "5.5", ValidationCompareOperator.Equal,            ValidationDataType.Double), "D39");
			Assert.IsTrue  (p.Compare ("10.5",  "5.5", ValidationCompareOperator.NotEqual,         ValidationDataType.Double), "D40");
			Assert.IsFalse (p.Compare ("10.5",  "5.5", ValidationCompareOperator.LessThan,         ValidationDataType.Double), "D41");
			Assert.IsFalse (p.Compare ("10.5",  "5.5", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Double), "D42");
			Assert.IsTrue  (p.Compare ("10.5",  "5.5", ValidationCompareOperator.GreaterThan,      ValidationDataType.Double), "D43");
			Assert.IsTrue  (p.Compare ("10.5",  "5.5", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Double), "D44");
			/* error conditions */
			Assert.IsFalse (p.Compare ("hi",  "5.5", ValidationCompareOperator.Equal,            ValidationDataType.Double), "D45");
			Assert.IsFalse (p.Compare ("hi",  "5.5", ValidationCompareOperator.NotEqual,         ValidationDataType.Double), "D46");
			Assert.IsFalse (p.Compare ("hi",  "5.5", ValidationCompareOperator.LessThan,         ValidationDataType.Double), "D47");
			Assert.IsFalse (p.Compare ("hi",  "5.5", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Double), "D48");
			Assert.IsFalse (p.Compare ("hi",  "5.5", ValidationCompareOperator.GreaterThan,      ValidationDataType.Double), "D49");
			Assert.IsFalse (p.Compare ("hi",  "5.5", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Double), "D50");
			Assert.IsFalse (p.Compare (null,  "5.5", ValidationCompareOperator.Equal,            ValidationDataType.Double), "D51");

			Assert.IsTrue (p.Compare ( "5.5", "hi", ValidationCompareOperator.Equal,            ValidationDataType.Double), "D52");
			Assert.IsTrue (p.Compare ( "5.5", "hi", ValidationCompareOperator.NotEqual,         ValidationDataType.Double), "D53");
			Assert.IsTrue (p.Compare ( "5.5", "hi", ValidationCompareOperator.LessThan,         ValidationDataType.Double), "D54");
			Assert.IsTrue (p.Compare ( "5.5", "hi", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Double), "D55");
			Assert.IsTrue (p.Compare ( "5.5", "hi", ValidationCompareOperator.GreaterThan,      ValidationDataType.Double), "D56");
			Assert.IsTrue (p.Compare ( "5.5", "hi", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Double), "D57");
			Assert.IsTrue (p.Compare ( "5.5", null, ValidationCompareOperator.Equal,            ValidationDataType.Double), "D58");
			Assert.IsFalse (p.Compare ( "hi", "hi", ValidationCompareOperator.Equal,            ValidationDataType.Double), "D26");
			Assert.IsFalse (p.Compare ( null, null, ValidationCompareOperator.Equal,            ValidationDataType.Double), "D26");

			/* string comparisons */
			/* equal */
			Assert.IsTrue  (p.Compare ("hi", "hi", ValidationCompareOperator.Equal,            ValidationDataType.String), "D59");
			Assert.IsFalse (p.Compare ("hi", "hi", ValidationCompareOperator.NotEqual,         ValidationDataType.String), "D60");
			Assert.IsFalse (p.Compare ("hi", "hi", ValidationCompareOperator.LessThan,         ValidationDataType.String), "D61");
			Assert.IsTrue  (p.Compare ("hi", "hi", ValidationCompareOperator.LessThanEqual,    ValidationDataType.String), "D62");
			Assert.IsFalse (p.Compare ("hi", "hi", ValidationCompareOperator.GreaterThan,      ValidationDataType.String), "D63");
			Assert.IsTrue  (p.Compare ("hi", "hi", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.String), "D64");
			/* less than */
			Assert.IsFalse (p.Compare ( "bye", "hi", ValidationCompareOperator.Equal,            ValidationDataType.String), "D65");
			Assert.IsTrue  (p.Compare ( "bye", "hi", ValidationCompareOperator.NotEqual,         ValidationDataType.String), "D66");
			Assert.IsTrue  (p.Compare ( "bye", "hi", ValidationCompareOperator.LessThan,         ValidationDataType.String), "D67");
			Assert.IsTrue  (p.Compare ( "bye", "hi", ValidationCompareOperator.LessThanEqual,    ValidationDataType.String), "D68");
			Assert.IsFalse (p.Compare ( "bye", "hi", ValidationCompareOperator.GreaterThan,      ValidationDataType.String), "D69");
			Assert.IsFalse (p.Compare ( "bye", "hi", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.String), "D70");
			/* greater than */
			Assert.IsFalse (p.Compare ("hi",  "bye", ValidationCompareOperator.Equal,            ValidationDataType.String), "D71");
			Assert.IsTrue  (p.Compare ("hi",  "bye", ValidationCompareOperator.NotEqual,         ValidationDataType.String), "D72");
			Assert.IsFalse (p.Compare ("hi",  "bye", ValidationCompareOperator.LessThan,         ValidationDataType.String), "D73");
			Assert.IsFalse (p.Compare ("hi",  "bye", ValidationCompareOperator.LessThanEqual,    ValidationDataType.String), "D74");
			Assert.IsTrue  (p.Compare ("hi",  "bye", ValidationCompareOperator.GreaterThan,      ValidationDataType.String), "D75");
			Assert.IsTrue  (p.Compare ("hi",  "bye", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.String), "D76");

			/* error conditions */
			Assert.IsFalse (p.Compare (null,  "hi", ValidationCompareOperator.Equal,             ValidationDataType.String), "D113");
			Assert.IsTrue  (p.Compare ("hi",  null, ValidationCompareOperator.Equal,             ValidationDataType.String), "D114");
			Assert.IsFalse (p.Compare ( null, null, ValidationCompareOperator.Equal,             ValidationDataType.String), "D115");


			/* date comparisons */
			/* equal */
			DateTime dt1 = new DateTime (2005, 7, 18);
			DateTime dt2 = new DateTime (2005, 7, 19);
			string dt1_str = dt1.ToString("d");
			string dt2_str = dt2.ToString("d");

			Assert.IsTrue  (p.Compare (dt2_str, dt2_str, ValidationCompareOperator.Equal,            ValidationDataType.Date), "D59");
			Assert.IsFalse (p.Compare (dt2_str, dt2_str, ValidationCompareOperator.NotEqual,         ValidationDataType.Date), "D60");
			Assert.IsFalse (p.Compare (dt2_str, dt2_str, ValidationCompareOperator.LessThan,         ValidationDataType.Date), "D61");
			Assert.IsTrue  (p.Compare (dt2_str, dt2_str, ValidationCompareOperator.LessThanEqual,    ValidationDataType.Date), "D62");
			Assert.IsFalse (p.Compare (dt2_str, dt2_str, ValidationCompareOperator.GreaterThan,      ValidationDataType.Date), "D63");
			Assert.IsTrue  (p.Compare (dt2_str, dt2_str, ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Date), "D64");
			/* less than */
			Assert.IsFalse (p.Compare ( dt1_str, dt2_str, ValidationCompareOperator.Equal,            ValidationDataType.Date), "D65");
			Assert.IsTrue  (p.Compare ( dt1_str, dt2_str, ValidationCompareOperator.NotEqual,         ValidationDataType.Date), "D66");
			Assert.IsTrue  (p.Compare ( dt1_str, dt2_str, ValidationCompareOperator.LessThan,         ValidationDataType.Date), "D67");
			Assert.IsTrue  (p.Compare ( dt1_str, dt2_str, ValidationCompareOperator.LessThanEqual,    ValidationDataType.Date), "D68");
			Assert.IsFalse (p.Compare ( dt1_str, dt2_str, ValidationCompareOperator.GreaterThan,      ValidationDataType.Date), "D69");
			Assert.IsFalse (p.Compare ( dt1_str, dt2_str, ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Date), "D70");
			/* greater than */
			Assert.IsFalse (p.Compare (dt2_str,  dt1_str, ValidationCompareOperator.Equal,            ValidationDataType.Date), "D71");
			Assert.IsTrue  (p.Compare (dt2_str,  dt1_str, ValidationCompareOperator.NotEqual,         ValidationDataType.Date), "D72");
			Assert.IsFalse (p.Compare (dt2_str,  dt1_str, ValidationCompareOperator.LessThan,         ValidationDataType.Date), "D73");
			Assert.IsFalse (p.Compare (dt2_str,  dt1_str, ValidationCompareOperator.LessThanEqual,    ValidationDataType.Date), "D74");
			Assert.IsTrue  (p.Compare (dt2_str,  dt1_str, ValidationCompareOperator.GreaterThan,      ValidationDataType.Date), "D75");
			Assert.IsTrue  (p.Compare (dt2_str,  dt1_str, ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Date), "D76");

			/* error conditions */
			Assert.IsFalse (p.Compare (null,  dt2_str, ValidationCompareOperator.Equal,             ValidationDataType.Date), "D77");
			Assert.IsTrue  (p.Compare (dt2_str,  null, ValidationCompareOperator.Equal,             ValidationDataType.Date), "D78");
			Assert.IsFalse (p.Compare ("hi",  null, ValidationCompareOperator.Equal,             ValidationDataType.Date), "D116");
			Assert.IsFalse (p.Compare ( null, null, ValidationCompareOperator.Equal,             ValidationDataType.Date), "D117");

			/* currency comparisons */
			/* equal */
			Assert.IsTrue  (p.Compare ("10.50", "10.50", ValidationCompareOperator.Equal,            ValidationDataType.Currency), "D79");
			Assert.IsFalse (p.Compare ("10.50", "10.50", ValidationCompareOperator.NotEqual,         ValidationDataType.Currency), "D80");
			Assert.IsFalse (p.Compare ("10.50", "10.50", ValidationCompareOperator.LessThan,         ValidationDataType.Currency), "D81");
			Assert.IsTrue  (p.Compare ("10.50", "10.50", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Currency), "D82");
			Assert.IsFalse (p.Compare ("10.50", "10.50", ValidationCompareOperator.GreaterThan,      ValidationDataType.Currency), "D83");
			Assert.IsTrue  (p.Compare ("10.50", "10.50", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Currency), "D84");
			/* less than */
			Assert.IsFalse (p.Compare ( "5.50", "10.50", ValidationCompareOperator.Equal,            ValidationDataType.Currency), "D85");
			Assert.IsTrue  (p.Compare ( "5.50", "10.50", ValidationCompareOperator.NotEqual,         ValidationDataType.Currency), "D86");
			Assert.IsTrue  (p.Compare ( "5.50", "10.50", ValidationCompareOperator.LessThan,         ValidationDataType.Currency), "D87");
			Assert.IsTrue  (p.Compare ( "5.50", "10.50", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Currency), "D88");
			Assert.IsFalse (p.Compare ( "5.50", "10.50", ValidationCompareOperator.GreaterThan,      ValidationDataType.Currency), "D89");
			Assert.IsFalse (p.Compare ( "5.50", "10.50", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Currency), "D90");
			/* greater than */
			Assert.IsFalse (p.Compare ("10.50",  "5.50", ValidationCompareOperator.Equal,            ValidationDataType.Currency), "D91");
			Assert.IsTrue  (p.Compare ("10.50",  "5.50", ValidationCompareOperator.NotEqual,         ValidationDataType.Currency), "D92");
			Assert.IsFalse (p.Compare ("10.50",  "5.50", ValidationCompareOperator.LessThan,         ValidationDataType.Currency), "D93");
			Assert.IsFalse (p.Compare ("10.50",  "5.50", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Currency), "D94");
			Assert.IsTrue  (p.Compare ("10.50",  "5.50", ValidationCompareOperator.GreaterThan,      ValidationDataType.Currency), "D95");
			Assert.IsTrue  (p.Compare ("10.50",  "5.50", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Currency), "D96");
			/* error conditions */
			Assert.IsFalse (p.Compare ("hi",  "5.50", ValidationCompareOperator.Equal,            ValidationDataType.Currency), "D97");
			Assert.IsFalse (p.Compare ("hi",  "5.50", ValidationCompareOperator.NotEqual,         ValidationDataType.Currency), "D98");
			Assert.IsFalse (p.Compare ("hi",  "5.50", ValidationCompareOperator.LessThan,         ValidationDataType.Currency), "D99");
			Assert.IsFalse (p.Compare ("hi",  "5.50", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Currency), "D100");
			Assert.IsFalse (p.Compare ("hi",  "5.50", ValidationCompareOperator.GreaterThan,      ValidationDataType.Currency), "D101");
			Assert.IsFalse (p.Compare ("hi",  "5.50", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Currency), "D102");
			Assert.IsFalse (p.Compare (null,  "5.50", ValidationCompareOperator.Equal,            ValidationDataType.Currency), "D103");

			Assert.IsTrue (p.Compare ( "5.50", "hi", ValidationCompareOperator.Equal,            ValidationDataType.Currency), "D104");
			Assert.IsTrue (p.Compare ( "5.50", "hi", ValidationCompareOperator.NotEqual,         ValidationDataType.Currency), "D105");
			Assert.IsTrue (p.Compare ( "5.50", "hi", ValidationCompareOperator.LessThan,         ValidationDataType.Currency), "D106");
			Assert.IsTrue (p.Compare ( "5.50", "hi", ValidationCompareOperator.LessThanEqual,    ValidationDataType.Currency), "D107");
			Assert.IsTrue (p.Compare ( "5.50", "hi", ValidationCompareOperator.GreaterThan,      ValidationDataType.Currency), "D108");
			Assert.IsTrue (p.Compare ( "5.50", "hi", ValidationCompareOperator.GreaterThanEqual, ValidationDataType.Currency), "D109");
			Assert.IsTrue (p.Compare ( "5.50", null, ValidationCompareOperator.Equal,            ValidationDataType.Currency), "D110");

			Assert.IsFalse (p.Compare ("hi",  null, ValidationCompareOperator.Equal,             ValidationDataType.Currency), "D118");
			Assert.IsFalse (p.Compare ( null, null, ValidationCompareOperator.Equal,             ValidationDataType.Currency), "D119");
		}

		[Test]
		public void MiscPropertiesAndMethods()
		{
			BaseCompareValidatorPoker p = new BaseCompareValidatorPoker ();

			Assert.AreEqual (p.GetCutoffYear(), 2029, "E1");
			Assert.AreEqual (p.GetFullYear (29), 2029, "E2");
#if NET_2_0
			Assert.AreEqual (p.GetFullYear (30), 1930, "E3");
#else
			Assert.AreEqual (p.GetFullYear (30), 2030, "E3"); // XXX this is broken
#endif

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-GB", false);
			Assert.AreEqual (p.GetDateElementOrder (), "dmy", "E4");

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
			Assert.AreEqual (p.GetDateElementOrder (), "mdy", "E5");

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("af-ZA", false);
			Assert.AreEqual (p.GetDateElementOrder (), "ymd", "E6");
		}

#if NET_2_0
		[Test]
		public void CultureInvariantValues_1 ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
			//  Current date format --> "dmy"
			Page p = new Page ();

			CompareValidator v = new CompareValidator ();
			v.ControlToValidate = "tb1";
			v.Type = ValidationDataType.Date;
			v.ValueToCompare = "2005/12/24";
			v.CultureInvariantValues = true;

			TextBox tb1 = new TextBox ();
			tb1.ID = "tb1";
			tb1.Text = "12.24.2005";

			p.Controls.Add (tb1);
			p.Controls.Add (v);

			v.Validate ();
			Assert.AreEqual (true, v.IsValid, "CultureInvariantValues#1");

			tb1.Text = "12/24/2005";
			v.Validate ();
			Assert.AreEqual (true, v.IsValid, "CultureInvariantValues#2");

			tb1.Text = "2005.12.24";
			v.Validate ();
			Assert.AreEqual (true, v.IsValid, "CultureInvariantValues#3");

			tb1.Text = "2005.24.12";
			v.Validate ();
			Assert.AreEqual (false, v.IsValid, "CultureInvariantValues#4");
		}

		[Test]
		public void CultureInvariantValues_2 ()
		{

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-GB", false);
			//  Current date format --> "dmy"
			Page p = new Page ();

			CompareValidator v = new CompareValidator ();
			v.ControlToValidate = "tb1";
			v.Type = ValidationDataType.Date;
			v.ValueToCompare = "24/12/2005";
			v.CultureInvariantValues = false;

			TextBox tb1 = new TextBox ();
			tb1.ID = "tb1";
			tb1.Text = "24.12.2005";

			p.Controls.Add (tb1);
			p.Controls.Add (v);

			v.Validate ();
			Assert.AreEqual (true, v.IsValid, "CultureInvariantValues#1");

			tb1.Text = "24-12-2005";
			v.Validate ();
			Assert.AreEqual (true, v.IsValid, "CultureInvariantValues#2");

			tb1.Text = "2005/12/24";
			v.Validate ();
			Assert.AreEqual (true, v.IsValid, "CultureInvariantValues#3");

			tb1.Text = "2005.24.12";
			v.Validate ();
			Assert.AreEqual (false, v.IsValid, "CultureInvariantValues#4");
		}

		[Test]
		[ExpectedException(typeof(HttpException))]
		public void CultureInvariantValues_Exception ()
		{

			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-GB", false);
			//  Current date format --> "dmy"
			Page p = new Page ();

			CompareValidator v = new CompareValidator ();
			v.ControlToValidate = "tb1";
			v.Type = ValidationDataType.Date;
			v.ValueToCompare = "12--24--2005";
			v.CultureInvariantValues = false;

			TextBox tb1 = new TextBox ();
			tb1.ID = "tb1";
			tb1.Text = "24.12.2005";

			p.Controls.Add (tb1);
			p.Controls.Add (v);

			v.Validate ();
			Assert.AreEqual (true, v.IsValid, "CultureInvariantValues#1");

			tb1.Text = "24-12-2005";
			v.Validate ();
		}
#endif
	}
}
