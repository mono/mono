//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Model 
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Text;

    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Resources;
    using System.Activities.Presentation.Internal.Properties;
    using System.Activities.Presentation.Internal.PropertyEditing.Editors;

    // <summary>
    // Concrete implementation of PropertyValue that delegates to ModelPropertyEntryBase for
    // all actions.
    // </summary>
    internal class ModelPropertyValue : PropertyValue 
    {

        // Object used to mark a property value that should be cleared instead of set
        private static readonly object ClearValueMarker = new object();

        // CultureInfo instance we use for formatting values so that they reflect what is in Xaml
        private static CultureInfo _xamlCultureInfo;

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="parentProperty">Parent ModelPropertyEntryBase</param>
        public ModelPropertyValue(ModelPropertyEntryBase parentProperty) : base(parentProperty) 
        {
        }

        // <summary>
        // Returns the source of this property value
        // </summary>
        public override PropertyValueSource Source 
        {
            get {
                return ParentModelPropertyEntry.Source;
            }
        }

        // <summary>
        // Returns true if this value represents the default value of the property
        // </summary>
        public override bool IsDefaultValue 
        {
            get {
                return Source == DependencyPropertyValueSource.DefaultValue;
            }
        }

        // <summary>
        // Returns true if the value contained by this property is mixed
        // </summary>
        public override bool IsMixedValue 
        {
            get {
                return ParentModelPropertyEntry.IsMixedValue;
            }
        }

        // <summary>
        // Returns true if custom TypeConverter exists and if it can convert
        // the value from string.
        // </summary>
        public override bool CanConvertFromString 
        {
            get {
                return ParentModelPropertyEntry.Converter != null &&
                    ParentModelPropertyEntry.Converter.CanConvertFrom(typeof(string));
            }
        }

        // <summary>
        // Gets a flag indicating whether this PropertyValue has sub properties
        // </summary>
        public override bool HasSubProperties 
        {
            get {
                return ParentModelPropertyEntry.HasSubProperties;
            }
        }

        // <summary>
        // Gets the sub-properties of the PropertyValue
        // </summary>
        public override PropertyEntryCollection SubProperties 
        {
            get {
                return ParentModelPropertyEntry.SubProperties;
            }
        }

        // <summary>
        // Gets a flag indicating whether this PropertyValue represents a collection
        // </summary>
        public override bool IsCollection 
        {
            get {
                return ParentModelPropertyEntry.IsCollection;
            }
        }

        // <summary>
        // Gets the collection represented by this PropertyValue
        // </summary>
        public override PropertyValueCollection Collection 
        {
            get {
                return ParentModelPropertyEntry.Collection;
            }
        }

        // <summary>
        // This is an internal helper to which we can bind and on which we fire PropertyChanged
        // event when the Name sub-property (if one exists) changes.  More-specifically,
        // we bind to this property in CollectionEditor to display the Type as well as the
        // Name of the items in the collection.  This property is accessed from XAML.
        // </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public PropertyValue NameSensitiveInstance 
        {
            get {
                return this;
            }
        }

        // <summary>
        // Always catch exceptions
        // </summary>
        protected override bool CatchExceptions 
        {
            get {
                return true;
            }
        }

        // <summary>
        // Gets an en-us CultureInfo that ignores user-specified settings
        // </summary>
        private static CultureInfo XamlCultureInfo 
        {
            get {
                if (_xamlCultureInfo == null)
                {
                    _xamlCultureInfo = new CultureInfo("en-us", false);
                }

                return _xamlCultureInfo;
            }
        }

        // Convenience accessor
        private ModelPropertyEntryBase ParentModelPropertyEntry 
        {
            get {
                return (ModelPropertyEntryBase)this.ParentProperty;
            }
        }

        // <summary>
        // Validates the value using the TypeConverter, if one exists
        // </summary>
        // <param name="valueToValidate">Value to validate</param>
        protected override void ValidateValue(object valueToValidate) 
        {
            // Noop.  We used to rely on TypeConverter.IsValid() here, but it turns out
            // that a bunch of standard TypeConverters don't really work (eg. Int32TypeConverter
            // returns true for IsValid("abc") and EnumConverter returns false for
            // IsValid(MyEnum.One | MyEnum.Two) even if MyEnum if adorned with FlagsAttribute)
        }

        // Called when there exists a Name sub-property for this value and it changes
        internal void OnNameSubPropertyChanged() 
        {
            // Updates XAML bindings (collection editor item-display-name-template for one)
            this.OnPropertyChanged("NameSensitiveInstance");
        }

        // <summary>
        // Convert the specified string to a value
        // </summary>
        // <param name="stringToConvert"></param>
        // <returns></returns>
        protected override object ConvertStringToValue(string stringToConvert) 
        {
            if (this.ParentProperty.PropertyType == typeof(string)) 
            {
                return stringToConvert;
            }
            else if (string.IsNullOrEmpty(stringToConvert)) 
            {

                // If the type of this property is string:
                //
                //      StringValue of ''   -> set Value to ''
                //      StringValue of null -> ClearValue()
                //
                // Otherwise
                //
                //      StringValue of ''   -> ClearValue()
                //      StringValue of null -> ClearValue()
                //
                if (stringToConvert != null && typeof(string).Equals(this.ParentProperty.PropertyType))
                {
                    return null;
                }
                else
                {
                    return ClearValueMarker;
                }

            }
            else if (EditorUtilities.IsNullableEnumType(this.ParentProperty.PropertyType) && stringToConvert.Equals(EditorUtilities.NullString, StringComparison.Ordinal))
            {
                // PS 107537: Special case handling when converting a string to a nullable enum type.
                return null;
            }
            else if (this.ParentModelPropertyEntry.Converter != null &&
                this.ParentModelPropertyEntry.Converter.CanConvertFrom(typeof(string)))
            {

                return this.ParentModelPropertyEntry.Converter.ConvertFromString(null, XamlCultureInfo, stringToConvert);
            }

            throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(
                CultureInfo.CurrentCulture,
                Resources.PropertyEditing_NoStringToValueConversion,
                this.ParentProperty.DisplayName)));
        }

        // <summary>
        // Convert the specified value to a string
        // </summary>
        // <param name="valueToConvert"></param>
        // <returns></returns>
        protected override string ConvertValueToString(object valueToConvert) 
        {
            string stringValue = string.Empty;

            if (valueToConvert == null) 
            {
                if (typeof(IList).IsAssignableFrom(this.ParentProperty.PropertyType)) 
                {
                    stringValue = Resources.PropertyEditing_DefaultCollectionStringValue;
                }
                else if (EditorUtilities.IsNullableEnumType(this.ParentProperty.PropertyType))
                {
                    // PS 107537: Special case handling when converting a nullable enum type to a string.
                    return EditorUtilities.NullString;
                }
                return stringValue;
            }
            else if ((stringValue = valueToConvert as string) != null) 
            {
                return stringValue;
            }

            TypeConverter typeConverter = this.ParentModelPropertyEntry.Converter;
            if (valueToConvert is Array) 
            {
                stringValue = Resources.PropertyEditing_DefaultArrayStringValue;
            }
            else if (valueToConvert is IList
                || valueToConvert is ICollection
                || ModelUtilities.ImplementsICollection(valueToConvert.GetType())
                || ModelUtilities.ImplementsIList(valueToConvert.GetType())) 
            {
                stringValue = Resources.PropertyEditing_DefaultCollectionStringValue;
            }
            else if (valueToConvert is IEnumerable) 
            {
                stringValue = Resources.PropertyEditing_DefaultEnumerableStringValue;
            }
            else if (typeConverter != null && typeConverter.CanConvertTo(typeof(string))) 
            {
                stringValue = typeConverter.ConvertToString(null, XamlCultureInfo, valueToConvert);
            }
            else 
            {
                stringValue = valueToConvert.ToString();
            }

            return stringValue ?? string.Empty;
        }


        // <summary>
        // Redirect the call to parent PropertyEntry
        // </summary>
        // <returns></returns>
        protected override object GetValueCore() 
        {
            return ParentModelPropertyEntry.GetValueCore();
        }

        // <summary>
        // Redirect the call to parent PropertyEntry
        // </summary>
        // <param name="value"></param>
        protected override void SetValueCore(object value) 
        {
            if (value == ClearValueMarker)
            {
                ParentModelPropertyEntry.ClearValue();
            }
            else
            {
                ParentModelPropertyEntry.SetValueCore(value);
            }
        }

        // <summary>
        // Apply the FlowDirection to the resource.
        // </summary>
        private void CheckAndSetFlowDirectionResource() 
        {
            // Check if the value being edited is FlowDirection
            // and if so, reset the resource to the current value that the user is setting.
            // This will refresh the property inspector and all the string editors, showing "string" properties,
            // would have their FlowDirection set to the current value.
            if (ParentModelPropertyEntry.PropertyName.Equals(FrameworkElement.FlowDirectionProperty.Name)) 
            {
                object value = Value;
                if (value != null) 
                {
                    PropertyInspectorResources.GetResources()["SelectedControlFlowDirectionRTL"] = value;
                }
            }
        }

        // <summary>
        // Redirect the call to parent PropertyEntry
        // </summary>
        public override void ClearValue() 
        {
            ParentModelPropertyEntry.ClearValue();
        }

        // <summary>
        // Fires the appropriate PropertyChanged events
        // </summary>
        public void OnUnderlyingModelChanged() 
        {
            CheckAndSetFlowDirectionResource();
            this.NotifyRootValueChanged();
        }

        // <summary>
        // Fires the appropriate PropertyChanged events
        // </summary>
        public void OnUnderlyingSubModelChanged() 
        {
            this.NotifySubPropertyChanged();
        }

        // <summary>
        // Called when there is an error setting or getting a PropertyValue.
        // Displays an error dialog.
        // </summary>
        // <param name="e"></param>
        protected override void OnPropertyValueException(PropertyValueExceptionEventArgs e) 
        {
            if (e.Source == PropertyValueExceptionSource.Set) 
            {
                if (e.Exception != null) 
                {
                    Debug.WriteLine(e.Exception.ToString());
                }

                ErrorReporting.ShowErrorMessage(e.Exception.Message);

                base.OnPropertyValueException(e);
            }
            else 
            {
                base.OnPropertyValueException(e);
            }
        }

        // <summary>
        // Debuging-friendly ToString()
        // </summary>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override string ToString() 
        {
            try 
            {
                string value;
                if (this.Value == null)
                {
                    value = "{null}";
                }
                else 
                {
                    value = this.StringValue;
                    if (string.IsNullOrEmpty(value))
                    {
                        value = "{empty}";
                    }
                }

                return string.Format(CultureInfo.CurrentCulture, "{0} (PropertyValue)", value ?? "{null}");
            }
            catch 
            {
                return base.ToString();
            }
        }
    }
}
