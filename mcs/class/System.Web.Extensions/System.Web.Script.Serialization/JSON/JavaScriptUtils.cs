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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI.WebControls;

namespace Newtonsoft.Json
{
	internal static class JavaScriptUtils
	{
		public static string EscapeJavaScriptString(string value)
		{
			return EscapeJavaScriptString(value, '"', true);
		}

		public static string EscapeJavaScriptString(string value, char delimiter, bool appendDelimiters)
		{
			if (string.IsNullOrEmpty(value))
			{
				if (appendDelimiters)
					return new string(delimiter, 2);
				else
					return string.Empty;
			}

			StringBuilder sb = null;
			int lastWritePosition = 0;
			int skipped = 0;

			// leading delimiter
			if (appendDelimiters)
			{
				sb = new StringBuilder(value.Length + 5);
				sb.Append(delimiter);
			}

			for (int i = 0; i < value.Length; i++)
			{
				char currentChar = value[i];
				string escapedValue;

				switch (currentChar)
				{
					case '\t':
						escapedValue = @"\t";
						break;
					case '\n':
						escapedValue = @"\n";
						break;
					case '\r':
						escapedValue = @"\r";
						break;
					case '\f':
						escapedValue = @"\f";
						break;
					case '\b':
						escapedValue = @"\b";
						break;
					case '"':
						// only escape if this charater is being used as the delimiter
						escapedValue = (delimiter == '"') ? "\\\"" : null;
						break;
					case '\'':
						// only escape if this charater is being used as the delimiter
						escapedValue = (delimiter == '\'') ? @"\'" : null;
						break;
					case '\\':
						escapedValue = @"\\";
						break;
					default:
						escapedValue = null;
						break;
				}

				// test if the char needs to be escaped or whether it can be skipped
				if (escapedValue != null)
				{
					if (sb == null)
						sb = new StringBuilder(value.Length + 5);

					// write skipped text
					if (skipped > 0)
					{
						sb.Append(value, lastWritePosition, skipped);
						skipped = 0;
					}

					// write escaped value and note position
					sb.Append(escapedValue);
					lastWritePosition = i + 1;
				}
				else
				{
					skipped++;
				}
			}

			// nothing was escaped. return initial string
			if (sb == null)
				return value;

			// write any remaining skipped text
			if (skipped > 0)
				sb.Append(value, lastWritePosition, skipped);

			// trailing delimiter
			if (appendDelimiters)
				sb.Append(delimiter);

			return sb.ToString();
		}
	}
}