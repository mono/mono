/******************************************************************************
* The MIT License
* Copyright (c) 2007 Novell Inc.,  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest (twiest@novell.com)
//		Rusty Howell  (rhowell@novell.com)
//
// (C)  Novell Inc.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Management.Internal.BaseDataTypes;

namespace System.Management.Internal.Batch
{
    internal class BatchResponse  : BaseCollection<SingleResponse>
    {
        string _cimVersion = string.Empty;
        string _dtdVersion = string.Empty;
        string _messageId = string.Empty;
        string _protocolVersion = string.Empty;

        #region Constructors
        public BatchResponse()
        {
        }

        public BatchResponse(CimXmlHeader header)        
        {
            SetFromHeader(header);
        }

        public BatchResponse(int capacity):base(capacity)
        {
        }

        public BatchResponse(CimXmlHeader header, int capacity): base(capacity)
        {
            SetFromHeader(header);
        }
        #endregion

        #region Properties and Indexers
        public string CimVersion
        {
            get { return _cimVersion; }
            set { _cimVersion = value; }
        }

        public string DtdVersion
        {
            get { return _dtdVersion; }
            set { _dtdVersion = value; }
        }

        public string MessageId
        {
            get { return _messageId; }
            set { _messageId = value; }
        }

        public string ProtocolVersion
        {
            get { return _protocolVersion; }
            set { _protocolVersion = value; }
        }

        public SingleResponse LastResponse
        {
            get { return items[items.Count - 1]; }
        }
        #endregion

        #region Methods
        private void SetFromHeader(CimXmlHeader header)
        {
            this.CimVersion = header.CimVersion;
            this.DtdVersion = header.DtdVersion;
            this.MessageId = header.MessageId;
            this.ProtocolVersion = header.ProtocolVersion;
        }
        #endregion
    }
}
