//------------------------------------------------------------------------------
// <copyright file="MobileControlPersister.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.MobileControls;
    using System.Web.UI.WebControls;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using AttributeCollection = System.Web.UI.AttributeCollection;
    using System.Globalization;

    /// <summary>
    ///    <para>
    ///       Provides helper functions used in persisting Controls.
    ///    </para>
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class MobileControlPersister 
    {

        /// <summary>
        ///    We don't want instances of this class to be created, so mark
        ///    the constructor as private.
        /// </summary>
        private MobileControlPersister() {
        }

        /// <summary>
        ///    <para>
        ///       Gets the delarative type for the
        ///       specified type.
        ///    </para>
        /// </summary>
        /// <param name='type'>
        ///    The type of the declarator.
        /// </param>
        /// <param name='host'>
        ///    The services interface exposed by the webforms designer.
        /// </param>
        private static string GetDeclarativeType(Type type, IDesignerHost host) {
            Debug.Assert(host != null, "Need an IDesignerHost to create declarative type names");
            string declarativeType = null;

            if (host != null) {
                IWebFormReferenceManager refMgr =
                    (IWebFormReferenceManager)host.GetService(typeof(IWebFormReferenceManager));
                Debug.Assert(refMgr != null, "Did not get back IWebFormReferenceManager service from host.");

                if (refMgr != null) {
                    string tagPrefix = refMgr.GetTagPrefix(type);
                    if ((tagPrefix != null) && (tagPrefix.Length != 0)) {
                        declarativeType = tagPrefix + ":" + type.Name;
                    }
                }
            }

            if (declarativeType == null) 
            {
/* Begin AUI 7201  */
/* Original declarativeType = type.FullName; */
                if (type == typeof(System.Web.UI.MobileControls.Style))
                {
                    declarativeType = type.Name;
                }
                else
                {
                    declarativeType = type.FullName;
                }
/* End AUI 7201 */
            }

            return declarativeType;
        }

        /// <summary>
        ///    <para>
        ///       Persists a collection property.
        ///    </para>
        /// </summary>
        /// <param name='persistMode'>
        ///    The persistance mode to use.
        /// </param>
        /// <param name=' sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' component'>
        ///    The component to persist.
        /// </param>
        /// <param name=' propDesc'>
        ///    A property descriptor for the collection property.
        /// </param>
        /// <param name='host'>
        ///    The services interface exposed by the webforms designer.
        /// </param>
        private static void PersistCollectionProperty(TextWriter sw, object component, PropertyDescriptor propDesc, PersistenceMode persistMode, IDesignerHost host) {
            Debug.Assert(typeof(ICollection).IsAssignableFrom(propDesc.PropertyType),
                "Invalid collection property : " + propDesc.Name);

            ICollection propValue = (ICollection)propDesc.GetValue(component);
            if ((propValue == null) || (propValue.Count == 0))
                return;

// Begin AUI Change #3785
// Original: sw.WriteLine();
            if (!(component is DeviceSpecific))
            {
                sw.WriteLine();
            }
// End of Change #3785

            if (persistMode == PersistenceMode.InnerProperty) {
                sw.Write('<');
                sw.Write(propDesc.Name);
                sw.WriteLine('>');
            }

            IEnumerator e = propValue.GetEnumerator();
            while (e.MoveNext()) {
                object collItem = e.Current;

                // Begin of AUI Change

                //string itemTypeName = GetDeclarativeType(collItem.GetType(), host);
                string itemTypeName;
                
                // AUI : To fix Hashtable objects used in Mobile Controls, only persist the value part
                if (collItem is DictionaryEntry)
                {
                    collItem = ((DictionaryEntry)collItem).Value;
                }
                
                // AUI : First check if the control already has a Default Persist Name,
                //       if not, use the Type as its name

                PersistNameAttribute pna = 
                    (PersistNameAttribute)TypeDescriptor.GetAttributes(collItem.GetType())[typeof(PersistNameAttribute)];

                // attribute should returns default value if it's null.
                // this is unlikely to happen, but just to be on the safe side.
                Debug.Assert (pna != null, "PersistNameAttribute returns null!");

                string persistName = (string)pna.Name;

                if (persistName != null && persistName.Length > 0)
                {
                    itemTypeName = persistName;
                }
/* AUI Change #3911 */
/* Original : else if (collItem is Control) */
                else if (collItem is Control || collItem.GetType() == typeof(System.Web.UI.MobileControls.Style))
                {
                    itemTypeName = GetDeclarativeType(collItem.GetType(), host);
                }
                else
                {
                    itemTypeName = collItem.GetType().Name;
                }
                // End of AUI Change

                sw.Write("<");
                sw.Write(itemTypeName);
                PersistAttributes(sw, collItem, String.Empty, null);
                sw.Write(">");
                
                if (collItem is Control) {
                    PersistChildrenAttribute pca =
                        (PersistChildrenAttribute)TypeDescriptor.GetAttributes(collItem.GetType())[typeof(PersistChildrenAttribute)];

                    if (pca.Persist == true) {
                        // asurt 106696: ensure the parent control's visibility is set to true.
                        Control parentControl = (Control)collItem;
                        if (parentControl.HasControls()) 
                        {
                            bool oldVisible = parentControl.Visible;
                            try 
                            {
                                parentControl.Visible = true;
                                PersistChildControls(sw, parentControl.Controls, host);
                            }
                            finally 
                            {
                                parentControl.Visible = oldVisible;
                            }
                        }
                    }
                    else {
                        PersistInnerProperties(sw, collItem, host);
                    }
                }
                else {
                    PersistInnerProperties(sw, collItem, host);
                }
                
                sw.Write("</");
                sw.Write(itemTypeName);
                sw.WriteLine(">");
            }

            if (persistMode == PersistenceMode.InnerProperty) {
                sw.Write("</");
                sw.Write(propDesc.Name);
                sw.WriteLine('>');
            }
        }

        /// <summary>
        ///    <para>
        ///       Persists a complex property.
        ///    </para>
        /// </summary>
        /// <param name='persistMode'>
        ///    The persistance mode to use.
        /// </param>
        /// <param name=' sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' component'>
        ///    The component to persist.
        /// </param>
        /// <param name=' propDesc'>
        ///    A property descriptor for the complex property.
        /// </param>
        /// <param name='host'>
        ///    The services interface exposed by the webforms designer.
        /// </param>
        private static void PersistComplexProperty(TextWriter sw, object component, PropertyDescriptor propDesc, IDesignerHost host) {
            object propValue = propDesc.GetValue(component);

            if (propValue == null) {
                return;
            }

            StringWriter tagProps = new StringWriter(CultureInfo.InvariantCulture);
            StringWriter innerProps = new StringWriter(CultureInfo.InvariantCulture);

            PersistAttributes(tagProps, propValue, String.Empty, null);
            PersistInnerProperties(innerProps, propValue, host);

            // the rule here is that if a complex property has all its subproperties
            // in the default state, then it itself is in the default state.
            // When this is the case, there shouldn't be any tag properties or inner properties
            if ((tagProps.GetStringBuilder().Length != 0) ||
                (innerProps.GetStringBuilder().Length != 0)) {

                sw.WriteLine();
                sw.Write('<');
                sw.Write(propDesc.Name);
                sw.Write(tagProps.ToString());
                sw.WriteLine(">");

                string innerPropsString = innerProps.ToString();
                sw.Write(innerPropsString);
                if (innerPropsString.Length != 0) {
// Begin AUI Change #3785
// Original: sw.WriteLine();
                    if (!propDesc.Name.Equals("DeviceSpecific"))
                    {
                        sw.WriteLine();
                    }
// End AUI Change #3785
                }

                sw.Write("</");
                sw.Write(propDesc.Name);
                sw.WriteLine('>');
            }
        }

        /// <summary>
        ///    <para>
        ///       Persists the data bindings of the specified control using the specified
        ///       string writer.
        ///    </para>
        /// </summary>
        /// <param name='sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' control'>
        ///    The control to use.
        /// </param>
        private static void PersistDataBindings(TextWriter sw, Control control) {
            DataBindingCollection bindings = ((IDataBindingsAccessor)control).DataBindings;
            IEnumerator bindingEnum = bindings.GetEnumerator();

            while (bindingEnum.MoveNext()) {
                DataBinding db = (DataBinding)bindingEnum.Current;
                string persistPropName = db.PropertyName.Replace('.', '-');

                sw.Write(" ");
                sw.Write(persistPropName);
                sw.Write("='<%# ");
                sw.Write(HttpUtility.HtmlEncode(db.Expression));
                sw.Write(" %>'");
            }
        }

        /// <overload>
        ///    <para>
        ///       Gets a string that can persist the inner properties of a control.
        ///    </para>
        /// </overload>
        /// <summary>
        ///    <para>
        ///       Gets a string that can persist the inner properties of a control.
        ///    </para>
        /// </summary>
        /// <param name='component'>
        ///    The component to persist.
        /// </param>
        /// <param name='host'>
        ///    The services interface exposed by the webforms designer.
        /// </param>
        /// <returns>
        ///    <para>
        ///       A string that contains the persistable information about
        ///       the inner properties
        ///       of the control.
        ///    </para>
        /// </returns>
        internal static string PersistInnerProperties(object component, IDesignerHost host) {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);

            PersistInnerProperties(sw, component, host);
            return sw.ToString();
        }

        /// <summary>
        ///    <para>
        ///       Persists the inner properties of the control.
        ///    </para>
        /// </summary>
        /// <param name='sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' component'>
        ///    The component to persist.
        /// </param>
        /// <param name='host'>
        ///    The services interface exposed by the webforms designer.
        /// </param>
        internal static void PersistInnerProperties(TextWriter sw, object component, IDesignerHost host) {
            PropertyDescriptorCollection propDescs = TypeDescriptor.GetProperties(component);

            for (int i = 0; i < propDescs.Count; i++) {
                // Only deal with inner attributes that need to be persisted
                if (propDescs[i].SerializationVisibility == DesignerSerializationVisibility.Hidden)
                    continue;

                PersistenceModeAttribute persistenceMode = (PersistenceModeAttribute)propDescs[i].Attributes[typeof(PersistenceModeAttribute)];
                if (persistenceMode.Mode == PersistenceMode.Attribute) {
                    continue;
                }
                    
                if (propDescs[i].PropertyType == typeof(string)) {
                    // String based property...
                    
                    DataBindingCollection dataBindings = null;
                    if (component is IDataBindingsAccessor) {
                        dataBindings = ((IDataBindingsAccessor)component).DataBindings;
                    }
                    if (dataBindings == null || dataBindings[propDescs[i].Name] == null) {
                        PersistenceMode mode = persistenceMode.Mode;
                        if ((mode == PersistenceMode.InnerDefaultProperty) ||
                            (mode == PersistenceMode.EncodedInnerDefaultProperty)) {
                            PersistStringProperty(sw, component, propDescs[i], mode);
                        }
                        else {
                            Debug.Fail("Cannot persist inner string property marked with PersistenceMode.InnerProperty");
                        }
                    }
                }
                else if (typeof(ITemplate).IsAssignableFrom(propDescs[i].PropertyType)) {
                    // Template based property...
                    if (persistenceMode.Mode == PersistenceMode.InnerProperty) {
                        PersistTemplateProperty(sw, component, propDescs[i]);
                    }
                    else {
                        Debug.Fail("Cannot persist template property " + propDescs[i].Name + " not marked with PersistenceMode.InnerProperty");
                    }
                }
/* AUI change 03/21/01 */
                else if (propDescs[i].DisplayName.Equals("Templates") &&
                    component is DeviceSpecificChoice &&
                    typeof(IDictionary).IsAssignableFrom(propDescs[i].PropertyType)) {

                    IDictionary templateCollection = (IDictionary)propDescs[i].GetValue(component);
                    foreach (String templateName in templateCollection.Keys) {
                        ITemplate template = (ITemplate)templateCollection[templateName];
                        PersistTemplateProperty(sw, templateName, template);
                    }
                }
/* End of AUI change*/        
                else if (typeof(ICollection).IsAssignableFrom(propDescs[i].PropertyType)) {
                    // Collection based property...
                    if ((persistenceMode.Mode == PersistenceMode.InnerProperty) ||
                        (persistenceMode.Mode == PersistenceMode.InnerDefaultProperty)) {
                        PersistCollectionProperty(sw, component, propDescs[i], persistenceMode.Mode, host);
                    }
                    else {
                        Debug.Fail("Cannot persist collection property " + propDescs[i].Name + " not marked with PersistenceMode.InnerProperty or PersistenceMode.InnerDefaultProperty");
                    }
                }
                else {
                    // Other complex property...
                    if (persistenceMode.Mode == PersistenceMode.InnerProperty) {
                        PersistComplexProperty(sw, component, propDescs[i], host);
                    }
                    else {
                        Debug.Fail("Cannot persist complex property " + propDescs[i].Name + " not marked with PersistenceMode.InnerProperty");
                    }
                }
            }
        }

        /// <summary>
        ///    <para>
        ///       Persists the properties of a
        ///       string.
        ///    </para>
        /// </summary>
        /// <param name='persistMode'>
        ///    The persistance mode to use.
        /// </param>
        /// <param name=' sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' component'>
        ///    The component to persist.
        /// </param>
        /// <param name=' propDesc'>
        ///    A property descriptor for the string properties.
        /// </param>
        private static void PersistStringProperty(TextWriter sw, object component, PropertyDescriptor propDesc, PersistenceMode mode) {
            Debug.Assert(propDesc.PropertyType == typeof(string),
                "Invalid string property : " + propDesc.Name);
            Debug.Assert((mode == PersistenceMode.InnerDefaultProperty) || (mode == PersistenceMode.EncodedInnerDefaultProperty),
                         "Inner string properties must be marked as either InnerDefaultProperty or EncodedInnerDefaultProperty");

            object propValue = propDesc.GetValue(component);
            if (propValue == null) {
                return;
            }

            if (mode == PersistenceMode.InnerDefaultProperty) {
                sw.Write((string)propValue);
            }
            else {
                HttpUtility.HtmlEncode((string)propValue, sw);
            }
        }
        
        /// <summary>
        ///    <para>
        ///       Persists the properties of a tag.
        ///    </para>
        /// </summary>
        /// <param name='sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' component'>
        ///    The component to persist.
        /// </param>
        /// <param name=' prefix'>
        ///    The prefix to store.
        /// </param>
        /// <param name=' propDesc'>
        ///    A property descriptor for the tag properties.
        /// </param>
        private static void PersistAttributes(TextWriter sw, object component, string prefix, PropertyDescriptor propDesc) {
            PropertyDescriptorCollection properties;
            string persistPrefix = String.Empty;
            object value = component;

            if (propDesc != null) {
                value = propDesc.GetValue(component);
                properties = TypeDescriptor.GetProperties(propDesc.PropertyType,
                                                               new Attribute[] {
                                                                   PersistenceModeAttribute.Attribute
                                                               });
            }
            else {
                properties = TypeDescriptor.GetProperties(component,
                                                               new Attribute[] {
                                                                   PersistenceModeAttribute.Attribute
                                                               });
            }

            if (value == null)
                return;

            if (prefix.Length != 0)
                persistPrefix = prefix + "-";

            DataBindingCollection dataBindings = null;
            bool isControl = (component is Control);
            if ((component is IDataBindingsAccessor))
                dataBindings = ((IDataBindingsAccessor)component).DataBindings;

            if (component is DeviceSpecificChoice)
            {
                properties = properties.Sort(new String[] {"Filter"});
            }

            for (int i = 0; i < properties.Count; i++) {
            
                // Skip properties that are hidden to the serializer
                if (properties[i].SerializationVisibility == DesignerSerializationVisibility.Hidden) {
                    continue;
                }
                
                // Skip design-time only properties such as DefaultModifiers and Name
                DesignOnlyAttribute doAttr = (DesignOnlyAttribute)properties[i].Attributes[typeof(DesignOnlyAttribute)];
                if ((doAttr != null) && doAttr.IsDesignOnly) {
                    continue;
                }

                string propName = properties[i].Name;
                Type propType = properties[i].PropertyType;

                object obj = properties[i].GetValue(value);
                if (obj == null)
                    continue;

                DefaultValueAttribute defValAttr =
                    (DefaultValueAttribute)properties[i].Attributes[typeof(DefaultValueAttribute)];
                if ((defValAttr != null) && (obj.Equals(defValAttr.Value)))
                    continue;

                string persistName = propName;
/* AUI Change 3876 -- Change is taken out because of 4347
                if (component is DeviceSpecificChoice && persistName.Equals("Xmlns"))
                {
                    persistName = "xmlns";
                }
   End of Change */

                if (prefix.Length != 0)
                    persistName = persistPrefix + persistName;

                PropertyDescriptorCollection subProps = null;
                if (properties[i].SerializationVisibility == DesignerSerializationVisibility.Content) {
                    subProps = TypeDescriptor.GetProperties(propType);
                }
                if ((subProps == null) || (subProps.Count == 0)) {
                    string persistValue = null;

                    // 
                    DataBinding db = null;
                    if (dataBindings != null)
                        db = dataBindings[persistName.Replace('.', '-')];
                    
                    if (db == null) {
                        if (propType.IsEnum) {
                            persistValue = Enum.Format(propType, obj, "G");
                        }
                        else if (propType == typeof(string)) {
                            persistValue = HttpUtility.HtmlEncode(obj.ToString());
                        }
                        else {
                            TypeConverter converter = properties[i].Converter;
                            if (converter != null) {
                                persistValue = converter.ConvertToInvariantString(null, obj);
                            }
                            else {
                                persistValue = obj.ToString();
                            }
                            persistValue = HttpUtility.HtmlEncode(persistValue);
                        }

                        if ((persistValue == null) ||
                            (persistValue.Equals("NotSet")) ||
                            (propType.IsArray && (persistValue.Length == 0)))
                            continue;

                        sw.Write(" ");
                        sw.Write(persistName);
                        sw.Write("=\"");

                        sw.Write(persistValue);
                        sw.Write("\"");
                    }
                }
                else {
/* 
 *  This will force all ListDictionary properties with DesignerSerializationVisibility.Content atttribute be
 *  persisted as a series of attributes eg. <PropertyFoo Key1="Value1" Key2="Value2" ... />. The 
 *  WebControlPersistor is not able to handle this case and will return undesired results.
 */
// AUI Change to handle DeviceSpecificChoice.Contents
                    if (obj is ListDictionary)
                    {
                        IDictionaryEnumerator enumerator = ((ListDictionary)obj).GetEnumerator ();
                        String persistValue = null;

                        while (enumerator.MoveNext ())
                        {
                            propName = enumerator.Key as String;
                            persistValue = enumerator.Value as String;

                            Debug.Assert (propName != null, 
                                "Non-string key in DeviceSpecificChoice Contents.");

                            if ((propName.Length == 0) ||
                                (persistValue == null))
                                continue;

                            sw.Write(" ");
                            sw.Write(propName);
                            sw.Write("=\"");

                            HttpUtility.HtmlEncode((String)persistValue, sw);
                            sw.Write("\"");                            
                        }
                    }
                    else
                    {
// End of AUI Change

                        // there are sub properties, don't persist this object, but
                        // recursively persist the subproperties.
                        PersistAttributes(sw, obj, persistName, null);
                    }
                }
            }

            // Persist all the databindings on this control
            if (isControl) {
                PersistDataBindings(sw, (Control)component);

                AttributeCollection expandos = null;
                if (component is WebControl) {
                    expandos = ((WebControl)component).Attributes;
                }
                else if (component is HtmlControl) {
                    expandos = ((HtmlControl)component).Attributes;
                }
                else if (component is UserControl) 
                {
                    expandos = ((UserControl)component).Attributes;
                }

                if (expandos != null) {
                    foreach (string key in expandos.Keys) {
                        sw.Write(" ");
                        sw.Write(key);
                        sw.Write("=\"");
                        sw.Write(expandos[key]);
                        sw.Write("\"");
                    }
                }
            }
        }

        /// <summary>
        ///    <para>
        ///       Persists a template property including the specified persistance mode,
        ///       string writer and property descriptor.
        ///    </para>
        /// </summary>
        /// <param name='persistMode'>
        ///    The persistence mode to use.
        /// </param>
        /// <param name=' sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' component'>
        ///    The component to persist.
        /// </param>
        /// <param name=' propDesc'>
        ///    A property descriptor for the property.
        /// </param>
