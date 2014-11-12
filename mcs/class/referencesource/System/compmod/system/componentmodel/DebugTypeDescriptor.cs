//------------------------------------------------------------------------------
// <copyright file="DebugTypeDescriptor.cs" company="Microsoft">
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
    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System;
    using CodeAccessPermission = System.Security.CodeAccessPermission;
    using System.Security.Permissions;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Microsoft.Win32;
    using System.ComponentModel.Design;

    /// <devdoc>
    ///    <para>Provides information about the properties and events
    ///       for a component. This class cannot be inherited.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    internal sealed class DebugTypeDescriptor {
        private static Hashtable cachedComponentEntries = new Hashtable();
        private static Hashtable editorTables = new Hashtable();
        private static Hashtable attributeCache = new Hashtable();
        private static volatile RefreshEventHandler refreshHandler = null;
#pragma warning disable 618
        private static volatile IComNativeDescriptorHandler comNativeDescriptorHandler = null;
#pragma warning restore 618
        private static TraceSwitch CompDescrSwitch = new TraceSwitch("CompDescr", "Debug DebugTypeDescriptor.");
        
        private DebugTypeDescriptor() {
        }
        
        private static ComponentEntry GetEntry(object component, Type componentType) {
            ComponentEntry entry = null;
            
            lock(cachedComponentEntries) {
                entry = (ComponentEntry)cachedComponentEntries[componentType];
                if (entry == null) {
                    entry = new ComponentEntry(componentType);
                    cachedComponentEntries[componentType] = entry;
                }
            }

            return entry;
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
#pragma warning disable 618
        public static IComNativeDescriptorHandler ComNativeDescriptorHandler {
            [PermissionSetAttribute(SecurityAction.LinkDemand, Name="FullTrust")]
            get {
                return comNativeDescriptorHandler;
            }
            [PermissionSetAttribute(SecurityAction.LinkDemand, Name="FullTrust")]
            set {
                comNativeDescriptorHandler = value;
            }
        }
#pragma warning restore 618

        /// <devdoc>
        ///    <para>Occurs when Refreshed is raised for a component.</para>
        /// </devdoc>
        public static event RefreshEventHandler Refreshed {
            add {
                refreshHandler += value;
            }
            remove {
                refreshHandler -= value;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Adds an editor table for the given editor base type.
        ///       Typically, editors are specified as metadata on an object. If no metadata for a
        ///       requested editor base type can be found on an object, however, the
        ///       DebugTypeDescriptor will search an editor
        ///       table for the editor type, if one can be found.</para>
        /// </devdoc>
        public static void AddEditorTable(Type editorBaseType, Hashtable table) {
            lock(editorTables) {
                if (!editorTables.ContainsKey(editorBaseType)) {
                    editorTables[editorBaseType] = table;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Creates an instance of the designer associated with the
        ///       specified <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static IDesigner CreateDesigner(IComponent component, Type designerBaseType) {
            Type designerType = null;
            IDesigner designer = null;

            Debug.WriteLineIf(CompDescrSwitch.TraceVerbose, "CreateDesigner(" + component.GetType().FullName + ")");
            
            // Get the set of attributes for this type
            //
            AttributeCollection attributes = GetAttributes(component);
            
            for (int i = 0; i < attributes.Count; i++) {
                if (attributes[i] is DesignerAttribute) {
                    DesignerAttribute da = (DesignerAttribute)attributes[i];
                    Type attributeBaseType = Type.GetType(da.DesignerBaseTypeName);
                    if (attributeBaseType != null && attributeBaseType == designerBaseType) {
                        ISite site = component.Site;
                        bool foundService = false;
                        
                        if (site != null) {
                            ITypeResolutionService tr = (ITypeResolutionService)site.GetService(typeof(ITypeResolutionService));
                            if (tr != null) {
                                foundService = true;
                                designerType = tr.GetType(da.DesignerTypeName);
                            }
                        }
                        
                        if (!foundService) {
                            designerType = Type.GetType(da.DesignerTypeName);
                        }
                        
                        Debug.Assert(designerType != null, "It may be okay for the designer not to load, but we failed to load designer for component of type '" + component.GetType().FullName + "' because designer of type '" + da.DesignerTypeName + "'");
                        if (designerType != null) {
                            break;
                        }
                    }
                }
            }
            
            if (designerType != null) {
                designer = (IDesigner)Activator.CreateInstance(designerType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null);
            }
            else {
                Debug.WriteLineIf(CompDescrSwitch.TraceVerbose, "Could not find designer for: " + component.GetType().FullName);
            }
            return designer;
        }

        /// <devdoc>
        ///     This dynamically binds an EventDescriptor to a type.
        /// </devdoc>
        [ReflectionPermission(SecurityAction.LinkDemand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public static EventDescriptor CreateEvent(Type componentType, string name, Type type, params Attribute[] attributes) {
            return new DebugReflectEventDescriptor(componentType, name, type, attributes);
        }

        /// <devdoc>
        ///     This creates a new event descriptor identical to an existing event descriptor.  The new event descriptor
        ///     has the specified metadata attributes merged with the existing metadata attributes.
        /// </devdoc>
        [ReflectionPermission(SecurityAction.LinkDemand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public static EventDescriptor CreateEvent(Type componentType, EventDescriptor oldEventDescriptor, params Attribute[] attributes) {
            return new DebugReflectEventDescriptor(componentType, oldEventDescriptor, attributes);
        }

        /// <devdoc>
        ///     This dynamically binds a PropertyDescriptor to a type.
        /// </devdoc>
        [ReflectionPermission(SecurityAction.LinkDemand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public static PropertyDescriptor CreateProperty(Type componentType, string name, Type type, params Attribute[] attributes) {
            return new DebugReflectPropertyDescriptor(componentType, name, type, attributes);
        }

        /// <devdoc>
        ///     This creates a new property descriptor identical to an existing property descriptor.  The new property descriptor
        ///     has the specified metadata attributes merged with the existing metadata attributes.
        /// </devdoc>
        [ReflectionPermission(SecurityAction.LinkDemand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public static PropertyDescriptor CreateProperty(Type componentType, PropertyDescriptor oldPropertyDescriptor, params Attribute[] attributes) {

            // We must do some special case work here for extended properties.  If the old property descriptor is really
            // an extender property that is being surfaced on a component as a normal property, then we must
            // do work here or else DebugReflectPropertyDescriptor will fail to resolve the get and set methods.  We check
            // for the necessary ExtenderProvidedPropertyAttribute and if we find it, we create an
            // ExtendedPropertyDescriptor instead.  We only do this if the component class is the same, since the user
            // may want to re-route the property to a different target.
            //
            if (componentType == oldPropertyDescriptor.ComponentType) {
                ExtenderProvidedPropertyAttribute attr = (ExtenderProvidedPropertyAttribute)
                                                         oldPropertyDescriptor.Attributes[
                                                         typeof(ExtenderProvidedPropertyAttribute)];

                if (attr.ExtenderProperty != null) {
                    return new DebugExtendedPropertyDescriptor((DebugReflectPropertyDescriptor)attr.ExtenderProperty, attr.ReceiverType, attr.Provider, attributes);
                }
            }

            // This is either a normal prop or the caller has changed target classes.
            //
            return new DebugReflectPropertyDescriptor(componentType, oldPropertyDescriptor, attributes);
        }
        
        /// <devdoc>
        ///    <para>
        ///       Gets a
        ///       collection of attributes for the specified type of component.
        ///    </para>
        /// </devdoc>
        public static AttributeCollection GetAttributes(Type componentType) {
            return GetEntry(null, componentType).GetAttributes(null);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of attributes for the specified
        ///    <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static AttributeCollection GetAttributes(object component) {
            return GetAttributes(component, false);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of attributes for the specified
        ///    <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static AttributeCollection GetAttributes(object component, bool noCustomTypeDesc) {
            if (component == null) {
                return new AttributeCollection((Attribute[])null);
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
            
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning restore 618
                
                if (handler != null) {
                    return handler.GetAttributes(component);
                }
                return new AttributeCollection((Attribute[])null);
            }

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                return((ICustomTypeDescriptor)component).GetAttributes();
            }

            return GetEntry(component, component.GetType()).GetAttributes(component);
        }


        /// <devdoc>
        ///    <para>
        ///       Gets the name of the class for the specified
        ///    <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static string GetClassName(object component) {
            return GetClassName(component, false);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the class for the specified
        ///    <paramref name="component "/>using the custom type descriptor when <paramref name="noCustomTypeDesc 
        ///       "/>
        ///       is <see langword='false'/>.
        ///    </para>
        /// </devdoc>
        public static string GetClassName(object component, bool noCustomTypeDesc) {

            if (component == null) {
                throw new ArgumentNullException("component");
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
            
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning restore 618                
                if (handler != null) {
                    return handler.GetClassName(component);
                }
            }

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                string str = ((ICustomTypeDescriptor)component).GetClassName();
                if (str != null) {
                    return str;
                }
            }

            return component.GetType().FullName;
        }


        /// <devdoc>
        ///    <para>
        ///       The name of the class for the specified <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static string GetComponentName(object component) {
            return GetComponentName(component, false);
        }

        /// <devdoc>
        ///    <para>Gets the name of the class for the specified component.</para>
        /// </devdoc>
        public static string GetComponentName(object component, bool noCustomTypeDesc) {

            if (component == null) {
                throw new ArgumentNullException("component");
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
            
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning restore 618
                
                if (handler != null) {
                    return handler.GetName(component);
                }
            }

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                string str = ((ICustomTypeDescriptor)component).GetComponentName();
                if (str != null) {
                    return str;
                }
            }

            if (component is IComponent) {
                ISite site = ((IComponent)component).Site;
                if (site != null) {
                    return site.Name;
                }
            }
            return component.GetType().Name;
        }

        /// <devdoc>
        ///    <para>Gets a type converter for the type of the specified 
        ///       component.</para>
        /// </devdoc>
        public static TypeConverter GetConverter(object component) {
            return GetConverter(component, false);
        }

        /// <devdoc>
        ///    <para>Gets a type converter for the type of the specified 
        ///       component.</para>
        /// </devdoc>
        public static TypeConverter GetConverter(object component, bool noCustomTypeDesc) {
            if (component == null) {
                throw new ArgumentNullException("component");
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
            
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning restore 618
                
                if (handler != null) {
                    return handler.GetConverter(component);
                }
                return null;
            }

            TypeConverter converter = null;

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                converter = ((ICustomTypeDescriptor)component).GetConverter();
            }

            if (converter == null) {
                converter = GetEntry(component, component.GetType()).GetConverter(component);
            }

            return converter;
        }

        /// <devdoc>
        ///    <para>Gets a type converter for the specified type.</para>
        /// </devdoc>
        public static TypeConverter GetConverter(Type type) {
            return GetEntry(null, type).GetConverter();
        }

        /// <devdoc>
        ///     Querying custom attributes is very expensive, so we cache them
        ///     here.
        /// </devdoc>
        internal static object[] GetCustomAttributes(Type type) {
            object[] attributes = (object[])attributeCache[type];
            if (attributes == null) {
                attributes = type.GetCustomAttributes(false);
            
                lock(attributeCache) {
                    attributeCache[type] = attributes;
                }
            }
            
            return attributes;
        }
        
        /// <devdoc>
        ///     Querying custom attributes is very expensive, so we cache them
        ///     here.
        /// </devdoc>
        internal static object[] GetCustomAttributes(MemberInfo info) {
            object[] attributes = (object[])attributeCache[info];
            if (attributes == null) {
                attributes = info.GetCustomAttributes(false);
            
                lock(attributeCache) {
                    attributeCache[info] = attributes;
                }
            }
            
            return attributes;
        }
                
        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the default event for the specified
        ///       type of component.
        ///    </para>
        /// </devdoc>
        public static EventDescriptor GetDefaultEvent(Type componentType) {
            return GetEntry(null, componentType).GetDefaultEvent(null);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the default event for the specified
        ///    <paramref name="component"/>.
        /// </para>
        /// </devdoc>
        public static EventDescriptor GetDefaultEvent(object component) {
            return GetDefaultEvent(component, false);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the default event for a component.
        ///    </para>
        /// </devdoc>
        public static EventDescriptor GetDefaultEvent(object component, bool noCustomTypeDesc) {
            if (component == null) {
                return null;
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
            
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning disable 618                
                if (handler != null) {
                    return handler.GetDefaultEvent(component);
                }
                return null;
            }

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                return((ICustomTypeDescriptor)component).GetDefaultEvent();
            }

            return GetEntry(component, component.GetType()).GetDefaultEvent(component);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the default
        ///       property for the
        ///       specified type of component.
        ///    </para>
        /// </devdoc>
        public static PropertyDescriptor GetDefaultProperty(Type componentType) {
            return GetEntry(null, componentType).GetDefaultProperty(null);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the default property for the specified
        ///    <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static PropertyDescriptor GetDefaultProperty(object component) {
            return GetDefaultProperty(component, false);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the default property for the specified
        ///    <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static PropertyDescriptor GetDefaultProperty(object component, bool noCustomTypeDesc) {
            if (component == null) {
                return null;
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
            
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning restore 618                
                if (handler != null) {
                    return handler.GetDefaultProperty(component);
                }
                return null;
            }

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                return((ICustomTypeDescriptor)component).GetDefaultProperty();
            }

            return GetEntry(component, component.GetType()).GetDefaultProperty(component);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets an editor with the specified base type for the
        ///       specified <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static object GetEditor(object component, Type editorBaseType) {
            return GetEditor(component, editorBaseType, false);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets an editor with the specified base type for the
        ///       specified <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static object GetEditor(object component, Type editorBaseType, bool noCustomTypeDesc) {
            if (component == null) {
                throw new ArgumentNullException("component");
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
            
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning restore 618
                if (handler != null) {
                    return handler.GetEditor(component, editorBaseType);
                }
                return null;
            }

            object editor = null;

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                editor = ((ICustomTypeDescriptor)component).GetEditor(editorBaseType);
            }

            if (editor == null) {
                editor = GetEntry(component, component.GetType()).GetEditor(component, editorBaseType);
            }

            return editor;
        }

        /// <devdoc>
        ///    <para>Gets an editor with the specified base type for the specified type.</para>
        /// </devdoc>
        public static object GetEditor(Type type, Type editorBaseType) {
            Debug.Assert(type != null, "Can't get editor for null type");
            return GetEntry(null, type).GetEditor(editorBaseType);
        }

        /// <devdoc> 
        ///      Retrieves a default editor table for the given editor base type. 
        /// </devdoc> 
        private static Hashtable GetEditorTable(Type editorBaseType) {
            object table = null;
            
            lock(editorTables) {
                table = editorTables[editorBaseType];
            }
            
            if (table == null) {
                // Before we give up, it is possible that the
                // class initializer for editorBaseType hasn't 
                // actually run.  Force it now.  We try/catch
                // here in case editorBaseType throws us a curve ball.
                //
                if (!editorBaseType.IsAbstract) {
                    try {
                        object o = Activator.CreateInstance(editorBaseType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null);
                    }
                    catch {}
                }
                
                lock(editorTables) {
                    table = editorTables[editorBaseType];
                    
                    // If the table is still null, then throw a
                    // sentinel in there so we don't
                    // go through this again.
                    //
                    if (table == null) {
                        editorTables[editorBaseType] = editorTables;
                    }
                }
            }
            
            // Look for our sentinel value that indicates
            // we have already tried and failed to get
            // a table.
            //
            if (table == editorTables) {
                table = null;
            }
            
            return (Hashtable)table;
        }
        
        /// <devdoc>
        ///    <para>
        ///       Gets a collection of events for a specified type of component.
        ///    </para>
        /// </devdoc>
        public static EventDescriptorCollection GetEvents(Type componentType) {
            return GetEvents(componentType, null);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of events for a
        ///       specified <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static EventDescriptorCollection GetEvents(object component) {
            return GetEvents(component, false);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of events for a
        ///       specified <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static EventDescriptorCollection GetEvents(object component, bool noCustomTypeDesc) {
            if (component == null) {
                return new EventDescriptorCollection(null, true);
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
                
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning restore 618                
                if (handler != null) {
                    return handler.GetEvents(component);
                }
                return new EventDescriptorCollection(null, true);
            }


            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                EventDescriptorCollection events = ((ICustomTypeDescriptor)component).GetEvents();
                return GetEntry(component, component.GetType()).FilterEvents(component, null, events);
            }

            return GetEvents(component, null);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of events for a specified type of
        ///       component using a specified array of <paramref name="attributes "/>
        ///       as a filter.
        ///    </para>
        /// </devdoc>
        public static EventDescriptorCollection GetEvents(Type componentType, Attribute[] attributes) {
            return GetEntry(null, componentType).GetEvents(null, attributes);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of events for a
        ///       specified <paramref name="component "/>using a specified array of <paramref name="attributes"/>
        ///       as a filter.
        ///    </para>
        /// </devdoc>
        public static EventDescriptorCollection GetEvents(object component, Attribute[] attributes) {
            return GetEvents(component, attributes, false);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of events for a
        ///       specified <paramref name="component "/>using a specified array of <paramref name="attributes
        ///       "/>
        ///       as a filter.
        ///    </para>
        /// </devdoc>
        public static EventDescriptorCollection GetEvents(object component, Attribute[] attributes, bool noCustomTypeDesc) {
            if (component == null) {
                return new EventDescriptorCollection(null, true);
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
                
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning restore 618
                
                if (handler != null) {
                    return handler.GetEvents(component, attributes);
                }
                return new EventDescriptorCollection(null, true);
            }

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                EventDescriptorCollection events = ((ICustomTypeDescriptor)component).GetEvents(attributes);
                return GetEntry(component, component.GetType()).FilterEvents(component, attributes, events);
            }
            
            return GetEntry(component, component.GetType()).GetEvents(component, attributes, (component is ICustomTypeDescriptor));
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of properties for a specified type of
        ///       component.
        ///    </para>
        /// </devdoc>
        public static PropertyDescriptorCollection GetProperties(Type componentType) {
            return GetProperties(componentType, null);
        }

        /// <devdoc>
        ///    <para>Gets a collection of properties for a specified 
        ///       component.</para>
        /// </devdoc>
        public static PropertyDescriptorCollection GetProperties(object component) {
            return GetProperties(component, false);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a collection of properties for a
        ///       specified <paramref name="component"/>.
        ///    </para>
        /// </devdoc>
        public static PropertyDescriptorCollection GetProperties(object component, bool noCustomTypeDesc) {
            if (component == null) {
                throw new ArgumentNullException("component");
            }

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                PropertyDescriptorCollection properties = ((ICustomTypeDescriptor)component).GetProperties();
                return GetEntry(component, component.GetType()).FilterProperties(component, null, properties);
            }

            return GetProperties(component, null, (component is ICustomTypeDescriptor));
        }

        /// <devdoc>
        ///    <para>Gets a collection of properties for a specified type of 
        ///       component using a specified array of attributes as a filter.</para>
        /// </devdoc>
        public static PropertyDescriptorCollection GetProperties(Type componentType, Attribute[] attributes) {
            return GetEntry(null, componentType).GetProperties(null, attributes);
        }

        /// <devdoc>
        ///    <para>Gets a collection of properties for a specified 
        ///       component using a specified array of attributes
        ///       as a filter.</para>
        /// </devdoc>
        public static PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes) {
            if (component == null) {
                throw new ArgumentNullException("component");
            }

            return GetProperties(component, attributes, false);
        }

        /// <devdoc>
        ///    <para>Gets a collection of properties for a specified 
        ///       component using a specified array of attributes
        ///       as a filter.</para>
        /// </devdoc>
        public static PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes, bool noCustomTypeDesc) {
            if (component == null) {
                return new PropertyDescriptorCollection(null, true);
            }

            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
                // Do not rip this varible -- it is here to make the get to 
                // this static variable thread-safe.
                //
#pragma warning disable 618
                IComNativeDescriptorHandler handler = comNativeDescriptorHandler;
#pragma warning restore 618
                
                if (handler != null) {
                    return handler.GetProperties(component, attributes);
                }
                return new PropertyDescriptorCollection(null, true);
            }

            if (!noCustomTypeDesc && component is ICustomTypeDescriptor) {
                PropertyDescriptorCollection properties = ((ICustomTypeDescriptor)component).GetProperties(attributes);
                return GetEntry(component, component.GetType()).FilterProperties(component, attributes, properties);
            }

            return GetEntry(component, component.GetType()).GetProperties(component, attributes,(component is ICustomTypeDescriptor));
        }

        /// <devdoc>
        ///    <para>Clears the properties and events for the specified 
        ///       component from the
        ///       cache.</para>
        /// </devdoc>
        public static void Refresh(object component) {

            if (component == null) {
                return;
            }

            // COM objects aren't cached since we don't
            // want to be holding references to them.
            if (System.Runtime.InteropServices.Marshal.IsComObject(component)) {
                return;
            }

            // We only fire the change event if we have cached the componet.
            // Since we will recreate the cache entry if anyone has asked for
            // this component... this prevents getting tons of duplicate
            // Refresh events.
            //
            ComponentEntry entry = null;
            
            lock(cachedComponentEntries) {
                entry = (ComponentEntry)cachedComponentEntries[component.GetType()];
            }
            
            if (entry != null) {

                // Clear the attribute cache.  It's not that expensive to build
                // this back up if we need to.
                //
                lock(attributeCache) {
                    attributeCache.Clear();
                }
                
                // Remove the item from the cache, it will get recreated
                // on demand.
                //
                lock(cachedComponentEntries) {
                    cachedComponentEntries.Remove(component.GetType());
                }

                // Allow the entry to dispose itself.
                //
                entry.Dispose(component);

                // Notify listeners of the change
                //
                RefreshEventHandler handler = refreshHandler;
                
                if (handler != null) {
                    handler(new RefreshEventArgs(component));
                }
            }
        }

        /// <devdoc>
        ///    <para>Clears the properties and events for the specified type 
        ///       of component from the
        ///       cache.</para>
        /// </devdoc>
        public static void Refresh(Type type) {

            if (type == null) {
                return;
            }

            // We only fire the change event if we have cached the componet.
            // Since we will recreate the cache entry if anyone has asked for
            // this component... this prevents getting tons of duplicate
            // Refresh events.
            //
            bool found = false;
            
            lock (cachedComponentEntries) {
                
                ArrayList removeItems = null;

                // find all the instances of the requested type
                // and any types that derive from it,
                // and remove them.
                //
                foreach (Type cacheType in cachedComponentEntries.Keys) {
                    if (type.IsAssignableFrom(cacheType)) {
                        if (removeItems == null) {
                            removeItems = new ArrayList();
                        }
                        removeItems.Add(cacheType);
                    }
                }

                if (removeItems != null) {
                    foreach (Type t in removeItems) {
                        // Remove the item from the cache, it will get recreated
                        // on demand.
                        //
                        cachedComponentEntries.Remove(t);
                    }
                    found = true;
                }
            }

            if (found) {
            
                // Clear the attribute cache.  It's not that expensive to build
                // this back up if we need to.
                //
                lock(attributeCache) {
                    attributeCache.Clear();
                }
                
                RefreshEventHandler handler = refreshHandler;
            
                // Notify listeners of the change
                //
                if (handler != null) {
                    handler(new RefreshEventArgs(type));
                }
            }
        }

        /// <devdoc>
        ///    <para>Clears the properties and events for the specified 
        ///       module from the
        ///       cache.</para>
        /// </devdoc>
        public static void Refresh(Module module) {

            if (module == null) {
                return;
            }

            ArrayList list = null;
            
            lock(cachedComponentEntries) {
                foreach (Type curType in cachedComponentEntries.Keys) {
                    if (curType.Module.Equals(module)) {
                        if (list == null) {
                            list = new ArrayList();
                        }
                        list.Add(curType);
                    }
                }

                // now remove all the ones we tagged -- can't do this
                // in the enumeration.
                //
                if (list != null) {
                    foreach(Type t in list) {
                        cachedComponentEntries.Remove(t);
                    }
                }
            }
            
            if (list != null) {
                // Clear the attribute cache.  It's not that expensive to build
                // this back up if we need to.
                //
                lock(attributeCache) {
                    attributeCache.Clear();
                }
                
                RefreshEventHandler handler = refreshHandler;
                if (handler != null) {
                    foreach(Type curType in list) {
                        handler(new RefreshEventArgs(curType));
                    }
                }
            }
        }
        
        /// <devdoc>
        ///    <para>Clears the properties and events for the specified 
        ///       assembly from the
        ///       cache.</para>
        /// </devdoc>
        public static void Refresh(Assembly assembly) {

            if (assembly == null) {
                return;
            }
            
            foreach (Module mod in assembly.GetModules()) {
                Refresh(mod);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Sorts descriptors by name of the descriptor.
        ///    </para>
        /// </devdoc>
        public static void SortDescriptorArray(IList infos) {
            ArrayList.Adapter(infos).Sort(MemberDescriptorComparer.Instance);
        }


        /// <devdoc> 
        ///     ComponentEntry contains the properties, events, extenders, and attributes for 
        ///     a given type of component. 
        /// </devdoc> 
        private class ComponentEntry {
            private Type componentType;
            private PropertyDescriptorCollection properties;
            private EventDescriptorCollection events;
            private AttributeCollection attributes;
            private IList extenders;
            private Hashtable wrappedExtenderTable;
            private TypeConverter converter;
            private object[]      editors;
            private Type[]        editorTypes;
            private int           editorCount;
            
            // This is an index that we use to create a unique name for a property in the
            // event of a name collision.  The only time we should use this is when
            // a name collision happened on an extender property that has no site or
            // no name on its site.  Should be very rare.
            private static int collisionIndex = 0;
            
            // This is the signature we look for when creating types that are generic, but
            // want to know what type they are dealing with.  Enums are a good example of this;
            // there is one enum converter that can work with all enums, but it needs to know
            // the type of enum it is dealing with.
            //
            private static Type[] typeConstructor = new Type[] {typeof(Type)};

            // This is where we store the various converters, etc for the intrinsic types.
            //
            private static volatile Hashtable intrinsicTypeConverters;

            // For converters, etc that are bound to class attribute data, rather than a class
            // type, we have special key sentinel values that we put into the hash table.
            //
            private static object intrinsicEnumKey = new object();
            private static object intrinsicReferenceKey = new object();

            /// <devdoc> 
            ///     Creates a new ComponentEntry.  The ComponentManager should be the 
            ///     only component that creates this. 
            /// </devdoc> 
            /// <internalonly/> 
            public ComponentEntry(Type componentType) {
                this.componentType = componentType;
            }

            /// <devdoc> 
            ///      This is a table we create for intrinsic types. 
            ///      There should be entries here ONLY for intrinsic 
            ///      types, as all other types we should be able to 
            ///      add attributes directly as metadata. 
            /// </devdoc> 
            private static Hashtable IntrinsicTypeConverters {
                get {
                    // It is not worth taking a lock for this -- worst case of a collision
                    // would build two tables, one that garbage collects very quickly.
                    //
                    if (intrinsicTypeConverters == null) {
                        Hashtable temp = new Hashtable();

                        // Add the intrinsics
                        //
                        temp[typeof(bool)] = typeof(BooleanConverter);
                        temp[typeof(byte)] = typeof(ByteConverter);
                        temp[typeof(SByte)] = typeof(SByteConverter);
                        temp[typeof(char)] = typeof(CharConverter);
                        temp[typeof(double)] = typeof(DoubleConverter);
                        temp[typeof(string)] = typeof(StringConverter);
                        temp[typeof(int)] = typeof(Int32Converter);
                        temp[typeof(short)] = typeof(Int16Converter);
                        temp[typeof(long)] = typeof(Int64Converter);
                        temp[typeof(float)] = typeof(SingleConverter);
                        temp[typeof(UInt16)] = typeof(UInt16Converter);
                        temp[typeof(UInt32)] = typeof(UInt32Converter);
                        temp[typeof(UInt64)] = typeof(UInt64Converter);
                        temp[typeof(object)] = typeof(TypeConverter);
                        temp[typeof(void)] = typeof(TypeConverter);
                        temp[typeof(CultureInfo)] = typeof(CultureInfoConverter);
                        temp[typeof(DateTime)] = typeof(DateTimeConverter);
                        temp[typeof(DateTimeOffset)] = typeof(DateTimeOffsetConverter);
                        temp[typeof(Decimal)] = typeof(DecimalConverter);
                        temp[typeof(TimeSpan)] = typeof(TimeSpanConverter);
                        temp[typeof(Guid)] = typeof(GuidConverter);
                        temp[typeof(Array)] = typeof(ArrayConverter);
                        temp[typeof(ICollection)] = typeof(CollectionConverter);

                        // Special cases for things that are not bound to a specific type
                        //
                        temp[intrinsicEnumKey] = typeof(EnumConverter);
                        temp[intrinsicReferenceKey] = typeof(ReferenceConverter);
                        
                        intrinsicTypeConverters = temp;
                    }
                    return intrinsicTypeConverters;
                }
            }

            /// <devdoc> 
            ///      Creates an instance of the requested type.  This is used by converters 
            ///      and editors to create versions of those objects.  CreateInstance 
            ///      will look for a constructor that takes a type.  If one is found, the type of the 
            ///      component this entry is providing information for will be passed in.  This allows 
            ///      a single object to be re-used for more than one type. 
            /// </devdoc> 
            private object CreateInstance(Type type) {
                if ((!(type.IsPublic || type.IsNestedPublic)) && (type.Assembly == typeof(DebugTypeDescriptor).Assembly)) {
                    IntSecurity.FullReflection.Demand();
                }

                ConstructorInfo ctor = type.GetConstructor(typeConstructor);
                if (ctor != null) {
                    return ctor.Invoke(new object[] {componentType});
                }
                
                return Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null);
            }

            // <doc>
            // <desc>
            //      Disposes this entry.  The work we do here is to make sure that
            //      any cached information we saved on the given component has been
            //      cleared out.
            // </desc>
            // <param term='component'>
            //      The object to clear.
            // </param>
            // </doc>
            public void Dispose(object component) {
                if (component is IComponent) {
                    IDictionaryService ds = (IDictionaryService)GetService((IComponent)component, typeof(IDictionaryService));
                    if (ds != null) {
                        ds.SetValue(typeof(AttributeCollection), null);
                        ds.SetValue(typeof(EventDescriptorCollection), null);
                        ds.SetValue(typeof(PropertyStash), null);
                    }
                }
            }

            /// <devdoc> 
            ///     This function will pass through the set of members and select only those which match 
            ///     all of the given attributes. 
            /// </devdoc> 
            public virtual void FilterMembers(Type infoType, IList infos, Attribute[] attributes) {
                if (attributes != null && attributes.Length > 0 && infos.Count > 0) {
                    
                    switch (attributes.Length) {
                        case 1: {
                                Attribute filterAttribute = attributes[0];
                                for (int i=infos.Count - 1; i>=0; i--) {
                                    if (ShouldHideMember((MemberDescriptor)infos[i],filterAttribute)) {
                                        infos.RemoveAt(i);
                                    }
                                }
                                break;
                            }
                        case 2: {
                                Attribute filterAttribute1 = attributes[0];
                                Attribute filterAttribute2 = attributes[1];

                                for (int i=infos.Count - 1; i>=0; i--) {
                                    if (ShouldHideMember((MemberDescriptor)infos[i], filterAttribute1) ||
                                        ShouldHideMember((MemberDescriptor)infos[i], filterAttribute2)) {

                                        infos.RemoveAt(i);
                                    }
                                }
                                break;
                            }
                        default:
                            for (int i=infos.Count - 1; i>=0; i--) {
                                for (int j = 0; j < attributes.Length; j++) {
                                    if (ShouldHideMember((MemberDescriptor)infos[i], attributes[j])) {
                                        infos.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            /// <devdoc> 
            ///     Retrieves the class level attributes for this type of componet that 
            ///     satisfy all of the passed-in attributes.  For a class attribute to 
            ///     satisfy a particular attribute, the attribute must be present in the 
            ///     class's attribute list, or the attribute must match it's own default. 
            /// </devdoc> 
            public AttributeCollection GetAttributes(object component) {

                // Another worst case collision scenario:  we don't want the perf hit
                // of taking a lock.
                //
                if (this.attributes == null) {
                    this.attributes = new AttributeCollection(new MemberList(this).GetAttributes());
                }
                
                AttributeCollection filteredAttributes = attributes;
                
                if (component is IComponent) {
                    ITypeDescriptorFilterService tf = (ITypeDescriptorFilterService)GetService(component, typeof(ITypeDescriptorFilterService));
                    if (tf != null) {
                    
                        // The component's site is interested in filtering attributes.  See if we
                        // have filtered them before.  If so, then we're done.  Otherwise we
                        // need to filter.
                        //
                        IDictionaryService ds = (IDictionaryService)GetService(component, typeof(IDictionaryService));
                        if (ds != null) {
                            AttributeCollection savedAttributes = null;
                            
                            lock(ds) {
                                savedAttributes = (AttributeCollection)ds.GetValue(typeof(AttributeCollection));
                            }
                            if (savedAttributes != null) {
                            
                                // Check that the filter that was used to create these attributes is the same
                                // filter we currently have.  People may replace the filter, and if we do 
                                // we must refresh the cache.
                                //
                                object savedFilter = ds.GetValue(typeof(ITypeDescriptorFilterService));
                                if (savedFilter == null || savedFilter == tf) {
                                    filteredAttributes = savedAttributes;
                                }
                            }
                        }
                        
                        if (filteredAttributes == attributes) {
                            Hashtable filterTable = new Hashtable(attributes.Count);
                            
                            if (attributes != null) {
                                foreach (Attribute attr in attributes) {
                                    filterTable[attr.TypeId] = attr;
                                }
                            }
                            
                            bool cache = tf.FilterAttributes((IComponent)component, filterTable);
                            Attribute[] temp = new Attribute[filterTable.Values.Count];
                            filterTable.Values.CopyTo(temp, 0);
                            filteredAttributes = new AttributeCollection(temp);
                            
                            if (ds != null && cache) {
                                lock(ds) {
                                    ds.SetValue(typeof(AttributeCollection), filteredAttributes);
                                    ds.SetValue(typeof(ITypeDescriptorFilterService), tf);
                                }
                            }
                        }
                    }
                }
                
                return filteredAttributes;
            }
            
            /* 
               The following code has been removed to fix FXCOP violations.  The code
               is left here incase it needs to be resurrected in the future.

            /// <devdoc> 
            ///      Retrieves the designer for the given object, or null if there 
            ///      is no designer. 
            /// </devdoc> 
            /// <internalonly/> 
            private IDesigner GetComponentDesigner(object component) {
                IDesigner designer = null;

                if (component is IComponent) {
                    IComponent comp = (IComponent)component;
                    ISite site = comp.Site;
                    if (site != null) {
                        IDesignerHost host = (IDesignerHost)site.GetService(typeof(IDesignerHost));
                        if (host != null) {
                            designer = host.GetDesigner(comp);
                        }
                    }
                }

                return designer;
            }

            /// <devdoc> 
            ///     Retrieves the type of this kind of component. 
            /// </devdoc> 
            public Type GetComponentType() {
                return componentType;
            }
            */


            /// <devdoc> 
            ///      Retrieves the type converter for this entry. 
            ///      Type converters are found by either looking for a 
            ///      TypeConverterAttribute on the component's class, or by 
            ///      traversing the base class heirarchy of the class until 
            ///      a primitive type is found. 
            /// </devdoc> 
            public TypeConverter GetConverter() {
                if (converter == null) {
                    TypeConverterAttribute attr = (TypeConverterAttribute)GetAttributes(null)[typeof(TypeConverterAttribute)];
                    if (attr != null) {
                        Type converterType = GetTypeFromName(attr.ConverterTypeName);
                        if (converterType != null && typeof(TypeConverter).IsAssignableFrom(converterType)) {
                            converter = (TypeConverter)CreateInstance(converterType);
                        }
                    }

                    if (converter == null) {

                        // We did not get a converter.  Traverse up the base class chain until
                        // we find one in the stock hashtable.
                        //
                        converter = (TypeConverter)SearchIntrinsicTable(IntrinsicTypeConverters);
                        Debug.Assert(converter != null, "There is no intrinsic setup in the hashtable for the Object type");
                    }
                }
                return converter;
            }

            /// <devdoc> 
            ///      Retrieves the type converter for this entry. 
            ///      Type converters are found by either looking for a 
            ///      TypeConverterAttribute on the component's class, or by 
            ///      traversing the base class heirarchy of the class until 
            ///      a primitive type is found. 
            /// </devdoc> 
            public TypeConverter GetConverter(object component) {

                TypeConverter obj = null;

                // For components, the design time object for them may want to redefine the
                // attributes.  So, we search the attribute here based on the component.  If found,
                // we then search on the same attribute based on type.  If the two don't match, then
                // we cannot cache the value and must re-create every time.  It is rare for a designer
                // to override these attributes, so we want to be smart here.
                //
                TypeConverterAttribute attr = (TypeConverterAttribute)GetAttributes(component)[typeof(TypeConverterAttribute)];

                if (attr != null) {
                    TypeConverterAttribute baseAttr = (TypeConverterAttribute)GetAttributes(null)[typeof(TypeConverterAttribute)];
                    if (baseAttr != attr) {
                        Type converterType = GetTypeFromName(attr.ConverterTypeName);
                        if (converterType != null && typeof(TypeConverter).IsAssignableFrom(converterType)) {
                            obj = (TypeConverter)CreateInstance(converterType);
                        }
                    }
                }

                // No custom attribute, so we can just use the stock one.
                //
                if (obj == null) {
                    obj = GetConverter();
                }

                return obj;
            }

            /// <devdoc> 
            ///     Retrieves the default event. 
            /// </devdoc> 
            public EventDescriptor GetDefaultEvent(object component) {
                string name = ((DefaultEventAttribute)GetAttributes(component)[typeof(DefaultEventAttribute)]).Name;
                if (name != null) {
                    EventDescriptorCollection evts = GetEvents(component, null);
                    EventDescriptor evt = evts[name];
                    if (evt == null && evts.Count > 0) {
                        evt = evts[0];
                    }
                    return evt;
                }
                return null;
            }

            /// <devdoc> 
            ///     Retrieves the default property. 
            /// </devdoc> 
            public PropertyDescriptor GetDefaultProperty(object component) {
                string name = ((DefaultPropertyAttribute)GetAttributes(component)[typeof(DefaultPropertyAttribute)]).Name;
                if (name != null) {
                    return GetProperties(component, null)[name];
                }
                return null;
            }

            /// <devdoc> 
            ///      Retrieves an editor with the given base type.  You may define multiple editor 
            ///      attributes for a type or property.  This allows you to select the one you want. 
            /// </devdoc> 
            public object GetEditor(Type editorBaseType) {
                object editor = null;

                // Check the editors we've already created for this type.
                //
                lock(this) {
                    if (editorTypes != null) {
                        for (int i = 0; i < editorCount; i++) {
                            if (editorTypes[i] == editorBaseType) {
                                return editors[i];
                            }
                        }
                    }
                }

                // If one wasn't found, then we must go through the attributes.
                //
                if (editor == null) {
                    AttributeCollection attrs = GetAttributes(null);

                    for (int i = 0; i < attrs.Count; i++) {

                        if (attrs[i] is EditorAttribute) {
                            EditorAttribute attr = (EditorAttribute)attrs[i];
                            Type attrEditorBaseType = GetTypeFromName(attr.EditorBaseTypeName);
                            
                            if (attrEditorBaseType != null && attrEditorBaseType == editorBaseType) {
                                Type type = GetTypeFromName(attr.EditorTypeName);
    
                                if (type != null) {
                                    editor = CreateInstance(type);
                                    break;
                                }
                            }
                        }
                    }
                    
                    // Check our set of intrinsic editors.  These are provided by an external party.
                    //
                    if (editor == null) {
                        Hashtable intrinsicEditors = DebugTypeDescriptor.GetEditorTable(editorBaseType);
                        if (intrinsicEditors != null) {
                            editor  = SearchIntrinsicTable(intrinsicEditors);
                        }
                    }
                    
                    // As a quick sanity check, check to see that the editor we got back is of 
                    // the correct type.
                    //
                    if (editor != null && !editorBaseType.IsInstanceOfType(editor)) {
                        Debug.Fail("Editor " + editor.GetType().FullName + " is not an instance of " + editorBaseType.FullName + " but it is in that base types table.");
                        editor = null;
                    }
                        
                    // Now, another slot in our editor cache for next time
                    //
                    lock(this) {
                    
                        // we do a redundant check here for the editor, just in 
                        // case another thread added it.  We could have locked
                        // the entire method, but with all of the other
                        // callouts from this method deadlock becomes more probable.
                        //
                        // This is very safe, but I'm not sure there are any
                        // bad consequences of having duplicate editor types in the
                        // array.  Better to be safe, but we do take an ever so minor
                        // hit here...
                        //
                        bool redundantEditor = false;
                        
                        if (editorTypes != null) {
                            for (int i = 0; i < editorCount; i++) {
                                if (editorTypes[i] == editorBaseType) {
                                    redundantEditor = true;
                                    break;
                                }
                            }
                        }
                    
                        if (!redundantEditor) {
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
                    }
                }

                return editor;
            }

            /// <devdoc> 
            ///      Retrieves an editor with the given base type.  You may define multiple editor 
            ///      attributes for a type or property.  This allows you to select the one you want. 
            /// </devdoc> 
            public object GetEditor(object component, Type editorBaseType) {
                object editor = null;

                // For components, the design time object for them may want to redefine the
                // attributes.  So, we search the attribute here based on the component.  If found,
                // we then search on the same attribute based on type.  If the two don't match, then
                // we cannot cache the value and must re-create every time.  It is rare for a designer
                // to override these attributes, so we want to be smart here.
                //
                AttributeCollection attrs;

                EditorAttribute attr = null;

                attrs = GetAttributes(component);
                for (int i = 0; i < attrs.Count; i++) {

                    if (attrs[i] is EditorAttribute) {
                        EditorAttribute a = (EditorAttribute)attrs[i];
                        Type attrEditorBaseType = GetTypeFromName(a.EditorBaseTypeName);
    
                        if (attrEditorBaseType != null && attrEditorBaseType == editorBaseType) {
                            attr = a;
                            break;
                        }
                    }
                }

                if (attr != null) {
                    // Now, compare this attribute against the one provided by the normal attribute set for the
                    // type.  If it is the same, then we can resort to the normal GetEditor call.  Otherwise,
                    // we must create the editor anew.
                    //
                    EditorAttribute baseAttr = null;

                    attrs = GetAttributes(null);
                    for (int i = 0; i < attrs.Count; i++) {

                        if (attrs[i] is EditorAttribute) {
                            EditorAttribute a = (EditorAttribute)attrs[i];
                            Type attrEditorBaseType = GetTypeFromName(a.EditorBaseTypeName);
    
                            if (attrEditorBaseType != null && attrEditorBaseType == editorBaseType) {
                                baseAttr = a;
                                break;
                            }
                        }
                    }

                    if (attr != baseAttr) {
                        // The attribute we should use is a custom attribute provided by the
                        // designer of this object.  Create the editor directly.  This will
                        // be fairly rare (nothing in .NET uses this today, and we're pretty
                        // broad), so I'm not too concerned with caching this.
                        //
                        Type type = GetTypeFromName(attr.EditorTypeName);
                        if (type != null) {
                            editor = CreateInstance(type);
                        }
                    }
                }

                if (editor == null) {
                    editor = GetEditor(editorBaseType);
                }
                else {
                    // As a quick sanity check, check to see that the editor we got back is of 
                    // the correct type.
                    //
                    if (editor != null && !editorBaseType.IsInstanceOfType(editor)) {
                        Debug.Fail("Editor " + editor.GetType().FullName + " is not an instance of " + editorBaseType.FullName + " but it is in that base types table.");
                        editor = null;
                    }
                }

                return editor;
            }

            /// <devdoc> 
            ///     Retrieves events that satisfy all of the passed-in attributes. 
            ///     For an event to satisfy a particular attribute, the attribute must be 
            ///     present in the event's attribute list, or the attribute must match it's 
            ///     own default.  The returned array is sorted based on the sort parameter. 
            /// </devdoc> 
            public EventDescriptorCollection GetEvents(object component, Attribute[] attributes) {
                return GetEvents(component, attributes, false);
            }

            /// <devdoc> 
            ///     Retrieves events that satisfy all of the passed-in attributes. 
            ///     For an event to satisfy a particular attribute, the attribute must be 
            ///     present in the event's attribute list, or the attribute must match it's 
            ///     own default.  The returned array is sorted based on the sort parameter. 
            /// </devdoc> 
            public EventDescriptorCollection GetEvents(object component, Attribute[] attributes, bool noFilter) {
                // Worst case event collision scenario is two sets of events.  Much cheaper than
                // a constant lock.
                //
                if (this.events == null) {
                    this.events = new EventDescriptorCollection(new MemberList(this).GetEvents(), true);
                }

                EventDescriptorCollection filteredEvents = events;

                if (component is IComponent) {
                    ITypeDescriptorFilterService tf = (ITypeDescriptorFilterService)GetService(component, typeof(ITypeDescriptorFilterService));
                    
                    if (!noFilter && tf != null) {
                        // The component's site is interested in filtering events.  See if we
                        // have filtered them before.  If so, then we're done.  Otherwise we
                        // need to filter.
                        //
                        IDictionaryService ds = (IDictionaryService)GetService(component, typeof(IDictionaryService));
                        if (ds != null) {
                            EventDescriptorCollection savedEvents = null;
                             
                            lock(ds) {
                                savedEvents = (EventDescriptorCollection)ds.GetValue(typeof(EventDescriptorCollection));
                            }
                            
                            if (savedEvents != null) {
                            
                                // Check that the filter that was used to create these attributes is the same
                                // filter we currently have.  People may replace the filter, and if we do 
                                // we must refresh the cache.
                                //
                                object savedFilter = ds.GetValue(typeof(ITypeDescriptorFilterService));
                                if (savedFilter == null || savedFilter == tf) {
                                    filteredEvents = savedEvents;
                                }
                            }
                        }
                        
                        if (filteredEvents == events) {
                            Hashtable filterTable = new Hashtable(events.Count);
                            
                            if (events != null) {
                                foreach (EventDescriptor ev in events) {
                                    filterTable[ev.Name] = ev;
                                }
                            }
                            
                            bool cache = tf.FilterEvents((IComponent)component, filterTable);
                            EventDescriptor[] temp = new EventDescriptor[filterTable.Values.Count];
                            filterTable.Values.CopyTo(temp, 0);
                            filteredEvents = new EventDescriptorCollection(temp, true);
                            
                            if (ds != null && cache) {
                                lock(ds) {
                                    ds.SetValue(typeof(EventDescriptorCollection), filteredEvents);
                                    ds.SetValue(typeof(ITypeDescriptorFilterService), tf);
                                }
                            }
                        }
                    }
                }
                
                if (attributes != null && attributes.Length > 0) {
                    ArrayList list = new ArrayList(filteredEvents);
                    FilterMembers(typeof(EventDescriptor), list, attributes);
                    EventDescriptor[] temp = new EventDescriptor[list.Count];
                    list.CopyTo(temp, 0);
                    filteredEvents = new EventDescriptorCollection(temp, true);
                }
                
                return filteredEvents;
            }

            internal EventDescriptorCollection FilterEvents(object component, Attribute[] attributes, EventDescriptorCollection events) {
                EventDescriptorCollection filteredEvents = events;

                if (component is IComponent) {
                    ITypeDescriptorFilterService tf = (ITypeDescriptorFilterService)GetService(component, typeof(ITypeDescriptorFilterService));
                    if (tf != null) {
                        Hashtable filterTable = new Hashtable(events.Count);
                        
                        if (events != null) {
                            foreach (EventDescriptor ev in events) {
                                filterTable[ev.Name] = ev;
                            }
                        }
                        
                        bool cache = tf.FilterEvents((IComponent)component, filterTable);
                        EventDescriptor[] temp = new EventDescriptor[filterTable.Values.Count];
                        filterTable.Values.CopyTo(temp, 0);
                        filteredEvents = new EventDescriptorCollection(temp, true);
                    }
                }
                
                if (attributes != null && attributes.Length > 0) {
                    ArrayList list = new ArrayList(filteredEvents);
                    FilterMembers(typeof(EventDescriptor), list, attributes);
                    EventDescriptor[] temp = new EventDescriptor[list.Count];
                    list.CopyTo(temp, 0);
                    filteredEvents = new EventDescriptorCollection(temp, true);
                }
                
                return filteredEvents;
            }

            /// <devdoc> 
            ///     Retrieves the filtered set of extended properties that this specific component receives.  These 
            ///     extended properties wrap both the base extender info and the extender provider to make it appear as a 
            ///     regular property.  The filtering is based on the given attributes. 
            ///     For an extended property to satisfy a particular attribute, the attribute must be 
            ///     present in the extended property's attribute list, or the attribute must match it's 
            ///     own default. The array is sorted based on the sort property. 
            /// </devdoc> 
            private ICollection GetExtendedProperties(IComponent comp, IExtenderProvider[] providers) {
                Type componentType = comp.GetType();
                
                Debug.WriteLineIf(CompDescrSwitch.TraceVerbose, "\tFound " + providers.Length.ToString(CultureInfo.InvariantCulture) + " providers");
                IList list = new ArrayList();

                for (int i = 0; i < providers.Length; i++) {

                    if (!providers[i].CanExtend(comp)) {
                        continue;
                    }

                    IExtenderProvider provider = providers[i];
                    Type providerType = provider.GetType();
                    ComponentEntry providerCompInfo = DebugTypeDescriptor.GetEntry(comp, providerType);

                    IList providerWrappedExtenders = providerCompInfo.GetWrappedExtenders(provider);
                    Debug.WriteLineIf(CompDescrSwitch.TraceVerbose, "\twrapped extenders: " + providerWrappedExtenders.Count.ToString(CultureInfo.InvariantCulture));

                    for (int j = 0; j < providerWrappedExtenders.Count; j++) {
                        PropertyDescriptor currentWrappedExtender = (PropertyDescriptor)providerWrappedExtenders[j];
                        Type receiverType = null;

                        ExtenderProvidedPropertyAttribute eppa = (ExtenderProvidedPropertyAttribute)currentWrappedExtender.Attributes[typeof(ExtenderProvidedPropertyAttribute)];
                        if (eppa.ReceiverType != null) {
                            receiverType = eppa.ReceiverType;
                        }

                        if (receiverType != null && receiverType.IsAssignableFrom(componentType)) {
                            list.Add(currentWrappedExtender);
                        }
                    }
                }
                return list;
            }
            
            /// <devdoc>
            ///     This method is invoked during property filtering when a name
            ///     collision is encountered between two properties.  This returns
            ///     a suffix that can be appended to the property name to make
            ///     it unique.  This will first attempt ot use the name of the
            ///     extender.  Failing that it will fall back to a static
            ///     index that is continually incremented.
            /// </devdoc>
            private string GetExtenderCollisionSuffix(PropertyDescriptor prop) {
                string suffix = null;
                
                ExtenderProvidedPropertyAttribute exAttr = (ExtenderProvidedPropertyAttribute)prop.Attributes[typeof(ExtenderProvidedPropertyAttribute)];
                if (exAttr != null) {
                    IExtenderProvider prov = exAttr.Provider;
                    
                    if (prov != null) {
                        string name = null;
                        
                        if (prov is IComponent) {
                            ISite site = ((IComponent)prov).Site;
                            if (site != null) {
                                name = site.Name;
                            }
                        }
                        
                        if (name == null) {
                            int ci = System.Threading.Interlocked.Increment(ref collisionIndex) - 1;
                            name = ci.ToString(CultureInfo.InvariantCulture);
                        }
                        
                        suffix = "_" + name;
                    }
                }
                
                return suffix;
            }

            /// <devdoc> 
            ///      Retrieves the set of extender providers providing services for the given component. 
            /// </devdoc> 
            private IExtenderProvider[] GetExtenderProviders(ISite site) {
                // See if this component's site has an IExtenderListService.  If it
                // does, we get our list of extenders from that, not from the container.
                //
                IExtenderListService listService = (IExtenderListService)site.GetService(typeof(IExtenderListService));

                if (listService != null) {
                    return listService.GetExtenderProviders();
                }
                else {
                    ComponentCollection comps = site.Container.Components;
                    ArrayList exList = null;
                    foreach(IComponent comp in comps) {
                        if (comp is IExtenderProvider) {
                            if (exList == null) {
                                exList = new ArrayList(2);
                            }
                            exList.Add(comp);
                        }
                    }
                    if (exList == null) {
                        return null;
                    }
                    else {
                        IExtenderProvider[] temp = new IExtenderProvider[exList.Count];
                        exList.CopyTo(temp, 0);
                        return temp;
                    }
                }
            }

            /// <devdoc> 
            ///     Retrieves base extenders that this type of component provides that satisfy all of the 
            ///     passed-in attributes. For an extender to satisfy a particular attribute, the attribute 
            ///     must be present in the extender's attribute list, or the attribute must match it's 
            ///     own default.  The array is sorted based on the sort parameter. 
            /// </devdoc> 
            public IList GetExtenders() {
                if (this.extenders == null) {
                    this.extenders = new MemberList(this).GetExtenders();
                }
                
                return extenders;
            }

            /// <devdoc> 
            ///     Retrieves properties that satisfy all of the passed-in attributes. 
            ///     For a property to satisfy a particular attribute, the attribute must be 
            ///     present in the property's attribute list, or the attribute must match it's 
            ///     own default.  The array is sorted based on the sort parameter. 
            /// </devdoc> 
            public PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes) {
                return GetProperties(component, attributes, false);
            }

            /// <devdoc> 
            ///     Retrieves properties that satisfy all of the passed-in attributes. 
            ///     For a property to satisfy a particular attribute, the attribute must be 
            ///     present in the property's attribute list, or the attribute must match it's 
            ///     own default.  The array is sorted based on the sort parameter. 
            /// </devdoc> 
            public PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes, bool noFilter) {
                if (this.properties == null) {
                    this.properties = new PropertyDescriptorCollection(new MemberList(this).GetProperties(), true);
                }
                
                // We will use this to reconstruct the set of properties in case we have extenders, etc.
                //
                ArrayList allProps = null;
                PropertyStash propStash = null;
                bool inDictionary = false;
                
                // This will be what we finally return.  we default to the current static set of 
                // properties.
                //
                PropertyDescriptorCollection filteredProperties = properties;
                
                if (component is IComponent) {
                    ISite site = ((IComponent)component).Site;
                    IExtenderProvider[] providers;
                    
                    if (site != null) {
                        providers = GetExtenderProviders(site);
                    }
                    else {
                        providers = null;
                    }
                
                    ITypeDescriptorFilterService tf = null;
                    if (!noFilter)
                        tf = (ITypeDescriptorFilterService)GetService(component, typeof(ITypeDescriptorFilterService));
                    
                    // First, check to see if we found the properties in the dictionary service.  If so, we
                    // can fall out immediately because we have already stashed them.
                    //
                    IDictionaryService ds = (IDictionaryService)GetService(component, typeof(IDictionaryService));
                    if (ds != null) {
                        object filterStash = null;
                        
                        lock(ds) {
                            propStash = (PropertyStash)ds.GetValue(typeof(PropertyStash));
                            filterStash = ds.GetValue(typeof(ITypeDescriptorFilterService));
                        }
                            
                        // Check that the filter that was used to create these attributes is the same
                        // filter we currently have.  People may replace the filter, and if we do 
                        // we must refresh the cache.
                        //
                        if ((filterStash == null || filterStash == tf) && propStash != null && propStash.ExtendersMatch(providers, component)) {
                            // Now check to see if the stashed array of attributes also matches.  If
                            // it does, then we can return a stashed pre-filtered set of props.
                            //
                            if (propStash.AttributesMatch(attributes)) {
                                filteredProperties = propStash.FilteredProperties;
                                attributes = null;
                            }
                            else {
                                allProps = propStash.Properties;
                            }
                            inDictionary = true;
                        }
                    }
                    
                    // Now check to see if the container wants to filter the set of properties.
                    //
                    if (!inDictionary) {
                        if (providers != null && providers.Length > 0) {
                            ICollection extendedProperties = GetExtendedProperties((IComponent)component, providers);
        
                            if (extendedProperties != null && extendedProperties.Count > 0) {
                                allProps = new ArrayList(properties.Count + extendedProperties.Count);
                                allProps.AddRange(properties);
                                allProps.AddRange(extendedProperties);
                            }
                        }
                    
                        if (tf != null) {
                            Hashtable filterTable = new Hashtable(properties.Count);
                            
                            if (allProps == null) {
                                allProps = new ArrayList(properties);
                            }
                            
                            foreach (PropertyDescriptor p in allProps) {
                                // We must handle the case of duplicate property names
                                // because extender providers can provide any arbitrary
                                // name.  Our rule for this is simple:  If we find a
                                // duplicate name, resolve it back to the extender
                                // provider that offered it and append "_" + the
                                // provider name.  If the provider has no name,
                                // then append the object hash code.
                                //
                                if (filterTable.Contains(p.Name)) {
                                    
                                    // First, handle the new property.  Because
                                    // of the order in which we added extended
                                    // properties above, we can be sure that
                                    // the new property is an extender.  We
                                    // cannot be sure that the existing property
                                    // in the table is an extender, so we will 
                                    // have to check.
                                    //
                                    string suffix = GetExtenderCollisionSuffix(p);
                                    Debug.Assert(suffix != null, "Name collision with non-extender property.");
                                    if (suffix != null) {
                                        filterTable[p.Name + suffix] = p;
                                    }
                                    
                                    // Now, handle the original property.
                                    //
                                    PropertyDescriptor origProp = (PropertyDescriptor)filterTable[p.Name];
                                    suffix = GetExtenderCollisionSuffix(origProp);
                                    if (suffix != null) {
                                        filterTable.Remove(p.Name);
                                        filterTable[origProp.Name + suffix] = origProp;
                                    }
                                }
                                else {
                                    filterTable[p.Name] = p;
                                }
                            }
                            
                            bool cache = tf.FilterProperties((IComponent)component, filterTable);
    
                            allProps = new ArrayList(filterTable.Values);
                            
                            if (ds != null && cache) {
                                propStash = new PropertyStash(allProps, providers, component);
                                lock(ds) {
                                    ds.SetValue(typeof(PropertyStash), propStash);
                                    ds.SetValue(typeof(ITypeDescriptorFilterService), tf);
                                }
                                inDictionary = true;
                            }
                        }
                    }
                }
    
                if (attributes != null && attributes.Length > 0) {
                    if (allProps == null) {
                        allProps = new ArrayList(properties);
                    }
                    else if (inDictionary) {
                        // If we just poked this into the dictionary, we must clone.  Otherwise
                        // we will disrupt the dictionary.
                        //
                        allProps = new ArrayList(allProps);
                        inDictionary = false;
                    }
                    FilterMembers(typeof(PropertyDescriptor), allProps, attributes);
                }
                
                if (allProps != null) {

                    PropertyDescriptor[] temp = new PropertyDescriptor[allProps.Count];
                    allProps.CopyTo(temp, 0);
                    filteredProperties = new PropertyDescriptorCollection(temp, true);
                    if (propStash != null) {
                        propStash.FilteredProperties = filteredProperties;
                        propStash.Attributes = attributes;
                    }
                }
                
                return filteredProperties;
            }

           internal PropertyDescriptorCollection FilterProperties(object component, Attribute[] attributes, PropertyDescriptorCollection properties) {
               PropertyDescriptorCollection filteredProperties = properties;
            
               ArrayList allProps = null;

                if (component is IComponent) {
                    ITypeDescriptorFilterService tf = (ITypeDescriptorFilterService)GetService(component, typeof(ITypeDescriptorFilterService));
                    
                    if (tf != null) {
                        Hashtable filterTable = new Hashtable(properties.Count);

                        foreach(PropertyDescriptor p in properties) {
                            string suffix = GetExtenderCollisionSuffix(p);
                            if (suffix == null)
                                suffix = "";

                            Debug.Assert(!filterTable.Contains(p.Name + suffix), "Overwriting PropertyDescriptor during filtering!!! " + p.Name + suffix);
                            filterTable[p.Name + suffix] = p;
                        }
                        
                        tf.FilterProperties((IComponent)component, filterTable);
                        allProps = new ArrayList(filterTable.Values);
                    }
                }
            
                if (attributes != null && attributes.Length > 0) {
                    if (allProps == null) {
                        allProps = new ArrayList(properties);
                    }
                    FilterMembers(typeof(PropertyDescriptor), allProps, attributes);
                }
                
                if (allProps != null) {
                    PropertyDescriptor[] temp = new PropertyDescriptor[allProps.Count];
                    allProps.CopyTo(temp, 0);
                    filteredProperties = new PropertyDescriptorCollection(temp, true);
                }
                
                return filteredProperties;
            }
            
            private object GetService(object component, Type type) {
                if (component == null) {
                    throw new ArgumentNullException("component");
                }

                if (component is IComponent) 
                {
                    ISite site = ((IComponent)component).Site;
                    if (site != null) {
                        return site.GetService(type);
                    }
                }
                return null;
            }
           
            /// <devdoc>
            ///       Retrieves a type from a name.  The Assembly of the type
            ///        that this PropertyDescriptor came from is first checked,
            ///        then a global Type.GetType is performed.
            /// </devdoc>
            protected Type GetTypeFromName(string typeName) {
                  
                if (typeName == null || typeName.Length == 0) {
                     return null;
                }
            
                int commaIndex = typeName.IndexOf(',');
                Type t = null;
                
                if (commaIndex == -1) {
                    t = componentType.Assembly.GetType(typeName);
                }
                
                if (t == null) {
                    t = Type.GetType(typeName);
                }
                
                return t;
            }
            
            /// <devdoc> 
            ///     Retrieves filtered, wrapped extended properties based on the provider and the extenderInfos for this type of provider. 
            ///     These are the extended properties that will be handed out to components through 
            ///     getExtendedProperties and getMergedProperties.  These are wrapped versions of what is returned 
            ///     through getExtenders. They are filtered based on the attributes. They are sorted 
            ///     based on the sort property. 
            /// </devdoc> 
            private IList GetWrappedExtenders(IExtenderProvider provider) {
                Debug.WriteLineIf(CompDescrSwitch.TraceVerbose, "GetWrappedExtenders");
                if (wrappedExtenderTable == null) {
                    wrappedExtenderTable = new Hashtable();
                }

                if (provider == null) {
                    return null;
                }

                Type providerType = ((object)provider).GetType();
                if (!componentType.IsAssignableFrom(providerType)) {
                    throw new ArgumentException(SR.GetString(SR.ErrorBadExtenderType, providerType.Name, componentType.Name), "provider");
                }

                bool sitedProvider = false;

                if (provider is IComponent && ((IComponent)provider).Site != null) {
                    sitedProvider = true;
                }

                IList wrappedExtenders = null;
                
                lock(this) {
                    wrappedExtenders = (IList) wrappedExtenderTable[provider];
                    
                    if (wrappedExtenders == null) {
                        Debug.WriteLineIf(CompDescrSwitch.TraceVerbose, "\tentry not found in table... creating");
                        IList extenders = GetExtenders();
                        Debug.WriteLineIf(CompDescrSwitch.TraceVerbose, "\tfound " + extenders.Count.ToString(CultureInfo.InvariantCulture) + " extenders");
                        wrappedExtenders = new ArrayList(extenders.Count);
                        for (int i = 0; i < extenders.Count; i++) {
                            DebugReflectPropertyDescriptor ex = (DebugReflectPropertyDescriptor)extenders[i];
                            Attribute[] attrs = null;
    
                            // If the extender provider is not a sited component, then mark all
                            // of its properties as design time only, since there is no way we
                            // can persist them.  We can assume this because this is only called if
                            // we could get the extender provider list from a component site.
                            //
                            if (!sitedProvider) {
                                attrs = new Attribute[] {DesignOnlyAttribute.Yes};
                            }
    
                            wrappedExtenders.Add(new DebugExtendedPropertyDescriptor(ex, ex.ExtenderGetReceiverType(), provider, attrs));
                        }
                        wrappedExtenderTable[provider] = wrappedExtenders;
                    }
                    else {
                        Debug.WriteLineIf(CompDescrSwitch.TraceVerbose, "\tentry found in table...");
                    }
                }

                return wrappedExtenders;
            }

            /// <devdoc> 
            ///      Searches the provided intrinsic hashtable for a match with the compnent type. 
            ///      At the beginning, the hashtable contains types for the various converters. 
            ///      As this table is searched, the types for these objects 
            ///      are replaced with instances, so we only create as needed.  This method 
            ///      does the search up the base class hierarchy and will create instances 
            ///      for types as needed.  These instances are stored back into the table 
            ///      for the base type, and for the original component type, for fast access. 
            /// </devdoc> 
            private object SearchIntrinsicTable(Hashtable table) {

                object hashEntry = null;
                Type baseType = componentType;
                
                // We take a lock on this table.  Nothing in this code calls out to
                // other methods that lock, so it should be fairly safe to grab this
                // lock.  Also, this allows multiple intrinsic tables to be searched
                // at once.
                //
                lock(table) {
                    // Here is where we put all special case logic for various intrinsics
                    // that cannot be directly mapped to a type.
                    //
                    if (componentType.IsCOMObject) {
                        // placeholder for COM converters, etc.
                    }
                    else {
                        if (componentType.IsEnum) {
                            hashEntry = table[intrinsicEnumKey];
                        }
                    }
    
                    // Normal path -- start traversing up the class hierarchy
                    // looking for matches.
                    //
                    if (hashEntry == null) {
                        while (baseType != null && baseType != typeof(object)) {
                            hashEntry = table[baseType];
                        
                            // If the entry is a late-bound type, then try to
                            // resolve it.
                            //
                            if (hashEntry is string) {
                                hashEntry = Type.GetType((string)hashEntry);
                                if (hashEntry != null) {
                                    table[baseType] = hashEntry;
                                }
                            }
                            
                            if (hashEntry != null) {
                                break;
                            }
                            
                            baseType = baseType.BaseType;
                        }
                    }
                    
                    // Now make a scan through each value in the table, looking for interfaces.
                    // If we find one, see if the object implements the interface.
                    //
                    if (hashEntry == null) {
                    
                        foreach (object current in table.Keys) {
                            if (current is Type) {
                                Type keyType = (Type)current;
                                if (keyType.IsInterface && keyType.IsAssignableFrom(componentType)) {
                                    hashEntry = table[keyType];
                                    
                                    if (hashEntry is string) {
                                        hashEntry = Type.GetType((string)hashEntry);
                                        if (hashEntry != null) {
                                            table[componentType] = hashEntry;
                                        }
                                    }
                                    
                                    if (hashEntry != null) {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    
                    // Finally, check to see if the component type is some unknown interface.
                    // We have a custom converter for that.
                    //
                    if (hashEntry == null && componentType.IsInterface) {
                        hashEntry = table[intrinsicReferenceKey];
                    }
                        
                    // Interfaces do not derive from object, so we
                    // must handle the case of no hash entry here.
                    //
                    if (hashEntry == null) {
                        hashEntry = table[typeof(object)];
                    }
                    
                    // If the entry is a type, create an instance of it and then
                    // replace the entry.  This way we only need to create once.
                    // We can only do this if the object doesn't want a type
                    // in its constructor.
                    //
                    if (hashEntry is Type) {
                        Type type = (Type)hashEntry;
                        hashEntry = CreateInstance(type);
                        
                        if (type.GetConstructor(typeConstructor) == null) {
                            table[componentType] = hashEntry;
                        }
                    }
                }

                return hashEntry;
            }

            /// <devdoc> 
            ///     This function takes an info and an attribute and determines whether 
            ///     the info satisfies the particular attribute.  This either means that the info 
            ///     contains the attribute or the info does not contain the attribute and the default 
            ///     for the attribute matches the passed in attribute. 
            /// </devdoc> 
            /// <internalonly/> 
            internal static bool ShouldHideMember(MemberDescriptor info, Attribute attribute) {
                if (info == null || attribute == null) {
                    return true;
                }
                Attribute infoAttribute = info.Attributes[attribute.GetType()];
                if (infoAttribute == null)
                    return !attribute.IsDefaultAttribute();
                else {
                    return !(attribute.Match(infoAttribute));
                }
            }

            /// <devdoc> 
            /// </devdoc> 
            /// <internalonly/> 
            private sealed class MemberList {
                private Hashtable memberHash = new Hashtable();
                private ComponentEntry owner;

                public MemberList(ComponentEntry owner) {
                    this.owner = owner;
                }
                
                public void AddMember(object member) {
                    if (member is MemberDescriptor) {
                        memberHash[((MemberDescriptor)member).Name] = member;
                    }
                    else if (member is Attribute) {
                        object id = ((Attribute)member).TypeId;
                        memberHash[id] = member;
                    }
                    else {
                        memberHash[member.GetType()] = member;
                    }
                }

                internal Attribute[] GetAttributes() {
                    Attribute[] ret = null;
                    
                    memberHash.Clear();

                    Debug.WriteLineIf(CompDescrSwitch.TraceVerbose, "Getting attributes from " + owner.componentType.FullName);
                    
                    // Before we reflect on the type attributes, do the same for the type's interfaces, if it implements any.
                    // We add these in reflection order.
                    //
                    Type[] interfaces = owner.componentType.GetInterfaces();
                    foreach(Type iface in interfaces) {
                        // only do this for public interfaces
                        //
                        if ((iface.Attributes & (TypeAttributes.Public | TypeAttributes.NestedPublic)) != 0) {
                            ReflectGetCustomAttributes(iface, typeof(Attribute));
                        }
                    }
                    
                    // Now do the actual type.  Last one in wins.
                    //
                    ReflectGetCustomAttributes(owner.componentType, typeof(Attribute));

                    // NOTE : You cannot combine designer attributes with the base class attributes.
                    // To find the designer you must get the attributes on this class, thus causing
                    // an infinite recursion... don't do that!
                    //
                    ret = new Attribute[memberHash.Values.Count];
                    memberHash.Values.CopyTo(ret, 0);

                    return ret;
                }

                /// <devdoc> 
                /// </devdoc> 
                /// <internalonly/> 
                internal EventDescriptor[] GetEvents() {
                    EventDescriptor[] ret = null;

                    memberHash.Clear();

                    ReflectGetEvents(owner.componentType);

                    ret = new EventDescriptor[memberHash.Values.Count];
                    memberHash.Values.CopyTo(ret, 0);
                    
                    return ret;
                }

                /// <devdoc> 
                /// </devdoc> 
                /// <internalonly/> 
                internal IList GetExtenders() {
                    ArrayList ret = null;

                    memberHash.Clear();

                    ReflectGetExtenders(owner.componentType);

                    ret = new ArrayList(memberHash.Values);

                    return ret;
                }

                internal PropertyDescriptor[] GetProperties() {
                    PropertyDescriptor[] ret = null;

                    memberHash.Clear();

                    ReflectGetProperties(owner.componentType);

                    ret = new PropertyDescriptor[memberHash.Values.Count];
                    memberHash.Values.CopyTo(ret, 0);
                    
                    return ret;
                }

                /// <devdoc> 
                ///     Reflects on a class and it's base classes to find any custom 
                ///     attributes of the specified type. The comparison to the metadataType 
                ///     is a not strict comparison and will return derived instances. 
                /// </devdoc> 
                private void ReflectGetCustomAttributes(Type classToReflect, Type metadataType) {
                    AttributeCollection baseAttributes = null;
                    Hashtable           attrHash = new Hashtable();
#if DEBUG
                    if (CompDescrSwitch.TraceVerbose) {
                        Debug.WriteLine("  retrieving metadata for " + classToReflect.FullName);
                        Debug.WriteLine("  classToRefelct: " + classToReflect.FullName);
                        Debug.WriteLine("  metadataType: " + metadataType.FullName);
                    }
#endif

                    // We only want to reflect on one level at a time,
                    // so if we have a base class above object, we get the
                    // attributes for that first.
                    //
                    Type baseType = classToReflect.BaseType;
                    if (baseType != typeof(object) && baseType != null) {
                        baseAttributes = DebugTypeDescriptor.GetAttributes(baseType);
                    }

                    if (baseAttributes != null && baseAttributes.Count > 0) {
                        foreach (Attribute attr in baseAttributes) {
                            Type attrType = attr.GetType();
                            if (metadataType.IsAssignableFrom(attrType)) {
                                attrHash[attr.TypeId] = attr;    
                            }
                        }
                    }

                    // now get our attributes
                    //
                    object[] attributes = DebugTypeDescriptor.GetCustomAttributes(classToReflect);

                    foreach (Attribute attr in attributes) {
                        Type attrType = attr.GetType();
                        if (metadataType.IsAssignableFrom(attrType)) {
                            attrHash[attr.TypeId] = attr;    
                        }
                    }

                    // push the values up to the top level
                    //
                    foreach (Attribute attr in attrHash.Values) {
                        AddMember(attr);
                    }
                }

                /// <devdoc> 
                ///      Reflects on a class to find the events for that class.  This 
                ///      calls AddMember for each new event that it finds. 
                /// </devdoc> 
                private void ReflectGetEvents(Type classToReflect) {
                    Type currentType = classToReflect;
                    Hashtable eventHash = null;
                    
                    EventDescriptorCollection baseTypeEvents = null;


                    // We only want to reflect on one level at a time,
                    // so if we have a base class above object, we get the
                    // properties for that first.
                    //
                    Type baseType = classToReflect.BaseType;
                    if (baseType != typeof(object) && baseType != null) {
                        baseTypeEvents = DebugTypeDescriptor.GetEvents(baseType);
                    }

                    // for this particular type, we get _only_ the properties
                    // declared on that type
                    //
                    EventInfo[] events = classToReflect.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

                    // if we have properties from the base type, stick them into
                    // a hashtable because we'll want to override them with re-declared
                    // properties on this particular type.
                    //
                    if (baseTypeEvents != null && baseTypeEvents.Count > 0) {
                        eventHash = new Hashtable();
                        foreach(EventDescriptor ed in baseTypeEvents) {
                            eventHash[ed.Name] = ed;
                        }
                    }   

                    // now walk each event we got an make sure it's got an add and a remove
                    //
                    foreach(EventInfo eventInfo in events) {
                        if ((!(eventInfo.DeclaringType.IsPublic || eventInfo.DeclaringType.IsNestedPublic)) && (eventInfo.DeclaringType.Assembly == typeof(DebugTypeDescriptor).Assembly)) {
                            continue;
                        }
                        
                        MethodInfo addMethod = eventInfo.GetAddMethod();
                        MethodInfo removeMethod = eventInfo.GetRemoveMethod();
                        bool allGood = addMethod != null && removeMethod != null;
                        
                        // if we have a base list, push the new descriptor
                        // into the hashtable, otherwise just add it directly.
                        //
                        if (eventHash != null) {
                            EventInfo currentEvent = eventInfo;

                            // okay, we have to get tricky here...
                            // if we got an event without an add and remove...which means one is defined on a base class
                            //
                            if (!allGood) {
                                if (eventHash.Contains(eventInfo.Name)) {

                                    // a base class has a property for this,
                                    // so we should just pick up it's
                                    // getter.
                                    //
                                    EventDescriptor basePd = (EventDescriptor)eventHash[eventInfo.Name];
                                    Type declaringType = basePd.ComponentType;
                                    if (declaringType != null) {
                                        EventInfo baseEvent = declaringType.GetEvent(eventInfo.Name);
                                        if (baseEvent != null && baseEvent.GetAddMethod() != null && baseEvent.GetRemoveMethod() != null) {
                                            currentEvent = baseEvent;
                                        }
                                    }
                                }
                            }

                            // push the new info into the hash table.
                            //
                            eventHash[eventInfo.Name] = new DebugReflectEventDescriptor(classToReflect, currentEvent);
                        }
                        else {
                            AddMember(new DebugReflectEventDescriptor(classToReflect, eventInfo));
                        }
                        
                    }

                    // now all the things in the hashtable are our "actual" list
                    // of events, so just set it directly.
                    //
                    if (eventHash != null) {
                        this.memberHash = eventHash;
                    }
                }

                /// <devdoc> 
                ///      Reflects on a class to find the extender properties for that class.  This 
                ///      calls AddMember for each new property that it finds. 
                /// </devdoc> 
                private void ReflectGetExtenders(Type classToReflect) {
                    foreach(Attribute attr in DebugTypeDescriptor.GetAttributes(classToReflect)) {
                        if (attr is ProvidePropertyAttribute) {
                            ProvidePropertyAttribute provided = (ProvidePropertyAttribute)attr;
                            
                            Type receiverType = Type.GetType(provided.ReceiverTypeName);
                            
                            if (receiverType != null) {
                                MethodInfo getMethod = classToReflect.GetMethod("Get" + provided.PropertyName, new Type[] {receiverType});
                                if (getMethod != null && !getMethod.IsStatic && getMethod.IsPublic) {
                                    MethodInfo setMethod = classToReflect.GetMethod("Set" + provided.PropertyName, new Type[] {receiverType, getMethod.ReturnType});

                                    if (setMethod != null && (setMethod.IsStatic || !setMethod.IsPublic)) {
                                        setMethod = null;
                                    }
                                    AddMember(new DebugReflectPropertyDescriptor(classToReflect, provided.PropertyName, getMethod.ReturnType, receiverType, getMethod, setMethod, null));
                                }
                            }
                        }
                    }
                }

                /// <devdoc> 
                ///      Reflects on a class to find the properties for that class.  This 
                ///      calls AddMember for each new property that it finds. 
                /// </devdoc> 
                private void ReflectGetProperties(Type classToReflect) {
                    // CLR gives us a full set of properties for all the base classes, which is what
                    // we want.  Unfortunately, it does not give us the accessor methods for properties
                    // that are only partially overridden.  So, for these, we have to traverse the base
                    // class list.
                    //
                    Type currentType = classToReflect;
                    Hashtable propertyHash = null;
                    
                    PropertyDescriptorCollection baseTypeProps = null;

                    // We only want to reflect on one level at a time,
                    // so if we have a base class above object, we get the
                    // properties for that first.
                    //
                    Type baseType = classToReflect.BaseType;
                    if (baseType != typeof(object) && baseType != null) {
                        baseTypeProps = DebugTypeDescriptor.GetProperties(baseType);
                    }

                    // for this particular type, we get _only_ the properties
                    // declared on that type
                    //
                    PropertyInfo[] props = classToReflect.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

                    // if we have properties from the base type, stick them into
                    // a hashtable because we'll want to override them with re-declared
                    // properties on this particular type.
                    //
                    if (baseTypeProps != null && baseTypeProps.Count > 0) {
                        propertyHash = new Hashtable();
                        foreach(PropertyDescriptor pd in baseTypeProps) {
                            propertyHash[pd.Name] = pd;
                        }
                    }   

                    // now walk each property we got an make sure it's got a getter and a setter
                    //
                    foreach(PropertyInfo pi in props) {
                        if ((!(pi.DeclaringType.IsPublic || pi.DeclaringType.IsNestedPublic)) && (pi.DeclaringType.Assembly == typeof(DebugTypeDescriptor).Assembly)) {
                            continue;
                        }
                        
                        bool addProp = true;
                        MethodInfo getMethod = pi.GetGetMethod();
                        MethodInfo setMethod = pi.GetSetMethod();
                        MethodInfo mi = getMethod;
                        
                        // first just look to see if we have a getter
                        //
                        if (mi != null) {
                            // Ensure the get method has no parameters.
                            //
                            ParameterInfo[] parameters = mi.GetParameters();
                            
                            if (parameters != null && parameters.Length != 0) {
                                addProp = false;
                            }
                        }
                        else {
                            addProp = false;
                        }
                         
                        // build the property descriptor
                        // for this property.
                        //
                        if (addProp && mi != null && !mi.IsStatic && mi.IsPublic) {
                            
                            // if we have a base list, push the new descriptor
                            // into the hashtable, otherwise just add it directly.
                            //
                            if (propertyHash != null) {
                                
                                // okay, we have to get tricky here...
                                // if we got a property without a GetMethod, see if there's one in here we can use
                                //
                                if (getMethod == null) {
                                    if (propertyHash.Contains(pi.Name)) {

                                        // a base class has a property for this,
                                        // so we should just pick up it's
                                        // getter.
                                        //
                                        PropertyDescriptor basePd = (PropertyDescriptor)propertyHash[pi.Name];
                                        Type declaringType = basePd.ComponentType;
                                        if (declaringType != null) {
                                            PropertyInfo baseProp = declaringType.GetProperty(pi.Name, pi.PropertyType);
                                            if (baseProp != null) {
                                                getMethod = baseProp.GetGetMethod();
                                            }
                                            else {
                                                Debug.Fail("Base prop list has '" + pi.Name + "' but reflection didn't find it.");
                                            }
                                        }
                                    }
                                }

                                // push the new info into the hash table.
                                //
                                propertyHash[pi.Name] = new DebugReflectPropertyDescriptor(classToReflect, pi.Name, pi.PropertyType, pi, getMethod, setMethod, null);;
                            }
                            else {
                                AddMember(new DebugReflectPropertyDescriptor(classToReflect, pi.Name, pi.PropertyType, pi, getMethod, setMethod, null));
                            }
                        }
                    }

                    // now all the things in the hashtable are our "actual" list
                    // of properties, so just set it directly.
                    //
                    if (propertyHash != null) {
                        this.memberHash = propertyHash;
                    }
                    
                }
            }


            /* 
               The following code has been removed to fix FXCOP violations.  The code
               is left here incase it needs to be resurrected in the future.

            // This is a private class used to contain events while we hash
            //
            private class EventHashHolder {
                public EventInfo RealEvent;
                public string Name;
                public Type ReflectedType;
                public MethodInfo AddMethod;
                public MethodInfo RemoveMethod;
                
                public EventHashHolder(EventInfo evt) {
                    RealEvent = evt;
                    Name = evt.Name;
                    AddMethod = evt.GetAddMethod();
                    RemoveMethod = evt.GetRemoveMethod();
                    if (AddMethod != null && RemoveMethod != null) {
                        ReflectedType = evt.EventHandlerType;
                    }
                }
                
                public EventHashHolder(string name, Type reflectedType, MethodInfo addMethod, MethodInfo removeMethod) {
                    Name = name;
                    ReflectedType = reflectedType;
                    AddMethod = addMethod;
                    RemoveMethod = removeMethod;
                }
            }
             
            // This is a private class used to contain properties while we hash
            //
            private class PropertyHashHolder {
                public string Name;
                public Type PropertyType;
                public MethodInfo GetMethod;
                public MethodInfo SetMethod;
                public PropertyInfo PropertyInfo;
                
                public PropertyHashHolder(PropertyInfo prop) {
                    Name = prop.Name;
                    PropertyType = prop.PropertyType;
                    GetMethod = prop.GetGetMethod();
                    SetMethod = prop.GetSetMethod();
                    PropertyInfo = prop;
                }
            }
            */
            
            // This is a private class used to stash a set of properties on
            // a component's site so we can quickly recover them.
            //
            private class PropertyStash {
                private long extenderHash;
                public ArrayList Properties;
                public PropertyDescriptorCollection FilteredProperties;
                public Attribute[] Attributes;
                
                public PropertyStash(ArrayList props, IExtenderProvider[] providers, object instance) {
                    Properties = props;
                    extenderHash = HashExtenders(providers, instance);
                }
                
                // This will return true if the current set of attributes match
                // the given set.
                //
                public bool AttributesMatch(Attribute[] attributes) {
                    int ourCount = (Attributes == null ? 0 : Attributes.Length);
                    int count = (attributes == null ? 0 : attributes.Length);
                    
                    if (ourCount != count) {
                        return false;
                    }
                    
                    bool match = true;
                    
                    for (int i = 0; i < ourCount; i++) {
                        for (int j = 0; j < count; j++) {
                            if (!Attributes[i].Match(attributes[j])) {
                                match = false;
                                break;
                            }
                        }
                    }
                    
                    return match;
                }
                
                // This will return true if the given array if extenders is the same
                // as the set that these stashed properties were created with.
                //
                public bool ExtendersMatch(IExtenderProvider[] providers, object instance) {
                    long hash = HashExtenders(providers, instance);
                    return extenderHash == hash;
                }
                
                // This is a simple hashing algorithm that attempts to create
                // a unique number for a given set of extender providers.
                //
                private long HashExtenders(IExtenderProvider[] providers, object instance) {
                    long hash = 0;

                    int count = (providers == null ? 0 : providers.Length);
                    for (int i = 0; i < count; i++) {
                        if (providers[i].CanExtend(instance))
                        {
                            hash += providers[i].GetHashCode();
                        }
                    }
                    return hash;
                }
            }
        }

        class MemberDescriptorComparer : IComparer {
            public static readonly MemberDescriptorComparer Instance = new MemberDescriptorComparer();

            // When we change the comparers to ordinal string comparison, it fails Properties1/Events1: potential breaking change
            [SuppressMessage("Microsoft.Globalization", "CA130:UseOrdinalStringComparison", MessageId="System.String.Compare(System.String,System.String,System.Boolean,System.Globalization.CultureInfo)")]
            public int Compare(object left, object right) {
                return string.Compare(((MemberDescriptor)left).Name, ((MemberDescriptor)right).Name, false, CultureInfo.InvariantCulture);
            }
        }
    }
}
#endif
