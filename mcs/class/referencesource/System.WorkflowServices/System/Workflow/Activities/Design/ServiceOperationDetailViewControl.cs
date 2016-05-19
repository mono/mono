//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;

    internal partial class ServiceOperationDetailViewControl : ListItemViewControl
    {
        static CodeDomProvider csharpProvider = null;
        static CodeDomProvider vbProvider = null;
        DataGridViewComboBoxColumn directionColumn;
        Button formCancelButton = null;
        bool isEditable = false;
        DataGridViewTextBoxColumn nameColumn;
        OperationInfoBase operationInfoBase = null;
        TypeChooserCellItem typeChooserCellItem;
        DataGridViewComboBoxColumn typeColumn;
        public ServiceOperationDetailViewControl()
        {
            InitializeComponent();

            this.protectionLevelComboBox.Items.Add(SR2.GetString(SR2.UseRuntimeDefaults));
            this.protectionLevelComboBox.Items.Add(ProtectionLevel.None);
            this.protectionLevelComboBox.Items.Add(ProtectionLevel.Sign);
            this.protectionLevelComboBox.Items.Add(ProtectionLevel.EncryptAndSign);
        }

        public override event EventHandler ItemChanged;

        private string ParameterTemplateRowName
        {
            get
            {
                return SR2.GetString(SR2.ParameterTemplateRowName);
            }

        }

        public override void UpdateView()
        {
            // set the dialog fonts from vs.
            IUIService uisvc = (IUIService) this.ServiceProvider.GetService(typeof(IUIService));
            if (uisvc != null)
            {
                this.Font = (Font) uisvc.Styles["DialogFont"];
            }
            TypedServiceOperationListItem operationListItem = this.Item as TypedServiceOperationListItem;
            WorkflowServiceOperationListItem workflowOperationListItem = this.Item as WorkflowServiceOperationListItem;

            if (operationListItem != null)
            {
                operationInfoBase = operationListItem.Operation;
                SetEditability(false);
            }
            if (workflowOperationListItem != null)
            {
                operationInfoBase = workflowOperationListItem.Operation;
                if (workflowOperationListItem.Operation.HasProtectionLevel)
                {
                    this.protectionLevelComboBox.SelectedItem = workflowOperationListItem.Operation.ProtectionLevel;
                }
                else
                {
                    this.protectionLevelComboBox.SelectedItem = SR2.GetString(SR2.UseRuntimeDefaults);
                }
                SetEditability(true);
            }
            Fx.Assert(operationInfoBase != null, "list Item should be either ReflectedServiceOperationListItem or WorkflowServiceOperationListItem");
            this.oneWayCheckBox.Checked = operationInfoBase.GetIsOneWay(ServiceProvider);
            Fx.Assert(operationInfoBase != null, "Operation Info should be non-null at this point");
            SetupColumns();
            PopulateParametersGrid(operationInfoBase);
            if (!this.parametersGrid.ReadOnly)
            {
                this.parametersGrid.Rows.Add(this.ParameterTemplateRowName, null, null);
            }
            this.operationNameTextBox.Text = operationInfoBase.Name;
            this.permissionRoleTextBox.Text = operationInfoBase.PrincipalPermissionRole;
            this.permissionNameTextBox.Text = operationInfoBase.PrincipalPermissionName;
            this.permissionRoleTextBox.TextChanged += new EventHandler(permissionRoleTextChanged);
            if (workflowOperationListItem != null)
            {
                this.operationNameTextBox.Validating += new CancelEventHandler(operationNameTextBox_Validating);
                this.operationNameTextBox.Validated += new EventHandler(operationNameTextBox_Validated);

            }
            this.oneWayCheckBox.CheckedChanged += new EventHandler(oneWayCheckBoxCheckedChanged);
            this.permissionNameTextBox.TextChanged += new EventHandler(permissionNameTextChanged);
            this.parametersGrid.SelectionChanged += new EventHandler(parametersGrid_SelectionChanged);
            if (!this.parametersGrid.ReadOnly)
            {
                this.parametersGrid.CellValidating += new DataGridViewCellValidatingEventHandler(parametersGrid_CellValidating);
                this.parametersGrid.CellEndEdit += new DataGridViewCellEventHandler(parametersGridCellEndEdit);
                this.parametersGrid.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(parametersGrid_EditingControlShowing);
                this.parametersGrid.KeyDown += new KeyEventHandler(parametersGrid_KeyDown);
            }
            this.protectionLevelComboBox.SelectedValueChanged += new System.EventHandler(this.protectionLevelComboBoxSelectedValueChanged);
            RefreshParameterOperationButtons();
        }

        internal static string GetTypeSignature(Type type)
        {
            Fx.Assert(type != null, "type cannot be null");
            if (type.IsPrimitive)
            {
                return type.Name;
            }
            if (!type.IsGenericType)
            {
                return type.FullName;
            }
            // run time types already have complete signature in type.FullName , we only have to worry about design time types here
            if (type.FullName.Contains("["))
            {
                return type.FullName;
            }
            StringBuilder signature = new StringBuilder();
            signature.Append(type.FullName);
            signature.Append("[");
            Type[] argumentTypes = type.GetGenericArguments();
            for (int index = 0; index < argumentTypes.Length; index++)
            {
                Type argumentType = argumentTypes[index];
                signature.Append(GetTypeSignature(argumentType));
                if (index != argumentTypes.Length - 1)
                {
                    signature.Append(", ");
                }
            }
            signature.Append("]");
            return signature.ToString();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.parametersGrid.ClearSelection();
            base.OnLoad(e);
        }


        private void addParameterButton_Click(object sender, EventArgs e)
        {
            Fx.Assert(this.parametersGrid.Rows.Count != 0, "parameters grid should have atleast the dummy <add new> item");
            this.parametersGrid.Rows.Insert(this.parametersGrid.Rows.Count - 1, GenerateParameterName(), typeof(string), SR2.GetString(SR2.ParameterDirectionIn));
            // move focus to newly added cell
            this.parametersGrid.CurrentCell = this.parametersGrid.Rows[this.parametersGrid.Rows.Count - 2].Cells[this.nameColumn.Index];
            UpdateOperationParameters();
            RefreshParameterOperationButtons();
        }

        private void AddPrimitiveTypesToTypesList()
        {
            typeColumn.Items.Add(new TypeCellItem(typeof(byte)));
            typeColumn.Items.Add(new TypeCellItem(typeof(sbyte)));
            typeColumn.Items.Add(new TypeCellItem(typeof(int)));
            typeColumn.Items.Add(new TypeCellItem(typeof(uint)));
            typeColumn.Items.Add(new TypeCellItem(typeof(short)));
            typeColumn.Items.Add(new TypeCellItem(typeof(ushort)));
            typeColumn.Items.Add(new TypeCellItem(typeof(long)));
            typeColumn.Items.Add(new TypeCellItem(typeof(ulong)));
            typeColumn.Items.Add(new TypeCellItem(typeof(float)));
            typeColumn.Items.Add(new TypeCellItem(typeof(double)));
            typeColumn.Items.Add(new TypeCellItem(typeof(char)));
            typeColumn.Items.Add(new TypeCellItem(typeof(bool)));
            typeColumn.Items.Add(new TypeCellItem(typeof(object)));
            typeColumn.Items.Add(new TypeCellItem(typeof(string)));
            typeColumn.Items.Add(new TypeCellItem(typeof(decimal)));
            typeColumn.Items.Add(new TypeCellItem(typeof(void)));
        }

        void AddToTypeList(Type type)
        {
            Fx.Assert(type != null, "The type should not be null");
            foreach (TypeCellItem typeCellItem in this.typeColumn.Items)
            {
                Fx.Assert(typeCellItem != null, "should never have a null entry in this list");
                Fx.Assert(typeCellItem.Type != null, " This object should always hold a valid Type");
                if (typeCellItem.Type.Equals(type))
                {
                    return;
                }
            }
            this.typeColumn.Items.Add(new TypeCellItem(type));
        }

        private void BrowseAndSelectType(DataGridViewCell currentCell)
        {
            bool doneSelectingType = false;
            while (!doneSelectingType)
            {
                using (TypeBrowserDialog typeBrowserDialog = new TypeBrowserDialog(
                    this.ServiceProvider as IServiceProvider, new ParameterTypeFilterProvider(), "System.String"))
                {
                    doneSelectingType = TrySelectType(typeBrowserDialog, currentCell);
                }
            }
        }

        void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DataGridViewComboBoxEditingControl combo = sender as DataGridViewComboBoxEditingControl;

            DataGridViewCell currentCell = this.parametersGrid.Rows[combo.EditingControlRowIndex].Cells[this.typeColumn.Index];
            if (combo.DroppedDown && combo.Text.Equals(SR2.GetString(SR2.BrowseType)) && !typeof(TypeChooserCellItem).Equals(currentCell.Value))
            {
                BrowseAndSelectType(currentCell);
                this.parametersGrid.EndEdit();
            }
        }

        void DisableFormCancelButton()
        {
            formCancelButton = Form.ActiveForm.CancelButton as Button;
            Form.ActiveForm.CancelButton = null;
        }

        void EnableFormCancelButton()
        {
            if (formCancelButton != null)
            {
                Form.ActiveForm.CancelButton = formCancelButton;
            }
        }

        private string GenerateParameterName()
        {
            int index = 0;
            string generatedNameBase = SR2.GetString(SR2.GeneratedParameterNameBase);
            string generatedName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", generatedNameBase, ++index);
            List<string> existingNames = new List<string>();
            Fx.Assert(operationInfoBase != null, "operation info base should not be null here");
            foreach (OperationParameterInfo paramInfo in operationInfoBase.GetParameters(this.ServiceProvider))
            {
                existingNames.Add(paramInfo.Name);
            }
            while (existingNames.Contains(generatedName))
            {
                generatedName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", generatedNameBase, ++index);
            }
            return generatedName;
        }

        bool IsValidIdentifier(string name)
        {
            if (csharpProvider == null)
            {
                csharpProvider = new CSharpCodeProvider();
            }

            if (vbProvider == null)
            {
                vbProvider = new VBCodeProvider();
            }

            if (!csharpProvider.IsValidIdentifier(name))
            {
                return false;
            }

            if (!vbProvider.IsValidIdentifier(name))
            {
                return false;
            }
            return true;
        }

        private void moveParameterDownButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow currentRow = this.parametersGrid.CurrentRow;
            Fx.Assert((currentRow != null), "current row cannot be null");
            int currentRowIndex = currentRow.Index;
            Fx.Assert((currentRowIndex >= 0), "must be valid index");
            Fx.Assert(currentRowIndex < (this.parametersGrid.Rows.Count - 2), "cant move down the template row or the one above it");

            this.parametersGrid.Rows.Remove(currentRow);
            this.parametersGrid.Rows.Insert(currentRowIndex + 1, currentRow);
            this.parametersGrid.CurrentCell = currentRow.Cells[this.nameColumn.Index];
            UpdateOperationParameters();
            RefreshParameterOperationButtons();
        }

        private void moveParameterUpButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow currentRow = this.parametersGrid.CurrentRow;
            Fx.Assert((currentRow != null), "current row cannot be null");
            int currentRowIndex = currentRow.Index;
            Fx.Assert((currentRowIndex >= 1), "must be seond row or higher");
            Fx.Assert(currentRowIndex != (this.parametersGrid.Rows.Count - 1), "cant move up the template row");

            this.parametersGrid.Rows.Remove(currentRow);
            this.parametersGrid.Rows.Insert(currentRowIndex - 1, currentRow);
            this.parametersGrid.CurrentCell = currentRow.Cells[this.nameColumn.Index];
            UpdateOperationParameters();
            RefreshParameterOperationButtons();
        }

        void oneWayCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            WorkflowServiceOperationListItem workflowOperationListItem = this.Item as WorkflowServiceOperationListItem;
            Fx.Assert(workflowOperationListItem != null, "this method should only be called on workflowOperations");
            workflowOperationListItem.Operation.IsOneWay = this.oneWayCheckBox.Checked;
            OnOperationPropertiesChanged();
        }

        void OnOperationPropertiesChanged()
        {
            if (this.ItemChanged != null)
            {
                this.ItemChanged.Invoke(this, null);
            }
            UpdateImplementingActivities();
        }

        void operationNameTextBox_Validated(object sender, EventArgs e)
        {
            WorkflowServiceOperationListItem workflowOperationListItem = this.Item as WorkflowServiceOperationListItem;
            Fx.Assert(workflowOperationListItem != null, "this method should only be called on workflowOperations");
            workflowOperationListItem.Operation.Name = this.operationNameTextBox.Text;
            OnOperationPropertiesChanged();
        }

        void operationNameTextBox_Validating(object sender, CancelEventArgs e)
        {
            ServiceOperationListItem operationListItem = (ServiceOperationListItem) this.Item;
            string oldName = operationListItem.Name;
            operationListItem.Name = this.operationNameTextBox.Text;
            if (operationListItem.Validating != null)
            {
                operationListItem.Validating.Invoke(operationListItem, e);
            }
            if (e.Cancel)
            {
                this.operationNameTextBox.Text = oldName;
                operationListItem.Name = oldName;
                e.Cancel = false;
            }

        }

        void parametersGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            string errorString = string.Empty;
            bool valid = true;
            if (e.ColumnIndex == this.nameColumn.Index)
            {
                valid = ValidateParamaterName((string) e.FormattedValue, e, out errorString);
            }
            // void is not valid for parameters other than returnvalue
            if (e.ColumnIndex == this.typeColumn.Index)
            {
                if (typeof(void).ToString().Equals(e.FormattedValue) && !(this.parametersGrid.Rows[e.RowIndex].Cells[this.nameColumn.Index].Value.Equals(SR2.GetString(SR2.ReturnValueString))))
                {
                    valid = false;
                    errorString = SR2.GetString(SR2.VoidIsNotAValidParameterType);
                }
            }
            if (!valid)
            {
                e.Cancel = true;
                DesignerHelpers.ShowMessage(this.ServiceProvider, errorString, DR.GetString(DR.WorkflowDesignerTitle), MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        void parametersGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DisableFormCancelButton();
            if (this.parametersGrid.CurrentCell.ColumnIndex == this.typeColumn.Index)
            {
                DataGridViewComboBoxEditingControl comboBox = e.Control as DataGridViewComboBoxEditingControl;
                comboBox.SelectionChangeCommitted -= new EventHandler(comboBox_SelectionChangeCommitted);
                comboBox.SelectionChangeCommitted += new EventHandler(comboBox_SelectionChangeCommitted);
            }
        }

        void parametersGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && this.RemoveParameterButton.Enabled)
            {
                RemoveParameterButton_Click(this, null);
            }
        }

        void parametersGrid_SelectionChanged(object sender, EventArgs e)
        {
            RefreshParameterOperationButtons();
        }

        void parametersGridCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            EnableFormCancelButton();
            if (!this.parametersGrid.Rows[this.parametersGrid.RowCount - 1].Cells[this.nameColumn.Index].Value.Equals(this.ParameterTemplateRowName))
            {
                DataGridViewRow editedRow = this.parametersGrid.Rows[e.RowIndex];
                editedRow.Cells[this.typeColumn.Index].Value = typeof(string);
                editedRow.Cells[this.directionColumn.Index].Value = SR2.GetString(SR2.ParameterDirectionIn);
                // add a new template row to the end of the list.
                this.parametersGrid.Rows.Add(this.ParameterTemplateRowName, null, null);
                RefreshParameterOperationButtons();
            }

            DataGridViewCell currentCell = this.parametersGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (typeof(TypeChooserCellItem).Equals(currentCell.Value))
            {
                // need this check beacuse one of the ways of choosing browse types( keyboard selection from dropdown)
                // overwrites the value of the cell with BrowseType.. again after setting the type, thus bringing up the type browser, 
                // for the second time here. 
                if (!typeChooserCellItem.TypeChosen)
                {
                    BrowseAndSelectType(currentCell);
                }
                else
                {
                    currentCell.Value = typeChooserCellItem.ChosenType;
                }

            }
            if (e.ColumnIndex.Equals(this.typeColumn.Index))
            {
                typeChooserCellItem.Reset();

            }
            UpdateOperationParameters();
        }

        void permissionNameTextChanged(object sender, EventArgs e)
        {
            WorkflowServiceOperationListItem workflowOperationListItem = this.Item as WorkflowServiceOperationListItem;
            if (workflowOperationListItem != null)
            {
                workflowOperationListItem.Operation.PrincipalPermissionName = this.permissionNameTextBox.Text;
            }
            else
            {
                TypedServiceOperationListItem reflectedOperationListItem = this.Item as TypedServiceOperationListItem;
                if (reflectedOperationListItem != null)
                {
                    reflectedOperationListItem.Operation.PrincipalPermissionName = this.permissionNameTextBox.Text;
                }
            }
            OnOperationPropertiesChanged();
        }

        void permissionRoleTextChanged(object sender, EventArgs e)
        {
            WorkflowServiceOperationListItem workflowOperationListItem = this.Item as WorkflowServiceOperationListItem;
            if (workflowOperationListItem != null)
            {
                workflowOperationListItem.Operation.PrincipalPermissionRole = this.permissionRoleTextBox.Text;
            }
            else
            {
                TypedServiceOperationListItem reflectedOperationListItem = this.Item as TypedServiceOperationListItem;
                if (reflectedOperationListItem != null)
                {
                    reflectedOperationListItem.Operation.PrincipalPermissionRole = this.permissionRoleTextBox.Text;
                }
            }
            OnOperationPropertiesChanged();
        }

        private void PopulateParametersGrid(OperationInfoBase operationInfoBase)
        {
            if (operationInfoBase != null)
            {
                string paramName;
                string direction;
                Type paramType;
                Type returnType = typeof(void);

                foreach (OperationParameterInfo paramInfo in operationInfoBase.GetParameters(this.ServiceProvider))
                {
                    paramType = paramInfo.ParameterType;
                    paramName = paramInfo.Name;
                    if (paramInfo.Name.Equals(SR2.GetString(SR2.ReturnValueString)))
                    {
                        returnType = paramType;
                        continue;
                    }
                    else
                    {
                        if (paramInfo.IsOut)
                        {
                            if (paramInfo.IsIn)
                            {
                                direction = SR2.GetString(SR2.ParameterDirectionRef);
                            }
                            else
                            {
                                direction = SR2.GetString(SR2.ParameterDirectionOut);
                            }
                        }
                        else if (paramType.IsByRef)
                        {
                            direction = SR2.GetString(SR2.ParameterDirectionRef);
                        }
                        else
                        {
                            // If neither IsOut nor IsByRef is true we assume it is an In parameter.
                            direction = SR2.GetString(SR2.ParameterDirectionIn);
                        }
                    }
                    //Add this type to list of typeColumn Items
                    AddToTypeList(paramType);
                    this.parametersGrid.Rows.Add(new object[] { paramName, paramType, direction });
                }

                // add the retval as the first parameter
                AddToTypeList(returnType);
                this.parametersGrid.Rows.Insert(0, new object[] { SR2.GetString(SR2.ReturnValueString), returnType, SR2.GetString(SR2.ParameterDirectionOut) });
                DataGridViewRow returnValueRow = this.parametersGrid.Rows[0];
                returnValueRow.Cells[this.nameColumn.Index].ReadOnly = true;
                returnValueRow.Cells[this.directionColumn.Index].ReadOnly = true;
            }
        }

        void protectionLevelComboBoxSelectedValueChanged(object sender, EventArgs e)
        {
            WorkflowServiceOperationListItem item = (WorkflowServiceOperationListItem) this.Item;
            if (this.protectionLevelComboBox.SelectedItem.Equals(SR2.GetString(SR2.UseRuntimeDefaults)))
            {
                item.Operation.ResetProtectionLevel();
            }
            else
            {
                item.Operation.ProtectionLevel = (ProtectionLevel) this.protectionLevelComboBox.SelectedItem;
            }
            OnOperationPropertiesChanged();
        }

        private void RefreshParameterOperationButtons()
        {
            if (!isEditable)
            {
                return;
            }
            DataGridViewRow currentRow = this.parametersGrid.CurrentRow;
            // disable move up, move down, remove
            moveParameterDownButton.Enabled = false;
            moveParameterUpButton.Enabled = false;
            RemoveParameterButton.Enabled = false;
            if (currentRow == null)
            {
                return;
            }
            int currentRowIndex = currentRow.Index;
            // dont enable any operation on the template row or the retval row
            if (currentRowIndex != (this.parametersGrid.Rows.Count - 1) && (currentRowIndex != 0))
            {
                RemoveParameterButton.Enabled = true;
                if (currentRowIndex > 1)
                {
                    moveParameterUpButton.Enabled = true;
                }
                if (currentRowIndex < (this.parametersGrid.Rows.Count - 2))
                {
                    moveParameterDownButton.Enabled = true;
                }
            }
        }

        private void RemoveParameterButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow currentRow = this.parametersGrid.CurrentRow;
            Fx.Assert((currentRow != null), "current row cannot be null");
            int currentRowIndex = currentRow.Index;
            Fx.Assert(currentRowIndex != (this.parametersGrid.Rows.Count - 1), "cant delete the template row");

            this.parametersGrid.Rows.Remove(currentRow);
            UpdateOperationParameters();
            RefreshParameterOperationButtons();
        }

        void SetEditability(bool editable)
        {
            this.isEditable = editable;
            this.addParameterButton.Enabled = editable;
            this.RemoveParameterButton.Enabled = editable;
            this.moveParameterDownButton.Enabled = editable;
            this.moveParameterUpButton.Enabled = editable;
            this.oneWayCheckBox.Enabled = editable;
            this.operationNameTextBox.Enabled = true;
            this.operationNameTextBox.ReadOnly = !editable;
            this.parametersGrid.ReadOnly = !editable;
            this.protectionLevelComboBox.Enabled = editable;
        }

        private void SetupColumns()
        {
            this.parametersGrid.Columns.Clear();

            nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.HeaderText = SR2.GetString(SR2.ParameterColumnHeaderName);
            nameColumn.Name = "Name";
            nameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.parametersGrid.Columns.Add(nameColumn);

            typeColumn = new DataGridViewComboBoxColumn();
            typeColumn.HeaderText = SR2.GetString(SR2.ParameterColumnHeaderType);
            typeColumn.Name = "Type";
            typeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            typeColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            typeColumn.AutoComplete = true;
            typeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            typeChooserCellItem = new TypeChooserCellItem();
            typeColumn.Items.Add(typeChooserCellItem);
            AddPrimitiveTypesToTypesList(); typeColumn.ValueMember = "Type";
            typeColumn.DisplayMember = "DisplayString";
            this.parametersGrid.Columns.Add(typeColumn);

            directionColumn = new DataGridViewComboBoxColumn();
            directionColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            directionColumn.AutoComplete = true;
            directionColumn.Items.Add(SR2.GetString(SR2.ParameterDirectionIn));
            directionColumn.Items.Add(SR2.GetString(SR2.ParameterDirectionOut));
            directionColumn.Items.Add(SR2.GetString(SR2.ParameterDirectionRef));
            directionColumn.HeaderText = SR2.GetString(SR2.ParameterColumnHeaderDirection);
            directionColumn.Name = "Direction";
            directionColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.parametersGrid.Columns.Add(directionColumn);
            this.operationsToolStrip.BackColor = this.parametersTabPage.BackColor;
        }

        bool TrySelectType(TypeBrowserDialog typeBrowserDialog, DataGridViewCell cell)
        {
            typeBrowserDialog.ShowDialog();
            if (typeBrowserDialog.SelectedType != null)
            {
                if (!ParameterTypeFilterProvider.IsValidType(typeBrowserDialog.SelectedType))
                {
                    DesignerHelpers.ShowMessage(this.ServiceProvider, SR2.GetString(SR2.InvalidParameterType,
                        typeBrowserDialog.SelectedType), DR.GetString(DR.WorkflowDesignerTitle), MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return false;
                }
                AddToTypeList(typeBrowserDialog.SelectedType);
            }
            typeChooserCellItem.ChosenType = typeBrowserDialog.SelectedType;
            cell.Value = typeBrowserDialog.SelectedType;

            return true;
        }

        private void UpdateImplementingActivities()
        {
            ServiceOperationListItem operationListItem = this.Item as ServiceOperationListItem;
            WorkflowServiceOperationListItem workflowOperationListItem = this.Item as WorkflowServiceOperationListItem;
            TypedServiceOperationListItem reflectedOperationListItem = this.Item as TypedServiceOperationListItem;
            OperationInfoBase operation = null;
            if (workflowOperationListItem != null)
            {
                operation = workflowOperationListItem.Operation;
            }
            else if (reflectedOperationListItem != null)
            {
                operation = reflectedOperationListItem.Operation;
            }
            Fx.Assert(operation != null, "operation should not be null at this point");
            // workflow operations list item will complain if some operationInfo objects have different signature than others
            // for the same operation name. This will happen when we are updating the activities, since they already have  a operation
            // object for that operation name, but with a different signature. so make them all point to null first ,then update.
            if (workflowOperationListItem != null)
            {
                foreach (Activity activity in operationListItem.ImplementingActivities)
                {
                    PropertyDescriptorUtils.SetPropertyValue(this.ServiceProvider, ServiceOperationHelpers.GetServiceOperationInfoPropertyDescriptor(activity), activity, null);
                }
            }
            foreach (Activity activity in operationListItem.ImplementingActivities)
            {
                PropertyDescriptorUtils.SetPropertyValue(this.ServiceProvider, ServiceOperationHelpers.GetServiceOperationInfoPropertyDescriptor(activity), activity, operation.Clone());
            }
        }

        void UpdateOperationParameters()
        {
            WorkflowServiceOperationListItem workflowOperationListItem = this.Item as WorkflowServiceOperationListItem;
            Fx.Assert(workflowOperationListItem != null, "UpdateOperation should only be called on workflowOperations");
            workflowOperationListItem.Operation.Parameters.Clear();
            int paramPosition = 0;
            foreach (DataGridViewRow row in this.parametersGrid.Rows)
            {
                string name = row.Cells[this.nameColumn.Index].Value as string;
                object typeCell = row.Cells[this.typeColumn.Index].Value;
                string direction = row.Cells[this.directionColumn.Index].Value as string;
                if (string.IsNullOrEmpty(name) || name.Equals(this.ParameterTemplateRowName))
                {
                    continue;
                }
                if (typeCell == null)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(direction))
                {
                    continue;
                }
                OperationParameterInfo operationParameterInfo = new OperationParameterInfo(name);
                operationParameterInfo.ParameterType = typeCell as Type;
                if (direction.Equals(SR2.GetString(SR2.ParameterDirectionIn)))
                {
                    operationParameterInfo.Attributes |= ParameterAttributes.In;
                }
                if (direction.Equals(SR2.GetString(SR2.ParameterDirectionOut)))
                {
                    operationParameterInfo.Attributes |= ParameterAttributes.Out;
                }
                if (direction.Equals(SR2.GetString(SR2.ParameterDirectionRef)))
                {
                    operationParameterInfo.Attributes |= ParameterAttributes.In;
                    operationParameterInfo.Attributes |= ParameterAttributes.Out;

                }
                if (name.Equals(SR2.GetString(SR2.ReturnValueString)))
                {
                    operationParameterInfo.Attributes |= ParameterAttributes.Retval;
                    operationParameterInfo.Position = -1;
                }
                else
                {
                    operationParameterInfo.Position = paramPosition;
                    paramPosition++;
                }
                workflowOperationListItem.Operation.Parameters.Add(operationParameterInfo);
            }
            UpdateImplementingActivities();
        }

        private bool ValidateParamaterName(string parameterName, DataGridViewCellValidatingEventArgs e, out string errorString)
        {
            errorString = string.Empty;

            if (string.IsNullOrEmpty(parameterName))
            {
                errorString = SR2.GetString(SR2.ParameterNameCannotBeEmpty);
                return false;
            }

            foreach (DataGridViewRow row in this.parametersGrid.Rows)
            {
                if (row.Index == e.RowIndex)
                {
                    continue;
                }

                if (parameterName.Equals(row.Cells[this.nameColumn.Index].Value.ToString()))
                {
                    errorString = SR2.GetString(SR2.DuplicateOfExistingParameter);
                    return false;
                }
            }
            if (parameterName.Equals(this.ParameterTemplateRowName) && (e.RowIndex == this.parametersGrid.Rows.Count - 1))
            {
                return true;
            }
            if (parameterName.Equals(SR2.GetString(SR2.ReturnValueString)))
            {
                return true;
            }
            if (!IsValidIdentifier(parameterName))
            {
                errorString = SR2.GetString(SR2.ParameterNameIsInvalid);
                return false;
            }


            return true;
        }

        class ParameterTypeFilterProvider : ITypeFilterProvider
        {
            string ITypeFilterProvider.FilterDescription
            {
                get { return SR2.GetString(SR2.ChooseAParameterTypeFromBelow); }
            }

            // This is a helper method called after a type has been selected to determine whether the type is valid 
            // for use as a parameter type.  This check it too expensive to perform during filtering.
            public static bool IsValidType(Type type)
            {
                bool result = true;

                if (!IsExemptType(type))
                {
                    try
                    {
                        XsdDataContractExporter exporter = new XsdDataContractExporter();
                        if (!exporter.CanExport(type))
                        {
                            result = false;
                        }
                    }
                    catch (InvalidDataContractException exception)
                    {
                        DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                        result = false;
                    }
                    catch (NotImplementedException exception)
                    {
                        // This occurs when a design-time type is involved (for example, as a type parameter), in
                        // which case we don't want to exclude the type from use as parameter type.
                        DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }

                return result;
            }

            bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
            {
                // We don't perform any actual filtering here because the XsdDataContractExporter validation is too 
                // expensive to do on the full list of types in all referenced assemblies.
                return true;
            }

            // Exempt types are those not subject to the XsdDataContractExporter validation.  Design-time types are 
            // exempt because the XsdDataContractExporter does not support them.  The others are exempt because they
            // are serialized by mechanisms other than DataContractSerializer.
            static bool IsExemptType(Type type)
            {
                return (type is DesignTimeType) ||
                    type.IsDefined(typeof(MessageContractAttribute), true) ||
                    type.Equals(typeof(System.ServiceModel.Channels.Message)) ||
                    type.Equals(typeof(System.IO.Stream));
            }
        }

        class TypeCellItem
        {
            private Type type;

            public TypeCellItem(Type type)
            {
                this.type = type;
            }

            public virtual string DisplayString
            {
                get
                {
                    return ServiceOperationDetailViewControl.GetTypeSignature(type);
                }
            }

            public virtual Type Type
            {
                get { return type; }
                set { type = value; }
            }
        }

        class TypeChooserCellItem : TypeCellItem
        {
            private Type chosenType = null;
            private bool typeChosen = false;


            public TypeChooserCellItem()
                : base(typeof(TypeChooserCellItem))
            {

            }

            public Type ChosenType
            {
                get { return chosenType; }
                set
                {
                    if (value != null)
                    {
                        typeChosen = true;
                    }
                    chosenType = value;
                }
            }
            public override string DisplayString
            {
                get
                {
                    return SR2.GetString(SR2.BrowseType);
                }
            }

            public bool TypeChosen
            {
                get { return typeChosen; }
                set { typeChosen = value; }
            }



            internal void Reset()
            {
                this.ChosenType = null;
                this.TypeChosen = false;
            }
        }

    }
}
