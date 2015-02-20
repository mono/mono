//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    internal static class ViewUtilities
    {
        const string ExpandViewStateKey = "IsExpanded";
        internal static bool DoesParentAlwaysExpandChildren(ModelItem modelItem, EditingContext context)
        {
            return IsParentOfType(modelItem, typeof(IExpandChild), context);
        }

        internal static bool DoesParentAlwaysCollapseChildren(ModelItem modelItem, EditingContext context)
        {
            bool parentAlwaysCollapsesChild = false;
            Type parentDesignerType = GetParentDesignerType(modelItem, context);
            if (typeof(WorkflowViewElement).IsAssignableFrom(parentDesignerType))
            {
                ActivityDesignerOptionsAttribute options = WorkflowViewService.GetAttribute<ActivityDesignerOptionsAttribute>(parentDesignerType);
                parentAlwaysCollapsesChild = (options != null && options.AlwaysCollapseChildren);
            }
            return parentAlwaysCollapsesChild;
        }

        // Determines whether a particular ModelItem's view will be visible for a given breadcrumb root. 
        //It depends on whether the intermediate designers are expanded or collapsed.
        internal static bool IsViewVisible(ModelItem child, ModelItem root, EditingContext context)
        {
            if (child == root)
            {
                return !IsDefaultDesigner(context, root);
            }

            if (child.Parent == null)
            {
                return false;
            }

            WorkflowViewService viewService = GetViewService(context);
            ModelItem parent = ModelUtilities.ReverseFindFirst(child.Parent, (ModelItem current) =>
            {
                return object.Equals(current, root) ||
                    HasView(current, viewService, false) &&
                    (!IsViewExpanded(current, context) || IsDefaultDesigner(context, current));
            });

            return object.Equals(parent, root) && !IsDefaultDesigner(context, root);
        }

        private static bool IsDefaultDesigner(EditingContext context, ModelItem item)
        {
            WorkflowViewService viewService = GetViewService(context);
            Type viewType = viewService.GetDesignerType(item.ItemType);
            return viewType == typeof(ActivityDesigner);
        }

        private static bool HasView(ModelItem modelItem, WorkflowViewService viewService, bool allowDrillIn)
        {
            ActivityDesignerOptionsAttribute options = WorkflowViewService.GetAttribute<ActivityDesignerOptionsAttribute>(modelItem.ItemType);
            Type viewType = viewService.GetDesignerType(modelItem.ItemType);
            return typeof(WorkflowViewElement).IsAssignableFrom(viewType) && (!allowDrillIn || options == null || options.AllowDrillIn);
        }

        // Get the first parent ModelItem that has a view
        internal static ModelItem GetParentModelItemWithView(ModelItem modelItem, EditingContext context, bool allowDrillIn)
        {
            if (modelItem == null || modelItem.Parent == null)
            {
                return null;
            }

            WorkflowViewService viewService = GetViewService(context);

            return ModelUtilities.ReverseFindFirst(modelItem.Parent, (ModelItem current) =>
                {
                    return HasView(current, viewService, allowDrillIn);
                });
        }

        // Determine whether the view of a ModelItem is expanded without querying the view itself - the view may have not been constructed.
        internal static bool IsViewExpanded(ModelItem modelItem, EditingContext context)
        {
            if (modelItem == null)
            {
                return false;
            }

            bool isDesignerExpanded = true;
            bool isDesignerPinned = false;
            object isExpandedViewState = GetViewStateService(context).RetrieveViewState(modelItem, ExpandViewStateKey);
            object isPinnedViewState = GetViewStateService(context).RetrieveViewState(modelItem, WorkflowViewElement.PinnedViewStateKey);
            if (isExpandedViewState != null)
            {
                isDesignerExpanded = (bool)isExpandedViewState;
            }
            if (isPinnedViewState != null)
            {
                isDesignerPinned = (bool)isPinnedViewState;
            }

            DesignerView designerView = context.Services.GetService<DesignerView>();
             
            return ShouldShowExpanded(IsBreadcrumbRoot(modelItem, context), DoesParentAlwaysExpandChildren(modelItem, context),
                DoesParentAlwaysCollapseChildren(modelItem, context), isDesignerExpanded, designerView.ShouldExpandAll, designerView.ShouldCollapseAll, isDesignerPinned);
        }

        internal static bool IsBreadcrumbRoot(ModelItem modelItem, EditingContext context)
        {
            DesignerView designerView = context.Services.GetService<DesignerView>();
            return modelItem != null && modelItem.View != null && modelItem.View.Equals(designerView.RootDesigner);
        }

        internal static bool ShouldShowExpanded(
            bool isRootDesigner,
            bool parentAlwaysExpandChildren,
            bool parentAlwaysCollapseChildren,
            bool expandState,
            bool expandAll,
            bool collapseAll,
            bool pinState)
        {
            //ShowExpanded based on ExpandAll, CollapseAll, PinState, ExpandState
            bool showExpanded = ShouldShowExpanded(expandState, expandAll, collapseAll, pinState);

            //return value based on the position of the element in the workflow tree.
            return (isRootDesigner || parentAlwaysExpandChildren || (!parentAlwaysCollapseChildren && showExpanded));
        }

        internal static bool ShouldShowExpanded(bool isExpanded, bool shouldExpandAll, bool shouldCollapseAll, bool isPinned)
        {
            if (isPinned)
            {
                return isExpanded;
            }
            else
            {
                return !shouldCollapseAll && (shouldExpandAll || isExpanded);
            }
        }

        static WorkflowViewService GetViewService(EditingContext context)
        {
            return context.Services.GetService<ViewService>() as WorkflowViewService;
        }

        static ViewStateService GetViewStateService(EditingContext context)
        {
            return context.Services.GetService<ViewStateService>();
        }

        //Checks to see if the immediate parent WorkflowViewElement is of type "parentType".
        static bool IsParentOfType(ModelItem modelItem, Type parentType, EditingContext context)
        {
            Type parentDesignerType = GetParentDesignerType(modelItem, context);
            return parentType.IsAssignableFrom(parentDesignerType);
        }

        static Type GetParentDesignerType(ModelItem modelItem, EditingContext context)
        {
            ModelItem parent = GetParentModelItemWithView(modelItem, context, false);
            if (parent != null)
            {
                return GetViewService(context).GetDesignerType(parent.ItemType);
            }
            return null;
        }

        internal static void MeasureView(WorkflowViewElement view, bool measureAsCollapsed)
        {
            bool expandState = view.ExpandState;
            bool pinState = view.PinState;
         
            if (measureAsCollapsed)
            {
                view.ForceCollapse();
            }

            view.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            view.UpdateLayout();

            if (view.ExpandState != expandState)
            {
                view.ExpandState = expandState;
            }
            if (view.PinState != pinState)
            {
                view.PinState = pinState;
            }
        }

        // the job of this method is to construct a DisplayName for ActivityBuilder
        // This name will be shown in breadcrumb bar.
        // if ActivityBuilder.Name = "workflowconsoleApp.Sequence1"
        // we want DisplayName = "Sequence1"
        internal static string GetActivityBuilderDisplayName(ModelItem modelItem)
        {
            Fx.Assert(modelItem != null, "modelItem != null");
            ModelItem nameModelItem = modelItem.Properties["Name"].Value;
            string name = (nameModelItem == null) ? null : (string)nameModelItem.GetCurrentValue();

            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            string displayName = string.Empty;
            int indexOfDot = name.LastIndexOf('.');
            if (indexOfDot > -1)
            {
                // if make sure there at least one character after .
                if (indexOfDot < name.Length - 1)
                {
                    displayName = name.Substring(indexOfDot + 1);
                }
            }
            else
            {
                displayName = name;
            }

            return displayName;
        }

        internal static GeneralTransform GetTransformToRoot(Visual visual)
        {
            Visual rootVisual = GetRootVisual(visual);

            return (rootVisual != null) ? visual.TransformToAncestor(rootVisual) : null;
        }

        private static Visual GetRootVisual(Visual visual)
        {
            Fx.Assert(visual != null, "visual should not be null");

            Visual root = null;

            PresentationSource source = PresentationSource.FromDependencyObject(visual);
            if (source != null)
            {
                root = source.RootVisual;
            }

            // PresentationSource will be null if the element is not in a window
            // Window w = new Window();
            // Button b = new Button();
            // w.Show();
            // w.Content = b;
            // PresentationSource.FromDependencyObject(b) will return an instance
            // w.Content = null;
            // PresentationSource.FromDependencyObject(b) will return null
            // The reason of tree walk is to make some effort to support 
            // the scenario where user wants to capture a visual that is 
            // not in a window. 
            if (root == null)
            {
                for (DependencyObject current = visual;
                    current != null; current = VisualTreeHelper.GetParent(current))
                {
                    // Maybe Visual is not enought in some case, but I don't get a sample 
                    // till now. If it happens, add LogicalTreeHelper.GetParent() to the 
                    // parent getting chain.
                    Visual currentVisual = current as Visual;
                    if (currentVisual != null)
                    {
                        root = currentVisual;
                    }
                }
            }

            return root;
        }
    }
}
