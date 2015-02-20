//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;

    public class ActivityDesigner : WorkflowViewElement
    {
        UserControl defaultDisplayNameReadOnlyControl;
        TextBox defaultDisplayNameBox;
        bool defaultDisplayNameReadOnlyControlMouseDown;

        private AnnotationManager annotationManager;

        [Fx.Tag.KnownXamlExternal]
        public DrawingBrush Icon
        {
            get { return (DrawingBrush)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(DrawingBrush), typeof(ActivityDesigner), new UIPropertyMetadata(null));

        internal static readonly DependencyProperty ActivityDelegatesProperty = DependencyProperty.Register("ActivityDelegates", typeof(ObservableCollection<ActivityDelegateInfo>), typeof(ActivityDesigner));

        internal static readonly DependencyProperty HasActivityDelegatesProperty = DependencyProperty.Register("HasActivityDelegates", typeof(bool), typeof(ActivityDesigner));

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline,
            Justification = "Calls to OverrideMetadata for a dependency property should be done in the static constructor.")]
        static ActivityDesigner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityDesigner), new FrameworkPropertyMetadata(typeof(ActivityDesigner)));
        }

        public ActivityDesigner()
        {
            this.Loaded += (sender, args) =>
            {
                this.SetupDefaultIcon();
                this.annotationManager.Initialize();
            };

            this.Unloaded += (sender, args) =>
            {
                this.annotationManager.Uninitialize();
            };

            this.annotationManager = new AnnotationManager(this);
        }

        internal ObservableCollection<ActivityDelegateInfo> ActivityDelegates
        {
            get { return (ObservableCollection<ActivityDelegateInfo>)GetValue(ActivityDelegatesProperty); }
            set { SetValue(ActivityDelegatesProperty, value); }
        }

        internal bool HasActivityDelegates
        {
            get { return (bool)GetValue(HasActivityDelegatesProperty); }
            set { SetValue(HasActivityDelegatesProperty, value); }
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            this.PopulateActivityDelegates((ModelItem)newItem);
        }

        private void PopulateActivityDelegates(ModelItem modelItem)
        {
            if (this.ActivityDelegates == null)
            {
                this.ActivityDelegates = new ObservableCollection<ActivityDelegateInfo>();
            }
            else
            {
                this.ActivityDelegates.Clear();
            }

            List<ActivityDelegateInfo> list = ActivityDelegateUtilities.CreateActivityDelegateInfo(modelItem);

            if (list.Count > 0)
            {
                foreach (ActivityDelegateInfo entry in list)
                {
                    this.ActivityDelegates.Add(entry);
                }

                this.HasActivityDelegates = true;
            }
            else
            {
                this.HasActivityDelegates = false;
            }
        }

        protected override string GetAutomationIdMemberName()
        {
            return "DisplayName";
        }

        protected internal override string GetAutomationItemStatus()
        {
            StringBuilder descriptiveText = new StringBuilder();

            EmitPropertyValuePair(descriptiveText, "IsPrimarySelection");
            EmitPropertyValuePair(descriptiveText, "IsSelection");
            EmitPropertyValuePair(descriptiveText, "IsCurrentLocation");
            EmitPropertyValuePair(descriptiveText, "IsCurrentContext");
            EmitPropertyValuePair(descriptiveText, "IsBreakpointEnabled");
            EmitPropertyValuePair(descriptiveText, "IsBreakpointBounded");
            EmitPropertyValuePair(descriptiveText, "ValidationState");
            descriptiveText.Append(base.GetAutomationItemStatus());

            return descriptiveText.ToString();
        }

        void EmitPropertyValuePair(StringBuilder description, string propertyName)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(this.ModelItem)[propertyName];
            object propertyValue = (property == null) ? null : property.GetValue(this.ModelItem);
            string propertyValueString = propertyValue == null ? "null" : propertyValue.ToString();
            description.AppendFormat("{0}={1} ", propertyName, propertyValueString);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.defaultDisplayNameBox != null)
            {
                this.defaultDisplayNameBox.LostFocus -= new RoutedEventHandler(OnDefaultDisplayNameBoxLostFocus);
                this.defaultDisplayNameBox.ContextMenuOpening -= new ContextMenuEventHandler(OnDefaultDisplayNameBoxContextMenuOpening);
            }
            if (this.defaultDisplayNameReadOnlyControl != null)
            {
                this.defaultDisplayNameReadOnlyControl.MouseLeftButtonDown -= new MouseButtonEventHandler(OnDefaultDisplayNameReadOnlyControlMouseLeftButtonDown);
                this.defaultDisplayNameReadOnlyControl.GotKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnDefaultDisplayNameReadOnlyControlGotKeyboardFocus);
            }

            this.defaultDisplayNameReadOnlyControl = this.Template.FindName("DisplayNameReadOnlyControl_6E8E4954_F6B2_4c6c_9E28_33A7A78F1E81", this) as UserControl;
            this.defaultDisplayNameBox = this.Template.FindName("DisplayNameBox_570C5205_7195_4d4e_953A_8E4B57EF7E7F", this) as TextBox;

            UIElement defaultAnnotationIndicator = this.Template.FindName("AnnotationIndicator_570C5205_7195_4d4e_953A_8E4B57EF7E7F", this) as UIElement;
            DockedAnnotationDecorator defaultDockedAnnotationDecorator = this.Template.FindName("DockedAnnotationDecorator_570C5205_7195_4d4e_953A_8E4B57EF7E7F", this) as DockedAnnotationDecorator;

            if (defaultAnnotationIndicator != null && defaultDockedAnnotationDecorator != null)
            {
                this.annotationManager.AnnotationVisualProvider = new ActivityDesignerAnnotationVisualProvider(new UIElementToAnnotationIndicatorAdapter(defaultAnnotationIndicator), defaultDockedAnnotationDecorator);
            }

            if (this.defaultDisplayNameBox != null && this.defaultDisplayNameReadOnlyControl != null)
            {
                this.defaultDisplayNameBox.LostFocus += new RoutedEventHandler(OnDefaultDisplayNameBoxLostFocus);
                this.defaultDisplayNameBox.ContextMenuOpening += new ContextMenuEventHandler(OnDefaultDisplayNameBoxContextMenuOpening);
                this.defaultDisplayNameReadOnlyControl.MouseLeftButtonDown += new MouseButtonEventHandler(OnDefaultDisplayNameReadOnlyControlMouseLeftButtonDown);
                this.defaultDisplayNameReadOnlyControl.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnDefaultDisplayNameReadOnlyControlGotKeyboardFocus);
            }

            Border titleBar = this.Template.FindName("TitleBar_C36A1CF2_4B36_4F0D_B427_9825C2E110DE", this) as Border;
            if (titleBar != null)
            {
                this.DragHandle = titleBar;
            }
        }

        void OnDefaultDisplayNameReadOnlyControlGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (this.defaultDisplayNameBox != null && this.defaultDisplayNameReadOnlyControl != null)
            {
                DesignerView designerView = this.Context.Services.GetService<DesignerView>();
                if (!designerView.IsReadOnly && !designerView.IsMultipleSelectionMode)
                {
                    this.EnterDisplayNameEditMode();
                }
            }
        }

        void OnDefaultDisplayNameReadOnlyControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.defaultDisplayNameReadOnlyControlMouseDown = true;
        }

        void OnDefaultDisplayNameBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.defaultDisplayNameBox != null && this.defaultDisplayNameReadOnlyControl != null)
            {
                this.ExitDisplayNameEditMode();
            }
        }

        void OnDefaultDisplayNameBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // to disable the context menu
            e.Handled = true;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            this.defaultDisplayNameReadOnlyControlMouseDown = false;
            base.OnPreviewMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            // We have to check the defaultDisplayNameReadOnlyControlMouseDown flag to determine whether the mouse is clicked on 
            // the defaultDisplayNameReadOnlyControl. This is because the mouse capture is set on the WorkflowViewElement in 
            // OnMouseDown, and as a result MouseUp event is not fired on the defaultDisplayNameReadOnlyControl.
            if (this.defaultDisplayNameBox != null && this.defaultDisplayNameReadOnlyControl != null &&
                this.defaultDisplayNameReadOnlyControlMouseDown)
            {
                this.defaultDisplayNameReadOnlyControlMouseDown = false;
                DesignerView designerView = this.Context.Services.GetService<DesignerView>();
                if (!designerView.IsReadOnly)
                {
                    this.EnterDisplayNameEditMode();
                }
            }
            base.OnMouseUp(e);
        }

        void EnterDisplayNameEditMode()
        {
            this.defaultDisplayNameBox.Visibility = Visibility.Visible;
            this.defaultDisplayNameReadOnlyControl.Visibility = Visibility.Collapsed;
            this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
            {
                Keyboard.Focus(this.defaultDisplayNameBox);
                this.defaultDisplayNameBox.ScrollToHome();
            }));
        }

        void ExitDisplayNameEditMode()
        {
            this.defaultDisplayNameReadOnlyControl.Visibility = Visibility.Visible;
            this.defaultDisplayNameBox.Visibility = Visibility.Collapsed;
        }

        private void SetupDefaultIcon()
        {
            if (this.Icon == null)
            {
                this.Icon = GetDefaultIcon();
            }
        }

        internal DrawingBrush GetDefaultIcon()
        {
            DrawingBrush icon = null;

            // Look for a named icon if this property is not set

            if (this.ModelItem != null)
            {
                string iconKey = this.ModelItem.ItemType.IsGenericType ? this.ModelItem.ItemType.GetGenericTypeDefinition().Name : this.ModelItem.ItemType.Name;
                int genericParamsIndex = iconKey.IndexOf('`');
                if (genericParamsIndex > 0)
                {
                    iconKey = iconKey.Remove(genericParamsIndex);
                }
                iconKey = iconKey + "Icon";
                try
                {
                    if (WorkflowDesignerIcons.IconResourceDictionary.Contains(iconKey))
                    {
                        object resourceItem = WorkflowDesignerIcons.IconResourceDictionary[iconKey];
                        if (resourceItem is DrawingBrush)
                        {
                            icon = (DrawingBrush)resourceItem;
                        }
                    }
                }
                catch (ResourceReferenceKeyNotFoundException) { }
                catch (InvalidCastException) { }
            }
            if (icon == null)
            {
                // as a last resort fall back to the default generic leaf activity icon.
                icon = WorkflowDesignerIcons.Activities.DefaultCustomActivity;
            }

            return icon;
        }

        protected internal override void OnEditAnnotation()
        {
            this.annotationManager.OnEditAnnotation();
        }

        private class ActivityDesignerAnnotationVisualProvider : IAnnotationVisualProvider
        {
            private DockedAnnotationDecorator decorator;
            private IAnnotationIndicator indicator;
            private IFloatingAnnotation floatingAnnotation;
            private IDockedAnnotation dockedAnnotation;

            public ActivityDesignerAnnotationVisualProvider(IAnnotationIndicator indicator, DockedAnnotationDecorator decorator)
            {
                this.indicator = indicator;
                this.decorator = decorator;
            }

            public IAnnotationIndicator GetAnnotationIndicator()
            {
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

                    Binding annotationTextbinding = new Binding("ModelItem.AnnotationText");
                    view.SetBinding(DockedAnnotationView.AnnotationTextProperty, annotationTextbinding);

                    Binding maxWidthBinding = new Binding("ActualWidth");
                    maxWidthBinding.ElementName = "annotationWidthSetter";
                    view.SetBinding(DockedAnnotationView.MaxWidthProperty, maxWidthBinding);

                    this.dockedAnnotation = view;
                    this.decorator.Child = view;
                }

                return this.dockedAnnotation;
            }
        }
    }
}
