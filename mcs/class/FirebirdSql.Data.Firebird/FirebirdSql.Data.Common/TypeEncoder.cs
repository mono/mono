/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	internal sealed class TypeEncoder
	{
		#region Constructors

		private TypeEncoder()
		{
		}

		#endregion

		#region Static Methods

		public static object EncodeDecimal(decimal d, int scale, int sqltype)
		{
			long multiplier = 1;

			if (scale < 0)
			{
				multiplier = (long)System.Math.Pow(10, scale * (-1));
			}

			switch (sqltype & ~1)
			{
				case IscCodes.SQL_SHORT:
					return (short)(d * multiplier);

				case IscCodes.SQL_LONG:
					return (int)(d * multiplier);

				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					return (long)(d * multiplier);

				case IscCodes.SQL_DOUBLE:
				default:
					return d;
			}
		}

		public static int EncodeTime(DateTime d)
		{
			GregorianCalendar calendar = new GregorianCalendar();

			int millisInDay =
				(int)(calendar.GetHour(d) * 3600000 +
				calendar.GetMinute(d) * 60000 +
				calendar.GetSecond(d) * 1000 +
				calendar.GetMilliseconds(d)) * 10;

			return millisInDay;
		}

		public static int EncodeDate(DateTime d)
		{
			int day, month, year;
			int c, ya;

			GregorianCalendar calendar = new GregorianCalendar();

			day = calendar.GetDayOfMonth(d);
			month = calendar.GetMonth(d);
			year = calendar.GetYear(d);

			if (month > 2)
			{
				month -= 3;
			}
			else
			{
				month += 9;
				year -= 1;
			}

			c = year / 100;
			ya = year - 100 * c;

			return ((146097 * c) / 4 +
				(1461 * ya) / 4 +
				(153 * month + 2) / 5 +
				day + 1721119 - 2400001);
		}

		#endregion
	}
}