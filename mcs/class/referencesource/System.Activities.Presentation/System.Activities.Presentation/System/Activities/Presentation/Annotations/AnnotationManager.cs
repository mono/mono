//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Annotations
{
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.ComponentModel;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Threading;

    internal class AnnotationManager
    {
        private bool isInitialized;

        private WorkflowViewElement workflowViewElement;
        private IAnnotationVisualProvider annotationVisualProvider;

        private ModelItem modelItem;
        private EditingContext editingContext;
        private IAnnotationIndicator indicator;
        private IDockedAnnotation dockedAnnotation;
        private IFloatingAnnotation floatingAnnotation;

        private bool isViewStateChangedInternally;
        private AnnotationStatus status;
        private AnnotationAdorner annotationAdorner;
        private DispatcherTimer tryHideTimer;
        private bool hasAnnotation;

        private IIntegratedHelpService helpService;

        internal AnnotationManager(WorkflowViewElement workflowViewElement)
        {
            this.workflowViewElement = workflowViewElement;
        }

        public IAnnotationVisualProvider AnnotationVisualProvider
        {
            get
            {
                return this.annotationVisualProvider;
            }

            set
            {
                if (this.annotationVisualProvider != value)
                {
                    if (this.isInitialized)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotChangeValueAfterInitialization));
                    }

                    this.annotationVisualProvider = value;
                }
            }
        }

        protected internal virtual AnnotationAdornerService AnnotationAdornerService
        {
            get
            {
                return this.EditingContext.Services.GetService<AnnotationAdornerService>();
            }
        }

        private ModelItem ModelItem
        {
            get
            {
                return this.modelItem;
            }

            set
            {
                if (this.modelItem != value)
                {
                    Fx.Assert(!this.isInitialized, "could not change value after initialization");

                    this.modelItem = value;
                }
            }
        }

        private EditingContext EditingContext
        {
            get
            {
                return this.editingContext;
            }

            set
            {
                if (this.editingContext != value)
                {
                    Fx.Assert(!this.isInitialized, "could not change value after initialization");

                    this.editingContext = value;
                }
            }
        }

        private IAnnotationIndicator Indicator
        {
            get
            {
                if (this.indicator == null)
                {
                    this.indicator = this.AnnotationVisualProvider.GetAnnotationIndicator();
                }

                return this.indicator;
            }
        }

        private IFloatingAnnotation FloatingAnnotation
        {
            get
            {
                if (this.floatingAnnotation == null)
                {
                    this.floatingAnnotation = this.AnnotationVisualProvider.GetFloatingAnnotation();

                    this.floatingAnnotation.IsReadOnly = this.EditingContext.Items.GetValue<ReadOnlyState>().IsReadOnly;
                    this.floatingAnnotation.IsMouseOverChanged += new EventHandler(this.OnFloatingAnnotationIsMouseOverChanged);
                    this.floatingAnnotation.IsKeyboardFocusWithinChanged += new DependencyPropertyChangedEventHandler(this.OnFloatingAnnotationIsKeyboardFocusWithinChanged);
                    this.floatingAnnotation.DockButtonClicked += new Action(this.OnDockButtonClicked);
                    this.floatingAnnotation.ModelItem = this.ModelItem;
                }

                return this.floatingAnnotation;
            }
        }

        private IDockedAnnotation DockedAnnotation
        {
            get
            {
                if (this.dockedAnnotation == null)
                {
                    this.dockedAnnotation = this.AnnotationVisualProvider.GetDockedAnnotation();

                    this.dockedAnnotation.IsReadOnly = this.EditingContext.Items.GetValue<ReadOnlyState>().IsReadOnly;
                    this.dockedAnnotation.UndockButtonClicked += new Action(this.OnUndockButtonClicked);
                }

                return this.dockedAnnotation;
            }
        }

        private ViewStateService ViewStateService
        {
            get
            {
                return this.EditingContext.Services.GetService<ViewStateService>();
            }
        }

        private AnnotationAdorner AnnotationAdorner
        {
            get
            {
                if (this.annotationAdorner == null)
                {
                    this.annotationAdorner = new AnnotationAdorner(this.workflowViewElement);
                    this.annotationAdorner.Content = this.FloatingAnnotation as UIElement;
                }

                return this.annotationAdorner;
            }
        }

        private bool IsAnnotationDocked
        {
            get
            {
                bool? value = this.ViewStateService.RetrieveViewState(this.ModelItem, Annotation.IsAnnotationDockedViewStateName) as bool?;
                if (value.HasValue)
                {
                    return value.Value;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                this.isViewStateChangedInternally = true;
                this.ViewStateService.StoreViewState(this.ModelItem, Annotation.IsAnnotationDockedViewStateName, value);
                this.isViewStateChangedInternally = false;
            }
        }

        private DispatcherTimer TryHideTimer
        {
            get
            {
                if (this.tryHideTimer == null)
                {
                    this.tryHideTimer = new DispatcherTimer();
                    this.tryHideTimer.Interval = TimeSpan.FromMilliseconds(200);
                    this.tryHideTimer.Tick += this.TryHideAnnotation;
                }

                return this.tryHideTimer;
            }
        }

        public void Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }

            this.ModelItem = this.workflowViewElement.ModelItem;
            this.EditingContext = this.workflowViewElement.Context;

            if (!this.CanInitialize())
            {
                return;
            }

            this.EditingContext.Items.Subscribe<ReadOnlyState>(this.OnReadOnlyStateChanged);
            this.ViewStateService.ViewStateChanged += new ViewStateChangedEventHandler(this.OnViewStateChanged);
            this.ModelItem.PropertyChanged += new PropertyChangedEventHandler(this.OnModelItemPropertyChanged);
            this.Indicator.IsMouseOverChanged += new EventHandler(this.OnIndicatorIsMouseOverChanged);
            this.helpService = this.EditingContext.Services.GetService<IIntegratedHelpService>();

            this.hasAnnotation = this.ModelItem.Properties[Annotation.AnnotationTextPropertyName].ComputedValue != null;

            if (this.ModelItem.Properties[Annotation.AnnotationTextPropertyName].ComputedValue == null)
            {
                this.Indicator.Visibility = Visibility.Collapsed;
                if (this.dockedAnnotation != null)
                {
                    this.DockedAnnotation.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (this.IsAnnotationDocked)
                {
                    this.Indicator.Visibility = Visibility.Collapsed;
                    this.DockedAnnotation.Visibility = Visibility.Visible;
                    this.status = AnnotationStatus.Docked;
                }
                else
                {
                    this.Indicator.Visibility = Visibility.Visible;
                    this.DockedAnnotation.Visibility = Visibility.Collapsed;
                }
            }

            this.isInitialized = true;
        }

        public void Uninitialize()
        {
            if (!this.isInitialized)
            {
                return;
            }

            this.EditingContext.Items.Unsubscribe<ReadOnlyState>(this.OnReadOnlyStateChanged);
            this.ViewStateService.ViewStateChanged -= new ViewStateChangedEventHandler(this.OnViewStateChanged);
            this.ModelItem.PropertyChanged -= new PropertyChangedEventHandler(this.OnModelItemPropertyChanged);
            this.Indicator.IsMouseOverChanged -= new EventHandler(this.OnIndicatorIsMouseOverChanged);

            if (this.dockedAnnotation != null)
            {
                this.dockedAnnotation.UndockButtonClicked -= new Action(this.OnUndockButtonClicked);
            }

            if (this.floatingAnnotation != null)
            {
                this.floatingAnnotation.IsMouseOverChanged -= new EventHandler(this.OnFloatingAnnotationIsMouseOverChanged);
                this.floatingAnnotation.IsKeyboardFocusWithinChanged -= new DependencyPropertyChangedEventHandler(this.OnFloatingAnnotationIsKeyboardFocusWithinChanged);
                this.floatingAnnotation.DockButtonClicked -= new Action(this.OnDockButtonClicked);
            }

            if (this.tryHideTimer != null)
            {
                this.tryHideTimer.Tick -= this.TryHideAnnotation;
                if (this.tryHideTimer.IsEnabled)
                {
                    this.tryHideTimer.Stop();
                }
            }

            this.tryHideTimer = null;

            this.isInitialized = false;
        }

        public void OnEditAnnotation()
        {
            if (!this.isInitialized)
            {
                return;
            }

            if (this.status == AnnotationStatus.Docked)
            {
                this.DockedAnnotation.FocusOnContent();
                return;
            }

            if (this.status == AnnotationStatus.Hidden)
            {
                this.ShowAnnotation();
            }

            this.FloatingAnnotation.FocusOnContent();
        }

        private void OnReadOnlyStateChanged(ReadOnlyState state)
        {
            if (this.floatingAnnotation != null)
            {
                this.floatingAnnotation.IsReadOnly = state.IsReadOnly;
            }

            if (this.dockedAnnotation != null)
            {
                this.dockedAnnotation.IsReadOnly = state.IsReadOnly;
            }
        }

        private void OnViewStateChanged(object sender, ViewStateChangedEventArgs e)
        {
            if (e.ParentModelItem == this.ModelItem && e.Key == Annotation.IsAnnotationDockedViewStateName && !this.isViewStateChangedInternally)
            {
                bool? isAnnotationDocked = e.NewValue as bool?;
                if (isAnnotationDocked.HasValue)
                {
                    if (this.hasAnnotation)
                    {
                        if (isAnnotationDocked.Value)
                        {
                            this.DockAnnotation();
                        }
                        else
                        {
                            this.HideAnnotation();
                        }
                    }
                }
            }
        }

        private void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Annotation.AnnotationTextPropertyName)
            {
                bool previouslyHadAnnotation = this.hasAnnotation;
                this.hasAnnotation = this.ModelItem.Properties[Annotation.AnnotationTextPropertyName].ComputedValue != null;

                if (!this.hasAnnotation)
                {
                    //// annotation is removed

                    if (this.status == AnnotationStatus.Floating)
                    {
                        this.AnnotationAdornerService.Hide(this.AnnotationAdorner);
                    }

                    this.Indicator.Visibility = Visibility.Collapsed;
                    if (this.dockedAnnotation != null)
                    {
                        this.dockedAnnotation.Visibility = Visibility.Collapsed;
                    }

                    this.status = AnnotationStatus.Hidden;
                }
                else if (!previouslyHadAnnotation)
                {
                    //// annotation is added

                    if (this.IsAnnotationDocked)
                    {
                        this.Indicator.Visibility = Visibility.Collapsed;
                        this.DockedAnnotation.Visibility = Visibility.Visible;
                        this.status = AnnotationStatus.Docked;
                    }
                    else
                    {
                        this.Indicator.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void OnUndockButtonClicked()
        {
            this.ShowAnnotation();
            this.OnEditAnnotation();
        }

        private void OnFloatingAnnotationIsMouseOverChanged(object sender, EventArgs e)
        {
            if (!this.FloatingAnnotation.IsMouseOver)
            {
                if (this.status == AnnotationStatus.Floating)
                {
                    this.DelayedTryHide();
                }
            }
        }

        private void OnFloatingAnnotationIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.FloatingAnnotation.IsKeyboardFocusWithin)
            {
                this.TryHideAnnotation(null, null);
                if (this.helpService != null)
                {
                    this.helpService.RemoveContextAttribute(string.Empty, typeof(Annotation).FullName);
                    this.helpService.AddContextAttribute(string.Empty, WorkflowViewManager.GetF1HelpTypeKeyword(this.ModelItem.ItemType), ComponentModel.Design.HelpKeywordType.F1Keyword);
                }
            }
            else
            {
                Selection.SelectOnly(this.EditingContext, this.ModelItem);
                if (this.helpService != null)
                {
                    this.helpService.RemoveContextAttribute(string.Empty, WorkflowViewManager.GetF1HelpTypeKeyword(this.ModelItem.ItemType));
                    this.helpService.AddContextAttribute(string.Empty, typeof(Annotation).FullName, ComponentModel.Design.HelpKeywordType.F1Keyword);
                }
            }
        }

        private void OnIndicatorIsMouseOverChanged(object sender, EventArgs e)
        {
            if (this.Indicator.IsMouseOver)
            {
                if (this.status == AnnotationStatus.Hidden)
                {
                    this.ShowAnnotation();
                }
            }
            else
            {
                if (this.status == AnnotationStatus.Floating)
                {
                    this.DelayedTryHide();
                }
            }
        }

        private void OnDockButtonClicked()
        {
            this.DockAnnotation();
        }

        private bool CanInitialize()
        {
            return this.ModelItem != null &&
                   this.EditingContext != null &&
                   this.AnnotationVisualProvider != null &&
                   this.EditingContext.Services.GetService<DesignerConfigurationService>().AnnotationEnabled;
        }

        private void TryHideAnnotation(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            if (timer != null)
            {
                timer.Stop();
            }

            if (this.status == AnnotationStatus.Floating)
            {
                if (!this.FloatingAnnotation.IsMouseOver &&
                    !this.Indicator.IsMouseOver &&
                    !this.FloatingAnnotation.IsKeyboardFocusWithin)
                {
                    this.HideAnnotation();
                }
            }
        }

        private void DelayedTryHide()
        {
            if (this.TryHideTimer.IsEnabled)
            {
                this.TryHideTimer.Stop();
            }

            this.TryHideTimer.Start();
        }

        private void ShowAnnotation()
        {
            System.Diagnostics.Debug.WriteLine("ShowAnnotation called.");

            switch (this.status)
            {
                case AnnotationStatus.Floating:
                    return;
                case AnnotationStatus.Hidden:
                    this.AnnotationAdornerService.Show(this.AnnotationAdorner);
                    break;
                case AnnotationStatus.Docked:
                    if (this.Indicator != null)
                    {
                        this.Indicator.Visibility = Visibility.Visible;
                    }

                    this.DockedAnnotation.Visibility = Visibility.Collapsed;

                    this.AnnotationAdornerService.Show(this.AnnotationAdorner);
                    break;
            }

            this.status = AnnotationStatus.Floating;
            this.IsAnnotationDocked = false;
        }

        private void HideAnnotation()
        {
            System.Diagnostics.Debug.WriteLine("HideAnnotation called.");

            if (this.status == AnnotationStatus.Hidden)
            {
                return;
            }

            if (this.status == AnnotationStatus.Floating)
            {
                this.FloatingAnnotation.UpdateModelItem();
                this.AnnotationAdornerService.Hide(this.AnnotationAdorner);
            }

            if (this.status == AnnotationStatus.Docked)
            {
                this.Indicator.Visibility = Visibility.Visible;

                this.DockedAnnotation.Visibility = Visibility.Collapsed;
            }

            this.status = AnnotationStatus.Hidden;
            this.IsAnnotationDocked = false;
        }

        private void DockAnnotation()
        {
            System.Diagnostics.Debug.WriteLine("DockAnnotation called.");

            if (this.status == AnnotationStatus.Docked)
            {
                return;
            }

            if (this.status == AnnotationStatus.Floating)
            {
                this.AnnotationAdornerService.Hide(this.AnnotationAdorner);
            }

            this.Indicator.Visibility = Visibility.Collapsed;
            this.DockedAnnotation.Visibility = Visibility.Visible;

            this.status = AnnotationStatus.Docked;
            this.IsAnnotationDocked = true;
        }
    }
}
