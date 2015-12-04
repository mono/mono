namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Controls;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics;

    /// <summary>
    /// Convenience button that allows the user to switch between the different PropertyContainer modes.
    /// This button is styled to follow the look and feel specific to the host application.  It can operate
    /// in two modes - either it always executes a specified mode-switching command, or it adapts to
    /// the current mode of the containing PropertyContainer and "does the right thing".  If set manually,
    /// SyncModeToOwningContainer must be set to false and the mode-switching command needs to be specified
    /// using the TargetEditMode property.  To set the mode automatically, SyncModeToOwningContainer must
    /// be set to true in which case the TargetEditMode property is ignored.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class EditModeSwitchButton : Button {

        private PropertyContainer _owningContainer;
        private bool _attachedToContainerEvents;

        /// <summary>
        /// Basic ctor
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EditModeSwitchButton() {
            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.Unloaded += new RoutedEventHandler(OnUnloaded);
            this.FontSize = SystemFonts.IconFontSize;
            this.FontFamily = SystemFonts.IconFontFamily;
            this.FontWeight = SystemFonts.IconFontWeight;
        }

        // TargetEditMode DP

        /// <summary>
        /// The mode to switch to when this control is clicked.  Only used when 
        /// SyncModeToOwningContainer is set to false.  Defaults to Inline.
        /// </summary>
        public static readonly DependencyProperty TargetEditModeProperty = DependencyProperty.Register(
            "TargetEditMode",
            typeof(PropertyContainerEditMode),
            typeof(EditModeSwitchButton),
            new FrameworkPropertyMetadata(
                PropertyContainerEditMode.Inline,
                null,  // PropertyChangedCallback
                new CoerceValueCallback(OnCoerceEditModeProperty)));

        /// <summary>
        /// The mode to switch to when this control is clicked.  Only used when 
        /// SyncModeToOwningContainer is set to false.  Defaults to Inline.
        /// </summary>
        public PropertyContainerEditMode TargetEditMode {
            get { return (PropertyContainerEditMode) this.GetValue(TargetEditModeProperty); }
            set { this.SetValue(TargetEditModeProperty, value); }
        }

        private static object OnCoerceEditModeProperty(DependencyObject obj, object value) {

            EditModeSwitchButton theThis = (EditModeSwitchButton) obj;

            // [....] to the owning PropertyContainer only if requested to do so
            if (!theThis.SyncModeToOwningContainer)
                return value;

            // Do we have an owning PropertyContainer?
            if (theThis._owningContainer == null)
                return value;

            PropertyContainerEditMode newMode;
            PropertyContainer owningContainer = theThis._owningContainer;

            switch (owningContainer.ActiveEditMode) {
                case PropertyContainerEditMode.Inline:
                    // when clicked, have this button switch to extended popup mode
                    // or dialog mode (dialog takes precedence)
                    if (owningContainer.SupportsEditMode(PropertyContainerEditMode.Dialog))
                        newMode = PropertyContainerEditMode.Dialog;
                    else if (owningContainer.SupportsEditMode(PropertyContainerEditMode.ExtendedPopup))
                        newMode = PropertyContainerEditMode.ExtendedPopup;
                    else
                        newMode = PropertyContainerEditMode.Inline;

                    break;

                case PropertyContainerEditMode.ExtendedPopup:
                    // when clicked, have this button switch to extended pinned mode
                    newMode = PropertyContainerEditMode.ExtendedPinned;
                    break;

                case PropertyContainerEditMode.ExtendedPinned:
                    // when clicked, have this button switch to inline mode
                    newMode = PropertyContainerEditMode.Inline;
                    break;

                case PropertyContainerEditMode.Dialog:
                    // do nothing
                    newMode = theThis.TargetEditMode;
                    break;

                default:
                    Debug.Fail(string.Format(
                        System.Globalization.CultureInfo.CurrentCulture,
                        "ModeSwitchControl does not yet support PropertyContainerEditMode '{0}'.",
                        owningContainer.ActiveEditMode.ToString()));
                    newMode = (PropertyContainerEditMode) value;
                    break;
            }

            return newMode;
        }


        // SyncModeToOwningContainer DP

        /// <summary>
        /// When set to true, the TargetEditMode will be calculated automatically to match the ActiveEditMode
        /// of the owning PropertyContainer. Otherwise, the mode to switch to will be based on the
        /// TargetEditMode property.
        /// </summary>
        public static readonly DependencyProperty SyncModeToOwningContainerProperty = DependencyProperty.Register(
            "SyncModeToOwningContainer",
            typeof(bool),
            typeof(EditModeSwitchButton),
            new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnSyncModeToOwningContainerChanged)));

        /// <summary>
        /// When set to true, the TargetEditMode will be calculated automatically to match the ActiveEditMode
        /// of the owning PropertyContainer. Otherwise, the mode to switch to will be based on the
        /// TargetEditMode property.
        /// </summary>
        public bool SyncModeToOwningContainer {
            get { return (bool)this.GetValue(SyncModeToOwningContainerProperty); }
            set { this.SetValue(SyncModeToOwningContainerProperty, value); }
        }

        // When the SyncModeToOwningContainer changes, we may need to update the current
        // TargetEditMode
        private static void OnSyncModeToOwningContainerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            EditModeSwitchButton theThis = (EditModeSwitchButton)obj;
            theThis.CoerceValue(TargetEditModeProperty);
        }


        /// <summary>
        /// Called when any DependencyProperties of this Control change.  If you override
        /// this method, call the base implementation first to preserve the desired functionality.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            // Check to see if this control has changed (or gained or lost) its owning PropertyContainer.
            // If so, it may need to update its state / appearance accordingly
            if (e.Property == PropertyContainer.OwningPropertyContainerProperty) {
                PropertyContainer oldContainer = (PropertyContainer)e.OldValue;
                PropertyContainer newContainer = (PropertyContainer)e.NewValue;
                _owningContainer = newContainer;

                if (oldContainer != null)
                    DisassociateContainerEventHandlers(oldContainer);

                if (newContainer != null)
                    AssociateContainerEventHandlers(newContainer);

                this.CoerceValue(TargetEditModeProperty);
            }

            base.OnPropertyChanged(e);
        }

        private void OnPropertyContainerDependencyPropertyChanged(object sender, DependencyPropertyChangedEventArgs e) {
            // All of these properties changing have the potential of affecting the appearance
            // of this control
            if (e.Property == PropertyContainer.ActiveEditModeProperty ||
                e.Property == PropertyContainer.PropertyEntryProperty ||
                e.Property == PropertyContainer.DefaultStandardValuesPropertyValueEditorProperty ||
                e.Property == PropertyContainer.DefaultPropertyValueEditorProperty) {

                this.CoerceValue(TargetEditModeProperty);
            }
        }

        private void AssociateContainerEventHandlers(PropertyContainer container) {
            if (!_attachedToContainerEvents) {
                container.DependencyPropertyChanged += new DependencyPropertyChangedEventHandler(OnPropertyContainerDependencyPropertyChanged);
                _attachedToContainerEvents = true;
            }
        }

        private void DisassociateContainerEventHandlers(PropertyContainer container) {
            if (_attachedToContainerEvents) {
                container.DependencyPropertyChanged -= new DependencyPropertyChangedEventHandler(OnPropertyContainerDependencyPropertyChanged);
                _attachedToContainerEvents = false;
            }
        }


        // When the control gets unloaded, unhook any remaining event handlers
        // so that it can be garbage collected
        private void OnUnloaded(object sender, RoutedEventArgs e) {
            if (_owningContainer != null)
                DisassociateContainerEventHandlers(_owningContainer);
        }

        // When the control gets re-loaded somewhere else in the tree, re-hook the
        // event handlers back up
        private void OnLoaded(object sender, RoutedEventArgs e) {
            if (_owningContainer != null)
                AssociateContainerEventHandlers(_owningContainer);
        }

        /// <summary>
        /// Because of the nature of the popup capturing mouse and disappearing when the
        /// user clicks outside of it, to pin an ExtendedEditor we listen to MouseDown event
        /// rather than the Click event.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnMouseDown(MouseButtonEventArgs e) {

            if (e.LeftButton == MouseButtonState.Pressed) {

                // Invoke the appropriate command
                switch (this.TargetEditMode) {
                    case PropertyContainerEditMode.Inline:
                        PropertyValueEditorCommands.ShowInlineEditor.Execute(null, this);
                        break;
                    case PropertyContainerEditMode.ExtendedPopup:
                        PropertyValueEditorCommands.ShowExtendedPopupEditor.Execute(null, this);
                        break;
                    case PropertyContainerEditMode.ExtendedPinned:
                        PropertyValueEditorCommands.ShowExtendedPinnedEditor.Execute(null, this);
                        break;
                    case PropertyContainerEditMode.Dialog:
                        PropertyValueEditorCommands.ShowDialogEditor.Execute(null, this);
                        break;
                    default:
                        Debug.Fail(string.Format(
                            System.Globalization.CultureInfo.CurrentCulture,
                            "ModeSwitchControl does not yet support PropertyContainerEditMode '{0}'.",
                            this.TargetEditMode.ToString()));
                        break;
                }
            }

            base.OnMouseDown(e);
        }
    }
}
