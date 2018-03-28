//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Globalization;
    using System.Runtime;
    using System.Windows.Data;

    sealed class FlowSwitchLinkMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isDefaultCase = (bool)values[1];
            if (isDefaultCase)
            {
                // Fx.Assert(values.Length == 3, "The multi-binding must have been constructed by FlowSwitchLink.");
                // For default case, we should have got three bindings. Two binding is possible when the linkModelItem is replaced but the view is not completely re-constructed yet.
                return values.Length == 3 ? values[2] : null;
            }
            else
            {
                return GenericFlowSwitchHelper.GetString(values[0], (Type) parameter);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
