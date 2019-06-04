//------------------------------------------------------------------------------
// <copyright file="CorrelationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Runtime.Remoting.Messaging;


namespace System.Diagnostics {
    public class CorrelationManager {
        private const string transactionSlotName = "System.Diagnostics.Trace.CorrelationManagerSlot";
        private const string activityIdSlotName = "E2ETrace.ActivityID";
        
        internal CorrelationManager() { }

        public Guid ActivityId {
            get {
#if !DISABLE_REMOTING
                Object id = CallContext.LogicalGetData(activityIdSlotName);
                if (id != null)
                    return (Guid) id;
                else
#endif
                    return Guid.Empty;
            }
            set {
#if !DISABLE_REMOTING
                CallContext.LogicalSetData(activityIdSlotName, value);
#endif
            }
        }

#if !DISABLE_REMOTING
        public Stack LogicalOperationStack {
            get {
                return GetLogicalOperationStack();
            }
        }
        
        public void StartLogicalOperation(object operationId) {
            if (operationId == null)
                throw new ArgumentNullException("operationId");

            Stack idStack = GetLogicalOperationStack();
            idStack.Push(operationId);
        }

        public void StartLogicalOperation() {
            StartLogicalOperation(Guid.NewGuid());
        }

        public void StopLogicalOperation() {
            Stack idStack = GetLogicalOperationStack();
            idStack.Pop();
        }

        private Stack GetLogicalOperationStack() {
            Stack idStack = CallContext.LogicalGetData(transactionSlotName) as Stack;
            if (idStack == null) {
                idStack = new Stack();
                CallContext.LogicalSetData(transactionSlotName, idStack);
            }

            return idStack;
        }
#else
        public Stack LogicalOperationStack => throw new PlatformNotSupportedException ();
        public void StartLogicalOperation (object operationId) => throw new PlatformNotSupportedException ();
        public void StartLogicalOperation () => throw new PlatformNotSupportedException ();
        public void StopLogicalOperation () => throw new PlatformNotSupportedException ();
#endif
    }
}
