//------------------------------------------------------------------------------
// <copyright file="StreamUpdate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    //
    // Tracks updates to a stream.
    //
    internal class StreamUpdate {
        private string  _newStreamname;
        private bool    _writeCompleted;

        internal StreamUpdate(string newStreamname) {
            _newStreamname = newStreamname;
        }

        // desired new stream name
        internal string NewStreamname {
            get {return _newStreamname;}
        }

        // indicates whether the change from the old stream name
        // to the new stream name has been completed.
        internal bool WriteCompleted {
            get {return _writeCompleted;}
            set {_writeCompleted = value;}
        }
    }
}
