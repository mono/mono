//------------------------------------------------------------------------------
// <copyright file="_NestedSingleAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    //
    // The NestedAsyncResult - used to wrap async requests
    //      this is used to hold another async result made
    //      through a call to another Begin call within.
    //
    internal class NestedSingleAsyncResult : LazyAsyncResult {
        //
        // this is usually for operations on streams/buffers,
        // we save information passed in on the Begin call:
        // since some calls might need several completions, we
        // need to save state on the user's IO request
        //
        internal byte[] Buffer;
        internal int Offset;
        internal int Size;

        //
        // Constructors
        //

        // Completed in advance.
        internal NestedSingleAsyncResult(Object asyncObject, Object asyncState, AsyncCallback asyncCallback, object result) :
            base(asyncObject, asyncState, asyncCallback, result)
        { }

        internal NestedSingleAsyncResult(Object asyncObject, Object asyncState, AsyncCallback asyncCallback, byte[] buffer, int offset, int size)
        : base( asyncObject, asyncState, asyncCallback ) {
            Buffer = buffer;
            Offset = offset;
            Size = size;
        }

    }; // class NestedAsyncResult
} // namespace System.Net
