//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal partial class ServiceContractDetailViewControl : ListItemViewControl
    {

        public ServiceContractDetailViewControl()
        {
            InitializeComponent();
        }
        public override event EventHandler ItemChanged;

        public override void UpdateView()
        {
            ServiceContractListItem listItem = this.Item as ServiceContractListItem;
            Fx.Assert(listItem != null, "listItem needs to be a ServiceContractListItem");
            contractNameTextBox.Text = listItem.Name;
            contractNameTextBox.Enabled = true;
            contractNameTextBox.ReadOnly = true;
            if (listItem.IsCustomContract)
            {
                this.contractNameTextBox.ReadOnly = false;
                this.contractNameTextBox.Validated += new EventHandler(contractNameTextBox_Validated);
                this.contractNameTextBox.Validating += new CancelEventHandler(contractNameTextBox_Validating);
            }
            base.UpdateView();
        }

        void contractNameTextBox_Validated(object sender, EventArgs e)
        {
            ServiceContractListItem contractListItem = (ServiceContractListItem) this.Item;
            UpdateImplementingActivities(contractListItem);
            // finally notify other observers of this change
            if (this.ItemChanged != null)
            {
                this.ItemChanged.Invoke(this, null);
            }
        }



        void contractNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            ServiceContractListItem contractListItem = (ServiceContractListItem) this.Item;
            string oldName = contractListItem.Name;
            contractListItem.Name = this.contractNameTextBox.Text;
            if (contractListItem.Validating != null)
            {
                contractListItem.Validating.Invoke(contractListItem, e);
            }
            if (e.Cancel)
            {
                this.contractNameTextBox.Text = oldName;
                contractListItem.Name = oldName;
                e.Cancel = false;
            }

        }


        private void UpdateImplementingActivities(ServiceContractListItem listItem)
        {
            foreach (WorkflowServiceOperationListItem workflowOperationListItem in listItem.Operations)
            {
                Fx.Assert(workflowOperationListItem != null, "Operations inside an editable contract should only be workflow first operations");
                workflowOperationListItem.Operation.ContractName = listItem.Name;
                // update the activities implementing the operation too
                foreach (Activity activity in workflowOperationListItem.ImplementingActivities)
                {
                    PropertyDescriptorUtils.SetPropertyValue(this.ServiceProvider, ServiceOperationHelpers.GetServiceOperationInfoPropertyDescriptor(activity), activity, workflowOperationListItem.Operation.Clone());
                }

            }
        }
    }
}