/* AUI change 03/21/01 added support for persisting Template collection */
        private static void PersistTemplateProperty(TextWriter sw, object component, PropertyDescriptor propDesc) 
        {
            Debug.Assert(typeof(ITemplate).IsAssignableFrom(propDesc.PropertyType),
                "Invalid template property : " + propDesc.Name);

            ITemplate template = (ITemplate)propDesc.GetValue(component);
            String templateName = propDesc.Name;
            PersistTemplateProperty(sw, templateName, template);
        }

/* AUI change 03/21/01 made the following code a seperate method */
        private static void PersistTemplateProperty(TextWriter sw, String templateName, ITemplate template)
        {
            if (template == null) {
                return;
            }

            //string templateContent = ((TemplateBuilder)template).Text;
            string templateContent;
            
            Debug.Assert(template is TemplateBuilder, "Unexpected ITemplate implementation.");
            if (template is TemplateBuilder) {
                templateContent = ((TemplateBuilder)template).Text;
            }
            else {
                templateContent = String.Empty;
            }

            sw.WriteLine();
            sw.Write('<');
// changed propDesc.Name to templateName
            sw.Write(templateName);
            sw.Write('>');
            if (!templateContent.StartsWith("\r\n", StringComparison.Ordinal)) {
                sw.WriteLine();
            }
            
            sw.Write(templateContent);

            if (!templateContent.EndsWith("\r\n", StringComparison.Ordinal)) {
                sw.WriteLine();
            }
            sw.Write("</");
// changed propDesc.Name to templateName
            sw.Write(templateName);
            sw.WriteLine('>');
        }

        /// <overload>
        ///    <para>
        ///       Gets a string that can
        ///       persist a control.
        ///    </para>
        /// </overload>
        /// <summary>
        ///    <para>
        ///       Gets a string that can
        ///       persist a control.
        ///    </para>
        /// </summary>
        /// <param name='control'>
        ///    The control to persist.
        /// </param>
        /// <returns>
        ///    <para>
        ///       A string that contains the persistable information about
        ///       the control.
        ///    </para>
        /// </returns>
        internal static string PersistControl(Control control) {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);

            PersistControl(sw, control);
            return sw.ToString();
        }

        /// <overload>
        ///    <para>
        ///       Returns a string that can
        ///       persist a control.
        ///    </para>
        /// </overload>
        /// <summary>
        ///    <para>
        ///       Returns a string that can
        ///       persist a control.
        ///    </para>
        /// </summary>
        /// <param name='control'>
        ///    The control to persist.
        /// </param>
        /// <param name='host'>
        ///    The services interface exposed by the webforms designer.
        /// </param>
        /// <returns>
        ///    <para>
        ///       A string that contains the persistable information about
        ///       the control.
        ///    </para>
        /// </returns>
        internal static string PersistControl(Control control, IDesignerHost host) {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);

            PersistControl(sw, control, host);
            return sw.ToString();
        }

        /// <summary>
        ///    <para>
        ///       Persists a control using the
        ///       specified string writer.
        ///    </para>
        /// </summary>
        /// <param name='sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' control'>
        ///    The control to persist.
        /// </param>
        internal static void PersistControl(TextWriter sw, Control control) {
            if (control is LiteralControl) {
                PersistLiteralControl(sw, (LiteralControl)control);
                return;
            }
            if (control is DesignerDataBoundLiteralControl) {
                PersistDataBoundLiteralControl(sw, (DesignerDataBoundLiteralControl)control);
                return;
            }

            ISite site = control.Site;
            if (site == null) {
                IComponent baseComponent = (IComponent)control.Page;
                Debug.Assert(baseComponent != null, "Control does not have its Page set!");
                if (baseComponent != null) {
                    site = baseComponent.Site;
                }
            }

            IDesignerHost host = null;
            if (site != null) {
                host = (IDesignerHost)site.GetService(typeof(IDesignerHost));
            }

            Debug.Assert(host != null, "Did not get a valid IDesignerHost reference. Expect persistence problems!");

            PersistControl(sw, control, host);
        }

        /// <summary>
        ///    <para>
        ///       Persists a control using the
        ///       specified string writer.
        ///    </para>
        /// </summary>
        /// <param name='sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' control'>
        ///    The control to persist.
        /// </param>
        /// <param name='host'>
        ///    The services interface exposed by the webforms designer.
        /// </param>
        internal static void PersistControl(TextWriter sw, Control control, IDesignerHost host) {
            // Literals and DataBoundLiterals must be handled specially, since they
            // don't have a tag around them
            if (control is LiteralControl) {
                PersistLiteralControl(sw, (LiteralControl)control);
                return;
            }
            if (control is DesignerDataBoundLiteralControl) {
                PersistDataBoundLiteralControl(sw, (DesignerDataBoundLiteralControl)control);
                return;
            }

            Debug.Assert(host != null, "Did not get a valid IDesignerHost reference. Expect persistence problems!");

            string tagName = null;
            bool isUserControl = false;

            if (control is HtmlControl) {
                tagName = ((HtmlControl)control).TagName;
            }
            else if (control is UserControl) 
            {
                tagName = ((IUserControlDesignerAccessor)control).TagName;
                Debug.Assert((tagName != null) && (tagName.Length != 0));
                
                if (tagName.Length == 0) 
                {
                    // not enough information to go any further... no options, other than to throw this control out
                    return;
                }
                
                isUserControl = true;
            }
            else {
                tagName = GetDeclarativeType(control.GetType(), host);
            }

            sw.Write('<');
            sw.Write(tagName);
            sw.Write(" runat=\"server\"");
            PersistAttributes(sw, control, String.Empty, null);
            sw.Write('>');

            if (isUserControl == false) 
            {
                PersistChildrenAttribute pca =
                    (PersistChildrenAttribute)TypeDescriptor.GetAttributes(control.GetType())[typeof(PersistChildrenAttribute)];

                if (pca.Persist == true) 
                {
                    if (control.HasControls()) 
                    {
                        // asurt 106696: Ensure parent control's visibility is true.
                        bool oldVisible = control.Visible;
                        try 
                        {
                            control.Visible = true;
                            PersistChildControls(sw, control.Controls, host);
                        }
                        finally 
                        {
                            control.Visible = oldVisible;
                        }
                    }
                }
                else 
                {
                    // controls marked with LiteralContent == true shouldn't have
                    // children in their persisted form. They only build children
                    // collections at runtime.

                    PersistInnerProperties(sw, control, host);
                }
            }
            else 
            {
                string innerText = ((IUserControlDesignerAccessor)control).InnerText;
                if ((innerText != null) && (innerText.Length != 0)) 
                {
                    sw.Write(innerText);
                }
            }

            sw.Write("</");
            sw.Write(tagName);
            sw.WriteLine('>');
        }

        /// <summary>
        ///    <para>
        ///       Persists the child controls of
        ///       the control using the specified string writer.
        ///    </para>
        /// </summary>
        /// <param name='sw'>
        ///    The string writer to use.
        /// </param>
        /// <param name=' controls'>
        ///    The control collection to persist.
        /// </param>
        /// <param name='host'>
        ///    The services interface exposed by the webforms designer.
        /// </param>
        private static void PersistChildControls(TextWriter sw, ControlCollection controls, IDesignerHost host) {
            int children = controls.Count;
  
            for (int i = 0; i < children; i++) {
                PersistControl(sw, controls[i], host);
            }
        }

        private static void PersistDataBoundLiteralControl(TextWriter sw, DesignerDataBoundLiteralControl control) {
            Debug.Assert(((IDataBindingsAccessor)control).HasDataBindings == true);

            DataBindingCollection bindings = ((IDataBindingsAccessor)control).DataBindings;
            DataBinding textBinding = bindings["Text"];
            Debug.Assert(textBinding != null, "Did not get a Text databinding from DesignerDataBoundLiteralControl");
            
            if (textBinding != null) {
                sw.Write("<%# ");
                sw.Write(textBinding.Expression);
                sw.Write(" %>");
            }
        }

        private static void PersistLiteralControl(TextWriter sw, LiteralControl control) {
            Debug.Assert(control.Text != null);
            sw.Write(control.Text);
        }
    }
}
