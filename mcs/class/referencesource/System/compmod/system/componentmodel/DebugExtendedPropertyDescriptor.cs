//------------------------------------------------------------------------------
// <copyright file="DebugExtendedPropertyDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

#if DEBUG

/*
    
    This class exists in debug only.  It is a complete copy of the
    V1.0 TypeDescriptor object and is used to validate that the
    behavior of the V2.0 TypeDescriptor matches 1.0 behavior.
    
 */
namespace System.ComponentModel {
    

    using System.Diagnostics;

    using System;
    using System.ComponentModel.Design;
    using System.Collections;
    using Microsoft.Win32;
    using System.Security.Permissions;

    /// <internalonly/>
    /// <devdoc>
    ///    <para>
    ///       This class wraps an PropertyDescriptor with something that looks like a property. It
    ///       allows you to treat extended properties the same as regular properties.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    internal sealed class DebugExtendedPropertyDescriptor : PropertyDescriptor {

        private readonly DebugReflectPropertyDescriptor      extenderInfo;       // the extender property
        private readonly IExtenderProvider provider;           // the guy providing it
        private TypeConverter converter;
        private object[]      editors;
        private Type[]        editorTypes;
        private int           editorCount;

        /// <devdoc>
        ///     Creates a new extended property info.  Callers can then treat this as
        ///     a standard property.
        /// </devdoc>
        public DebugExtendedPropertyDescriptor(DebugReflectPropertyDescriptor extenderInfo, Type receiverType, IExtenderProvider provider) : this(extenderInfo, receiverType, provider, null) {
        }

        /// <devdoc>
        ///     Creates a new extended property info.  Callers can then treat this as
        ///     a standard property.
        /// </devdoc>
        public DebugExtendedPropertyDescriptor(DebugReflectPropertyDescriptor extenderInfo, Type receiverType, IExtenderProvider provider, Attribute[] attributes)
            : base(extenderInfo, attributes) {

            Debug.Assert(extenderInfo != null, "DebugExtendedPropertyDescriptor must have extenderInfo");
            Debug.Assert(provider != null, "DebugExtendedPropertyDescriptor must have provider");

            ArrayList attrList = new ArrayList(AttributeArray);
            attrList.Add(ExtenderProvidedPropertyAttribute.Create(extenderInfo, receiverType, provider));
            if (extenderInfo.IsReadOnly) {
                attrList.Add(ReadOnlyAttribute.Yes);
            }
            
            Attribute[] temp = new Attribute[attrList.Count];
            attrList.CopyTo(temp, 0);
            AttributeArray = temp;

            this.extenderInfo = extenderInfo;
            this.provider = provider;
        }

        public DebugExtendedPropertyDescriptor(PropertyDescriptor extender,  Attribute[] attributes) : base(extender, attributes) {
            Debug.Assert(extender != null, "The original PropertyDescriptor must be non-null");
            
            ExtenderProvidedPropertyAttribute attr = extender.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;

            Debug.Assert(attr != null, "The original PropertyDescriptor does not have an ExtenderProvidedPropertyAttribute");

            
            DebugReflectPropertyDescriptor reflectDesc = attr.ExtenderProperty as DebugReflectPropertyDescriptor;

            Debug.Assert(reflectDesc != null, "The original PropertyDescriptor has an invalid ExtenderProperty");

            this.extenderInfo = reflectDesc;
            this.provider = attr.Provider;
        }

        /// <devdoc>
        ///     Determines if the the component will allow its value to be reset.
        /// </devdoc>
        public override bool CanResetValue(object comp) {
            return extenderInfo.ExtenderCanResetValue(provider, comp);
        }

        /// <devdoc>
        ///     Retrieves the type of the component this PropertyDescriptor is bound to.
        /// </devdoc>
        public override Type ComponentType {
            get {
                return extenderInfo.ComponentType;
            }
        }
        
        public override TypeConverter Converter {
            get {
                if (converter == null) {
                    TypeConverterAttribute attr = (TypeConverterAttribute)Attributes[typeof(TypeConverterAttribute)];
                    if (attr.ConverterTypeName != null && attr.ConverterTypeName.Length > 0) {
                        Type converterType = GetTypeFromName(attr.ConverterTypeName);
                        if (converterType != null && typeof(TypeConverter).IsAssignableFrom(converterType)) {
                            converter = (TypeConverter)CreateInstance(converterType);
                        }
                    }

                    if (converter == null) {
                        converter = DebugTypeDescriptor.GetConverter(PropertyType);
                    }
                }
                return converter;
            }
        }

