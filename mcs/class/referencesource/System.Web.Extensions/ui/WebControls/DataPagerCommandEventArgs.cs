//------------------------------------------------------------------------------
// <copyright file="DataPagerCommandEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI.WebControls {
    public class DataPagerCommandEventArgs : CommandEventArgs {
        private DataPagerField _pagerField;
        private int _totalRowCount;
        private int _newMaximumRows = -1;
        private int _newStartRowIndex = -1;
        private DataPagerFieldItem _item;

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "2#")]
        public DataPagerCommandEventArgs(DataPagerField pagerField, int totalRowCount, CommandEventArgs originalArgs, DataPagerFieldItem item) : base(originalArgs) {
            _pagerField = pagerField;
            _totalRowCount = totalRowCount;
            _item = item;
        }
        
        public DataPagerFieldItem Item {
            get {
                return _item;
            }
        }

        public int NewMaximumRows {
            get {
                return _newMaximumRows;
            }
            set {
                _newMaximumRows = value;
            }
        }

        public int NewStartRowIndex {
            get {
                return _newStartRowIndex;
            }
            set {
                _newStartRowIndex = value;
            }
        }

        public DataPagerField PagerField {
            get {
                return _pagerField;
            }
        }

        public int TotalRowCount {
            get {
                return _totalRowCount;
            }
        }
    }
}
