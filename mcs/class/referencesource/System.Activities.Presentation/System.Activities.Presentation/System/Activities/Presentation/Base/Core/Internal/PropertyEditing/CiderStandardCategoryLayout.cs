//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;

    using System.Activities.Presentation.PropertyEditing;

    using Blend = System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;
    using System.Activities.Presentation.Internal.PropertyEditing.State;

    // <summary>
    // Container for PropertyContainers - fancy wrapper for ItemsControl that eliminates the need
    // for intermediate ContentControls.
    //
    // This class is referenced from XAML
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class CiderStandardCategoryLayout : Blend.StandardCategoryLayout 
    {

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) 
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element != null) 
            {
                PropertyEntry property = ((PropertyContainer)element).PropertyEntry;

                if (property != null) 
                {
                    // Make each PropertyContainer its own selection stop
                    PropertySelection.SetSelectionStop(element, new PropertySelectionStop(property));
                }

                PropertyContainer container = element as PropertyContainer;
                if (container != null)
                {
                    container.Loaded += new RoutedEventHandler(OnPropertyContainerLoaded);
                }
            }
        }

        private void OnPropertyContainerLoaded(object sender, RoutedEventArgs e) 
        {
            PropertyContainer container = sender as PropertyContainer;
            if (container != null) 
            {

                // When each PropertyContainer gets loaded, restore its ActiveEditMode
                // based on stored state
                //
                container.ActiveEditMode = PropertyActiveEditModeStateContainer.Instance.GetActiveEditMode(container.PropertyEntry);

                // HACK: Here, we would want to hook into the ActiveEditModeChanged
                // event to store (and restore) the correct expaded state to each
                // value editor across domain reloads.  However, we already shipped
                // System.Activities.Presentation.dll which contains the PropertyContainer
                // class and it's dangerous to add new events to this assembly at this
                // point.  Once we refactor MWD, we need to add this new event and
                // simplify this code.  In the meanwhile, we can cheat and accomplish
                // the same effect by listening to SizeChanged event, which _will_
                // change when the expanded value editor is pinned down.  It will also
                // change at other times (user-triggered resize events, etc.), but
                // it shouldn't cause any noticeable perf degradation.
                //
                container.SizeChanged += new SizeChangedEventHandler(OnPropertyContainerSizeChanged);
                container.Unloaded += new RoutedEventHandler(OnPropertyContainerUnloaded);
            }
        }

        // When the size of each PropertyContainer changes, assume that ActiveEditMode
        // may have been changed and record its current state
        //
        private void OnPropertyContainerSizeChanged(object sender, SizeChangedEventArgs e) 
        {
            PropertyContainer container = sender as PropertyContainer;
            if (container != null && container.IsLoaded)
            {
                PropertyActiveEditModeStateContainer.Instance.StoreActiveEditMode(container.PropertyEntry, container.ActiveEditMode);
            }
        }

        // When each PropertyContainer gets unloaded, stop listening to all events so
        // that it can be garbage collected
        //
        private void OnPropertyContainerUnloaded(object sender, RoutedEventArgs e) 
        {
            PropertyContainer container = sender as PropertyContainer;

            if (container != null) 
            {
                container.SizeChanged -= new SizeChangedEventHandler(OnPropertyContainerSizeChanged);
                container.Loaded -= new RoutedEventHandler(OnPropertyContainerLoaded);
                container.Unloaded -= new RoutedEventHandler(OnPropertyContainerUnloaded);
            }
        }
    }
}
