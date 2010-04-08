//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace System.Xaml
{
	public static class XamlServices
	{
		public static Object Load (Stream stream)
		{
			return Load (new XamlXmlReader (stream));
		}
		public static Object Load (TextReader textReader)
		{
			return Load (new XamlXmlReader (textReader));
		}
		public static Object Load (XmlReader xmlReader)
		{
			return Load (new XamlXmlReader (xmlReader));
		}
		public static Object Load (XamlReader xamlReader)
		{
			throw new NotImplementedException ();
		}

		public static Object Parse (string xaml)
		{
			return Load (new StringReader (xaml));
		}

		public static void Transform (XamlReader xamlReader, XamlWriter xamlWriter)
		{
			Transform (xamlReader, xamlWriter, true);
		}
		public static void Transform (XamlReader xamlReader, XamlWriter xamlWriter, bool closeWriter)
		{
			throw new NotImplementedException ();
		}
	}
}
