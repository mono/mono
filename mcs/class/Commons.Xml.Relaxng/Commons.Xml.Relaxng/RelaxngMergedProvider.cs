//
// RelaxngMergedProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
//

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
using System.Collections;
using Commons.Xml.Relaxng.XmlSchema;

using XSchema = System.Xml.Schema.XmlSchema;

namespace Commons.Xml.Relaxng
{
	public class RelaxngMergedProvider : RelaxngDatatypeProvider
	{
		static RelaxngMergedProvider defaultProvider;
		static RelaxngMergedProvider ()
		{
			RelaxngMergedProvider p = new RelaxngMergedProvider ();
#if !PNET
			p ["http://www.w3.org/2001/XMLSchema-datatypes"] = XsdDatatypeProvider.Instance;
			p [XSchema.Namespace] = XsdDatatypeProvider.Instance;
#endif
			p [String.Empty] = RelaxngNamespaceDatatypeProvider.Instance;
			defaultProvider = p;
		}

		public static RelaxngMergedProvider DefaultProvider {
			get { return defaultProvider; }
		}

		Hashtable table = new Hashtable ();

		public RelaxngMergedProvider ()
		{
		}

		public RelaxngDatatypeProvider this [string ns] {
			get { return table [ns] as RelaxngDatatypeProvider; }
			set { table [ns] = value; }
		}

		public override RelaxngDatatype GetDatatype (string name, string ns, RelaxngParamList parameters) {
			// TODO: parameter support (write schema and get type)

			RelaxngDatatypeProvider p = table [ns] as RelaxngDatatypeProvider;
			if (p == null)
				return null;
			return p.GetDatatype (name, ns, parameters);
		}
	}
}
