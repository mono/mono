// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Contents:  Service for providing and finding custom serialization for
//             value and value like types.
//
//  Created:   04/28/2005 Microsoft
//

using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using MS.Internal.Serialization;
using System.Xaml.Replacements; //DateTimeConverter2
using System.Xaml;
using System.Runtime.CompilerServices; //SRID

#pragma warning disable 1634, 1691  // suppressing PreSharp warnings

namespace System.Windows.Markup
{
    /// <summary>
    /// ValueSerializer allows a type to declare a serializer to control how the type is serializer to and from string 
    /// If a TypeConverter is declared for a type that converts to and from a string, a default value serializer will 
    /// be created for the type. The string values must be loss-less (i.e. converting to and from a string doesn't loose 
    /// data) and must be stable (i.e. returns the same string for the same value). If a type converter doesn't  meet 
    /// these requirements, a custom ValueSerializer must be declared that meet the requirements or associate a null 
    /// ValueSerializer with the type to indicate the type converter should be ignored. Implementation of ValueSerializer 
    /// should avoid throwing exceptions. Any exceptions thrown could possibly terminate serialization.
    /// </summary>
    /// 
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class ValueSerializer
    {
        /// <summary>
        /// Constructor for a ValueSerializer
        /// </summary>
        protected ValueSerializer() { }

        /// <summary>
        /// Returns true if the given value can be converted to a string.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="context">Context information</param>
        /// <returns>Whether or not the value can be converted to a string</returns>
        public virtual bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the given value can be converted from a string.
        /// </summary>
        /// <param name="value">The string to convert</param>
        /// <param name="context">Context information</param>
        /// <returns>Whether or not the value can be converted from a string</returns>
        public virtual bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return false;
        }

        /// <summary>
        /// Converts the given value to a string for use in serialization. This method should only be
        /// called if CanConvertToString returns true for the given value.
        /// </summary>
        /// <param name="value">The value to convert to a string</param>
        /// <param name="context">Context information</param>
        /// <returns>A string representation of value</returns>
        public virtual string ConvertToString(object value, IValueSerializerContext context)
        {
            throw GetConvertToException(value, typeof(string));
        }

        /// <summary>
        /// Convert a string to an object. This method should only be called if CanConvertFromString
        /// returns true for the given string.
        /// </summary>
        /// <param name="value">The string value to convert</param>
        /// <param name="context">Context information</param>
        /// <returns>An object corresponding to the string value</returns>
        public virtual object ConvertFromString(string value, IValueSerializerContext context)
        {
            throw GetConvertFromException(value);
        }

        static List<Type> Empty = new List<Type>();

        /// <summary>
        /// Returns an enumeration of the types referenced by the value serializer. If the value serializer asks for
        /// a value serializer for System.Type, any types it asks to convert should be supplied in the returned
        /// enumeration. This allows a serializer to ensure a de-serializer has enough information about the types
        /// this serializer converts.
        /// 
        /// Since a value serializer doesn't exist by default, it is important the value serializer be requested from
        /// the IValueSerializerContext, not ValueSerializer.GetSerializerFor. This allows a serializer to encode
        /// context information (such as xmlns definitions) to the System.Type converter (for example, which prefix
        /// to generate).
        /// </summary>
        /// <param name="value">The value being serialized</param>
        /// <param name="context">Context information</param>
        /// <returns>An enumeration of the types converted by this serializer</returns>
        public virtual IEnumerable<Type> TypeReferences(object value, IValueSerializerContext context)
        {
            return Empty;
        }

