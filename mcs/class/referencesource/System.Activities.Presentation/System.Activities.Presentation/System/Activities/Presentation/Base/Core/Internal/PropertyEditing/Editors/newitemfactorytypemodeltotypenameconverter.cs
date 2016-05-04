//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Windows.Data;
    using System.Globalization;

    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;
    using System.Activities.Presentation;

    // <summary>
    // Converts an instance of NewItemFactoryTypeModel to its contained Type name.
    // </summary>
    internal class NewItemFactoryTypeModelToTypeNameConverter : IValueConverter 
    {

        // Converts an instance of NewItemFactoryTypeModel to its corresponding type name
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {

            if (typeof(string).IsAssignableFrom(targetType)) 
            {
                NewItemFactoryTypeModel model = value as NewItemFactoryTypeModel;
                if (model != null && model.Type != null) 
                {
                    return model.Type.Name;
                }
            }

            return string.Empty;
        }

        // This class is only a one-way converter
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException());
        }
    }
}
