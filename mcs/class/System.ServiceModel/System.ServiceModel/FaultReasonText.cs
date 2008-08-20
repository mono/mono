//
// FaultReasonText.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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

namespace System.ServiceModel
{
	public class FaultReasonText
	{
		readonly string text, xmllang;

		public FaultReasonText (string text)
			: this (text, CultureInfo.CurrentCulture)
		{
		}

		public FaultReasonText (string text, CultureInfo cultureInfo)
			: this (text, cultureInfo.Name)
		{
		}

		public FaultReasonText (string text, string xmlLang)
		{
			this.text = text;
			this.xmllang = xmlLang;
		}

		public string Text {
			get { return text; }
		}

		public string XmlLang {
			get { return xmllang; }
		}

		public bool Matches (CultureInfo cultureInfo)
		{
			if (cultureInfo.Name == xmllang)
				return true;
			if (cultureInfo.Parent != null)
				return cultureInfo.Parent.Name == xmllang;
			else
				return false;
		}
	}
}
