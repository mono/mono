//
// System.Xml.XPath.XPathDocument
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) Copyright 2002 Tim Coleman
// (C) 2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;
using Mono.Xml.XPath;

namespace System.Xml.XPath
{

	public class XPathDocument : IXPathNavigable
	{
		DTMXPathDocument document;

#region Constructors

		public XPathDocument (Stream stream)
		{
			XmlValidatingReader vr = new XmlValidatingReader (new XmlTextReader (stream));
			vr.ValidationType = ValidationType.None;
			Initialize (vr, XmlSpace.None);
		}

		public XPathDocument (string uri) 
			: this (uri, XmlSpace.None)
		{
		}

		public XPathDocument (TextReader reader)
		{
			XmlValidatingReader vr = new XmlValidatingReader (new XmlTextReader (reader));
			vr.ValidationType = ValidationType.None;
			Initialize (vr, XmlSpace.None);
		}

		public XPathDocument (XmlReader reader)
			: this (reader, XmlSpace.None)
		{
		}

		public XPathDocument (string uri, XmlSpace space)
		{
			XmlValidatingReader vr = null;
			try {
				vr = new XmlValidatingReader (new XmlTextReader (uri));
				vr.ValidationType = ValidationType.None;
				Initialize (vr, space);
			} finally {
				if (vr != null)
					vr.Close ();
			}
		}

		public XPathDocument (XmlReader reader, XmlSpace space)
		{
			Initialize (reader, space);
		}

		private void Initialize (XmlReader reader, XmlSpace space)
		{
			document = new DTMXPathDocumentBuilder (reader, space).CreateDocument ();
		}

#endregion

#region Events

#if NET_2_0

		public event NodeChangedEventHandler ChangeRejected;

		public event NodeChangedEventHandler ItemChanged;

		public event NodeChangedEventHandler ItemChanging;

		public event NodeChangedEventHandler ItemInserted;

		public event NodeChangedEventHandler ItemInserting;

		public event NodeChangedEventHandler ItemRemoved;

		public event NodeChangedEventHandler ItemRemoving;

		public event NodeChangedEventHandler RejectingChange;

#endif // NET_2_0

#endregion // Events

#region Properties

#if NET_2_0

		[MonoTODO]
		public virtual bool ContainsListCollection {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool EnableChangeTracking {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Encoding Encoding {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlNameTable NameTable {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool PreserveWhitespace {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlSchemaSet Schemas {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

#endif // NET_2_0

#endregion // Properies


#region Methods

#if NET_2_0

		[MonoTODO]
		public XPathChangeNavigator CreateChangeNavigator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathEditableNavigator CreateEditor ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("This code is only for compatibility.")]
		public XPathNavigator CreateNavigator ()
		{
			return document.CreateNavigator ();
		}

		[MonoTODO]
		public XmlWriter CreateWriter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IList GetList ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasChanges ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasChanges (XmlChangeFilters changeFilter)  
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Load (string xml)  
		{
			throw new NotImplementedException ();
//			tree = new XPathDocumentTree (xmlReader);
//			if (acceptChangesOnLoad)
//				AcceptChanges ();
		}

		[MonoTODO]
		public void RejectChanges ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Confirm writer settings etc.")]
		public void Save (Stream stream)
		{
			Save (new XmlTextWriter (stream, null));
		}

		[MonoTODO ("Confirm writer settings etc.")]
		public void Save (string filename)
		{
			using (XmlWriter w = new XmlTextWriter (filename, null)) {
				Save (w);
			}
		}

		[MonoTODO ("Confirm writer settings etc.")]
		public void Save (TextWriter writer)
		{
			Save (new XmlTextWriter (writer));
		}

		[MonoTODO]
		public void Save (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathNodeIterator SelectNodes (string xpath)
		{
			return SelectNodes (xpath, null);
		}

		[MonoTODO]
		public XPathNodeIterator SelectNodes (XPathExpression expr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathNodeIterator SelectNodes (string xpath ,IXmlNamespaceResolver nsResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathEditableNavigator SelectSingleNode (string xpath)
		{
			return SelectSingleNode (xpath, null);
		}

		[MonoTODO]
		public XPathEditableNavigator SelectSingleNode (XPathExpression expr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XPathEditableNavigator SelectSingleNode (string xpath ,IXmlNamespaceResolver nsResolver)
		{
			throw new NotImplementedException ();
		}

#else // !NET_2_0

		public XPathNavigator CreateNavigator ()
		{
			return document.CreateNavigator ();
		}

#endif

#endregion

	}

}


