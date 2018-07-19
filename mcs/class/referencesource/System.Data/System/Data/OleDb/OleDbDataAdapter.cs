//------------------------------------------------------------------------------
// <copyright file="OleDbDataAdapter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Data.OleDb {

    [
    DefaultEvent("RowUpdated"),
    ToolboxItem("Microsoft.VSDesigner.Data.VS.OleDbDataAdapterToolboxItem, " + AssemblyRef.MicrosoftVSDesigner),
    Designer("Microsoft.VSDesigner.Data.VS.OleDbDataAdapterDesigner, " + AssemblyRef.MicrosoftVSDesigner),
    ]
    public sealed class OleDbDataAdapter : DbDataAdapter, IDbDataAdapter, ICloneable {

        static private readonly object EventRowUpdated = new object();
        static private readonly object EventRowUpdating = new object();

        private OleDbCommand _deleteCommand, _insertCommand, _selectCommand, _updateCommand;

        public OleDbDataAdapter() : base() {
            GC.SuppressFinalize(this);
        }

        public OleDbDataAdapter(OleDbCommand selectCommand) : this() {
            SelectCommand = selectCommand;
        }

        public OleDbDataAdapter(string selectCommandText, string selectConnectionString) : this() {
            OleDbConnection connection = new OleDbConnection(selectConnectionString);
            SelectCommand = new OleDbCommand(selectCommandText, connection);
        }

        public OleDbDataAdapter(string selectCommandText, OleDbConnection selectConnection) : this() {
            SelectCommand = new OleDbCommand(selectCommandText, selectConnection);
        }

        private OleDbDataAdapter(OleDbDataAdapter from) : base(from) {
            GC.SuppressFinalize(this);
        }

        [
        DefaultValue(null),
        Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_DeleteCommand),
        ]
        new public OleDbCommand DeleteCommand {
            get { return _deleteCommand; }
            set { _deleteCommand = value; }
        }

        IDbCommand IDbDataAdapter.DeleteCommand {
            get { return _deleteCommand; }
            set { _deleteCommand = (OleDbCommand)value; }
        }
 
        [
        DefaultValue(null),
        Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_InsertCommand),
        ]
        new public OleDbCommand InsertCommand {
            get { return _insertCommand; }
            set { _insertCommand = value; }
        }

        IDbCommand IDbDataAdapter.InsertCommand {
            get { return _insertCommand; }
            set { _insertCommand = (OleDbCommand)value; }
        }

        [
        DefaultValue(null),
        Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResCategoryAttribute(Res.DataCategory_Fill),
        ResDescriptionAttribute(Res.DbDataAdapter_SelectCommand),
        ]
        new public OleDbCommand SelectCommand {
            get { return _selectCommand; }
            set { _selectCommand = value; }
        }

        IDbCommand IDbDataAdapter.SelectCommand {
            get { return _selectCommand; }
            set { _selectCommand = (OleDbCommand)value; }
        }

        [
        DefaultValue(null),
        Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_UpdateCommand),
        ]
        new public OleDbCommand UpdateCommand {
            get { return _updateCommand; }
            set { _updateCommand = value; }
        }
        
        IDbCommand IDbDataAdapter.UpdateCommand {
            get { return _updateCommand; }
            set { _updateCommand = (OleDbCommand)value; }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_RowUpdated),
        ]
        public event OleDbRowUpdatedEventHandler RowUpdated {
            add { Events.AddHandler(EventRowUpdated, value); }
            remove { Events.RemoveHandler(EventRowUpdated, value); }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_RowUpdating),
        ]
        public event OleDbRowUpdatingEventHandler RowUpdating {
            add {
                OleDbRowUpdatingEventHandler handler = (OleDbRowUpdatingEventHandler) Events[EventRowUpdating];

                // MDAC 58177, 64513
                // prevent someone from registering two different command builders on the adapter by
                // silently removing the old one
                if ((null != handler) && (value.Target is DbCommandBuilder)) {
                    OleDbRowUpdatingEventHandler d = (OleDbRowUpdatingEventHandler) ADP.FindBuilder(handler);
                    if (null != d) {
                        Events.RemoveHandler(EventRowUpdating, d);
                    }
                }
                Events.AddHandler(EventRowUpdating, value);
            }
            remove { Events.RemoveHandler(EventRowUpdating, value); }
        }

        object ICloneable.Clone() {
            return new OleDbDataAdapter(this);
        }

        override protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) {
            return new OleDbRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        override protected RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) {
            return new OleDbRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        internal static void FillDataTable(OleDbDataReader dataReader, params DataTable[] dataTables) {
            OleDbDataAdapter adapter = new OleDbDataAdapter();
            adapter.Fill(dataTables, dataReader, 0, 0);
        }

        public int Fill(DataTable dataTable, object ADODBRecordSet) {
            System.Security.PermissionSet permissionSet = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
            permissionSet.AddPermission(OleDbConnection.ExecutePermission); // MDAC 77737
            permissionSet.AddPermission(new System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode));
            permissionSet.Demand();

            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<oledb.OleDbDataAdapter.Fill|API> %d#, dataTable, ADODBRecordSet\n", ObjectID);
            try {
                if (null == dataTable) {
                    throw ADP.ArgumentNull("dataTable");
                }
                if (null == ADODBRecordSet) {
                    throw ADP.ArgumentNull("adodb");
                }
                // user was required to have UnmanagedCode Permission just to create ADODB
                // (new SecurityPermission(SecurityPermissionFlag.UnmanagedCode)).Demand();
                return FillFromADODB((object)dataTable, ADODBRecordSet, null, false); // MDAC 59249
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public int Fill(DataSet dataSet, object ADODBRecordSet, string srcTable) {
            System.Security.PermissionSet permissionSet = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
            permissionSet.AddPermission(OleDbConnection.ExecutePermission); // MDAC 77737
            permissionSet.AddPermission(new System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode));
            permissionSet.Demand();

            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<oledb.OleDbDataAdapter.Fill|API> %d#, dataSet, ADODBRecordSet, srcTable='%ls'\n", ObjectID, srcTable);
            try {
                if (null == dataSet) {
                    throw ADP.ArgumentNull("dataSet");
                }
                if (null == ADODBRecordSet) {
                    throw ADP.ArgumentNull("adodb");
                }
                if (ADP.IsEmpty(srcTable)) {
                    throw ADP.FillRequiresSourceTableName("srcTable");
                }
                // user was required to have UnmanagedCode Permission just to create ADODB
                // (new SecurityPermission(SecurityPermissionFlag.UnmanagedCode)).Demand();
                return FillFromADODB((object)dataSet, ADODBRecordSet, srcTable, true);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private int FillFromADODB(Object data, object adodb, string srcTable, bool multipleResults) {
            Debug.Assert(null != data, "FillFromADODB: null data object");
            Debug.Assert(null != adodb, "FillFromADODB: null ADODB");
            Debug.Assert(!(adodb is DataTable), "call Fill( (DataTable) value)");
            Debug.Assert(!(adodb is DataSet), "call Fill( (DataSet) value)");

            /*
            IntPtr adodbptr = ADP.PtrZero;
            try { // generate a new COM Callable Wrapper around the user object so they can't ReleaseComObject on us.
                adodbptr = Marshal.GetIUnknownForObject(adodb);
                adodb = System.Runtime.Remoting.Services.EnterpriseServicesHelper.WrapIUnknownWithComObject(adodbptr);
            }
            finally {
                if (ADP.PtrZero != adodbptr) {
                    Marshal.Release(adodbptr);
                }
            }
            */

            bool closeRecordset = multipleResults; // MDAC 60332, 66668
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|ADODB> ADORecordsetConstruction\n");
            UnsafeNativeMethods.ADORecordsetConstruction recordset = (adodb as UnsafeNativeMethods.ADORecordsetConstruction);
            UnsafeNativeMethods.ADORecordConstruction record = null;

            if (null != recordset) { // MDAC 78415
                if (multipleResults) {
                    // The NextRecordset method is not available on a disconnected Recordset object, where ActiveConnection has been set to NULL
                    object activeConnection;

                    Bid.Trace("<oledb.Recordset15.get_ActiveConnection|API|ADODB>\n");
                    activeConnection = ((UnsafeNativeMethods.Recordset15)adodb).get_ActiveConnection();
                    
                    if (null == activeConnection) {
                        multipleResults = false;
                    }
                }
            }
            else {
                Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|ADODB> ADORecordConstruction\n");
                record = (adodb as UnsafeNativeMethods.ADORecordConstruction);
                
                if (null != record) { // MDAC 78415
                    multipleResults = false; // IRow implies CommandBehavior.SingleRow which implies CommandBehavior.SingleResult
                }
            }
            // else throw ODB.Fill_NotADODB("adodb"); /* throw later, less code here*/

            int results = 0;
            if (null != recordset) {
                int resultCount = 0;
                bool incrementResultCount; // MDAC 59632
                object[] value = new object[1];

                do {
                    string tmp = null;
                    if (data is DataSet) {
                        tmp = GetSourceTableName(srcTable, resultCount);
                    }
                    results += FillFromRecordset(data, recordset, tmp, out incrementResultCount);

                    if (multipleResults) {
                        value[0] = DBNull.Value;

                        object recordsAffected;
                        object nextresult;

                        Bid.Trace("<oledb.Recordset15.NextRecordset|API|ADODB>\n");
                        OleDbHResult hr = ((UnsafeNativeMethods.Recordset15)adodb).NextRecordset(out recordsAffected, out nextresult); // MDAC 78415
                        Bid.Trace("<oledb.Recordset15.NextRecordset|API|ADODB|RET> %08X{HRESULT}\n", hr);

                        if (0 > hr) {
                            // Current provider does not support returning multiple recordsets from a single execution.
                            if (ODB.ADODB_NextResultError != (int)hr) {
                                UnsafeNativeMethods.IErrorInfo errorInfo = null;
                                UnsafeNativeMethods.GetErrorInfo(0, out errorInfo);

                                string message = String.Empty;
                                if (null != errorInfo) {                                    
                                    OleDbHResult hresult = ODB.GetErrorDescription(errorInfo, hr, out message);
                                }
                                throw new COMException(message, (int)hr);
                            }
                            break;
                        }
                        adodb = nextresult;
                        if (null != adodb) {
                            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|ADODB> ADORecordsetConstruction\n");
                            recordset = (UnsafeNativeMethods.ADORecordsetConstruction) adodb;

                            if (incrementResultCount) {
                                resultCount++;
                            }
                            continue;
                        }
                    }
                    break;
                } while(null != recordset);

                if ((null != recordset) && (closeRecordset || (null == adodb))) { // MDAC 59746, 60902
                    FillClose(true, recordset);
                }
            }
            else if (null != record) {
                results = FillFromRecord(data, record, srcTable);
                if (closeRecordset) { // MDAC 66668
                    FillClose(false, record); // MDAC 60848
                }
            }
            else {
                throw ODB.Fill_NotADODB("adodb");
            }
            return results;
        }

        //override protected int Fill(DataTable dataTable, IDataReader dataReader) { // MDAC 65506
        //    return base.Fill(dataTable, dataReader);
        //}

        private int FillFromRecordset(Object data, UnsafeNativeMethods.ADORecordsetConstruction recordset, string srcTable, out bool incrementResultCount) {
            incrementResultCount = false;

            IntPtr chapter; /*ODB.DB_NULL_HCHAPTER*/
            object result = null;
            try {
                Bid.Trace("<oledb.ADORecordsetConstruction.get_Rowset|API|ADODB>\n");
                result = recordset.get_Rowset(); // MDAC 83342
                Bid.Trace("<oledb.ADORecordsetConstruction.get_Rowset|API|ADODB|RET> %08X{HRESULT}\n", 0);

                Bid.Trace("<oledb.ADORecordsetConstruction.get_Chapter|API|ADODB>\n");
                chapter = recordset.get_Chapter(); // MDAC 83342
                Bid.Trace("<oledb.ADORecordsetConstruction.get_Chapter|API|ADODB|RET> %08X{HRESULT}\n", 0);
            }
            catch (Exception e) {
                // 
                if (!ADP.IsCatchableExceptionType(e)) {
                    throw;
                }
            
                throw ODB.Fill_EmptyRecordSet("ADODBRecordSet", e);
            }

            if (null != result) {
                CommandBehavior behavior = (MissingSchemaAction.AddWithKey != MissingSchemaAction) ? 0 : CommandBehavior.KeyInfo;
                behavior |= CommandBehavior.SequentialAccess;

                OleDbDataReader dataReader = null;
                try {
                     // intialized with chapter only since we don't want ReleaseChapter called for this chapter handle
                    ChapterHandle chapterHandle = ChapterHandle.CreateChapterHandle(chapter);
                     
                    dataReader = new OleDbDataReader(null, null, 0, behavior);
                    dataReader.InitializeIRowset(result, chapterHandle, ADP.RecordsUnaffected);
                    dataReader.BuildMetaInfo();

                    incrementResultCount = (0 < dataReader.FieldCount); // MDAC 59632
                    if (incrementResultCount) {
                        if (data is DataTable) {
                            return base.Fill((DataTable) data, dataReader); // MDAC 65506
                        }
                        else {
                            return base.Fill((DataSet) data, srcTable, dataReader, 0, 0);
                        }
                    }
                }
                finally {
                    if (null != dataReader) {
                        dataReader.Close();
                    }
                }
            }
            return 0;
        }

        private int FillFromRecord(Object data, UnsafeNativeMethods.ADORecordConstruction record, string srcTable) {
            object result = null;
            try {
                Bid.Trace("<oledb.ADORecordConstruction.get_Row|API|ADODB>\n");
                result = record.get_Row(); // MDAC 83342
                Bid.Trace("<oledb.ADORecordConstruction.get_Row|API|ADODB|RET> %08X{HRESULT}\n", 0);
            }
            catch(Exception e) {
                // 
                if (!ADP.IsCatchableExceptionType(e)) {
                    throw;
                }                        
            
                throw ODB.Fill_EmptyRecord("adodb", e);
            }

            if (null != result) {
                CommandBehavior behavior = (MissingSchemaAction.AddWithKey != MissingSchemaAction) ? 0 : CommandBehavior.KeyInfo;
                behavior |= CommandBehavior.SequentialAccess | CommandBehavior.SingleRow;

                OleDbDataReader dataReader = null;
                try {
                    dataReader = new OleDbDataReader(null, null, 0, behavior);
                    dataReader.InitializeIRow(result, ADP.RecordsUnaffected);
                    dataReader.BuildMetaInfo();

                    if (data is DataTable) {
                        return base.Fill((DataTable) data, dataReader); // MDAC 65506
                    }
                    else {
                        return base.Fill((DataSet) data, srcTable, dataReader, 0, 0);
                    }
                }
                finally {
                    if (null != dataReader) {
                        dataReader.Close();
                    }
                }
            }
            return 0;
        }

        private void FillClose(bool isrecordset, object value) {
            OleDbHResult hr;
            if (isrecordset) {
                Bid.Trace("<oledb.Recordset15.Close|API|ADODB>\n");
                hr = ((UnsafeNativeMethods.Recordset15)value).Close(); // MDAC 78415
                Bid.Trace("<oledb.Recordset15.Close|API|ADODB|RET> %08X{HRESULT}\n", hr);
            }
            else {
                Bid.Trace("<oledb._ADORecord.Close|API|ADODB>\n");
                hr = ((UnsafeNativeMethods._ADORecord)value).Close(); // MDAC 78415
                Bid.Trace("<oledb._ADORecord.Close|API|ADODB|RET> %08X{HRESULT}\n", hr);
            }
            if ((0 < (int)hr) && (ODB.ADODB_AlreadyClosedError != (int)hr)) {
                UnsafeNativeMethods.IErrorInfo errorInfo = null;
                UnsafeNativeMethods.GetErrorInfo(0, out errorInfo);
                string message = String.Empty;
                if (null != errorInfo) {
                    OleDbHResult hresult = ODB.GetErrorDescription(errorInfo, hr, out message);
                }
                throw new COMException(message, (int)hr);
            }
        }

        override protected void OnRowUpdated(RowUpdatedEventArgs value) {
            OleDbRowUpdatedEventHandler handler = (OleDbRowUpdatedEventHandler) Events[EventRowUpdated];
            if ((null != handler) && (value is OleDbRowUpdatedEventArgs)) {
                handler(this, (OleDbRowUpdatedEventArgs) value);
            }
            base.OnRowUpdated(value);
        }

        override protected void OnRowUpdating(RowUpdatingEventArgs value) {
            OleDbRowUpdatingEventHandler handler = (OleDbRowUpdatingEventHandler) Events[EventRowUpdating];
            if ((null != handler) && (value is OleDbRowUpdatingEventArgs)) {
                handler(this, (OleDbRowUpdatingEventArgs) value);
            }
            base.OnRowUpdating(value);
        }

        static private string GetSourceTableName(string srcTable, int index) {
            //if ((null != srcTable) && (0 <= index) && (index < srcTable.Length)) {
            if (0 == index) {
                return srcTable; //[index];
            }
            return srcTable + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
