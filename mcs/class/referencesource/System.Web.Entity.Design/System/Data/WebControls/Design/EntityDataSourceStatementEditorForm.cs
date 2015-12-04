//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceStatementEditorForm.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//
// Enables a user to edit CommandText, OrderBy, Select, and
// Where properties and parameters
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Web.UI.Design.WebControls.Util;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Web.UI.Design.WebControls;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls
{
    internal class EntityDataSourceStatementEditorForm : DesignerForm
    {
        private System.Windows.Forms.Panel _checkBoxPanel;
        private System.Windows.Forms.CheckBox _autoGenerateCheckBox;
        private System.Windows.Forms.Panel _statementPanel;
        private System.Windows.Forms.Label _statementLabel;
        private System.Windows.Forms.TextBox _statementTextBox;
        private ParameterEditorUserControl _parameterEditorUserControl;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;

        private System.Web.UI.Control _entityDataSource;
        private ParameterCollection _parameters;

        private string _cachedStatementText;
        private readonly string _helpTopic;

        public EntityDataSourceStatementEditorForm(System.Web.UI.Control entityDataSource, IServiceProvider serviceProvider,
            bool hasAutoGen, bool isAutoGen, string propertyName, string statementLabelText, string statementAccessibleName,
            string helpTopic, string statement, ParameterCollection parameters)
            : base(serviceProvider)
        {

            _entityDataSource = entityDataSource;
            InitializeComponent();
            InitializeUI(propertyName, statementLabelText, statementAccessibleName);
            InitializeTabIndexes();
            InitializeAnchors();

            _helpTopic = helpTopic;

            if (!hasAutoGen)
            {
                HideCheckBox();
            }

            _parameters = parameters;

            _autoGenerateCheckBox.Checked = isAutoGen;
            _statementPanel.Enabled = !isAutoGen;

            _statementTextBox.Text = statement;
            _statementTextBox.Select(0, 0);

            List<Parameter> paramList = new List<Parameter>();
            foreach (Parameter p in parameters)
            {
                paramList.Add(p);
            }
            _parameterEditorUserControl.AddParameters(paramList.ToArray());

            _cachedStatementText = null;
        }

        public bool AutoGen
        {
            get
            {
                return _autoGenerateCheckBox.Checked;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return _helpTopic;
            }
        }

        public ParameterCollection Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public string Statement
        {
            get
            {
                return _statementTextBox.Text;
            }
        }

        private void HideCheckBox()
        {
            _autoGenerateCheckBox.Checked = false;
            _checkBoxPanel.Visible = false;

            int moveUp = _statementPanel.Location.Y - _checkBoxPanel.Location.Y;

            Point loc = _statementPanel.Location;
            loc.Y -= moveUp;
            _statementPanel.Location = loc;

            loc = _parameterEditorUserControl.Location;
            loc.Y -= moveUp;
            _parameterEditorUserControl.Location = loc;

            Size size = _parameterEditorUserControl.Size;
            size.Height += moveUp;
            _parameterEditorUserControl.Size = size;

            size = this.MinimumSize;
            size.Height -= moveUp;
            this.MinimumSize = size;
            this.Size = size;
        }

        private void InitializeAnchors()
        {
            _checkBoxPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _autoGenerateCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            _statementPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _statementLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            _statementTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            
            _parameterEditorUserControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            _okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._statementLabel = new System.Windows.Forms.Label();
            this._statementTextBox = new System.Windows.Forms.TextBox();
            this._autoGenerateCheckBox = new System.Windows.Forms.CheckBox();
            this._parameterEditorUserControl = (ParameterEditorUserControl)Activator.CreateInstance(typeof(ParameterEditorUserControl), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { ServiceProvider, _entityDataSource }, null);
            this._checkBoxPanel = new System.Windows.Forms.Panel();
            this._statementPanel = new System.Windows.Forms.Panel();
            this._checkBoxPanel.SuspendLayout();
            this._statementPanel.SuspendLayout();
            this.SuspendLayout();
            this.InitializeSizes();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Name = "_okButton";            
            this._okButton.Click += new System.EventHandler(this.OnOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Name = "_cancelButton";            
            this._cancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
            // 
            // _commandLabel
            // 
            this._statementLabel.Name = "_commandLabel";            
            // 
            // _statementTextBox
            // 
            this._statementTextBox.AcceptsReturn = true;
            this._statementTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._statementTextBox.Multiline = true;
            this._statementTextBox.Name = "_statementTextBox";
            this._statementTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;            
            // 
            // _autoGenerateCheckBox
            // 
            this._autoGenerateCheckBox.CheckAlign = ContentAlignment.TopLeft;
            this._autoGenerateCheckBox.TextAlign = ContentAlignment.TopLeft;
            this._autoGenerateCheckBox.Name = "_autoGenerateCheckBox";
            this._autoGenerateCheckBox.UseVisualStyleBackColor = true;
            this._autoGenerateCheckBox.CheckedChanged += new EventHandler(OnAutoGenerateCheckBoxCheckedChanged);
            // 
            // _checkBoxPanel
            // 
            this._checkBoxPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._checkBoxPanel.Controls.Add(this._autoGenerateCheckBox);
            this._checkBoxPanel.Name = "_radioPanel";            
            // 
            // _statementPanel
            // 
            this._statementPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._statementPanel.Controls.Add(this._statementLabel);
            this._statementPanel.Controls.Add(this._statementTextBox);
            this._statementPanel.Name = "_statementPanel";            
            // 
            // _parameterEditorUserControl
            // 
            this._parameterEditorUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._parameterEditorUserControl.Name = "_parameterEditorUserControl";            
            // 
            // EntityDataSourceStatementEditorForm
            // 
            this.AcceptButton = this._okButton;
            this.CancelButton = this._cancelButton;
            this.Controls.Add(this._statementPanel);
            this.Controls.Add(this._checkBoxPanel);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._parameterEditorUserControl);
            this.Name = "EntityDataSourceStatementEditorForm";
            this._checkBoxPanel.ResumeLayout(false);
            this._checkBoxPanel.PerformLayout();
            this._statementPanel.ResumeLayout(false);
            this._statementPanel.PerformLayout();
            this.ResumeLayout(false);

            InitializeForm();
        }

        #endregion

        private void InitializeSizes()
        {
            int top = 0;

            _checkBoxPanel.Location = new Point(12, 12);
            _checkBoxPanel.Size = new Size(456, 32);
            _autoGenerateCheckBox.Location = new Point(0, 0);
            _autoGenerateCheckBox.Size = new Size(456, 30);
            top = _checkBoxPanel.Bottom;

            _statementPanel.Location = new Point(12, top + 4);
            _statementPanel.Size = new Size(456, 124);

            top = 0;
            _statementLabel.Location = new Point(0, 0);
            _statementLabel.Size = new Size(200, 16);
            top = _statementLabel.Bottom;

            _statementTextBox.Location = new Point(0, top + 3);
            _statementTextBox.Size = new Size(456, 78);
            top = _statementPanel.Bottom;

            _parameterEditorUserControl.Location = new Point(12, top + 5);
            _parameterEditorUserControl.Size = new Size(460, 216);
            top = _parameterEditorUserControl.Bottom;

            _okButton.Location = new Point(313, top + 6);
            _okButton.Size = new Size(75, 23);
            _cancelButton.Location = new Point(393, top + 6);
            _cancelButton.Size = new Size(75, 23);
            top = _cancelButton.Bottom;

            ClientSize = new Size(480, top + 12);
            MinimumSize = new Size(480 + 8, top + 12 + 27);
        }

        private void InitializeTabIndexes()
        {
            _checkBoxPanel.TabStop = false;
            _autoGenerateCheckBox.TabStop = true;

            _statementPanel.TabStop = false;
            _statementLabel.TabStop = false;
            _statementTextBox.TabStop = true;

            _parameterEditorUserControl.TabStop = true;

            _okButton.TabStop = true;
            _cancelButton.TabStop = true;

            int tabIndex = 0;

            _checkBoxPanel.TabIndex = tabIndex += 10;
            _autoGenerateCheckBox.TabIndex = tabIndex += 10;

            _statementPanel.TabIndex = tabIndex += 10;
            _statementLabel.TabIndex = tabIndex += 10;
            _statementTextBox.TabIndex = tabIndex += 10;

            _parameterEditorUserControl.TabIndex = tabIndex += 10;

            _okButton.TabIndex = tabIndex += 10;
            _cancelButton.TabIndex = tabIndex += 10;
        }

        private void InitializeUI(string propertyName, string labelText, string accessibleName)
        {
            this.Text = Strings.ExpressionEditor_Caption;
            this.AccessibleName = Strings.ExpressionEditor_Caption;
            _okButton.Text = Strings.OKButton;
            _okButton.AccessibleName = Strings.OKButtonAccessibleName;
            _cancelButton.Text = Strings.CancelButton;
            _cancelButton.AccessibleName = Strings.CancelButtonAccessibleName;
            _statementLabel.Text = labelText;
            _statementTextBox.AccessibleName = accessibleName;
            if (String.Equals(propertyName, "Where", StringComparison.OrdinalIgnoreCase))
            {
                _autoGenerateCheckBox.Text = Strings.ExpressionEditor_AutoGenerateWhereCheckBox;
                _autoGenerateCheckBox.AccessibleName = Strings.ExpressionEditor_AutoGenerateWhereCheckBoxAccessibleName;
            }
            else if (String.Equals(propertyName, "OrderBy", StringComparison.OrdinalIgnoreCase))
            {
                _autoGenerateCheckBox.Text = Strings.ExpressionEditor_AutoGenerateOrderByCheckBox;
                _autoGenerateCheckBox.AccessibleName = Strings.ExpressionEditor_AutoGenerateOrderByCheckBoxAccessibleName;
            }
        }

        private void OnAutoGenerateCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (_autoGenerateCheckBox.Checked)
            {
                _cachedStatementText = _statementTextBox.Text;
                _statementTextBox.Text = null;
            }
            else if (!String.IsNullOrEmpty(_cachedStatementText))
            {
                _statementTextBox.Text = _cachedStatementText;
            }
            _statementPanel.Enabled = !_autoGenerateCheckBox.Checked;
        }

        private void OnCancelButtonClick(System.Object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnOkButtonClick(System.Object sender, System.EventArgs e)
        {
            _parameters.Clear();
            Parameter[] paramList = _parameterEditorUserControl.GetParameters();
            foreach (Parameter p in paramList)
            {
                _parameters.Add(p);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

