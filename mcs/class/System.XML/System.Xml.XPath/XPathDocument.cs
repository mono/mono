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
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Mono.Xml.XPath;

namespace System.Xml.XPath
{
#if NET_2_0
	[XmlSchemaProvider ("GetSchema")]
	public class XPathDocument : IXPathNavigable, IXPathEditable,
		IChangeTracking, IRevertibleChangeTracking, IXmlSerializable
#else
	public class XPathDocument : IXPathNavigable
#endif
	{
		DTMXPathDocument document;
#if NET_2_0
		XPathEditableDocument editable;
#endif

#region Constructors

#if NET_2_0
		[MonoTODO]
		public XPathDocument ()
			: this (new NameTable ())
		{
		}

		[MonoTODO]
		public XPathDocument (XmlNameTable nameTable)
		{
			editable = new XPathEditableDocument (new XmlDocument (nameTable));
		}

		public XPathDocument (Stream stream)
			: this (stream, true)
		{
		}

		public XPathDocument (string uri) 
			: this (uri, XmlSpace.None, true)
		{
		}

		public XPathDocument (string uri, bool acceptChangesOnLoad) 
			: this (uri, XmlSpace.None, acceptChangesOnLoad)
		{
		}

		public XPathDocument (TextReader reader)
			: this (reader, true)
		{
		}

		[MonoTODO]
		public XPathDocument (XmlReader reader)
			: this (reader, XmlSpace.None, true)
		{
		}

		[MonoTODO]
		public XPathDocument (XmlReader reader, bool acceptChangesOnLoad)
			: this (reader, XmlSpace.None, acceptChangesOnLoad)
		{
		}

		[MonoTODO]
		public XPathDocument (string uri, XmlSpace space)
			: this (uri, space, true)
		{
		}

		[MonoTODO]
		public XPathDocument (XmlReader reader, XmlSpace space)
			: this (reader, space, true)
		{
		}

		[MonoTODO]
		public XPathDocument (string uri, XmlSpace space, bool acceptChangesOnLoad)
		{
			XmlValidatingReader vr = null;
			try {
				vr = new XmlValidatingReader (new XmlTextReader (uri));
				vr.ValidationType = ValidationType.None;
				Initialize (vr, space, acceptChangesOnLoad);
			} finally {
				if (vr != null)
					vr.Close ();
			}
		}

		[MonoTODO]
		public XPathDocument (Stream stream, bool acceptChangesOnLoad)
		{
			XmlValidatingReader vr = new XmlValidatingReader (new XmlTextReader (stream));
			vr.ValidationType = ValidationType.None;
			Initialize (vr, XmlSpace.None, acceptChangesOnLoad);
		}

		[MonoTODO]
		public XPathDocument (TextReader reader, bool acceptChangesOnLoad)
		{
			XmlValidatingReader vr = new XmlValidatingReader (new XmlTextReader (reader));
			vr.ValidationType = ValidationType.None;
			Initialize (vr, XmlSpace.None, acceptChangesOnLoad);
		}

		[MonoTODO]
		public XPathDocument (XmlReader reader, XmlSpace space, bool acceptChangesOnLoad)
		{
			Initialize (reader, space, acceptChangesOnLoad);
		}
#else
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
#endif

		private void Initialize (XmlReader reader, XmlSpace space)
		{
			document = new DTMXPathDocumentBuilder (reader, space).CreateDocument ();
		}

		private void Initialize (XmlReader reader, XmlSpace space, bool acceptChangesOnLoad)
		{
			document = new DTMXPathDocumentBuilder (reader, space).CreateDocument ();
		}

#endregion

#region Events

#if NET_2_0

		public event NodeChangedEventHandler ChangeRejected;

		public event NodeChangedEventHandler ItemUpdated;

		public event NodeChangedEventHandler ItemUpdating;

		public event NodeChangedEventHandler ItemInserted;

		public event NodeChangedEventHandler ItemInserting;

		public event NodeChangedEventHandler ItemDeleted;

		public event NodeChangedEventHandler ItemDeleting;

		public event NodeChangedEventHandler RejectingChange;

#endif // NET_2_0

#endregion // Events

#region Properties

#if NET_2_0

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
		bool IChangeTracking.IsChanged {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlNameTable NameTable {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool PreserveWhiteSpace {
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
		public void AcceptChanges ()
		{
			throw new NotImplementedException ();
		}

		/* It will disappear in 2.0 RTM
		[MonoTODO]
		public XPathChangeNavigator CreateChangeNavigator ()
		{
			throw new NotImplementedException ();
		}
		*/

		[MonoTODO]
		public XPathEditableNavigator CreateEditor ()
		{
			if (editable == null)
				throw new NotImplementedException ();
			return editable.CreateEditor ();
		}

		[MonoTODO ("This code is only for compatibility.")]
		public XPathNavigator CreateNavigator ()
		{
			if (editable == null)
				return document.CreateNavigator ();
			else
				return editable.CreateNavigator ();
		}

		public XmlWriter CreateWriter ()
		{
			return CreateEditor ().AppendChild ();
		}

		[MonoTODO]
		public virtual XmlSchema GetSchema ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static XmlQualifiedName GetXPathDocumentSchema (XmlSchemaSet schemas)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasChanges ()
		{
			if (editable == null)
				throw new NotImplementedException ();
			return editable.HasChanges ();
		}

		[Obsolete]
		[MonoTODO]
		public void LoadXml (string xml)  
		{
			throw new NotImplementedException ();
//			tree = new XPathDocumentTree (xmlReader);
//			if (acceptChangesOnLoad)
//				AcceptChanges ();
		}

		[MonoTODO]
		public void ReadXml (XmlReader reader)
		{
			if (editable == null)
				throw new NotImplementedException ();
			editable.ReadXml (reader);
		}

		[MonoTODO]
		public void RejectChanges ()
		{
			if (editable == null)
				throw new NotImplementedException ();
			editable.RejectChanges ();
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

		[MonoTODO]
		public void WriteXml (XmlWriter writer)
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


