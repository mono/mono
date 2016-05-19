namespace System.Activities.Presentation.PropertyEditing {
    using System.Globalization;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation.Internal.Properties;
    using System.Activities.Presentation;

    /// <summary>
    /// This class provides a data model for the underlying property value.
    /// </summary>
    public abstract class PropertyValue : INotifyPropertyChanged {

        private PropertyEntry _parentProperty;

        /// <summary>
        /// Creates a PropertyValue.  For host infrastructure.
        /// </summary>
        /// <param name="parentProperty">The PropertyEntry that corresponds to this PropertyValue</param>
        /// <exception cref="ArgumentNullException">When parentProperty is null</exception>
        protected PropertyValue(PropertyEntry parentProperty) {
            if (parentProperty == null)
                throw FxTrace.Exception.ArgumentNull("parentProperty");

            _parentProperty = parentProperty;
        }

        /// <summary>
        /// Event that gets fired when any properties of the PropertyValue class change.
        /// Note that a "Value" and "StringValue" property changed notification gets fired
        /// either when the Value or StringValue get set to a new instance OR when any
        /// sub-properties of this PropertyValue change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event fired when the Value or StringValue properties of this class
        /// get updated with new instances.
        /// </summary>
        public event EventHandler RootValueChanged;

        /// <summary>
        /// Event fired when the any of the sub-properties of this PropertyValue
        /// or its sub-properties get updated with new value instances.
        /// </summary>
        public event EventHandler SubPropertyChanged;

        /// <summary>
        /// Gets the parent PropertyEntry.
        /// </summary>
        public PropertyEntry ParentProperty { get { return _parentProperty; } }

        /// <summary>
        /// Gets a PropertyValueSource that contains information 
        /// about where this value is coming from.
        /// </summary>
        public abstract PropertyValueSource Source { get; }

        /// <summary>
        /// Returns true if Value is the default value for the property
        /// </summary>
        public abstract bool IsDefaultValue { get; }

        /// <summary>
        /// Returns true if this value represents a property for multiple objects with 
        /// more than one value - for example 2 Buttons with different values for Background
        /// If this property is true then Value will return null and and StringValue will return 
        /// String.Empty.
        /// </summary>
        public abstract bool IsMixedValue { get; }

        /// <summary>
        /// Throws if the value is invalid
        /// </summary>
        /// <param name="valueToValidate">value to validate</param>
        protected abstract void ValidateValue(object valueToValidate);

        /// <summary>
        /// Gets a flag indicating whether the underlying value can be converted from a string 
        /// </summary>
        public abstract bool CanConvertFromString { get; }

        /// <summary>
        /// Returns the given string as a value - used to convert StringValue to Value
        /// Typical implementations would use the TypeConverter for the underlying property
        /// This method should not catch exceptions, it should propagate them.
        /// </summary>
        protected abstract object ConvertStringToValue(string value);

        /// <summary>
        /// Returns the value as a String - used to convert Value to StringValue
        /// Typical implementations would use the TypeConverter for the underlying property
        /// </summary>
        protected abstract string ConvertValueToString(object value);

        /// <summary>
        /// Gets the underlying property value.
        /// </summary>
        protected abstract object GetValueCore();

        /// <summary>
        /// Sets the underlying property value.  This method should not catch
        /// exceptions, but allow them to propagate.
        /// </summary>
        protected abstract void SetValueCore(object value);

        /// <summary>
        /// Clears this value such that it is unset.
        /// </summary>
        public abstract void ClearValue();

        // Value Property

        /// <summary>
        /// Gets or sets the underlying property value.  Both Value and StringValue
        /// will raise the appropriate change notifications.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        public object Value
        {
            get {
                object returnValue = null;

                if (this.CatchExceptions) {
                    try {
                        returnValue = GetValueCore();
                    }
                    catch (Exception ex) {
                        OnPropertyValueException(new PropertyValueExceptionEventArgs(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Error_ValueGetFailed),
                            this,
                            PropertyValueExceptionSource.Get,
                            ex));
                    }
                }
                else {
                    returnValue = GetValueCore();
                }

                return returnValue;
            }
            set 
            {
                try 
                {
                    SetValueImpl(value);
                }
                catch (Exception ex) 
                {
                    bool isValidationException = ex is ValidationException;

                    //show error message if we do catch exception or exception is ValidationException
                    if (this.CatchExceptions || isValidationException)
                    {
                        OnPropertyValueException(new PropertyValueExceptionEventArgs(
                            string.Format(CultureInfo.CurrentCulture, Resources.Error_ValueSetFailed),
                            this,
                            PropertyValueExceptionSource.Set,
                            ex));
                    }

                    //rethrow if we do not catch exception or exception is ValidationException (it should be handled by the control)
                    if (!this.CatchExceptions || isValidationException)
                    {
                        throw;
                    }
                }
            }
        }

        private void SetValueImpl(object value) {
            ValidateValue(value);
            SetValueCore(value);
            NotifyValueChanged();
            OnRootValueChanged();
        }


        // StringValue Property

        /// <summary>
        /// Gets or sets the underlying property value as a string.  Both Value and StringValue
        /// will raise the appropriate change notifications.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        public string StringValue
        {
            get {
                string returnValue = string.Empty;

                //If there is an error event handler then use it otherwise let the exception
                //propogate
                if (this.CatchExceptions) {
                    try {
                        //Caching opportunity here
                        returnValue = this.ConvertValueToString(this.Value);
                    }
                    catch (Exception ex) {
                        OnPropertyValueException(new PropertyValueExceptionEventArgs(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Error_CannotConvertValueToString),
                            this,
                            PropertyValueExceptionSource.Get,
                            ex));
                    }
                }
                else {
                    //Caching opportunity here
                    returnValue = this.ConvertValueToString(this.Value);
                }

                return returnValue;
            }
            set {
                //If there is an error event handler then use it otherwise let the exception
                //propogate
                if (CatchExceptions) {
                    try {
                        this.Value = this.ConvertStringToValue(value);
                    }
                    catch (Exception ex) {
                        OnPropertyValueException(
                            new PropertyValueExceptionEventArgs(
                                string.Format(
                                    CultureInfo.CurrentCulture,
                                    Resources.Error_CannotUpdateValueFromStringValue),
                                this,
                                PropertyValueExceptionSource.Set,
                                ex));
                    }
                }
                else {
                    this.Value = this.ConvertStringToValue(value);
                }
            }
        }


        // SubProperties

        /// <summary>
        /// Gets a flag indicating whether the type of this property 
        /// supports sub-properties.  Typical implementations will use a TypeConverter
        /// to verify whether sub-properties exist.
        /// </summary>
        public abstract bool HasSubProperties { get; }
        
        /// <summary>
        /// Gets a collection of sub-properties as PropertyEntry instances
        /// </summary>
        public abstract PropertyEntryCollection SubProperties { get; }


        // Collections

        /// <summary>
        /// Gets a flag indicating whether this PropertyValue models a property
        /// whose value is a collection.
        /// </summary>
        public abstract bool IsCollection { get; }

        /// <summary>
        /// Gets a collection of PropertyValue instances that correspond to the items
        /// in the collection when IsCollection is true.
        /// </summary>
        public abstract PropertyValueCollection Collection { get; }


        // Error Handling

        /// <summary>
        /// Event for host implementations to use for error handling.  Raised when StringValue or Value throws 
        /// and CatchExceptions is true.  If CatchExceptions is false, the exception will be thrown up the stack.
        /// </summary>
        public event EventHandler<PropertyValueExceptionEventArgs> PropertyValueException;

        /// <summary>
        /// Gets a boolean indicating whether exceptions thrown during value gets and sets 
        /// should be caught or propagated directly to the caller.  By default, exceptions
        /// are caught if there is at least one subscriber to the PropertyValueException event.
        /// </summary>
        protected virtual bool CatchExceptions {
            get {
                return PropertyValueException != null;
            }
        }

        /// <summary>
        /// Called when PropertyValue get or set fails.  Default implementation raises the
        /// PropertyValueException event.
        /// </summary>
        /// <param name="e">PropertyValueExceptionEventArgs</param>
        /// <exception cref="ArgumentNullException">When e is null</exception>
        protected virtual void OnPropertyValueException(PropertyValueExceptionEventArgs e) {
            if (e == null)
                throw FxTrace.Exception.ArgumentNull("e");

            if (PropertyValueException != null)
                PropertyValueException(this, e);
        }


        // Notification Helpers

        /// <summary>
        /// Raises change notification for all properties.  This should be called when
        /// the underlying object is changed externally (for example Button.Width is 
        /// changed on the design surface)
        /// </summary>
        protected virtual void NotifyRootValueChanged() {
            //When Value is updated or the model is reset we 
            //need to fire an "everything has changed" notification

            //This doesn't appear to work at all...
            //PropertyChanged(this, new PropertyChangedEventArgs("")); 

            //So notify these key changes individually
            OnPropertyChanged("IsDefaultValue");
            OnPropertyChanged("IsMixedValue");
            OnPropertyChanged("IsCollection");
            OnPropertyChanged("Collection");
            OnPropertyChanged("HasSubProperties");
            OnPropertyChanged("SubProperties");
            OnPropertyChanged("Source");
            OnPropertyChanged("CanConvertFromString");

            NotifyValueChanged();
            OnRootValueChanged();
        }

        /// <summary>
        /// Called to raise the SubPropertyChanged event.  This method should be called
        /// when one of the sub-properties of this property changes.  It raises changed
        /// events for Value, StringValue, and SubProperty
        /// </summary>
        protected void NotifySubPropertyChanged() {
            NotifyValueChanged();
            OnSubPropertyChanged();
        }

        /// <summary>
        /// Called to raise the changed events for Value and StringValue.  This method
        /// should only be called to trigger the refresh of the visual representation
        /// of this value.  If the value content actually changes, call NotifyRootValueChanged
        /// instead.
        /// </summary>
        private void NotifyValueChanged() {
            OnPropertyChanged("Value");
            NotifyStringValueChanged();
        }

        /// <summary>
        /// Raise change notification for StringValue
        /// </summary>
        private void NotifyStringValueChanged() {
            OnPropertyChanged("StringValue");
        }

        /// <summary>
        /// Called to raise the RootValueChanged event
        /// </summary>
        private void OnRootValueChanged() {
            if (RootValueChanged != null)
                RootValueChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called to raise the SubPropertyChanged event
        /// </summary>
        private void OnSubPropertyChanged() {
            if (SubPropertyChanged != null)
                SubPropertyChanged(this, EventArgs.Empty);
        }


        // INotifyPropertyChanged

        /// <summary>
        /// Raises the PropertyChanged event.  Subclasses that override this method
        /// should call the base class implementation.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

    }
}

