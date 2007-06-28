#region License
// Copyright 2006 James Newton-King
// http://www.newtonsoft.com
//
// This work is licensed under the Creative Commons Attribution 2.5 License
// http://creativecommons.org/licenses/by/2.5/
//
// You are free:
//    * to copy, distribute, display, and perform the work
//    * to make derivative works
//    * to make commercial use of the work
//
// Under the following conditions:
//    * For any reuse or distribution, you must make clear to others the license terms of this work.
//    * Any of these conditions can be waived if you get permission from the copyright holder.
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data.SqlTypes;
using Newtonsoft.Json.Utilities;
using System.Xml;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Provides methods for converting between common language runtime types and JavaScript types.
	/// </summary>
	static class JavaScriptConvert
	{
		/// <summary>
		/// Represents JavaScript's boolean value true as a string. This field is read-only.
		/// </summary>
		public static readonly string True;

		/// <summary>
		/// Represents JavaScript's boolean value false as a string. This field is read-only.
		/// </summary>
		public static readonly string False;

		/// <summary>
		/// Represents JavaScript's null as a string. This field is read-only.
		/// </summary>
		public static readonly string Null;

		/// <summary>
		/// Represents JavaScript's undefined as a string. This field is read-only.
		/// </summary>
		public static readonly string Undefined;

		internal static readonly long InitialJavaScriptDateTicks;
		internal static readonly DateTime MinimumJavaScriptDate;

		static JavaScriptConvert()
		{
			True = "true";
			False = "false";
			Null = "null";
			Undefined = "undefined";

			InitialJavaScriptDateTicks = (new DateTime(1970, 1, 1)).Ticks;
			MinimumJavaScriptDate = new DateTime(100, 1, 1);
		}

		/// <summary>
		/// Converts the <see cref="DateTime"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="DateTime"/>.</returns>
		public static string ToString(DateTime value)
		{
			long javaScriptTicks = ConvertDateTimeToJavaScriptTicks(value);

			return "new Date(" + javaScriptTicks + ")";
		}

		internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime)
		{
			if (dateTime < MinimumJavaScriptDate)
				dateTime = MinimumJavaScriptDate;

			long javaScriptTicks = (dateTime.Ticks - InitialJavaScriptDateTicks) / (long)10000;

			return javaScriptTicks;
		}

		internal static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
		{
			DateTime dateTime = new DateTime((javaScriptTicks * 10000) + InitialJavaScriptDateTicks);

			return dateTime;
		}

		/// <summary>
		/// Converts the <see cref="Boolean"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Boolean"/>.</returns>
		public static string ToString(bool value)
		{
			return (value) ? True : False;
		}

		/// <summary>
		/// Converts the <see cref="Char"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Char"/>.</returns>
		public static void WriteChar (char value, TextWriter writer) {
			if (value == '\0')
				writer.Write(Null);
			else
				JavaScriptUtils.WriteEscapedJavaScriptChar (value, '"', true, writer);
		}

		/// <summary>
		/// Converts the <see cref="Enum"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Enum"/>.</returns>
		public static string ToString(Enum value)
		{
			return value.ToString();
		}

		/// <summary>
		/// Converts the <see cref="Int32"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Int32"/>.</returns>
		public static string ToString(int value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="Int16"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Int16"/>.</returns>
		public static string ToString(short value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="UInt16"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="UInt16"/>.</returns>
		public static string ToString(ushort value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="UInt32"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="UInt32"/>.</returns>
		public static string ToString(uint value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="Int64"/>  to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Int64"/>.</returns>
		public static string ToString(long value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="UInt64"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="UInt64"/>.</returns>
		public static string ToString(ulong value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="Single"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Single"/>.</returns>
		public static string ToString(float value)
		{
			return value.ToString ("R", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="Double"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Double"/>.</returns>
		public static string ToString(double value)
		{
			return value.ToString ("R", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="Byte"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Byte"/>.</returns>
		public static string ToString(byte value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="SByte"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="SByte"/>.</returns>
		public static string ToString(sbyte value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="Decimal"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="SByte"/>.</returns>
		public static string ToString(decimal value)
		{
			return value.ToString(null, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Converts the <see cref="Guid"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="Guid"/>.</returns>
		//public static string ToString(Guid value)
		//{
		//    return '"' + value.ToString("D", CultureInfo.InvariantCulture) + '"';
		//}

		/// <summary>
		/// Converts the <see cref="String"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>A Json string representation of the <see cref="String"/>.</returns>
		public static void WriteString (string value, TextWriter writer)
		{
			WriteString (value, '"', writer);
		}

		/// <summary>
		/// Converts the <see cref="String"/> to it's JavaScript string representation.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="delimter">The string delimiter character.</param>
		/// <returns>A Json string representation of the <see cref="String"/>.</returns>
		public static void WriteString (string value, char delimter, TextWriter writer)
		{
			JavaScriptUtils.WriteEscapedJavaScriptString(value, delimter, true, writer);
		}
	}
}