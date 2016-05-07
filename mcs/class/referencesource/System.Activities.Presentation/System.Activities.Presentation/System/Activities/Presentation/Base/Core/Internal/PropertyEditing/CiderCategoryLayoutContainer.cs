//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using Blend = System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;
    using System.Activities.Presentation.Internal.PropertyEditing.Selection;

    // <summary>
    // Container for CategoryEditors - fancy wrapper for ItemsControl that eliminates the need
    // for extra bindings.
    //
    // This class is referenced from XAML
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class CiderCategoryLayoutContainer : Blend.CategoryLayoutContainer 
    {

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) 
        {
            base.PrepareContainerForItemOverride(element, item);

            if (item != null) 
            {
                string editorTypeName = item.GetType().Name;

                // Make each CategoryEditor its own selection stop
                PropertySelection.SetSelectionStop(element, new CategoryEditorSelectionStop(editorTypeName));
            }
        }
    }
}
