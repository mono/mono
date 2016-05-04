//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Windows;
    using System.Activities.Presentation.View;
    using System.Runtime;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly,
        Justification = "Following the naming of IMultiValueConverter")]
    internal sealed class ShowExpandedMultiValueConverter : IMultiValueConverter
    {

        //Calculates whether ShowExpanded for a given WorklfowViewElement should be true or false.
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ModelItem modelItem = (ModelItem)values[0];
            bool isRootDesigner = (bool)values[1];
            bool shouldExpandAll = (bool)values[2];
            bool shouldCollapseAll = (bool)values[3];
            bool expandState = (bool)values[4];
            bool pinState = (bool)values[5];
            WorkflowViewElement viewElement = (WorkflowViewElement)values[6];

            //Pinstate should be false in following cases (Designer should be unpinned in following cases):
            //1. ExpandAll is not enabled.
            //2. ExpandAll is enabled and ExpandState is true.
            //Similarly for Collapse All.
            if ((!shouldExpandAll || expandState)
                && (!shouldCollapseAll || !expandState))
            {
                viewElement.PinState = false;
            }
            if (viewElement.IsAncestorOfRootDesigner)
            {
                return true;
            }
            return ViewUtilities.ShouldShowExpanded(isRootDesigner, viewElement.DoesParentAlwaysExpandChild(),
                viewElement.DoesParentAlwaysCollapseChildren(), expandState, shouldExpandAll, shouldCollapseAll, viewElement.PinState);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

    }
}
