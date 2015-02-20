#region Using directives

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Workflow.ComponentModel.Compiler;
using System.Runtime.CompilerServices;
using System.Security.Permissions;

#endregion

namespace System.Workflow.ComponentModel
{
    //

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class DependencyProperty : ISerializable
    {
        private static IDictionary<int, DependencyProperty> dependencyProperties = new Dictionary<int, DependencyProperty>();

        internal enum PropertyValidity
        {
            Uninitialize,
            Reexecute,
            Always
        }

        class KnownDependencyProperty
        {
            internal DependencyProperty dependencyProperty;
            //indicates whether this property, survives beyond Uninitialize.
            internal PropertyValidity propertyValidity;

            internal KnownDependencyProperty(DependencyProperty dependencyProperty, PropertyValidity propertyValidity)
            {
                this.dependencyProperty = dependencyProperty;
                this.propertyValidity = propertyValidity;
            }
        }

        private static KnownDependencyProperty[] knownProperties = new KnownDependencyProperty[256];
        
        private bool isRegistered = false;
        private string name = String.Empty;
        private System.Type propertyType = null;
        private System.Type ownerType = null;
        private System.Type validatorType = null;
        private PropertyMetadata defaultMetadata = null;
        private byte knownIndex = (byte)0;
        [NonSerialized]
        private bool isEvent = false;

        public static DependencyProperty Register(string name, System.Type propertyType, System.Type ownerType)
        {
            return ValidateAndRegister(name, propertyType, ownerType, null, null, true);
        }

        public static DependencyProperty Register(string name, System.Type propertyType, System.Type ownerType, PropertyMetadata defaultMetadata)
        {
            return ValidateAndRegister(name, propertyType, ownerType, defaultMetadata, null, true);
        }

        public static DependencyProperty RegisterAttached(string name, System.Type propertyType, System.Type ownerType)
        {
            return ValidateAndRegister(name, propertyType, ownerType, null, null, false);
        }

        public static DependencyProperty RegisterAttached(string name, System.Type propertyType, System.Type ownerType, PropertyMetadata defaultMetadata)
        {
            return ValidateAndRegister(name, propertyType, ownerType, defaultMetadata, null, false);
        }
        
        internal static void RegisterAsKnown(DependencyProperty dependencyProperty, byte byteVal, PropertyValidity propertyValidity)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");
            if (knownProperties[byteVal] != null)
            {
                throw new InvalidOperationException(SR.GetString(SR.Error_AlreadyRegisteredAs, knownProperties[byteVal].dependencyProperty.ToString()));
            }

            dependencyProperty.KnownIndex = byteVal;
            knownProperties[byteVal] = new KnownDependencyProperty(dependencyProperty, propertyValidity);
        }

        internal static DependencyProperty FromKnown(Byte byteVal)
        {
            if (knownProperties[byteVal] == null)
            {
                throw new InvalidOperationException(SR.GetString(SR.Error_NotRegisteredAs, knownProperties[byteVal].dependencyProperty.ToString()));
            }

            return knownProperties[byteVal].dependencyProperty;
        }

        //TBD: Dharma - Get rid of this overload
        public static DependencyProperty RegisterAttached(string name, System.Type propertyType, System.Type ownerType, PropertyMetadata defaultMetadata, System.Type validatorType)
        {
            if (validatorType == null)
                throw new ArgumentNullException("validatorType");
            else if (!typeof(Validator).IsAssignableFrom(validatorType))
                throw new ArgumentException(SR.GetString(SR.Error_ValidatorTypeIsInvalid), "validatorType");

            return ValidateAndRegister(name, propertyType, ownerType, defaultMetadata, validatorType, false);
        }

        public static DependencyProperty FromName(string propertyName, Type ownerType)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            if (ownerType == null)
                throw new ArgumentNullException("ownerType");

            DependencyProperty dp = null;
            while ((dp == null) && (ownerType != null))
            {
                // Ensure static constructor of type has run
                RuntimeHelpers.RunClassConstructor(ownerType.TypeHandle);

                // Locate property
                int hashCode = propertyName.GetHashCode() ^ ownerType.GetHashCode();
                lock (((ICollection)DependencyProperty.dependencyProperties).SyncRoot)
                {
                    if (DependencyProperty.dependencyProperties.ContainsKey(hashCode))
                        dp = DependencyProperty.dependencyProperties[hashCode];
                }

                ownerType = ownerType.BaseType;
            }

