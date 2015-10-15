//------------------------------------------------------------------------------
// <copyright file="DataTableReaderListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Data.Common;
    using System.Collections;
    using System.ComponentModel;

    internal sealed class DataTableReaderListener {

        private DataTable currentDataTable = null;
        private bool  isSubscribed = false;
        private WeakReference readerWeak;

        internal DataTableReaderListener(DataTableReader reader) {
            if (reader == null)
                throw ExceptionBuilder.ArgumentNull("DataTableReader");
            if (currentDataTable != null) {
                UnSubscribeEvents();
            }
            this.readerWeak = new WeakReference(reader);
            currentDataTable = reader.CurrentDataTable;
            if (currentDataTable != null)
                SubscribeEvents();
        }
       
        internal void CleanUp() {
            UnSubscribeEvents();
        }

        internal void UpdataTable(DataTable datatable) {
            if (datatable == null)
                throw ExceptionBuilder.ArgumentNull("DataTable");

            UnSubscribeEvents();
            currentDataTable = datatable;
            SubscribeEvents();
        }
        
        private void SubscribeEvents() {
            if (currentDataTable == null)
                return;
            if (isSubscribed)
                return;
            currentDataTable.Columns.ColumnPropertyChanged    += new CollectionChangeEventHandler(SchemaChanged);
            currentDataTable.Columns.CollectionChanged         += new CollectionChangeEventHandler(SchemaChanged);
           
            currentDataTable.RowChanged    += new DataRowChangeEventHandler(DataChanged );
            currentDataTable.RowDeleted    += new DataRowChangeEventHandler(DataChanged);

            currentDataTable.TableCleared    += new DataTableClearEventHandler(DataTableCleared);
            isSubscribed = true;
        }

        private void UnSubscribeEvents() {
            if (currentDataTable == null)
                return;
            if (!isSubscribed)
                return;
            
            currentDataTable.Columns.ColumnPropertyChanged    -= new CollectionChangeEventHandler(SchemaChanged);
            currentDataTable.Columns.CollectionChanged         -= new CollectionChangeEventHandler(SchemaChanged);

            currentDataTable.RowChanged    -= new DataRowChangeEventHandler(DataChanged );
            currentDataTable.RowDeleted    -= new DataRowChangeEventHandler(DataChanged);

            currentDataTable.TableCleared    -= new DataTableClearEventHandler(DataTableCleared);
            isSubscribed = false;
        }
 

       private void DataTableCleared(object sender, DataTableClearEventArgs e) {
           DataTableReader reader = (DataTableReader) readerWeak.Target;
           if (reader != null) {
               reader.DataTableCleared();
           }
           else {
               UnSubscribeEvents();
           }
            
       }

       private void SchemaChanged(object sender, CollectionChangeEventArgs e) {
           DataTableReader reader = (DataTableReader) readerWeak.Target;
           if (reader != null) {
               reader.SchemaChanged();
           }
           else {
               UnSubscribeEvents();
           }
       }

       private void DataChanged( object sender, DataRowChangeEventArgs args ) {          
           DataTableReader reader = (DataTableReader) readerWeak.Target;
           if (reader != null) {
               reader.DataChanged(args);
           }
           else {
               UnSubscribeEvents();
           }
       }
    }
}
