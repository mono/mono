//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.ComponentModel;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using Microsoft.Activities.Presentation;

    partial class FlowSwitchDesigner
    {
        public static readonly DependencyProperty ExpressionButtonVisibilityProperty =
            DependencyProperty.Register("ExpressionButtonVisibility", typeof(Visibility), typeof(FlowSwitchDesigner));

        public static readonly DependencyProperty ExpressionButtonColorProperty =
            DependencyProperty.Register("ExpressionButtonColor", typeof(Brush), typeof(FlowSwitchDesigner));

        static readonly DependencyProperty ShowAllConditionsProperty =
            DependencyProperty.Register("ShowAllConditions", typeof(bool), typeof(FlowSwitchDesigner),
            new UIPropertyMetadata(new PropertyChangedCallback(OnShowAllConditionsChanged)));

        bool isPinned;
        bool expressionShown = false;

        private AnnotationManager annotationManager;

        public FlowSwitchDesigner()
        {
            InitializeComponent();
            this.Loaded += (sender, e) =>
            {
                //UnRegistering because of 137896: Inside tab control multiple Loaded events happen without an Unloaded event.
                this.ModelItem.PropertyChanged -= OnModelItemPropertyChanged;
                this.ModelItem.PropertyChanged += OnModelItemPropertyChanged;
                OnModelItemPropertyChanged(this.ModelItem, new PropertyChangedEventArgs("Expression"));

                SetupBinding();

                if (this.Context.Services.GetService<DesignerConfigurationService>().TargetFrameworkName.IsLessThan45())
                {
                    this.displayNameTextBox.IsReadOnly = true;
                }

                this.annotationManager.Initialize();
            };
            this.Unloaded += (sender, e) =>
            {
                this.ModelItem.PropertyChanged -= OnModelItemPropertyChanged;
                this.annotationManager.Uninitialize();
            };
            this.MouseEnter += (sender, e) =>
            {
                Update();
            };
            this.MouseLeave += (sender, e) =>
            {
                Update();
            };

            this.InitializeAnnotation();
        }

        private void InitializeAnnotation()
        {
            this.annotationManager = new AnnotationManager(this);
            this.annotationManager.AnnotationVisualProvider = new FlowSwitchDesignerAnnotationVisualProvider(this);
        }

        void SetupBinding()
        {
            Binding showAllConditionsBinding = new Binding();
            showAllConditionsBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(FlowchartDesigner), 1);
            showAllConditionsBinding.Path = new PropertyPath(FlowchartDesigner.ShowAllConditionsProperty);
            showAllConditionsBinding.Mode = BindingMode.OneWay;

            BindingOperations.SetBinding(this, FlowSwitchDesigner.ShowAllConditionsProperty, showAllConditionsBinding);
        }

        public Visibility ExpressionButtonVisibility
        {
            get { return (Visibility)GetValue(ExpressionButtonVisibilityProperty); }
            set { SetValue(ExpressionButtonVisibilityProperty, value); }
        }

        public Brush ExpressionButtonColor
        {
            get { return (Brush)GetValue(ExpressionButtonColorProperty); }
            set { SetValue(ExpressionButtonColorProperty, value); }
        }

        public bool ExpressionShown
        {
            get { return this.expressionShown; }
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(System.Activities.Statements.FlowSwitch<>);

            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(FlowSwitchDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Default"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, new ActivityDesignerOptionsAttribute
            {
                AllowDrillIn = false,
                OutlineViewIconProvider = (modelItem) =>
                {
                    if (modelItem != null)
                    {
                        ResourceDictionary icons = EditorResources.GetIcons();
                        if (icons.Contains("FlowSwitchIcon") && icons["FlowSwitchIcon"] is DrawingBrush)
                        {
                            return (DrawingBrush)icons["FlowSwitchIcon"];
                        }
                    }

                    return null;
                }
            });
            builder.AddCustomAttributes(type, new FeatureAttribute(typeof(FlowSwitchLabelFeature)));
            builder.AddCustomAttributes(type, new FeatureAttribute(typeof(FlowSwitchDefaultLinkFeature)));

            builder.AddCustomAttributes(type, type.GetProperty("Cases"), new ShowPropertyInOutlineViewAttribute() { CurrentPropertyVisible = false, ChildNodePrefix = "Case : " });
            builder.AddCustomAttributes(type, type.GetProperty("Expression"), new HidePropertyInOutlineViewAttribute());

            Type flowSwitchLinkType = typeof(FlowSwitchCaseLink<>);
            builder.AddCustomAttributes(flowSwitchLinkType, "Case", PropertyValueEditor.CreateEditorAttribute(typeof(FlowSwitchLinkCasePropertyEditor)), new EditorReuseAttribute(false));
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new FlowchartExpressionAutomationPeer(this, base.OnCreateAutomationPeer());
        }

        void OnExpressionButtonClicked(object sender, RoutedEventArgs e)
        {
            this.isPinned = !this.isPinned;
        }

        void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Expression")
            {
                Update();
            }
            else if (e.PropertyName == "DefaultCaseDisplayName")
            {
                // To fix 218600 without losing PropertyGrid focus (Bug 210326), the only workaround is to
                // update the connector label manually, because FlowSwitchLink.ModelItem["DefaultCaseDisplayName"]
                // is a FakeModelPropertyImpl, and would not generate a Undo unit 
                // (FakeModelNotifyPropertyChange.GetInverse() returns null).
                // However, there is a known issue with PropertyGrid bound to a fake ModelItem.  The workaround is 
                // to shift the focus to the FlowchartDesigner IF the keyboard focus is on the connector when the user
                // calls Undo/Redo, to avoid the problem of PropertyGrid not refreshable.
                FlowchartDesigner flowchartDesigner = VisualTreeUtils.FindVisualAncestor<FlowchartDesigner>(this);
                Fx.Assert(null != flowchartDesigner, "flowchart designer cannot be null because FlowswitchDesigner must exist within the same visual tree ofthe parent Flowchart.");

                if (null != flowchartDesigner &&
                    null != this.ModelItem.Properties["Default"].Value &&
                    this.Context.Services.GetService<UndoEngine>().IsUndoRedoInProgress)
                {
                    // the designer is available
                    Connector connector = flowchartDesigner.GetLinkOnCanvas(this.ModelItem, this.ModelItem.Properties["Default"].Value, "Default");
                    Fx.Assert(null != connector, "Connector should not be null.");
                    ModelItem linkModelItem = FlowchartDesigner.GetLinkModelItem(connector);
                    Fx.Assert(linkModelItem is FakeModelItemImpl, "ModelItem of FlowSwitch link is fake.");
                    IFlowSwitchDefaultLink link = (IFlowSwitchDefaultLink)linkModelItem.GetCurrentValue();
                    string defaultDisplayName =
                        (string)this.ModelItem.Properties[FlowSwitchLabelFeature.DefaultCaseDisplayNamePropertyName].Value.GetCurrentValue();

                    if (link.DefaultCaseDisplayName != defaultDisplayName)
                    {
                        // the purpose of re-setting the link value during Undo/Redo is to update the FlowSwitch label
                        using (ModelEditingScope scope = this.ModelItem.BeginEdit(SR.FlowSwitchDefaultCaseDisplayNameEditingScopeDesc))
                        {
                            linkModelItem.Properties[FlowSwitchLabelFeature.DefaultCaseDisplayNamePropertyName].SetValue(defaultDisplayName);
                            link.DefaultCaseDisplayName = defaultDisplayName;
                            scope.Complete();
                        }

                        if (Selection.IsSelection(linkModelItem))
                        {
                            // cause the connector to lose focus, because the PropertyGrid would not have focus.
                            // this scenario only happens if the user explicitly selects the FlowSwitch link after
                            // editing the DefaultDisplayName.  This behavior is only a workaround due to the fact
                            // that PropertyGrid does not receive update from change in a FakeModelPropertyImpl 
                            // (i.e. FlowSwitchLink).
                            Keyboard.ClearFocus();
                            Selection.SelectOnly(this.Context, this.ModelItem);
                            linkModelItem.Highlight();
                        }
                    }
                }
            }
        }

        void Update()
        {
            Activity expressionActivity = this.ModelItem.Properties["Expression"].ComputedValue as Activity;
            string expressionString = ExpressionHelper.GetExpressionString(expressionActivity, this.ModelItem);
            bool expressionSpecified = !string.IsNullOrEmpty(expressionString);
            if (!expressionSpecified)
            {
                this.isPinned = false;
            }

            this.ExpressionButtonVisibility = expressionSpecified ? Visibility.Visible : Visibility.Collapsed;

            if (this.isPinned)
            {
                this.ExpressionButtonColor = WorkflowDesignerColors.FlowchartExpressionButtonPressedBrush;
            }
            else if (this.IsMouseOver)
            {
                this.ExpressionButtonColor = WorkflowDesignerColors.FlowchartExpressionButtonMouseOverBrush;
            }
            else
            {
                this.ExpressionButtonColor = WorkflowDesignerColors.FlowchartExpressionButtonBrush;
            }
            expressionShown = false;
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                Adorner[] adorners = adornerLayer.GetAdorners(this);
                if (adorners != null)
                {
                    foreach (Adorner adorner in adorners)
                    {
                        if (adorner is FlowchartExpressionAdorner)
                        {
                            adornerLayer.Remove(adorner);
                        }
                    }
                }
                if ((this.IsMouseOver && expressionSpecified) || this.isPinned)
                {
                    expressionShown = true;
                    adornerLayer.Add(new FlowchartExpressionAdorner(this));
                }
            }
        }

        static void OnShowAllConditionsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != DependencyProperty.UnsetValue)
            {
                FlowSwitchDesigner designer = obj as FlowSwitchDesigner;
                designer.OnShowAllConditionsChanged((bool)e.NewValue);
            }
        }

        void OnShowAllConditionsChanged(bool isOpen)
        {
            this.isPinned = isOpen;
            Update();
        }

        protected internal override void OnEditAnnotation()
        {
            this.annotationManager.OnEditAnnotation();
        }

        private class FlowSwitchDesignerAnnotationVisualProvider : IAnnotationVisualProvider
        {
            private FlowSwitchDesigner designer;
            private IAnnotationIndicator indicator;
            private IFloatingAnnotation floatingAnnotation;
            private IDockedAnnotation dockedAnnotation;

            public FlowSwitchDesignerAnnotationVisualProvider(FlowSwitchDesigner designer)
            {
                this.designer = designer;
            }

            public IAnnotationIndicator GetAnnotationIndicator()
            {
                if (this.indicator == null)
                {
                    this.indicator = new UIElementToAnnotationIndicatorAdapter(this.designer.defaultAnnotationIndicator);
                }

                return this.indicator;
            }

            public IFloatingAnnotation GetFloatingAnnotation()
            {
                if (this.floatingAnnotation == null)
                {
                    this.floatingAnnotation = new FloatingAnnotationView();
                }

                return this.floatingAnnotation;
            }

            public IDockedAnnotation GetDockedAnnotation()
            {
                if (this.dockedAnnotation == null)
                {
                    DockedAnnotationView view = new DockedAnnotationView();
                    Binding binding = new Binding("ModelItem.AnnotationText");
                    view.SetBinding(DockedAnnotationView.AnnotationTextProperty, binding);
                    view.Visibility = Visibility.Collapsed;
                    Grid.SetRow(view, 0);

                    this.dockedAnnotation = view;
                    this.designer.rootGrid.Children.Insert(0, view);
                }

                return this.dockedAnnotation;
            }
        }
    }
}
