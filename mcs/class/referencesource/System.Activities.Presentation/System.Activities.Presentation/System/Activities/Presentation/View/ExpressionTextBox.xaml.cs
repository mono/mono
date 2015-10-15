//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Activities.Expressions;
    using System.Activities.Presentation.Expressions;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Xaml;
    using Microsoft.Activities.Presentation;
    using ExpressionEditor = System.Activities.Presentation.Expressions.ExpressionActivityEditor;

    public sealed partial class ExpressionTextBox : UserControl
    {
        #region Legacy public properties
        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register("HintText", typeof(string), typeof(ExpressionTextBox));

        public static readonly DependencyProperty ExpressionProperty = DependencyProperty.Register("Expression", typeof(ModelItem), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnInnerControlRelatedPropertyChanged)));

        public static readonly DependencyProperty ExpressionTypeProperty = DependencyProperty.Register("ExpressionType", typeof(Type), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty OwnerActivityProperty = DependencyProperty.Register("OwnerActivity", typeof(ModelItem), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnInnerControlRelatedPropertyChanged)));

        public static readonly DependencyProperty UseLocationExpressionProperty = DependencyProperty.Register("UseLocationExpression", typeof(bool), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnInnerControlRelatedPropertyChanged)));

        public static readonly DependencyProperty PathToArgumentProperty = DependencyProperty.Register("PathToArgument", typeof(string), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty IsSupportedExpressionProperty = DependencyProperty.Register("IsSupportedExpression", typeof(bool), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));

        public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register("MaxLines", typeof(int), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(Int32.MaxValue));

        public static readonly DependencyProperty MinLinesProperty = DependencyProperty.Register("MinLines", typeof(int), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(1));

        public static readonly DependencyProperty ExplicitCommitProperty = DependencyProperty.Register("ExplicitCommit", typeof(bool), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(string), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register("AcceptsReturn", typeof(bool), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty.Register("AcceptsTab", typeof(bool), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ExpressionTextBox),
                new FrameworkPropertyMetadata(false));

        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public ModelItem Expression
        {
            get { return (ModelItem)GetValue(ExpressionProperty); }
            set { SetValue(ExpressionProperty, value); }
        }

        public Type ExpressionType
        {
            get { return (Type)GetValue(ExpressionTypeProperty); }
            set { SetValue(ExpressionTypeProperty, value); }
        }

        public bool UseLocationExpression
        {
            get { return (bool)GetValue(UseLocationExpressionProperty); }
            set { SetValue(UseLocationExpressionProperty, value); }
        }


        [Fx.Tag.KnownXamlExternal]
        public ModelItem OwnerActivity
        {
            get { return (ModelItem)GetValue(OwnerActivityProperty); }
            set { SetValue(OwnerActivityProperty, value); }
        }

        public string PathToArgument
        {
            get { return (string)GetValue(PathToArgumentProperty); }
            set { SetValue(PathToArgumentProperty, value); }
        }

        public bool IsSupportedExpression
        {
            get { return (bool)GetValue(IsSupportedExpressionProperty); }
            set { SetValue(IsSupportedExpressionProperty, value); }
        }

        public string DefaultValue
        {
            get { return (string)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public bool AcceptsTab
        {
            get { return (bool)GetValue(AcceptsTabProperty); }
            set { SetValue(AcceptsTabProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        public int MaxLines
        {
            get { return (int)GetValue(MaxLinesProperty); }
            set { SetValue(MaxLinesProperty, value); }
        }

        public int MinLines
        {
            get { return (int)GetValue(MinLinesProperty); }
            set { SetValue(MinLinesProperty, value); }
        }

        public bool ExplicitCommit
        {
            get { return (bool)GetValue(ExplicitCommitProperty); }
            set { SetValue(ExplicitCommitProperty, value); }
        }

        //Microsoft 
        public IExpressionEditorService ExpressionEditorService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ViewModel.GetExpressionService(this.editor);
            }
        }
        #endregion

        #region Legacy routed events
        public static readonly RoutedEvent EditorLostLogicalFocusEvent =
            EventManager.RegisterRoutedEvent("EditorLostLogicalFocus", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ExpressionTextBox));

        public event RoutedEventHandler EditorLostLogicalFocus
        {
            add
            {
                this.AddHandler(ExpressionTextBox.EditorLostLogicalFocusEvent, value);
            }
            remove
            {
                this.RemoveHandler(ExpressionTextBox.EditorLostLogicalFocusEvent, value);
            }
        }
        #endregion

        #region Legacy commands
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand CompleteWordCommand = new RoutedCommand("CompleteWordCommand", typeof(ExpressionTextBox));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand GlobalIntellisenseCommand = new RoutedCommand("GlobalIntellisenseCommand", typeof(ExpressionTextBox));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand ParameterInfoCommand = new RoutedCommand("ParameterInfoCommand", typeof(ExpressionTextBox));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand QuickInfoCommand = new RoutedCommand("QuickInfoCommand", typeof(ExpressionTextBox));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand IncreaseFilterLevelCommand = new RoutedCommand("IncreaseFilterLevelCommand", typeof(ExpressionTextBox));
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly ICommand DecreaseFilterLevelCommand = new RoutedCommand("DecreaseFilterLevelCommand", typeof(ExpressionTextBox));
        #endregion

        #region Legacy internal properties used by other designer parts
        internal static readonly DependencyProperty IsIndependentExpressionProperty = DependencyProperty.Register("IsIndependentExpression", typeof(bool), typeof(ExpressionTextBox));

        internal bool IsIndependentExpression
        {
            get { return (bool)GetValue(IsIndependentExpressionProperty); }
            set { SetValue(IsIndependentExpressionProperty, value); }
        }
        #endregion

        #region New added public properties

        public static readonly DependencyProperty ExpressionActivityEditorProperty =
            DependencyProperty.Register("ExpressionActivityEditor", typeof(string), typeof(ExpressionTextBox), new UIPropertyMetadata(new PropertyChangedCallback(OnInnerControlRelatedPropertyChanged)));

        public const string ExpressionActivityEditorOptionName = "ExpressionActivityEditorName";

        public string ExpressionActivityEditor
        {
            get { return (string)GetValue(ExpressionActivityEditorProperty); }
            set { SetValue(ExpressionActivityEditorProperty, value); }
        }

        #endregion

        #region New added internal properties
        internal static readonly DependencyProperty InternalHintTextProperty =
            DependencyProperty.Register("InternalHintText", typeof(string), typeof(ExpressionTextBox));

        internal string InternalHintText
        {
            get { return (string)GetValue(InternalHintTextProperty); }
            set { SetValue(InternalHintTextProperty, value); }
        }
        #endregion

        string currentEditorName;
        ExpressionActivityEditor editor;
        ExpressionTextBoxViewModel viewModel;
        bool? isDesignMode;
        bool isHintTextSetInternally;

        internal static readonly DependencyProperty IsInlinePropertyEditorProperty =
            DependencyProperty.Register("IsInlinePropertyEditor", typeof(bool), typeof(ExpressionTextBox), new FrameworkPropertyMetadata(false, OnInnerControlRelatedPropertyChanged));

        private static readonly DependencyProperty UnsupportedEditorMessageProperty =
            DependencyProperty.Register("UnsupportedEditorMessage", typeof(string), typeof(ExpressionTextBox));

        internal bool IsInlinePropertyEditor
        {
            get { return (bool)GetValue(IsInlinePropertyEditorProperty); }
            set { SetValue(IsInlinePropertyEditorProperty, value); }
        }

        internal ExpressionActivityEditor Editor
        {
            get
            {
                return this.editor;
            }
        }

        internal string CurrentEditorName
        {
            get
            {
                return this.currentEditorName;
            }
        }

        private string UnsupportedEditorMessage
        {
            get { return (string)GetValue(UnsupportedEditorMessageProperty); }
            set { SetValue(UnsupportedEditorMessageProperty, value); }
        }

        ExpressionTextBoxViewModel ViewModel
        {
            get
            {
                if (this.viewModel == null)
                {
                    this.viewModel = new ExpressionTextBoxViewModel();
                }

                return this.viewModel;
            }
        }

        string HintTextSetbyUser
        {
            get;
            set;
        }

        EditingContext Context
        {
            get
            {
                if (this.OwnerActivity != null)
                {
                    Fx.Assert(this.OwnerActivity.GetEditingContext() != null, "Any acitvity should be associated with an EditingContext");
                    return this.OwnerActivity.GetEditingContext();
                }
                else
                {
                    return null;
                }
            }
        }

        //Indicate whether it's in the context of WPF designer, e.g. when designing activity designer
        bool IsDesignMode
        {
            get
            {
                if (!this.isDesignMode.HasValue)
                {
                    this.isDesignMode = DesignerProperties.GetIsInDesignMode(this);
                }

                return this.isDesignMode.Value;
            }
        }

        public ExpressionTextBox()
        {
            InitializeComponent();

            if (this.IsDesignMode)
            {
                this.Content = null;
                DataTemplate designModeTemplate = this.FindResource("designModelTemplate") as DataTemplate;
                Fx.Assert(designModeTemplate != null, "There should be a DataTemplate named \"designModelTemplate\"");
                this.ContentTemplate = designModeTemplate;
            }
        }

        void OnExpressionTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.InitializeChildEditor();
        }

        protected override void OnInitialized(EventArgs e)
        {
            if (!this.IsDesignMode)
            {
                this.LoadInnerControl(this.GetExpressionEditorType());
            }
            base.OnInitialized(e);
        }

        void LoadInnerControl(string editorName)
        {
            if (editorName != this.currentEditorName)
            {

                ExpressionActivityEditor newEditor;
                if (this.ViewModel.TryCreateEditor(editorName, out newEditor))
                {
                    if ((this.IsInlinePropertyEditor) && !(newEditor is TextualExpressionEditor))
                    {
                        InitializeHintText(null);
                        this.editor = null;
                        this.currentEditorName = null;
                        this.Content = null;
                        this.ContentTemplate = this.FindResource("disabledInlineEditingTemplate") as DataTemplate;
                    }
                    else
                    {
                        this.currentEditorName = editorName;
                        this.editor = newEditor;
                        this.Content = this.editor;
                        this.ContentTemplate = null;
                        if (this.IsLoaded)
                        {
                            this.InitializeChildEditor();
                        }
                    }
                }
                else
                {
                    this.UnsupportedEditorMessage = string.Format(CultureInfo.CurrentUICulture, SR.NonRegisteredExpressionEditor, editorName);
                    InitializeHintText(null);
                    this.editor = null;
                    this.currentEditorName = null;
                    this.Content = null;
                    this.ContentTemplate = this.FindResource("unsupportedEditorTemplate") as DataTemplate;
                }
            }
        }

        public static void RegisterExpressionActivityEditor(string name, Type expressionActivityEditorType, CreateExpressionFromStringCallback convertFromString)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw FxTrace.Exception.AsError(new ArgumentException(string.Format(CultureInfo.CurrentUICulture, SR.InvalidExpressionEditorName,
                    name)));
            }
            ExpressionTextBoxViewModel.RegisterExpressionActivityService(name, expressionActivityEditorType, convertFromString);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "ExpressionActivityEditor":
                    ExpressionEditor.ValidateExpressionActivityEditorName((string)e.NewValue);
                    break;
                case "HintText":
                    if (!this.isHintTextSetInternally)
                    {
                        this.HintTextSetbyUser = (string)e.NewValue;
                    }
                    break;
                case "InternalHintText":
                    this.isHintTextSetInternally = true;
                    this.HintText = this.InternalHintText;
                    this.isHintTextSetInternally = false;
                    break;
            }
            base.OnPropertyChanged(e);
        }

        //Following properties have impact on inner editor type:
        //1) Expression
        //2) OwnerActivity
        //3) ExpressionActivityEditor
        //4) IsInlinePropertyEditor
        //5) UseLocationExpression
        static void OnInnerControlRelatedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ExpressionTextBox expressionTextBox = sender as ExpressionTextBox;
            if (!expressionTextBox.IsDesignMode)
            {
                expressionTextBox.LoadInnerControl(expressionTextBox.GetExpressionEditorType());
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ExpressionTextBoxAutomationPeer(this);
        }

        private string GetExpressionEditorType()
        {
            object root = null;
            if (this.OwnerActivity != null)
            {
                ModelItem rootItem = this.Context.Services.GetService<ModelService>().Root;
                if (rootItem != null)
                {
                    root = rootItem.GetCurrentValue();
                }
            }

            return this.ViewModel.GetExpressionEditorType(this.ExpressionActivityEditor, root, WorkflowDesigner.GetTargetFramework(this.Context));
        }

        void InitializeChildEditor()
        {
            this.ViewModel.InitializeEditor(this.editor, this);
        }

        void OnCommitCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ExpressionTextBox expressionTextBox = sender as ExpressionTextBox;
            if (expressionTextBox.editor != null)
            {
                e.CanExecute = expressionTextBox.editor.CanCommit();
                e.Handled = true;
            }
        }

        void OnCommitCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ExpressionTextBox expressionTextBox = sender as ExpressionTextBox;
            if (expressionTextBox.editor != null)
            {
                expressionTextBox.editor.Commit(expressionTextBox.ExplicitCommit);
                e.Handled = true;
            }
        }

        public void BeginEdit()
        {
            if (this.editor != null)
            {
                this.editor.BeginEdit();
            }
        }

        internal void OnCompleteWordCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnCompleteWordCommandCanExecute(e);
            }
        }

        internal void OnCompleteWordCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnCompleteWordCommandExecute(e);
            }
        }

        internal void OnGlobalIntellisenseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnGlobalIntellisenseCommandCanExecute(e);
            }
        }

        internal void OnGlobalIntellisenseCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnGlobalIntellisenseCommandExecute(e);
            }
        }

        internal void OnParameterInfoCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnParameterInfoCommandCanExecute(e);
            }
        }

        internal void OnParameterInfoCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnParameterInfoCommandExecute(e);
            }
        }

        internal void OnQuickInfoCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnQuickInfoCommandCanExecute(e);
            }
        }

        internal void OnQuickInfoCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnQuickInfoCommandExecute(e);
            }
        }

        internal void OnIncreaseFilterLevelCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnIncreaseFilterLevelCommandCanExecute(e);
            }
        }

        internal void OnIncreaseFilterLevelCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnIncreaseFilterLevelCommandExecute(e);
            }
        }

        internal void OnDecreaseFilterLevelCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnDecreaseFilterLevelCommandCanExecute(e);
            }
        }

        internal void OnDecreaseFilterLevelCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if ((this.editor != null) && (this.editor is TextualExpressionEditor))
            {
                (this.editor as TextualExpressionEditor).OnDecreaseFilterLevelCommandExecute(e);
            }
        }

        internal void InitializeHintText(string defaultEditorHintText)
        {
            //If user doesn't specify any custom hint text, we should honor the default hinttext of inner editor
            //otherwise we should restore the hint text to what set by user when switch inner editor
            this.isHintTextSetInternally = true;

            if (this.HintTextSetbyUser == null)
            {
                this.HintText = defaultEditorHintText;
            }
            else
            {
                this.HintText = this.HintTextSetbyUser;
            }

            this.isHintTextSetInternally = false;
        }

        internal static bool TryConvertFromString(string targetEditor, string expressionText, bool isLocationExpression, Type resultType, out ActivityWithResult expression)
        {
            return ExpressionTextBoxViewModel.TryConvertString(targetEditor, expressionText, isLocationExpression, resultType, out expression);
        }
    }

    internal class ExpressionTextBoxViewModel
    {
        static string ITextExpressionInterfaceName = typeof(ITextExpression).Name;

        static Dictionary<string, Type> ExpressionEditorTypeTable = new Dictionary<string, Type>();
        static Dictionary<string, CreateExpressionFromStringCallback> ConvertFromStringDelegates = new Dictionary<string, CreateExpressionFromStringCallback>();

        internal static void RegisterExpressionActivityService(string name, Type expressionEditorType, CreateExpressionFromStringCallback convertFromString)
        {
            if (!typeof(ExpressionActivityEditor).IsAssignableFrom(expressionEditorType))
            {
                throw FxTrace.Exception.AsError(new ArgumentException(string.Format(CultureInfo.CurrentUICulture, SR.InvalidExpressionEditorType,
                    expressionEditorType.FullName, typeof(ExpressionActivityEditor).FullName)));
            }
            if (expressionEditorType.GetConstructor(System.Type.EmptyTypes) == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(string.Format(CultureInfo.CurrentUICulture, SR.ExpressionActivityEditorRequiresDefaultConstructor,
                    expressionEditorType.FullName)));
            }
            if (!ExpressionEditorTypeTable.Keys.Contains(name))
            {
                ExpressionEditorTypeTable.Add(name, expressionEditorType);
            }
            else
            {
                ExpressionEditorTypeTable[name] = expressionEditorType;
            }

            if (convertFromString != null)
            {
                if (!ConvertFromStringDelegates.Keys.Contains(name))
                {
                    ConvertFromStringDelegates.Add(name, convertFromString);
                }
                else
                {
                    ConvertFromStringDelegates[name] = convertFromString;
                }
            }
        }

        internal bool TryCreateEditor(string expressionEditorName, out ExpressionActivityEditor editorInstance)
        {
            editorInstance = null;
            if (expressionEditorName == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException(expressionEditorName));
            }

            if (!ExpressionEditorTypeTable.Keys.Contains(expressionEditorName))
            {
                return false;
            }
            Type expressionEditorType = ExpressionEditorTypeTable[expressionEditorName];
            editorInstance = Activator.CreateInstance(expressionEditorType) as ExpressionActivityEditor;
            return true;
        }

        internal IExpressionEditorService GetExpressionService(ExpressionActivityEditor editor)
        {
            TextualExpressionEditor textualEditor = editor as TextualExpressionEditor;
            if (textualEditor != null)
            {
                return textualEditor.ExpressionEditorService;
            }
            else
            {
                return null;
            }
        }

        internal string GetExpressionEditorType(string localSetting, object root, FrameworkName targetFramework)
        {
            string expressionEditorType = null;
            if (targetFramework.Is45OrHigher())
            {
                //1) check local setting of ETB
                if (expressionEditorType == null)
                {
                    expressionEditorType = localSetting;
                }
                //2) check global setting on root object of the XAML
                if (expressionEditorType == null && root != null)
                {
                    expressionEditorType = ExpressionEditor.GetExpressionActivityEditor(root);
                }

                //3) for all the other cases, always load VB editor for backward compatibility
                if (expressionEditorType == null)
                {
                    expressionEditorType = VisualBasicEditor.ExpressionLanguageName;
                }
            }
            else
            {
                //When the targeting framework is less than 4.5, always load VB editor
                expressionEditorType = VisualBasicEditor.ExpressionLanguageName;
            }

            return expressionEditorType;
        }

        void SetBinding(string path, DependencyProperty property, FrameworkElement target, ExpressionTextBox source,
            BindingMode mode = BindingMode.OneWay, IEnumerable<ValidationRule> validationRules = null)
        {
            Binding binding = new Binding(path);
            binding.Source = source;
            binding.Mode = mode;
            if ((validationRules != null) && (validationRules.Count<ValidationRule>() > 0))
            {
                foreach (ValidationRule rule in validationRules)
                {
                    binding.ValidationRules.Add(rule);
                }
            }
            target.SetBinding(property, binding);
        }

        internal void InitializeEditor(ExpressionActivityEditor editor, ExpressionTextBox expressionTextBox)
        {
            if (editor != null)
            {
                expressionTextBox.InitializeHintText(editor.HintText);
                expressionTextBox.InternalHintText = expressionTextBox.HintText;
                SetBinding("InternalHintText", ExpressionActivityEditor.HintTextProperty, editor, expressionTextBox, BindingMode.TwoWay);
                SetBinding("IsSupportedExpression", ExpressionActivityEditor.IsSupportedExpressionProperty, editor, expressionTextBox, BindingMode.OneWayToSource, null);
                SetBinding("HorizontalScrollBarVisibility", ExpressionActivityEditor.HorizontalScrollBarVisibilityProperty, editor, expressionTextBox);
                SetBinding("VerticalScrollBarVisibility", ExpressionActivityEditor.VerticalScrollBarVisibilityProperty, editor, expressionTextBox);
                SetBinding("AcceptsReturn", ExpressionActivityEditor.AcceptsReturnProperty, editor, expressionTextBox);
                SetBinding("AcceptsTab", ExpressionActivityEditor.AcceptsTabProperty, editor, expressionTextBox);
                SetBinding("Expression", ExpressionActivityEditor.ExpressionProperty, editor, expressionTextBox, BindingMode.TwoWay, new Collection<ValidationRule> { new ExpressionValidationRule(expressionTextBox) });
                SetBinding("ExpressionType", ExpressionActivityEditor.ExpressionTypeProperty, editor, expressionTextBox);
                SetBinding("OwnerActivity", ExpressionActivityEditor.OwnerActivityProperty, editor, expressionTextBox);
                SetBinding("UseLocationExpression", ExpressionActivityEditor.UseLocationExpressionProperty, editor, expressionTextBox);
                SetBinding("PathToArgument", ExpressionActivityEditor.PathToArgumentProperty, editor, expressionTextBox);
                SetBinding("IsReadOnly", ExpressionActivityEditor.IsReadOnlyProperty, editor, expressionTextBox);
                SetBinding("ExplicitCommit", ExpressionActivityEditor.ExplicitCommitProperty, editor, expressionTextBox);
                SetBinding("ClipToBounds", ExpressionActivityEditor.ClipToBoundsProperty, editor, expressionTextBox);

                TextualExpressionEditor textEditor = editor as TextualExpressionEditor;
                if (textEditor != null)
                {
                    SetBinding("MaxLines", TextualExpressionEditor.MaxLinesProperty, textEditor, expressionTextBox);
                    SetBinding("MinLines", TextualExpressionEditor.MinLinesProperty, textEditor, expressionTextBox);
                    SetBinding("DefaultValue", TextualExpressionEditor.DefaultValueProperty, textEditor, expressionTextBox);
                }

                SetBinding("IsIndependentExpression", ExpressionActivityEditor.IsIndependentExpressionProperty, editor, expressionTextBox);
            }
        }

        internal static bool TryConvertString(string targetEditor, string expressionText, bool isLocationExpression, Type resultType, out ActivityWithResult expression)
        {
            expression = null;
            if (targetEditor != null)
            {
                CreateExpressionFromStringCallback convertFromStringAction;
                if (ConvertFromStringDelegates.TryGetValue(targetEditor, out convertFromStringAction))
                {
                    if (convertFromStringAction != null)
                    {
                        expression = convertFromStringAction(expressionText, isLocationExpression, resultType);
                        return true;
                    }
                }
            }
            return false;
        }

    }

    class ExpressionValidationRule : ValidationRule
    {
        ExpressionTextBox owner;

        public ExpressionValidationRule(ExpressionTextBox owner)
        {
            this.owner = owner;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string errorMessage = string.Empty;
            if (owner.Expression != value)
            {
                if (value != null)
                {
                    ActivityWithResult expression = (value as ModelItem).GetCurrentValue() as ActivityWithResult;
                    if (expression == null)
                    {
                        errorMessage = string.Format(CultureInfo.CurrentUICulture, SR.ExpressionTypeDonnotMatch,
                            expression.GetType().FullName, typeof(ActivityWithResult).FullName);
                    }
                    else if ((owner.UseLocationExpression) && (!ExpressionHelper.IsGenericLocationExpressionType(expression)))
                    {
                        errorMessage = string.Format(CultureInfo.CurrentUICulture, SR.ExpressionTypeDonnotMatch,
                            expression.GetType().FullName, typeof(Activity<Location>).FullName);
                    }
                    else if ((!owner.UseLocationExpression && (owner.ExpressionType != null) && (expression.ResultType != owner.ExpressionType)) ||
                        (owner.UseLocationExpression && (owner.ExpressionType != null) && (expression.ResultType != typeof(Location<>).MakeGenericType(owner.ExpressionType))))
                    {
                        errorMessage = string.Format(CultureInfo.CurrentUICulture, SR.ExpressionTypeDonnotMatch,
                            expression.GetType().FullName, typeof(Activity<>).MakeGenericType(owner.ExpressionType).FullName);
                    }
                }
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                //Disable ToolTip on inner editor if it has
                ToolTipService.SetIsEnabled(owner.Editor, false);
                owner.ToolTip = errorMessage;
                return new ValidationResult(false, errorMessage);
            }
            else
            {
                ToolTipService.SetIsEnabled(owner.Editor, true);
                owner.ToolTip = null;
                return new ValidationResult(true, null);
            }
        }
    }
}
