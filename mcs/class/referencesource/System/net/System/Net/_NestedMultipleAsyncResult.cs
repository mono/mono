//------------------------------------------------------------------------------
// <copyright file="_NestedMultipleAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    //
    // The NestedAsyncResult - used to wrap async requests
    //      this is used to hold another async result made
    //      through a call to another Begin call within.
    //
    internal class NestedMultipleAsyncResult : LazyAsyncResult {
        //
        // this is usually for operations on streams/buffers,
        // we save information passed in on the Begin call:
        // since some calls might need several completions, we
        // need to save state on the user's IO request
        //
        internal BufferOffsetSize[] Buffers;
        internal int Size;

        //
        // Constructor:
        //
        internal NestedMultipleAsyncResult(Object asyncObject, Object asyncState, AsyncCallback asyncCallback, BufferOffsetSize[] buffers)
        : base( asyncObject, asyncState, asyncCallback ) {
            Buffers = buffers;
            Size = 0;
            for (int i = 0; i < Buffers.Length; i++) {
                Size += Buffers[i].Size;
            }
        }

    }; // class NestedMultipleAsyncResult
} // namespace System.Net
