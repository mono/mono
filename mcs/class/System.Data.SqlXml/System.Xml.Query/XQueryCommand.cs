//
// System.Xml.Query.XQueryCommand
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Novell Inc., 2004
//

#if NET_2_0

using System.Data;
using System.Data.SqlXml;
using System.IO;

namespace System.Xml.Query 
{
	public class XQueryCommand
	{
		#region Constructors

		[MonoTODO]
		public XQueryCommand ()
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public SqlQueryOptions SqlQueryOptions {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlMappingDictionary XmlMappingDictionary {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		// Compile

		[MonoTODO ("Null Evidence allowed?")]
		public void Compile (string query)
		{
			Compile (query, null);
		}

		[MonoTODO]
		public void Compile (string query, Evidence evidence)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Null Evidence allowed?")]
		public void Compile (TextReader query)
		{
			Compile (query, null);
		}

		[MonoTODO]
		public void Compile (TextReader query, Evidence evidence)
		{
			throw new NotImplementedException ();
		}

		// CompileView

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

		// Execute

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlWriter results)
		{
			Execute (contextDocument, null, null, results);
		}

		[MonoTODO ("Output StartDocument?")]
		public void Execute (
			XmlResolver dataSources, 
			TextWriter results)
		{
			XmlTextWriter w = new XmlTextWriter (results);
			Execute (dataSources, null, w);
		}

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			XmlResolver dataSources, 
			XmlWriter results)
		{
			Execute ((IXPathNavigable) null, dataSources, null, results);
		}

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlArgumentList args,
			XmlWriter results)
		{
			Execute (contextDocument, null, args, results);
		}

		[MonoTODO ("Null args allowed?")]
		public void Execute (
			IXPathNavigable contextDocument,
			XmlResolver resolver,
			XmlWriter results)
		{
			Execute (contextDocument, resolver, null, results);
		}

		[MonoTODO("Indentation?;write StartDocument?;Null args allowed?")]
		public void Execute (
			string contextDocumentUri,
			XmlResolver dataSources,
			Stream results)
		{
			Execute (contextDocumentUri, dataSources, null, results);
		}

		[MonoTODO("Indentation?;write StartDocument?;Null args allowed?")]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			TextWriter results)
		{
			Execute (contextDocumentUri, dataSources, null, results);
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources, 
			XmlWriter results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (
			XmlResolver dataSources, 
			XmlArgumentList args,
			XmlWriter results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (
			IXPathNavigable contextDocument, 
			XmlResolver dataSources,
			XmlArgumentList args,
			XmlWriter results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources,
			XmlArgumentList args,
			Stream results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources,
			XmlArgumentList args,
			TextWriter results)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Execute (
			string contextDocumentUri, 
			XmlResolver dataSources,
			XmlArgumentList args,
			XmlWriter results)
		{
			throw new NotImplementedException ();
		}

		// ExecuteView

		[MonoTODO]
		public void ExecuteView (IDbConnection connection, TextWriter results)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
