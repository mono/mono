//
// XmlAdapter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2003 Novell inc.
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
#if NET_2_0
using System;
using System.Collections;


namespace System.Xml
{
	public class XmlAdapter
	{
		public XmlAdapter ()
		{
			throw new NotImplementedException ();
		}

		public XmlAdapter (XmlResolver dataSource)
		{
			throw new NotImplementedException ();
		}

		public event UpdateEventHandler OnUpdateError;

		public void Fill (XPathDocument2 doc, XmlCommand query)
		{
			throw new NotImplementedException ();
		}

		public void Fill (XPathDocument2 doc, XmlCommand query, XmlQueryArgumentList argumentList)
		{
			throw new NotImplementedException ();
		}

		public void Update (IEnumerable changes, MappingSchema mappingSchema)
		{
			throw new NotImplementedException ();
		}

		public void Update (XPathChangeNavigator navigator, MappingSchema mappingSchema)
		{
			throw new NotImplementedException ();
		}

		public void Update (XPathDocument2 doc, MappingSchema mappingSchema)
		{
			throw new NotImplementedException ();
		}

		public bool AcceptChangesDuringFill {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public bool AcceptChangesDuringUpdate {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public bool ContinueUpdateOnError {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public XmlResolver DataSources {
			set { throw new NotImplementedException (); }
		}
	}

}


#endif
