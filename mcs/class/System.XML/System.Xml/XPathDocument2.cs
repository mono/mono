//
// XPathDocument2.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
//
#if NET_1_2

using System;
using System.Collections;
using System.IO;
using System.Xml.Schema;

namespace System.Xml
{
	public class XPathDocument2
	{
//		XPathDocumentTree tree;
		bool acceptChangesOnLoad;
		XmlNameTable nameTable;
		bool createDefaultDocument;

		public  XPathDocument2 () : this (null, false) {}

		public  XPathDocument2 (XmlNameTable nameTable) : this (nameTable, false) {}

		public  XPathDocument2 (bool createDefaultDocument) : this (null, createDefaultDocument) {}

		// TODO
		public  XPathDocument2 (XmlNameTable nameTable, bool createDefaultDocument)
		{
			this.nameTable = nameTable;
			this.createDefaultDocument = createDefaultDocument;
		}

//		internal XPathDocumentTree Tree {
//			get { return tree; }
//		}

		internal void DeleteNode (XPathNavigator2 nav)
		{
			throw new NotImplementedException ();
		}

		public event XPathDocument2ChangedEventHandler ChangeRejected;

		public event XPathDocument2ChangedEventHandler ItemChanged;

		public event XPathDocument2ChangedEventHandler ItemChanging; 

		public event XPathDocument2ChangedEventHandler ItemInserted; 

		public event XPathDocument2ChangedEventHandler ItemInserting; 

		public event XPathDocument2ChangedEventHandler ItemRemoved; 

		public event XPathDocument2ChangedEventHandler ItemRemoving; 

		public event XPathDocument2ChangedEventHandler RejectingChange;

		public bool AcceptChangesOnLoad { 
			 get { return acceptChangesOnLoad; }
			 set { acceptChangesOnLoad = value; }
		}

		public bool DefaultRoot {
			 get { throw new NotImplementedException (); }
		}

		public XmlNameTable NameTable {
			 get { return nameTable; }
		}

		public void AcceptChanges ()
		{
			throw new NotImplementedException ();
		}

		public bool CheckValidity (XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}

		public XPathChangeNavigator CreateXPathChangeNavigator ()
		{
			throw new NotImplementedException ();
		}

		public XPathEditor CreateXPathEditor ()
		{
//			return new XPathDocumentEditor (this);
			throw new NotImplementedException ();
		}

		public XPathNavigator2 CreateXPathNavigator2 ()
		{
//			return new XPathDocumentNavigator2 (this);
			throw new NotImplementedException ();
		}

		public bool HasChanges ()
		{
			throw new NotImplementedException ();
		}

		public bool HasChanges (XmlChangeFilters changeFilter)  
		{
			throw new NotImplementedException ();
		}

		public bool IsDeletedFragment (XPathNavigator2 xmlNavigator, bool isPermanent)
		{
			throw new NotImplementedException ();
		}

		public bool IsDeletedFragment (XPathNavigator2 xmlNavigator)
		{
			throw new NotImplementedException ();
		}

		public void Load (string url)  
		{
			XmlTextReader xtr = new XmlTextReader (url);
			Load (xtr);
			xtr.Close ();
		}

		public void Load (TextReader reader)  
		{
			XmlTextReader xtr = new XmlTextReader (reader);
			Load (xtr);
		}

		public void Load (Stream stream)  
		{
			XmlTextReader xtr = new XmlTextReader (stream);
			Load (xtr);
		}

		public void LoadXml (string xml)
		{
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			Load (xtr);
			xtr.Close ();
		}

		public void Load (XmlReader xmlReader)  
		{
//			tree = new XPathDocumentTree (xmlReader);
			if (acceptChangesOnLoad)
				AcceptChanges ();
		}

		public void RejectChanges ()
		{
			throw new NotImplementedException ();
		}

		public void Validate (XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
		{
			throw new NotImplementedException ();
		}
	}

	public class XPathDocument2ChangedEventArgs : EventArgs
	{
		XPathDocument2ChangedEventAction action;

		internal XPathDocument2ChangedEventArgs (XPathDocument2ChangedEventAction action, XPathNavigator2 nav)
		{
			this.action = action;
			throw new NotImplementedException ();
		}

		public XPathDocument2ChangedEventAction Action {
			 get { return action; }
		}

		public XPathNavigator2 Item {
			 get { throw new NotImplementedException (); }
		}
		public XPathNavigator2 NewParent {
			 get { throw new NotImplementedException (); }
		}
		public XPathNavigator2 NewPreviousItem {
			 get { throw new NotImplementedException (); }
		}
		public string NewValue {
			 get { throw new NotImplementedException (); }
		}
		public XPathNavigator2 OldParent {
			 get { throw new NotImplementedException (); }
		}
		public XPathNavigator2 OldPreviousItem {
			 get { throw new NotImplementedException (); }
		}
		public string OldValue {
			 get { throw new NotImplementedException (); }
		}
	}
}

#endif
