//------------------------------------------------------------------------------
// <copyright file="PagePropertiesChangingEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;

namespace System.Web.UI.WebControls {

    public class PagePropertiesChangingEventArgs : EventArgs {
        private int _startRowIndex;
        private int _maximumRows;

        public PagePropertiesChangingEventArgs(int startRowIndex, int maximumRows) {
            _startRowIndex = startRowIndex;
            _maximumRows = maximumRows;
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
    }
}
