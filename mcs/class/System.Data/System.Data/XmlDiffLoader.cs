//
// mcs/class/System.Data/System.Data/XmlDiffLoader.cs
//
// Purpose: Loads XmlDiffGrams to DataSet 
//
// class: XmlDiffLoader
// assembly: System.Data.dll
// namespace: System.Data
//
// Author:
//     Ville Palo <vi64pa@koti.soon.fi>
//     Lluis Sanchez Gual (lluis@ximian.com)
//
// (c)copyright 2003 Ville Palo
//
using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Globalization;

namespace System.Data {

	internal class XmlDiffLoader 
	{

		#region Fields
		private DataSet DSet;
		private DataTable table;
		private Hashtable DiffGrRows = new Hashtable ();
		private Hashtable ErrorRows = new Hashtable ();

		#endregion // Fields

		#region ctors

		public XmlDiffLoader (DataSet DSet) 
		{
			this.DSet = DSet;
		}

		public XmlDiffLoader (DataTable table) 
		{
			this.table = table;
		}

		#endregion //ctors

		#region Public methods

		public void Load (XmlReader reader) 
		{
			bool origEnforceConstraint = false;
			if (DSet != null) {
				origEnforceConstraint = DSet.EnforceConstraints;
				DSet.EnforceConstraints = false;
			}
			
			reader.MoveToContent ();
			if (reader.IsEmptyElement) return;
			
			reader.ReadStartElement ("diffgram", XmlConstants.DiffgrNamespace);
			reader.MoveToContent ();
			
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "before" && reader.NamespaceURI == XmlConstants.DiffgrNamespace)
						LoadBefore (reader);
					else if (reader.LocalName == "errors" && reader.NamespaceURI == XmlConstants.DiffgrNamespace)
						LoadErrors (reader);
					else
						LoadCurrent (reader);
				}
				else
					reader.Skip ();
			}
			
			reader.ReadEndElement ();
			
			if (DSet != null)
				DSet.EnforceConstraints = origEnforceConstraint;
		}

		#endregion // Public methods

		#region Private methods

		private void LoadCurrent (XmlReader reader) 
		{
			if (reader.IsEmptyElement) return;
			
			reader.ReadStartElement ();		// Dataset root
			reader.MoveToContent ();

			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					DataTable t = GetTable (reader.LocalName);
					if (t != null)
						LoadCurrentTable (t, reader);
					else 
						throw new DataException (Locale.GetText ("Cannot load diffGram. Table '" + reader.LocalName + "' is missing in the destination dataset"));	
				}
				else
					reader.Skip ();
			}
			
			reader.ReadEndElement ();
		}

		private void LoadBefore (XmlReader reader) 
		{
			if (reader.IsEmptyElement) return;
			
			reader.ReadStartElement ();
			reader.MoveToContent ();

			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					DataTable t = GetTable (reader.LocalName);
					if (t != null)
						LoadBeforeTable(t, reader);
					else
						throw new DataException (Locale.GetText ("Cannot load diffGram. Table '" + reader.LocalName + "' is missing in the destination dataset"));
				}
				else
					reader.Skip ();
			}
			
			reader.ReadEndElement ();
		}				 
				
					   
		private void LoadErrors (XmlReader reader) 
		{
			if (reader.IsEmptyElement) return;
			
			reader.ReadStartElement ();
			reader.MoveToContent ();

			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					DataRow Row = null;
		
					// find the row in 'current' section
					
					string id = reader.GetAttribute ("id", XmlConstants.DiffgrNamespace);
					
					if (id != null)
						Row = (DataRow) ErrorRows [id];
		
					if (reader.IsEmptyElement) continue;
					reader.ReadStartElement ();
					while (reader.NodeType != XmlNodeType.EndElement)
					{
						if (reader.NodeType == XmlNodeType.Element) {
							string error = reader.GetAttribute ("Error", XmlConstants.DiffgrNamespace);
							Row.SetColumnError (reader.LocalName, error);
						}
						reader.Read ();
					}
				}
				else
					reader.Skip ();
			}
			reader.ReadEndElement ();
		}

		private void LoadColumns (DataTable Table, DataRow Row, XmlReader reader, DataRowVersion loadType) 
		{
			if (reader.IsEmptyElement) return;
			
			reader.ReadStartElement ();
			reader.MoveToContent ();
			
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType != XmlNodeType.Element) { reader.Read (); continue; }
				
				if (Table.Columns.Contains (reader.LocalName)) 
				{
					string colName = reader.LocalName;
					object data = XmlDataLoader.StringToObject (Table.Columns[colName].DataType, reader.ReadString ());
					
					if (loadType == DataRowVersion.Current) Row [colName] = data;
					else Row.SetOriginalValue (colName, data);
					reader.Read ();
				}
				else 
				{
					DataTable t = GetTable (reader.LocalName);
					if (t != null) {
						if (loadType == DataRowVersion.Original)
							LoadBeforeTable (t, reader);
						else if (loadType == DataRowVersion.Current)
							LoadCurrentTable (t, reader);
					}
				}
			}
			
			reader.ReadEndElement ();
		}

		private void LoadBeforeTable (DataTable Table, XmlReader reader) 
		{
			string id = reader.GetAttribute ("id", XmlConstants.DiffgrNamespace);
			string rowOrder = reader.GetAttribute ("rowOrder", XmlConstants.MsdataNamespace);
			DataRow Row = (DataRow) DiffGrRows [id];
			
			if (Row == null)
			{
				// Deleted row
				Row = Table.NewRow ();
				LoadColumns (Table, Row, reader, DataRowVersion.Current);
				Table.Rows.InsertAt (Row, int.Parse (rowOrder));
				Row.AcceptChanges ();
				Row.Delete ();
			}
			else
			{
				LoadColumns (Table, Row, reader, DataRowVersion.Original);
			}
		}

		private void LoadCurrentTable (DataTable Table, XmlReader reader) 
		{
			DataRowState state;
			DataRow Row = Table.NewRow ();

			string id = reader.GetAttribute	 ("id", XmlConstants.DiffgrNamespace);
			string error = reader.GetAttribute ("hasErrors");
			string changes = reader.GetAttribute ("hasChanges", XmlConstants.DiffgrNamespace);
			
			if (changes != null)
			{
				if (string.Compare (changes, "modified", true) == 0) {
					DiffGrRows.Add (id, Row); // for later use
					state = DataRowState.Modified;
				}
				else if (string.Compare (changes, "inserted", true) == 0) {
					state = DataRowState.Added;
				}
				else
					throw new InvalidOperationException ("Invalid row change state");
			}
			else
				state = DataRowState.Unchanged;
			
			// If row had errors add row to hashtable for later use
			if (error != null && string.Compare (error, "true", true) == 0)
				ErrorRows.Add (id, Row);
		
			LoadColumns (Table, Row, reader, DataRowVersion.Current);
			Table.Rows.Add (Row);
			
			if (state != DataRowState.Added)
				Row.AcceptChanges ();
		}

		DataTable GetTable (string name)
		{
			if (DSet != null) 
				return DSet.Tables [name];
			else if (name == table.TableName) 
				return table;
			else
				return null;
		}


		#endregion // Private methods
	}
}
