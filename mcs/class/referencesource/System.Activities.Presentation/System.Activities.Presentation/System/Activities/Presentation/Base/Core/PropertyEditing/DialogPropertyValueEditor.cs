namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows;

    using System.Runtime;

    /// <summary>
    /// Container for any and all dialog-editing logic for PropertyEntries.  This class can hold
    /// either a DataTemplate for a dialog editor or custom logic that will be called when
    /// the dialog is invoked.
    /// </summary>
    public class DialogPropertyValueEditor : PropertyValueEditor {

        private DataTemplate _dialogEditorTemplate;

        /// <summary>
        /// Creates a DialogPropertyValueEditor
        /// </summary>
        public DialogPropertyValueEditor()
            : this(null, null) { }

        /// <summary>
        /// Creates a DialogPropertyValueEditor
        /// </summary>
        /// <param name="dialogEditorTemplate">A DataTemplate that is hosted in a host specific dialog chrome
        /// and has its DataContext set to the PropertyValue that corresponds to the property being edited.</param>
        /// <param name="inlineEditorTemplate">A DataTemplate that is used for the inline editor UI.  If used,
        /// its DataContext will be set to the PropertyValue that corresponds to the property being edited.</param>
        public DialogPropertyValueEditor(DataTemplate dialogEditorTemplate, DataTemplate inlineEditorTemplate) 
            : base(inlineEditorTemplate) {
            _dialogEditorTemplate = dialogEditorTemplate;
        }

        /// <summary>
        /// Gets or sets the DataTemplate that is hosted by a host specific dialog and has its DataContext set to
        /// a PropertyValue.  If this property returns null, ShowDialog() will be called instead.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public DataTemplate DialogEditorTemplate
        {
            get { return _dialogEditorTemplate; }
            set { _dialogEditorTemplate = value; }
        }

        /// <summary>
        /// This method is called when the DialogEditorTemplate is null and a dialog has been invoked by the user.
        /// Overriding this method allows you to implement any custom dialog logic, such invoking existing
        /// system dialogs.
        /// </summary>
        /// <param name="propertyValue">The PropertyValue for the property being edited</param>
        /// <param name="commandSource">The IInputElement that can be used as a source for execution of
        /// PropertyValueEditorCommands.  Since these commands get handled by the host and since custom
        /// dialogs are not going to be part of that visual tree, the commandSource exposes an IInputElement
        /// that is part of the host's visual tree and which, therefore, may be used to execute property
        /// editing commands in such a way that they will be handled correctly.</param>
        public virtual void ShowDialog(PropertyValue propertyValue, IInputElement commandSource) {
        }

        internal override DataTemplate GetPropertyValueEditor(PropertyContainerEditMode mode) {
            return base.GetPropertyValueEditor(mode) ??
                (mode == PropertyContainerEditMode.Dialog ? _dialogEditorTemplate : (DataTemplate)null);
        }
    }
}
