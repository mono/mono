//
// System.Xml.Query.XsltProcessor
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;
using System.IO;

namespace System.Xml.Query {
        public class XsltProcessor
        {
		#region Constructors
	
		[MonoTODO]
		public XsltProcessor ()
		{
		}

		#endregion // Constructors

		#region Properties
	
		[MonoTODO]
		public XmlCommand XmlCommand {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Compile (string stylesheetUri, XmlResolver resolver)
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Compile (string stylesheetUri)
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (string contextDocumentUri, XmlResolver dataSources, XmlQueryArgumentList argList, TextWriter results)
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (string contextDocumentUri, XmlResolver dataSources, XmlQueryArgumentList argList, Stream results)
		{ 
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (string contextDocumentUri, string resultDocumentUri)
		{ 
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
