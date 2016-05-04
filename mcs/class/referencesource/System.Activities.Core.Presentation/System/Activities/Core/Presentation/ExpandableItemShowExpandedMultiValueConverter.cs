//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.View;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used in XAML")]
    internal sealed class ExpandableItemShowExpandedMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] != DependencyProperty.UnsetValue && values[1] != DependencyProperty.UnsetValue)
            {
                bool isExpanded = (bool)values[0];
                bool isPinned = (bool)values[1];
                bool shouldExpandAll = (bool)values[2];
                bool shouldCollapseAll = (bool)values[3];
                ExpandableItemWrapper wrapper = (ExpandableItemWrapper)values[4];

                if ((!shouldExpandAll || isExpanded)
                    && (!shouldCollapseAll || !isExpanded))
                {
                    wrapper.SetPinState(false);
                }

                return ViewUtilities.ShouldShowExpanded(isExpanded, shouldExpandAll, shouldCollapseAll, wrapper.IsPinned);
            }
            else
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
