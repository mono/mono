//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Activities.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    [ListItemView(typeof(ServiceContractViewControl))]
    [ListItemDetailView(typeof(ServiceContractDetailViewControl))]
    class ServiceContractListItem : object
    {
        ListBox container;
        Type contractType;
        bool isCustomContract;
        ServiceOperationListItem lastItemAdded;
        string name;
        ServiceOperationListItemList operations;

        public ServiceContractListItem(ListBox container)
        {
            if (container == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("container");
            }
            this.operations = new ServiceOperationListItemList();
            this.container = container;
        }

        public CancelEventHandler Validating;

        public Type ContractType
        {
            get { return contractType; }
            set { contractType = value; }
        }

        public bool IsCustomContract
        {
            get { return isCustomContract; }
            set { isCustomContract = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public IEnumerable<ServiceOperationListItem> Operations
        {
            get { return operations; }
        }



        public void AddOperation(ServiceOperationListItem operation)
        {
            if (operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
            }
            // Dont add operation if operation.Name is broken
            if (String.IsNullOrEmpty(operation.Name))
            {
                return;
            }
            ServiceOperationListItem cachedItem = this.operations.Find(operation.Name);
            if (cachedItem != null)
            {
                foreach (Activity activity in operation.ImplementingActivities)
                {
                    if (!cachedItem.ImplementingActivities.Contains(activity))
                    {
                        cachedItem.ImplementingActivities.Add(activity);
                    }
                }
            }
            else
            {
                this.operations.Add(operation);
                int positionToAddAt = this.container.Items.IndexOf(this) + 1;
                if (this.operations.Count > 1)
                {
                    positionToAddAt = this.container.Items.IndexOf(lastItemAdded) + 1;
                }
                lastItemAdded = operation;
                this.container.Items.Insert(positionToAddAt, operation);
            }
        }

        public WorkflowServiceOperationListItem CreateOperation()
        {
            WorkflowServiceOperationListItem result = (WorkflowServiceOperationListItem) this.operations.CreateWithUniqueName();
            result.Operation.ContractName = this.Name;
            return result;
        }

        public ServiceOperationListItem Find(string operationName)
        {
            Fx.Assert(operationName != null, "operationName != null");
            return this.operations.Find(operationName);
        }

        public void SelectionOperation(ServiceOperationListItem operation)
        {
            if (operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
            }
            // Dont select if operation.Name is broken
            if (String.IsNullOrEmpty(operation.Name))
            {
                return;
            }
            ServiceOperationListItem operationListItem = this.operations.Find(operation.Name);
            if (operationListItem != null)
            {
                this.container.SetSelected(container.Items.IndexOf(operationListItem), true);
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
