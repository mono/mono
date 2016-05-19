namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.Text;
    using System.Windows;
    using System.Runtime;

    /// <summary>
    /// Container for any and all exended editor logic for properties.  This class can hold
    /// two DataTemplates - one for Inline editor and one for Extended editor.
    /// </summary>
    public class ExtendedPropertyValueEditor : PropertyValueEditor {

        private DataTemplate _extendedEditorTemplate;

        /// <summary>
        /// Creates an ExtendedPropertyValueEditor
        /// </summary>
        public ExtendedPropertyValueEditor()
            : this(null, null) { }

        /// <summary>
        /// Creates an ExtendedPropertyValueEditor with the specified extended and inline editor
        /// DataTemplates
        /// </summary>
        /// <param name="extendedEditorTemplate">The DataTemplate used for the extended popup/pinned editor.  
        /// When used, its DataContext will be set to a PropertyValue</param>
        /// <param name="inlineEditorTemplate">The DataTemplate used for the inline editor.  
        /// When used, its DataContext will be set to a PropertyValue</param>
        public ExtendedPropertyValueEditor(DataTemplate extendedEditorTemplate, DataTemplate inlineEditorTemplate)
            : base(inlineEditorTemplate) {
            _extendedEditorTemplate = extendedEditorTemplate;
        }

        /// <summary>
        /// The DataTemplate used for the extended popup/pinned editor.  
        /// Its DataContext will be set to a PropertyValue
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public DataTemplate ExtendedEditorTemplate
        {
            get { return _extendedEditorTemplate; }
            set { _extendedEditorTemplate = value; }
        }

        internal override DataTemplate GetPropertyValueEditor(PropertyContainerEditMode mode) {
            return base.GetPropertyValueEditor(mode) ??
                ((mode == PropertyContainerEditMode.ExtendedPinned ||
                  mode == PropertyContainerEditMode.ExtendedPopup) ? _extendedEditorTemplate : (DataTemplate)null);
        }
    }
}
