//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------


namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "The class in used in xaml.")]
    class ConnectorLabelVisibilityConverter : IMultiValueConverter
    {

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "The class is only used internally and not accessible externally.")]
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility connectorLabelVisibility = Visibility.Collapsed;
            string labelText = values[0] as string;
            PointCollection connectorPoints = values[1] as PointCollection;
            if (!String.IsNullOrEmpty(labelText) && connectorPoints != null)
            {
                int maxSegmentStartPoint;
                if (DesignerGeometryHelper.LongestSegmentLength(connectorPoints, out maxSegmentStartPoint) > Connector.MinConnectorSegmentLengthForLabel)
                {
                    connectorLabelVisibility = Visibility.Visible;
                }
            }
            return connectorLabelVisibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

    }
}
