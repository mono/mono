//------------------------------------------------------------------------------
// <copyright file="SettingsPropertyValue.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using  System.Collections;
    using  System.Collections.Specialized;
    using  System.Runtime.Serialization;
    using  System.Configuration.Provider;
    using  System.Globalization;
    using  System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Reflection;
    using System.Runtime.Versioning;

    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    public class SettingsPropertyValue
    {
        public string Name                  { get { return _Property.Name; } }
        public bool   IsDirty               { get { return _IsDirty; } set { _IsDirty = value; }}
        public SettingsProperty Property    { get { return _Property; } }

        public bool UsingDefaultValue { get { return _UsingDefaultValue; } }

        public SettingsPropertyValue(SettingsProperty property)
        {
            _Property = property;
        }

        public object PropertyValue
        {
            get
            {
                if (!_Deserialized)
                {
                    _Value = Deserialize();
                    _Deserialized = true;
                }

                if (_Value != null && !Property.PropertyType.IsPrimitive && !(_Value is string) && !(_Value is DateTime))
                {
                    _UsingDefaultValue = false;
                    _ChangedSinceLastSerialized = true;
                    _IsDirty = true;
                }

                return _Value;
            }
            set
            {
                _Value = value;
                _IsDirty = true;
                _ChangedSinceLastSerialized = true;
                _Deserialized = true;
                _UsingDefaultValue = false;
            }
        }

        public object SerializedValue
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
            get {
                if (_ChangedSinceLastSerialized) {
                    _ChangedSinceLastSerialized = false;
                    _SerializedValue = SerializePropertyValue();
                }
                return _SerializedValue;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
            set {
                _UsingDefaultValue = false;
                _SerializedValue = value;
            }
        }

        public bool Deserialized
        {
            get { return _Deserialized; }
            set { _Deserialized = value; }
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.AppDomain, ResourceScope.AppDomain)]
        private bool IsHostedInAspnet() {
            // See System.Web.Hosting.ApplicationManager::PopulateDomainBindings
            return AppDomain.CurrentDomain.GetData(".appDomain") != null;
        }

        private object Deserialize()
        {
            object val = null;
            //////////////////////////////////////////////
            /// Step 1: Try creating from Serailized value
            if (SerializedValue != null)
            {
                try {
                    if (SerializedValue is string) {
                        val = GetObjectFromString(Property.PropertyType, Property.SerializeAs, (string)SerializedValue);
                    } else {
                        MemoryStream ms = new System.IO.MemoryStream((byte[])SerializedValue);
                        try {
                            val = (new BinaryFormatter()).Deserialize(ms);
                        } finally {
                            ms.Close();
                        }
                    }
                } 
                catch (Exception exception) { 
                    try {
                        if (IsHostedInAspnet()) {
                            object[]    args = new object[] { Property, this, exception};

                            const string webBaseEventTypeName = "System.Web.Management.WebBaseEvent, " +  AssemblyRef.SystemWeb;
                            
                            Type type = Type.GetType(webBaseEventTypeName, true);
                            
                            type.InvokeMember("RaisePropertyDeserializationWebErrorEvent",
                                BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.InvokeMethod, 
                                null, null, args, CultureInfo.InvariantCulture);
                        }
                    }
                    catch {
                    }
                }

               if (val != null && !Property.PropertyType.IsAssignableFrom(val.GetType())) // is it the correct type
                    val = null;
            }

            //////////////////////////////////////////////
            /// Step 2: Try creating from default value
            if (val == null)
            {
                _UsingDefaultValue = true;
                if (Property.DefaultValue == null || Property.DefaultValue.ToString() == "[null]") {
                    if (Property.PropertyType.IsValueType)
                        return SecurityUtils.SecureCreateInstance(Property.PropertyType);
                    else
                        return null;
                }
                if (!(Property.DefaultValue is string)) {
                    val = Property.DefaultValue;
                } else {
                    try {
                        val = GetObjectFromString(Property.PropertyType, Property.SerializeAs, (string)Property.DefaultValue);
                    } catch(Exception e) {
                        throw new ArgumentException(SR.GetString(SR.Could_not_create_from_default_value, Property.Name, e.Message));
                    }
                }
                if (val != null && !Property.PropertyType.IsAssignableFrom(val.GetType())) // is it the correct type
                    throw new ArgumentException(SR.GetString(SR.Could_not_create_from_default_value_2, Property.Name));
            }

            //////////////////////////////////////////////
            /// Step 3: Create a new one by calling the parameterless constructor
            if (val == null)
            {
                if (Property.PropertyType == typeof(string)) {
                    val = "";
                } else {
                    try {
                        val = SecurityUtils.SecureCreateInstance(Property.PropertyType);
                    } catch {}
                }
            }

            return val;
        }

        private static object GetObjectFromString(Type type, SettingsSerializeAs serializeAs, string attValue)
        {
            // Deal with string types
            if (type == typeof(string) && (attValue == null || attValue.Length < 1 || serializeAs == SettingsSerializeAs.String))
                return attValue;

            // Return null if there is nothing to convert
            if (attValue == null || attValue.Length < 1)
                return null;

            // Convert based on the serialized type
            switch (serializeAs)
            {
                case SettingsSerializeAs.Binary:
                    byte[]          buf = Convert.FromBase64String(attValue);
                    MemoryStream    ms  = null;
                    try {
                        ms = new System.IO.MemoryStream(buf);
                        return (new BinaryFormatter()).Deserialize(ms);
                    } finally {
                        if (ms != null)
                            ms.Close();
                    }

                case SettingsSerializeAs.Xml:
                    StringReader    sr = new StringReader(attValue);
                    XmlSerializer   xs = new XmlSerializer(type);
                    return xs.Deserialize(sr);

                case SettingsSerializeAs.String:
                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    if (converter != null && converter.CanConvertTo(typeof(String)) && converter.CanConvertFrom(typeof(String)))
                        return converter.ConvertFromInvariantString(attValue);
                    throw new ArgumentException(SR.GetString(SR.Unable_to_convert_type_from_string, type.ToString()), "type");

                default:
                    return null;
            }
        }

        private object SerializePropertyValue()
        {
            if (_Value == null)
                return null;

            if (Property.SerializeAs != SettingsSerializeAs.Binary)
                return ConvertObjectToString(_Value, Property.PropertyType, Property.SerializeAs, Property.ThrowOnErrorSerializing);

            MemoryStream ms = new System.IO.MemoryStream();
            try {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, _Value);
                return ms.ToArray();
            } finally {
                ms.Close();
            }
        }


        private static string ConvertObjectToString(object propValue, Type type, SettingsSerializeAs serializeAs, bool throwOnError)
        {
            if (serializeAs == SettingsSerializeAs.ProviderSpecific) {
                if (type == typeof(string) || type.IsPrimitive)
                    serializeAs = SettingsSerializeAs.String;
                else
                    serializeAs = SettingsSerializeAs.Xml;
            }

            try {
                switch (serializeAs) {
                case SettingsSerializeAs.String:
                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    if (converter != null && converter.CanConvertTo(typeof(String)) && converter.CanConvertFrom(typeof(String)))
                        return converter.ConvertToInvariantString(propValue);
                    throw new ArgumentException(SR.GetString(SR.Unable_to_convert_type_to_string, type.ToString()), "type");
                case SettingsSerializeAs.Binary :
                    MemoryStream ms = new System.IO.MemoryStream();
                    try {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, propValue);
                        byte[] buffer = ms.ToArray();
                        return Convert.ToBase64String(buffer);
                    } finally {
                        ms.Close();
                    }

                case SettingsSerializeAs.Xml :
                    XmlSerializer xs = new XmlSerializer(type);
                    StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);

                    xs.Serialize(sw, propValue);
                    return sw.ToString();
                }
            } catch (Exception) {
                if (throwOnError)
                    throw;
            }
            return null;
        }

        private object  _Value              = null;
        private object  _SerializedValue    = null;
        private bool    _Deserialized       = false;
        private bool    _IsDirty            = false;
        private SettingsProperty _Property  = null;
        private bool    _ChangedSinceLastSerialized = false;
        private bool _UsingDefaultValue = true;
    }
}
