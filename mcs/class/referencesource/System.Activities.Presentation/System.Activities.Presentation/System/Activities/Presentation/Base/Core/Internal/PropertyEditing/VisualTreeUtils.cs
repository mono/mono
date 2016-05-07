//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using System.Collections.Generic;

    // <summary>
    // Collection of simple utilities that deal with the VisualTree
    // </summary>
    internal static class VisualTreeUtils 
    {

        // The depth of the visual tree we explore in looking for templated children
        // (see GetTemplateChild<>())
        private const int MaxSearchDepth = 5;

        // The maxium wpf visual tree depth
        // this value should be kept in [....] with WPF's limit
        private const int MaxAllowedTreeDepth = 250;

        // <summary>
        // Examines the visual children of the given element and returns the one
        // with the specified name and type, if one exists
        // </summary>
        // <typeparam name="T">Type of child to look for</typeparam>
        // <param name="container">Container to look in</param>
        // <param name="name">Name to look for</param>
        // <returns>The specified named child if found, null otherwise</returns>
        public static T GetNamedChild<T>(DependencyObject container, string name)
            where T : FrameworkElement 
        {
            return GetNamedChild<T>(container, name, 0);
        }

        // <summary>
        // Examines the visual children of the given element and returns the one
        // with the specified name and type, if one exists
        // </summary>
        // <typeparam name="T">Type of child to look for</typeparam>
        // <param name="container">Container to look in</param>
        // <param name="name">Name to look for</param>
        // <param name="searchDepth">Visual depth to search in.  Default is 0.</param>
        // <returns>The specified named child if found, null otherwise</returns>
        public static T GetNamedChild<T>(DependencyObject container, string name, int searchDepth)
            where T : FrameworkElement 
        {
            if (container == null || string.IsNullOrEmpty(name) || searchDepth < 0)
            {
                return null;
            }

            if (container is T && string.Equals( name, ((T)container).Name))
            {
                return (T)container;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(container);
            if (childCount == 0)
            {
                return null;
            }

            // Look for the first child that matches
            for (int index = 0; index < childCount; index++) 
            {
                FrameworkElement child = VisualTreeHelper.GetChild(container, index) as FrameworkElement;
                if (child == null)
                {
                    continue;
                }

                // Search recursively until we reach the requested search depth
                T namedChild =
                    (child.FindName(name) as T) ??
                    (searchDepth > 0 ? GetNamedChild<T>(child, name, searchDepth - 1) : null);

                if (namedChild != null)
                {
                    return namedChild;
                }
            }

            return null;
        }

        // <summary>
        // Helper method that goes down the first-child visual tree and looks for the visual element of the
        // specified type.  Useful for when you have one control that is templated to look like another
        // and you need to get to the template instance itself.
        // </summary>
        // <typeparam name="T">Type of the visual template to look for</typeparam>
        // <param name="element">Element to start from</param>
        // <returns>The first matching instance in the visual tree of first children, null otherwise</returns>
        public static T GetTemplateChild<T>(DependencyObject element) where T : DependencyObject 
        {
            int availableSearchDepth = MaxSearchDepth;
            while (availableSearchDepth > 0 && element != null) 
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(element);
                if (childrenCount < 1)
                {
                    return null;
                }

                // Just worry about the first child, since we are looking for a template control,
                // not a complicated visual tree
                //
                element = VisualTreeHelper.GetChild(element, 0);
                T childAsT = element as T;
                if (childAsT != null)
                {
                    return childAsT;
                }

                availableSearchDepth--;
            }

            return null;
        }

        // <summary>
        // Goes up the visual tree, looking for an ancestor of the specified Type
        // </summary>
        // <typeparam name="T">Type to look for</typeparam>
        // <param name="child">Starting point</param>
        // <returns>Visual ancestor, if any, of the specified starting point and Type</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static T FindVisualAncestor<T>(DependencyObject child) where T : DependencyObject 
        {
            if (child == null)
            {
                return null;
            }

            do 
            {
                child = VisualTreeHelper.GetParent(child);
            }
            while (child != null && !typeof(T).IsAssignableFrom(child.GetType()));

            return child as T;
        }

        // <summary>
        // Recursively looks through all the children and looks for and returns the first
        // element with IsFocusable set to true.
        // </summary>
        // <param name="reference">Starting point for the search</param>
        // <typeparam name="T">Type of child to look for</typeparam>
        // <returns>The first focusable child of the specified Type, null otherwise.</returns>
        public static T FindFocusableElement<T>(T reference) where T : UIElement 
        {
            if (reference == null || (reference.Focusable && reference.Visibility == Visibility.Visible))
            {
                return reference;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(reference);
            for (int i = 0; i < childCount; i++) 
            {
                T child = VisualTreeHelper.GetChild(reference, i) as T;
                if (child == null)
                {
                    continue;
                }

                if (child.Visibility != Visibility.Visible)
                {
                    continue;
                }

                if (child.Focusable)
                {
                    return child;
                }

                child = FindFocusableElement<T>(child);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        // <summary>
        // Walks up the parent tree and returns the first
        // element with IsFocusable set to true.
        // </summary>
        // <param name="reference">Starting point for the search</param>
        // <typeparam name="T">Type of parent to look for</typeparam>
        // <returns>The first focusable parent of the specified Type, null otherwise.</returns>
        public static T FindFocusableParent<T>(UIElement reference) where T : UIElement
        {
            if (null != reference)
            {
                UIElement parent = VisualTreeHelper.GetParent(reference) as UIElement;
                while (null != parent)
                {
                    if (parent.Visibility == Visibility.Visible && parent is T && parent.Focusable)
                    {
                        return parent as T;
                    }

                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }
            return null;
        }

        // <summary>
        // Helper method identical to VisualTreeHelper.GetParent() but that also returns the index of the given child
        // with respect to any of its visual siblings
        // </summary>
        // <param name="child">Child to examine</param>
        // <param name="childrenCount">Total number of children that the specified child's parent has</param>
        // <param name="childIndex">Index of the specified child in the parent's visual children array</param>
        // <returns>Visual parent of the specified child, if any</returns>
        public static DependencyObject GetIndexedVisualParent(DependencyObject child, out int childrenCount, out int childIndex) 
        {
            childrenCount = 0;
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent != null) 
            {
                childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (childIndex = 0; childIndex < childrenCount; childIndex++) 
                {
                    if (child.Equals(VisualTreeHelper.GetChild(parent, childIndex)))
                    {
                        return parent;
                    }
                }
            }

            childIndex = -1;
            return null;
        }

        // <summary>
        // Helper method that goes up the visual tree checking the visibility flag of the specified element
        // and all its parents.  If an invisible parent is found, the method return false.  Otherwise it returns
        // true.
        // </summary>
        // <param name="element">Element to check</param>
        // <returns>True if the specified element and all of its visual parents are visible,
        // false otherwise.</returns>
        public static bool IsVisible(UIElement element) 
        {
            UIElement parent = element;
            while (parent != null) 
            {
                if (parent.Visibility != Visibility.Visible)
                {
                    return false;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return true;
        }

        /// <summary>
        /// Helper method that trasverse the visual tree checking for immediate parent of specified type that has children
        /// that is too deep for WPF visual tree
        /// </summary>
        /// <typeparam name="T">The type of parent to look for</typeparam>
        /// <param name="root">The root element to start checking</param>
        /// <returns></returns>
        public static ICollection<T> PrunVisualTree<T>(Visual root) where T : DependencyObject
        {
            HashSet<T> deepElements = new HashSet<T>();
            Stack<VisualState> stack = new Stack<VisualState>();
            VisualState currentObject = new VisualState(root, GetTreeDepth(root));
            stack.Push(currentObject);
            int totalChildCount = 0;
            Visual child;
            T violatingParent;
            //doing a depth first walk of the visual tree.
            while (stack.Count > 0)
            {
                currentObject = stack.Pop();

                // currentObject.Depth + 1 is the children's depth
                // if it's too deep, we would try to find the parent and stop traversing 
                // this node further
                if (currentObject.Depth + 1 >= MaxAllowedTreeDepth)
                {
                    violatingParent = currentObject.Visual as T;
                    if (violatingParent != null)
                    {
                        deepElements.Add(violatingParent);
                    }
                    else
                    {
                        violatingParent = FindVisualAncestor<T>(currentObject.Visual);
                        if (violatingParent != null)
                        {
                            deepElements.Add(violatingParent);
                        }
                    }
                }
                else // continue the depth first traversal
                {
                    totalChildCount = VisualTreeHelper.GetChildrenCount(currentObject.Visual);
                    for (int i = 0; i < totalChildCount; i++)
                    {
                        child = VisualTreeHelper.GetChild(currentObject.Visual, i) as Visual;
                        if (child != null)
                        {
                            stack.Push(new VisualState(child, currentObject.Depth + 1));
                        }
                    }
                }
            }
           return deepElements;
        }

        //find the depth of an DependencyObject
        public static uint GetTreeDepth(DependencyObject element)
        {
            uint depth = 0;
            while (element != null)
            {
                depth++;
                if (element is Visual || element is Visual3D)
                {
                    element = VisualTreeHelper.GetParent(element);
                }
                else
                {
                    element = LogicalTreeHelper.GetParent(element);
                }
            }
            return depth;
        }

        private class VisualState
        {
            internal Visual Visual { get; set; }
            internal uint Depth { get; set; }

            internal VisualState(Visual visual, uint depth)
            {
                this.Visual = visual;
                this.Depth = depth;
            }
        }
    }
}
