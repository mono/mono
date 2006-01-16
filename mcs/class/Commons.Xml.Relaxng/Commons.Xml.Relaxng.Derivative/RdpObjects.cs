//
// Commons.Xml.Relaxng.Derivative.RdpObjects.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
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
using System.Collections.Specialized;
using System.Xml;

namespace Commons.Xml.Relaxng.Derivative
{
	///
	/// Datatype Related Classes
	///
	public class RdpParamList : ArrayList
	{
		public RdpParamList () : base ()
		{
		}
	}

	public class RdpParam
	{
		public RdpParam (string localName, string value)
		{
			this.localName = localName;
			this.value = value;
		}

		string value;
		public string Value {
			get { return this.value; }
		}

		string localName;
		public string LocalName {
			get { return localName; }
		}
	}

	public class RdpDatatype
	{
		//RelaxngDatatypeProvider provider;
		string localName;
		string ns;
		RelaxngDatatype datatype;

		public RdpDatatype (string ns, string localName, RelaxngParamList parameters, RelaxngDatatypeProvider provider)
		{
			this.ns = ns;
			this.localName = localName;
			//this.provider = provider;
			if (provider == null)
				provider = RelaxngMergedProvider.DefaultProvider;
			datatype = provider.GetDatatype (localName, ns, parameters);
			if (datatype == null) {
				throw new RelaxngException (String.Format ("Invalid datatype was found for namespace '{0}' and local name '{1}'", ns, localName));
			}
		}

		public string NamespaceURI {
			get { return ns; }
		}

		public string LocalName {
			get { return localName; }
		}

		public bool IsContextDependent {
			get { return datatype.IsContextDependent; }
		}

		public virtual bool IsAllowed (string value, XmlReader reader)
		{
			return datatype.IsValid (value, reader);
		}

		public virtual bool IsTypeEqual (string s1, string s2, XmlReader reader)
		{
			return datatype.CompareString (s1, s2, reader);
		}
	}

}

