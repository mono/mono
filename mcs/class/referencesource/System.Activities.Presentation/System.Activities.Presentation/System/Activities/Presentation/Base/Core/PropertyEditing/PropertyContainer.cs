namespace System.Activities.Presentation.PropertyEditing
{
    using System.Windows.Controls;
    using System.Windows;
    using System;
    using System.Windows.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Input;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Activities.Presentation;
    using System.Runtime;
    using System.Activities.Presentation.Internal.PropertyEditing.Editors;
    using System.Activities.Presentation.Internal.PropertyEditing;

    /// <summary>
    /// This control is used as a graphical container for PropertyEntry instances.  The control is
    /// look-less.  However, it is generally styled as a horizontal row that includes the
    /// name of the property followed by an editor for its value.  This control, however, is
    /// intended to be restiled by 3rd-parties to suite their needs.  The style is controled
    /// by three ControlTemplates (InlineRowTemplate, ExtendedPopupRowTemplate, and
    /// ExtendedPinnedRowTemplate) that are chosed by the logic within this control based
    /// on the current value of ActiveEditMode.  This control also exposes three DataTemplates
    /// (InlineEditor, ExtendedEditor, and DialogEditor) that each of the row templates can use 
    /// to display the appropriate value editor for the PropertyValue being edited.
    /// </summary>
    class PropertyContainer : Control, INotifyPropertyChanged
    {

        private static RoutedCommand _openDialogWindow;

        DataTemplate flagEditorTemplate;

        /// <summary>
        /// INotifyPropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal event DependencyPropertyChangedEventHandler DependencyPropertyChanged;

        private bool _attachedToPropertyEntryEvents;

        /// <summary>
        /// Creates a PropertyContainer
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PropertyContainer()
        {

            // Set the OwningPropertyContainer (attached, inherited DP) to self,
            // so that all of the children of this control know which PropertyContainer
            // they belong to
            SetOwningPropertyContainer(this, this);

            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.Unloaded += new RoutedEventHandler(OnUnloaded);
        }

        // Useful DPs

        // PropertyEntry DP

        public static readonly DependencyProperty IsValueEditEnabledProperty = DependencyProperty.Register(
            "IsValueEditEnabled",
            typeof(bool),
            typeof(PropertyContainer),
            new UIPropertyMetadata(true));


        /// <summary>
        /// Gets or sets the PropertyEntry instance on which this PropertyContainer operates. 
        /// That is the context of the PropertyContainer.  The exposed editor templates 
        /// (InlineEditor, ExtendedEditor, and DialogEditor) are based on the value of this property.
        /// </summary>
        public static readonly DependencyProperty PropertyEntryProperty =
            DependencyProperty.Register(
                "PropertyEntry",
                typeof(PropertyEntry),
                typeof(PropertyContainer),
                new FrameworkPropertyMetadata(
                    null,
                    new PropertyChangedCallback(PropertyEntryPropertyChanged)));

        /// <summary>
        /// Gets or sets the PropertyEntry instance on which this PropertyContainer operates. 
        /// That is the context of the PropertyContainer.  The exposed editor templates 
        /// (InlineEditor, ExtendedEditor, and DialogEditor) are based on the value of this property.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public PropertyEntry PropertyEntry
        {
            get { return (PropertyEntry)this.GetValue(PropertyContainer.PropertyEntryProperty); }
            set { this.SetValue(PropertyContainer.PropertyEntryProperty, value); }
        }

        public bool IsValueEditEnabled
        {
            get { return (bool)GetValue(IsValueEditEnabledProperty); }
            set { SetValue(IsValueEditEnabledProperty, value); }
        }

        private static void PropertyEntryPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            PropertyContainer theThis = (PropertyContainer)obj;

            // Since the underlying property has changed, chances are that the inline or extended editors
            // have changed as well, so fire events indicating that their values have changed
            theThis.NotifyTemplatesChanged();
            theThis.OnPropertyChanged("MatchesFilter");

            // Switch back to Inline mode
            theThis.ActiveEditMode = PropertyContainerEditMode.Inline;

            // Ensure that the Template property of this control is set to the right ControlTemplate
            UpdateControlTemplate(theThis);

            // Hook into PropertyEntry's changed events
            if (e.OldValue != null)
                theThis.DisassociatePropertyEventHandlers((PropertyEntry)e.OldValue);
            if (e.NewValue != null)
                theThis.AssociatePropertyEventHandlers((PropertyEntry)e.NewValue);
        }


        // ActiveEditMode DP

        // 




        /// <summary>
        /// Gets or sets currently displayed edit mode of this container (ie. ExtendedPopup,
        /// ExtendedPinned, Inline or Dialog).  
        /// </summary>
        public static readonly DependencyProperty ActiveEditModeProperty =
            DependencyProperty.Register(
                "ActiveEditMode",
                typeof(PropertyContainerEditMode),
                typeof(PropertyContainer),
                new FrameworkPropertyMetadata(
                    PropertyContainerEditMode.Inline,
                    new PropertyChangedCallback(OnActiveEditModePropertyChanged)));

        /// <summary>
        /// Gets or sets currently displayed edit mode of this container (ie. ExtendedPopup,
        /// ExtendedPinned, Inline or Dialog).  
        /// </summary>
        public PropertyContainerEditMode ActiveEditMode
        {
            get { return (PropertyContainerEditMode)this.GetValue(PropertyContainer.ActiveEditModeProperty); }
            set { this.SetValue(PropertyContainer.ActiveEditModeProperty, value); }
        }

        private static void OnActiveEditModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            PropertyContainer theThis = (PropertyContainer)obj;

            // Ensure that the Template property of this control is set to the right ControlTemplate
            UpdateControlTemplate(theThis);

            // Invoke a dialog editor if needed
            if (object.Equals(e.NewValue, PropertyContainerEditMode.Dialog))
            {

                if (OpenDialogWindow.CanExecute(null, theThis))
                {
                    // There is someone who handles this command, so let it deal with it
                    // however it wants.
                    OpenDialogWindow.Execute(null, theThis);
                }
                else
                {
                    // There is no-one handling this command, so see if there is a virtual
                    // method we can invoke
                    DialogPropertyValueEditor editor = theThis.FindDialogPropertyValueEditor();
                    if (editor != null)
                    {
                        // If the DialogCommandSource is not explicitly set, use this control as the
                        // command source
                        IInputElement dialogCommandSource = theThis.DialogCommandSource ?? theThis;
                        editor.ShowDialog(theThis.PropertyEntry.PropertyValue, dialogCommandSource);
                    }
                }

                // And revert back to old edit mode once done
                theThis.ActiveEditMode = (PropertyContainerEditMode)e.OldValue;
            }
        }


        // DialogCommandSource DP

        /// <summary>
        /// Gets or sets the IInputElement to pass into the ShowDialog() method as the command source.
        /// If null (default), _this_ will be passed in.
        /// Note: ShowDialog() method is called on a DialogPropertyValueEditor instance if a dialog editor
        /// is invoked and the editor does not specify any dialog DataTemplate.
        /// </summary>
        public static readonly DependencyProperty DialogCommandSourceProperty =
            DependencyProperty.Register(
                "DialogCommandSource",
                typeof(IInputElement),
                typeof(PropertyContainer),
                new PropertyMetadata((IInputElement)null));

        /// <summary>
        /// Gets or sets the IInputElement to pass into the ShowDialog() method as the command source.
        /// If null (default), _this_ will be passed in.
        /// Note: ShowDialog() method is called on a DialogPropertyValueEditor instance if a dialog editor
        /// is invoked and the editor does not specify any dialog DataTemplate.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public IInputElement DialogCommandSource
        {
            get { return (IInputElement)this.GetValue(DialogCommandSourceProperty); }
            set { this.SetValue(DialogCommandSourceProperty, value); }
        }


        // OwningPropertyContainer Attached, Inherited DP

        /// <summary>
        /// Attached, inherited DP that can be used by UI elements of PropertyValueEditors
        /// to gain access to their parent PropertyContainer.
        /// </summary>
        public static readonly DependencyProperty OwningPropertyContainerProperty =
            DependencyProperty.RegisterAttached(
                "OwningPropertyContainer",
                typeof(PropertyContainer),
                typeof(PropertyContainer),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Setter for attached, inherited DP that can be used by UI elements of PropertyValueEditors
        /// to gain access to their parent PropertyContainer.
        /// </summary>
        /// <param name="dependencyObject">The DO to set the property on</param>
        /// <param name="value">The Owning PropertyContainer</param>
        public static void SetOwningPropertyContainer(DependencyObject dependencyObject, PropertyContainer value)
        {
            if (dependencyObject == null)
                throw FxTrace.Exception.ArgumentNull("dependencyObject");

            dependencyObject.SetValue(PropertyContainer.OwningPropertyContainerProperty, value);
        }

        /// <summary>
        /// Getter for attached, inherited DP that can be used by UI elements of PropertyValueEditors
        /// to gain access to their parent PropertyContainer.
        /// </summary>
        /// <param name="dependencyObject">The DO to get the property from</param>
        /// <returns>The owning PropertyContainer</returns>
        public static PropertyContainer GetOwningPropertyContainer(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw FxTrace.Exception.ArgumentNull("dependencyObject");

            return (PropertyContainer)dependencyObject.GetValue(PropertyContainer.OwningPropertyContainerProperty);
        }


        // ControlTemplates for PropertyContainer to define the UI for the different edit modes

        // InlineRowTemplate DP

        /// <summary>
        /// This DP is used to get/set the InlineRowTemplate for the PropertyContainer.  The 
        /// InlineRowTemplate defines how the PropertyContainer renders itself when 
        /// ActiveEditMode = Inline.
        /// </summary>
        public static readonly DependencyProperty InlineRowTemplateProperty =
            DependencyProperty.Register(
                "InlineRowTemplate",
                typeof(ControlTemplate),
                typeof(PropertyContainer),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.None,
                    new PropertyChangedCallback(RowTemplateChanged)));

        /// <summary>
        /// Gets or sets the InlineRowTemplate for the PropertyContainer.  The 
        /// InlineRowTemplate defines how the PropertyContainer renders itself when 
        /// ActiveEditMode = Inline.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public ControlTemplate InlineRowTemplate
        {
            get { return (ControlTemplate)this.GetValue(PropertyContainer.InlineRowTemplateProperty); }
            set { this.SetValue(PropertyContainer.InlineRowTemplateProperty, value); }
        }

        // Called when any of the row templates (InlineRowTemplate, ExtendedPopupRowTemplate, ExtendedPinnedRowTemplate)
        // change
        private static void RowTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            PropertyContainer theThis = (PropertyContainer)obj;
            bool updateControlTemplate = false;

            // Check InlineRowTemplate
            updateControlTemplate = updateControlTemplate |
                (e.Property == PropertyContainer.InlineRowTemplateProperty &&
                theThis.ActiveEditMode == PropertyContainerEditMode.Inline);

            // Check ExtendedPopup
            updateControlTemplate = updateControlTemplate |
                (e.Property == PropertyContainer.ExtendedPopupRowTemplateProperty &&
                theThis.ActiveEditMode == PropertyContainerEditMode.ExtendedPopup);

            // Check ExtendedPinned
            updateControlTemplate = updateControlTemplate |
                (e.Property == PropertyContainer.ExtendedPinnedRowTemplateProperty &&
                theThis.ActiveEditMode == PropertyContainerEditMode.ExtendedPinned);

            if (updateControlTemplate)
                UpdateControlTemplate(theThis);
        }


        // ExtendedPopupRowTemplate DP

        /// <summary>
        /// This DP is used to get/set the ExtendedPopupRowTemplate for this PropertyContainer.
        /// The ExtendedPopupRowTemplate defines how the PropertyContainer renders itself when 
        /// ActiveEditMode = ExtendedPopup.  Generally, host implementations will define this
        /// template to automatically include the InlineRowTemplate as well.
        /// </summary>
        public static readonly DependencyProperty ExtendedPopupRowTemplateProperty =
            DependencyProperty.Register(
                "ExtendedPopupRowTemplate",
                typeof(ControlTemplate),
                typeof(PropertyContainer),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.None,
                    new PropertyChangedCallback(RowTemplateChanged)));

        /// <summary>
        /// Gets or sets the ExtendedPopupRowTemplate for this PropertyContainer.  
        /// The ExtendedPopupRowTemplate defines how the PropertyContainer renders itself when 
        /// ActiveEditMode = ExtendedPopup.  Generally, host implementations will define this
        /// template to automatically include the InlineRowTemplate as well.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public ControlTemplate ExtendedPopupRowTemplate
        {
            get { return (ControlTemplate)this.GetValue(PropertyContainer.ExtendedPopupRowTemplateProperty); }
            set { this.SetValue(PropertyContainer.ExtendedPopupRowTemplateProperty, value); }
        }


        // ExtendedPinnedRowTemplate DP

        /// <summary>
        /// This DP is used to get/set the ExtendedPinnedRowTemplate for this PropertyContainer.  
        /// The ExtendedPinnedRowTemplate defines how the PropertyContainer renders itself when 
        /// ActiveEditMode = ExtendedPinned.  Generally, host implementations will define this
        /// template to automatically include the InlineRowTemplate as well.
        /// </summary>
        public static readonly DependencyProperty ExtendedPinnedRowTemplateProperty =
            DependencyProperty.Register(
                "ExtendedPinnedRowTemplate",
                typeof(ControlTemplate),
                typeof(PropertyContainer),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.None,
                    new PropertyChangedCallback(RowTemplateChanged)));

        /// <summary>
        /// Get/set the ExtendedPinnedRowTemplate for this PropertyContainer.  
        /// The ExtendedPinnedRowTemplate defines how the PropertyContainer renders itself when 
        /// ActiveEditMode = ExtendedPinned.  Generally, host implementations will define this
        /// template to automatically include the InlineRowTemplate as well.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public ControlTemplate ExtendedPinnedRowTemplate
        {
            get { return (ControlTemplate)this.GetValue(PropertyContainer.ExtendedPinnedRowTemplateProperty); }
            set { this.SetValue(PropertyContainer.ExtendedPinnedRowTemplateProperty, value); }
        }


        // Default PropertyValueEditors to use when a given property doesn't specify its own

        // DefaultStandardValuesPropertyValueEditor DP

        /// <summary>
        /// DP to get or set the default standard-values editor which is used when a PropertyEntry supports
        /// StandardValues (enum or through a TypeConverter) and there isn't a PropertyValueEditor
        /// defined for the PropertyEntry or Type explicitely.
        /// </summary>
        public static readonly DependencyProperty DefaultStandardValuesPropertyValueEditorProperty =
            DependencyProperty.Register(
                "DefaultStandardValuesPropertyValueEditor",
                typeof(PropertyValueEditor),
                typeof(PropertyContainer),
                new FrameworkPropertyMetadata(
                    null,
                    new PropertyChangedCallback(DefaultPropertyValueEditorChanged)));

        /// <summary>
        /// Gets or set the default standard-values editor which is used when a PropertyEntry supports
        /// StandardValues (enum or through a TypeConverter) and there isn't a PropertyValueEditor
        /// defined for the PropertyEntry or Type explicitely.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public PropertyValueEditor DefaultStandardValuesPropertyValueEditor
        {
            get { return (PropertyValueEditor)this.GetValue(PropertyContainer.DefaultStandardValuesPropertyValueEditorProperty); }
            set { this.SetValue(PropertyContainer.DefaultStandardValuesPropertyValueEditorProperty, value); }
        }

        private static void DefaultPropertyValueEditorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            PropertyContainer theThis = (PropertyContainer)obj;

            // Since one of the default PVE's has changed, chances are that the Inline or the Extended
            // editor template has changed as well.
            theThis.NotifyTemplatesChanged();
        }


        // DefaultPropertyValueEditor DP

        /// <summary>
        /// DP to get or set the default PropertyValueEditor which is the editor used when the
        /// PropertyEntry or Type does not explicitely define its own PropertyValueEditor and does not
        /// support StandardValues.
        /// </summary>
        public static readonly DependencyProperty DefaultPropertyValueEditorProperty =
            DependencyProperty.Register(
                "DefaultPropertyValueEditor",
                typeof(PropertyValueEditor),
                typeof(PropertyContainer),
                new FrameworkPropertyMetadata(
                    null,
                    new PropertyChangedCallback(DefaultPropertyValueEditorChanged)));

        /// <summary>
        /// Gets or sets the default PropertyValueEditor which is the editor used when the
        /// PropertyEntry or Type does not explicitely define its own PropertyValueEditor and does not
        /// support StandardValues.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public PropertyValueEditor DefaultPropertyValueEditor
        {
            get { return (PropertyValueEditor)this.GetValue(PropertyContainer.DefaultPropertyValueEditorProperty); }
            set { this.SetValue(PropertyContainer.DefaultPropertyValueEditorProperty, value); }
        }


        // Regular properties (read-only values for DataBinding)

        // InlineEditorTemplate read-only CLR property

        /// <summary>
        /// Gets the most appropriate InlineEditorTemplate for the current PropertyEntry.
        /// A row template may decide to use this value to render the editor on the appropriate place.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public DataTemplate InlineEditorTemplate
        {
            get
            {
                return FindPropertyValueEditorTemplate(PropertyContainerEditMode.Inline);
            }
        }


        // ExtendedEditorTemplate read-only CLR property

        /// <summary>
        /// Gets the most appropriate ExtendedEditorTemplate for the current PropertyEntry.
        /// A row template may decide to use this value to render the editor on the appropriate place.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public DataTemplate ExtendedEditorTemplate
        {
            get
            {
                return FindPropertyValueEditorTemplate(PropertyContainerEditMode.ExtendedPinned);
            }
        }


        // DialogEditorTemplate read-only CLR property

        /// <summary>
        /// Gets the most appropriate DialogEditorTemplate for the current PropertyEntry.
        /// A row template or a Dialog may decide to use this value to render the editor on the
        /// appropriate place.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public DataTemplate DialogEditorTemplate
        {
            get
            {
                return FindPropertyValueEditorTemplate(PropertyContainerEditMode.Dialog);
            }
        }


        // MatchesFilter read-only CLR property

        /// <summary>
        /// Gets the value for MatchesFilter stored in the contained PropertyEntry.  If the PropertyEntry
        /// is null, the value returned is false.
        /// This property can be used to trigger UI changes to the PropertyContainer based on
        /// whether the current PropertyEntry matches the current filter or not.
        /// </summary>
        public bool MatchesFilter
        {
            get
            {
                PropertyEntry property = this.PropertyEntry;
                return property != null && property.MatchesFilter;
            }
        }


        // OpenDialogWindow static, read-only command property

        /// <summary>
        /// Gets the command that is fired when someone changes the ActiveEditMode property to "Dialog".
        /// The host may choose to handle this command and, display the DialogEditorTemplate
        /// (if one exists) in a host-specific dialog container.  If the host does not handle
        /// this command (OpenDialogWindow.CanExecute is false), the PropertyContainer itself
        /// defaults to calling into the virtual DialogPropertyValueEditor.ShowDialog()
        /// method, but only if DialogPropertyValueEditor is found.
        /// </summary>
        public static RoutedCommand OpenDialogWindow
        {
            get
            {
                if (_openDialogWindow == null)
                    _openDialogWindow = new RoutedCommand("OpenDialogWindow", typeof(PropertyContainer));

                return _openDialogWindow;
            }
        }


        internal bool SupportsEditMode(PropertyContainerEditMode mode)
        {
            // special handling for dialog editor
            if (mode == PropertyContainerEditMode.Dialog)
                return FindDialogPropertyValueEditor() != null;

            // for everything else
            return FindPropertyValueEditorTemplate(mode) != null;
        }

        // When the control gets unloaded, unhook any remaining event handlers
        // so that it can be garbage collected
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            PropertyEntry entry = this.PropertyEntry;
            if (entry != null)
                DisassociatePropertyEventHandlers(entry);
        }

        // When the control gets re-loaded, re-hook any previous event handlers
        // that may have been disassociated during Unload
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PropertyEntry entry = this.PropertyEntry;
            if (entry != null)
                AssociatePropertyEventHandlers(entry);
        }

        // Helper that hooks into PropertyChanged events on the given Property
        private void AssociatePropertyEventHandlers(PropertyEntry property)
        {
            if (!_attachedToPropertyEntryEvents)
            {
                property.PropertyChanged += new PropertyChangedEventHandler(OnPropertyPropertyChanged);
                _attachedToPropertyEntryEvents = true;
            }
        }

        // Helper that unhooks from PropertyChanged events on the given Property
        private void DisassociatePropertyEventHandlers(PropertyEntry property)
        {
            if (_attachedToPropertyEntryEvents)
            {
                property.PropertyChanged -= new PropertyChangedEventHandler(OnPropertyPropertyChanged);
                _attachedToPropertyEntryEvents = false;
            }
        }

        // Called when the properties of the Property object change
        private void OnPropertyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Propagate MatchesFilter change notifications outwards so that
            // people can set up triggers against it
            if ("MatchesFilter".Equals(e.PropertyName))
                this.OnPropertyChanged("MatchesFilter");
            else if ("PropertyValueEditor".Equals(e.PropertyName))
                this.NotifyTemplatesChanged();
        }

        /// <summary>
        /// Helper class that attempts to find the DataTemplate for the requested PropertyContainerEditMode
        /// given the currently displayed PropertyEntry and the currently set default PropertyValueEditors.
        /// 
        /// If the requested DataTemplate is found on the PropertyValueEditor associated with the displayed
        /// control, is it returned.
        /// 
        /// Otherwise if the requested DataTemplate is found on DefaultStandardValuesPropertyValueEditor,
        /// it is returned.
        /// 
        /// Otherwise if the requested DataTemplate is found on DefaultPropertyValueEditor,
        /// it is returned.
        /// 
        /// Otherwise null is returned.
        /// </summary>
        /// <param name="editMode">The editMode for which the DataTemplate should be retrieved</param>
        /// <returns>Most relevant DataTemplate for the specified edit mode based on the currently
        /// displayed PropertyEntry and the current set of default PropertyValueEditors, or null if not
        /// found.</returns>
        private DataTemplate FindPropertyValueEditorTemplate(PropertyContainerEditMode editMode)
        {
            PropertyEntry property = this.PropertyEntry;
            PropertyValueEditor editor = null;
            DataTemplate requestedTemplate = null;

            // Look at property
            if (property != null)
            {
                editor = property.PropertyValueEditor;
                if (editor != null)
                {
                    requestedTemplate = editor.GetPropertyValueEditor(editMode);
                }
            }

            if (requestedTemplate != null)
                return requestedTemplate;

            // Is the property of type enum and used as flags?
            if (IsFlagsProperty(property))
            {
                requestedTemplate = this.GetFlagEditorTemplate(editMode);
            }

            if (requestedTemplate != null)
            {
                return requestedTemplate;
            }

            // Does the property have standard values?
            if (property != null && property.HasStandardValuesInternal)
            {
                editor = this.DefaultStandardValuesPropertyValueEditor;
                if (editor != null)
                {
                    requestedTemplate = editor.GetPropertyValueEditor(editMode);
                }
            }

            if (requestedTemplate != null)
                return requestedTemplate;

            // Use the default
            editor = this.DefaultPropertyValueEditor;
            if (editor != null)
            {
                requestedTemplate = editor.GetPropertyValueEditor(editMode);
            }

            return requestedTemplate;
        }

        bool IsFlagsProperty(PropertyEntry property)
        {
            return property != null && property.PropertyType != null && property.PropertyType.IsEnum &&
                   ExtensibilityAccessor.GetAttribute<FlagsAttribute>(property.PropertyType) != null;
        }

        DataTemplate GetFlagEditorTemplate(PropertyContainerEditMode editMode)
        {
            Type propertyType = this.PropertyEntry.PropertyType;
            if (editMode == PropertyContainerEditMode.Inline)
            {
                if (this.flagEditorTemplate == null)
                {
                    this.flagEditorTemplate = new DataTemplate();
                    this.flagEditorTemplate.VisualTree = new FrameworkElementFactory(typeof(FlagEditor));
                    this.flagEditorTemplate.VisualTree.SetValue(FlagEditor.FlagTypeProperty, propertyType);
                    Binding binding = new Binding("Value");
                    binding.Converter = new FlagStringConverter();
                    binding.ConverterParameter = propertyType;
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                    this.flagEditorTemplate.VisualTree.SetBinding(FlagEditor.TextProperty, binding);
                }
                return this.flagEditorTemplate;
            }
            else
            {
                return null;
            }
        }

        // Helper that tries to find the first applicable DialogPropertyValueEditor
        private DialogPropertyValueEditor FindDialogPropertyValueEditor()
        {
            PropertyEntry property = this.PropertyEntry;
            DialogPropertyValueEditor editor = null;

            // Look at property
            if (property != null)
            {
                editor = property.PropertyValueEditor as DialogPropertyValueEditor;
            }

            if (editor != null)
                return editor;

            // Does the property have standard values?
            if (property != null && property.HasStandardValuesInternal)
            {
                editor = this.DefaultStandardValuesPropertyValueEditor as DialogPropertyValueEditor;
            }

            if (editor != null)
                return editor;

            // Use the default
            editor = this.DefaultPropertyValueEditor as DialogPropertyValueEditor;

            return editor;
        }

        // Updates the ControlTemplate of this control based on the currently ActiveEditMode
        private static void UpdateControlTemplate(PropertyContainer container)
        {

            PropertyContainerEditMode editMode = container.ActiveEditMode;
            ControlTemplate newTemplate = null;

            switch (editMode)
            {
                case PropertyContainerEditMode.Inline:
                    newTemplate = container.InlineRowTemplate;
                    break;
                case PropertyContainerEditMode.ExtendedPopup:
                    newTemplate = container.ExtendedPopupRowTemplate;
                    break;
                case PropertyContainerEditMode.ExtendedPinned:
                    newTemplate = container.ExtendedPinnedRowTemplate;
                    break;
                case PropertyContainerEditMode.Dialog:
                    // In dialog case, just keep the same value
                    return;
                default:
                    Debug.Fail(
                        string.Format(
                            System.Globalization.CultureInfo.CurrentCulture,
                            "PropertyContainerEditMode does not yet support PropertyContainerEditMode '{0}'.",
                            editMode.ToString()));
                    newTemplate = container.Template;
                    break;
            }

            if (newTemplate != container.Template)
                container.Template = newTemplate;
        }

        private void NotifyTemplatesChanged()
        {
            OnPropertyChanged("InlineEditorTemplate");
            OnPropertyChanged("ExtendedEditorTemplate");
            OnPropertyChanged("DialogEditorTemplate");
        }

        /// <summary>
        /// Called when a property changes
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called when a property changes
        /// </summary>
        /// <param name="e">Name of the property</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (DependencyPropertyChanged != null)
                DependencyPropertyChanged(this, e);

            base.OnPropertyChanged(e);
        }
    }
}

