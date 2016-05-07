//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    interface IPersistencePipelineModule
    {
        bool IsIOParticipant { get; }
        bool IsSaveTransactionRequired { get; }
        bool IsLoadTransactionRequired { get; }

        void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues);
        IDictionary<XName, object> MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues);
        void PublishValues(IDictionary<XName, object> readWriteValues);

        IAsyncResult BeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state);
        void EndOnSave(IAsyncResult result);

        IAsyncResult BeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state);
        void EndOnLoad(IAsyncResult result);

        void Abort();
    }
}
