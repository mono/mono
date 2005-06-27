//
// XQueryCompileOptions.cs - XQuery compiler option stucture.
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using Mono.Xml.XPath2;

namespace Mono.Xml.XPath2
{
	public class XQueryCompileOptions
	{
		public XQueryCompileOptions ()
			: this (new NameTable (), null)
		{
		}

		public XQueryCompileOptions (XmlNameTable nameTable, CultureInfo defaultCollation)
		{
			this.nameTable = nameTable;
			this.defaultCollation = defaultCollation;
			if (this.defaultCollation == null)
				this.defaultCollation = CultureInfo.InvariantCulture;

			knownCollections = new Hashtable ();
		}

		XmlNameTable nameTable;
		XmlQueryDialect compat;
		CultureInfo defaultCollation;
		Hashtable knownCollections;
		bool xqueryFlagger;
		bool xqueryStaticFlagger;

		// XPath 1.0 Compatibility Mode.
		public XmlQueryDialect Compatibility {
			get { return compat; }
			set { compat = value; }
		}

		public XmlNameTable NameTable {
			get { return nameTable; }
			set { nameTable = value; }
		}

		public CultureInfo DefaultCollation {
			get { return defaultCollation; }
			set { defaultCollation = value; }
		}

		// FIXME: implement
		public bool XQueryFlagger {
			get { return xqueryFlagger; }
			set { xqueryFlagger = value; }
		}

		// FIXME: implement
		public bool XQueryStaticFlagger {
			get { return xqueryStaticFlagger; }
			set { xqueryStaticFlagger = value; }
		}

		// FIXME: implement
		public Hashtable KnownCollections {
			get { return knownCollections; }
		}
	}
}

#endif
