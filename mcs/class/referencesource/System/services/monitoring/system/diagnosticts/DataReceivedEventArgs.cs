//------------------------------------------------------------------------------
// <copyright file="Process.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System;

    public delegate void DataReceivedEventHandler(Object sender, DataReceivedEventArgs e);
    
    public class DataReceivedEventArgs : EventArgs {
        internal String _data;
        
        internal DataReceivedEventArgs(String data) {
            _data = data;
        }

        public String Data {
            get {
                return _data;
            }
        }
    }
}
