//------------------------------------------------------------------------------
// <copyright file="PageEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;

namespace System.Web.UI.WebControls {

    public class PageEventArgs : EventArgs {
        private int _startRowIndex;
        private int _maximumRows;
        private int _totalRowCount;

        public PageEventArgs(int startRowIndex, int maximumRows, int totalRowCount) {
            _startRowIndex = startRowIndex;
            _maximumRows = maximumRows;
            _totalRowCount = totalRowCount;
        }

        public int MaximumRows {
            get {
                return _maximumRows;
            }
        }

        public int StartRowIndex {
            get {
                return _startRowIndex;
            }
        }

        public int TotalRowCount {
            get {
                return _totalRowCount;
            }
        }
    }
}
