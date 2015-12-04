//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.Activities;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    partial class OperationPickerDialog : Form
    {
        private OperationInfoBase selectedOperation;
        private ServiceContractListItemList serviceContracts;
        private IServiceProvider serviceProvider = null;

        public OperationPickerDialog(IServiceProvider serviceProvider, bool allowNewContracts)
        {
            if (serviceProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
            //Set dialog fonts
            IUIService uisvc = (IUIService) this.serviceProvider.GetService(typeof(IUIService));
            if (uisvc != null)
            {
                this.Font = (Font) uisvc.Styles["DialogFont"];
            }
            InitializeComponent();
            this.serviceContracts = new ServiceContractListItemList(this.operationsListBox);
            this.operationsListBox.ServiceProvider = this.serviceProvider;
            this.toolStripButton1.Visible = allowNewContracts;
        }

        public OperationInfoBase SelectedOperation
        {
            get
            {
                return selectedOperation;
            }
            internal set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                selectedOperation = value;
                SelectServiceOperation(value);
            }
        }

        public void AddServiceOperation(OperationInfoBase operationInfo, Activity implementingActivity)
        {
            if (operationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationInfo");
            }

            TypedOperationInfo typedOperationInfo = operationInfo as TypedOperationInfo;
            OperationInfo workflowOperationInfo = operationInfo as OperationInfo;
            string contractName = operationInfo.GetContractFullName(null);
            // Do not add operation if the contractName is not valid. Not throwing here gives the user to fix 
            // a broken contract/operation by selecting a different operation from the UI.
            if (String.IsNullOrEmpty(contractName))
            {
                return;
            }
            ServiceContractListItem serviceContract = this.serviceContracts.Find(contractName);

            if (typedOperationInfo != null)
            {
                if (serviceContract == null)
                {
                    serviceContract = new ServiceContractListItem(this.operationsListBox);
                    serviceContract.Validating += new CancelEventHandler(ServiceContractValidating);
                    serviceContract.Name = contractName;
                    serviceContract.ContractType = typedOperationInfo.ContractType;
                    serviceContract.IsCustomContract = false;
                    AddServiceContract(serviceContract);
                }

                TypedServiceOperationListItem operationItem = new TypedServiceOperationListItem();
                operationItem.Validating += new CancelEventHandler(ServiceOperationValidating);
                operationItem.Name = typedOperationInfo.Name;
                operationItem.Operation = typedOperationInfo;

                operationItem.ImplementingActivities.Add(implementingActivity);
                serviceContract.AddOperation(operationItem);
            }
            else if (workflowOperationInfo != null)
            {
                if (serviceContract == null)
                {
                    serviceContract = new ServiceContractListItem(this.operationsListBox);
                    serviceContract.Validating += new CancelEventHandler(ServiceContractValidating);
                    serviceContract.Name = workflowOperationInfo.ContractName;
                    serviceContract.IsCustomContract = true;
                    AddServiceContract(serviceContract);
                }
                WorkflowServiceOperationListItem workflowOperationItem = new WorkflowServiceOperationListItem();
                workflowOperationItem.Validating += new CancelEventHandler(ServiceOperationValidating);
                workflowOperationItem.Operation = workflowOperationInfo;
                workflowOperationItem.ImplementingActivities.Add(implementingActivity);
                serviceContract.AddOperation(workflowOperationItem);
            }

        }

        protected override void OnHelpButtonClicked(CancelEventArgs e)
        {
            e.Cancel = true;
            GetHelp();
        }

        protected override void OnHelpRequested(HelpEventArgs e)
        {
            e.Handled = true;
            GetHelp();
        }

        private void AddHelpListItem()
        {
            if (this.operationsListBox.Items.Count == 0)
            {
                this.operationsListBox.Items.Add(new HelpListItem());
            }
        }

        private void addOperationButton_Click(object sender, EventArgs e)
        {
            ServiceContractListItem serviceContractListItem = this.operationsListBox.SelectedItem as ServiceContractListItem;
            Fx.Assert(serviceContractListItem != null, "service contract list item cannot be null");
            Fx.Assert(serviceContractListItem.IsCustomContract, " this should work only on a custom contract item");
            WorkflowServiceOperationListItem newWorkflowServiceOperationListItem = serviceContractListItem.CreateOperation();
            newWorkflowServiceOperationListItem.Validating += new CancelEventHandler(ServiceOperationValidating);
            this.AddServiceOperation(newWorkflowServiceOperationListItem);
            serviceContractListItem.SelectionOperation(newWorkflowServiceOperationListItem);
        }

        private void AddServiceContract(ServiceContractListItem serviceContractListItem)
        {
            String key = serviceContractListItem.Name;
            if (this.serviceContracts.Find(key) == null)
            {
                RemoveHelpListItem();
                serviceContracts.Add(serviceContractListItem);
                operationsListBox.Items.Add(serviceContractListItem);
            }
        }
        private void AddServiceOperation(ServiceOperationListItem serviceOperation)
        {
            if (serviceOperation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceOperation");
            }
            String key = serviceOperation.ContractName;
            ServiceContractListItem serviceContract = this.serviceContracts.Find(key);
            serviceContract.AddOperation(serviceOperation);
            serviceContract.SelectionOperation(serviceOperation);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void GetHelp()
        {
            DesignerHelpers.ShowHelpFromKeyword(this.serviceProvider, typeof(OperationPickerDialog).FullName + ".UI");
        }
        private List<MethodInfo> GetMethodsFromInterface(Type serviceContractInterfaceType)
        {
            List<MethodInfo> methodInfos = new List<MethodInfo>();
            Queue<Type> interfacesQueue = new Queue<Type>();
            List<Type> interfacesList = new List<Type>();
            interfacesQueue.Enqueue(serviceContractInterfaceType);

            while (interfacesQueue.Count > 0)
            {
                Type currentInterfaceType = interfacesQueue.Dequeue();
                interfacesList.Add(currentInterfaceType);
                foreach (Type baseInteface in currentInterfaceType.GetInterfaces())
                {
                    if (!interfacesList.Contains(baseInteface))
                    {
                        interfacesQueue.Enqueue(baseInteface);
                    }
                }
                methodInfos.AddRange(currentInterfaceType.GetMethods());
            }
            return methodInfos;
        }

        private void ImportContract(Type serviceContractType)
        {

            foreach (MethodInfo methodInfo in GetMethodsFromInterface(serviceContractType))
            {
                if (!ServiceOperationHelpers.IsValidServiceOperation(methodInfo))
                {
                    continue;
                }
                TypedServiceOperationListItem operationItem = new TypedServiceOperationListItem();
                operationItem.Validating += new CancelEventHandler(ServiceOperationValidating);
                operationItem.Name = ServiceOperationHelpers.GetOperationName(this.serviceProvider, methodInfo);
                operationItem.Operation.ContractType = serviceContractType;
                operationItem.Operation.Name = ServiceOperationHelpers.GetOperationName(this.serviceProvider, methodInfo);
                this.AddServiceOperation(operationItem);
            }
        }

        void ImportContractButtonClicked(object sender, EventArgs e)
        {
            using (TypeBrowserDialog typeBrowserDialog = new TypeBrowserDialog(serviceProvider as IServiceProvider, new ServiceContractsTypeFilterProvider(), "System.String"))
            {
                typeBrowserDialog.ShowDialog();
                if (typeBrowserDialog.SelectedType != null)
                {
                    ServiceContractListItem contractItem = new ServiceContractListItem(this.operationsListBox);
                    contractItem.Validating += new CancelEventHandler(ServiceContractValidating);
                    contractItem.Name = typeBrowserDialog.SelectedType.FullName;
                    contractItem.ContractType = typeBrowserDialog.SelectedType;
                    contractItem.IsCustomContract = false;
                    CancelEventArgs cancelEventArgs = new CancelEventArgs();
                    contractItem.Validating.Invoke(contractItem, cancelEventArgs);
                    if (cancelEventArgs.Cancel)
                    {
                        return;
                    }
                    AddServiceContract(contractItem);

                    ImportContract(typeBrowserDialog.SelectedType);
                }
            }
        }

        void NewContractButtonClicked(object sender, EventArgs e)
        {
            ServiceContractListItem contractItem = this.serviceContracts.CreateWithUniqueName();
            contractItem.Validating += new CancelEventHandler(ServiceContractValidating);
            contractItem.IsCustomContract = true;
            this.AddServiceContract(contractItem);
            WorkflowServiceOperationListItem newWorkflowServiceOperationListItem = contractItem.CreateOperation();
            newWorkflowServiceOperationListItem.Validating += new CancelEventHandler(ServiceOperationValidating);
            this.AddServiceOperation(newWorkflowServiceOperationListItem);
            contractItem.SelectionOperation(newWorkflowServiceOperationListItem);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            object selectedObject = operationsListBox.SelectedItem;
            if (selectedObject == null)
            {
                selectedOperation = null;
            }
            if (selectedObject is TypedServiceOperationListItem)
            {
                selectedOperation = ((TypedServiceOperationListItem) selectedObject).Operation;
            }
            else if (selectedObject is WorkflowServiceOperationListItem)
            {
                selectedOperation = ((WorkflowServiceOperationListItem) selectedObject).Operation;
            }
            else
            {
                // dont close the dialog when contracts are selected
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OperationPickerDialogLoad(object sender, EventArgs e)
        {
            if (this.serviceContracts.Count == 0)
            {
                AddHelpListItem();
            }
            foreach (ServiceContractListItem serviceContractListItem in this.serviceContracts)
            {
                if (!serviceContractListItem.IsCustomContract)
                {
                    ImportContract(serviceContractListItem.ContractType);
                }
            }
            this.importContractButton.Click += new EventHandler(ImportContractButtonClicked);
            this.operationsListBox.DoubleClick += new EventHandler(operationsListBox_DoubleClick);
            this.operationsListBox.SelectedIndexChanged += new EventHandler(operationsListBox_SelectedIndexChanged);
            this.addOperationButton.Click += new EventHandler(addOperationButton_Click);
            this.toolStripButton1.Click += new EventHandler(NewContractButtonClicked);
            this.okButton.Click += new EventHandler(okButton_Click);


            // This is to make the selected operation the selected item in the operationsListBox.
            // This needs to be done to work around the [....] bug causing selection events to not fire till form is loaded.
            if (this.selectedOperation != null)
            {
                SelectServiceOperation(this.selectedOperation);
            }
            else // select the first operation if no selectedoperation is set
            {
                if (operationsListBox.Items.Count > 0)
                {
                    operationsListBox.SetSelected(0, true);
                }
            }
        }


        void operationsListBox_DoubleClick(object sender, EventArgs e)
        {
            okButton_Click(sender, e);
        }

        private void operationsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (operationsListBox.SelectedItemViewControl != null)
            {
                this.addOperationButton.Visible = false;
                detailsViewPanel.Visible = false;
                // Give the old details control a chance to valiadte its fields before tearing it down.
                foreach (Control control in detailsViewPanel.Controls)
                {
                    UserControl userControl = control as UserControl;
                    if (userControl != null)
                    {
                        userControl.ValidateChildren();
                    }
                }
                detailsViewPanel.Controls.Clear();
                operationsListBox.SelectedItemViewControl.Dock = DockStyle.Fill;
                detailsViewPanel.Controls.Add(operationsListBox.SelectedItemViewControl);
                detailsViewPanel.Visible = true;
                if (operationsListBox.SelectedItem is ServiceOperationListItem)
                {
                    this.okButton.Enabled = true;
                }
                else
                {
                    this.okButton.Enabled = false;
                    ServiceContractListItem serviceContractListItem = this.operationsListBox.SelectedItem as ServiceContractListItem;
                    if ((serviceContractListItem != null) && (serviceContractListItem.IsCustomContract))
                    {
                        this.addOperationButton.Visible = true;
                    }
                }
            }

        }

        private void RemoveHelpListItem()
        {
            if (this.operationsListBox.Items.Count > 0 && this.operationsListBox.Items[0] is HelpListItem)
            {
                this.operationsListBox.Items.Clear();
            }
        }

        private void SelectServiceOperation(OperationInfoBase operationInfo)
        {
            Fx.Assert(operationInfo != null, "operationInfo cannot be null");
            ServiceContractListItem serviceContract = this.serviceContracts.Find(operationInfo.GetContractFullName(null));
            // Dont select operation if the contract cannot be found in the serviceContracts list
            if (serviceContract == null)
            {
                return;
            }
            ServiceOperationListItem operationItem = null;
            if (operationInfo is OperationInfo)
            {
                operationItem = new WorkflowServiceOperationListItem();
                operationItem.Validating += new CancelEventHandler(ServiceOperationValidating);

                operationItem.Name = operationInfo.Name;
                ((WorkflowServiceOperationListItem) operationItem).Operation = operationInfo as OperationInfo;

            }
            else if (operationInfo is TypedOperationInfo)
            {
                operationItem = new TypedServiceOperationListItem();
                operationItem.Validating += new CancelEventHandler(ServiceOperationValidating);
                operationItem.Name = operationInfo.Name;
                ((TypedServiceOperationListItem) operationItem).Operation = operationInfo as TypedOperationInfo;
            }

            serviceContract.SelectionOperation(operationItem);
        }

        void ServiceContractValidating(object sender, CancelEventArgs e)
        {
            ServiceContractListItem serviceContractListItem = (ServiceContractListItem) sender;

            if (string.IsNullOrEmpty(serviceContractListItem.Name))
            {
                e.Cancel = true;
                string errorString = SR2.GetString(SR2.ContractNameCannotBeEmpty);
                DesignerHelpers.ShowMessage(this.serviceProvider, errorString, System.Workflow.ComponentModel.Design.DR.GetString(System.Workflow.ComponentModel.Design.DR.WorkflowDesignerTitle), MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

            }
            bool duplicatesFound = false;
            foreach (ServiceContractListItem foundContract in serviceContracts)
            {
                if (foundContract == serviceContractListItem)
                {
                    continue;
                }

                // allow reimport of existing imported contracts
                if (!serviceContractListItem.IsCustomContract && serviceContractListItem.ContractType.Equals(foundContract.ContractType))
                {
                    continue;
                }

                if (foundContract.Name.Equals(serviceContractListItem.Name))
                {
                    duplicatesFound = true;
                    break;
                }

            }
            // contract names must be unique
            if (duplicatesFound)
            {

                e.Cancel = true;
                string errorString = SR2.GetString(SR2.ContractNameMustBeUnique);
                DesignerHelpers.ShowMessage(this.serviceProvider, errorString, System.Workflow.ComponentModel.Design.DR.GetString(System.Workflow.ComponentModel.Design.DR.WorkflowDesignerTitle), MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

            }
        }




        void ServiceOperationValidating(object sender, CancelEventArgs e)
        {

            ServiceOperationListItem serviceOperationListItem = (ServiceOperationListItem) sender;
            string newOperationName = serviceOperationListItem.Name;
            string contractName = serviceOperationListItem.ContractName;
            if (string.IsNullOrEmpty(newOperationName))
            {
                e.Cancel = true;
                string errorString = SR2.GetString(SR2.OperationNameCannotBeEmpty);
                DesignerHelpers.ShowMessage(this.serviceProvider, errorString, System.Workflow.ComponentModel.Design.DR.GetString(System.Workflow.ComponentModel.Design.DR.WorkflowDesignerTitle), MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            Fx.Assert(!string.IsNullOrEmpty(contractName), "contract name should be valid to run this check");
            ServiceContractListItem contractListItem = serviceContracts.Find(contractName);
            Fx.Assert(contractListItem != null, "contract should be present in the list to run this check");

            // operation names must be unique inside a contract
            bool duplicatesFound = false;
            foreach (ServiceOperationListItem foundOperation in contractListItem.Operations)
            {
                if (foundOperation == serviceOperationListItem)
                {
                    continue;
                }
                if (foundOperation.Name.Equals(newOperationName))
                {
                    duplicatesFound = true;
                    break;
                }

            }
            if (duplicatesFound)
            {

                e.Cancel = true;
                string errorString = SR2.GetString(SR2.OperationNameMustBeUnique);
                DesignerHelpers.ShowMessage(this.serviceProvider, errorString, System.Workflow.ComponentModel.Design.DR.GetString(System.Workflow.ComponentModel.Design.DR.WorkflowDesignerTitle), MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

            }

        }

        [ListItemView(typeof(HelpListItemViewControl))]
        [ListItemDetailView(typeof(ListItemViewControl))]
        private class HelpListItem
        {
        }

        private class HelpListItemViewControl : ListItemViewControl
        {

            protected override void OnLoad(EventArgs e)
            {
                this.Width = this.Parent.Width - 10;
                Label label = new Label();
                label.ForeColor = System.Drawing.SystemColors.GrayText;
                label.Dock = DockStyle.Fill;
                this.Font = new Font(this.Font, FontStyle.Italic);
                label.Text = SR2.GetString(SR2.AddOperationsUsingImportAddButtons);
                this.Controls.Add(label);
            }
        }

        private class ServiceContractsTypeFilterProvider : ITypeFilterProvider
        {

            string ITypeFilterProvider.FilterDescription
            {
                get { return SR2.GetString(SR2.ChooseAServiceContractFromBelow); }
            }
            bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
            {
                if (!type.IsInterface)
                {
                    return false;
                }
                if (type.IsDefined(typeof(ServiceContractAttribute), true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


    }
}
