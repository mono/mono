//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Windows;

    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Documents;

    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Documents;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.View;
    using System.Collections.ObjectModel;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using System.Windows.Shapes;
    using System.Windows.Input;
    using System.Runtime;
    using System.Activities.Presentation.Debug;
    using System.Diagnostics.CodeAnalysis;
    using System.Xaml;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    [Fx.Tag.XamlVisible(false)]
    public class VirtualizedContainerService
    {
        EditingContext context;
        QuadTree<VirtualizingContainer> tree;
        bool isWorking = false;
        DesignerView designerView;
        ViewStateService viewStateService;
        ViewService viewService;
        IDictionary<ModelItem, FrameworkElement> modelItemToContainer;

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly AttachableMemberIdentifier HintSizeName = new AttachableMemberIdentifier(typeof(VirtualizedContainerService), "HintSize");


        public VirtualizedContainerService(EditingContext context)
        {
            this.context = context;
            this.tree = new QuadTree<VirtualizingContainer>();
            this.modelItemToContainer = new Dictionary<ModelItem, FrameworkElement>();
            this.tree.Bounds = new Rect(0, 0, int.MaxValue, int.MaxValue);

            this.context.Services.Subscribe<DesignerView>((designerView) =>
            {
                designerView.ScrollViewer.ScrollChanged += (sender, args) =>
                {
                    if (!isWorking)
                    {
                        isWorking = true;
                        PopulateItemsInView();
                        isWorking = false;
                    }
                };
            });

        }


        ViewStateService ViewStateService
        {
            get
            {
                if (this.viewStateService == null)
                {
                    this.viewStateService = this.context.Services.GetService<ViewStateService>();
                }
                Fx.Assert(this.viewStateService != null, "ViewStateService should not be null");
                return this.viewStateService;
            }
        }

        ViewService ViewService
        {
            get
            {
                if (this.viewService == null)
                {
                    this.viewService = this.context.Services.GetService<ViewService>();
                }
                Fx.Assert(this.viewService != null, "ViewService should not be null");
                return this.viewService;
            }
        }


        DesignerView DesignerView
        {
            get
            {
                if (this.designerView == null)
                {
                    this.designerView = this.context.Services.GetService<DesignerView>();
                }
                Fx.Assert(this.designerView != null, "Designer view should not be null");
                return this.designerView;
            }
        }

        public static object GetHintSize(object instance)
        {
            object viewState;
            AttachablePropertyServices.TryGetProperty(instance, HintSizeName, out viewState);
            return viewState;
        }

        public static void SetHintSize(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, HintSizeName, value);
        }


        // This method populates all items in the current scroll region.
        // we first get the virtualizing containers inthe current scroll region
        // ask them to populate the content, and then wait for a layout pass
        // so that the first round of population can cause more populations
        // we do this till all items in the current view are completely populated.
        private void PopulateItemsInView()
        {
            var designers = this.tree.GetNodesInside(GetViewerBounds());
            bool rePopulationNeeded = false;
            foreach (VirtualizingContainer container in designers)
            {
                if (!container.IsPopulated)
                {
                    container.Populate();
                    rePopulationNeeded = true;
                }
            }

            if (rePopulationNeeded)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() =>
                {
                    PopulateItemsInView();
                }));
            }
        }

        // This method populates all items in the entire designer canvas
        // this uses the same technique Populateitemsinview uses to bring items into view.
        internal void BeginPopulateAll(Action onAfterPopulateAll)
        {
            Cursor oldCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            PopulateAllWithWaitCursor( oldCursor, onAfterPopulateAll);
        }

        void PopulateAllWithWaitCursor( Cursor oldCursor, Action onAfterPopulateAll)
        {
            var designers = this.tree.GetNodesInside(new Rect(0, 0, double.MaxValue, double.MaxValue));
            bool rePopulationNeeded = false;
            
            foreach (VirtualizingContainer container in designers)
            {
                if (!container.IsPopulated)
                {
                    container.Populate();
                    rePopulationNeeded = true;
                }
            }

            if (rePopulationNeeded)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(() =>
                {
                    PopulateAllWithWaitCursor(oldCursor, onAfterPopulateAll);
                }));
            }
            else
            {
                Mouse.OverrideCursor = oldCursor;
                if (onAfterPopulateAll != null)
                {
                    onAfterPopulateAll();
                }
            }
        }


        Rect GetViewerBounds()
        {
            ScrollViewer parentView = this.DesignerView.ScrollViewer;
            Rect viewerBounds = new Rect(parentView.HorizontalOffset, parentView.VerticalOffset, parentView.ViewportWidth, parentView.ViewportHeight);
            viewerBounds.Scale(1 / this.designerView.ZoomFactor, 1 / this.designerView.ZoomFactor);
            return viewerBounds;
        }

        bool IsVirtualiztionEnabled
        {
            get
            {
                return true;
            }
        }

        internal FrameworkElement QueryContainerForItem(ModelItem item)
        {
            if (null == item)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            FrameworkElement element;
            this.modelItemToContainer.TryGetValue(item, out element);
            return element;
        }

        public UIElement GetContainer(ModelItem modelItem, ICompositeView sourceContainer)
        {
            FrameworkElement view = null;
            if (IsVirtualiztionEnabled)
            {
                view = new VirtualizingContainer(this, modelItem, sourceContainer);
                view.Loaded += this.OnViewLoaded;
                view.Unloaded += this.OnViewUnloaded;
            }
            else
            {
                view = this.GetViewElement(modelItem, sourceContainer);
            }
            return view;
        }

        void OnViewLoaded(object view, RoutedEventArgs e)
        {
            var virtualView = view as VirtualizingContainer;
            var viewElement = view as WorkflowViewElement;

            if (null != virtualView && !this.modelItemToContainer.ContainsKey(virtualView.ModelItem))
            {
                this.modelItemToContainer.Add(virtualView.ModelItem, virtualView);
            }
            else if (null != viewElement && !this.modelItemToContainer.ContainsKey(viewElement.ModelItem))
            {
                this.modelItemToContainer.Add(viewElement.ModelItem, viewElement);
            }
        }

        void OnViewUnloaded(object view, RoutedEventArgs e)
        {
            var virtualView = view as VirtualizingContainer;
            var viewElement = view as WorkflowViewElement;

            if (null != virtualView && this.modelItemToContainer.ContainsKey(virtualView.ModelItem))
            {
                this.modelItemToContainer.Remove(virtualView.ModelItem);
            }
            else if (null != viewElement && this.modelItemToContainer.ContainsKey(viewElement.ModelItem))
            {
                this.modelItemToContainer.Remove(viewElement.ModelItem);
            }
        }


        public WorkflowViewElement GetViewElement(ModelItem modelItem, ICompositeView sourceContainer)
        {
            WorkflowViewElement itemView = (WorkflowViewElement)this.ViewService.GetView(modelItem);
            if (null != sourceContainer)
            {
                DragDropHelper.SetCompositeView(itemView, (UIElement)sourceContainer);
            }
            itemView.Loaded += this.OnViewLoaded;
            itemView.Unloaded += this.OnViewUnloaded;

            return itemView;
        }




        internal class VirtualizingContainer : Border
        {
            VirtualizedContainerService containerService;
            ModelItem modelItem;
            ICompositeView sourceContainer;
            UIElement designerRoot;
            bool isPopulated = false;
            Size defaultContainerSize = new Size(20, 20);
            Rect oldBounds;
            VirtualizingContainer parentContainer;
            List<VirtualizingContainer> children;

            public ModelItem ModelItem
            {
                get
                {
                    return this.modelItem;
                }
            }

            public ICompositeView ICompositeView
            {
                get
                {
                    return this.sourceContainer;
                }
            }

            public IEnumerable<VirtualizingContainer> ChildContainers
            {
                get { return this.children; }
            }

            [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
            public VirtualizingContainer(VirtualizedContainerService containerService, ModelItem modelItem, ICompositeView sourceContainer)
            {
                this.containerService = containerService;
                this.modelItem = modelItem;
                this.sourceContainer = sourceContainer;
                this.Focusable = false;
                this.BorderThickness = new Thickness(1);
                SetupPlaceHolder();
                this.children = new List<VirtualizingContainer>();
                this.Unloaded += (sender, args) =>
                {
                    this.containerService.tree.Remove(this);
                    this.oldBounds = new Rect(0, 0, 0, 0);
                    UnRegisterFromParentContainer();
                };

                this.Loaded += (sender, args) =>
                {
                    RegisterWithParentContainer();
                };

            }

            private void SetupPlaceHolder()
            {
                string sizeString = (string)(VirtualizedContainerService.GetHintSize(this.modelItem.GetCurrentValue()));
                Size? size = null;
                if (!string.IsNullOrEmpty(sizeString))
                {
                    size = Size.Parse(sizeString);
                }
                if (size == null)
                {
                    size = defaultContainerSize;
                }
                this.MinWidth = size.Value.Width;
                this.MinHeight = size.Value.Height;
            }

            protected override Size ArrangeOverride(Size arrangeBounds)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() =>
                {
                    AddToQuadTree();
                }));
                return base.ArrangeOverride(arrangeBounds);
            }


            void RegisterWithParentContainer()
            {
                DependencyObject parent = VisualTreeHelper.GetParent(this);
                while (null != parent && !(parent is VirtualizingContainer))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                this.parentContainer = parent as VirtualizingContainer;
                if (parentContainer != null)
                {
                    if (!parentContainer.children.Contains(this))
                    {
                        parentContainer.children.Add(this);
                    }
                }
            }

            void UnRegisterFromParentContainer()
            {
                if (parentContainer != null)
                {
                    parentContainer.children.Remove(this);
                    this.parentContainer = null;
                }
            }

            private void AddToQuadTree()
            {
                try
                {
                    Point currentPoint = GetPosition();
                    if (this.ActualHeight > 0 && this.ActualWidth > 0)
                    {
                        Rect bounds = new Rect(currentPoint, new Size(this.ActualWidth, this.ActualHeight));
                        Rect viewerBounds = this.containerService.GetViewerBounds();
                        bool isInView = viewerBounds.IntersectsWith(bounds) || viewerBounds.Contains(bounds) || bounds.Contains(viewerBounds);
                        if (isInView)
                        {
                            this.Populate();
                            currentPoint = GetPosition();
                            bounds = new Rect(currentPoint, new Size(this.ActualWidth, this.ActualHeight));
                        }
                        else
                        {
                            // a previous Arrange could have led to adding this to the quadtree already.
                            // so remove previos instances from quadtree.
                            if (!this.isPopulated)
                            {
                                if (this.BorderBrush != SystemColors.GrayTextBrush)
                                {
                                    this.BorderBrush = SystemColors.GrayTextBrush;
                                }
                            }
                         


                        }

                        if (this.oldBounds != bounds)
                        {

                            this.containerService.tree.Remove(this);
                            this.containerService.tree.Insert(this, bounds);
                            if (this.oldBounds != Rect.Empty)
                            {
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() =>
                                {
                                    foreach (VirtualizingContainer childContainer in this.children)
                                    {
                                        // if there were designers registered under the old bounds let them re-register
                                        childContainer.AddToQuadTree();
                                    }
                                }));
                            }

                            this.oldBounds = bounds;
                        }
                        

                        if (this.IsPopulated)
                        {
                            VirtualizedContainerService.SetHintSize(this.modelItem.GetCurrentValue(), bounds.Size.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // This can happen if an arrange happened within the child of the container, when not in the visual tree
                    // for the current breadcrumb root. The GetTransform will throw invalidoperation in this case.
                    this.containerService.tree.Remove(this);
                }
            }

            UIElement DesignerRoot
            {
                get
                {
                    if (this.designerRoot == null)
                    {
                        this.designerRoot = this.containerService.DesignerView.scrollableContent;
                    }
                    Fx.Assert(this.designerRoot != null, "Designer's scrollable content should not be null now ");
                    return this.designerRoot;
                }
            }

            private Point GetPosition()
            {
                GeneralTransform generalTransform1 = this.TransformToAncestor((Visual)this.DesignerRoot);
                // Get current position by transforming origin using the current transform.
                Point currentPoint = generalTransform1.Transform(new Point(0, 0));
                return currentPoint;
            }



            public bool IsPopulated
            {
                get
                {
                    return this.isPopulated;
                }
            }

            internal void Populate()
            {
                if (!IsPopulated)
                {
                    this.BorderBrush = Brushes.Transparent;
                    this.BorderThickness = new Thickness(0);
                    this.Child = this.containerService.GetViewElement(this.ModelItem, this.ICompositeView);
                    this.MinHeight = defaultContainerSize.Height;
                    this.MinWidth = defaultContainerSize.Width;
                    isPopulated = true;
                }
            }

        }

        internal static UIElement TryGetVirtualizedElement(UIElement element)
        {
            if (element is VirtualizedContainerService.VirtualizingContainer)
            {
                if (((VirtualizedContainerService.VirtualizingContainer)element).IsPopulated)
                {
                    return ((VirtualizedContainerService.VirtualizingContainer)element).Child;
                }
            }
            return element;
        }
    }
}




