//------------------------------------------------------------------------------
// <copyright file="PropertyDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2113:SecureLateBindingMethods", Scope="member", Target="System.ComponentModel.PropertyDescriptor.GetTypeFromName(System.String):System.Type")]

namespace System.ComponentModel {

    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Provides a description of a property.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public abstract class PropertyDescriptor : MemberDescriptor {
        private TypeConverter converter = null;
        private Hashtable     valueChangedHandlers;
        private object[]      editors;
        private Type[]        editorTypes;
        private int           editorCount;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.PropertyDescriptor'/> class with the specified name and
        ///       attributes.
        ///    </para>
        /// </devdoc>
        protected PropertyDescriptor(string name, Attribute[] attrs)
        : base(name, attrs) {
        }
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.PropertyDescriptor'/> class with
        ///       the name and attributes in the specified <see cref='System.ComponentModel.MemberDescriptor'/>.
        ///    </para>
        /// </devdoc>
        protected PropertyDescriptor(MemberDescriptor descr)
        : base(descr) {
        }
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.PropertyDescriptor'/> class with
        ///       the name in the specified <see cref='System.ComponentModel.MemberDescriptor'/> and the
        ///       attributes in both the <see cref='System.ComponentModel.MemberDescriptor'/> and the
        ///    <see cref='System.Attribute'/> array. 
        ///    </para>
        /// </devdoc>
        protected PropertyDescriptor(MemberDescriptor descr, Attribute[] attrs)
        : base(descr, attrs) {
        }

        /// <devdoc>
        ///    <para>
        ///       When overridden in a derived class, gets the type of the
        ///       component this property
        ///       is bound to.
        ///    </para>
        /// </devdoc>
        public abstract Type ComponentType {get;}

        /// <devdoc>
        ///    <para>
        ///       Gets the type converter for this property.
        ///    </para>
        /// </devdoc>
        public virtual TypeConverter Converter {
            get {
                // Always grab the attribute collection first here, because if the metadata version
                // changes it will invalidate our type converter cache.
                AttributeCollection attrs = Attributes;

                if (converter == null) {
                    TypeConverterAttribute attr = (TypeConverterAttribute)attrs[typeof(TypeConverterAttribute)];
                    if (attr.ConverterTypeName != null && attr.ConverterTypeName.Length > 0) {
                        Type converterType = GetTypeFromName(attr.ConverterTypeName);
                        if (converterType != null && typeof(TypeConverter).IsAssignableFrom(converterType)) 
                        {
                            converter = (TypeConverter)CreateInstance(converterType);
                        }
                    }

                    if (converter == null) {
                        converter = TypeDescriptor.GetConverter(PropertyType);
                    }
                }
                return converter;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value
        ///       indicating whether this property should be localized, as
        ///       specified in the <see cref='System.ComponentModel.LocalizableAttribute'/>.
        ///    </para>
        /// </devdoc>
        public virtual bool IsLocalizable {
            get {
                return(LocalizableAttribute.Yes.Equals(Attributes[typeof(LocalizableAttribute)]));
            }
        }

        /// <devdoc>
        ///    <para>
        ///       When overridden in
        ///       a derived class, gets a value
        ///       indicating whether this property is read-only.
        ///    </para>
        /// </devdoc>
        public abstract bool IsReadOnly { get;}

        /// <devdoc>
        ///    <para>
        ///       Gets a value
        ///       indicating whether this property should be serialized as specified in the <see cref='System.ComponentModel.DesignerSerializationVisibilityAttribute'/>.
        ///    </para>
        /// </devdoc>
        public DesignerSerializationVisibility SerializationVisibility {
            get {
                DesignerSerializationVisibilityAttribute attr = (DesignerSerializationVisibilityAttribute)Attributes[typeof(DesignerSerializationVisibilityAttribute)];
                return attr.Visibility;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       When overridden in a derived class,
        ///       gets the type of the property.
        ///    </para>
        /// </devdoc>
        public abstract Type PropertyType { get;}

        /// <devdoc>
        ///     Allows interested objects to be notified when this property changes.
        /// </devdoc>
        public virtual void AddValueChanged(object component, EventHandler handler) {
            if (component == null) throw new ArgumentNullException("component");
            if (handler == null) throw new ArgumentNullException("handler");
            
            if (valueChangedHandlers == null) {
                valueChangedHandlers = new Hashtable();
            }
            
            EventHandler h = (EventHandler)valueChangedHandlers[component];
            valueChangedHandlers[component] = Delegate.Combine(h, handler);
        }
        
        /// <devdoc>
        ///    <para>
        ///       When overridden in a derived class, indicates whether
        ///       resetting the <paramref name="component "/>will change the value of the
        ///    <paramref name="component"/>.
        /// </para>
        /// </devdoc>
        public abstract bool CanResetValue(object component);

        /// <devdoc>
        ///    <para>
        ///       Compares this to another <see cref='System.ComponentModel.PropertyDescriptor'/>
        ///       to see if they are equivalent.
        ///       NOTE: If you make a change here, you likely need to change GetHashCode() as well.
        ///    </para>
        /// </devdoc>
        public override bool Equals(object obj) {
            try {
                if (obj == this) {
                    return true;
                }
                
                if (obj == null) {
                    return false;
                }
                
                // Assume that 90% of the time we will only do a .Equals(...) for
                // propertydescriptor vs. propertydescriptor... avoid the overhead
                // of an instanceof call.
                PropertyDescriptor pd = obj as PropertyDescriptor;

                if (pd != null && pd.NameHashCode == this.NameHashCode
                    && pd.PropertyType == this.PropertyType
                    && pd.Name.Equals(this.Name)) {
    
                    return true;
                }
            }
            catch {}

            return false;
        }

        /// <devdoc>
        ///    <para>
        ///       Creates an instance of the
        ///       specified type.
        ///    </para>
        /// </devdoc>
        protected object CreateInstance(Type type) {
            Type[] typeArgs = new Type[] {typeof(Type)};
            ConstructorInfo ctor = type.GetConstructor(typeArgs);
            if (ctor != null) {
                return TypeDescriptor.CreateInstance(null, type, typeArgs, new object[] {PropertyType});
            }
            
            return TypeDescriptor.CreateInstance(null, type, null, null);
        }

        /// <devdoc>
        ///       In an inheriting class, adds the attributes of the inheriting class to the
        ///       specified list of attributes in the parent class.  For duplicate attributes,
        ///       the last one added to the list will be kept.
        /// </devdoc>
        protected override void FillAttributes(IList attributeList) {

            // Each time we fill our attributes, we should clear our cached
            // stuff.
            converter = null;
            editors = null;
            editorTypes = null;
            editorCount = 0;

            base.FillAttributes(attributeList);
        }

        /// <include file='doc\PropertyDescriptor.uex' path='docs/doc[@for="PropertyDescriptor.GetChildProperties"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PropertyDescriptorCollection GetChildProperties() {
            return GetChildProperties(null, null);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PropertyDescriptorCollection GetChildProperties(Attribute[] filter) {
            return GetChildProperties(null, filter);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PropertyDescriptorCollection GetChildProperties(object instance) {
            return GetChildProperties(instance, null);
        }
        
        /// <devdoc>
        ///    Retrieves the properties 
        /// </devdoc>
        public virtual PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter) {
            if (instance == null) {
                return TypeDescriptor.GetProperties(PropertyType, filter);
            }
            else {
                return TypeDescriptor.GetProperties(instance, filter);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets an editor of the specified type.
        ///    </para>
        /// </devdoc>
        public virtual object GetEditor(Type editorBaseType) {
            object editor = null;

            // Always grab the attribute collection first here, because if the metadata version
            // changes it will invalidate our editor cache.
            AttributeCollection attrs = Attributes;

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
                for (int i = 0; i < attrs.Count; i++) {
                    EditorAttribute attr = attrs[i] as EditorAttribute;
                    if (attr == null) {
                        continue;
                    }

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
                    editor = TypeDescriptor.GetEditor(PropertyType, editorBaseType);
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
        ///     Try to keep this reasonable in sync with Equals(). Specifically, 
        ///     if A.Equals(B) returns true, A & B should have the same hash code.
        /// </devdoc>
        public override int GetHashCode() {
            return this.NameHashCode ^ PropertyType.GetHashCode();
        }

        /// <devdoc>
        ///     This method returns the object that should be used during invocation of members.
        ///     Normally the return value will be the same as the instance passed in.  If
        ///     someone associated another object with this instance, or if the instance is a
        ///     custom type descriptor, GetInvocationTarget may return a different value.
        /// </devdoc>
        protected override object GetInvocationTarget(Type type, object instance) {
            object target = base.GetInvocationTarget(type, instance);
            ICustomTypeDescriptor td = target as ICustomTypeDescriptor;
            if (td != null) {
                target = td.GetPropertyOwner(this);
            }

            return target;
        }
       
        /// <devdoc>
        ///    <para>Gets a type using its name.</para>
        /// </devdoc>
        protected Type GetTypeFromName(string typeName) {

            if (typeName == null || typeName.Length == 0) {
                return null;
            }

            //  try the generic method.
            Type typeFromGetType = Type.GetType(typeName);

            // If we didn't get a type from the generic method, or if the assembly we found the type
            // in is the same as our Component's assembly, use or Component's assembly instead.  This is
            // because the CLR may have cached an older version if the assembly's version number didn't change
            // See VSWhidbey 560732
            Type typeFromComponent = null;
            if (ComponentType != null) {
                if ((typeFromGetType == null) ||
                    (ComponentType.Assembly.FullName.Equals(typeFromGetType.Assembly.FullName))) {

                    int comma = typeName.IndexOf(',');

                    if (comma != -1)
                        typeName = typeName.Substring(0, comma);

                    typeFromComponent = ComponentType.Assembly.GetType(typeName);
                }
            }

            return typeFromComponent ?? typeFromGetType;
        }

        /// <devdoc>
        ///    <para>
        ///       When overridden in a derived class, gets the current
        ///       value
        ///       of the
        ///       property on a component.
        ///    </para>
        /// </devdoc>
        public abstract object GetValue(object component);

        /// <devdoc>
        ///     This should be called by your property descriptor implementation
        ///     when the property value has changed.
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected virtual void OnValueChanged(object component, EventArgs e) {
            if (component != null && valueChangedHandlers != null) {
                EventHandler handler = (EventHandler)valueChangedHandlers[component];
                if (handler != null) {
                    handler(component, e);
                }
            }
        }

        /// <devdoc>
        ///     Allows interested objects to be notified when this property changes.
        /// </devdoc>
        public virtual void RemoveValueChanged(object component, EventHandler handler) {
            if (component == null) throw new ArgumentNullException("component");
            if (handler == null) throw new ArgumentNullException("handler");
            
            if (valueChangedHandlers != null) {
                EventHandler h = (EventHandler)valueChangedHandlers[component];
                h = (EventHandler)Delegate.Remove(h, handler);
                if (h != null) {
                    valueChangedHandlers[component] = h;
                }
                else {
                    valueChangedHandlers.Remove(component);
                }
            }
        }
        
        /// <devdoc>
        ///     Return current set of ValueChanged event handlers for a specific
        ///     component, in the form of a combined multicast event handler.
        ///     Returns null if no event handlers currently assigned to component.
        /// </devdoc>
        internal protected EventHandler GetValueChangedHandler(object component) {
            if (component != null && valueChangedHandlers != null) {
                return (EventHandler) valueChangedHandlers[component];
            }
            else {
                return null;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       When overridden in a derived class, resets the
        ///       value
        ///       for this property
        ///       of the component.
        ///    </para>
        /// </devdoc>
        public abstract void ResetValue(object component);

        /// <devdoc>
        ///    <para>
        ///       When overridden in a derived class, sets the value of
        ///       the component to a different value.
        ///    </para>
        /// </devdoc>
        public abstract void SetValue(object component, object value);

        /// <devdoc>
        ///    <para>
        ///       When overridden in a derived class, indicates whether the
        ///       value of
        ///       this property needs to be persisted.
        ///    </para>
        /// </devdoc>
        public abstract bool ShouldSerializeValue(object component);

        /// <devdoc>
        ///     Indicates whether value change notifications for this property may originate from outside the property
        ///     descriptor, such as from the component itself (value=true), or whether notifications will only originate
        ///     from direct calls made to PropertyDescriptor.SetValue (value=false). For example, the component may
        ///     implement the INotifyPropertyChanged interface, or may have an explicit '{name}Changed' event for this property.
        /// </devdoc>
        public virtual bool SupportsChangeEvents {
            get {
                return false;
            }
        }

    }
}

