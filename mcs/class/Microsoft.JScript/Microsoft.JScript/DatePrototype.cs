//
// DataPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) Cesar Lopez Nataren
//

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

using System;
using System.Globalization;

namespace Microsoft.JScript {

	public class DatePrototype : DateObject
	{
		internal DatePrototype ()
			: base (Double.NaN)
		{
		}

		internal static DatePrototype Proto = new DatePrototype ();

		public static DateConstructor constructor {
			get { return DateConstructor.Ctr; }
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getDate)]
		public static double getDate (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.DateFromTime (DateConstructor.LocalTime (val));
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getDay)]
		public static double getDay (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.WeekDay (DateConstructor.LocalTime (val));
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getFullYear)]
		public static double getFullYear (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.YearFromTime (DateConstructor.LocalTime (val));
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getHours)]
		public static double getHours (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;
			
			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.HourFromTime (DateConstructor.LocalTime (val));
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getMilliseconds)]
		public static double getMilliseconds (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.msFromTime (DateConstructor.LocalTime (val));
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getMinutes)]
		public static double getMinutes (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.MinFromTime (DateConstructor.LocalTime (val));
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getMonth)]
		public static double getMonth (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.MonthFromTime (DateConstructor.LocalTime (val));
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getSeconds)]
		public static double getSeconds (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.SecFromTime (DateConstructor.LocalTime (val));
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getTime)]
		public static double getTime (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getTimezoneOffset)]
		public static double getTimezoneOffset (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else 
				return (val - DateConstructor.LocalTime (val)) / DateConstructor.MS_PER_MINUTE;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCDate)]
		public static double getUTCDate (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.DateFromTime (val);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCDay)]
		public static double getUTCDay (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.WeekDay (val);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCFullYear)]
		public static double getUTCFullYear (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.YearFromTime (val);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCHours)]
		public static double getUTCHours (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.HourFromTime (val);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCMilliseconds)]
		public static double getUTCMilliseconds (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.msFromTime (val);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCMinutes)]
		public static double getUTCMinutes (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.MinFromTime (val);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCMonth)]
		public static double getUTCMonth (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.MonthFromTime (val);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getUTCSeconds)]
		public static double getUTCSeconds (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date = (DateObject) thisObj;
			double val = date.ms;

			if (Double.IsNaN (val))
				return Double.NaN;
			else
				return DateConstructor.SecFromTime (val);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getVarDate)]
		public static object getVarDate (object thisObj)
		{
			//
			// FIXME: This seems to handle the most simple
			// cases, but surely we need to do more
			// to comply with Microsoft's implementation.
			//
			return thisObj;
		}

		//
		// Note: This method is obsolete, but users might
		// accidently use it instead of getFullYear(). The
		// standard says to return the year - 1900 (likely for
		// compatibility), but in this case not confusing the
		// user's expectations is more important than not
		// breaking obsolete code.
		//
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_getYear)]
		public static double getYear (object thisObj)
		{
			return getFullYear (thisObj);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setDate)]
		public static double setDate (object thisObj, double ddate)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = DateConstructor.LocalTime (date.ms);
			double day = DateConstructor.MakeDay ((double) DateConstructor.YearFromTime (t),
				(double) DateConstructor.MonthFromTime (t), ddate);
			double new_val = DateConstructor.ToUTC (DateConstructor.MakeDate (day, t % DateConstructor.MS_PER_DAY));
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setFullYear)]
		public static double setFullYear (object thisObj, double dyear,
						  object month, object date)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject dt = (DateObject) thisObj;
			double t = DateConstructor.LocalTime (dt.ms);
			if (Double.IsNaN (t))
				t = 0;

			double new_month;
			if (month == null)
				new_month = DateConstructor.MonthFromTime (t);
			else
				new_month = Convert.ToNumber (month);

			double new_date;
			if (date == null)
				new_date = DateConstructor.DateFromTime (t);
			else
				new_date = Convert.ToNumber (date);

			double day = DateConstructor.MakeDay (dyear, new_month, new_date);
			double new_val = DateConstructor.ToUTC (DateConstructor.MakeDate (day, t % DateConstructor.MS_PER_DAY));
			dt.ms = DateConstructor.TimeClip (new_val);
			return dt.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setHours)]
		public static double setHours (object thisObj, double dhour, object min,
					       object sec, object msec)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = DateConstructor.LocalTime (date.ms);

			double new_min;
			if (min == null)
				new_min = DateConstructor.MinFromTime (t);
			else
				new_min = Convert.ToNumber (min);

			double new_sec;
			if (sec == null)
				new_sec = DateConstructor.SecFromTime (t);
			else
				new_sec = Convert.ToNumber (sec);

			double new_ms;
			if (msec == null)
				new_ms = DateConstructor.msFromTime (t);
			else
				new_ms = Convert.ToNumber (msec);

			double time = DateConstructor.MakeTime (dhour, new_min, new_sec, new_ms);
			double day = Math.Floor (t / DateConstructor.MS_PER_DAY);
			double new_val = DateConstructor.ToUTC (DateConstructor.MakeDate (day, time));
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setMinutes)]
		public static double setMinutes (object thisObj, double dmin,
						 object sec, object msec)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = DateConstructor.LocalTime (date.ms);

			double new_sec;
			if (sec == null)
				new_sec = DateConstructor.SecFromTime (t);
			else
				new_sec = Convert.ToNumber (sec);

			double new_ms;
			if (msec == null)
				new_ms = DateConstructor.msFromTime (t);
			else
				new_ms = Convert.ToNumber (msec);

			double time = DateConstructor.MakeTime (DateConstructor.HourFromTime (t), dmin, new_sec, new_ms);
			double day = Math.Floor (t / DateConstructor.MS_PER_DAY);
			double new_val = DateConstructor.ToUTC (DateConstructor.MakeDate (day, time));
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setMilliseconds)]
		public static double setMilliseconds (object thisObj, double dmsec)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = DateConstructor.LocalTime (date.ms);
			double time = DateConstructor.MakeTime (DateConstructor.HourFromTime (t), DateConstructor.MinFromTime (t),
				DateConstructor.SecFromTime (t), dmsec);
			double day = Math.Floor (t / DateConstructor.MS_PER_DAY);
			double new_val = DateConstructor.ToUTC (DateConstructor.MakeDate (day, time));
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setMonth)]
		public static double setMonth (object thisObj, double dmonth,
					       object date)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject dt = (DateObject) thisObj;
			double t = DateConstructor.LocalTime (dt.ms);

			double new_date;
			if (date == null)
				new_date = DateConstructor.DateFromTime (t);
			else
				new_date = Convert.ToNumber (date);
			
			double day = DateConstructor.MakeDay ((double) DateConstructor.YearFromTime (t),
				dmonth, new_date);
			double new_val = DateConstructor.ToUTC (DateConstructor.MakeDate (day, t % DateConstructor.MS_PER_DAY));
			dt.ms = DateConstructor.TimeClip (new_val);
			return dt.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject,
			JSBuiltin.Date_setSeconds)]
		public static double setSeconds (object thisObj, double dsec, object msec)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = DateConstructor.LocalTime (date.ms);

			double new_ms;
			if (msec == null)
				new_ms = DateConstructor.msFromTime (t);
			else
				new_ms = Convert.ToNumber (msec);

			double time = DateConstructor.MakeTime (DateConstructor.HourFromTime (t), DateConstructor.MinFromTime (t),
				dsec, new_ms);
			double day = Math.Floor (t / DateConstructor.MS_PER_DAY);
			double new_val = DateConstructor.ToUTC (DateConstructor.MakeDate (day, time));
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setTime)]
		public static double setTime (object thisObj, double time)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			date.ms = DateConstructor.TimeClip (time);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCDate)]
		public static double setUTCDate (object thisObj, double ddate)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = date.ms;
			double day = DateConstructor.MakeDay ((double) DateConstructor.YearFromTime (t),
				(double) DateConstructor.MonthFromTime (t), ddate);
			double new_val = DateConstructor.MakeDate (day, t % DateConstructor.MS_PER_DAY);
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCFullYear)]
		public static double setUTCFullYear (object thisObj, double dyear,	
						     object month, object date)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject dt = (DateObject) thisObj;
			double t = dt.ms;
			if (Double.IsNaN (t))
				t = 0;

			double new_month;
			if (month == null)
				new_month = DateConstructor.MonthFromTime (t);
			else
				new_month = Convert.ToNumber (month);

			double new_date;
			if (date == null)
				new_date = DateConstructor.DateFromTime (t);
			else
				new_date = Convert.ToNumber (date);

			double day = DateConstructor.MakeDay (dyear, new_month, new_date);
			double new_val = DateConstructor.MakeDate (day, t % DateConstructor.MS_PER_DAY);
			dt.ms = DateConstructor.TimeClip (new_val);
			return dt.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCHours)]
		public static double setUTCHours (object thisObj, double dhour,
						  object min, object sec, object msec)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = date.ms;

			double new_min;
			if (min == null)
				new_min = DateConstructor.MinFromTime (t);
			else
				new_min = Convert.ToNumber (min);

			double new_sec;
			if (sec == null)
				new_sec = DateConstructor.SecFromTime (t);
			else
				new_sec = Convert.ToNumber (sec);

			double new_ms;
			if (msec == null)
				new_ms = DateConstructor.msFromTime (t);
			else
				new_ms = Convert.ToNumber (msec);

			double time = DateConstructor.MakeTime (dhour, new_min, new_sec, new_ms);
			double day = Math.Floor (t / DateConstructor.MS_PER_DAY);
			double new_val = DateConstructor.MakeDate (day, time);
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCMinutes)]
		public static double setUTCMinutes (object thisObj, double dmin,
						    object sec, object msec)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = date.ms;

			double new_sec;
			if (sec == null)
				new_sec = DateConstructor.SecFromTime (t);
			else
				new_sec = Convert.ToNumber (sec);

			double new_ms;
			if (msec == null)
				new_ms = DateConstructor.msFromTime (t);
			else
				new_ms = Convert.ToNumber (msec);

			double time = DateConstructor.MakeTime (DateConstructor.HourFromTime (t), dmin, new_sec, new_ms);
			double day = Math.Floor (t / DateConstructor.MS_PER_DAY);
			double new_val = DateConstructor.MakeDate (day, time);
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCMilliseconds)]
		public static double setUTCMilliseconds (object thisObj, double msec)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = date.ms;
			double time = DateConstructor.MakeTime (DateConstructor.HourFromTime (t), DateConstructor.MinFromTime (t),
				DateConstructor.SecFromTime (t), msec);
			double day = Math.Floor (t / DateConstructor.MS_PER_DAY);
			double new_val = DateConstructor.MakeDate (day, time);
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCMonth)]
		public static double setUTCMonth (object thisObj, double dmonth, object date)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject dt = (DateObject) thisObj;
			double t = dt.ms;

			double new_date;
			if (date == null)
				new_date = DateConstructor.DateFromTime (t);
			else
				new_date = Convert.ToNumber (date);

			double day = DateConstructor.MakeDay ((double) DateConstructor.YearFromTime (t),
				dmonth, new_date);
			double new_val = DateConstructor.MakeDate (day, t % DateConstructor.MS_PER_DAY);
			dt.ms = DateConstructor.TimeClip (new_val);
			return dt.ms;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setUTCSeconds)]
		public static double setUTCSeconds (object thisObj, double dsec, object msec)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			DateObject date = (DateObject) thisObj;
			double t = date.ms;

			double new_ms;
			if (msec == null)
				new_ms = DateConstructor.msFromTime (t);
			else
				new_ms = Convert.ToNumber (msec);

			double time = DateConstructor.MakeTime (DateConstructor.HourFromTime (t), DateConstructor.MinFromTime (t),
				dsec, new_ms);
			double day = Math.Floor (t / DateConstructor.MS_PER_DAY);
			double new_val = DateConstructor.MakeDate (day, time);
			date.ms = DateConstructor.TimeClip (new_val);
			return date.ms;
		}

		/* Note: See Note for GetYear() for an explanation why we do not emulate obsolete behavior here. */
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_setYear)]
		public static double setYear (object thisObj, double dyear)
		{
			return setFullYear (thisObj, dyear, null, null);
		}

		internal const string InvalidDateString = "Invalid Date";

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toDateString)]
		public static string toDateString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date_obj = (DateObject) thisObj;
			double val = date_obj.ms;
			double lv = DateConstructor.LocalTime (val);
			int year = DateConstructor.YearFromTime (lv);
			int month = DateConstructor.MonthFromTime (lv);
			int date = DateConstructor.DateFromTime (lv);

			DateTime dt;
			try {
				dt = new DateTime (year, month + 1, date);
			} catch (ArgumentOutOfRangeException) {
				return InvalidDateString;
			}

			return dt.ToString ("ddd MMM d yyyy", CultureInfo.InvariantCulture);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toGMTString)]
		public static string toGMTString (object thisObj)
		{
			return toUTCString (thisObj);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toLocaleDateString)]
		public static string toLocaleDateString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date_obj = (DateObject) thisObj;
			double val = date_obj.ms;
			double lv = DateConstructor.LocalTime (val);
			int year = DateConstructor.YearFromTime (lv);
			int month = DateConstructor.MonthFromTime (lv);
			int date = DateConstructor.DateFromTime (lv);

			DateTime dt;
			try {
				dt = new DateTime (year, month + 1, date);
			} catch (ArgumentOutOfRangeException) {
				return InvalidDateString;
			}

			return dt.ToString ("D");
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toLocaleString)]
		public static string toLocaleString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date_obj = (DateObject) thisObj;
			double val = date_obj.ms;
			double lv = DateConstructor.LocalTime (val);
			int year = DateConstructor.YearFromTime (lv);
			int month = DateConstructor.MonthFromTime (lv);
			int date = DateConstructor.DateFromTime (lv);
			int hour = DateConstructor.HourFromTime (lv);
			int min = DateConstructor.MinFromTime (lv);
			int sec = DateConstructor.SecFromTime (lv);

			DateTime dt;
			try {
				dt = new DateTime (year, month + 1, date, hour, min, sec);
			} catch (ArgumentOutOfRangeException) {
				return InvalidDateString;
			}

			return dt.ToString ("F");
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toLocaleTimeString)]
		public static string toLocaleTimeString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date_obj = (DateObject) thisObj;
			double val = date_obj.ms;
			double lv = DateConstructor.LocalTime (val);
			int year = DateConstructor.YearFromTime (lv);
			int month = DateConstructor.MonthFromTime (lv);
			int date = DateConstructor.DateFromTime (lv);
			int hour = DateConstructor.HourFromTime (lv);
			int min = DateConstructor.MinFromTime (lv);
			int sec = DateConstructor.SecFromTime (lv);

			DateTime dt;
			try {
				dt = new DateTime (year, month + 1, date, hour, min, sec);
			} catch (ArgumentOutOfRangeException) {
				return InvalidDateString;
			}

			return dt.ToString ("T");
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toString)]
		public static string toString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));
			string date_str = toDateString (thisObj);
			if (date_str == InvalidDateString)
				return date_str;

			return date_str.Insert (date_str.LastIndexOf (' '), " " + toTimeString (thisObj));
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toTimeString)]
		public static string toTimeString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date_obj = (DateObject) thisObj;
			double val = date_obj.ms;
			double lv = DateConstructor.LocalTime (val);
			int hour = DateConstructor.HourFromTime (lv);
			int min = DateConstructor.MinFromTime (lv);
			int sec = DateConstructor.SecFromTime (lv);
			double off = getTimezoneOffset (thisObj);

			return String.Format (@"{0:00}:{1:00}:{2:00} UTC{3:\+0;\-0;\+0}", hour, min, sec, -off / 60);
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_toUTCString)]
		public static string toUTCString (object thisObj)
		{
			SemanticAnalyser.assert_type (thisObj, typeof (DateObject));

			DateObject date_obj = (DateObject) thisObj;
			double val = date_obj.ms;
			int year = DateConstructor.YearFromTime (val);
			int month = DateConstructor.MonthFromTime (val);
			int date = DateConstructor.DateFromTime (val);
			int hour = DateConstructor.HourFromTime (val);
			int min = DateConstructor.MinFromTime (val);
			int sec = DateConstructor.SecFromTime (val);

			DateTime dt;
			try {
				dt = new DateTime (year, month + 1, date);
			} catch (ArgumentOutOfRangeException) {
				return InvalidDateString;
			}

			string date_string = dt.ToString ("ddd, d MMM yyyy ", CultureInfo.InvariantCulture);
			string time_string = String.Format (@"{0:00}:{1:00}:{2:00} UTC", hour, min, sec);

			return date_string + time_string;
		}

		[JSFunctionAttribute (JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Date_valueOf)]
		public static double valueOf (object thisObj)
		{
			return getTime (thisObj);
		}
	}
}
