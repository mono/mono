namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// When this control is placed on a one of the PropertyContainer *RowTemplates, it acts as a
    /// marker for the spowhere a host specific PropertyMarker should be put at runtime.
    /// Some hosts may not provide a PropertyMarker, in which case this control will render
    /// with Visibility.Collapsed.
    /// </summary>
    class PropertyMarker : Control {
    }
}