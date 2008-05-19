#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
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