//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows.Data;

    using System.Runtime;
    using System.Activities.Presentation;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.Properties;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;

    // <summary>
    // Retrieves StandardValues from a passed in PropertyValue, making sure that if a TypeConverter
    // exists and if it supports ConvertToString() method, it will be called on each value contained
    // in the StandardValues collection.
    // This class is instantiated through XAML
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class PropertyValueToStandardValuesConverter : IValueConverter 
    {
        // IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {

            Fx.Assert(typeof(IEnumerable).IsAssignableFrom(targetType), "Expecting IEnumerable as the targetType");

            PropertyValue propertyValue = value as PropertyValue;
            if (propertyValue == null)
            {
                return null;
            }

            ModelPropertyEntryBase parentProperty = (ModelPropertyEntryBase)propertyValue.ParentProperty;
            return new ConvertedStandardValuesCollection(parentProperty, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException());
        }


        // <summary>
        // An implementation of ICollection that defers access to the standard
        // values collection.  Accessing the StandardValues collection may be
        // expensive, so we defer it until we absolutely need it.
        // </summary>
        private class ConvertedStandardValuesCollection : ICollection 
        {

            private ModelPropertyEntryBase _property;
            private CultureInfo _culture;
            private ICollection _contents;

            internal ConvertedStandardValuesCollection(ModelPropertyEntryBase property, CultureInfo culture) 
            {
                _property = property;
                _culture = culture;
            }

            public int Count 
            {
                get { return Contents.Count; }
            }

            public bool IsSynchronized 
            {
                get { return false; }
            }

            public object SyncRoot 
            {
                get { return Contents.SyncRoot; }
            }

        // IEnumerable Members

            private ICollection Contents 
            {
                get {
                    if (_contents == null) 
                    {
                        ICollection standardValues = _property.StandardValues;
                        TypeConverter converter = _property.Converter;

                        if (standardValues != null && (converter == null || !converter.CanConvertTo(typeof(string)))) 
                        {
                            _contents = standardValues;
                        }
                        else 
                        {
                            ArrayList convertedStandardValues = new ArrayList(standardValues == null ? 0 : standardValues.Count);
                            if (standardValues != null) 
                            {
                                foreach (object standardValue in standardValues) 
                                {
                                    convertedStandardValues.Add(converter.ConvertToString(null, _culture, standardValue));
                                }
                            }
                            _contents = convertedStandardValues;
                        }

                        // PS 107537: Special-case handling for nullable enum types
                        if (EditorUtilities.IsNullableEnumType(_property.PropertyType))
                        {
                            ArrayList filteredStandardValues = new ArrayList();
                            filteredStandardValues.Add(EditorUtilities.NullString);
                            foreach (var i in (ArrayList)_contents)
                            {
                                if (i != null)
                                {
                                    filteredStandardValues.Add(i);
                                }
                            }
                            _contents = filteredStandardValues;
                        }
                    }

                    return _contents;
                }
            }

        // ICollection Members

            public void CopyTo(Array array, int index) 
            {
                Contents.CopyTo(array, index);
            }

            public IEnumerator GetEnumerator() 
            {
                return Contents.GetEnumerator();
            }

        }
    }
}
