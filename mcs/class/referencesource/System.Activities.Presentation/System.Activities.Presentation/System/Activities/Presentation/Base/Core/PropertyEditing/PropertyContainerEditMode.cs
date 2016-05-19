namespace System.Activities.Presentation.PropertyEditing {

    /// <summary>
    /// Enum that defines the different edit modes a PropertyContainer can be in
    /// </summary>
    public enum PropertyContainerEditMode {
        /// <summary>
        /// Inline editor mode
        /// </summary>
        Inline,

        /// <summary>
        /// Extended editor, popped up over the property inspector
        /// </summary>
        ExtendedPopup,

        /// <summary>
        /// Extended editor, pinned into place within the property inspector
        /// </summary>
        ExtendedPinned,

        /// <summary>
        /// Dialog popup editor
        /// </summary>
        Dialog
    }
}
