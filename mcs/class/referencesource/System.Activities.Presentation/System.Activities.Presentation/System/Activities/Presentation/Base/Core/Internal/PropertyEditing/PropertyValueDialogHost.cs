//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;

    using System.Runtime;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Editors;
    using Microsoft.Win32;

    // <summary>
    // Helper class that is responsible for opening a [....] Form that hosts the WPF
    // PropertyValueDialogControl that ultimately hosts a DialogPropertyValueEditor.
    // Both PropertyInspector control as well as PropertyValueDialogControl use this class.
    // </summary>
    internal class PropertyValueDialogHost
    {

        private System.Windows.Forms.Form _dialogWindow;

        // Ctor is private because it is created automatically when the OpenDialogWindow fires. 
        private PropertyValueDialogHost()
        {
        }

        // <summary>
        // Helper that attaches the OpenDialogWindow command to a handler within this host
        // class.  The handler will automatically open and populate the dialog window when
        // the OpenDialogWindow command is fired.
        // </summary>
        // <param name="dialogRoot">UIElement that should handle the OpenDialogWindow command</param>
        public static void AttachOpenDialogHandlers(UIElement dialogRoot)
        {
            Fx.Assert(dialogRoot != null, "dialogRoot parameter is null");

            PropertyValueDialogHost dialogHost = new PropertyValueDialogHost();
            dialogRoot.CommandBindings.Add(new CommandBinding(PropertyContainer.OpenDialogWindow, dialogHost.OnOpenDialogWindow, dialogHost.OnCanOpenDialogWindow));
        }

        // <summary>
        // Called in response to OpenDialogWindow command firing. A dialog can be opened if
        // a dialog editor DataTemplate exists on the corresponding property being edited.
        // </summary>
        // <param name="sender"></param>
        // <param name="e"></param>
        public void OnCanOpenDialogWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            DataTemplate dialogEditorTemplate = GetDialogEditorTemplate(GetParentProperty(e.OriginalSource));
            if (dialogEditorTemplate == null)
            {
                return;
            }

            // We can show a dialog
            e.CanExecute = true;
        }

        // <summary>
        // Called in response to OpenDialogWindow command firing. The method opens a
        // [....] Form that contains an ElementHost that, in turn, contains
        // PropertyValueDialogControl.
        // </summary>
        // <param name="sender"></param>
        // <param name="e"></param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void OnOpenDialogWindow(object sender, ExecutedRoutedEventArgs e)
        {
            // Hook on to the system user-preference changed event.
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(OnUserPreferenceChanged);

            // Is there a DataTemplate and PropertyValue to display?
            PropertyEntry parentProperty = GetParentProperty(e.OriginalSource);
            DataTemplate dialogEditorTemplate = GetDialogEditorTemplate(parentProperty);
            if (dialogEditorTemplate == null)
            {
                return;
            }

            // Create and populate a new Form
            _dialogWindow = new Form();
            _dialogWindow.ShowInTaskbar = false;
            _dialogWindow.ShowIcon = false;
            _dialogWindow.MaximizeBox = false;
            _dialogWindow.MinimizeBox = false;
            _dialogWindow.HelpButton = false;
            _dialogWindow.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Auto;
            _dialogWindow.Width = 600;
            _dialogWindow.Height = 400;

            // We need to change the title based on the type of the property being edited.
            // e.g: For CollectionEditors we should say "CollectionEditor : <DisplayName>"
            // For everything else we should say "Property Editor : <DisplayName>"
            string title = System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_DialogValueEditorTitle;
            _dialogWindow.MinimumSize = new System.Drawing.Size(575, 400); // Magic min-size numbers from Dan
            _dialogWindow.Text = string.Format(
                CultureInfo.CurrentCulture,
                title,
                parentProperty.DisplayName);

            PropertyValueDialogControl dialogControl = new PropertyValueDialogControl(parentProperty, dialogEditorTemplate);
            dialogControl.CloseParentDialog += new EventHandler(OnCloseParentDialog);

            using (ElementHost elementHost = new ElementHost())
            {
                elementHost.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
                elementHost.Size = _dialogWindow.ClientSize;
                elementHost.Child = dialogControl;

                _dialogWindow.Controls.Add(elementHost);
                _dialogWindow.ShowDialog();
                dialogControl.OnParentDialogClosing();
            }
        }

        // <summary>
        // Event handler for system color changes. This will change the BackColor of the dialog
        // matching the current tool window background color for Visual Studio.
        // </summary>
        // <param name="sender"></param>
        // <param name="e"></param>

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // 
            //             if (PropertyInspectorFontAndColorDictionary.ColorService != null) {
            //                 System.Drawing.Color backColor = PropertyInspectorFontAndColorDictionary.ColorService.GetColor(Microsoft.VisualStudio.Shell.Interop.__VSSYSCOLOREX.VSCOLOR_TOOLWINDOW_BACKGROUND, 255);
            //                 _dialogWindow.BackColor = backColor;
            //                 _dialogWindow.Refresh();
            //                 _dialogWindow.Invalidate(true);
            //             }//
        }

        // Event handler invoked when the PropertyValueDialogControl indicates that the dialog should close
        private void OnCloseParentDialog(object sender, EventArgs e)
        {
            // Unhook on to the system user-preference changed event.
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(OnUserPreferenceChanged);

            if (_dialogWindow != null)
            {
                _dialogWindow.Close();
            }
        }

        private static DataTemplate GetDialogEditorTemplate(PropertyEntry property)
        {
            if (property == null)
            {
                return null;
            }

            // Does the current PropertyEntry have a dialog editor?
            DialogPropertyValueEditor dialogEditor = property.PropertyValueEditor as DialogPropertyValueEditor;
            if (dialogEditor == null)
            {
                return null;
            }

            return dialogEditor.DialogEditorTemplate;
        }

        private static PropertyEntry GetParentProperty(object showDialogCommandSource)
        {
            // Was the command invoked from a control that we expect?
            DependencyObject source = showDialogCommandSource as DependencyObject;
            if (source == null)
            {
                return null;
            }

            PropertyContainer container = PropertyContainer.GetOwningPropertyContainer(source);
            if (container == null)
            {
                return null;
            }

            // Does the current PropertyEntry have a dialog editor?
            return container.PropertyEntry;
        }
    }
}
