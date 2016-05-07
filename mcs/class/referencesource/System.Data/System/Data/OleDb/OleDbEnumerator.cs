//------------------------------------------------------------------------------
// <copyright file="OleDbEnumerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.OleDb {

    using System;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;

    public sealed class OleDbEnumerator  {

        public OleDbEnumerator() {
        }

        public DataTable GetElements() {
            OleDbConnection.ExecutePermission.Demand();
             
            DataTable dataTable = new DataTable("MSDAENUM"); // WebData 112482
            dataTable.Locale = CultureInfo.InvariantCulture;
            OleDbDataReader dataReader = GetRootEnumerator();
            OleDbDataAdapter.FillDataTable(dataReader, dataTable);
            return dataTable;
        }

        static public OleDbDataReader GetEnumerator(Type type) {
            OleDbConnection.ExecutePermission.Demand();

            return GetEnumeratorFromType(type);
        }
        
        static internal OleDbDataReader GetEnumeratorFromType(Type type) { // WebData 99005
            // will demand security appropriately
            object value = Activator.CreateInstance(type, System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance, null, null, CultureInfo.InvariantCulture, null);
            return GetEnumeratorReader(value);
        }
        
        static private OleDbDataReader GetEnumeratorReader(object value) {
            NativeMethods.ISourcesRowset srcrowset = null;

            try {
                srcrowset = (NativeMethods.ISourcesRowset) value;
            }
            catch(InvalidCastException) {
                throw ODB.ISourcesRowsetNotSupported();
            }
            if (null == srcrowset) {
                throw ODB.ISourcesRowsetNotSupported();
            }
            value = null; // still held by ISourcesRowset, reused for IRowset

            int propCount = 0;
            IntPtr propSets = ADP.PtrZero;

            Bid.Trace("<oledb.ISourcesRowset.GetSourcesRowset|API|OLEDB> IID_IRowset\n");
            OleDbHResult hr = srcrowset.GetSourcesRowset(ADP.PtrZero, ODB.IID_IRowset, propCount, propSets, out value);
            Bid.Trace("<oledb.ISourcesRowset.GetSourcesRowset|API|OLEDB|RET> %08X{HRESULT}\n", hr);

            Exception f = OleDbConnection.ProcessResults(hr, null, null);
            if (null != f) {
                throw f;
            }

            OleDbDataReader dataReader = new OleDbDataReader(null, null, 0, CommandBehavior.Default);
            dataReader.InitializeIRowset(value, ChapterHandle.DB_NULL_HCHAPTER, ADP.RecordsUnaffected);
            dataReader.BuildMetaInfo();
            dataReader.HasRowsRead();
            return dataReader;
        }
        
        static public OleDbDataReader GetRootEnumerator() {
            OleDbConnection.ExecutePermission.Demand();

            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<oledb.OleDbEnumerator.GetRootEnumerator|API>\n");
            try {
                //readonly Guid CLSID_MSDAENUM = new Guid(0xc8b522d0,0x5cf3,0x11ce,0xad,0xe5,0x00,0xaa,0x00,0x44,0x77,0x3d);
                //Type msdaenum = Type.GetTypeFromCLSID(CLSID_MSDAENUM, true);
                const string PROGID_MSDAENUM = "MSDAENUM";
                Type msdaenum = Type.GetTypeFromProgID(PROGID_MSDAENUM, true);
                return GetEnumeratorFromType(msdaenum);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }
    }
}

