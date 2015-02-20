//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using System.Runtime;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Editors;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Resources;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors;

    // <summary>
    // Root WPF control used for DialogPropertyValueEditors.  It contains a place for the
    // editor and adds standard OK / Cancel buttons.  It's also responsible for handling
    // PropertyValueEditorCommands and opening and closing the root transaction that
    // responds to the OK / Cancel buttons.
    // </summary>
    internal partial class PropertyValueDialogControl 
    {

        private ModelEditingScope _rootTransaction;
        private PropertyValueEditorCommandHandler _defaultCommandHandler;

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="property">Property to display</param>
        // <param name="valueDialogTemplate">Template to use</param>
        public PropertyValueDialogControl(PropertyEntry property, DataTemplate valueDialogTemplate) 
        {
            Fx.Assert(property != null, "property parameter is null");
            Fx.Assert(valueDialogTemplate != null, "valueDialogTemplate parameter is null");

            ModelPropertyEntry modelPropertyValue = property as ModelPropertyEntry;
            if (modelPropertyValue != null) 
            {
                _rootTransaction = modelPropertyValue.FirstModelProperty.Value.BeginEdit();
            }

            InitializeComponent();

            // Make sure we use PI-specific resources within this control
            this.Resources.MergedDictionaries.Add(PropertyInspectorResources.GetResources());

            // Hook into an opening of nested dialogs.  PropertyInspector class takes care of this for us.
            // However, we are using Blend's collection editor which doesn't do the same thing, so
            // we need to reproduce that behavior manually.
            PropertyValueDialogHost.AttachOpenDialogHandlers(this);

            // Hook into the standard set of Commands
            _defaultCommandHandler = new PropertyValueEditorCommandHandler(this);


            _OKButton.Click += new RoutedEventHandler(OnOkButtonClicked);
            _cancelButton.Click += new RoutedEventHandler(OnCancelButtonClicked);
            _contentControl.Content = property.PropertyValue;
            _contentControl.ContentTemplate = valueDialogTemplate;

            //Handle the commit and cancel keys within the property inspector, that is hosted in the collection editor
            ValueEditorUtils.SetHandlesCommitKeys(this, true);

        }

        // Internal event we use for this control to signal that the uber-dialog (which is actually
        // a Form) should be closed.  This event is raised whenever the OK / Cancel buttons are
        // pressed
        internal event EventHandler CloseParentDialog;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        private void OnOkButtonClicked(object sender, RoutedEventArgs e) 
        {
            try 
            {
                CommitDanglingTransactions();
                CommitRootTransaction();
                SignalCloseParentDialog();
            }
            catch (Exception ex) 
            {
                ErrorReporting.ShowErrorMessage(ex.Message);
            }
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e) 
        {
            AbortDanglingTransactions();
            AbortRootTransaction();
            SignalCloseParentDialog();
        }


        private void CommitDanglingTransactions() 
        {
            _defaultCommandHandler.CommitOpenTransactions();
        }

        private void AbortDanglingTransactions() 
        {
            _defaultCommandHandler.AbortOpenTransactions();
        }

        private void SignalCloseParentDialog() 
        {
            if (CloseParentDialog != null)
            {
                CloseParentDialog(this, EventArgs.Empty);
            }
        }

        // Called when the parent dialog gets closed.  If this happened without us clicking on
        // the OK or Cancel button, the root transaction will still exist and we need to manually
        // abort it.
        public void OnParentDialogClosing() 
        {
            AbortRootTransaction();
        }

        private void CommitRootTransaction() 
        {
            if (_rootTransaction == null)
            {
                return;
            }

            _rootTransaction.Complete();
            _rootTransaction = null;
        }

        private void AbortRootTransaction() 
        {
            if (_rootTransaction == null)
            {
                return;
            }

            _rootTransaction.Revert();
            _rootTransaction = null;
        }
    }
}
