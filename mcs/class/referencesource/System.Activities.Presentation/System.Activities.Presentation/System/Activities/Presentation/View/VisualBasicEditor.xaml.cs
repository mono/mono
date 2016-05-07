//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.ExpressionParser;
    using System.Activities.Expressions;
    using System.Activities.Presentation.Expressions;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Validation;
    using System.Activities.Presentation.Xaml;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Microsoft.VisualBasic.Activities;
    using Microsoft.Activities.Presentation;

    //This control is used for expression editing in activity designers.
    //It uses the Activity<T> TypeConverter to convert between a Activity<T> and its string representation.
    //It defines 3 dependency properties - OwnerActivity, ExpressionModelItem and ExpressionType.
    //ActivityModelItem is used to create the parser context required by the TypeConverter.
    //ExpressionType is the type of the expression associated with this text box. This is required by the TypeConverter.

    internal sealed partial class VisualBasicEditor : TextualExpressionEditor
    {
        private static readonly Type VariableValueType = typeof(VariableValue<>);
        private static readonly Type VariableReferenceType = typeof(VariableReference<>);
        private static readonly Type LiteralType = typeof(Literal<>);
        private static readonly Type VisualBasicValueType = typeof(VisualBasicValue<>);
        private static readonly Type VisualBasicReferenceType = typeof(VisualBasicReference<>);

        internal static string ExpressionLanguageName = (new VisualBasicValue<string>() as ITextExpression).Language;

        internal static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(VisualBasicEditor),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTextChanged), new CoerceValueCallback(OnTextCoerceValue)));

        internal static readonly DependencyProperty ValidationStateProperty = DependencyProperty.Register("ValidationState", typeof(ValidationState), typeof(VisualBasicEditor),
                new FrameworkPropertyMetadata(ValidationState.Valid));

        internal static readonly DependencyProperty EditingStateProperty = DependencyProperty.Register("EditingState", typeof(EditingState), typeof(VisualBasicEditor),
                new PropertyMetadata(EditingState.Idle));

        internal static readonly DependencyProperty HasValidationErrorProperty = DependencyProperty.Register("HasValidationError", typeof(bool), typeof(VisualBasicEditor),
                new PropertyMetadata(false));

        internal static readonly DependencyProperty ValidationErrorMessageProperty = DependencyProperty.Register("ValidationErrorMessage", typeof(string), typeof(VisualBasicEditor),
                new PropertyMetadata(null));

        internal static readonly DependencyProperty ExpressionTextProperty = DependencyProperty.Register("ExpressionText", typeof(string), typeof(VisualBasicEditor),
                new PropertyMetadata(null));

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CompleteWordCommand = new RoutedCommand("CompleteWordCommand", typeof(VisualBasicEditor));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand GlobalIntellisenseCommand = new RoutedCommand("GlobalIntellisenseCommand", typeof(VisualBasicEditor));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ParameterInfoCommand = new RoutedCommand("ParameterInfoCommand", typeof(VisualBasicEditor));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand QuickInfoCommand = new RoutedCommand("QuickInfoCommand", typeof(VisualBasicEditor));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand IncreaseFilterLevelCommand = new RoutedCommand("IncreaseFilterLevelCommand", typeof(VisualBasicEditor));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand DecreaseFilterLevelCommand = new RoutedCommand("DecreaseFilterLevelCommand", typeof(VisualBasicEditor));

        bool internalModelItemChange = false;
        string previousText = null;
        ModelProperty expressionModelProperty;
        TypeConverter expressionConverter;
        bool initialized = false;
        bool isEditorLoaded = false;

        IExpressionEditorService expressionEditorService;
        IExpressionEditorInstance expressionEditorInstance;
        TextBox editingTextBox;

        Control hostControl;
        string editorName;
        double blockHeight = double.NaN;
        double blockWidth = double.NaN;
        bool isExpressionLoaded = false;
        bool isBeginEditPending = false;

        DesignerPerfEventProvider perfProvider;
        ModelItem boundedExpression = null;
        BackgroundWorker validator = null;
        const int ValidationWaitTime = 800;

        PropertyChangedEventHandler onExpressionModelItemChangedHandler;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "This is recommended way by WPF to override metadata of DependencyProperty in derived class")]
        static VisualBasicEditor()
        {
            ExpressionActivityEditor.HintTextProperty.OverrideMetadata(typeof(VisualBasicEditor), new FrameworkPropertyMetadata(SR.ExpressionDefaultText));
        }

        public VisualBasicEditor()
        {
            InitializeComponent();

            this.MinHeight = this.FontSize + 4; /* 4 pixels for border*/

            this.editorName = null;

            this.ContentTemplate = (DataTemplate)FindResource("textblock");
            this.Loaded += this.OnExpressionTextBoxLoaded;
            this.Unloaded += this.OnExpressionTextBoxUnloaded;
        }

        PropertyChangedEventHandler OnExpressionModelItemChanged
        {
            get
            {
                if (this.onExpressionModelItemChangedHandler == null)
                {
                    this.onExpressionModelItemChangedHandler = new PropertyChangedEventHandler(this.expressionModelItem_PropertyChanged);
                }

                return this.onExpressionModelItemChangedHandler;
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "HintText":
                    this.OnHintTextChanged(e);
                    break;
                case "Expression":
                    this.OnExpressionChanged(e);
                    break;
                case "ExpressionType":
                    this.OnExpressionTypeChanged(e);
                    break;
                case "OwnerActivity":
                    this.OnOwnerModelItemChanged(e);
                    break;
                case "UseLocationExpression":
                    this.OnUseLocationExpressionChanged(e);
                    break;
                case "PathToArgument":
                    this.OnPathToArgumentChanged(e);
                    break;
                case "MaxLines":
                case "MinLines":
                    this.OnLinesChanged(e);
                    break;
                case "AcceptsReturn":
                    this.OnAcceptsReturnChanged(e);
                    break;
                case "AcceptsTab":
                    this.OnAcceptsTabChanged(e);
                    break;
                case "IsIndependentExpressionProperty":
                    this.OnIsIndependentExpressionChanged();
                    break;
            }
            base.OnPropertyChanged(e);
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);
            e.Handled = true;
        }

        static object OnTextCoerceValue(DependencyObject dp, object value)
        {
            string tempText = value as string;
            VisualBasicEditor etb = dp as VisualBasicEditor;
            if (etb != null)
            {
                if (tempText != null)
                {
                    tempText = tempText.Trim();
                }
            }
            return tempText;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new VisualBasicEditorAutomationPeer(this);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if (this.expressionEditorInstance != null &&
            (this.expressionEditorInstance.HasAggregateFocus ||
             (this.hostControl != null && this.hostControl.IsFocused)))
            {
                return;
            }

            DoLostFocus();
        }

        private void DoLostFocus()
        {
            KillValidator();

            ValidateExpression(this);

            if (this.Context != null)
            {   // Unselect if this is the currently selected one.
                ExpressionSelection current = this.Context.Items.GetValue<ExpressionSelection>();
                if (current != null && current.ModelItem == this.Expression)
                {
                    ExpressionSelection emptySelection = new ExpressionSelection(null);
                    this.Context.Items.SetValue(emptySelection);
                }
            }

            // Generate and validate the expression.
            // Get the text from the snapshot and set it to the Text property
            if (this.expressionEditorInstance != null)
            {
                this.expressionEditorInstance.ClearSelection();
            }

            bool committed = false;
            if (!this.ExplicitCommit)
            {
                //commit change and let the commit change code do the revert
                committed = Commit(false);

                //reset the error icon if we didn't get to set it in the commit
                if (!committed || this.IsIndependentExpression)
                {
                    this.EditingState = EditingState.Idle;
                    // Switch the control back to a textbox -
                    // but give it the text from the editor (textbox should be bound to the Text property, so should
                    // automatically be filled with the correct text, from when we set the Text property earlier)
                    if (!this.ContentTemplate.Equals((DataTemplate)FindResource("textblock")))
                    {
                        this.ContentTemplate = (DataTemplate)FindResource("textblock");
                    }
                }
            }

            //raise EditorLostLogical focus - in case when some clients need to do explicit commit
            this.RaiseEvent(new RoutedEventArgs(ExpressionTextBox.EditorLostLogicalFocusEvent, this));
        }

        private void KillValidator()
        {
            if (validator != null)
            {
                this.validator.CancelAsync();
                this.validator.Dispose();
                this.validator = null;
            }
        }

        internal static bool ShouldGenerateExpression(string oldText, string newText)
        {
            return newText != null && !string.Equals(newText, oldText) && !(oldText == null && newText.Equals(string.Empty));
        }

        public override bool Commit(bool isExplicitCommit)
        {
            bool committed = false;
            //only generate and validate the expression when when we don't require explicit commit change
            //or when the commit is explicit
            if (!this.ExplicitCommit || isExplicitCommit)
            {
                // Generate and validate the expression.
                // Get the text from the snapshot and set it to the Text property
                this.previousText = null;
                // In VS
                if (this.expressionEditorInstance != null)
                {
                    this.previousText = this.Text;
                    this.Text = this.expressionEditorInstance.GetCommittedText();
                }
                // In rehost
                else
                {
                    if (this.Expression != null)
                    {
                        Activity expression = this.Expression.GetCurrentValue() as Activity;
                        // if expression is null, GetExpressionString will return null                           
                        this.previousText = ExpressionHelper.GetExpressionString(expression, this.OwnerActivity);
                    }
                    else
                    {
                        this.previousText = null;
                    }

                    if (this.editingTextBox != null)
                    {
                        this.editingTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                    }
                }

                // If the Text is null, or equal to the previous value, or changed from null to empty, don't bother generating the expression
                // We still need to generate the expression when it is changed from other value to EMPTY however - otherwise
                // the case where you had an expression (valid or invalid), then deleted the whole thing will not be evaluated.
                if (ShouldGenerateExpression(this.previousText, this.Text))
                {
                    GenerateExpression();
                    committed = true;
                }
            }
            if (!this.ContentTemplate.Equals((DataTemplate)FindResource("textblock")))
            {
                this.ContentTemplate = (DataTemplate)FindResource("textblock");
            }
            return committed;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (this.Context != null)
            {
                ExpressionSelection expressionSelection = new ExpressionSelection(this.Expression);
                this.Context.Items.SetValue(expressionSelection);
            }
        }

        public override void BeginEdit()
        {
            //am i loaded? is current template a textblock?
            if (this.isExpressionLoaded || null == this.Expression)
            {
                this.isBeginEditPending = false;
                this.IsReadOnly = false;
                if (this.IsLoaded && this.ContentTemplate.Equals(this.FindResource("textblock")))
                {
                    //get control's content presenter
                    ContentPresenter presenter = VisualTreeUtils.GetTemplateChild<ContentPresenter>(this);
                    if (null != presenter)
                    {
                        //and look for the loaded textblock
                        TextBlock tb = (TextBlock)this.ContentTemplate.FindName("expresionTextBlock", presenter);
                        if (null != tb)
                        {
                            //now - give focus to the textblock - it will trigger OnGotTextBlockFocus event, which eventually
                            //swithc ETB into expression editing mode.
                            tb.Focus();
                        }
                    }
                }
            }
            else
            {
                this.isBeginEditPending = true;
            }
        }

        internal bool HasAggregateFocus()
        {
            bool result = false;

            if (this.IsLoaded)
            {
                if (this.expressionEditorInstance != null)
                {
                    result = (this.hostControl != null && this.hostControl.IsFocused) || this.expressionEditorInstance.HasAggregateFocus;
                }
                else
                {
                    result = !this.IsKeyboardFocused && this.IsKeyboardFocusWithin;
                }
            }

            return result;
        }

        void OnTextBlockMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (!this.IsReadOnly)
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock != null)
                {
                    Keyboard.Focus(textBlock);
                    e.Handled = true;
                }
            }
        }

        void OnGotTextBlockFocus(object sender, RoutedEventArgs e)
        {
            if (this.Context == null)
            {
                return;
            }

            DesignerView designerView = this.Context.Services.GetService<DesignerView>();

            if (!designerView.IsMultipleSelectionMode)
            {
                TextBlock textBlock = sender as TextBlock;
                bool isInReadOnlyMode = this.IsReadOnly;
                if (this.Context != null)
                {
                    ReadOnlyState readOnlyState = this.Context.Items.GetValue<ReadOnlyState>();
                    isInReadOnlyMode |= readOnlyState.IsReadOnly;
                }
                if (null != textBlock)
                {
                    this.blockHeight = textBlock.ActualHeight;
                    this.blockHeight = Math.Max(this.blockHeight, textBlock.MinHeight);
                    this.blockHeight = Math.Min(this.blockHeight, textBlock.MaxHeight);
                    this.blockWidth = textBlock.ActualWidth;
                    this.blockWidth = Math.Max(this.blockWidth, textBlock.MinWidth);
                    this.blockWidth = Math.Min(this.blockWidth, textBlock.MaxWidth);

                    // If it's already an editor, don't need to switch it/reload it (don't create another editor/grid if we don't need to)
                    // Also don't create editor when we are in read only mode
                    if (this.ContentTemplate.Equals((DataTemplate)FindResource("textblock")) && !isInReadOnlyMode)
                    {
                        if (this.Context != null)
                        {
                            // Get the ExpressionEditorService
                            this.expressionEditorService = this.Context.Services.GetService<IExpressionEditorService>();
                        }

                        // If the service exists, use the editor template - else switch to the textbox template
                        if (this.expressionEditorService != null)
                        {
                            this.PerfProvider.WorkflowDesignerExpressionEditorLoadStart();
                            this.ContentTemplate = (DataTemplate)FindResource("editor");
                        }
                        else
                        {
                            this.ContentTemplate = (DataTemplate)FindResource("textbox");
                        }
                    }
                }

                if (!isInReadOnlyMode)
                {
                    //disable the error icon
                    this.StartValidator();
                    this.EditingState = EditingState.Editing;
                    e.Handled = true;
                }
            }
        }

        void OnGotEditingFocus(object sender, RoutedEventArgs e)
        {
            //disable the error icon
            this.EditingState = EditingState.Editing;
            this.StartValidator();
        }

        // This method is called when the editor data template is loaded - when the editor data template
        // is loaded, create the editor session and the expression editor
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "CreateExpressionEditor is part of a public API. Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "CreateExpressionEditor is part of a public API. Propagating exceptions might lead to VS crash.")]
        void OnEditorLoaded(object sender, RoutedEventArgs e)
        {
            if (!this.isEditorLoaded)
            {
                // If the service exists, create an expression editor and add it to the grid - else switch to the textbox data template
                if (this.expressionEditorService != null)
                {
                    Border border = (Border)sender;
                    // Get the references and variables in scope
                    AssemblyContextControlItem assemblies = (AssemblyContextControlItem)this.Context.Items.GetValue(typeof(AssemblyContextControlItem));
                    List<ModelItem> declaredVariables = VisualBasicEditor.GetVariablesInScope(this.OwnerActivity);

                    ImportedNamespaceContextItem importedNamespaces = this.Context.Items.GetValue<ImportedNamespaceContextItem>();
                    importedNamespaces.EnsureInitialized(this.Context);
                    //if the expression text is empty and the expression type is set, then we initialize the text to prompt text
                    if (String.Equals(this.ExpressionText, string.Empty, StringComparison.OrdinalIgnoreCase) && this.ExpressionType != null)
                    {
                        this.Text = TypeToPromptTextConverter.GetPromptText(this.ExpressionType);
                    }

                    //this is a hack
                    this.blockWidth = Math.Max(this.ActualWidth - 8, 0);  //8 is the margin
                    if (this.HasErrors)
                    {
                        this.blockWidth = Math.Max(this.blockWidth - 16, 0); //give 16 for error icon
                    }
                    try
                    {
                        if (this.ExpressionType != null)
                        {
                            this.expressionEditorInstance = this.expressionEditorService.CreateExpressionEditor(assemblies, importedNamespaces, declaredVariables, this.Text, this.ExpressionType, new Size(this.blockWidth, this.blockHeight));
                        }
                        else
                        {
                            this.expressionEditorInstance = this.expressionEditorService.CreateExpressionEditor(assemblies, importedNamespaces, declaredVariables, this.Text, new Size(this.blockWidth, this.blockHeight));
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }

                    if (this.expressionEditorInstance != null)
                    {
                        try
                        {
                            this.expressionEditorInstance.VerticalScrollBarVisibility = this.VerticalScrollBarVisibility;
                            this.expressionEditorInstance.HorizontalScrollBarVisibility = this.HorizontalScrollBarVisibility;

                            this.expressionEditorInstance.AcceptsReturn = this.AcceptsReturn;
                            this.expressionEditorInstance.AcceptsTab = this.AcceptsTab;

                            // Add the expression editor to the text panel, at column 1
                            this.hostControl = this.expressionEditorInstance.HostControl;

                            // Subscribe to this event to change scrollbar visibility on the fly for auto, and to resize the hostable editor
                            // as necessary
                            this.expressionEditorInstance.LostAggregateFocus += new EventHandler(OnEditorLostAggregateFocus);
                            this.expressionEditorInstance.Closing += new EventHandler(OnEditorClosing);

                            // Set up Hostable Editor properties
                            this.expressionEditorInstance.MinLines = this.MinLines;
                            this.expressionEditorInstance.MaxLines = this.MaxLines;

                            this.expressionEditorInstance.HostControl.Style = (Style)FindResource("editorStyle");

                            border.Child = this.hostControl;
                            this.expressionEditorInstance.Focus();
                        }
                        catch (KeyNotFoundException ex)
                        {
                            Debug.Fail("Unable to find editor with the following editor name: " + this.editorName, ex.Message);
                        }
                    }
                }

                if (this.expressionEditorInstance == null)
                {
                    this.ContentTemplate = (DataTemplate)FindResource("textbox");
                }
                this.PerfProvider.WorkflowDesignerExpressionEditorLoaded();

                this.isEditorLoaded = true;
            }
        }

        void OnEditorClosing(object sender, EventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                //these events are expected to be unregistered during lost focus event, but
                //we are unregistering them during unload just in case.  Ideally we want to
                //do this in the CloseExpressionEditor method
                this.expressionEditorInstance.LostAggregateFocus -= new EventHandler(OnEditorLostAggregateFocus);

                this.expressionEditorInstance.Closing -= new EventHandler(OnEditorClosing);
                this.expressionEditorInstance = null;
            }
            Border boarder = this.hostControl.Parent as Border;
            if (boarder != null)
            {
                boarder.Child = null;
            }
            this.hostControl = null;
            this.editorName = null;

        }

        void OnEditorLostAggregateFocus(object sender, EventArgs e)
        {
            this.DoLostFocus();
        }

        //void BindEditorProperties()
        //{
        //    this.hostControl.SetBinding(Control.ContextMenuProperty, "ContextMenu");
        //    this.hostControl.SetBinding(Control.FlowDirectionProperty, "FlowDirection");
        //    this.hostControl.SetBinding(Control.FontFamilyProperty, "FontFamily");
        //    this.hostControl.SetBinding(Control.FontSizeProperty, "FontSize");
        //    this.hostControl.SetBinding(Control.FontStretchProperty, "FontStretch");
        //    this.hostControl.SetBinding(Control.FontStyleProperty, "FontStyle");
        //    this.hostControl.SetBinding(Control.FontWeightProperty, "FontWeight");
        //    this.hostControl.SetBinding(Control.HeightProperty, "Height");
        //    this.hostControl.SetBinding(Control.LanguageProperty, "Language");
        //    this.hostControl.SetBinding(Control.SnapsToDevicePixelsProperty, "SnapsToDevicePixels");
        //}

        // This method is called when the editor data template is unloaded - when the editor data template
        // is unloaded, close the editor session and set the expression editor and editor session to null
        void OnEditorUnloaded(object sender, RoutedEventArgs e)
        {
            // Blank the editorSession and the expressionEditor so as not to use up memory
            // Destroy both as you can only ever spawn one editor per session
            if (this.expressionEditorInstance != null)
            {
                //if we are unloaded during editing, this means we got here by someone clicking breadcrumb, we should try to commit
                if (this.EditingState == EditingState.Editing)
                {
                    this.Commit(false);
                }
                this.expressionEditorInstance.Close();
            }
            else
            {
                this.editingTextBox = null;
            }

            this.isEditorLoaded = false;
        }

        // This method is to give focus and set the caret position when the TextBox DataTemplate is loaded
        void OnTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            this.editingTextBox = textbox;
            textbox.ContextMenu = null;

            //to workaround a but in the TextBox layout code
            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath(VisualBasicEditor.TextProperty);
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

            textbox.SetBinding(TextBox.TextProperty, binding);

            // Set the cursor to correct text position
            int index = GetCharacterIndexFromPoint(textbox);

            textbox.SelectionStart = index;
            textbox.SelectionLength = 0;

            textbox.Focus();
        }

        // This method is to workaround the fact that textbox.GetCharacterIndexFromPoint returns the caret
        // to the left of the character... Thus you can never get the caret after the last character in the
        // expression string.
        int GetCharacterIndexFromPoint(TextBox textbox)
        {
            Point position = Mouse.GetPosition(textbox);
            int index = textbox.GetCharacterIndexFromPoint(position, false);

            if (index < 0)
            {
                // May have clicked outside the text area, get the index of nearest char
                index = textbox.GetCharacterIndexFromPoint(position, true);
                if (index < 0)
                {
                    index = 0;
                }

                // Adjust the cursor position if we clicked to the right of returned character
                Rect charRect = textbox.GetRectFromCharacterIndex(index, true);
                if (position.X > charRect.Left + charRect.Width / 2)
                {
                    index++;
                }
            }

            return index;
        }

        static void ValidateExpression(VisualBasicEditor etb)
        {
            string errorMessage;
            if (etb.DoValidation(new ExpressionValidationContext(etb), out errorMessage))
            {
                etb.UpdateValidationError(errorMessage);
            }
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.ReviewUnusedParameters, Justification = "Existing code")]
        void OnLinesChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.MinLines > this.MaxLines)
            {
                this.MaxLines = this.MinLines;
            }
        }

        static void OnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            VisualBasicEditor textBox = (VisualBasicEditor)dependencyObject;

            if (textBox.ExpressionEditorService != null && textBox.expressionEditorInstance != null)
            {
                textBox.expressionEditorInstance.Text = textBox.Text;
            }

        }

        void OnOwnerModelItemChanged(DependencyPropertyChangedEventArgs e)
        {
            this.InitializeContext(e);
            this.OnPathToArgumentChanged(this.PathToArgument);
        }

        public override IExpressionEditorService ExpressionEditorService
        {
            get { return this.expressionEditorService; }
        }

        internal string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        internal string ExpressionText
        {
            get { return (string)GetValue(ExpressionTextProperty); }
            set { SetValue(ExpressionTextProperty, value); }
        }

        internal ValidationState ValidationState
        {
            get { return (ValidationState)GetValue(VisualBasicEditor.ValidationStateProperty); }
            set { SetValue(VisualBasicEditor.ValidationStateProperty, value); }
        }

        internal bool HasErrors
        {
            get
            {
                bool hasErrors = false;
                if (this.EditingState == EditingState.Idle
                    && !this.IsIndependentExpression)
                {
                    if (this.Expression != null && this.ValidationService != null && this.ValidationService.ValidationStateProperty != null)
                    {
                        ValidationState state = this.ValidationService.ValidationStateProperty.Getter(this.Expression);
                        hasErrors = state == ValidationState.Error;
                    }
                }
                else
                {
                    hasErrors = this.HasValidationError;
                }
                return hasErrors;
            }
        }

        internal string ErrorMessage
        {
            get
            {
                string errorMessage = string.Empty;

                if (this.EditingState == EditingState.Idle && !this.IsIndependentExpression)
                {
                    if (this.Expression != null && this.ValidationService != null && this.ValidationService.ValidationStateProperty != null)
                    {
                        errorMessage = this.ValidationService.ValidationMessageProperty.Getter(this.Expression);
                    }
                }
                else
                {
                    errorMessage = this.ValidationErrorMessage;
                }
                return errorMessage;
            }
        }

        internal EditingState EditingState
        {
            get { return (EditingState)GetValue(EditingStateProperty); }
            set { SetValue(EditingStateProperty, value); }
        }

        internal bool HasValidationError
        {
            get { return (bool)GetValue(HasValidationErrorProperty); }
            set { SetValue(HasValidationErrorProperty, value); }
        }

        internal string ValidationErrorMessage
        {
            get { return (string)GetValue(ValidationErrorMessageProperty); }
            set { SetValue(ValidationErrorMessageProperty, value); }
        }

        DesignerPerfEventProvider PerfProvider
        {
            get
            {
                if (this.perfProvider == null && this.Context != null)
                {
                    perfProvider = this.Context.Services.GetService<DesignerPerfEventProvider>();
                }
                return this.perfProvider;
            }
        }

        ValidationService ValidationService
        {
            get
            {
                if (this.Context != null)
                {
                    return this.Context.Services.GetService<ValidationService>();
                }
                else
                {
                    return null;
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        internal void GenerateExpression()
        {
            Activity valueExpression = null;

            // If the text is null we don't need to bother generating the expression (this would be the case the
            // first time you enter an ETB. We still need to generate the expression when it is EMPTY however - otherwise
            // the case where you had an expression (valid or invalid), then deleted the whole thing will not be evaluated.
            if (this.Text != null)
            {
                using (ModelEditingScope scope = this.OwnerActivity.BeginEdit(SR.PropertyChangeEditingScopeDescription))
                {
                    this.EditingState = EditingState.Validating;
                    // we set the expression to null
                    // a) when the expressionText is empty AND it's a reference expression or
                    // b) when the expressionText is empty AND the DefaultValue property is null
                    if (this.Text.Length == 0 &&
                        (this.UseLocationExpression || (this.DefaultValue == null)))
                    {
                        valueExpression = null;
                    }
                    else
                    {
                        if (this.Text.Length == 0)
                        {
                            this.Text = this.DefaultValue;
                        }
                        valueExpression = CreateVBExpression();
                    }
                    CreateExpressionModelItem(valueExpression);
                    scope.Complete();
                }
            }
        }

        void OnValidationCompleted(object sender, EventArgs e)
        {
            if (this.EditingState != EditingState.Editing)
            {
                if (this.Expression != null && this.ValidationService != null && this.ValidationService.ValidationStateProperty != null)
                {
                    this.ValidationState = this.ValidationService.ValidationStateProperty.Getter(this.Expression);
                }

                this.EditingState = EditingState.Idle;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        private void CreateExpressionModelItem(object valueExpression)
        {
            // try to wrap the droppedObject in  a ModelItem.
            ModelItem expressionModelItem = null;
            if (valueExpression != null)
            {
                ModelServiceImpl modelService = (ModelServiceImpl)this.Context.Services.GetService<ModelService>();
                expressionModelItem = modelService.WrapAsModelItem(valueExpression);
                expressionModelItem.PropertyChanged += this.OnExpressionModelItemChanged;
                this.boundedExpression = expressionModelItem;
            }
            try
            {
                this.internalModelItemChange = true;
                this.EditingState = EditingState.Validating;
                SetValue(ExpressionProperty, expressionModelItem);
            }
            catch (Exception err)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentUICulture, "{0}\r\n{1}", err.Message, err.StackTrace));
                this.internalModelItemChange = false;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        private Activity CreateVBExpression()
        {
            Activity valueExpression = null;
            if (this.OwnerActivity != null)
            {
                ExpressionValidationContext validationContext = new ExpressionValidationContext(this);

                Type expReturnType = null;
                string newExpressionText = null;
                SourceExpressionException compileErrorMessages;

                try
                {
                    VisualBasicSettings settings = null;
                    valueExpression = CreateVBExpression(validationContext, out newExpressionText, out expReturnType, out compileErrorMessages, out settings);

                    if (settings != null)
                    {
                        Fx.Assert(this.Context != null, "editing context cannot be null");
                        //merge with import designer
                        foreach (VisualBasicImportReference reference in settings.ImportReferences)
                        {
                            ImportDesigner.AddImport(reference.Import, this.Context);
                        }
                    }

                    if (!string.IsNullOrEmpty(newExpressionText))
                    {
                        this.previousText = this.Text;
                        this.Text = newExpressionText;
                    }
                }
                catch (Exception err)
                {
                    //if the VisualBasicDesignerHelper were able to resolve the type we use that
                    if (expReturnType == null)
                    {
                        //if not we try to use the expression type
                        expReturnType = this.ExpressionType;
                    }

                    //if expression type is also null, the we use object
                    if (expReturnType == null)
                    {
                        expReturnType = typeof(object);
                    }

                    valueExpression = VisualBasicEditor.CreateExpressionFromString(expReturnType, this.Text, UseLocationExpression, validationContext.ParserContext);

                    Trace.WriteLine(string.Format(CultureInfo.CurrentUICulture, "{0}\r\n{1}", err.Message, err.StackTrace));
                }
                this.ExpressionText = this.Text;
            }
            else
            {
                // If the OwnerActivity is null, do not try to compile the expression otherwise VS will crash
                // Inform the user that OwnerActivity is null (i.e. there is a error in their code)
                Trace.WriteLine("ExpressionTextBox OwnerActivity is null.");
            }
            return valueExpression;
        }

        internal static ActivityWithResult CreateExpressionFromString(string expressionText, bool isLocation, Type type)
        {
            return VisualBasicEditor.CreateExpressionFromString(type, expressionText, isLocation, new ParserContext());
        }

        internal static ActivityWithResult CreateExpressionFromString(Type type, string expressionText, bool isLocation, ParserContext context)
        {
            ActivityWithResult newExpression;

            if (!isLocation)
            {
                newExpression = ExpressionHelper.TryCreateLiteral(type, expressionText, context);

                if (newExpression != null)
                {
                    return newExpression;
                }
            }

            Type targetExpressionType = null;
            if (isLocation)
            {
                targetExpressionType = typeof(VisualBasicReference<>).MakeGenericType(type);
            }
            else
            {
                targetExpressionType = typeof(VisualBasicValue<>).MakeGenericType(type);
            }

            //create new visual basic value and pass expression text into it
            newExpression = (ActivityWithResult)Activator.CreateInstance(targetExpressionType, expressionText);
            //targetExpressionType.GetProperty("Settings").SetValue(newExpression, settings, null);
            //this code below is never executed - it is placed here only to enable compilation support whenver VisualBasicValue constructor
            //changes its parameter list.
            if (null == newExpression)
            {
                //if this gives compilation error, please update CreateInstance parameter list above as well!
                newExpression = new VisualBasicValue<string>(expressionText);
            }

            return newExpression;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "The conversion to an expression might fail due to invalid user input. Propagating exceptions might lead to VS crash.")]
        private static Activity CreateVBExpression(ExpressionValidationContext context, out string newExpressionText, out Type expReturnType, out SourceExpressionException compileErrorMessages, out VisualBasicSettings vbSettings)
        {
            expReturnType = null;
            newExpressionText = null;
            compileErrorMessages = null;
            vbSettings = null;

            //try easy way first - look if there is a type converter which supports conversion between expression type and string
            ActivityWithResult literal = null;
            try
            {
                if (!context.UseLocationExpression)
                {
                    literal = ExpressionHelper.TryCreateLiteral(context.ExpressionType, context.ExpressionText, context.ParserContext);
                }

                if (literal != null)
                {
                    //need to get new expression text - converter might have changed its format, and we want it to be up to date
                    IValueSerializableExpression serializableExpression = literal as IValueSerializableExpression;
                    Fx.Assert(serializableExpression != null, "the expression has to be a Literal<>, which should be IValueSerializableExpression");
                    if (serializableExpression.CanConvertToString(context.ParserContext))
                    {
                        bool shouldBeQuoted = typeof(string) == context.ExpressionType || typeof(Uri) == context.ExpressionType;

                        //whether string begins and ends with quotes '"'. also, if there are
                        //more quotes within than those begining and ending ones, do not bother with literal - assume this is an expression.
                        bool isQuotedString = shouldBeQuoted &&
                                context.ExpressionText.StartsWith("\"", StringComparison.CurrentCulture) &&
                                context.ExpressionText.EndsWith("\"", StringComparison.CurrentCulture) &&
                                context.ExpressionText.IndexOf("\"", 1, StringComparison.CurrentCulture) == context.ExpressionText.Length - 1;
                        var formatString = isQuotedString ? "\"{0}\"" : "{0}";
                        newExpressionText = string.Format(CultureInfo.InvariantCulture, formatString, serializableExpression.ConvertToString(context.ParserContext));
                    }
                }
            }
            //conversion failed - do nothing, let VB compiler take care of the expression
            catch
            {
            }

            Activity valueExpression = literal;

            if (null == valueExpression)
            {
                if (!context.UseLocationExpression)
                {
                    //Compile for validation.
                    valueExpression = VisualBasicDesignerHelper.CreatePrecompiledVisualBasicValue(context.ExpressionType, context.ExpressionText, context.ParserContext.Namespaces, context.ReferencedAssemblies, context.ParserContext, out expReturnType, out compileErrorMessages, out vbSettings);
                }
                else
                {
                    //Compile for validation.
                    valueExpression = VisualBasicDesignerHelper.CreatePrecompiledVisualBasicReference(context.ExpressionType, context.ExpressionText, context.ParserContext.Namespaces, context.ReferencedAssemblies, context.ParserContext, out expReturnType, out compileErrorMessages, out vbSettings);
                }

                ////It's possible the inferred type of expression is a dynamic type (e.g. delegate type), in this case it will cause serialization failure.
                ////To prevent this, we'll always convert the expression type to be object if the inferred type is in dynamic assembly and user doesn't specify any ExpressionType property
                if ((expReturnType.Assembly.IsDynamic) && (context.ExpressionType == null))
                {
                    ActivityWithResult originalExpression = valueExpression as ActivityWithResult;
                    ActivityWithResult morphedExpression;
                    if (ExpressionHelper.TryMorphExpression(originalExpression, ExpressionHelper.IsGenericLocationExpressionType(originalExpression), typeof(object), context.EditingContext, out morphedExpression))
                    {
                        valueExpression = morphedExpression;
                    }
                }
            }

            return valueExpression;
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.ReviewUnusedParameters, Justification = "Existing code")]
        void OnExpressionTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            //for independent expressions, when the type changes, we need to validate the expressions
            if (this.initialized
                && this.IsIndependentExpression
                && this.EditingState == EditingState.Idle)
            {
                ValidateExpression(this);
            }
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.ReviewUnusedParameters, Justification = "Existing code")]
        void OnUseLocationExpressionChanged(DependencyPropertyChangedEventArgs e)
        {
            //for independent expressions, when the type changes, we need to validate the expressions
            if (this.initialized
                && this.IsIndependentExpression
                && this.EditingState == EditingState.Idle
                && this.OwnerActivity != null)
            {
                ValidateExpression(this);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "The entered expression might be invalid and may throw on deserialization. Propagating exception might lead to VS crash")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "The entered expression might be invalid and may throw on deserialization. Propagating exception might lead to VS crash")]
        void OnExpressionChanged(DependencyPropertyChangedEventArgs e)
        {
            ModelItem oldExpression = e.OldValue as ModelItem;
            if (oldExpression != null)
            {
                oldExpression.PropertyChanged -= this.OnExpressionModelItemChanged;
            }
            ModelItem expression = e.NewValue as ModelItem;
            if (expression != null && expression != this.boundedExpression)
            {
                expression.PropertyChanged += this.OnExpressionModelItemChanged;
            }

            try
            {
                this.boundedExpression = expression;

                this.OnExpressionChanged();
            }
            catch (Exception err)
            {
                //if context is set - use error reporting
                if (null != this.Context)
                {
                    this.Context.Items.SetValue(new ErrorItem() { Message = err.Message, Details = err.ToString() });
                }
                //otherwise - fallback to message box
                else
                {
                    MessageBox.Show(err.ToString(), err.Message);
                }
            }

        }

        void expressionModelItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ModelItem item = sender as ModelItem;
            if (item != null)
            {
                if (e.PropertyName.Equals("ExpressionText", StringComparison.Ordinal))
                {
                    this.OnExpressionChanged();
                }
            }
        }

        void OnPathToArgumentChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnPathToArgumentChanged((string)e.NewValue);
        }

        void OnIsIndependentExpressionChanged()
        {
            //if this is an independent expression, we need to initialize the validation error because validation service will not validate it
            if (this.initialized
                && this.IsIndependentExpression
                && this.EditingState == EditingState.Idle)
            {
                ValidateExpression(this);
            }
        }

        void UpdateValidationState()
        {
            if (this.Expression != null && this.ValidationService != null && this.ValidationService.ValidationStateProperty != null)
            {
                this.ValidationState = this.ValidationService.ValidationStateProperty.Getter(this.Expression);
            }
            else
            {
                this.ValidationState = ValidationState.Valid;
            }
        }

        //We need to react to OnExpressionChanged, since there might be multiple ExpressionTextBoxes(ETB) associated to a single Expression.
        //All the ETBs should be updated if the value in any of the ETBs is changed.
        void OnExpressionChanged()
        {
            if (this.HintText == SR.UnsupportedExpressionHintText)
            {
                this.HintText = SR.ExpressionDefaultText;
                this.InitializeHintText();
            }

            if (!this.internalModelItemChange)
            {
                if (this.Expression == null)
                {
                    //new expression is null - there is no text, no previous text, erros should be clear as well as error message
                    this.Text = string.Empty;
                    this.previousText = this.Text;
                    this.ExpressionText = null;
                    this.ValidationState = ValidationState.Valid;
                }
                else
                {
                    this.UpdateValidationState();

                    object expressionObject = this.Expression.GetCurrentValue();
                    ActivityWithResult expression = expressionObject as ActivityWithResult;
                    if (VisualBasicEditor.IsSupportedExpressionType(expression))
                    {
                        String expressionString = null;
                        //create parser context - do not pass ownerActivity - it might be null at this time
                        ParserContext context = new ParserContext();
                        expressionString = ExpressionHelper.GetExpressionString(expression, context);

                        this.Text = expressionString;
                        this.ExpressionText = expressionString;
                        this.previousText = this.Text;
                        this.IsSupportedExpression = true;
                    }
                    else
                    {
                        this.Text = string.Empty;
                        this.IsSupportedExpression = false;
                        this.HintText = SR.UnsupportedExpressionHintText;
                    }

                    this.isExpressionLoaded = true;
                    if (this.isBeginEditPending)
                    {
                        this.BeginEdit();
                    }
                }
            }
            internalModelItemChange = false;
        }

        private static bool IsSupportedExpressionType(object expressionObject)
        {
            ActivityWithResult expression = expressionObject as ActivityWithResult;
            bool isSupported = false;
            if (expression != null)
            {
                Type expressionType = expression.GetType();
                Type genericExpressionType = null;
                if (expressionType.IsGenericType)
                {
                    genericExpressionType = expressionType.GetGenericTypeDefinition();
                }

                if (genericExpressionType == VisualBasicValueType ||
                    genericExpressionType == VisualBasicReferenceType ||
                    genericExpressionType == VariableReferenceType ||
                    genericExpressionType == VariableValueType ||
                    (genericExpressionType == LiteralType && ExpressionHelper.CanTypeBeSerializedAsLiteral(expression.ResultType)))
                {
                    isSupported = true;
                }
            }

            return isSupported;
        }

        void OnPathToArgumentChanged(string pathAsString)
        {
            this.expressionModelProperty = null;
            this.expressionConverter = null;
            if (!string.IsNullOrEmpty(pathAsString) && null != this.OwnerActivity)
            {
                string[] path = pathAsString.Split('.');
                if (path.Length > 0)
                {
                    this.expressionModelProperty = this.OwnerActivity.Properties[path[0]];
                    for (int i = 1; i < path.Length; ++i)
                    {
                        if (null != this.expressionModelProperty && null != this.expressionModelProperty.Value)
                        {
                            this.expressionModelProperty = this.expressionModelProperty.Value.Properties[path[i]];
                        }
                        else
                        {
                            this.expressionModelProperty = null;
                            break;
                        }
                    }
                }
            }
            if (null != this.expressionModelProperty)
            {
                this.expressionConverter = ((ModelPropertyImpl)this.expressionModelProperty).PropertyDescriptor.Converter;
            }
            this.InitializeHintText();
        }

        void InitializeHintText()
        {
            DescriptionAttribute customHint = null;
            if (this.expressionModelProperty != null && this.expressionModelProperty.Attributes.Count != 0)
            {
                customHint = this.expressionModelProperty.Attributes
                    .OfType<DescriptionAttribute>()
                    .FirstOrDefault();
            }
            this.HintText = (null == customHint || string.IsNullOrEmpty(customHint.Description) ?
                (string.Equals(this.HintText, SR.ExpressionDefaultText) ? SR.ExpressionDefaultText : this.HintText) : customHint.Description);

            string hint = null;
            if (this.HintText != null)
            {
                hint = this.HintText.Trim(new char[] { '<', '>' });
                this.SetValue(AutomationProperties.HelpTextProperty, hint);
            }
        }

        void InitializeContext(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                // remove the OnValidationCompleted event handler on the old value
                ModelItem modelItem = (ModelItem)e.OldValue;
                if (modelItem != null)
                {
                    EditingContext context = modelItem.GetEditingContext();
                    ValidationService validationService = context.Services.GetService<ValidationService>();
                    if (validationService != null)
                    {
                        validationService.ValidationCompleted -= this.OnValidationCompleted;
                    }
                }
            }


            if (null != this.OwnerActivity)
            {
                if (this.ValidationService != null)
                {
                    this.ValidationService.ValidationCompleted += this.OnValidationCompleted;
                    this.UpdateValidationState();
                }
            }
        }

        #region Command Handlers

        public override bool CanCommit()
        {
            string currentText = this.Text;
            if (this.expressionEditorInstance != null)
            {
                currentText = this.expressionEditorInstance.Text ?? this.expressionEditorInstance.Text.Trim();
            }
            else if (this.editingTextBox != null)
            {
                currentText = this.editingTextBox.Text ?? this.editingTextBox.Text.Trim();
            }

            //we dont need to commit change if currentText and previousText is the same
            //null and empty should be considered equal in this context

            return !string.Equals(currentText, this.previousText) &&
                   !(string.IsNullOrEmpty(currentText) && string.IsNullOrEmpty(this.previousText));
        }

        void OnExpressionTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.InitializeHintText();

            //if this is an independent expression, we need to initialize the validation error because validation service will not validate it
            if (this.IsIndependentExpression)
            {
                ValidateExpression(this);
            }

            this.initialized = true;
        }

        void OnExpressionTextBoxUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.ValidationService != null)
            {
                this.ValidationService.ValidationCompleted -= this.OnValidationCompleted;
            }
            KillValidator();

            if (this.boundedExpression != null)
            {
                this.boundedExpression.PropertyChanged -= this.OnExpressionModelItemChanged;
            }
        }

        public override void OnCompleteWordCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {

                e.CanExecute = this.expressionEditorInstance.CanCompleteWord();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        public override void OnGlobalIntellisenseCommandCanExecute(CanExecuteRoutedEventArgs e)
        {

            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanGlobalIntellisense();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        public override void OnParameterInfoCommandCanExecute(CanExecuteRoutedEventArgs e)
        {

            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanParameterInfo();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        public override void OnQuickInfoCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanQuickInfo();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        public override void OnIncreaseFilterLevelCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanIncreaseFilterLevel();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        public override void OnDecreaseFilterLevelCommandCanExecute(CanExecuteRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanDecreaseFilterLevel();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        void OnCutCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanCut();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        public override void OnCompleteWordCommandExecute(ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.CompleteWord();
            }
            else
            {
                e.Handled = false;
            }

        }

        public override void OnGlobalIntellisenseCommandExecute(ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.GlobalIntellisense();
            }
            else
            {
                e.Handled = false;
            }

        }

        public override void OnParameterInfoCommandExecute(ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.ParameterInfo();
            }
            else
            {
                e.Handled = false;
            }

        }

        public override void OnQuickInfoCommandExecute(ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.QuickInfo();
            }
            else
            {
                e.Handled = false;
            }

        }

        public override void OnDecreaseFilterLevelCommandExecute(ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.DecreaseFilterLevel();
            }
            else
            {
                e.Handled = false;
            }

        }

        public override void OnIncreaseFilterLevelCommandExecute(ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.IncreaseFilterLevel();
            }
            else
            {
                e.Handled = false;
            }

        }

        void OnCutCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.Cut();
            }
            else
            {
                e.Handled = false;
            }
        }

        void OnCopyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanCopy();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        void OnCopyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.Copy();
            }
            else
            {
                e.Handled = false;
            }
        }

        void OnPasteCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanPaste();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        void OnPasteCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.Paste();
            }
            else
            {
                e.Handled = false;
            }
        }

        void OnUndoCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanUndo();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Catch all execeptions to prevent crash.")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "Catch all execeptions to prevent crash.")]
        void OnUndoCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                try
                {
                    e.Handled = this.expressionEditorInstance.Undo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                e.Handled = false;
            }
        }

        void OnRedoCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.CanExecute = this.expressionEditorInstance.CanRedo();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        void OnRedoCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                e.Handled = this.expressionEditorInstance.Redo();
            }
            else
            {
                e.Handled = false;
            }
        }

        void OnHelpExecute(object sender, ExecutedRoutedEventArgs e)
        {
            IIntegratedHelpService help = this.Context.Services.GetService<IIntegratedHelpService>();
            if (help != null)
            {
                help.ShowHelpFromKeyword(HelpKeywords.ExpressionEditorPage);
            }
            else
            {
                System.Diagnostics.Process.Start(SR.DefaultHelpUrl);
            }
        }

        #endregion

        void OnHintTextChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!string.Equals(SR.ExpressionDefaultText, e.NewValue) && !string.Equals(e.OldValue, e.NewValue))
            {
                this.InitializeHintText();
            }
        }

        void OnAcceptsReturnChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                this.expressionEditorInstance.AcceptsReturn = (bool)e.NewValue;
            }
        }

        void OnAcceptsTabChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.expressionEditorInstance != null)
            {
                this.expressionEditorInstance.AcceptsTab = (bool)e.NewValue;
            }
        }

        void StartValidator()
        {
            if (this.validator == null)
            {
                this.validator = new BackgroundWorker();
                this.validator.WorkerReportsProgress = true;
                this.validator.WorkerSupportsCancellation = true;

                this.validator.DoWork += delegate(object obj, DoWorkEventArgs args)
                {
                    BackgroundWorker worker = obj as BackgroundWorker;
                    if (worker.CancellationPending)
                    {
                        args.Cancel = true;
                        return;
                    }
                    ExpressionValidationContext validationContext = args.Argument as ExpressionValidationContext;
                    if (validationContext != null)
                    {
                        string errorMessage;
                        if (DoValidation(validationContext, out errorMessage))
                        {
                            worker.ReportProgress(0, errorMessage);
                        }

                        //sleep
                        if (worker.CancellationPending)
                        {
                            args.Cancel = true;
                            return;
                        }

                        Thread.Sleep(ValidationWaitTime);
                        args.Result = validationContext;
                    }

                };

                this.validator.RunWorkerCompleted += delegate(object obj, RunWorkerCompletedEventArgs args)
                {
                    if (!args.Cancelled)
                    {
                        ExpressionValidationContext validationContext = args.Result as ExpressionValidationContext;
                        if (validationContext != null)
                        {
                            Dispatcher.BeginInvoke(new Action<ExpressionValidationContext>((target) =>
                            {
                                //validator could be null by the time we try to validate again or
                                //if it's already busy
                                if (this.validator != null && !this.validator.IsBusy)
                                {
                                    target.Update(this);
                                    this.validator.RunWorkerAsync(target);
                                }
                            }), validationContext);
                        }
                    }
                };

                this.validator.ProgressChanged += delegate(object obj, ProgressChangedEventArgs args)
                {
                    string error = args.UserState as string;
                    Dispatcher.BeginInvoke(new Action<string>(UpdateValidationError), error);
                };

                this.validator.RunWorkerAsync(new ExpressionValidationContext(this));
            }
        }

        //perform one validation synchronously
        //return value indicates whether errorMessage is updated.
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "The entered expression might be invalid and may throw on deserialization. Propagating exception might lead to VS crash")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "The entered expression might be invalid and may throw on deserialization. Propagating exception might lead to VS crash")]
        bool DoValidation(ExpressionValidationContext validationContext, out string errorMessage)
        {
            Fx.Assert(validationContext != null, "only work when context is not null");
            errorMessage = null;

            //validate
            //if the text is empty we clear the error message
            if (string.IsNullOrEmpty(validationContext.ExpressionText))
            {
                errorMessage = null;
                return true;
            }
            // if the expression text is different from the last time we run the validation we run the validation
            else if (!string.Equals(validationContext.ExpressionText, validationContext.ValidatedExpressionText))
            {
                Type expReturnType = null;
                string newExpressionText = null;
                SourceExpressionException compileErrorMessages = null;
                VisualBasicSettings settings = null;
                try
                {
                    CreateVBExpression(validationContext, out newExpressionText, out expReturnType, out compileErrorMessages, out settings);
                    if (compileErrorMessages != null)
                    {
                        errorMessage = compileErrorMessages.Message;
                    }
                }
                catch (Exception err)
                {
                    errorMessage = err.Message;
                }

                return true;
            }

            return false;
        }

        void UpdateValidationError(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                //report error
                this.HasValidationError = true;
                this.ValidationErrorMessage = errorMessage;
            }
            else
            {
                this.HasValidationError = false;
                this.ValidationErrorMessage = null;
            }
        }

        private class ExpressionValidationContext
        {
            internal ParserContext ParserContext { get; set; }
            internal Type ExpressionType { get; set; }
            internal String ExpressionText { get; set; }
            internal EditingContext EditingContext { get; set; }
            internal String ValidatedExpressionText { get; set; }
            internal bool UseLocationExpression { get; set; }

            internal ExpressionValidationContext(VisualBasicEditor etb)
            {
                Update(etb);
            }

            internal void Update(VisualBasicEditor etb)
            {
                Fx.Assert(etb.OwnerActivity != null, "Owner Activity is null");
                this.EditingContext = etb.OwnerActivity.GetEditingContext();

                //setup ParserContext
                this.ParserContext = new ParserContext(etb.OwnerActivity)
                {
                    //callee is a ExpressionTextBox
                    Instance = etb,
                    //pass property descriptor belonging to epression's model property (if one exists)
                    PropertyDescriptor = (null != etb.expressionModelProperty ? ((ModelPropertyImpl)etb.expressionModelProperty).PropertyDescriptor : null),
                };

                this.ExpressionType = etb.ExpressionType;
                this.ValidatedExpressionText = this.ExpressionText;
                if (etb.expressionEditorInstance != null)
                {
                    this.ExpressionText = etb.expressionEditorInstance.Text;
                }
                else if (etb.editingTextBox != null)
                {
                    this.ExpressionText = etb.editingTextBox.Text;
                }
                else
                {
                    this.ExpressionText = etb.Text;
                }
                this.UseLocationExpression = etb.UseLocationExpression;
            }

            internal IEnumerable<string> ReferencedAssemblies
            {
                get
                {
                    Fx.Assert(this.EditingContext != null, "ModelItem.Context = null");
                    AssemblyContextControlItem assemblyContext = this.EditingContext.Items.GetValue<AssemblyContextControlItem>();
                    if (assemblyContext != null)
                    {
                        return assemblyContext.AllAssemblyNamesInContext;
                    }
                    return null;
                }
            }
        }

        private static List<ModelItem> GetVariablesInScopeWithShadowing(ModelItem ownerActivity, bool includeArguments)
        {
            List<ModelItem> variablesInScope = new List<ModelItem>();
            if (ownerActivity != null)
            {
                HashSet<string> variableNames = new HashSet<string>();
                ModelItem currentItem = ownerActivity;
                Func<ModelItem, bool> filterDelegate = new Func<ModelItem, bool>((variable) =>
                    {
                        string variableName = (string)variable.Properties["Name"].ComputedValue;
                        if (variableName == null)
                        {
                            return false;
                        }
                        else
                        {
                            return !variableNames.Contains(variableName.ToUpperInvariant());
                        }
                    });

                while (currentItem != null)
                {
                    List<ModelItem> variables = new List<ModelItem>();
                    ModelItemCollection variablesCollection = currentItem.GetVariableCollection();
                    if (variablesCollection != null)
                    {
                        variables.AddRange(variablesCollection);
                    }
                    variables.AddRange(currentItem.FindActivityDelegateArguments());

                    // For the variables defined at the same level, shadowing doesn't apply. If there're multiple variables defined at the same level
                    // have duplicate names when case is ignored, all of these variables should bee added as variables in scope and let validation reports
                    // ambiguous reference error. So that we need to scan all variables defined at the same level first and then add names to the HashSet.                                        
                    IEnumerable<ModelItem> filteredVariables = variables.Where<ModelItem>(filterDelegate);
                    variablesInScope.AddRange(filteredVariables);
                    foreach (ModelItem variable in filteredVariables)
                    {
                        variableNames.Add(((string)variable.Properties["Name"].ComputedValue).ToUpperInvariant());
                    }

                    currentItem = currentItem.Parent;
                }

                if (includeArguments)
                {
                    List<ModelItem> arguments = VisualBasicEditor.GetVariablesForArguments(ownerActivity.Root);
                    variablesInScope.AddRange(arguments.Where<ModelItem>(filterDelegate));
                }
            }

            return variablesInScope;
        }

        private static List<ModelItem> GetVariablesForArguments(ModelItem modelItem)
        {
            List<ModelItem> arguments = new List<ModelItem>();
            //if expression editor is loaded in the WF which is hosted within ActivityBuilder, there is a need to pickup defined arguments
            //and feed them as variables, so intellisense can include them
            if (null != modelItem && ActivityBuilderHelper.IsActivityBuilderType(modelItem))
            {
                ModelTreeManager treeManager = ((IModelTreeItem)modelItem).ModelTreeManager;
                //call ActivityBuilderHelper.GetVariables - it will create a collection of Variable - each variable corresponds to a specific argument
                arguments.AddRange(
                    ActivityBuilderHelper.GetVariables(modelItem)
                    //create a fake model item implementation - there is no need to store that model item anywhere in the model tree, it is required
                    //of the expression editor interface to pass instances of model items wrapping variables, rather than actual variables
                    .Select<Variable, ModelItem>(entry => new FakeModelItemImpl(treeManager, typeof(Variable), entry, null)));
            }

            return arguments;
        }

        internal static List<ModelItem> GetVariablesInScope(ModelItem ownerActivity)
        {
            List<ModelItem> declaredVariables = new List<ModelItem>();
            if (ownerActivity != null)
            {
                bool includeArguments = !(ownerActivity.GetCurrentValue() is ActivityBuilder);
                FrameworkName targetFramework = WorkflowDesigner.GetTargetFramework(ownerActivity.GetEditingContext());
                if ((targetFramework != null) && (targetFramework.IsLessThan45()))
                {
                    declaredVariables.AddRange(VariableHelper.FindVariablesInScope(ownerActivity));
                    declaredVariables.AddRange(VariableHelper.FindActivityDelegateArgumentsInScope(ownerActivity));
                    if (includeArguments)
                    {
                        declaredVariables.AddRange(VisualBasicEditor.GetVariablesForArguments(ownerActivity.Root));
                    }
                }
                else
                {
                    declaredVariables.AddRange(VisualBasicEditor.GetVariablesInScopeWithShadowing(ownerActivity, includeArguments));
                }
            }

            return declaredVariables;
        }
    }

    internal sealed class LineToHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double convertedValue = Double.NaN;
            bool isDefault = true;

            // Calculate the height for the textblock as ExpressionTextBox exposes lines properties,
            // and TextBlock doesn't have lines properties.
            FontFamily fontFamily = values.OfType<FontFamily>().FirstOrDefault();
            int lines = values.OfType<int>().FirstOrDefault();
            double[] doubleArray = values.OfType<double>().ToArray<double>();

            if (doubleArray.Length == 2)
            {
                double height = doubleArray[0]; // The first element of the array is going to be the height
                double fontSize = doubleArray[1]; // The seconed element of the array is going to be the fontSize

                // 0.0 is default for MinHeight, PositiveInfinity is default for MaxHeight
                if (string.Equals(parameter as string, "MinHeight"))
                {
                    isDefault = (height == 0.0);
                }
                else if (string.Equals(parameter as string, "MaxHeight"))
                {
                    isDefault = (double.IsPositiveInfinity(height));
                }

                // If the height value we are evaluating is default, use Lines for sizing...
                // If no heights (height or lines) have been explicitly specified, we would rather default the height
                // as if the Line was 1 - so use the line heights, rather than 0.0 and/or PositiveInfinity.
                if (isDefault)
                {
                    double lineHeight = fontSize * fontFamily.LineSpacing;

                    if (fontFamily != null)
                    {
                        convertedValue = lineHeight * (double)lines + 4;
                    }
                }
                else
                {
                    convertedValue = height;
                }
            }

            return convertedValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }

    internal sealed class ValidationStateToErrorConverter : IMultiValueConverter
    {

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            VisualBasicEditor etb = values[0] as VisualBasicEditor;
            if (values[0] == DependencyProperty.UnsetValue || etb == null)
            {
                return false;
            }
            return etb.HasErrors;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        #endregion
    }

    internal sealed class ValidationErrorMessageConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            VisualBasicEditor etb = values[0] as VisualBasicEditor;
            if (values[0] == DependencyProperty.UnsetValue || etb == null)
            {
                return false;
            }
            return etb.ErrorMessage;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        #endregion
    }

    internal sealed class TypeToPromptTextConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TypeToPromptTextConverter.GetPromptText(value);
        }

        internal static string GetPromptText(object value)
        {
            Type expressionType = value as Type;
            if (value == DependencyProperty.UnsetValue || expressionType == null || !expressionType.IsValueType)
            {
                return "Nothing";
            }
            else
            {
                return Activator.CreateInstance(expressionType).ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        #endregion
    }


    public enum EditingState
    {
        Editing,
        Validating,
        Idle
    }
}
