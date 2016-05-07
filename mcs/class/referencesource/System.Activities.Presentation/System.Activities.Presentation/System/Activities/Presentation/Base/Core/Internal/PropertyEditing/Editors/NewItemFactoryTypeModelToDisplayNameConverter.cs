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
    // Converts an instance of NewItemFactoryTypeModel to its appropriate display name.
    // One way binding to NewItemFactoryTypeModel's DisplayName property also works, but
    // for the sake of having a single place that converts NewItemFactoryTypeModel to
    // strings, we expose this internal converter.
    // </summary>
    internal class NewItemFactoryTypeModelToDisplayNameConverter : IValueConverter 
    {

        private static NewItemFactoryTypeModelToDisplayNameConverter _instance;

        // <summary>
        // Static instance accessor for all non-XAML related conversion needs
        // </summary>
        public static NewItemFactoryTypeModelToDisplayNameConverter Instance 
        {
            get {
                if (_instance == null)
                {
                    _instance = new NewItemFactoryTypeModelToDisplayNameConverter();
                }

                return _instance;
            }
        }

        // Converts an instance of NewItemFactoryTypeModel to its appropriate display name
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {

            if (typeof(string).IsAssignableFrom(targetType)) 
            {

                NewItemFactoryTypeModel model = value as NewItemFactoryTypeModel;
                if (model != null) 
                {
                    return model.DisplayName ?? string.Empty;
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
