//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Media;
    using Microsoft.Activities.Presentation;

    partial class FlowDecisionDesigner
    {
        public static readonly DependencyProperty ExpressionButtonVisibilityProperty =
            DependencyProperty.Register("ExpressionButtonVisibility", typeof(Visibility), typeof(FlowDecisionDesigner));

        public static readonly DependencyProperty ExpressionButtonColorProperty =
            DependencyProperty.Register("ExpressionButtonColor", typeof(Brush), typeof(FlowDecisionDesigner));

        static readonly DependencyProperty ShowAllConditionsProperty =
            DependencyProperty.Register("ShowAllConditions", typeof(bool), typeof(FlowDecisionDesigner),
            new UIPropertyMetadata(new PropertyChangedCallback(OnShowAllConditionsChanged)));

        bool isPinned;
        bool expressionShown = false;

        private AnnotationManager annotationManager;

        public FlowDecisionDesigner()
        {
            InitializeComponent();
            this.Loaded += (sender, e) =>
            {
                //UnRegistering because of 137896: Inside tab control multiple Loaded events happen without an Unloaded event.
                this.ModelItem.PropertyChanged -= OnModelItemPropertyChanged;
                this.ModelItem.PropertyChanged += OnModelItemPropertyChanged;
                OnModelItemPropertyChanged(this.ModelItem, new PropertyChangedEventArgs("Condition"));

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
            this.annotationManager.AnnotationVisualProvider = new FlowDecisionDesignerAnnotationVisualProvider(this);
        }

        void SetupBinding()
        {
            Binding showAllConditionsBinding = new Binding();
            showAllConditionsBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(FlowchartDesigner), 1);
            showAllConditionsBinding.Path = new PropertyPath(FlowchartDesigner.ShowAllConditionsProperty);
            showAllConditionsBinding.Mode = BindingMode.OneWay;

            BindingOperations.SetBinding(this, FlowDecisionDesigner.ShowAllConditionsProperty, showAllConditionsBinding);
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
            Type type = typeof(FlowDecision);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(FlowDecisionDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("True"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("False"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Condition"), new HidePropertyInOutlineViewAttribute());
            builder.AddCustomAttributes(type, new FeatureAttribute(typeof(FlowDecisionLabelFeature)));
            builder.AddCustomAttributes(type, new ActivityDesignerOptionsAttribute
            {
                AllowDrillIn = false,
                OutlineViewIconProvider = (modelItem) =>
                {
                    if (modelItem != null)
                    {
                        ResourceDictionary icons = EditorResources.GetIcons();
                        if (icons.Contains("FlowDecisionIcon") && icons["FlowDecisionIcon"] is DrawingBrush)
                        {
                            return (DrawingBrush)icons["FlowDecisionIcon"];
                        }
                    }

                    return null;
                }
            });
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new FlowchartExpressionAutomationPeer(this, base.OnCreateAutomationPeer());
        }


        void OnExpressionButtonClicked(object sender, RoutedEventArgs e)
        {
            this.isPinned = !this.isPinned;
            Update();
        }

        void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Condition")
            {
                Update();
            }
        }

        void Update()
        {
            Activity expressionActivity = this.ModelItem.Properties["Condition"].ComputedValue as Activity;
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
                FlowDecisionDesigner designer = obj as FlowDecisionDesigner;
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

        private class FlowDecisionDesignerAnnotationVisualProvider : IAnnotationVisualProvider
        {
            private FlowDecisionDesigner designer;
            private IAnnotationIndicator indicator;
            private IFloatingAnnotation floatingAnnotation;
            private IDockedAnnotation dockedAnnotation;

            public FlowDecisionDesignerAnnotationVisualProvider(FlowDecisionDesigner designer)
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
