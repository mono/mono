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
                Object id = CallContext.LogicalGetData(activityIdSlotName);
                if (id != null)
                    return (Guid) id;
                else
                    return Guid.Empty;
            }
            set {
                CallContext.LogicalSetData(activityIdSlotName, value);
            }
        }

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

    }
}
