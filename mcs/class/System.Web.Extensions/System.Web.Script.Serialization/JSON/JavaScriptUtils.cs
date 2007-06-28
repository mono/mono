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
		public static void WriteEscapedJavaScriptString (string value, TextWriter writer) {
			WriteEscapedJavaScriptString (value, '"', true, writer);
		}

		public static void WriteEscapedJavaScriptString (string value, char delimiter, bool appendDelimiters, TextWriter writer) {
			// leading delimiter
			if (appendDelimiters)
				writer.Write (delimiter);

			if (!string.IsNullOrEmpty (value))
				for (int i = 0; i < value.Length; i++)
					WriteJavaScriptChar (value [i], delimiter, writer);

			// trailing delimiter
			if (appendDelimiters)
				writer.Write (delimiter);
		}

		public static void WriteEscapedJavaScriptChar (char value, char delimiter, bool appendDelimiters, TextWriter writer) {
			// leading delimiter
			if (appendDelimiters)
				writer.Write (delimiter);

			WriteJavaScriptChar (value, delimiter, writer);

			// trailing delimiter
			if (appendDelimiters)
				writer.Write (delimiter);
		}

		public static void WriteJavaScriptChar (char value, char delimiter, TextWriter writer) {
			switch (value) {
			case '\t':
				writer.Write (@"\t");
				break;
			case '\n':
				writer.Write (@"\n");
				break;
			case '\r':
				writer.Write (@"\r");
				break;
			case '\f':
				writer.Write (@"\f");
				break;
			case '\b':
				writer.Write (@"\b");
				break;
			case '<':
				writer.Write (@"\u003c");
				break;
			case '>':
				writer.Write (@"\u003e");
				break;
			case '"':
				// only escape if this charater is being used as the delimiter
				if (delimiter == '"')
					writer.Write (@"\""");
				else
					writer.Write (value);
				break;
			case '\'':
				writer.Write (@"\u0027");
				break;
			case '\\':
				writer.Write (@"\\");
				break;
			default:
				if (value > '\u001f')
					writer.Write (value);
				else {
					writer.Write("\\u00");
					int intVal = (int) value;
					writer.Write ((char) ('0' + (intVal >> 4)));
					intVal &= 0xf;
					writer.Write ((char) (intVal < 10 ? '0' + intVal : 'a' + (intVal - 10)));
				}
				break;
			}
		}
	}
}