//------------------------------------------------------------------------------
// <copyright file="FieldNameLookup.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------


namespace System.Data 
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    
    internal sealed class FieldNameLookup { // V1.2.3300, MDAC 69015, 71470

        // hashtable stores the index into the _fieldNames, match via case-sensitive
        private Hashtable _fieldNameLookup;

        // original names for linear searches when exact matches fail
        private string[] _fieldNames;

        // if _defaultLocaleID is -1 then _compareInfo is initialized with InvariantCulture CompareInfo
        // otherwise it is specified by the server? for the correct compare info
        private CompareInfo _compareInfo;
        private int _defaultLocaleID;

        public FieldNameLookup(System.Collections.ObjectModel.ReadOnlyCollection<string> columnNames, int defaultLocaleID) {

            int length = columnNames.Count;
            string[] fieldNames = new string[length];
            for (int i = 0; i < length; ++i) {
                fieldNames[i] = columnNames[i];
                Debug.Assert(null != fieldNames[i], "MDAC 66681");
            }
            _fieldNames = fieldNames;
            _defaultLocaleID = defaultLocaleID;
            GenerateLookup();
        }

        public FieldNameLookup(IDataRecord reader, int defaultLocaleID) { // V1.2.3300

            int length = reader.FieldCount;
            string[] fieldNames = new string[length];
            for (int i = 0; i < length; ++i) {
                fieldNames[i] = reader.GetName(i);
                Debug.Assert(null != fieldNames[i], "MDAC 66681");
            }
            _fieldNames = fieldNames;
            _defaultLocaleID = defaultLocaleID;
        }

        public int GetOrdinal(string fieldName) { // V1.2.3300
            if (null == fieldName) {
                throw EntityUtil.ArgumentNull("fieldName");
            }
            int index = IndexOf(fieldName);
            if (-1 == index) {
                throw EntityUtil.IndexOutOfRange(fieldName);
            }
            return index;
        }

        public int IndexOf(string fieldName) { // V1.2.3300
            if (null == _fieldNameLookup) {
                GenerateLookup();
            }
            int index;
            object value = _fieldNameLookup[fieldName];
            if (null != value) {
                // via case sensitive search, first match with lowest ordinal matches
                index = (int) value;
            }
            else {
                // via case insensitive search, first match with lowest ordinal matches
                index = LinearIndexOf(fieldName, CompareOptions.IgnoreCase);
                if (-1 == index) {
                    // do the slow search now (kana, width insensitive comparison)
                    index = LinearIndexOf(fieldName, EntityUtil.StringCompareOptions);
                }
            }
            return index;
        }

        private int LinearIndexOf(string fieldName, CompareOptions compareOptions) {
            CompareInfo compareInfo = _compareInfo;
            if (null == compareInfo) {
                if (-1 != _defaultLocaleID) {
                    compareInfo = CompareInfo.GetCompareInfo(_defaultLocaleID);
                }
                if (null == compareInfo) {
                    compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                }
                _compareInfo = compareInfo;
            }
            int length = _fieldNames.Length;
            for (int i = 0; i < length; ++i) {
                if (0 == compareInfo.Compare(fieldName, _fieldNames[i], compareOptions)) {
                    _fieldNameLookup[fieldName] = i; // add an exact match for the future
                    return i;
                }
            }
            return -1;
        }

        // RTM common code for generating Hashtable from array of column names
        private void GenerateLookup() {
            int length = _fieldNames.Length;
            Hashtable hash = new Hashtable(length);

            // via case sensitive search, first match with lowest ordinal matches
            for (int i = length-1; 0 <= i; --i) {
                string fieldName = _fieldNames[i];
                hash[fieldName] = i;
            }
            _fieldNameLookup = hash;
        }
    }
}
