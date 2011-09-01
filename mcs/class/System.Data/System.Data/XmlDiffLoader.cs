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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Data;
using System.Xml;
using System.Xml.Schema;
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
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return;
			}
			
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
			if (reader.IsEmptyElement) {
				reader.Skip();
				return;
			}
			reader.ReadStartElement ();		// Dataset root
			reader.MoveToContent ();

			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					DataTable t = GetTable (reader.LocalName);
					if (t != null)
						LoadCurrentTable (t, reader);
#if true
					else
						reader.Skip ();
#else
					else 
						throw new DataException (Locale.GetText ("Cannot load diffGram. Table '" + reader.LocalName + "' is missing in the destination dataset"));	
#endif
				}
				else
					reader.Skip ();
			}
			
			reader.ReadEndElement ();
		}

		private void LoadBefore (XmlReader reader) 
		{
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return;
			}
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
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return;
			}
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

		private void LoadColumns (DataTable Table, DataRow Row, 
			XmlReader reader, DataRowVersion loadType)
		{
			// attributes
			LoadColumnAttributes (Table, Row, reader, loadType);
			LoadColumnChildren (Table, Row, reader, loadType);
		}

		private void LoadColumnAttributes (DataTable Table, DataRow Row,
			XmlReader reader, DataRowVersion loadType)
		{
			if (!reader.HasAttributes // this check will be faster
				|| !reader.MoveToFirstAttribute ())
				return;
			do {
				switch (reader.NamespaceURI) {
				case XmlConstants.XmlnsNS:
#if NET_2_0
				case XmlConstants.XmlNS:
#endif
				case XmlConstants.DiffgrNamespace:
				case XmlConstants.MsdataNamespace:
				case XmlConstants.MspropNamespace:
				case XmlSchema.Namespace:
					continue;
				}
				DataColumn c = Table.Columns [XmlHelper.Decode (reader.LocalName)];
				if (c == null ||
					c.ColumnMapping != MappingType.Attribute)					continue;
				if (c.Namespace == null && reader.NamespaceURI == String.Empty ||
					c.Namespace == reader.NamespaceURI) {
					object data = XmlDataLoader.StringToObject (c.DataType, reader.Value);
					if (loadType == DataRowVersion.Current)
						Row [c] = data;
					else
						Row.SetOriginalValue (c.ColumnName, data);
				} // otherwise just ignore as well as unknown elements.
			} while (reader.MoveToNextAttribute ());
			reader.MoveToElement ();
		}

		private void LoadColumnChildren (DataTable Table, DataRow Row,
			XmlReader reader, DataRowVersion loadType) 
		{
			// children
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return;
			}
			reader.ReadStartElement ();
			reader.MoveToContent ();
			
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType != XmlNodeType.Element) { reader.Read (); continue; }
				
				string colName = XmlHelper.Decode (reader.LocalName);
				if (Table.Columns.Contains (colName)) 
				{
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
					} else
						reader.Skip ();
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
				if (string.Compare (changes, "modified", true, CultureInfo.InvariantCulture) == 0) {
					DiffGrRows.Add (id, Row); // for later use
					state = DataRowState.Modified;
				}
				else if (string.Compare (changes, "inserted", true, CultureInfo.InvariantCulture) == 0) {
					state = DataRowState.Added;
				}
				else
					throw new InvalidOperationException ("Invalid row change state");
			}
			else
				state = DataRowState.Unchanged;
			
			// If row had errors add row to hashtable for later use
			if (error != null && string.Compare (error, "true", true, CultureInfo.InvariantCulture) == 0)
				ErrorRows.Add (id, Row);
		
			LoadColumns (Table, Row, reader, DataRowVersion.Current);
			Table.Rows.Add (Row);
			
			if (state != DataRowState.Added)
				Row.AcceptChanges ();
		}

		DataTable GetTable (string name)
		{
			name = XmlConvert.DecodeName (name);
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
