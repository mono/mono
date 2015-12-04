// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\ValueEditors
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors
{
    using System;
    using System.Text;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Controls.Primitives;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data;
    using System.Runtime;

    // <summary>
    // Determines the view type of the choice editor.
    // Combo - a combo box
    // Buttons - a set of radio buttons with icons
    // ToggleButtons - a set of radio buttons with icons that use the ToggleIcon style, which allows separate active and inactive icons.
    // Toggle - a single toggle, good for values that have only two choices, like Boolean
    // </summary>
    internal enum ChoiceEditorViewType
    {
        Combo,
        Buttons,
        ToggleButtons,
        Toggle
    }
    // <summary>
    // A ChoiceEditor selects a single item from a list of choices. Think combobox or set of radio buttons.
    // </summary>
    internal class ChoiceEditor : Control, INotifyPropertyChanged, IIconProvider
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ChoiceEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChoiceEditor.ValueChanged), null, false, UpdateSourceTrigger.Explicit));
        public static readonly DependencyProperty ValueIndexProperty = DependencyProperty.Register("ValueIndex", typeof(int), typeof(ChoiceEditor), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChoiceEditor.ValueIndexChanged)));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ChoiceEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ChoiceEditor.ItemsSourceChanged)));
        public static readonly DependencyProperty ConverterProperty = DependencyProperty.Register("Converter", typeof(TypeConverter), typeof(ChoiceEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty ViewTypeProperty = DependencyProperty.Register("ViewType", typeof(ChoiceEditorViewType), typeof(ChoiceEditor), new FrameworkPropertyMetadata(ChoiceEditorViewType.Combo, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(ChoiceEditor), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ChoiceEditor.IsEditableChanged)));
        public static readonly DependencyProperty IconResourcePrefixProperty = DependencyProperty.Register("IconResourcePrefix", typeof(string), typeof(ChoiceEditor), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IconResourceSuffixProperty = DependencyProperty.Register("IconResourceSuffix", typeof(string), typeof(ChoiceEditor), new FrameworkPropertyMetadata("Icon"));
        public static readonly DependencyProperty IsNinchedProperty = DependencyProperty.Register("IsNinched", typeof(bool), typeof(ChoiceEditor), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ChoiceEditor.IsNinchedChanged)));
        public static readonly DependencyProperty ShowFullControlProperty = DependencyProperty.Register("ShowFullControl", typeof(bool), typeof(ChoiceEditor), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, null, new CoerceValueCallback(ChoiceEditor.CoerceShowFullControl)));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ChoiceEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ChoiceEditor.ItemTemplateChanged)));
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(ChoiceEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ChoiceEditor.ItemTemplateSelectorChanged)));
        public static readonly DependencyProperty UseItemTemplateForSelectionProperty = DependencyProperty.Register("UseItemTemplateForSelection", typeof(Nullable<bool>), typeof(ChoiceEditor), new FrameworkPropertyMetadata(null, null, new CoerceValueCallback(ChoiceEditor.CoerceUseItemTemplateForSelection)));

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register("BorderCornerRadius", typeof(double), typeof(ChoiceEditor), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty DropButtonInsetProperty = DependencyProperty.Register("DropButtonInset", typeof(Thickness), typeof(ChoiceEditor), new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TextAreaInsetProperty = DependencyProperty.Register("TextAreaInset", typeof(Thickness), typeof(ChoiceEditor), new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty DropButtonBrushProperty = DependencyProperty.Register("DropButtonBrush", typeof(Brush), typeof(ChoiceEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty InnerCornerRadiusProperty = DependencyProperty.Register("InnerCornerRadius", typeof(double), typeof(ChoiceEditor), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ButtonIconProperty = DependencyProperty.Register("ButtonIcon", typeof(ImageSource), typeof(ChoiceEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(double), typeof(ChoiceEditor), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(double), typeof(ChoiceEditor), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BeginCommandProperty = DependencyProperty.Register("BeginCommand", typeof(ICommand), typeof(ChoiceEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty UpdateCommandProperty = DependencyProperty.Register("UpdateCommand", typeof(ICommand), typeof(ChoiceEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(ChoiceEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty CommitCommandProperty = DependencyProperty.Register("CommitCommand", typeof(ICommand), typeof(ChoiceEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty FinishEditingCommandProperty = DependencyProperty.Register("FinishEditingCommand", typeof(ICommand), typeof(ChoiceEditor), new PropertyMetadata(null));

        public static readonly DependencyProperty ComboBoxLoadingCursorProperty = DependencyProperty.Register("ComboBoxLoadingCursor", typeof(Cursor), typeof(ChoiceEditor), new PropertyMetadata(null));

        // WORKAROUND this property is used in combination with a trigger to kick the combobox when it clears its bindings Avalon bug: 1756023
        public static readonly DependencyProperty ForceBindingProperty = DependencyProperty.Register("ForceBinding", typeof(bool), typeof(ChoiceEditor), new FrameworkPropertyMetadata(false));


        // True if the user is editing the text in an editable combo
        private bool isTextEditing = false;
        // True if the user is selecting a value (e.g. in a combo when the dropdown is open)
        private bool isSelectingValue = false;

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
        // ###################################################

        // True if the full editor is being shown
        private bool isShowingFullEditor = false;

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
        // ###################################################

        // Used to lock out internal changes. Do not change this directly. Use Begin/EndIgnoreInternalChangeBlock
        private int internalChangeLockCount = 0;
        // Used to not set commit action = lost focus when cancelling an edit
        private int internalStringValueChangeLockCount = 0;
        // True if changes to value should be ignored.
        private bool ignoreValueChanges = false;

        // Action to take when this control looses focus
        private LostFocusAction lostFocusAction = LostFocusAction.None;
        // The selected item of the internal selector. Used to determine Value
        private object internalValue = null;
        // The string value of the internal selector if it allows text editing
        private string internalStringValue = String.Empty;

        // A Collection view over the ItemsSource that we have been passed
        private CollectionView collectionView = null;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ChoiceEditor()
        {
            this.CoerceValue(ChoiceEditor.UseItemTemplateForSelectionProperty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
 
        // <summary>
        // The currently selected choice
        // </summary>
        public object Value
        {
            get { return this.GetValue(ChoiceEditor.ValueProperty); }
            set { this.SetValue(ChoiceEditor.ValueProperty, value); }
        }

        // <summary>
        // The index of the selected choice. -1 if Value is not in the list.
        // </summary>
        public int ValueIndex
        {
            get { return (int)this.GetValue(ChoiceEditor.ValueIndexProperty); }
            set { this.SetValue(ChoiceEditor.ValueIndexProperty, value); }
        }

        // <summary>
        // The items source used to populate the choice
        // </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ChoiceEditor.ItemsSourceProperty); }
            set { this.SetValue(ChoiceEditor.ItemsSourceProperty, value); }
        }

        // <summary>
        // Converter used to convert text values to object values.
        // </summary>
        public TypeConverter Converter
        {
            get { return (TypeConverter)this.GetValue(ChoiceEditor.ConverterProperty); }
            set { this.SetValue(ChoiceEditor.ConverterProperty, value); }
        }

        // <summary>
        // Sets the view type of this choice. This will change the behavior of the choice.
        // </summary>
        public ChoiceEditorViewType ViewType
        {
            get { return (ChoiceEditorViewType)this.GetValue(ChoiceEditor.ViewTypeProperty); }
            set { this.SetValue(ChoiceEditor.ViewTypeProperty, value); }
        }

        // <summary>
        // Set to true if this choice editor should be editable. Currently only works for Combo
        // </summary>
        public bool IsEditable
        {
            get { return (bool)this.GetValue(ChoiceEditor.IsEditableProperty); }
            set { this.SetValue(ChoiceEditor.IsEditableProperty, value); }
        }

        // <summary>
        // The prefix put on all icon resource references before they are looked up
        // </summary>
        public string IconResourcePrefix
        {
            get { return (string)this.GetValue(ChoiceEditor.IconResourcePrefixProperty); }
            set { this.SetValue(ChoiceEditor.IconResourcePrefixProperty, value); }
        }

        // <summary>
        // The prefix put on all icon resource references before they are looked up
        // </summary>
        public string IconResourceSuffix
        {
            get { return (string)this.GetValue(ChoiceEditor.IconResourceSuffixProperty); }
            set { this.SetValue(ChoiceEditor.IconResourceSuffixProperty, value); }
        }

        // <summary>
        // True if this value editor is ninched
        // </summary>
        public bool IsNinched
        {
            get { return (bool)this.GetValue(ChoiceEditor.IsNinchedProperty); }
            set { this.SetValue(ChoiceEditor.IsNinchedProperty, value); }
        }

        // <summary>
        // True to show the full control instead of the optimized drawing of the control
        // </summary>
        public bool ShowFullControl
        {
            get { return (bool)this.GetValue(ChoiceEditor.ShowFullControlProperty); }
            set { this.SetValue(ChoiceEditor.ShowFullControlProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ChoiceEditor.ItemTemplateProperty); }
            set { this.SetValue(ChoiceEditor.ItemTemplateProperty, value); }
        }

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)this.GetValue(ChoiceEditor.ItemTemplateSelectorProperty); }
            set { this.SetValue(ChoiceEditor.ItemTemplateSelectorProperty, value); }
        }

        public bool UseItemTemplateForSelection
        {
            get { return (bool)this.GetValue(ChoiceEditor.UseItemTemplateForSelectionProperty); }
            set { this.SetValue(ChoiceEditor.UseItemTemplateForSelectionProperty, value); }
        }

        public double BorderCornerRadius
        {
            get { return (double)this.GetValue(ChoiceEditor.BorderCornerRadiusProperty); }
            set { this.SetValue(ChoiceEditor.BorderCornerRadiusProperty, value); }
        }

        public Thickness DropButtonInset
        {
            get { return (Thickness)this.GetValue(ChoiceEditor.DropButtonInsetProperty); }
            set { this.SetValue(ChoiceEditor.DropButtonInsetProperty, value); }
        }

        public Thickness TextAreaInset
        {
            get { return (Thickness)this.GetValue(ChoiceEditor.TextAreaInsetProperty); }
            set { this.SetValue(ChoiceEditor.TextAreaInsetProperty, value); }
        }

        public Brush DropButtonBrush
        {
            get { return (Brush)this.GetValue(ChoiceEditor.DropButtonBrushProperty); }
            set { this.SetValue(ChoiceEditor.DropButtonBrushProperty, value); }
        }

        public double InnerCornerRadius
        {
            get { return (double)this.GetValue(ChoiceEditor.InnerCornerRadiusProperty); }
            set { this.SetValue(ChoiceEditor.InnerCornerRadiusProperty, value); }
        }

        public ImageSource ButtonIcon
        {
            get { return (ImageSource)this.GetValue(ChoiceEditor.ButtonIconProperty); }
            set { this.SetValue(ChoiceEditor.ButtonIconProperty, value); }
        }

        public double IconWidth
        {
            get { return (double)this.GetValue(ChoiceEditor.IconWidthProperty); }
            set { this.SetValue(ChoiceEditor.IconWidthProperty, value); }
        }

        public double IconHeight
        {
            get { return (double)this.GetValue(ChoiceEditor.IconHeightProperty); }
            set { this.SetValue(ChoiceEditor.IconHeightProperty, value); }
        }

        // <summary>
        // Command fired when editing begins
        // </summary>
        public ICommand BeginCommand
        {
            get { return (ICommand)this.GetValue(ChoiceEditor.BeginCommandProperty); }
            set { this.SetValue(ChoiceEditor.BeginCommandProperty, value); }
        }

        // <summary>
        // Command fired when an edit value is updated. This command will fire after the
        // binding has been updated.
        // </summary>
        public ICommand UpdateCommand
        {
            get { return (ICommand)this.GetValue(ChoiceEditor.UpdateCommandProperty); }
            set { this.SetValue(ChoiceEditor.UpdateCommandProperty, value); }
        }

        // <summary>
        // Command fired when an edit is canceled
        // </summary>
        public ICommand CancelCommand
        {
            get { return (ICommand)this.GetValue(ChoiceEditor.CancelCommandProperty); }
            set { this.SetValue(ChoiceEditor.CancelCommandProperty, value); }
        }

        // <summary>
        // Command fired when an edit is commited
        // </summary>
        public ICommand CommitCommand
        {
            get { return (ICommand)this.GetValue(ChoiceEditor.CommitCommandProperty); }
            set { this.SetValue(ChoiceEditor.CommitCommandProperty, value); }
        }

        // <summary>
        // Command fired when the editor is done editing.  At this point the host
        // may decide to move on to the next property in the list, return focus
        // to the design surface, or perform any other action it pleases to do.
        // </summary>
        public ICommand FinishEditingCommand
        {
            get { return (ICommand)this.GetValue(ChoiceEditor.FinishEditingCommandProperty); }
            set { this.SetValue(ChoiceEditor.FinishEditingCommandProperty, value); }
        }

        // <summary>
        // True if cursor should be an hourglass when loading a ComboBox popup
        // </summary>
        public Cursor ComboBoxLoadingCursor
        {
            get { return (Cursor)this.GetValue(ChoiceEditor.ComboBoxLoadingCursorProperty); }
            set { this.SetValue(ChoiceEditor.ComboBoxLoadingCursorProperty, value); }
        }

        // <summary>
        // Command to rotate the selection of the ChoiceEditor to the next possible item
        // or cycle to the first if at the end of the current view.
        // </summary>
        public ICommand NextValueCommand
        {
            get
            {
                return new DelegateCommand(new DelegateCommand.SimpleEventHandler(SelectNextValue));
            }
        }

        // <summary>
        // Command to rotate the selection of the ChoiceEditor the previously selected
        // item in the view, or to the last item in the view if at the first item in the view.
        // </summary>
        public ICommand PreviousValueCommand
        {
            get
            {
                return new DelegateCommand(new DelegateCommand.SimpleEventHandler(SelectPreviousValue));
            }
        }

        public bool ForceBinding
        {
            get { return (bool)this.GetValue(ChoiceEditor.ForceBindingProperty); }
            set { this.SetValue(ChoiceEditor.ForceBindingProperty, value); }
        }

        // <summary>
        // This is the value internal to this control. Controls in the
        // template of this control should bind to this.
        // </summary>
        public object InternalValue
        {
            get
            {
                return this.internalValue;
            }
            set
            {
                if (this.internalValue != value)
                {
                    this.internalValue = value;
                    if (this.ShouldCommitInternalValueChanges)
                    {
                        if (!this.isTextEditing)
                        {
                            this.CommitChange();
                        }
                        else
                        {
                            this.lostFocusAction = LostFocusAction.Commit;
                        }
                    }
                    this.SendPropertyChanged("InternalValue");
                }
            }
        }

        public string InternalStringValue
        {
            get { return this.internalStringValue; }
            set
            {
                if (!String.Equals(value, this.internalStringValue))
                {
                    if (this.ShouldCommitInternalStringValueChanges && this.isTextEditing)
                    {
                        this.InternalValue = null;
                        this.lostFocusAction = LostFocusAction.Commit;
                    }
                    this.internalStringValue = value;
                    this.SendPropertyChanged("InternalStringValue");
                }
            }
        }

        public bool InternalIsSelectingValue
        {
            get { return this.isSelectingValue; }
            set
            {
                if (this.isSelectingValue != value)
                {
                    this.isSelectingValue = value;

                    if (this.isTextEditing && !this.isSelectingValue)
                    {
                        //  
                        //                          * FinishedEditingCommand in Cider does not 
                        //                          * move focus off of the control (as we do in Sparkle) The fix is to add code to 
                        //                          * InternalIsSelectingValue when the value goes to false (when the popup is closing) 
                        //                          * that checks to see if there is a lostFocusAction of Commit and if so calls CommitChange()//                          
                        LostFocusAction oldAction = this.lostFocusAction;
                        this.lostFocusAction = LostFocusAction.None;
                        if (oldAction == LostFocusAction.Commit)
                        {
                            this.CommitChange();
                        }
                        this.OnFinishEditing();
                    }
                    if (this.isSelectingValue && this.CollectionView != null)
                    {
                        this.BeginIgnoreExternalValueChangeBlock();
                        try
                        {
                            this.ValueIndex = this.CollectionView.IndexOf(this.Value);
                        }
                        finally
                        {
                            this.EndIgnoreExternalValueChangeBlock();
                        }
                    }
                    Cursor loadingCursor = this.ComboBoxLoadingCursor;
                    if (value && this.ViewType == ChoiceEditorViewType.Combo && loadingCursor != null)
                    {
                        bool foundPopup = false;
                        Mouse.OverrideCursor = loadingCursor;
                        ComboBox comboBox = this.Template.FindName("PART_Combo", this) as ComboBox;
                        if (comboBox != null)
                        {
                            Popup popup = comboBox.Template.FindName("PART_Popup", comboBox) as Popup;
                            if (popup != null)
                            {
                                foundPopup = true;
                                popup.Opened += new EventHandler(OnPopupLoaded);
                            }
                        }
                        if (!foundPopup)
                        {
                            // if we couldn't set up the event handler, just return
                            // the cursor now so they aren't stuck with an hourglass
                            Mouse.OverrideCursor = null;
                        }
                    }
                }
            }
        }

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
        // ###################################################

        private CollectionView CollectionView
        {
            get {
                if (collectionView == null)
                {
                    IEnumerable newItems = this.ItemsSource;
                    if (newItems != null)
                    {
                        this.collectionView = new CollectionView(this.ItemsSource);
                    }
                    else
                    {
                        this.collectionView = null;
                    }
                }
                return collectionView;
            }
        }

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
        // ###################################################

        private bool ShouldCommitInternalValueChanges
        {
            get { return this.internalChangeLockCount == 0; }
        }

        private bool ShouldCommitInternalStringValueChanges
        {
            get { return this.internalStringValueChangeLockCount == 0; }
        }

        private bool ShouldIgnoreExternalValueChanges
        {
            get { return this.ignoreValueChanges; }
        }


        public void SelectNextValue()
        {
            int currentIndex = this.ValueIndex;

            Fx.Assert(this.CollectionView != null, "ChoiceEditor CollectionView cannot be null.");
            if (currentIndex < 0 || currentIndex >= this.CollectionView.Count - 1)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex = currentIndex + 1;
            }
            this.ValueIndex = currentIndex;
        }

        public void SelectPreviousValue()
        {
            int currentIndex = this.ValueIndex;

            Fx.Assert(this.CollectionView != null, "ChoiceEditor CollectionView cannot be null.");
            if (currentIndex <= 0 || currentIndex > this.CollectionView.Count - 1)
            {
                currentIndex = this.CollectionView.Count - 1;
            }
            else
            {
                currentIndex = currentIndex - 1;
            }
            this.ValueIndex = currentIndex;
        }

        public ImageSource GetIconAsImageSource(object key, object parameter)
        {
            // This is the place to cache icons if these lookups are costing us too much.
            try
            {
                StringBuilder sb = new StringBuilder();
                string prefix = this.IconResourcePrefix;
                string suffix = this.IconResourceSuffix;
                string parameterString = parameter as string;

                if (prefix != null)
                {
                    sb.Append(prefix);
                }
                sb.Append(key.ToString());
                if (suffix != null)
                {
                    sb.Append(suffix);
                }
                if (parameterString != null)
                {
                    sb.Append(parameterString);
                }

                object resource = this.FindResource(sb.ToString());
                ImageSource resourceImageSource = resource as ImageSource;
                return resourceImageSource;
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                return null;
            }
        }

        private void OnPopupLoaded(object sender, EventArgs e)
        {
            Popup popup = sender as Popup;

            Mouse.OverrideCursor = null;
            if (popup != null)
            {
                popup.Opened -= new EventHandler(OnPopupLoaded);
            }
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Make sure the value actually changed
            if (!object.Equals(e.OldValue, e.NewValue))
            {
                ChoiceEditor choice = d as ChoiceEditor;
                if (choice != null && !choice.ShouldIgnoreExternalValueChanges)
                {
                    choice.UpdateInternalValuesFromValue();
                }
            }
        }

        private static void ValueIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChoiceEditor choice = d as ChoiceEditor;
            if (choice != null && !choice.ShouldIgnoreExternalValueChanges)
            {
                choice.UpdateInternalValuesFromValueIndex();
                choice.UpdateValueFromInternalValues();
            }
        }

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChoiceEditor choice = d as ChoiceEditor;
            if (choice != null)
            {
                choice.ItemsSourceChanged();
            }
        }

        private static void IsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Make sure that the internal values are up to date now that we know we are editable or not.
            ChoiceEditor choice = d as ChoiceEditor;
            if (choice != null && !choice.ShouldIgnoreExternalValueChanges)
            {
                choice.UpdateInternalValuesFromValue();
            }
        }

        private static void IsNinchedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChoiceEditor choice = d as ChoiceEditor;
            if (choice != null && !choice.ShouldIgnoreExternalValueChanges)
            {
                choice.UpdateInternalValuesFromValue();
            }
        }

        private static object CoerceShowFullControl(DependencyObject target, object value)
        {
            ChoiceEditor choice = target as ChoiceEditor;
            if (choice != null && value is bool)
            {
                bool boolValue = (bool)value;

                // ###################################################
                // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
                // ###################################################

                choice.isShowingFullEditor = boolValue;
                choice.CheckUpdateValueIndex(false);

                // ###################################################
                // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
                // ###################################################

                // If we are text editing force the full control to stay showing.
                if (!boolValue)
                {
                    return choice.isTextEditing;
                }
            }

            return value;
        }

        private static void ItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChoiceEditor choice = d as ChoiceEditor;
            if (choice != null)
            {
                choice.CoerceValue(ChoiceEditor.UseItemTemplateForSelectionProperty);
            }
        }

        private static void ItemTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ChoiceEditor choice = d as ChoiceEditor;
            if (choice != null)
            {
                choice.CoerceValue(ChoiceEditor.UseItemTemplateForSelectionProperty);
            }
        }

        private static object CoerceUseItemTemplateForSelection(DependencyObject target, object value)
        {
            ChoiceEditor choice = target as ChoiceEditor;
            if (choice != null)
            {
                if (value == null)
                {
                    return choice.ItemTemplate != null || choice.ItemTemplateSelector != null;
                }
            }
            return value;
        }

        private void SendPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewGotKeyboardFocus(e);
            FrameworkElement element = e.NewFocus as FrameworkElement;
            if (element != null && element.Name.Equals("PART_EditableTextBox", StringComparison.Ordinal))
            {
                this.isTextEditing = true;
            }
        }

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
        // ###################################################
        // CIDER needs to override the Keydown behavior : 
        //   
        //          * The problem here is just a difference in expectation for control behavior 
        //          * between the two products (CIDER AND BLEND). The fix is entirely in OnPreviewKeyDown 
        //          * a.    Remove the condition that we handle navigation keys only when the popup is open
        //          * b.    Add logic that forces a commit when a navigation happens (you could optionally only do the commit when the popup is closed, but I�m not sure what the desired behavior is here). You may or may not want to limit this behavior to editable ChoiceEditors as well.//          

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            bool commitChange = false;
            bool finishEditing = false;

            bool markHandled = ValueEditorUtils.GetHandlesCommitKeys(this);

            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                e.Handled |= markHandled;

                commitChange = true;

                finishEditing = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0;
                //Hide dropdown on excape key
                HideDropDown();
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled |= markHandled;

                LostFocusAction savedAction = this.lostFocusAction;
                this.lostFocusAction = LostFocusAction.None;
                if (savedAction != LostFocusAction.None)
                {
                    this.CancelChange();
                }

                //Hide dropdown on excape key
                HideDropDown();
                this.OnFinishEditing();
            }
            // Only pay attention to navigation keys if we are selecting a value from the popup
            if (this.CollectionView != null && !this.CollectionView.IsEmpty)
            {
                bool navigated = false;
                if (e.Key == Key.Up || (!this.IsEditable && e.Key == Key.Left))
                {
                    this.SelectPreviousValue();
                    navigated = true;
                }
                else if (e.Key == Key.Down || (!this.IsEditable && e.Key == Key.Right))
                {
                    this.SelectNextValue();
                    navigated = true;
                }
                else if (!this.IsEditable && e.Key == Key.Home)
                {
                    this.ValueIndex = 0;
                    navigated = true;
                }
                else if (!this.IsEditable && e.Key == Key.End)
                {
                    this.ValueIndex = this.CollectionView.Count - 1;
                    navigated = true;
                }

                if (navigated)
                {
                    this.lostFocusAction = LostFocusAction.Commit;
                    e.Handled = true;
                    ComboBox comboBox = this.Template.FindName("_comboChoiceEditor", this) as ComboBox;
                    if (!comboBox.IsDropDownOpen)
                    {
                        commitChange = true;
                    }
                }
            }

            if (commitChange)
            {
                LostFocusAction savedAction = this.lostFocusAction;
                this.lostFocusAction = LostFocusAction.None;
                if (savedAction == LostFocusAction.Commit)
                {
                    this.CommitChange();
                }
            }
            if (finishEditing)
            {
                this.OnFinishEditing();
            }
            base.OnPreviewKeyDown(e);
        }

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
        // ###################################################

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
        // ###################################################
        private void HideDropDown()
        {
            ComboBox comboBox = this.Template.FindName("_comboChoiceEditor", this) as ComboBox;
            if (comboBox != null && comboBox.IsDropDownOpen)
            {
                comboBox.IsDropDownOpen = false;
            }
        }

        // When the focus is lost by clicking ouside the choice-editor
        // we dont get a preview message.
        // The string edtior has OnLostKeyboardFocus and hence we imitate the 
        // same behavior.
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            FrameworkElement element = e.OldFocus as FrameworkElement;
            if (element != null && element.Name.Equals("PART_EditableTextBox", StringComparison.Ordinal))
            {
                this.HandleLostFocus();
            }
        }
        // #################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
        // #################################################


        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewLostKeyboardFocus(e);
            FrameworkElement element = e.OldFocus as FrameworkElement;
            if (element != null && element.Name.Equals("PART_EditableTextBox", StringComparison.Ordinal))
            {
                this.HandleLostFocus();
            }
        }

        private void HandleLostFocus()
        {
            if (this.isTextEditing)
            {
                LostFocusAction oldLostFocusAction = this.lostFocusAction;
                this.lostFocusAction = LostFocusAction.None;
                this.isTextEditing = false;


                if (oldLostFocusAction == LostFocusAction.Commit)
                {
                    this.CommitChange();
                }
                else if (oldLostFocusAction == LostFocusAction.Cancel)
                {
                    this.CancelChange();
                }

                this.CoerceValue(ChoiceEditor.ShowFullControlProperty);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (this.ViewType == ChoiceEditorViewType.Combo && !this.ShowFullControl)
            {
                double borderCornerRadius = this.BorderCornerRadius;
                Thickness dropButtonInset = this.DropButtonInset;
                Thickness textAreaInset = this.TextAreaInset;
                Brush dropButtonBrush = this.DropButtonBrush;
                double innerCornerRadius = this.InnerCornerRadius;
                ImageSource buttonIcon = this.ButtonIcon;
                double iconWidth = this.IconWidth;
                double iconHeight = this.IconHeight;

                // Draw something that looks like an Expression combo
                Rect fullRect = new Rect(0d, 0d, this.ActualWidth, this.ActualHeight);

                if (RenderUtils.DrawInscribedRoundedRect(drawingContext, this.BorderBrush, null, fullRect, borderCornerRadius))
                {
                    Rect innerRect = RenderUtils.CalculateInnerRect(fullRect, 0);
                    double dropButtonLeft = (innerRect.Right > textAreaInset.Right ? innerRect.Right - textAreaInset.Right : 0d) + dropButtonInset.Left;
                    double dropButtonTop = innerRect.Top + dropButtonInset.Top;
                    double dropButtonRight = innerRect.Right - dropButtonInset.Right;
                    double dropButtonBottom = innerRect.Bottom - dropButtonInset.Bottom;

                    RenderUtils.DrawInscribedRoundedRect(drawingContext, dropButtonBrush, null, new Rect(new Point(dropButtonLeft, dropButtonTop), new Point(dropButtonRight, dropButtonBottom)), innerCornerRadius);
                    if (buttonIcon != null)
                    {
                        double buttonCenterX = dropButtonLeft + (dropButtonRight - dropButtonLeft) / 2d;
                        double buttonCenterY = dropButtonTop + (dropButtonBottom - dropButtonTop) / 2d;
                        drawingContext.DrawImage(buttonIcon, new Rect(new Point(buttonCenterX - iconWidth / 2d, buttonCenterY - iconHeight / 2d), new Point(buttonCenterX + iconWidth / 2d, buttonCenterY + iconHeight / 2d)));
                    }

                    double textAreaLeft = innerRect.Left + textAreaInset.Left;
                    double textAreaTop = innerRect.Top + textAreaInset.Top;
                    double textAreaRight = innerRect.Right > textAreaInset.Right ? innerRect.Right - textAreaInset.Right : textAreaLeft;
                    double textAreaBottom = innerRect.Bottom - textAreaInset.Bottom;

                    RenderUtils.DrawInscribedRoundedRect(drawingContext, this.Background, null, new Rect(new Point(textAreaLeft, textAreaTop), new Point(textAreaRight, textAreaBottom)), innerCornerRadius);
                }
            }
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            this.BeginNoCommitInternalValueChangeBlock();
            // WORKAROUND Turn off bindings on the internal combo while the template is udpating. This works around Avalon bug: 1756023
            this.ForceBinding = false;
            base.OnTemplateChanged(oldTemplate, newTemplate);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // WORKAROUND Force the bindings on our internal combo (if there is one) to update. This works around Avalon bug: 1756023
            this.ForceBinding = true;

            this.EndNoCommitInternalValueChangeBlock();

            this.HandleLostFocus();
        }

        private void CommitChange()
        {
            ValueEditorUtils.ExecuteCommand(this.BeginCommand, this, null);

            if (this.UpdateValueFromInternalValues())
            {
                // We need to update both the binding on the value property and the valueindex property
                // so that the value does not bounce around when we commit.
                ValueEditorUtils.UpdateBinding(this, ChoiceEditor.ValueProperty, UpdateBindingType.Source);
                ValueEditorUtils.UpdateBinding(this, ChoiceEditor.ValueIndexProperty, UpdateBindingType.Source);
                ValueEditorUtils.ExecuteCommand(this.CommitCommand, this, null);
                ValueEditorUtils.UpdateBinding(this, ChoiceEditor.ValueProperty, UpdateBindingType.Target);
                ValueEditorUtils.UpdateBinding(this, ChoiceEditor.ValueIndexProperty, UpdateBindingType.Target);
            }
            else
            {
                this.CancelStartedChange();
            }

            this.lostFocusAction = LostFocusAction.None;
        }

        private void CancelChange()
        {
            ValueEditorUtils.ExecuteCommand(this.BeginCommand, this, null);
            this.CancelStartedChange();
        }

        private void CancelStartedChange()
        {
            // Revert External values
            ValueEditorUtils.UpdateBinding(this, ChoiceEditor.ValueProperty, UpdateBindingType.Target);
            ValueEditorUtils.UpdateBinding(this, ChoiceEditor.ValueIndexProperty, UpdateBindingType.Target);
            this.UpdateInternalValuesFromValue();
            ValueEditorUtils.ExecuteCommand(this.CancelCommand, this, null);
        }

        private void OnFinishEditing()
        {
            ICommand finishedEditingCommand = this.FinishEditingCommand;
            if (finishedEditingCommand != null)
            {
                ValueEditorUtils.ExecuteCommand(finishedEditingCommand, this, null);
            }
            else
            {
                Keyboard.Focus(null);
            }
        }

        private void ItemsSourceChanged()
        {
            // The collection just changed, so we need to make sure that things are in [....]

            // ###################################################
            // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
            // ###################################################
            this.collectionView = null;
            // ###################################################
            // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - end
            // ###################################################

            this.UpdateInternalValuesFromValue();
            this.UpdateValueFromInternalValues();
        }

        private int IndexOf(object item)
        {
            if (this.CollectionView != null)
            {
                return this.CollectionView.IndexOf(item);
            }
            else
            {
                return -1;
            }
        }

        private object GetItemAt(int index)
        {
            if (this.CollectionView != null)
            {
                return this.CollectionView.GetItemAt(index);
            }
            else
            {
                return null;
            }
        }

        protected void UpdateInternalValuesFromValue()
        {
            this.BeginNoCommitInternalValueChangeBlock();
            this.BeginNoCommitInternalStringValueChangeBlock();
            try
            {
                if (!this.IsNinched)
                {
                    this.InternalValue = this.Value;
                }
                else
                {
                    this.InternalValue = null;
                }

                if (this.IsEditable)
                {
                    string newStringValue = String.Empty;
                    if (this.InternalValue != null && !this.IsNinched)
                    {
                        TypeConverter converter = this.Converter;
                        if (converter != null && converter.CanConvertFrom(this.InternalValue.GetType()))
                        {
                            newStringValue = converter.ConvertToString(this.InternalValue);
                        }
                        else
                        {
                            newStringValue = this.InternalValue.ToString();
                        }
                    }

                    this.InternalStringValue = newStringValue;
                }
            }
            finally
            {
                this.EndNoCommitInternalStringValueChangeBlock();
                this.EndNoCommitInternalValueChangeBlock();
            }
        }

        public void UpdateInternalValuesFromValueIndex()
        {
            this.BeginNoCommitInternalValueChangeBlock();
            this.BeginNoCommitInternalStringValueChangeBlock();
            try
            {
                this.InternalValue = this.GetItemAt(this.ValueIndex);
            }
            finally
            {
                this.EndNoCommitInternalStringValueChangeBlock();
                this.EndNoCommitInternalValueChangeBlock();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        protected bool UpdateValueFromInternalValues()
        {
            this.BeginIgnoreExternalValueChangeBlock();
            try
            {
                if (!this.IsEditable)
                {
                    this.Value = this.InternalValue;
                }
                else
                {
                    if (this.InternalValue != null)
                    {
                        this.Value = this.InternalValue;
                    }
                    else
                    {
                        string stringValue = this.InternalStringValue;
                        // Try to parse a new value from the string value
                        if (stringValue != null)
                        {
                            TypeConverter converter = this.Converter;
                            object newValue = null;
                            if (converter != null)
                            {
                                try
                                {
                                    newValue = converter.ConvertFromString(stringValue);
                                }
                                catch (Exception)
                                {
                                    // Have to catch Exception here because Various of these converters (e.g. DoubleConverter) will throw it.
                                    // Do nothing since newValue should still be null;
                                    return false;
                                }
                            }
                            else
                            {
                                newValue = stringValue;
                            }

                            this.Value = newValue;

                            // At this point it is possible that the value that we just set is out of [....] with the internal value
                            if (newValue != this.InternalValue)
                            {
                                this.UpdateInternalValuesFromValue();
                            }
                        }
                    }

                }
            }
            finally
            {
                this.EndIgnoreExternalValueChangeBlock();
            }

            CheckUpdateValueIndex(true);
            return true;
        }

        protected void BeginNoCommitInternalValueChangeBlock()
        {
            this.internalChangeLockCount++;
        }

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
        // ###################################################

        // <summary>
        // Updates the value index for the current value, but
        // only if necessary.
        // </summary>
        private void CheckUpdateValueIndex(bool sourceChanged)
        {

            BeginIgnoreExternalValueChangeBlock();
            try
            {
                // Check if we need to update the value index.
                // We don't need to if we're a combo box and
                // we're not showing the drop-down.

                if (ViewType != ChoiceEditorViewType.Combo || this.isShowingFullEditor)
                {
                    if (sourceChanged || ReadLocalValue(ValueIndexProperty) == DependencyProperty.UnsetValue)
                    {
                        ValueIndex = IndexOf(Value);
                    }
                }
                else
                {
                    ClearValue(ValueIndexProperty);
                }
            }
            finally
            {
                EndIgnoreExternalValueChangeBlock();
            }
        }

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
        // ###################################################

        protected void EndNoCommitInternalValueChangeBlock()
        {
            this.internalChangeLockCount--;
            Fx.Assert(this.internalChangeLockCount >= 0, "internalChangeLockCount should be positive");
        }

        protected void BeginNoCommitInternalStringValueChangeBlock()
        {
            this.internalStringValueChangeLockCount++;
        }

        protected void EndNoCommitInternalStringValueChangeBlock()
        {
            this.internalStringValueChangeLockCount--;
            Fx.Assert(this.internalStringValueChangeLockCount >= 0, "internalStringValueChangeLockCount should be positive");
        }

        protected void BeginIgnoreExternalValueChangeBlock()
        {
            Fx.Assert(this.ignoreValueChanges == false, "ignoreValueChanges should be false");
            this.ignoreValueChanges = true;
        }

        protected void EndIgnoreExternalValueChangeBlock()
        {
            Fx.Assert(this.ignoreValueChanges == true, "ignoreValueChanges should be false");
            this.ignoreValueChanges = false;
        }


        enum LostFocusAction
        {
            None,
            Commit,
            Cancel
        }

    }
}
