//
// XmlAdapter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2003 Novell inc.
//
#if NET_1_2
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
