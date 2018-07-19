//------------------------------------------------------------------------------
// <copyright file="ObjectListCommandsPage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.Drawing;
    using System.Web.UI.MobileControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
//    using System.Web.UI.Design.Util;

    using System.Web.UI.Design.MobileControls.Util;

    using ObjectList = System.Web.UI.MobileControls.ObjectList;
    using Label      = System.Windows.Forms.Label;
    using TextBox    = System.Windows.Forms.TextBox;

    /// <summary>
    ///   The Commands page for the ObjectList control.
    /// </summary>
    /// <internalonly/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class ObjectListCommandsPage : ListComponentEditorPage
    {
        private ComboBox _cmbDefaultCommand;
        private TextBox _txtText = null;
        private ObjectList _objectList = null;

        public ObjectListCommandsPage()
        {
            Y = 24;
            CaseSensitive = false;
            TreeViewTitle           = SR.GetString(SR.ObjectListCommandsPage_CommandNameCaption);
            AddButtonTitle          = SR.GetString(SR.ObjectListCommandsPage_NewCommandBtnCaption);
            DefaultName             = SR.GetString(SR.ObjectListCommandsPage_DefaultCommandName);
            MessageTitle            = SR.GetString(SR.ObjectListCommandsPage_ErrorMessageTitle);
            EmptyNameMessage        = SR.GetString(SR.ObjectListCommandsPage_EmptyNameError);
            // DuplicateNameMessage = SR.GetString(SR.ObjectListCommandsPage_DuplicateNameError);
            // InvalidNameMessage   = SR.GetString(SR.ObjectListCommandsPage_InvalidName);  // DCR 4240
        }

        protected override String HelpKeyword 
        {
            get 
            {
                return "net.Mobile.ObjectListProperties.Commands";
            }
        }

        protected override void InitForm()
        {
            base.InitForm();

            this._objectList = (ObjectList)Component;
            this.CommitOnDeactivate = true;
            this.Icon = new Icon(
                typeof(System.Web.UI.Design.MobileControls.MobileControlDesigner),
                "Commands.ico"
            );
            this.Size = new Size(402, 300);
            this.Text = SR.GetString(SR.ObjectListCommandsPage_Title);

            GroupLabel grplblCommandList = new GroupLabel();
            grplblCommandList.SetBounds(4, 4, 392, LabelHeight);
            grplblCommandList.Text = SR.GetString(SR.ObjectListCommandsPage_CommandListGroupLabel);
            grplblCommandList.TabIndex = 0;
            grplblCommandList.TabStop = false;

            TreeList.TabIndex = 1;

            Label lblText = new Label();
            lblText.SetBounds(X, Y, ControlWidth, LabelHeight);
            lblText.Text = SR.GetString(SR.ObjectListCommandsPage_TextCaption);
            lblText.TabStop = false;
            lblText.TabIndex = TabIndex;

            _txtText = new TextBox();
            Y += LabelHeight;
            _txtText.SetBounds(X, Y, ControlWidth, CmbHeight);
            _txtText.TextChanged += new EventHandler(this.OnPropertyChanged);
            _txtText.TabIndex = TabIndex + 1;

            GroupLabel grplblData = new GroupLabel();
            grplblData.SetBounds(4, 238, 392, LabelHeight);
            grplblData.Text = SR.GetString(SR.ObjectListCommandsPage_DataGroupLabel);
            grplblData.TabIndex = TabIndex + 2;
            grplblData.TabStop = false;

            Label lblDefaultCommand = new Label();
            lblDefaultCommand.SetBounds(8, 260, 182, LabelHeight);
            lblDefaultCommand.Text = SR.GetString(SR.ObjectListCommandsPage_DefaultCommandCaption);
            lblDefaultCommand.TabStop = false;
            lblDefaultCommand.TabIndex = TabIndex + 3;

            _cmbDefaultCommand = new ComboBox();
            _cmbDefaultCommand.SetBounds(8, 276, 182, 64);
            _cmbDefaultCommand.DropDownStyle = ComboBoxStyle.DropDown;
            _cmbDefaultCommand.Sorted = true;
            _cmbDefaultCommand.TabIndex = TabIndex + 4;
            _cmbDefaultCommand.SelectedIndexChanged += new EventHandler(this.OnSetPageDirty);
            _cmbDefaultCommand.TextChanged += new EventHandler(this.OnSetPageDirty);

            this.Controls.AddRange(new Control[] 
                                    {
                                        grplblCommandList,
                                        lblText,
                                        _txtText,
                                        grplblData,
                                        lblDefaultCommand,
                                        _cmbDefaultCommand
                                    });
        }

        protected override void InitPage() 
        {
            base.InitPage();

            _cmbDefaultCommand.Text = _objectList.DefaultCommand;
            _txtText.Text = String.Empty;
        }

        protected override void LoadItems()
        {
            using (new LoadingModeResource(this))
            {
                foreach (ObjectListCommand command in _objectList.Commands)
                {
                    CommandTreeNode newNode = new CommandTreeNode(command.Name, command);
                    TreeList.TvList.Nodes.Add(newNode);
                }
            }
            LoadDefaultCommands();
        }

        protected override void LoadItemProperties() 
        {
            using (new LoadingModeResource(this))
            {
                if (CurrentNode != null)
                {
                    CommandTreeNode currentCommandNode = (CommandTreeNode)CurrentNode;
                    _txtText.Text = currentCommandNode.Text;
                }
                else
                {
                    _txtText.Text = String.Empty;
                }
            }
        }

        private void LoadDefaultCommands()
        {
            _cmbDefaultCommand.Items.Clear();
            foreach (CommandTreeNode commandNode in TreeList.TvList.Nodes)
            {
                _cmbDefaultCommand.Items.Add(commandNode.Name);
            }
        }

        private void OnSetPageDirty(Object source, EventArgs e) 
        {
            if (IsLoading())
            {
                return;
            }
            SetDirty();
        }

        protected override void OnClickAddButton(Object source, EventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            CommandTreeNode newNode = new CommandTreeNode(GetNewName());
            TreeList.TvList.Nodes.Add(newNode);

            TreeList.TvList.SelectedNode = newNode;
            CurrentNode = newNode;
            newNode.Dirty = true;
            newNode.BeginEdit();

            LoadItemProperties();
            LoadDefaultCommands();
            SetDirty();
        }

        protected override void OnClickRemoveButton(Object source, EventArgs e)
        {
            base.OnClickRemoveButton(source, e);
            LoadDefaultCommands();
        }

        protected override void OnNodeRenamed()
        {
            LoadDefaultCommands();
        }

        protected override void OnPropertyChanged(Object source, EventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            // This means there are no fields yet. Do nothing
            if (CurrentNode == null)
            {
                return;
            }

            ((CommandTreeNode)CurrentNode).Text = _txtText.Text;

            SetDirty();
            CurrentNode.Dirty = true;
        }

        protected override void SaveComponent()
        {
            // Delegate to base implementation first!
            // This will properly close ListTreeNode editing mode.
            base.SaveComponent();

            _objectList.DefaultCommand = _cmbDefaultCommand.Text;

            _objectList.Commands.Clear();

            foreach (CommandTreeNode commandNode in TreeList.TvList.Nodes)
            {
                if (commandNode.Dirty)
                {
                    commandNode.RuntimeCommand.Text = commandNode.Text;
                    commandNode.RuntimeCommand.Name = commandNode.Name;
                }

                _objectList.Commands.AddAt(-1, commandNode.RuntimeCommand);
            }

            TypeDescriptor.Refresh(_objectList);
        }

        protected override void UpdateControlsEnabling()
        {
            TreeList.TvList.Enabled = 
                _txtText.Enabled = (TreeList.TvList.SelectedNode != null);
        }

        /// <summary>
        ///    Internal object used to store all command properties
        /// </summary>
        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        private class CommandTreeNode : ListTreeNode
        {
            private ObjectListCommand   _runtimeCommand;
            private String              _text;

            /// <summary>
            /// </summary>
            internal CommandTreeNode(String name) : this(name, new ObjectListCommand())
            {
            }

            /// <summary>
            /// </summary>
            internal CommandTreeNode(String name, ObjectListCommand runtimeCommand) : base(name)
            {
                Debug.Assert(name != null, "invalid name for ObjectListCommand");
                Debug.Assert(runtimeCommand != null, "null ObjectListCommand");

                this._runtimeCommand = runtimeCommand;
                LoadAttributes();
            }

            internal void LoadAttributes()
            {
                this._text = _runtimeCommand.Text;
            }

            internal ObjectListCommand RuntimeCommand
            {
                get
                {
                    return _runtimeCommand;
                }
            }

            internal new String Text
            {
                get
                {
                    return _text;
                }

                set
                {
                    _text = value;
                }
            }
        }
    }
}
