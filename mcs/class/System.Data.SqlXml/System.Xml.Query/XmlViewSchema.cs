//
// System.Xml.Query.XmlViewSchema
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data.SqlXml;
using System.Xml.Schema;

namespace System.Xml.Query {
        public abstract class XmlViewSchema
        {
		#region Properties

		[MonoTODO]
		public XmlSchema ResultsSchema { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IEnumerator SourceDataNames { 
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
