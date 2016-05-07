//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows.Data;
    using System.Runtime;
    using System.Activities.Presentation;

    // <summary>
    // A converter that takes a value of a property and a boolean indicating whether it
    // is being edited and returns a string to use for the editor tool tip.
    // This class gets instantiated from XAML.
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class ValueToToolTipConverter : IMultiValueConverter 
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            Fx.Assert(values != null && values.Length == 2, "Invalid values passed into ValueToToolTipConverter");

            bool isEditing = (bool)values[1];
            string value = isEditing ? null : EditorUtilities.GetDisplayName(values[0]);

            return string.IsNullOrEmpty(value) ? null : value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
