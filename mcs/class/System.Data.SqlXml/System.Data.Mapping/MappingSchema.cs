//
// System.Data.Mapping.MappingSchema
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlXml;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace System.Data.Mapping {
        public class MappingSchema
        {
		#region Constructors

		[MonoTODO]
		public MappingSchema ()
		{
		}

		[MonoTODO]
		public MappingSchema (string url, ValidationEventHandler validationEventHandler)
		{
		}

		[MonoTODO]
		public MappingSchema (string url)
		{
		}

		[MonoTODO]
		public MappingSchema (XmlReader reader, ValidationEventHandler validationEventHandler)
		{
		}

		[MonoTODO]
		public MappingSchema (XmlReader reader)
		{
		}

		#endregion // Constructors

		#region Properties
	
		[MonoTODO]
		public DataSourceCollection DataSources {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MappingParameterCollection MappingParameters {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MapCollection Maps {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public RelationshipMapCollection RelationshipMaps {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string SourceUri {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public Relationship Find (string fromVariableName, string toVariableName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RelationshipCollection Find (string variableName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (string url, ValidationEventHandler validationEventHandler) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (string url) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (string url, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (string url)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (XmlWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (Stream stream, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (TextWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
