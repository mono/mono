/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	internal sealed class TypeDecoder
	{
		#region Constructors

		private TypeDecoder()
		{
		}

		#endregion

		#region Static Methods

		public static decimal DecodeDecimal(object value, int scale, int sqltype)
		{
			long divisor = 1;
			decimal returnValue = Convert.ToDecimal(value, CultureInfo.InvariantCulture);

			if (scale < 0)
			{
				divisor = (long)System.Math.Pow(10, scale * (-1));
			}

			switch (sqltype & ~1)
			{
				case IscCodes.SQL_SHORT:
				case IscCodes.SQL_LONG:
				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					returnValue = returnValue / divisor;
					break;
			}

			return returnValue;
		}

		public static DateTime DecodeTime(int sql_time)
		{
			GregorianCalendar calendar = new GregorianCalendar();

			int millisInDay = sql_time / 10;
			int hour = millisInDay / 3600000;
			int minute = (millisInDay - hour * 3600000) / 60000;
			int second = (millisInDay - hour * 3600000 - minute * 60000) / 1000;
			int millisecond = millisInDay - hour * 3600000 - minute * 60000 - second * 1000;

			return new DateTime(1970, 1, 1, hour, minute, second, millisecond, calendar);
		}

		public static DateTime DecodeDate(int sql_date)
		{
			int year, month, day, century;

			sql_date -= 1721119 - 2400001;
			century = (4 * sql_date - 1) / 146097;
			sql_date = 4 * sql_date - 1 - 146097 * century;
			day = sql_date / 4;

			sql_date = (4 * day + 3) / 1461;
			day = 4 * day + 3 - 1461 * sql_date;
			day = (day + 4) / 4;

			month = (5 * day - 3) / 153;
			day = 5 * day - 3 - 153 * month;
			day = (day + 5) / 5;

			year = 100 * century + sql_date;

			if (month < 10)
			{
				month += 3;
			}
			else
			{
				month -= 9;
				year += 1;
			}

			DateTime date = new System.DateTime(year, month, day);

			return date.Date;
		}

		#endregion
	}
}
