//
// System.Xml.Query.XQueryProcessor
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data;
using System.Data.SqlXml;
using System.IO;

namespace System.Xml.Query {
        public class XQueryProcessor
        {
		#region Fields
		
		XmlCommand xmlCommand;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public XQueryProcessor ()
		{
		}

		#endregion // Constructors

		#region Properties
	
		[MonoTODO]
		public SqlQueryOptions SqlQueryOptions {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public XmlCommand XmlCommand {
			get { return xmlCommand; }
		}

		[MonoTODO]
		public XmlViewSchemaDictionary XmlViewSchemaDictionary {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Compile (string query)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Compile (TextReader query)
		{
			// Should generate an XmlCommand

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CompileView (string query, string mappingLocation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CompileView (TextReader query, string mappingLocation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (string contextDocumentUri, XmlResolver dataSources, TextWriter results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (XmlResolver dataSources, TextWriter results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExecuteView (IDbConnection connection, TextWriter results)
		{
			throw new NotImplementedException ();
		}


		#endregion // Methods
        }
}

#endif // NET_1_2
