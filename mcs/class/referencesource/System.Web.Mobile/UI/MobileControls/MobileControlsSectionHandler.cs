//------------------------------------------------------------------------------
// <copyright file="MobileControlsSectionHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Xml;
using System.Web.Mobile;
using System.Reflection;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /// <include file='doc\MobileControlsSectionHandler.uex' path='docs/doc[@for="MobileControlsSectionHandler"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileControlsSectionHandler : IConfigurationSectionHandler
    {
        // IConfigurationSectionHandler methods
        /// <internalonly/>
        protected Object Create(Object parent, Object context, XmlNode input)
        {
            // see ASURT 123738
            if (context == null || context.GetType() != typeof(System.Web.Configuration.HttpConfigurationContext)) {
                return null;
            }
            
            ControlsConfig config = new ControlsConfig((ControlsConfig)parent);

            // First step through each attribute on the <mobilecontrols> element
            // and update the ControlsConfig dictionary with it.
            XmlAttributeCollection attributes = input.Attributes;
            foreach (XmlNode attribute in attributes)
            {
                config[attribute.Name] = attribute.Value;
            }

            //check validity of cookielessDataDictionary type
            String cookielessDataDictionaryType = config["cookielessDataDictionaryType"];
            if (!String.IsNullOrEmpty(cookielessDataDictionaryType)) {
                Type t = Type.GetType(cookielessDataDictionaryType);
                if (t == null)  
                {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.MobileControlsSectionHandler_TypeNotFound,
                                 cookielessDataDictionaryType,
                                 "IDictionary"),
                        input);
                }
                if (!(typeof(IDictionary).IsAssignableFrom(t)))
                {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.MobileControlsSectionHandler_NotAssignable,
                                     cookielessDataDictionaryType,
                                     "IDictionary"),
                        input);
                }

            }

            // Iterate through each <device> tag within the config section
            ConfigurationSectionHelper helper = new ConfigurationSectionHelper();
            foreach(XmlNode nextNode in input)
            {
                helper.Node = nextNode;

                if(helper.IsWhitespaceOrComment())
                {
                    continue;
                }

                helper.RejectNonElement();
                
                // handle <device> tags
                switch(nextNode.Name)
                {
                case "device":
                    String deviceName = helper.RemoveStringAttribute("name", false);
                    
                    IndividualDeviceConfig idc = CreateDeviceConfig(config, helper, deviceName);

                    helper.CheckForUnrecognizedAttributes();

                    // Iterate through every control adapter
                    // within the <device>
                    foreach(XmlNode currentChild in nextNode.ChildNodes)
                    {
                        helper.Node = currentChild;

                        if(helper.IsWhitespaceOrComment())
                        {
                            continue;
                        }

                        helper.RejectNonElement();
                        
                        if (!currentChild.Name.Equals("control"))
                        {
                            throw new ConfigurationErrorsException(
                                SR.GetString(SR.MobileControlsSectionHandler_UnknownElementName, "<control>"),
                                currentChild);
                        }
                        else
                        {
                            String controlName = helper.RemoveStringAttribute("name", true);
                            String adapterName = helper.RemoveStringAttribute("adapter", true);
                            helper.CheckForUnrecognizedAttributes();

                            idc.AddControl(CheckedGetType(controlName, "control", helper, typeof(MobileControl), currentChild),
                                           CheckedGetType(adapterName, "adapter", helper, typeof(IControlAdapter), currentChild));

                        }

                        helper.Node = null;
                    }

                    // Add complete device config to master configs.
                    if (String.IsNullOrEmpty(deviceName)) {
                        deviceName = Guid.NewGuid().ToString();
                    }
                    
                    if (!config.AddDeviceConfig(deviceName, idc))
                    {
                        // Problem is due to a duplicated name
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.MobileControlsSectionHandler_DuplicatedDeviceName, deviceName),
                            nextNode);
                        
                    }
                    
                    helper.Node = null;
                    break;
                default:
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.MobileControlsSectionHandler_UnknownElementName, "<device>"),
                        nextNode);
                }
            }

            config.FixupDeviceConfigInheritance(input);

            return config;
            
        }

        // Helper to create a device config given the names of methods
        private IndividualDeviceConfig CreateDeviceConfig(ControlsConfig config,
                                                          ConfigurationSectionHelper helper,
                                                          String deviceName)
        {
            String nameOfDeviceToInheritFrom =
                helper.RemoveStringAttribute("inheritsFrom", false);

            if (nameOfDeviceToInheritFrom != null && nameOfDeviceToInheritFrom.Length == 0) {
                nameOfDeviceToInheritFrom = null;
            }
            
            bool propertiesRequired = nameOfDeviceToInheritFrom == null;

            String predicateClass = helper.RemoveStringAttribute("predicateClass", propertiesRequired);
            // If a predicate class is specified, so must a method.
            String predicateMethod = helper.RemoveStringAttribute("predicateMethod", predicateClass != null);
            String pageAdapterClass = helper.RemoveStringAttribute("pageAdapter", propertiesRequired);

            IndividualDeviceConfig.DeviceQualifiesDelegate predicateDelegate = null;
            if (predicateClass != null || predicateMethod != null)
            {
                Type predicateClassType = CheckedGetType(predicateClass, "PredicateClass", helper, null, null);
                try
                {
                    predicateDelegate =
                        (IndividualDeviceConfig.DeviceQualifiesDelegate)
                        IndividualDeviceConfig.DeviceQualifiesDelegate.CreateDelegate(
                            typeof(IndividualDeviceConfig.DeviceQualifiesDelegate),
                            predicateClassType,
                            predicateMethod);
                }
                catch
                {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.MobileControlsSectionHandler_CantCreateMethodOnClass,
                                     predicateMethod, predicateClassType.FullName),
                        helper.Node);
                }
            }
                    
            Type pageAdapterType = null;
            if (pageAdapterClass != null)
            {
                pageAdapterType = CheckedGetType(pageAdapterClass, "PageAdapterClass", helper, typeof(IPageAdapter), null);
            }

            return new IndividualDeviceConfig(config,
                                              deviceName,
                                              predicateDelegate,
                                              pageAdapterType,
                                              nameOfDeviceToInheritFrom);
        }


        // Helper method to encapsulate type lookup followed by
        // throwing a ConfigurationErrorsException on failure.
        private Type CheckedGetType(String typename,
                                    String whereUsed,
                                    ConfigurationSectionHelper helper,
                                    Type typeImplemented,
                                    XmlNode input)
        {
            Type t = Type.GetType(typename);
            if (t == null) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.MobileControlsSectionHandler_TypeNotFound,
                                 typename,
                                 whereUsed),
                    helper.Node);
            }

            if (typeImplemented != null && !typeImplemented.IsAssignableFrom(t)) {
                if (input != null) {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.MobileControlsSectionHandler_NotAssignable,
                                     t,
                                     typeImplemented),
                        input);
                }
                else {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.MobileControlsSectionHandler_NotAssignable,
                                     t,
                                     typeImplemented),
                        helper.Node);
                }
            }

            return t;
        }

        #region IConfigurationSectionHandler implementation
        Object IConfigurationSectionHandler.Create(Object parent, Object context, XmlNode input) {
            return Create(parent, context, input);
        }
        #endregion
    }
}
