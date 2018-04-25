//------------------------------------------------------------------------------
// <copyright file="ReadAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * IAsyncResult for asynchronous read.
 *
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web.Hosting {
    using System;
    using System.Threading;
    using System.Web.Util;

    internal class ReadAsyncResult : AsyncResultBase {
        private int              _bytesRead;
        private byte[]           _buffer;
        private int              _offset;
        private int              _count;
        private bool             _updatePerfCounter;

        internal ReadAsyncResult(AsyncCallback cb, Object state, byte[] buffer, int offset, int count, bool updatePerfCounter): base(cb, state) {
            _buffer = buffer;
            _offset = offset;
            _count = count;
            _updatePerfCounter = updatePerfCounter;
        }

        internal override void Complete(int bytesRead, int hresult, IntPtr pbAsyncReceiveBuffer, bool synchronous) {
            if (_updatePerfCounter && bytesRead > 0) {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, bytesRead);
            }

            if (bytesRead != 0) {
                CopyBytes(pbAsyncReceiveBuffer, bytesRead);
            }
            _bytesRead = bytesRead;

            Complete(hresult, synchronous);
        }

        private unsafe void CopyBytes(IntPtr pbAsyncReceiveBuffer, int bytesRead) {
            byte * src = ((byte *)pbAsyncReceiveBuffer);
            fixed (byte * dst = _buffer) {
                StringUtil.memcpyimpl(src, dst + _offset, bytesRead);
            }
        }

        internal int BytesRead { get { return _bytesRead; } set { _bytesRead = value; } }
    }
}