            return dp;
        }

        public static IList<DependencyProperty> FromType(Type ownerType)
        {
            if (ownerType == null)
                throw new ArgumentNullException("ownerType");

            // Ensure static constructor of type has run
            RuntimeHelpers.RunClassConstructor(ownerType.TypeHandle);

            List<DependencyProperty> filteredProperties = new List<DependencyProperty>();
            lock (((ICollection)DependencyProperty.dependencyProperties).SyncRoot)
            {
                foreach (DependencyProperty dependencyProperty in DependencyProperty.dependencyProperties.Values)
                {
                    if (TypeProvider.IsSubclassOf(ownerType, dependencyProperty.ownerType)
                        || ownerType == dependencyProperty.ownerType)
                        filteredProperties.Add(dependencyProperty);
                }
            }

            return filteredProperties.AsReadOnly();
        }

        private static DependencyProperty ValidateAndRegister(string name, System.Type propertyType, System.Type ownerType, PropertyMetadata defaultMetadata, System.Type validatorType, bool isRegistered)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Error_EmptyArgument), "name");

            if (propertyType == null)
                throw new ArgumentNullException("propertyType");

            if (ownerType == null)
                throw new ArgumentNullException("ownerType");

            FieldInfo fieldInfo = null;
            bool isEvent = (typeof(System.Delegate).IsAssignableFrom(propertyType) && (defaultMetadata == null || (defaultMetadata.Options & DependencyPropertyOptions.DelegateProperty) == 0));

            // WinOE Bug 13807: events can not be meta properties.
            if (isEvent && defaultMetadata != null && defaultMetadata.IsMetaProperty)
                throw new ArgumentException(SR.GetString(SR.Error_DPAddHandlerMetaProperty), "defaultMetadata");

            //Field must exists
            if (isEvent)
                fieldInfo = ownerType.GetField(name + "Event", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.GetProperty);
            else
                fieldInfo = ownerType.GetField(name + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.GetProperty);

            if (fieldInfo == null)
            {
                string error = SR.GetString((isEvent) ? SR.Error_DynamicEventNotSupported : SR.Error_DynamicPropertyNotSupported, new object[] { ownerType.FullName, name });
                throw new ArgumentException(error, "ownerType");
            }


            PropertyMetadata metadata = null;
            object defaultValue = null;

            // Establish default metadata for all types, if none is provided
            if (defaultMetadata == null)
            {
                defaultValue = GetDefaultValue(name, propertyType, ownerType);
                metadata = new PropertyMetadata(defaultValue);
            }
            else
            {
                metadata = defaultMetadata;
                if (metadata.DefaultValue == null)
                    metadata.DefaultValue = GetDefaultValue(name, propertyType, ownerType);
            }

            DependencyProperty dependencyProperty = new DependencyProperty(name, propertyType, ownerType, metadata, validatorType, isRegistered);
            lock (((ICollection)DependencyProperty.dependencyProperties).SyncRoot)
            {
                if (DependencyProperty.dependencyProperties.ContainsKey(dependencyProperty.GetHashCode()))
                    throw new InvalidOperationException(SR.GetString(SR.Error_DPAlreadyExist, new object[] { name, ownerType.FullName }));

                DependencyProperty.dependencyProperties.Add(dependencyProperty.GetHashCode(), dependencyProperty);
            }

            return dependencyProperty;
        }

        private static object GetDefaultValue(string name, System.Type propertyType, System.Type ownerType)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Error_EmptyArgument), "name");

            if (propertyType == null)
                throw new ArgumentNullException("propertyType");

            if (ownerType == null)
                throw new ArgumentNullException("ownerType");

            object defaultValue = null;
            if (propertyType.IsValueType)
            {
                try
                {
                    if (propertyType.IsEnum)
                    {
                        Array values = Enum.GetValues(propertyType);
                        if (values.Length > 0)
                            defaultValue = values.GetValue(0);
                        else
                            defaultValue = Activator.CreateInstance(propertyType);
                    }
                    else
                        defaultValue = Activator.CreateInstance(propertyType);
                }
                catch
                {
                }
            }
            return defaultValue;
        }

        private DependencyProperty(string name, System.Type propertyType, System.Type ownerType, PropertyMetadata defaultMetadata, System.Type validatorType, bool isRegistered)
        {
            this.name = name;
            this.propertyType = propertyType;
            this.ownerType = ownerType;
            this.validatorType = validatorType;
            this.isRegistered = isRegistered;
            this.defaultMetadata = defaultMetadata;
            this.defaultMetadata.Seal(this, propertyType);
            this.isEvent = (typeof(System.Delegate).IsAssignableFrom(this.propertyType) && (this.defaultMetadata == null || (this.defaultMetadata.Options & DependencyPropertyOptions.DelegateProperty) == 0));
        }

        public bool IsEvent
        {
            get
            {
                return this.isEvent;
            }
        }

        public bool IsAttached
        {
            get
            {
                return !this.isRegistered;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public System.Type PropertyType
        {
            get
            {
                return this.propertyType;
            }
        }
        public System.Type OwnerType
        {
            get
            {
                return this.ownerType;
            }
        }

        public PropertyMetadata DefaultMetadata
        {
            get
            {
                return this.defaultMetadata;
            }
        }
        public System.Type ValidatorType
        {
            get
            {
                return this.validatorType;
            }
        }
        internal byte KnownIndex
        {
            get
            {
                return this.knownIndex;
            }
            set
            {
                this.knownIndex = value;
            }
        }
        
        internal bool IsKnown
        {
            get
            {
                return (this.knownIndex != 0);
            }
        }

        internal PropertyValidity Validity
        {
            get
            {
                return IsKnown ? knownProperties[this.knownIndex].propertyValidity : PropertyValidity.Always;
            }
        }

        public override string ToString()
        {
            return this.name;
        }

        public override int GetHashCode()
        {
            // 

            Debug.Assert(this.name != null && this.ownerType != null);
            return (this.name.GetHashCode() ^ this.ownerType.GetHashCode());
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("type", this.ownerType);
            info.AddValue("name", this.name);
            info.SetType(typeof(DependencyPropertyReference));
        }
        #endregion

        [Serializable]
        private sealed class DependencyPropertyReference : IObjectReference
        {
            private Type type = null;
            private string name = null;

            public Object GetRealObject(StreamingContext context)
            {
                return DependencyProperty.FromName(this.name, this.type);
            }
        }
    }
}
