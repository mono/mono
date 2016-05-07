//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    using System.Runtime;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;
    using System.Activities.Presentation;

    // <summary>
    // This is a container for attached properties used by PropertyInspector to track and manage
    // property selection.  It is public because WPF requires that attached properties used in XAML
    // be declared by public classes.
    // </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    static class PropertySelection
    {

        private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsSelected",
            typeof(bool),
            typeof(PropertySelection),
            new PropertyMetadata(false));

        // <summary>
        // Attached, ReadOnly DP that we use to mark objects as selected.  If they care, they can then render
        // themselves differently.
        // </summary>
        internal static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        // <summary>
        // Attached DP that we use in XAML to mark elements that can be selected.
        // </summary>
        internal static readonly DependencyProperty SelectionStopProperty = DependencyProperty.RegisterAttached(
            "SelectionStop",
            typeof(ISelectionStop),
            typeof(PropertySelection),
            new PropertyMetadata(null));

        // <summary>
        // Attached DP used in conjunction with SelectionStop DP.  It specifies the FrameworkElement to hook into
        // in order to handle double-click events to control the expanded / collapsed state of its parent SelectionStop.
        // </summary>
        internal static readonly DependencyProperty IsSelectionStopDoubleClickTargetProperty = DependencyProperty.RegisterAttached(
            "IsSelectionStopDoubleClickTarget",
            typeof(bool),
            typeof(PropertySelection),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsSelectionStopDoubleClickTargetChanged)));

        // <summary>
        // Attached DP that we use in XAML to mark elements as selection scopes - meaning selection
        // won't spill beyond the scope of the marked element.
        // </summary>
        internal static readonly DependencyProperty IsSelectionScopeProperty = DependencyProperty.RegisterAttached(
            "IsSelectionScope",
            typeof(bool),
            typeof(PropertySelection),
            new PropertyMetadata(false));

        // <summary>
        // Attached property we use to route non-navigational key strokes from one FrameworkElement to
        // another.  When this property is set on a FrameworkElement, we hook into its KeyDown event
        // and send any unhandled, non-navigational key strokes to the FrameworkElement specified
        // by this property.  The target FrameworkElement must be focusable or have a focusable child.
        // When the first eligible key stroke is detected, the focus will be shifted to the focusable
        // element and the key stroke will be sent to it.
        // </summary>
        internal static readonly DependencyProperty KeyDownTargetProperty = DependencyProperty.RegisterAttached(
            "KeyDownTarget",
            typeof(FrameworkElement),
            typeof(PropertySelection),
            new PropertyMetadata(null, new PropertyChangedCallback(OnKeyDownTargetChanged)));

        // Constant that determines how deep in the visual tree we search for SelectionStops that
        // are children or neighbors of a given element (usually one that the user clicked on) before
        // giving up.  This constant is UI-dependent.
        private const int MaxSearchDepth = 11;

        // <summary>
        // Gets PropertySelection.IsSelected property from the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to examine</param>
        // <returns>Value of the IsSelected property</returns>
        internal static bool GetIsSelected(DependencyObject obj)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            return (bool)obj.GetValue(IsSelectedProperty);
        }

        // Private (internal) setter that we use to mark objects as selected from within CategoryList class
        //
        internal static void SetIsSelected(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            obj.SetValue(IsSelectedPropertyKey, value);
        }


        // SelectionStop Attached DP

        // <summary>
        // Gets PropertySelection.SelectionStop property from the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to examine</param>
        // <returns>Value of the SelectionStop property.</returns>
        internal static ISelectionStop GetSelectionStop(DependencyObject obj)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            return (ISelectionStop)obj.GetValue(SelectionStopProperty);
        }

        // <summary>
        // Sets PropertySelection.SelectionStop property on the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to modify</param>
        // <param name="value">New value of SelectionStop</param>
        internal static void SetSelectionStop(DependencyObject obj, ISelectionStop value)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            obj.SetValue(SelectionStopProperty, value);
        }

        // <summary>
        // Clears PropertySelection.SelectionStop property from the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to clear</param>
        internal static void ClearSelectionStop(DependencyObject obj)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            obj.ClearValue(SelectionStopProperty);
        }


        // IsSelectionStopDoubleClickTarget Attached DP

        // <summary>
        // Gets PropertySelection.IsSelectionStopDoubleClickTarget property from the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to examine</param>
        // <returns>Value of the IsSelectionStopDoubleClickTarget property.</returns>
        internal static bool GetIsSelectionStopDoubleClickTarget(DependencyObject obj)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            return (bool)obj.GetValue(IsSelectionStopDoubleClickTargetProperty);
        }

        // <summary>
        // Sets PropertySelection.IsSelectionStopDoubleClickTarget property on the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to modify</param>
        // <param name="value">New value of IsSelectionStopDoubleClickTarget</param>
        internal static void SetIsSelectionStopDoubleClickTarget(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            obj.SetValue(IsSelectionStopDoubleClickTargetProperty, value);
        }

        // <summary>
        // Clears PropertySelection.IsSelectionStopDoubleClickTarget property from the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to modify</param>
        internal static void ClearIsSelectionStopDoubleClickTarget(DependencyObject obj)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            obj.ClearValue(IsSelectionStopDoubleClickTargetProperty);
        }

        // Called when some object gets specified as the SelectionStop double-click target:
        //
        //      * Hook into the MouseDown event so that we can detect double-clicks and automatically
        //        expand or collapse the corresponding SelectionStop, if possible
        //
        private static void OnIsSelectionStopDoubleClickTargetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement target = sender as FrameworkElement;
            if (target == null)
            {
                return;
            }

            if (bool.Equals(e.OldValue, false) && bool.Equals(e.NewValue, true))
            {
                AddDoubleClickHandler(target);
            }
            else if (bool.Equals(e.OldValue, true) && bool.Equals(e.NewValue, false))
            {
                RemoveDoubleClickHandler(target);
            }
        }

        // Called when some SelectionStop double-click target gets unloaded:
        //
        //      * Unhook from events so that we don't prevent garbage collection
        //
        private static void OnSelectionStopDoubleClickTargetUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement target = sender as FrameworkElement;
            Fx.Assert(target != null, "sender parameter should not be null");

            if (target == null)
            {
                return;
            }

            RemoveDoubleClickHandler(target);
        }

        // Called when the UI object representing a SelectionStop gets clicked:
        //
        //      * If this is a double-click and the SelectionStop can be expanded / collapsed,
        //        expand / collapse the SelectionStop
        //
        private static void OnSelectionStopDoubleClickTargetMouseDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject target = e.OriginalSource as DependencyObject;
            if (target == null)
            {
                return;
            }

            if (e.ClickCount > 1)
            {

                FrameworkElement parentSelectionStopVisual = PropertySelection.FindParentSelectionStop<FrameworkElement>(target);
                if (parentSelectionStopVisual != null)
                {

                    ISelectionStop parentSelectionStop = PropertySelection.GetSelectionStop(parentSelectionStopVisual);
                    if (parentSelectionStop != null && parentSelectionStop.IsExpandable)
                    {
                        parentSelectionStop.IsExpanded = !parentSelectionStop.IsExpanded;
                    }
                }
            }
        }

        private static void AddDoubleClickHandler(FrameworkElement target)
        {
            target.AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(OnSelectionStopDoubleClickTargetMouseDown), false);
            target.Unloaded += new RoutedEventHandler(OnSelectionStopDoubleClickTargetUnloaded);
        }

        private static void RemoveDoubleClickHandler(FrameworkElement target)
        {
            target.Unloaded -= new RoutedEventHandler(OnSelectionStopDoubleClickTargetUnloaded);
            target.RemoveHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(OnSelectionStopDoubleClickTargetMouseDown));
        }


        // IsSelectionScope Attached DP

        // <summary>
        // Gets PropertySelection.IsSelectionScope property from the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to examine</param>
        // <returns>Value of the IsSelectionScope property.</returns>
        internal static bool GetIsSelectionScope(DependencyObject obj)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            return (bool)obj.GetValue(IsSelectionScopeProperty);
        }

        // <summary>
        // Sets PropertySelection.IsSelectionScope property on the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to modify</param>
        // <param name="value">New value of IsSelectionScope</param>
        internal static void SetIsSelectionScope(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            obj.SetValue(IsSelectionScopeProperty, value);
        }

        // KeyDownTarget Attached DP

        // <summary>
        // Gets PropertySelection.KeyDownTarget property from the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to examine</param>
        // <returns>Value of the KeyDownTarget property.</returns>
        internal static FrameworkElement GetKeyDownTarget(DependencyObject obj)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            return (FrameworkElement)obj.GetValue(KeyDownTargetProperty);
        }

        // <summary>
        // Sets PropertySelection.KeyDownTarget property on the specified DependencyObject
        // </summary>
        // <param name="obj">DependencyObject to modify</param>
        // <param name="value">New value of KeyDownTarget</param>
        internal static void SetKeyDownTarget(DependencyObject obj, FrameworkElement value)
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            obj.SetValue(KeyDownTargetProperty, value);
        }

        // Called when some FrameworkElement gets specified as the target for KeyDown RoutedEvents -
        // hook into / unhook from the KeyDown event of the source
        private static void OnKeyDownTargetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement target = sender as FrameworkElement;
            if (target == null)
            {
                return;
            }

            if (e.OldValue != null && e.NewValue == null)
            {
                RemoveKeyStrokeHandlers(target);
            }
            else if (e.NewValue != null && e.OldValue == null)
            {
                AddKeyStrokeHandlers(target);
            }
        }

        // Called when a KeyDownTarget gets unloaded -
        // unhook from events so that we don't prevent garbage collection
        private static void OnKeyDownTargetUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement target = sender as FrameworkElement;
            Fx.Assert(target != null, "sender parameter should not be null");

            if (target == null)
            {
                return;
            }

            RemoveKeyStrokeHandlers(target);
        }

        // Called when a KeyDownTarget is specified and a KeyDown event is detected on the source
        private static void OnKeyDownTargetKeyDown(object sender, KeyEventArgs e)
        {

            // Ignore handled events
            if (e.Handled)
            {
                return;
            }

            // Ignore navigation keys
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down ||
                e.Key == Key.Tab || e.Key == Key.Escape || e.Key == Key.Return || e.Key == Key.Enter ||
                e.Key == Key.PageUp || e.Key == Key.PageDown || e.Key == Key.Home || e.Key == Key.End || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                return;
            }



            DependencyObject keySender = sender as DependencyObject;
            Fx.Assert(keySender != null, "keySender should not be null");
            if (keySender == null)
            {
                return;
            }

            FrameworkElement keyTarget = GetKeyDownTarget(keySender);
            Fx.Assert(keyTarget != null, "keyTarget should not be null");
            if (keyTarget == null)
            {
                return;
            }

            // Find a focusable element on the target, set focus to it, and send the keys over
            FrameworkElement focusable = VisualTreeUtils.FindFocusableElement<FrameworkElement>(keyTarget);
            if (focusable != null && focusable == Keyboard.Focus(focusable))
            {
                focusable.RaiseEvent(e);
            }
        }

        private static void AddKeyStrokeHandlers(FrameworkElement target)
        {
            target.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDownTargetKeyDown), false);
            target.Unloaded += new RoutedEventHandler(OnKeyDownTargetUnloaded);
        }

        private static void RemoveKeyStrokeHandlers(FrameworkElement target)
        {
            target.Unloaded -= new RoutedEventHandler(OnKeyDownTargetUnloaded);
            target.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDownTargetKeyDown));
        }


        // <summary>
        // Returns the closest parent (or the element itself) marked as a SelectionStop.
        // </summary>
        // <typeparam name="T">Type of element to look for</typeparam>
        // <param name="element">Element to examine</param>
        // <returns>The closest parent (or the element itself) marked as a SelectionStop;
        // null if not found.</returns>
        internal static T FindParentSelectionStop<T>(DependencyObject element) where T : DependencyObject
        {
            if (element == null)
            {
                return null;
            }

            do
            {
                // IsEligibleSelectionStop already checks for visibility, so we don't need to
                // to do a specific check somewhere else in this loop
                if (IsEligibleSelectionStop<T>(element))
                {
                    return (T)element;
                }

                element = VisualTreeHelper.GetParent(element);
            } while (element != null);

            return null;
        }

        // <summary>
        // Returns the closest neighbor in the given direction marked as a SelectionStop.
        // </summary>
        // <typeparam name="T">Type of element to look for</typeparam>
        // <param name="element">Element to examine</param>
        // <param name="direction">Direction to search in</param>
        // <returns>The closest neighboring element in the given direction marked as a IsSelectionStop,
        // if found, null otherwise.</returns>
        internal static T FindNeighborSelectionStop<T>(DependencyObject element, SearchDirection direction) where T : DependencyObject
        {

            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }

            T neighbor;
            int maxSearchDepth = MaxSearchDepth;

            // If we are looking for the NEXT element and we can dig deeper, start by digging deeper
            // before trying to look for any siblings.
            //
            if (direction == SearchDirection.Next && IsExpanded(element))
            {
                neighbor = FindChildSelectionStop<T>(element, 0, VisualTreeHelper.GetChildrenCount(element) - 1, direction, maxSearchDepth, MatchDirection.Down);

                if (neighbor != null)
                {
                    return neighbor;
                }
            }

            int childIndex, childrenCount, childDepth;
            bool isParentSelectionStop, isParentSelectionScope = false;
            DependencyObject parent = element;

            while (true)
            {
                while (true)
                {
                    // If we reached the selection scope, don't try to go beyond it
                    if (isParentSelectionScope)
                    {
                        return null;
                    }

                    parent = GetEligibleParent(parent, out childIndex, out childrenCount, out childDepth, out isParentSelectionStop, out isParentSelectionScope);
                    maxSearchDepth += childDepth;

                    if (parent == null)
                    {
                        return null;
                    }

                    if (direction == SearchDirection.Next && (childIndex + 1) >= childrenCount)
                    {
                        continue;
                    }

                    if (direction == SearchDirection.Previous && isParentSelectionStop == false && (childIndex < 1))
                    {
                        continue;
                    }

                    break;
                }

                // If we get here, that means we found a SelectionStop on which we need to look for children that are
                // SelectionStops themselves.  The first such child found should be returned.  Otherwise, if no such child
                // is found, we potentially look at the node itself and return it OR we repeat the process and keep looking
                // for a better parent.

                int leftIndex, rightIndex;
                MatchDirection matchDirection;

                if (direction == SearchDirection.Previous)
                {
                    leftIndex = 0;
                    rightIndex = childIndex - 1;
                    matchDirection = MatchDirection.Up;
                }
                else
                {
                    leftIndex = childIndex + 1;
                    rightIndex = childrenCount - 1;
                    matchDirection = MatchDirection.Down;
                }

                neighbor = FindChildSelectionStop<T>(parent, leftIndex, rightIndex, direction, maxSearchDepth, matchDirection);
                if (neighbor != null)
                {
                    return neighbor;
                }

                if (direction == SearchDirection.Previous &&
                    IsEligibleSelectionStop<T>(parent))
                {
                    return (T)parent;
                }
            }
        }

        // Helper method used from GetNeighborSelectionStop()
        // Returns a parent DependencyObject of the specified element that is
        // 
        //  * Visible AND
        //  * ( Marked with a SelectionStop OR
        //  *   Marked with IsSelectionScope = true OR
        //  *   Has more than one child )
        //
        private static DependencyObject GetEligibleParent(DependencyObject element, out int childIndex, out int childrenCount, out int childDepth, out bool isSelectionStop, out bool isSelectionScope)
        {
            childDepth = 0;
            isSelectionStop = false;
            isSelectionScope = false;
            bool isVisible;

            do
            {
                element = VisualTreeUtils.GetIndexedVisualParent(element, out childrenCount, out childIndex);
                isSelectionStop = element == null ? false : (GetSelectionStop(element) != null);
                isSelectionScope = element == null ? false : GetIsSelectionScope(element);
                isVisible = VisualTreeUtils.IsVisible(element as UIElement);

                childDepth++;
            }
            while (
                element != null &&
                (isVisible == false ||
                (isSelectionStop == false &&
                isSelectionScope == false &&
                childrenCount < 2)));

            return element;
        }

        // Helper method that performs a recursive, depth-first search of children starting at the specified parent,
        // looking for any children that conform to the specified Type and are marked with a SelectionStop
        //
        private static T FindChildSelectionStop<T>(DependencyObject parent, int leftIndex, int rightIndex, SearchDirection iterationDirection, int maxDepth, MatchDirection matchDirection)
            where T : DependencyObject
        {

            if (parent == null || maxDepth <= 0)
            {
                return null;
            }

            int step = iterationDirection == SearchDirection.Next ? 1 : -1;
            int index = iterationDirection == SearchDirection.Next ? leftIndex : rightIndex;

            for (; index >= leftIndex && index <= rightIndex; index = index + step)
            {

                DependencyObject child = VisualTreeHelper.GetChild(parent, index);

                // If MatchDirection is set to Down, do an eligibility match BEFORE we dive down into
                // more children.
                //
                if (matchDirection == MatchDirection.Down && IsEligibleSelectionStop<T>(child))
                {
                    return (T)child;
                }

                // If this child is not an eligible SelectionStop because it is not visible,
                // there is no point digging down to get to more children.
                //
                if (!VisualTreeUtils.IsVisible(child as UIElement))
                {
                    continue;
                }

                int grandChildrenCount = VisualTreeHelper.GetChildrenCount(child);
                if (grandChildrenCount > 0 && IsExpanded(child))
                {
                    T element = FindChildSelectionStop<T>(child, 0, grandChildrenCount - 1, iterationDirection, maxDepth - 1, matchDirection);

                    if (element != null)
                    {
                        return element;
                    }
                }

                // If MatchDirection is set to Up, do an eligibility match AFTER we tried diving into
                // more children and failed to find something we could return.
                //
                if (matchDirection == MatchDirection.Up && IsEligibleSelectionStop<T>(child))
                {
                    return (T)child;
                }
            }

            return null;
        }

        // Helper method that returns false if the given element is a collapsed SelectionStop,
        // true otherwise.
        //
        private static bool IsExpanded(DependencyObject element)
        {
            ISelectionStop selectionStop = PropertySelection.GetSelectionStop(element);
            return selectionStop == null || selectionStop.IsExpanded;
        }

        // Helper method that return true if the given element is marked with a SelectionStop,
        // if it derives from the specified Type, and if it is Visible (assuming it derives from UIElement)
        //
        private static bool IsEligibleSelectionStop<T>(DependencyObject element) where T : DependencyObject
        {
            return (GetSelectionStop(element) != null) && typeof(T).IsAssignableFrom(element.GetType()) && VisualTreeUtils.IsVisible(element as UIElement);
        }

        // <summary>
        // Private enum we use to specify whether FindSelectionStopChild() should return any matches
        // as it drills down into the visual tree (Down) or whether it should wait on looking at
        // matches until it's bubbling back up again (Up).
        // </summary>
        private enum MatchDirection
        {
            Down,
            Up
        }

        // IsSelected ReadOnly, Attached DP
    }
}
