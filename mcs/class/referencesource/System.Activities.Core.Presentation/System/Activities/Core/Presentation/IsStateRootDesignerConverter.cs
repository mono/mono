//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------
namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Globalization;
    using System.Windows.Data;

    // This converter is used to resolve Bug 198561 and 216429.
    // originally, using "IsRootDesigner" and the binding path directly would cause Bug 201214,
    // as a workaround, we resolved to use "ShowExpanded" as the binding path, but this cause Bug 216429.
    // The resolution would require us to go back to "IsRootDesigner" as the binding path. But
    // we would need to use a converter in between, and thus we made this converter
    internal sealed class IsStateRootDesignerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            WorkflowViewElement workflowViewElement = value as WorkflowViewElement;

            if (null != workflowViewElement)
            {
                return workflowViewElement.IsRootDesigner;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
