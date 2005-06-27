//
// XQueryCompileContext.cs
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
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using Mono.Xml.XPath2;

namespace Mono.Xml.XPath2
{
	// Holds dynamic compile context that is shared between one or more
	// compilers that are created during one XQueryCommand.Compile().
	internal class XQueryCompileContext
	{
		public XQueryCompileContext ()
		{
			schemaCache = new Hashtable ();
			moduleCache = new Hashtable ();

			inEffectSchemas = new XmlSchemaSet ();
			inEffectVariables = new Hashtable ();
			inEffectFunctions = new XQueryFunctionTable ();
		}

		// Compiled schema table; It is used to avoid multiple time
		// compilation of the same schemas that is likely to happen
		// when a library module is divided into multiple files.
		// [location string] -> XmlSchema (done) or null (not).
		IDictionary schemaCache;
		// ditto for local module resources.
		// [location] -> XQueryLibraryModule (done) or null (not).
		IDictionary moduleCache;

		// Collects the whole schemas, variables and functions.
		XmlSchemaSet inEffectSchemas;
		Hashtable inEffectVariables;
		XQueryFunctionTable inEffectFunctions;

		public IDictionary SchemaCache {
			get { return schemaCache; }
		}

		public IDictionary ModuleCache {
			get { return moduleCache; }
		}

		// Compilation results

		public XmlSchemaSet InEffectSchemas {
			get { return inEffectSchemas; }
		}

		public Hashtable InEffectVariables {
			get { return inEffectVariables; }
		}

		public XQueryFunctionTable InEffectFunctions {
			get { return inEffectFunctions; }
		}
	}
}

#endif
