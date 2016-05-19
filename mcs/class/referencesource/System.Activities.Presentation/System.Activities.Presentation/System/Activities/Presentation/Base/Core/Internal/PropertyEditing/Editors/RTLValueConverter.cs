//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{

    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Resources;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation;
    using System.Runtime;

    // <summary>
    // Checks the property entry and converts it
    // to appropriate FlowDirection value which is returned back.
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class RTLValueConveter : IMultiValueConverter 
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            FlowDirection returnValue = FlowDirection.LeftToRight;
            Fx.Assert(values.Length == 3, "Incorrect values in the MultiValueConverter!");
            if (values.Length == 3) 
            {
                ModelPropertyEntry propertyEntry = values[1] as ModelPropertyEntry;
                if (propertyEntry != null) 
                {
                    if (!propertyEntry.DisplayName.Equals("Name")) 
                    {
                        if (targetType == typeof(FlowDirection)) 
                        {
                            object propertyValue = values[0];
                            if (propertyValue == null || propertyValue.GetType() == typeof(string)) 
                            {
                                //customize it to controls FlowDirection Property
                                returnValue = (FlowDirection)PropertyInspectorResources.GetResources()["SelectedControlFlowDirectionRTL"];
                            }
                        }
                    }
                }
            }
            return returnValue;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

    }
}