        /// <summary>
        /// Get the value serializer declared for the given type.
        /// </summary>
        /// <param name="type">The value type to serialize</param>
        /// <returns>The value serializer associated with the given type</returns>
        public static ValueSerializer GetSerializerFor(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            object value = _valueSerializers[type];
            if (value != null)
                // This uses _valueSerializersLock's instance as a sentinal for null  (as opposed to not attempted yet).
                return value == _valueSerializersLock ? null : value as ValueSerializer;

            AttributeCollection attributes = TypeDescriptor.GetAttributes(type);
            ValueSerializerAttribute attribute = attributes[typeof(ValueSerializerAttribute)] as ValueSerializerAttribute;
            ValueSerializer result = null;

            if (attribute != null)
                result = (ValueSerializer)Activator.CreateInstance(attribute.ValueSerializerType);

            if (result == null)
            {
                if (type == typeof(string))
                {
                    result = new StringValueSerializer();
                }
                else
                {
                    // Try to use the type converter
                    TypeConverter converter = TypeConverterHelper.GetTypeConverter(type);

                    // DateTime is a special-case.  We can't use the DateTimeConverter, because it doesn't
                    // support anything other than user culture and invariant culture, and we need to specify
                    // en-us culture.
                    if (converter.GetType() == typeof(DateTimeConverter2))
                    {
                        result = new DateTimeValueSerializer();
                    }
                    else if (converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)) &&
                             !(converter is ReferenceConverter))
                    {
                        result = new TypeConverterValueSerializer(converter);
                    }
                }
            }
            lock (_valueSerializersLock)
            {
                // This uses _valueSerializersLock's instance as a sentinal for null (as opposed to not attempted yet).
                _valueSerializers[type] = result == null ? _valueSerializersLock : result;
            }

            return result;
        }

        /// <summary>
        /// Get the value serializer declared for the given property. ValueSerializer can be overriden by an attribute
        /// on the property declaration.
        /// </summary>
        /// <param name="descriptor">PropertyDescriptor for the property to be serialized</param>
        /// <returns>A value serializer associated with the given property</returns>
        public static ValueSerializer GetSerializerFor(PropertyDescriptor descriptor)
        {
            ValueSerializer result;
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            
            #pragma warning suppress 6506 // descriptor is obviously not null
            ValueSerializerAttribute serializerAttribute = descriptor.Attributes[typeof(ValueSerializerAttribute)] as ValueSerializerAttribute;
            if (serializerAttribute != null)
            {
                result = (ValueSerializer)Activator.CreateInstance(serializerAttribute.ValueSerializerType);
            }
            else
            {
                result = GetSerializerFor(descriptor.PropertyType);
                if (result == null || result is TypeConverterValueSerializer)
                {
                    TypeConverter converter = descriptor.Converter;
                    if (converter!=null && converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)) &&
                        !(converter is ReferenceConverter))
                        result = new TypeConverterValueSerializer(converter);
                }
            }
            return result;
        }

        /// <summary>
        /// Get the value serializer declared for the given type. This version should be called whenever the caller
        /// has a IValueSerializerContext to ensure that the correct value serializer is returned for the given
        /// context.
        /// </summary>
        /// <param name="type">The value type to serialize</param>
        /// <param name="context">Context information</param>
        /// <returns>The value serializer associated with the given type</returns>
        public static ValueSerializer GetSerializerFor(Type type, IValueSerializerContext context)
        {
            if (context != null)
            {
                ValueSerializer result = context.GetValueSerializerFor(type);
                if (result != null)
                    return result;
            }
            return GetSerializerFor(type);
        }

        /// <summary>
        /// Get the value serializer declared for the given property. ValueSerializer can be overriden by an attribute
        /// on the property declaration. This version should be called whenever the caller has a 
        /// IValueSerializerContext to ensure that the correct value serializer is returned for the given context.
        /// </summary>
        /// <param name="descriptor">PropertyDescriptor for the property to be serialized</param>
        /// <param name="context">Context information</param>
        /// <returns>A value serializer associated with the given property</returns>
        public static ValueSerializer GetSerializerFor(PropertyDescriptor descriptor, IValueSerializerContext context)
        {
            if (context != null)
            {
                ValueSerializer result = context.GetValueSerializerFor(descriptor);
                if (result != null)
                    return result;
            }
            return GetSerializerFor(descriptor);
        }

        /// <summary>
        /// Return a exception to throw if the value cannot be converted
        /// </summary>
        protected Exception GetConvertToException(object value, Type destinationType)
        {
            string text;
            if (value == null)
            {
                text = SR.Get(SRID.ToStringNull);
            }
            else
            {
                text = value.GetType().FullName;
            }
            return new NotSupportedException(SR.Get(SRID.ConvertToException, base.GetType().Name, text, destinationType.FullName));
        }

        /// <summary>
        /// Return a exception to throw if the string cannot be converted
        /// </summary>
        protected Exception GetConvertFromException(object value)
        {
            string text;
            if (value == null)
            {
                text = SR.Get(SRID.ToStringNull);
            }
            else
            {
                text = value.GetType().FullName;
            }
            return new NotSupportedException(SR.Get(SRID.ConvertFromException, base.GetType().Name, text));
        }

        private static void TypeDescriptorRefreshed(RefreshEventArgs args) {
            _valueSerializers = new Hashtable();
        }

        static ValueSerializer() {
            TypeDescriptor.Refreshed += TypeDescriptorRefreshed;
        }

        private static object _valueSerializersLock = new object();
        private static Hashtable _valueSerializers = new Hashtable();
    }
}
