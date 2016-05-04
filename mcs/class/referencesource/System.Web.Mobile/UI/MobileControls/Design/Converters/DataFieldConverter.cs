//------------------------------------------------------------------------------
// <copyright file="DataFieldConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Converters
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Globalization;
    using System.Web.UI.Design;
    using System.Web.UI.MobileControls;
    using System.Security.Permissions;

    /// <include file='doc\DataFieldConverter.uex' path='docs/doc[@for="DataFieldConverter"]/*' />
    /// <devdoc>
    ///    <para>
    ///       Provides design-time support for a component's data field properties.
    ///    </para>
    /// </devdoc>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class DataFieldConverter : TypeConverter 
    {
        private const String _dataMemberPropertyName = "DataMember";
        private const String _dataSourcePropertyName = "DataSource";

        /// <include file='doc\DataFieldConverter.uex' path='docs/doc[@for="DataFieldConverter.DataFieldConverter"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.Web.UI.Design.DataFieldConverter'/>.
        ///    </para>
        /// </devdoc>
        public DataFieldConverter() 
        {
        }

        /// <include file='doc\DataFieldConverter.uex' path='docs/doc[@for="DataFieldConverter.CanConvertFrom"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether this converter can
        ///       convert an object in the given source type to the native type of the converter
        ///       using the context.
        ///    </para>
        /// </devdoc>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        {
            if (sourceType == typeof(string)) 
            {
                return true;
            }
            return false;
        }

        /// <include file='doc\DataFieldConverter.uex' path='docs/doc[@for="DataFieldConverter.ConvertFrom"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Converts the given object to the converter's native type.
        ///    </para>
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        {
            if (value == null) 
            {
                return String.Empty;
            }
            else if (value.GetType() == typeof(string)) 
            {
                return (string)value;
            }
            throw GetConvertFromException(value);
        }

        /// <include file='doc\DataFieldConverter.uex' path='docs/doc[@for="DataFieldConverter.GetStandardValues"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the fields present within the selected data source if information about them is available.
        ///    </para>
        /// </devdoc>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) 
        {
            object[] names = null;
            String dataMember = null;
            bool autoGenerateFields = false;
            bool autoGenerateFieldsSet = false;
            ObjectList objectList = null;
            
            if (context != null) 
            {
                ArrayList list = new ArrayList();

                PropertyDescriptorCollection props = null;

                IComponent component = context.Instance as IComponent;
                if (component is IDeviceSpecificChoiceDesigner)
                {
                    Object owner = ((ChoicePropertyFilter)component).Owner;
                    PropertyDescriptor pd = 
                        ((ICustomTypeDescriptor)component).GetProperties()[_dataMemberPropertyName];
                    Debug.Assert(pd != null, "Cannot get DataMember");

                    if (owner is ObjectList)
                    {
                        autoGenerateFields = ((ObjectList)owner).AutoGenerateFields;
                        autoGenerateFieldsSet = true;
                    }

                    component = ((IDeviceSpecificChoiceDesigner)component).UnderlyingControl;

                    // See if owner already has a DataMember
                    dataMember = (String)pd.GetValue(owner);
                    Debug.Assert(dataMember != null);
                    if (dataMember != null && dataMember.Length == 0)
                    {
                        // Get it from underlying object.
                        dataMember = (String)pd.GetValue(component);
                        Debug.Assert(dataMember != null);
                    }
                }

                if (component != null) 
                {
                    objectList = component as ObjectList;

                    if (objectList != null)
                    {
                        foreach(ObjectListField field in objectList.Fields)
                        {
                            list.Add(field.Name);
                        }

                        if (!autoGenerateFieldsSet)
                        {
                            autoGenerateFields = objectList.AutoGenerateFields;
                        }
                    }

                    if (objectList == null || autoGenerateFields)
                    {
                        ISite componentSite = component.Site;
                        if (componentSite != null) 
                        {
                            IDesignerHost designerHost = (IDesignerHost)componentSite.GetService(typeof(IDesignerHost));
                            if (designerHost != null) 
                            {
                                IDesigner designer = designerHost.GetDesigner(component);

                                if (designer is IDataSourceProvider) 
                                {
                                    IEnumerable dataSource = null;
                                    if (!String.IsNullOrEmpty(dataMember))
                                    {
                                        DataBindingCollection dataBindings = 
                                            ((HtmlControlDesigner)designer).DataBindings;
                                        DataBinding binding = dataBindings[_dataSourcePropertyName];
                                        if (binding != null)
                                        {
                                            dataSource = 
                                                DesignTimeData.GetSelectedDataSource(
                                                component,
                                                binding.Expression,
                                                dataMember);
                                        }
                                    }
                                    else
                                    {
                                        dataSource = 
                                            ((IDataSourceProvider)designer).GetResolvedSelectedDataSource();
                                    }

                                    if (dataSource != null) 
                                    {
                                        props = DesignTimeData.GetDataFields(dataSource);
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (props != null) 
                {
                    foreach (PropertyDescriptor propDesc in props) 
                    {
                        list.Add(propDesc.Name);
                    }
                }

                names = list.ToArray();
                Array.Sort(names);
            }
            return new StandardValuesCollection(names);
        }

        /// <include file='doc\DataFieldConverter.uex' path='docs/doc[@for="DataFieldConverter.GetStandardValuesExclusive"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the collection of standard values returned from
        ///    <see cref='System.ComponentModel.TypeConverter.GetStandardValues'/> is an exclusive 
        ///       list of possible values, using the specified context.
        ///    </para>
        /// </devdoc>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) 
        {
            return false;
        }

        /// <include file='doc\DataFieldConverter.uex' path='docs/doc[@for="DataFieldConverter.GetStandardValuesSupported"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether this object supports a standard set of values
        ///       that can be picked from a list.
        ///    </para>
        /// </devdoc>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) 
        {
            if (context.Instance is IComponent) 
            {
                // We only support the dropdown in single-select mode.
                return true;
            }
            return false;
        }    
    }
}
