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
#if NET_2_0obsolete
	[XmlSchemaProvider ("GetSchema")]
	public class XPathDocument : IXPathNavigable//, IXPathEditable,
//		IChangeTracking, IRevertibleChangeTracking, IXmlSerializable
	{
		// FIXME: In the future this switch will disappear.
		// Regardless of this switch, those constructors that does 
		// not take input document use editable XPathDocument.
		static bool useEditable;

		static XPathDocument ()
		{
			// FIXME: remove when new XPathDocument2 got more 
			// stable. This environment value is temporary.
			if (Environment.GetEnvironmentVariable ("MONO_XPATH_DOCUMENT_2") == "yes")
				useEditable = true;
		}

		XPathDocument2Editable editable;
		DTMXPathDocument dtm;

		XmlSchemaSet schemas;

		// save parameters
		Encoding encoding;
		bool preserveWhitespace;

#region Constructors
		[MonoTODO]
		public XPathDocument ()
			: this (new NameTable ())
		{
		}

		[MonoTODO]
		public XPathDocument (XmlNameTable nameTable)
		{
			editable = new XPathDocument2Editable (new XPathDocument2 (nameTable));

			InitializeEvents ();
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

		private void Initialize (XmlReader reader, XmlSpace space, bool acceptChangesOnLoad)
		{
			if (useEditable)
				InitializeEditable (reader, space, acceptChangesOnLoad);
			else
				dtm = new DTMXPathDocumentBuilder (reader, space).CreateDocument ();
		}

		private void InitializeEditable (XmlReader reader, XmlSpace space, bool acceptChangesOnLoad)
		{
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.NameTable = reader.NameTable;
			settings.IgnoreWhitespace = (space == XmlSpace.Preserve);
			XmlReader r = XmlReader.Create (reader, settings);
			XPathDocument2 doc = new XPathDocument2 ();
			doc.Load (r, space);
			editable = new XPathDocument2Editable (doc);
			if (acceptChangesOnLoad)
				AcceptChanges ();
			this.preserveWhitespace = space == XmlSpace.Preserve;
			this.schemas = reader.Settings != null ? reader.Settings.Schemas : null;

			InitializeEvents ();
		}

		private void InitializeEvents ()
		{
			editable.ChangeRejected += this.ChangeRejected;
			editable.ItemUpdated += this.ItemUpdated;
			editable.ItemUpdating += this.ItemUpdating;
			editable.ItemInserted += this.ItemInserted;
			editable.ItemInserting += this.ItemInserting;
			editable.ItemDeleted += this.ItemDeleted;
			editable.ItemDeleting += this.ItemDeleting;
			editable.RejectingChange += this.RejectingChange;
		}
#endregion

#region Events

		public event NodeChangedEventHandler ChangeRejected;

		public event NodeChangedEventHandler ItemUpdated;

		public event NodeChangedEventHandler ItemUpdating;

		public event NodeChangedEventHandler ItemInserted;

		public event NodeChangedEventHandler ItemInserting;

		public event NodeChangedEventHandler ItemDeleted;

		public event NodeChangedEventHandler ItemDeleting;

		public event NodeChangedEventHandler RejectingChange;

#endregion // Events

#region Properties

		[MonoTODO]
		public bool EnableChangeTracking {
			get { return editable.EnableChangeTracking; }
			set { editable.EnableChangeTracking = value; }
		}

		public Encoding Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		[MonoTODO]
		bool IChangeTracking.IsChanged {
			get { return editable.IsChanged; }
		}

		public XmlNameTable NameTable {
			get { return editable.NameTable; }
		}

		public bool PreserveWhiteSpace {
			get { return preserveWhitespace; }
		}

		public XmlSchemaSet Schemas {
			get { return schemas; }
			set { schemas = value; }
		}

#endregion // Properies

#region Methods
		[MonoTODO]
		public void AcceptChanges ()
		{
			editable.AcceptChanges ();
		}

		/* It will disappear in 2.0 RTM
		[MonoTODO]
		public XPathChangeNavigator CreateChangeNavigator ()
		{
			throw new NotImplementedException ();
		}
		*/

		public XPathEditableNavigator CreateEditor ()
		{
			return editable.CreateEditor ();
		}

		[MonoTODO ("Remove switch")]
		public XPathNavigator CreateNavigator ()
		{
			if (editable != null)
				return editable.CreateNavigator ();
			else
				return dtm.CreateNavigator ();
		}

		public XmlWriter CreateWriter ()
		{
			return CreateEditor ().AppendChild ();
		}

		[MonoTODO]
		public virtual XmlSchema GetSchema ()
		{
			return editable.GetSchema ();
		}

		[MonoTODO]
		public static XmlQualifiedName GetXPathDocumentSchema (XmlSchemaSet schemas)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasChanges ()
		{
			return editable.HasChanges ();
		}

		/* It will disappear in 2.0 RTM
		[Obsolete]
		[MonoTODO]
		public void LoadXml (string xml)  
		{
			throw new NotImplementedException ();
//			tree = new XPathDocumentTree (xmlReader);
//			if (acceptChangesOnLoad)
//				AcceptChanges ();
		}
		*/

		public void ReadXml (XmlReader reader)
		{
			editable.ReadXml (reader);
		}

		[MonoTODO]
		public void RejectChanges ()
		{
			editable.RejectChanges ();
		}

		[MonoTODO ("Confirm writer settings etc.")]
		public void Save (Stream stream)
		{
			Save (new XmlTextWriter (stream, encoding));
		}

		[MonoTODO ("Confirm writer settings etc.")]
		public void Save (string filename)
		{
			using (XmlWriter w = new XmlTextWriter (filename, encoding)) {
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
			writer.WriteNode (CreateNavigator ().ReadSubtree (), false);
		}

		[MonoTODO]
		public XPathNodeIterator SelectNodes (string xpath)
		{
			return CreateEditor ().Select (xpath);
		}

		[MonoTODO]
		public XPathNodeIterator SelectNodes (XPathExpression expr)
		{
			return CreateEditor ().Select (expr);
		}

		[MonoTODO]
		public XPathNodeIterator SelectNodes (string xpath ,IXmlNamespaceResolver nsResolver)
		{
			return CreateEditor ().Select (xpath, nsResolver);
		}

		[MonoTODO]
		public XPathEditableNavigator SelectSingleNode (string xpath)
		{
			XPathNodeIterator iter = CreateEditor ().Select (xpath);
			if (iter.MoveNext ())
				return (XPathEditableNavigator) iter.Current;
			else
				return null;
		}

		[MonoTODO]
		public XPathEditableNavigator SelectSingleNode (XPathExpression expr)
		{
			XPathNodeIterator iter = CreateEditor ().Select (expr);
			if (iter.MoveNext ())
				return (XPathEditableNavigator) iter.Current;
			else
				return null;
		}

		[MonoTODO]
		public XPathEditableNavigator SelectSingleNode (string xpath ,IXmlNamespaceResolver nsResolver)
		{
			XPathNodeIterator iter = CreateEditor ().Select (xpath, nsResolver);
			if (iter.MoveNext ())
				return (XPathEditableNavigator) iter.Current;
			else
				return null;
		}

		[MonoTODO]
		public void WriteXml (XmlWriter writer)
		{
			Save (writer);
		}
#endregion
	}


#else // !NET_2_0

	public class XPathDocument : IXPathNavigable
	{
		DTMXPathDocument document;

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

		private void Initialize (XmlReader reader, XmlSpace space, bool acceptChangesOnLoad)
		{
			document = new DTMXPathDocumentBuilder (reader, space).CreateDocument ();
		}

		public XPathNavigator CreateNavigator ()
		{
			return document.CreateNavigator ();
		}
	}

#endif

}


