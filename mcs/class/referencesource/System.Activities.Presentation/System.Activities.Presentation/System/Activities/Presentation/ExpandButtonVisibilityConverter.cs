//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Windows;
    using System.Activities.Presentation.View;
    using System.Runtime;

    internal class ExpandButtonVisibilityConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRootDesigner = true;
            if (values[0] is bool)
            {
                isRootDesigner = (bool)values[0];
            }
            ModelItem modelItem = values[1] as ModelItem;
            WorkflowViewElement viewElement = values[2] as WorkflowViewElement;
            Fx.Assert(viewElement != null, "TemplatedParent should be of type WorkflowViewElement");
            return GetExpandCollapseButtonVisibility(viewElement);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public static Visibility GetExpandCollapseButtonVisibility(WorkflowViewElement viewElement)
        {
            ActivityDesigner designer = viewElement as ActivityDesigner;
            bool hasDelegates = (designer != null) && designer.HasActivityDelegates;

            Visibility visibility = Visibility.Visible;
            if (viewElement == null || viewElement.IsRootDesigner || viewElement.DoesParentAlwaysExpandChild() ||
                viewElement.DoesParentAlwaysCollapseChildren() || (viewElement.Content == null && !hasDelegates) ||
                !viewElement.Collapsible || !(viewElement is ActivityDesigner))
            {
                visibility = Visibility.Collapsed;
            }
            return visibility;
        }

    }
}
