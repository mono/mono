namespace System.Activities.Presentation.PropertyEditing {
    using System;

    /// <summary>
    /// Enum indicating the source of the exception thrown by PropertyValue
    /// </summary>
    public enum PropertyValueExceptionSource {
        /// <summary>
        /// Indicates that the exception occurred during a Get
        /// </summary>
        Get,

        /// <summary>
        /// Indicates that the exception occurred during a Set
        /// </summary>
        Set
    }
}
