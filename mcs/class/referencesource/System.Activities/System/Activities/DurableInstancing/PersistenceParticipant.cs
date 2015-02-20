//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Xml.Linq;

    public abstract class PersistenceParticipant : IPersistencePipelineModule
    {
        bool isSaveTransactionRequired;
        bool isLoadTransactionRequired;
        bool isIOParticipant;

        protected PersistenceParticipant()
        {
        }

        internal PersistenceParticipant(bool isSaveTransactionRequired, bool isLoadTransactionRequired)
        {
            this.isIOParticipant = true;
            this.isSaveTransactionRequired = isSaveTransactionRequired;
            this.isLoadTransactionRequired = isLoadTransactionRequired;
        }
        
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters, 
            Justification = "arch approved design. requires the two out dictionaries to avoid complex structures")]
        protected virtual void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        {
            readWriteValues = null;
            writeOnlyValues = null;
        }

        // Passed-in dictionaries are read-only.
        protected virtual IDictionary<XName, object> MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            return null;
        }

        // Passed-in dictionary is read-only.
        protected virtual void PublishValues(IDictionary<XName, object> readWriteValues)
        {
        }

        void IPersistencePipelineModule.CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        {
            CollectValues(out readWriteValues, out writeOnlyValues);
        }

        IDictionary<XName, object> IPersistencePipelineModule.MapValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            return MapValues(readWriteValues, writeOnlyValues);
        }

        void IPersistencePipelineModule.PublishValues(IDictionary<XName, object> readWriteValues)
        {
            PublishValues(readWriteValues);
        }

        bool IPersistencePipelineModule.IsIOParticipant
        {
            get
            {
                return this.isIOParticipant;
            }
        }

        bool IPersistencePipelineModule.IsSaveTransactionRequired
        {
            get
            {
                return this.isSaveTransactionRequired;
            }
        }

        bool IPersistencePipelineModule.IsLoadTransactionRequired
        {
            get
            {
                return this.isLoadTransactionRequired;
            }
        }

        IAsyncResult IPersistencePipelineModule.BeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InternalBeginOnSave(readWriteValues, writeOnlyValues, timeout, callback, state);
        }

        void IPersistencePipelineModule.EndOnSave(IAsyncResult result)
        {
            InternalEndOnSave(result);
        }

        IAsyncResult IPersistencePipelineModule.BeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InternalBeginOnLoad(readWriteValues, timeout, callback, state);
        }

        void IPersistencePipelineModule.EndOnLoad(IAsyncResult result)
        {
            InternalEndOnLoad(result);
        }

        void IPersistencePipelineModule.Abort()
        {
            InternalAbort();
        }

        internal virtual IAsyncResult InternalBeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("BeginOnSave should not be called on PersistenceParticipant.");
        }

        internal virtual void InternalEndOnSave(IAsyncResult result)
        {
            Fx.Assert("EndOnSave should not be called on PersistenceParticipant.");
        }

        internal virtual IAsyncResult InternalBeginOnLoad(IDictionary<XName, object> readWriteValues, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("BeginOnLoad should not be called on PersistenceParticipant.");
        }

        internal virtual void InternalEndOnLoad(IAsyncResult result)
        {
            Fx.Assert("EndOnLoad should not be called on PersistenceParticipant.");
        }

        internal virtual void InternalAbort()
        {
        }
    }
}
