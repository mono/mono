//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;

    using System.Activities.Presentation;
    using System.Runtime;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Model;

    // <summary>
    // Container for standard PropertyValueEditorCommand handlers.  These handlers need to do their
    // work both for the PropertyInspector control as well as for any standard WPF dialog editor
    // launched from the PropertyInspector itself.  As such, the functionality is extracted here
    // to be shared in both cases.
    // </summary>
    internal class PropertyValueEditorCommandHandler : IDisposable 
    {

        private UIElement _commandHost;
        private List<CommandBinding> _activeCommandBindings = new List<CommandBinding>();
        private List<ModelEditingScope> _pendingTransactions = new List<ModelEditingScope>();

        public PropertyValueEditorCommandHandler(UIElement commandHost) 
        {
            Fx.Assert(commandHost != null, "commandHost parameter is null");
            _commandHost = commandHost;

            AddEditModeCommandBindings();
            AddTransactionCommandBindings();

            foreach (CommandBinding binding in _activeCommandBindings)
            {
                _commandHost.CommandBindings.Add(binding);
            }
        }

        // <summary>
        // Cleans up by removing all CommandBindings added by this class from the commandHost
        // </summary>
        public void Dispose() 
        {
            if (_commandHost != null && _activeCommandBindings.Count > 0) 
            {
                foreach (CommandBinding binding in _activeCommandBindings) 
                {
                    _commandHost.CommandBindings.Remove(binding);
                }
                _activeCommandBindings.Clear();
            }
        }

        // <summary>
        // Commits any pending and open transactions in the correct order
        // </summary>
        public void CommitOpenTransactions() 
        {
            for (int i = _pendingTransactions.Count - 1; i >= 0; i--) 
            {
                _pendingTransactions[i].Complete();
                _pendingTransactions.RemoveAt(i);
            }
        }

        // <summary>
        // Aborts any pending and open transactions in the correct order
        // </summary>
        public void AbortOpenTransactions() 
        {
            for (int i = _pendingTransactions.Count - 1; i >= 0; i--) 
            {
                _pendingTransactions[i].Revert();
                _pendingTransactions.RemoveAt(i);
            }
        }

        // ActiveEditMode Switch Handlers

        // Adds handlers for ShowInlineEditor, ShowExtendedPopupEditor, ShowExtendedPinnedEditor,
        // and ShowDialogEditor commands.  These handlers the ActiveEditMode of the closest PropertyContainer
        // that contains the UIElement that invoked it
        private void AddEditModeCommandBindings() 
        {
            _activeCommandBindings.Add(new CommandBinding(PropertyValueEditorCommands.ShowInlineEditor, OnShowInlineEditor));
            _activeCommandBindings.Add(new CommandBinding(PropertyValueEditorCommands.ShowExtendedPopupEditor, OnShowExtendedPopupEditor));
            _activeCommandBindings.Add(new CommandBinding(PropertyValueEditorCommands.ShowExtendedPinnedEditor, OnShowExtendedPinnedEditor));
            _activeCommandBindings.Add(new CommandBinding(PropertyValueEditorCommands.ShowDialogEditor, OnShowDialogEditor));
        }

        private void OnShowInlineEditor(object sender, ExecutedRoutedEventArgs e) 
        {
            SwitchActiveEditMode(e, PropertyContainerEditMode.Inline);
        }

        private void OnShowExtendedPopupEditor(object sender, ExecutedRoutedEventArgs e) 
        {
            SwitchActiveEditMode(e, PropertyContainerEditMode.ExtendedPopup);
        }

        private void OnShowExtendedPinnedEditor(object sender, ExecutedRoutedEventArgs e) 
        {
            SwitchActiveEditMode(e, PropertyContainerEditMode.ExtendedPinned);
        }

        private void OnShowDialogEditor(object sender, ExecutedRoutedEventArgs e) 
        {
            SwitchActiveEditMode(e, PropertyContainerEditMode.Dialog);
        }

        private static void SwitchActiveEditMode(ExecutedRoutedEventArgs e, PropertyContainerEditMode newMode) 
        {
            PropertyContainer container = GetContainerFromEventArgs(e);
            if (container == null)
            {
                return;
            }

            container.ActiveEditMode = newMode;
        }



        // Transaction Handlers

        // Adds command handlers for BeginTransaction, CommitTransaction, and AbortTransaction
        // commands.  These handlers open, commit, or abort a transaction
        private void AddTransactionCommandBindings() 
        {
            _activeCommandBindings.Add(new CommandBinding(PropertyValueEditorCommands.BeginTransaction, OnBeginTransaction));
            _activeCommandBindings.Add(new CommandBinding(PropertyValueEditorCommands.CommitTransaction, OnCommitTransaction));
            _activeCommandBindings.Add(new CommandBinding(PropertyValueEditorCommands.AbortTransaction, OnAbortTransaction));
        }

        private void OnBeginTransaction(object sender, ExecutedRoutedEventArgs e) 
        {
            ModelPropertyEntryBase property = GetContainedPropertyFromEventArgs(e);
            if (property == null)
            {
                return;
            }

            _pendingTransactions.Add(property.BeginEdit(e.Parameter as string));
        }

        private void OnCommitTransaction(object sender, ExecutedRoutedEventArgs e) 
        {
            if (_pendingTransactions.Count == 0)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_ErrorCommit_NoTransactionsOpened));
            }

            try 
            {
                _pendingTransactions[_pendingTransactions.Count - 1].Complete();
            }
            finally 
            {
                _pendingTransactions.RemoveAt(_pendingTransactions.Count - 1);
            }
        }

        private void OnAbortTransaction(object sender, ExecutedRoutedEventArgs e) 
        {
            if (_pendingTransactions.Count == 0)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_ErrorAbort_NoTransactionsOpened));
            }

            try 
            {
                _pendingTransactions[_pendingTransactions.Count - 1].Revert();
            }
            finally 
            {
                _pendingTransactions.RemoveAt(_pendingTransactions.Count - 1);
            }
        }



        // Static Helpers

        private static ModelPropertyEntryBase GetContainedPropertyFromEventArgs(ExecutedRoutedEventArgs e) 
        {
            PropertyContainer container = GetContainerFromEventArgs(e);
            if (container == null)
            {
                return null;
            }

            return container.PropertyEntry as ModelPropertyEntryBase;
        }

        private static PropertyContainer GetContainerFromEventArgs(ExecutedRoutedEventArgs e) 
        {
            DependencyObject source = e.OriginalSource as DependencyObject;
            if (source == null)
            {
                return null;
            }

            return PropertyContainer.GetOwningPropertyContainer(source);
        }


    }
}
