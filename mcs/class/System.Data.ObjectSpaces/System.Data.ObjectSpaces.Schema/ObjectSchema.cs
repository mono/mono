//
// System.Data.ObjectSpaces.Schema.ObjectSchema.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Mapping;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace System.Data.ObjectSpaces.Schema {
	public class ObjectSchema : ICloneable, IDomainSchema
	{
		#region Fields

		string name;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ObjectSchema ()
		{
		}

		[MonoTODO]
		public ObjectSchema (string url)
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public SchemaClassCollection Classes {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ExtendedPropertyCollection ExtendedProperties {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		private MappingDataSourceType IDomainSchema.DomainType {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO ("Verify")]
		public string Name {
			get { return name; }
			set { name = value; }
		}

		[MonoTODO]
		public string Namespace {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ObjectRelationshipCollection Relationships {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string SourceUri {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public Object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetSchemaXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private IDomainConstraint IDomainSchema.GetDomainConstraint (string select, IXmlNamespaceResolver namespaces)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private IDomainStructure IDomainSchema.GetDomainStructure (string select, IXmlNamespaceResolver namespaces)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Read (string url)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Read (string url, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Read (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Read (XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.ReadExtensions (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.ReadExtensions (XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Write (string schemaLocation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Write (string schemaLocation, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Write (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Write (XmlWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Write (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Write (Stream stream, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Write (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.Write (TextWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.WriteExtensions (XmlWriter reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void IDomainSchema.WriteExtensions (XmlWriter reader, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (string url)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (string url, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Read (XmlReader reader, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Reset ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (string schemaLocation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (string schemaLocation, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (XmlWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (Stream stream, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Write (TextWriter writer, IXmlNamespaceResolver namespaceResolver)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif // NET_1_2
