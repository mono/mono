namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Activities.Presentation.Internal.Properties;
    using System.Activities.Presentation;
    using System.Runtime;

    /// <summary>
    /// Container for any and all inline editor logic for properties.  This class can hold
    /// a single DataTemplates - one for Inline editor.
    /// </summary>
    public class PropertyValueEditor {

        private DataTemplate _inlineEditorTemplate;

        /// <summary>
        /// Creates a PropertyValueEditor
        /// </summary>
        public PropertyValueEditor() {
        }

        /// <summary>
        /// Creates a PropertyValueEditor
        /// </summary>
        /// <param name="inlineEditorTemplate">The DataTemplate that is used for an inline editor.  
        /// This DataTemplate has its DataContext set to a PropertyValue</param>
        public PropertyValueEditor(DataTemplate inlineEditorTemplate) {
            _inlineEditorTemplate = inlineEditorTemplate;
        }

        /// <summary>
        /// Gets or sets the InlineEditorTemplate -- the DataTemplate that is used for an inline editor.  
        /// This DataTemplate has its DataContext set to a PropertyValue
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public DataTemplate InlineEditorTemplate
        {
            get { return _inlineEditorTemplate; }
            set { _inlineEditorTemplate = value; }
        }

        internal virtual DataTemplate GetPropertyValueEditor(PropertyContainerEditMode mode) {
            return (mode == PropertyContainerEditMode.Inline) ? _inlineEditorTemplate : null;
        }

        /// <summary>
        /// Utility method that creates a new EditorAttribute for the specified
        /// PropertyValueEditor
        /// </summary>
        /// <param name="editor">PropertyValueEditor instance for which to create
        /// the new EditorAttribute</param>
        /// <returns>New EditorAttribute for the specified PropertyValueEditor</returns>
        public static EditorAttribute CreateEditorAttribute(PropertyValueEditor editor) {
            if (editor == null)
                throw FxTrace.Exception.ArgumentNull("editor");

            return CreateEditorAttribute(editor.GetType());
        }

        /// <summary>
        /// Utility method that creates a new EditorAttribute for the specified
        /// PropertyValueEditor type
        /// </summary>
        /// <param name="propertyValueEditorType">PropertyValueEditor type for which to create
        /// the new EditorAttribute</param>
        /// <returns>New EditorAttribute for the specified PropertyValueEditor type</returns>
        public static EditorAttribute CreateEditorAttribute(Type propertyValueEditorType) {
            if (propertyValueEditorType == null)
                throw FxTrace.Exception.ArgumentNull("propertyValueEditorType");

            if (!typeof(PropertyValueEditor).IsAssignableFrom(propertyValueEditorType))
                throw FxTrace.Exception.AsError(new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error_ArgIncorrectType,
                        "propertyValueEditorType",
                        typeof(PropertyValueEditor).Name)));

            return new EditorAttribute(propertyValueEditorType, typeof(PropertyValueEditor));
        }
    }
}
