//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Activities.Design
{
    using System;
    using System.Runtime;

    [ListItemView(typeof(ServiceOperationViewControl))]
    [ListItemDetailView(typeof(ServiceOperationDetailViewControl))]
    internal class WorkflowServiceOperationListItem : ServiceOperationListItem
    {
        private OperationInfo operation;

        public WorkflowServiceOperationListItem()
        {
            this.Operation = new OperationInfo();
        }

        public override String ContractName
        {
            get
            {
                return this.operation.ContractName;
            }
        }

        public override string Name
        {
            get
            {
                return this.operation.Name;
            }
            set
            {
                this.operation.Name = value;
            }
        }
        public OperationInfo Operation
        {
            get { return operation; }
            set
            {
                Fx.Assert(value != null, "value should never be null");
                operation = value;
            }
        }
    }
}