        /// <devdoc>
        ///     Determines if the property can be written to.
        /// </devdoc>
        public override bool IsReadOnly {
            get {
                return Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);
            }
        }

        /// <devdoc>
        ///     Retrieves the data type of the property.
        /// </devdoc>
        public override Type PropertyType {
            get {
                return extenderInfo.ExtenderGetType(provider);
            }
        }

        /// <devdoc>
        ///     Retrieves the display name of the property.  This is the name that will
        ///     be displayed in a properties window.  This will be the same as the property
        ///     name for most properties.
        /// </devdoc>
        public override string DisplayName {
            get {
                string name = Name;
                ISite site = GetSite(provider);
                if (site != null) {
                    string providerName = site.Name;
                    if (providerName != null && providerName.Length > 0) {
                        name = SR.GetString(SR.MetaExtenderName, name, providerName);
                    }
                }
                return name;
            }
        }

        /// <devdoc>
        ///    Retrieves the properties 
        /// </devdoc>
        public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter) {
            if (instance == null) {
                return DebugTypeDescriptor.GetProperties(PropertyType, filter);
            }
            else {
                return DebugTypeDescriptor.GetProperties(instance, filter);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets an editor of the specified type.
        ///    </para>
        /// </devdoc>
        public override object GetEditor(Type editorBaseType) {
            object editor = null;

            // Check the editors we've already created for this type.
            //
            if (editorTypes != null) {
                for (int i = 0; i < editorCount; i++) {
                    if (editorTypes[i] == editorBaseType) {
                        return editors[i];
                    }
                }
            }

            // If one wasn't found, then we must go through the attributes.
            //
            if (editor == null) {
                for (int i = 0; i < Attributes.Count; i++) {

                    if (!(Attributes[i] is EditorAttribute)) {
                        continue;
                    }

                    EditorAttribute attr = (EditorAttribute)Attributes[i];
                    Type editorType = GetTypeFromName(attr.EditorBaseTypeName);

                    if (editorBaseType == editorType) {
                        Type type = GetTypeFromName(attr.EditorTypeName);
                        if (type != null) {
                            editor = CreateInstance(type);
                            break;
                        }
                    }
                }
                
                // Now, if we failed to find it in our own attributes, go to the
                // component descriptor.
                //
                if (editor == null) {
                    editor = DebugTypeDescriptor.GetEditor(PropertyType, editorBaseType);
                }
                
                // Now, another slot in our editor cache for next time
                //
                if (editorTypes == null) {
                    editorTypes = new Type[5];
                    editors = new object[5];
                }

                if (editorCount >= editorTypes.Length) {
                    Type[] newTypes = new Type[editorTypes.Length * 2];
                    object[] newEditors = new object[editors.Length * 2];
                    Array.Copy(editorTypes, newTypes, editorTypes.Length);
                    Array.Copy(editors, newEditors, editors.Length);
                    editorTypes = newTypes;
                    editors = newEditors;
                }

                editorTypes[editorCount] = editorBaseType;
                editors[editorCount++] = editor;
            }

            return editor;
        }

        /// <devdoc>
        ///     Retrieves the value of the property for the given component.  This will
        ///     throw an exception if the component does not have this property.
        /// </devdoc>
        public override object GetValue(object comp) {
            return extenderInfo.ExtenderGetValue(provider, comp);
        }

        /// <devdoc>
        ///     Resets the value of this property on comp to the default value.
        /// </devdoc>
        public override void ResetValue(object comp) {
            extenderInfo.ExtenderResetValue(provider, comp, this);
        }

        /// <devdoc>
        ///     Sets the value of this property on the given component.
        /// </devdoc>
        public override void SetValue(object component, object value) {
            extenderInfo.ExtenderSetValue(provider, component, value, this);
        }

        /// <devdoc>
        ///     Determines if this property should be persisted.  A property is
        ///     to be persisted if it is marked as persistable through a
        ///     PersistableAttribute, and if the property contains something other
        ///     than the default value.  Note, however, that this method will
        ///     return true for design time properties as well, so callers
        ///     should also check to see if a property is design time only before
        ///     persisting to runtime storage.
        /// </devdoc>
        public override bool ShouldSerializeValue(object comp) {
            return extenderInfo.ExtenderShouldSerializeValue(provider, comp);
        }
    }
}
#endif
