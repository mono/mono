namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using System.Activities.Presentation.Internal.Properties;
    using System.Activities.Presentation;

    /// <summary>
    /// Derive this class to provide a custom CategoryEditor for a set of Properties in a property
    /// browser host.
    /// </summary>
    public abstract class CategoryEditor {

        /// <summary>
        /// This method is called once for each property in the category to determine which properties
        /// are edited by this CategoryEditor.  When a property is consumed by a CategoryEditor, it does
        /// not show up as a separate row in that category.
        /// </summary>
        /// <param name="propertyEntry">The PropertyEntry to check to see if its edited by this CategoryEditor</param>
        /// <returns>true if this editor edits that property, otherwise false</returns>
        public abstract bool ConsumesProperty(PropertyEntry propertyEntry);

        /// <summary>
        /// Returns a localized string that indicates which category this editor belongs to. CategoryEditors are
        /// defined on types and, thus, at load time they need to indicate the actual category they belong to.
        /// </summary>
        public abstract string TargetCategory { get; }

        /// <summary>
        /// Returns a DataTemplate that is hosted by the PropertyInspector as the UI for a CategoryEditor.
        /// The DataSource of this DataTemplate is set to a CategoryEntry.
        /// </summary>
        public abstract DataTemplate EditorTemplate { get; }

        /// <summary>
        /// Returns an object that the host can place into a ContentControl in order to display it.
        /// This icon may be used to adorn the editor for this category in
        /// a collapsed mode, should it support one.
        /// </summary>
        /// <param name="desiredSize">The desired size of the image to return.  This method should make
        /// the best attempt in matching the requested size, but it doesn't guarantee it.</param>
        public abstract object GetImage(Size desiredSize);

        /// <summary>
        /// Utility method that creates a new EditorAttribute for the specified
        /// CategoryEditor
        /// </summary>
        /// <param name="editor">CategoryEditor instance for which to create
        /// the new EditorAttribute</param>
        /// <returns>New EditorAttribute for the specified CategoryEditor</returns>
        public static EditorAttribute CreateEditorAttribute(CategoryEditor editor) {
            if (editor == null)
                throw FxTrace.Exception.ArgumentNull("editor");

            return CreateEditorAttribute(editor.GetType());
        }

        /// <summary>
        /// Utility method that creates a new EditorAttribute for the specified
        /// CategoryEditor type
        /// </summary>
        /// <param name="categoryEditorType">CategoryEditor type for which to create
        /// the new EditorAttribute</param>
        /// <returns>New EditorAttribute for the specified CategoryEditor type</returns>
        public static EditorAttribute CreateEditorAttribute(Type categoryEditorType) {
            if (categoryEditorType == null)
                throw FxTrace.Exception.ArgumentNull("categoryEditorType");

            if (!typeof(CategoryEditor).IsAssignableFrom(categoryEditorType))
                throw FxTrace.Exception.AsError(new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error_ArgIncorrectType,
                        "categoryEditorType",
                        typeof(CategoryEditor).Name)));

            return new EditorAttribute(categoryEditorType, categoryEditorType);
        }
    }
}
