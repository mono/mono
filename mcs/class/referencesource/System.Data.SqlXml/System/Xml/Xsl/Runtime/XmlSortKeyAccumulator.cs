//------------------------------------------------------------------------------
// <copyright file="XmlSortKeyAccumulator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;

namespace System.Xml.Xsl.Runtime {

    /// <summary>
    /// Accumulates a list of sort keys and stores them in an array.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct XmlSortKeyAccumulator {
        private XmlSortKey[] keys;
        private int pos;

    #if DEBUG
        private const int DefaultSortKeyCount = 4;
    #else
        private const int DefaultSortKeyCount = 64;
    #endif

        /// <summary>
        /// Initialize the XmlSortKeyAccumulator.
        /// </summary>
        public void Create() {
            if (this.keys == null)
                this.keys = new XmlSortKey[DefaultSortKeyCount];

            this.pos = 0;
            this.keys[0] = null;
        }

        /// <summary>
        /// Create a new sort key and append it to the current run of sort keys.
        /// </summary>
        public void AddStringSortKey(XmlCollation collation, string value) {
            AppendSortKey(collation.CreateSortKey(value));
        }

        public void AddDecimalSortKey(XmlCollation collation, decimal value) {
            AppendSortKey(new XmlDecimalSortKey(value, collation));
        }

        public void AddIntegerSortKey(XmlCollation collation, long value) {
            AppendSortKey(new XmlIntegerSortKey(value, collation));
        }

        public void AddIntSortKey(XmlCollation collation, int value) {
            AppendSortKey(new XmlIntSortKey(value, collation));
        }

        public void AddDoubleSortKey(XmlCollation collation, double value) {
            AppendSortKey(new XmlDoubleSortKey(value, collation));
        }

        public void AddDateTimeSortKey(XmlCollation collation, DateTime value) {
            AppendSortKey(new XmlDateTimeSortKey(value, collation));
        }

        public void AddEmptySortKey(XmlCollation collation) {
            AppendSortKey(new XmlEmptySortKey(collation));
        }

        /// <summary>
        /// Finish creating the current run of sort keys and begin a new run.
        /// </summary>
        public void FinishSortKeys() {
            this.pos++;
            if (this.pos >= this.keys.Length) {
                XmlSortKey[] keysNew = new XmlSortKey[this.pos * 2];
                Array.Copy(this.keys, 0, keysNew, 0, this.keys.Length);
                this.keys = keysNew;
            }
            this.keys[this.pos] = null;
        }

        /// <summary>
        /// Append new sort key to the current run of sort keys.
        /// </summary>
        private void AppendSortKey(XmlSortKey key) {
            // Ensure that sort will be stable by setting index of key
            key.Priority = this.pos;

            if (this.keys[this.pos] == null)
                this.keys[this.pos] = key;
            else
                this.keys[this.pos].AddSortKey(key);
        }

        /// <summary>
        /// Get array of sort keys that was constructed by this internal class.
        /// </summary>
        public Array Keys {
            get { return this.keys; }
        }
    }
}
