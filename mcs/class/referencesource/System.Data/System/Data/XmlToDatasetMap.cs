//------------------------------------------------------------------------------
// <copyright file="XmlToDatasetMap.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Xml;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;

    // This is an internal helper class used during Xml load to DataSet/DataDocument.
    // XmlToDatasetMap class provides functionality for binding elemants/atributes
    // to DataTable / DataColumn 
    internal sealed class XmlToDatasetMap {

        private sealed class XmlNodeIdentety {
            public string LocalName;
            public string NamespaceURI;
            public XmlNodeIdentety(string localName, string namespaceURI) {
                this.LocalName    = localName;
                this.NamespaceURI = namespaceURI;
            }
            override public int GetHashCode() {
                return ((object) LocalName).GetHashCode();
            }
            override public bool Equals(object obj) {
                XmlNodeIdentety id = (XmlNodeIdentety) obj;
                return (
                  (String.Compare(this.LocalName, id.LocalName, StringComparison.OrdinalIgnoreCase) == 0) &&
                  (String.Compare(this.NamespaceURI, id.NamespaceURI, StringComparison.OrdinalIgnoreCase) == 0)              
                );
            }
        }

        // This class exist to avoid alocatin of XmlNodeIdentety to every acces to the hash table.
        // Unfortunetely XmlNode doesn't export single identety object.
        internal sealed class XmlNodeIdHashtable : Hashtable {
            private XmlNodeIdentety id = new XmlNodeIdentety(string.Empty, string.Empty);
            public XmlNodeIdHashtable(Int32 capacity) 
                : base(capacity) {}
            public object this[XmlNode node] {
                get {
                    id.LocalName    = node.LocalName;
                    id.NamespaceURI = node.NamespaceURI;
                    return this[id];
                }
            }
            
            public object this[XmlReader dataReader] {
                get {
                    id.LocalName    = dataReader.LocalName;
                    id.NamespaceURI = dataReader.NamespaceURI;
                    return this[id];
                }
            }

            public object this[DataTable table] {
                get {
                    id.LocalName    = table.EncodedTableName;
                    id.NamespaceURI = table.Namespace;
                    return this[id];
                }
            }

            public object this[string name] {
                get {
                    id.LocalName    = name;
                    id.NamespaceURI = String.Empty;
                    return this[id];
                }
            }
        }

        private sealed class TableSchemaInfo {
            public DataTable          TableSchema;
            public XmlNodeIdHashtable ColumnsSchemaMap;
            public TableSchemaInfo(DataTable tableSchema) {
                this.TableSchema      = tableSchema;
                this.ColumnsSchemaMap = new XmlNodeIdHashtable(tableSchema.Columns.Count);
            }
        }

        XmlNodeIdHashtable tableSchemaMap;              // Holds all the tables information

        TableSchemaInfo lastTableSchemaInfo = null;

        // Used to infer schema

        public XmlToDatasetMap(DataSet dataSet, XmlNameTable nameTable) {
            Debug.Assert(dataSet   != null, "DataSet can't be null");
            Debug.Assert(nameTable != null, "NameTable can't be null");
            BuildIdentityMap(dataSet, nameTable);
        }

        // Used to read data with known schema

        public XmlToDatasetMap(XmlNameTable nameTable, DataSet dataSet) {
            Debug.Assert(dataSet   != null, "DataSet can't be null");
            Debug.Assert(nameTable != null, "NameTable can't be null");
            BuildIdentityMap(nameTable, dataSet);
        }

        // Used to infer schema

        public XmlToDatasetMap(DataTable dataTable, XmlNameTable nameTable) {
            Debug.Assert(dataTable != null, "DataTable can't be null");
            Debug.Assert(nameTable != null, "NameTable can't be null");
            BuildIdentityMap(dataTable, nameTable);
        }

        // Used to read data with known schema

        public XmlToDatasetMap(XmlNameTable nameTable, DataTable dataTable) {
            Debug.Assert(dataTable  != null, "DataTable can't be null");
            Debug.Assert(nameTable != null, "NameTable can't be null");
            BuildIdentityMap(nameTable, dataTable);
        }
        static internal bool IsMappedColumn(DataColumn c) {
            return (c.ColumnMapping != MappingType.Hidden);
        }

        // Used to infere schema

        private TableSchemaInfo AddTableSchema(DataTable table, XmlNameTable nameTable) {
            // [....]: Because in our case reader already read the document all names that we can meet in the
            //       document already has an entry in NameTable.
            //       If in future we will build identity map before reading XML we can replace Get() to Add()
            // [....]: GetIdentity is called from two places: BuildIdentityMap() and LoadRows()
            //       First case deals with decoded names; Second one with encoded names.
            //       We decided encoded names in first case (instead of decoding them in second) 
            //       because it save us time in LoadRows(). We have, as usual, more data them schemas
            string tableLocalName = nameTable.Get(table.EncodedTableName);
            string tableNamespace = nameTable.Get(table.Namespace );
            if(tableLocalName == null) {
                // because name of this table isn't present in XML we don't need mapping for it.
                // Less mapping faster we work.
                return null;
            }
            TableSchemaInfo tableSchemaInfo = new TableSchemaInfo(table);
            tableSchemaMap[new XmlNodeIdentety(tableLocalName, tableNamespace)] = tableSchemaInfo;
            return tableSchemaInfo;
        }

        private TableSchemaInfo AddTableSchema(XmlNameTable nameTable, DataTable table) {
            // [....]:This is the opposite of the previous function:
            //       we populate the nametable so that the hash comparison can happen as
            //       object comparison instead of strings.
            // [....]: GetIdentity is called from two places: BuildIdentityMap() and LoadRows()
            //       First case deals with decoded names; Second one with encoded names.
            //       We decided encoded names in first case (instead of decoding them in second) 
            //       because it save us time in LoadRows(). We have, as usual, more data them schemas

            string _tableLocalName = table.EncodedTableName;            // Table name

            string tableLocalName = nameTable.Get(_tableLocalName);     // Look it up in nametable

            if(tableLocalName == null) {                                // If not found
                tableLocalName = nameTable.Add(_tableLocalName);        // Add it
            }

            table.encodedTableName = tableLocalName;                    // And set it back

            string tableNamespace = nameTable.Get(table.Namespace);     // Look ip table namespace

            if (tableNamespace == null) {                               // If not found
                tableNamespace = nameTable.Add(table.Namespace);        // Add it
            }
            else {
                if (table.tableNamespace != null)                       // Update table namespace
                    table.tableNamespace = tableNamespace;
            }


            TableSchemaInfo tableSchemaInfo = new TableSchemaInfo(table);
                                                                        // Create new table schema info
            tableSchemaMap[new XmlNodeIdentety(tableLocalName, tableNamespace)] = tableSchemaInfo;
                                                                        // And add it to the hashtable
            return tableSchemaInfo;                                     // Return it as we have to populate
                                                                        // Column schema map and Child table 
                                                                        // schema map in it
        }

        private bool AddColumnSchema(DataColumn col, XmlNameTable nameTable, XmlNodeIdHashtable columns) {
            string columnLocalName = nameTable.Get(col.EncodedColumnName );
            string columnNamespace = nameTable.Get(col.Namespace   );
            if(columnLocalName == null) {
                return false;
            }
            XmlNodeIdentety idColumn = new XmlNodeIdentety(columnLocalName, columnNamespace);

            columns[idColumn] = col;
            
            if (col.ColumnName.StartsWith("xml", StringComparison.OrdinalIgnoreCase)) {
                HandleSpecialColumn(col, nameTable, columns);
            }

            
            return true;
        }

        private bool AddColumnSchema(XmlNameTable nameTable, DataColumn col, XmlNodeIdHashtable columns) {
            string _columnLocalName = XmlConvert.EncodeLocalName(col.ColumnName);
            string columnLocalName = nameTable.Get(_columnLocalName);           // Look it up in a name table

            if(columnLocalName == null) {                                       // Not found?
                columnLocalName = nameTable.Add(_columnLocalName);              // Add it
            }

            col.encodedColumnName = columnLocalName;                            // And set it back

            string columnNamespace = nameTable.Get(col.Namespace );             // Get column namespace from nametable

            if(columnNamespace == null) {                                       // Not found ?
                columnNamespace = nameTable.Add(col.Namespace);                 // Add it
            }
            else {
                if (col._columnUri != null )                                    // Update namespace
                    col._columnUri = columnNamespace;
            }
                                                                                // Create XmlNodeIdentety 
                                                                                // for this column
            XmlNodeIdentety idColumn = new XmlNodeIdentety(columnLocalName, columnNamespace);
            columns[idColumn] = col;                                            // And add it to hashtable

            if (col.ColumnName.StartsWith("xml", StringComparison.OrdinalIgnoreCase)) {
                HandleSpecialColumn(col, nameTable, columns);
            }
            
            return true;
        }

        private void BuildIdentityMap(DataSet dataSet, XmlNameTable nameTable) {

            this.tableSchemaMap    = new XmlNodeIdHashtable(dataSet.Tables.Count);

            foreach(DataTable t in dataSet.Tables) {
                TableSchemaInfo tableSchemaInfo = AddTableSchema(t, nameTable);
                if(tableSchemaInfo != null) {
                    foreach( DataColumn c in t.Columns ) {
                        // don't include auto-generated PK, FK and any hidden columns to be part of mapping
                        if (IsMappedColumn(c)) {
                            AddColumnSchema(c, nameTable, tableSchemaInfo.ColumnsSchemaMap);
                        }
                    }
                }
            }
        }

        // This one is used while reading data with preloaded schema

        private void BuildIdentityMap(XmlNameTable nameTable, DataSet dataSet) {
            this.tableSchemaMap    = new XmlNodeIdHashtable(dataSet.Tables.Count);
                                                                        // This hash table contains
                                                                        // tables schemas as TableSchemaInfo objects
                                                                        // These objects holds reference to the table.
                                                                        // Hash tables with columns schema maps
                                                                        // and child tables schema maps

            string dsNamespace = nameTable.Get(dataSet.Namespace);      // Attept to look up DataSet namespace
                                                                        // in the name table

            if (dsNamespace == null) {                                  // Found ?
                dsNamespace = nameTable.Add(dataSet.Namespace);         // Nope. Add it
            }
            dataSet.namespaceURI = dsNamespace;                         // Set a DataSet namespace URI


            foreach(DataTable t in dataSet.Tables) {                    // For each table

                TableSchemaInfo tableSchemaInfo = AddTableSchema(nameTable, t);
                                                                        // Add table schema info to hash table

                if(tableSchemaInfo != null) {                          
                    foreach( DataColumn c in t.Columns ) {              // Add column schema map
                        // don't include auto-generated PK, FK and any hidden columns to be part of mapping
                        if (IsMappedColumn(c)) {                        // If mapped column
                            AddColumnSchema(nameTable, c, tableSchemaInfo.ColumnsSchemaMap);
                        }                                               // Add it to the map
                    }

                    // Add child nested tables to the schema

                    foreach( DataRelation r in t.ChildRelations ) {     // Do we have a child tables ?
                        if (r.Nested) {                                 // Is it nested?
                            // don't include non nested tables

                            // Handle namespaces and names as usuall

                            string _tableLocalName = XmlConvert.EncodeLocalName(r.ChildTable.TableName);
                            string tableLocalName = nameTable.Get(_tableLocalName);

                            if(tableLocalName == null) {
                                tableLocalName = nameTable.Add(_tableLocalName);
                            }

                            string tableNamespace = nameTable.Get(r.ChildTable.Namespace );

                            if(tableNamespace == null) {
                                tableNamespace = nameTable.Add(r.ChildTable.Namespace);
                            }

                            XmlNodeIdentety idTable = new XmlNodeIdentety(tableLocalName, tableNamespace);
                            tableSchemaInfo.ColumnsSchemaMap[idTable] = r.ChildTable;

                        }
                    }
                }
            }
        }

        // Used for inference

        private void BuildIdentityMap(DataTable dataTable, XmlNameTable nameTable) {
            this.tableSchemaMap    = new XmlNodeIdHashtable(1);

            TableSchemaInfo tableSchemaInfo = AddTableSchema(dataTable, nameTable);
            if(tableSchemaInfo != null) {
                foreach( DataColumn c in dataTable.Columns ) {
                      // don't include auto-generated PK, FK and any hidden columns to be part of mapping
                      if (IsMappedColumn(c)) {
                          AddColumnSchema(c, nameTable, tableSchemaInfo.ColumnsSchemaMap);
                      }
                  }
            }
            
        }

        // This one is used while reading data with preloaded schema

        private void BuildIdentityMap(XmlNameTable nameTable, DataTable dataTable) {

            ArrayList tableList = GetSelfAndDescendants(dataTable);     // Get list of tables we're loading
                                                                        // This includes our table and 
                                                                        // related tables tree

            this.tableSchemaMap = new XmlNodeIdHashtable( tableList.Count );
                                                                        // Create hash table to hold all
                                                                        // tables to load.
            
            foreach (DataTable t in tableList) {                        // For each table

                TableSchemaInfo tableSchemaInfo = AddTableSchema(nameTable, t );
                                                                        // Create schema info
                if(tableSchemaInfo != null) {
                    foreach( DataColumn c in t.Columns ) {              // Add column information
                        // don't include auto-generated PK, FK and any hidden columns to be part of mapping
                        if (IsMappedColumn(c)) {
                            AddColumnSchema(nameTable, c, tableSchemaInfo.ColumnsSchemaMap);
                        }
                    }

                    foreach( DataRelation r in t.ChildRelations ) {     // Add nested tables information
                        if (r.Nested) {                                 // Is it nested?
                            // don't include non nested tables

                            // Handle namespaces and names as usuall

                            string _tableLocalName = XmlConvert.EncodeLocalName(r.ChildTable.TableName);
                            string tableLocalName = nameTable.Get(_tableLocalName);

                            if(tableLocalName == null) {
                                tableLocalName = nameTable.Add(_tableLocalName);
                            }

                            string tableNamespace = nameTable.Get(r.ChildTable.Namespace );

                            if(tableNamespace == null) {
                                tableNamespace = nameTable.Add(r.ChildTable.Namespace);
                            }

                            XmlNodeIdentety idTable = new XmlNodeIdentety(tableLocalName, tableNamespace);
                            tableSchemaInfo.ColumnsSchemaMap[idTable] = r.ChildTable;

                        }
                    }
                }
            }
        }

        private ArrayList GetSelfAndDescendants(DataTable dt) { // breadth-first
             ArrayList tableList = new ArrayList();
             tableList.Add(dt);
             int nCounter = 0;
             
             while (nCounter < tableList.Count) {
                foreach(DataRelation childRelations in ((DataTable)tableList[nCounter]).ChildRelations) {
                    if (!tableList.Contains(childRelations.ChildTable))
                        tableList.Add(childRelations.ChildTable);
                }
                nCounter++;
             }
         
            return tableList;
        }
        // Used to infer schema and top most node

        public object GetColumnSchema(XmlNode node, bool fIgnoreNamespace) {
            Debug.Assert(node != null, "Argument validation");
            TableSchemaInfo tableSchemaInfo = null;

            XmlNode nodeRegion = (node.NodeType == XmlNodeType.Attribute) ? ((XmlAttribute)node).OwnerElement : node.ParentNode;

            do {
                if(nodeRegion == null || nodeRegion.NodeType != XmlNodeType.Element) {
                    return null;
                }
                tableSchemaInfo = (TableSchemaInfo) (fIgnoreNamespace ? tableSchemaMap[nodeRegion.LocalName] : tableSchemaMap[nodeRegion]);

                nodeRegion = nodeRegion.ParentNode;
            } while(tableSchemaInfo == null);

            if (fIgnoreNamespace)
                return tableSchemaInfo.ColumnsSchemaMap[node.LocalName];
            else
                return tableSchemaInfo.ColumnsSchemaMap[node];

        }


        public object GetColumnSchema(DataTable table, XmlReader dataReader, bool fIgnoreNamespace){
            if ((lastTableSchemaInfo == null) || (lastTableSchemaInfo.TableSchema != table)) {
                lastTableSchemaInfo = (TableSchemaInfo)(fIgnoreNamespace ? tableSchemaMap[table.EncodedTableName] : tableSchemaMap[table]);
            }

            if (fIgnoreNamespace)
                return lastTableSchemaInfo.ColumnsSchemaMap[dataReader.LocalName];
            return lastTableSchemaInfo.ColumnsSchemaMap[dataReader];
        }

        // Used to infer schema

        public object GetSchemaForNode(XmlNode node, bool fIgnoreNamespace) {
            TableSchemaInfo tableSchemaInfo = null;

            if (node.NodeType == XmlNodeType.Element) {         // If element
                tableSchemaInfo = (TableSchemaInfo) (fIgnoreNamespace ? tableSchemaMap[node.LocalName] : tableSchemaMap[node]);
            }                                                   // Look up table schema info for it

            if (tableSchemaInfo != null) {                      // Got info ?
                return tableSchemaInfo.TableSchema;             // Yes, Return table
            }

            return GetColumnSchema(node, fIgnoreNamespace);     // Attempt to locate column
        }        

        public DataTable GetTableForNode(XmlReader node, bool fIgnoreNamespace) {
            TableSchemaInfo tableSchemaInfo = (TableSchemaInfo) (fIgnoreNamespace ? tableSchemaMap[node.LocalName] : tableSchemaMap[node]);
            if (tableSchemaInfo != null) {
                lastTableSchemaInfo = tableSchemaInfo;
                return lastTableSchemaInfo.TableSchema;
            }
            return null;
        }

        private void HandleSpecialColumn(DataColumn col, XmlNameTable nameTable, XmlNodeIdHashtable columns) {
            // if column name starts with xml, we encode it manualy and add it for look up
            Debug.Assert(col.ColumnName.StartsWith("xml", StringComparison.OrdinalIgnoreCase), "column name should start with xml");
            string tempColumnName;

            if ('x' == col.ColumnName[0]) {
                tempColumnName = "_x0078_"; // lower case xml... -> _x0078_ml...
            }
            else {
                tempColumnName = "_x0058_"; // upper case Xml... -> _x0058_ml...
            }
            
            tempColumnName += col.ColumnName.Substring(1);
            
            if(nameTable.Get(tempColumnName) == null) {
                nameTable.Add(tempColumnName);
            }
            string columnNamespace = nameTable.Get(col.Namespace);
            XmlNodeIdentety idColumn = new XmlNodeIdentety(tempColumnName, columnNamespace);
            columns[idColumn] = col;
        }

    }
}
