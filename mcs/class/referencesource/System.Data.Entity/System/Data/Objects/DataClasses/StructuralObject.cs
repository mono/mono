//---------------------------------------------------------------------
// <copyright file="DataObject.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Data.Common.Utils;

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// This class contains the common methods need for an date object.
    /// </summary>
    [DataContract(IsReference = true)]
    [Serializable]
    public abstract class StructuralObject : INotifyPropertyChanging, INotifyPropertyChanged
    {
        // ------
        // Fields
        // ------

        // This class contains no fields that are serialized, but it's important to realize that
        // adding or removing a serialized field is considered a breaking change.  This includes
        // changing the field type or field name of existing serialized fields. If you need to make
        // this kind of change, it may be possible, but it will require some custom
        // serialization/deserialization code.

        /// <summary>
        /// Public constant name used for change tracking
        /// Providing this definition allows users to use this constant instead of
        /// hard-coding the string. This helps to ensure the property name is correct
        /// and allows faster comparisons in places where we are looking for this specific string.
        /// Users can still use the case-sensitive string directly instead of the constant,
        /// it will just be slightly slower on comparison.
        /// Including the dash (-) character around the name ensures that this will not conflict with
        /// a real data property, because -EntityKey- is not a valid identifier name
        /// </summary>
        public static readonly string EntityKeyPropertyName = "-EntityKey-";

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Notification that a property has been changed.
        /// </summary>
        /// <remarks>
        /// The PropertyChanged event can indicate all properties on the 
        /// object have changed by using either a null reference 
        /// (Nothing in Visual Basic) or String.Empty as the property name 
        /// in the PropertyChangedEventArgs.
        /// </remarks>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region INotifyPropertyChanging Members

        /// <summary>
        /// Notification that a property is about to be changed.
        /// </summary>
        /// <remarks>
        /// The PropertyChanging event can indicate all properties on the 
        /// object are changing by using either a null reference 
        /// (Nothing in Visual Basic) or String.Empty as the property name 
        /// in the PropertyChangingEventArgs.
        /// </remarks>
        [field: NonSerialized]
        public event PropertyChangingEventHandler PropertyChanging;

        #endregion
        #region Protected Overrideable

        /// <summary>
        /// Invokes the PropertyChanged event.  
        /// </summary>
        /// <param name="property">
        /// The string name of the of the changed property.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        /// <summary>
        /// Invokes the PropertyChanging event.  
        /// </summary>
        /// <param name="property">
        /// The string name of the of the changing property.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        protected virtual void OnPropertyChanging(string property)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging.Invoke(this, new PropertyChangingEventArgs(property));
            }
        }

        #endregion
        #region Protected Helper

        /// <summary>
        /// The minimum DateTime value allowed in the store
        /// </summary>
        /// <value>
        /// The minimum DateTime value allowed in the store
        /// </value>
        protected static DateTime DefaultDateTimeValue()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// This method is called whenever a change is going to be made to an object
        /// property's value.
        /// </summary>
        /// <param name="property">
        /// The name of the changing property.
        /// </param>
        /// <param name="value">
        /// The current value of the property.
        /// </param> 
        /// <exception cref="System.ArgumentNullException">
        /// When parameter member is null (Nothing in Visual Basic).
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        protected virtual void ReportPropertyChanging(
            string property)
        {
            EntityUtil.CheckStringArgument(property, "property");

            OnPropertyChanging(property);
        }

        /// <summary>
        /// This method is called whenever a change is made to an object 
        /// property's value.
        /// </summary>
        /// <param name="property">
        /// The name for the changed property.
        /// </param>        
        /// <exception cref="System.ArgumentNullException">
        /// When parameter member is null (Nothing in Visual Basic).
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        protected virtual void ReportPropertyChanged(
            string property)
        {
            EntityUtil.CheckStringArgument(property, "property");

            OnPropertyChanged(property);
        }

        /// <summary>
        /// Lazily creates a complex type if the current value is null
        /// </summary>
        /// <remarks>
        /// Unlike most of the other helper methods in this class, this one is not static
        /// because it references the SetValidValue for complex objects, which is also not static
        /// because it needs a reference to this.
        /// </remarks>
        /// <typeparam name="T">
        /// Type of complex type to get a valid value for
        /// </typeparam>
        /// <param name="currentValue">
        /// The current value of the complex type property
        /// </param>
        /// <param name="property">
        /// The name of the property that is calling this method
        /// </param>
        /// <param name="isInitialized">
        /// True if the field has already been explicitly set by the user.
        /// </param>
        /// <returns>
        /// The new value of the complex type property
        /// </returns>
        protected internal T GetValidValue<T>(T currentValue, string property, bool isNullable, bool isInitialized) where T : ComplexObject, new()
        {
            // If we support complex type inheritance we will also need to check if T is abstract            
            if (!isNullable && !isInitialized)
            {
                currentValue = SetValidValue(currentValue, new T(), property);
            }

            return currentValue;
        }

        /// <summary>
        /// This method is called by a ComplexObject contained in this Entity 
        /// whenever a change is about to be made to a property of the  
        /// ComplexObject so that the change can be forwarded to the change tracker.
        /// </summary>
        /// <param name="entityMemberName">
        /// The name of the top-level entity property that contains the ComplexObject that is calling this method.
        /// </param>
        /// <param name="complexObject">
        /// The instance of the ComplexObject on which the property is changing.
        /// </param>
        /// <param name="complexMemberName">
        /// The name of the changing property on complexObject.
        /// </param>                
        internal abstract void ReportComplexPropertyChanging(
            string entityMemberName, ComplexObject complexObject, string complexMemberName);

        /// <summary>
        /// This method is called by a ComplexObject contained in this Entity 
        /// whenever a change has been made to a property of the  
        /// ComplexObject so that the change can be forwarded to the change tracker.
        /// </summary>
        /// <param name="entityMemberName">
        /// The name of the top-level entity property that contains the ComplexObject that is calling this method.
        /// </param>
        /// <param name="complexObject">
        /// The instance of the ComplexObject on which the property is changing.
        /// </param>
        /// <param name="complexMemberName">
        /// The name of the changing property on complexObject.
        /// </param>        
        internal abstract void ReportComplexPropertyChanged(
            string entityMemberName, ComplexObject complexObject, string complexMemberName);

        /// <summary>
        /// Determines whether the structural object is attached to a change tracker or not
        /// </summary>
        internal abstract bool IsChangeTracked { get; }

        /// <summary>
        /// Determines whether the specified byte arrays contain identical values
        /// </summary>
        /// <param name="first">The first byte array value to compare</param>
        /// <param name="second">The second byte array value to compare</param>
        /// <returns>
        ///   <c>true</c> if both arrays are <c>null</c>, or if both arrays are of
        ///   the same length and contain the same byte values; otherwise <c>false</c>.
        /// </returns>
        protected internal static bool BinaryEquals(byte[] first, byte[] second)
        {
            if (object.ReferenceEquals(first, second))
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            return ByValueEqualityComparer.CompareBinaryValues(first, second);
        }

        /// <summary>
        /// Duplicates the current byte value.
        /// </summary>
        /// <param name="currentValue">
        /// The current byte array value
        /// </param>
        /// <returns>
        /// Must return a copy of the values because byte arrays are mutable without providing a
        /// reliable mechanism for us to track changes.  This allows us to treat byte arrays like
        /// structs which is at least a somewhat understood mechanism.
        /// </returns>
        protected internal static byte[] GetValidValue(byte[] currentValue)
        {
            if (currentValue == null)
            {
                return null;
            }
            return (byte[])currentValue.Clone();
        }

        /// <summary>
        /// Makes sure the Byte [] value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// The value passed into the property setter.
        /// </param>
        /// <param name="isNullable">
        /// Flag indicating if this property is allowed to be null.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// Returns the value if valid.
        /// </returns>
        /// <exception cref="System.Data.ConstraintException">
        /// If value is null for a non nullable value.
        /// </exception>
        protected internal static Byte[] SetValidValue(Byte[] value, bool isNullable, string propertyName)
        {
            if (value == null)
            {
                if (!isNullable)
                {
                    EntityUtil.ThrowPropertyIsNotNullable(propertyName);
                }
                return value;
            }
            return (byte[])value.Clone();
        }

        /// <summary>
        /// Makes sure the Byte [] value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// The value passed into the property setter.
        /// </param>
        /// <param name="isNullable">
        /// Flag indicating if this property is allowed to be null.
        /// </param>
        /// <returns>
        /// Returns the value if valid.
        /// </returns>
        /// <exception cref="System.Data.ConstraintException">
        /// If value is null for a non nullable value.
        /// </exception>
        protected internal static Byte[] SetValidValue(Byte[] value, bool isNullable)
        {
            return SetValidValue(value, isNullable, null);
        }

        /// <summary>
        /// Makes sure the boolean value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Boolean value.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The Boolean value.
        /// </returns>
        protected internal static bool SetValidValue(bool value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the boolean value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Boolean value.
        /// </param>
        /// <returns>
        /// The Boolean value.
        /// </returns>
        protected internal static bool SetValidValue(bool value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the boolean value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Boolean value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The Boolean value.
        /// </returns>
        protected internal static Nullable<bool> SetValidValue(Nullable<bool> value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the boolean value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Boolean value
        /// </param>
        /// <returns>
        /// The Boolean value.
        /// </returns>
        protected internal static Nullable<bool> SetValidValue(Nullable<bool> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the byte value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Byte value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The Byte value.
        /// </returns>
        protected internal static byte SetValidValue(byte value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the byte value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Byte value
        /// </param>
        /// <returns>
        /// The Byte value.
        /// </returns>
        protected internal static byte SetValidValue(byte value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the byte value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Byte value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The Byte value.
        /// </returns>
        protected internal static Nullable<byte> SetValidValue(Nullable<byte> value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the byte value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Byte value
        /// </param>
        /// <returns>
        /// The Byte value.
        /// </returns>
        protected internal static Nullable<byte> SetValidValue(Nullable<byte> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the sbyte value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// sbyte value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The sbyte value.
        /// </returns>
        [CLSCompliant(false)]
        protected internal static sbyte SetValidValue(sbyte value, string propertyName)
        {
            // no checks yet
            return value;
        }

 
        /// <summary>
        /// Makes sure the sbyte value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// sbyte value
        /// </param>
        /// <returns>
        /// The sbyte value.
        /// </returns>
        [CLSCompliant(false)]
        protected internal static sbyte SetValidValue(sbyte value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the sbyte value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// sbyte value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The sbyte value.
        /// </returns>
        [CLSCompliant(false)]
        protected internal static Nullable<sbyte> SetValidValue(Nullable<sbyte> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the sbyte value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// sbyte value
        /// </param>
        /// <returns>
        /// The sbyte value.
        /// </returns>
        [CLSCompliant(false)]
        protected internal static Nullable<sbyte> SetValidValue(Nullable<sbyte> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the datetime value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// datetime value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The datetime value.
        /// </returns>
        protected internal static DateTime SetValidValue(DateTime value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the datetime value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// datetime value
        /// </param>
        /// <returns>
        /// The datetime value.
        /// </returns>
        protected internal static DateTime SetValidValue(DateTime value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the datetime value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// datetime value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The datetime value.
        /// </returns>
        protected internal static Nullable<DateTime> SetValidValue(Nullable<DateTime> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the datetime value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// datetime value
        /// </param>
        /// <returns>
        /// The datetime value.
        /// </returns>
        protected internal static Nullable<DateTime> SetValidValue(Nullable<DateTime> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the timespan value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// timespan value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The timspan value.
        /// </returns>
        protected internal static TimeSpan SetValidValue(TimeSpan value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the timespan value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// timespan value
        /// </param>
        /// <returns>
        /// The timspan value.
        /// </returns>
        protected internal static TimeSpan SetValidValue(TimeSpan value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the TimeSpan value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// timespan value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The timespan value.
        /// </returns>
        protected internal static Nullable<TimeSpan> SetValidValue(Nullable<TimeSpan> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the TimeSpan value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// timespan value
        /// </param>
        /// <returns>
        /// The timespan value.
        /// </returns>
        protected internal static Nullable<TimeSpan> SetValidValue(Nullable<TimeSpan> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the datetimeoffset value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// datetimeoffset value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The datetimeoffset value.
        /// </returns>
        protected internal static DateTimeOffset SetValidValue(DateTimeOffset value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the datetimeoffset value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// datetimeoffset value
        /// </param>
        /// <returns>
        /// The datetimeoffset value.
        /// </returns>
        protected internal static DateTimeOffset SetValidValue(DateTimeOffset value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the datetimeoffset value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// datetimeoffset value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The datetimeoffset value.
        /// </returns>
        protected internal static Nullable<DateTimeOffset> SetValidValue(Nullable<DateTimeOffset> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the datetimeoffset value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// datetimeoffset value
        /// </param>
        /// <returns>
        /// The datetimeoffset value.
        /// </returns>
        protected internal static Nullable<DateTimeOffset> SetValidValue(Nullable<DateTimeOffset> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Ensure that the input is a valid decimal value
        /// </summary>
        /// <param name="value">
        /// decimal value.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The decimal value.
        /// </returns>
        protected internal static Decimal SetValidValue(Decimal value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Ensure that the input is a valid decimal value
        /// </summary>
        /// <param name="value">
        /// proposed value
        /// </param>
        /// <returns>new value</returns>
        protected internal static Decimal SetValidValue(Decimal value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Ensure that the input is a valid decimal value
        /// </summary>
        /// <param name="value">
        /// decimal value.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The decimal value.
        /// </returns>
        protected internal static Nullable<Decimal> SetValidValue(Nullable<Decimal> value, string propertyName)
        {
            // no checks yet
            return value;
        }
        

        /// <summary>
        /// Ensure that the input is a valid decimal value
        /// </summary>
        /// <param name="value">
        /// decimal value.
        /// </param>
        /// <returns>
        /// The decimal value.
        /// </returns>
        protected internal static Nullable<Decimal> SetValidValue(Nullable<Decimal> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the double value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// double value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// the double value
        /// </returns>
        protected internal static double SetValidValue(double value, string propertyName)
        {
            // no checks yet
            return value;
        }

        
        /// <summary>
        /// Makes sure the double value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// double value
        /// </param>
        /// <returns>
        /// the double value
        /// </returns>
        protected internal static double SetValidValue(double value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the double value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// double value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// the double value
        /// </returns>
        protected internal static Nullable<double> SetValidValue(Nullable<double> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the double value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// double value
        /// </param>
        /// <returns>
        /// the double value
        /// </returns>
        protected internal static Nullable<double> SetValidValue(Nullable<double> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Single value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// float value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// the float value.
        /// </returns>
        protected internal static float SetValidValue(Single value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Single value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// float value
        /// </param>
        /// <returns>
        /// the float value.
        /// </returns>
        protected internal static float SetValidValue(Single value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Single value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Single value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// the nullable Single value
        /// </returns>
        protected internal static Nullable<Single> SetValidValue(Nullable<Single> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Single value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Single value
        /// </param>
        /// <returns>
        /// the nullable Single value
        /// </returns>
        protected internal static Nullable<Single> SetValidValue(Nullable<Single> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Guid value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Guid value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The Guid value
        /// </returns>
        protected internal static Guid SetValidValue(Guid value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Guid value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Guid value
        /// </param>
        /// <returns>
        /// The Guid value
        /// </returns>
        protected internal static Guid SetValidValue(Guid value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Guid value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Guid value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The nullable Guid value
        /// </returns>
        protected internal static Nullable<Guid> SetValidValue(Nullable<Guid> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Guid value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Guid value
        /// </param>
        /// <returns>
        /// The nullable Guid value
        /// </returns>
        protected internal static Nullable<Guid> SetValidValue(Nullable<Guid> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Int16 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Int16 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The Int16 value
        /// </returns>
        protected internal static Int16 SetValidValue(Int16 value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Int16 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Int16 value
        /// </param>
        /// <returns>
        /// The Int16 value
        /// </returns>
        protected internal static Int16 SetValidValue(Int16 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Int16 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Int16
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The Int16 value
        /// </returns>
        protected internal static Nullable<Int16> SetValidValue(Nullable<Int16> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Int16 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Int16
        /// </param>
        /// <returns>
        /// The Int16 value
        /// </returns>
        protected internal static Nullable<Int16> SetValidValue(Nullable<Int16> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Int32 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Int32 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The Int32 value
        /// </returns>
        protected internal static Int32 SetValidValue(Int32 value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Int32 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Int32 value
        /// </param>
        /// <returns>
        /// The Int32 value
        /// </returns>
        protected internal static Int32 SetValidValue(Int32 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Int32 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Int32 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The nullable Int32</returns>
        protected internal static Nullable<Int32> SetValidValue(Nullable<Int32> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Int32 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Int32 value
        /// </param>
        /// <returns>
        /// The nullable Int32</returns>
        protected internal static Nullable<Int32> SetValidValue(Nullable<Int32> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Int64 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Int64 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The Int64 value
        /// </returns>
        protected internal static Int64 SetValidValue(Int64 value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Int64 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// Int64 value
        /// </param>
        /// <returns>
        /// The Int64 value
        /// </returns>
        protected internal static Int64 SetValidValue(Int64 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the Int64 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Int64 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The nullable Int64 value
        /// </returns>
        protected internal static Nullable<Int64> SetValidValue(Nullable<Int64> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the Int64 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable Int64 value
        /// </param>
        /// <returns>
        /// The nullable Int64 value
        /// </returns>
        protected internal static Nullable<Int64> SetValidValue(Nullable<Int64> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the UInt16 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// UInt16 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The UInt16 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static UInt16 SetValidValue(UInt16 value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the UInt16 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// UInt16 value
        /// </param>
        /// <returns>
        /// The UInt16 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static UInt16 SetValidValue(UInt16 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the UInt16 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable UInt16 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The nullable UInt16 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static Nullable<UInt16> SetValidValue(Nullable<UInt16> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the UInt16 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable UInt16 value
        /// </param>
        /// <returns>
        /// The nullable UInt16 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static Nullable<UInt16> SetValidValue(Nullable<UInt16> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the UInt32 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// UInt32 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The UInt32 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static UInt32 SetValidValue(UInt32 value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the UInt32 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// UInt32 value
        /// </param>
        /// <returns>
        /// The UInt32 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static UInt32 SetValidValue(UInt32 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the UInt32 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable UInt32 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The nullable UInt32 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static Nullable<UInt32> SetValidValue(Nullable<UInt32> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the UInt32 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable UInt32 value
        /// </param>
        /// <returns>
        /// The nullable UInt32 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static Nullable<UInt32> SetValidValue(Nullable<UInt32> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the UInt64 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// UInt64 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The UInt64 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static UInt64 SetValidValue(UInt64 value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the UInt64 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// UInt64 value
        /// </param>
        /// <returns>
        /// The UInt64 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static UInt64 SetValidValue(UInt64 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Makes sure the UInt64 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable UInt64 value
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <returns>
        /// The nullable UInt64 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static Nullable<UInt64> SetValidValue(Nullable<UInt64> value, string propertyName)
        {
            // no checks yet
            return value;
        }


        /// <summary>
        /// Makes sure the UInt64 value being set for a property is valid.
        /// </summary>
        /// <param name="value">
        /// nullable UInt64 value
        /// </param>
        /// <returns>
        /// The nullable UInt64 value
        /// </returns>
        [CLSCompliant(false)]
        protected internal static Nullable<UInt64> SetValidValue(Nullable<UInt64> value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        /// Validates that the property is not longer than allowed, and throws if it is
        /// </summary>
        /// <param name="value">
        /// string value to be checked.
        /// </param>
        /// <param name="isNullable">
        /// Flag indicating if this property is allowed to be null.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <exception cref="System.Data.ConstraintException">
        /// The string value is null for a non-nullable string
        /// </exception>
        protected internal static string SetValidValue(string value, bool isNullable, string propertyName)
        {
            if (value == null)
            {
                if (!isNullable)
                {
                    EntityUtil.ThrowPropertyIsNotNullable(propertyName);
                }
            }
            return value;
        }


        /// <summary>
        /// Validates that the property is not longer than allowed, and throws if it is
        /// </summary>
        /// <param name="value">
        /// string value to be checked.
        /// </param>
        /// <param name="isNullable">
        /// Flag indicating if this property is allowed to be null.
        /// </param>
        /// <exception cref="System.Data.ConstraintException">
        /// The string value is null for a non-nullable string
        /// </exception>
        protected internal static string SetValidValue(string value, bool isNullable)
        {
            return SetValidValue(value, isNullable, null);
        }

        /// <summary>
        /// Validates that the property is not null, and throws if it is
        /// </summary>
        /// <param name="value">
        /// <see cref="System.Data.Spatial.DbGeography"/> value to be checked.
        /// </param>
        /// <param name="isNullable">
        /// Flag indicating if this property is allowed to be null.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <exception cref="System.Data.ConstraintException">
        /// The value is null for a non-nullable property
        /// </exception>
        protected internal static System.Data.Spatial.DbGeography SetValidValue(System.Data.Spatial.DbGeography value, bool isNullable, string propertyName)
        {
            if (value == null)
            {
                if (!isNullable)
                {
                    EntityUtil.ThrowPropertyIsNotNullable(propertyName);
                }
            }
            return value;
        }


        /// <summary>
        /// Validates that the property is not null, and throws if it is
        /// </summary>
        /// <param name="value">
        /// <see cref="System.Data.Spatial.DbGeography"/> value to be checked.
        /// </param>
        /// <param name="isNullable">
        /// Flag indicating if this property is allowed to be null.
        /// </param>
        /// <exception cref="System.Data.ConstraintException">
        /// The value is null for a non-nullable property
        /// </exception>
        protected internal static System.Data.Spatial.DbGeography SetValidValue(System.Data.Spatial.DbGeography value, bool isNullable)
        {
            return SetValidValue(value, isNullable, null);
        }

        /// <summary>
        /// Validates that the property is not null, and throws if it is
        /// </summary>
        /// <param name="value">
        /// <see cref="System.Data.Spatial.DbGeometry"/> value to be checked.
        /// </param>
        /// <param name="isNullable">
        /// Flag indicating if this property is allowed to be null.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that is being validated.
        /// </param>
        /// <exception cref="System.Data.ConstraintException">
        /// The value is null for a non-nullable property
        /// </exception>
        protected internal static System.Data.Spatial.DbGeometry SetValidValue(System.Data.Spatial.DbGeometry value, bool isNullable, string propertyName)
        {
            if (value == null)
            {
                if (!isNullable)
                {
                    EntityUtil.ThrowPropertyIsNotNullable(propertyName);
                }
            }
            return value;
        }


        /// <summary>
        /// Validates that the property is not null, and throws if it is
        /// </summary>
        /// <param name="value">
        /// <see cref="System.Data.Spatial.DbGeometry"/> value to be checked.
        /// </param>
        /// <param name="isNullable">
        /// Flag indicating if this property is allowed to be null.
        /// </param>
        /// <exception cref="System.Data.ConstraintException">
        /// The value is null for a non-nullable property
        /// </exception>
        protected internal static System.Data.Spatial.DbGeometry SetValidValue(System.Data.Spatial.DbGeometry value, bool isNullable)
        {
            return SetValidValue(value, isNullable, null);
        }

        /// <summary>
        /// Set a whole ComplexObject on an Entity or another ComplexObject
        /// </summary>  
        /// <remarks>
        /// Unlike most of the other SetValidValue methods, this one is not static
        /// because it uses a reference to this in order to set the parent reference for the complex object.
        /// </remarks>
        /// <param name="oldValue">
        /// The current value that is set.
        /// </param>
        /// <param name="newValue">
        /// The new value that will be set.
        /// </param>
        /// <param name="property">
        /// The name of the complex type property that is being set.
        /// </param>        
        /// <returns>
        /// The new value of the complex type property
        /// </returns>
        protected internal T SetValidValue<T>(T oldValue, T newValue, string property) where T : ComplexObject
        {
            // Nullable complex types are not supported in v1, but we allow setting null here if the parent entity is detached
            if (newValue == null && IsChangeTracked)
            {
                throw EntityUtil.NullableComplexTypesNotSupported(property);
            }

            if (oldValue != null)
            {
                oldValue.DetachFromParent();
            }

            if (newValue != null)
            {
                newValue.AttachToParent(this, property);
            }    
            
            return newValue;
        }

        /// <summary>
        /// Helper method used in entity/complex object factory methods to verify that a complex object is not null
        /// </summary>
        /// <typeparam name="TComplex">Type of the complex property</typeparam>
        /// <param name="complexObject">Complex object being verified</param>
        /// <param name="propertyName">Property name associated with this complex object</param>
        /// <returns>the same complex object that was passed in, if an exception didn't occur</returns>
        protected internal static TComplex VerifyComplexObjectIsNotNull<TComplex>(TComplex complexObject, string propertyName) where TComplex : ComplexObject
        {
            if (complexObject == null)
            {
                EntityUtil.ThrowPropertyIsNotNullable(propertyName);
            }
            return complexObject;
        }
        #endregion
    }
}
