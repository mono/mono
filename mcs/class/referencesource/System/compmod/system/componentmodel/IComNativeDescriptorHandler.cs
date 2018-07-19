//------------------------------------------------------------------------------
// <copyright file="IComNativeDescriptorHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------


namespace System.ComponentModel {

    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    
    /// <internalonly/>
    /// <devdoc>
    ///    <para>
    ///       Top level mapping layer between a COM object and TypeDescriptor.
    ///    </para>
    /// </devdoc>
    [Obsolete("This interface has been deprecated. Add a TypeDescriptionProvider to handle type TypeDescriptor.ComObjectType instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
    public interface IComNativeDescriptorHandler {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        AttributeCollection GetAttributes(object component);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        string GetClassName(object component);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        TypeConverter GetConverter(object component);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        EventDescriptor GetDefaultEvent(object component);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        PropertyDescriptor GetDefaultProperty(object component);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        object GetEditor(object component, Type baseEditorType);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        string GetName(object component);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        EventDescriptorCollection GetEvents(object component);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        EventDescriptorCollection GetEvents(object component, Attribute[] attributes);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        object GetPropertyValue(object component, string propertyName, ref bool success);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        object GetPropertyValue(object component, int dispid, ref bool success);
    }
}
