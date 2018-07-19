//------------------------------------------------------------------------------
// <copyright file="MobileDeviceCapabilitiesSectionHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Mobile
{
    using System.Collections;
    using System.Configuration;
    using System.Reflection;
    using System.Xml;
    using System.Security.Permissions;

    /// <include file='doc\MobileDeviceCapabilitiesSectionHandler.uex' path='docs/doc[@for="MobileDeviceCapabilitiesSectionHandler"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileDeviceCapabilitiesSectionHandler : IConfigurationSectionHandler
    {
        // IConfigurationSectionHandler Methods
        /// <include file='doc\MobileDeviceCapabilitiesSectionHandler.uex' path='docs/doc[@for="MobileDeviceCapabilitiesSectionHandler.IConfigurationSectionHandler.Create"]/*' />
        /// <internalonly/>
        Object IConfigurationSectionHandler.Create(Object parent, Object context,
            XmlNode node)
        {
            return Create(parent, context, node);
        }

        protected object Create(Object parent, Object context, XmlNode node) {
            // see ASURT 123738
            if (context == null || context.GetType() != typeof(System.Web.Configuration.HttpConfigurationContext)) {
                return null;
            }
            
            DeviceFilterDictionary currentFilterDictionary;
            
            if(parent == null)
            {
                currentFilterDictionary = new DeviceFilterDictionary();
            }
            else
            {
                currentFilterDictionary = new DeviceFilterDictionary(
                    (DeviceFilterDictionary)parent);
            }

            ConfigurationSectionHelper helper = new ConfigurationSectionHelper();
            foreach(XmlNode child in node.ChildNodes)
            {
                helper.Node = child;

                // skip whitespace and comments
                if(helper.IsWhitespaceOrComment())
                {
                    continue;
                }
                    
                // reject nonelements
                helper.RejectNonElement();

                // handle <filter> tags

                if(child.Name.Equals("filter"))
                {
                    String name = helper.RemoveStringAttribute("name", true);
                    String className = helper.RemoveStringAttribute("type", false);

                    if(className != null)
                    {
                        const String methodAttributeName = "method";
                        String methodName = helper.RemoveStringAttribute(methodAttributeName, false);
                        String capabilityName = helper.RemoveStringAttribute("compare", false);
                        String argumentValue = helper.RemoveStringAttribute("argument", false);

                        helper.CheckForUnrecognizedAttributes();

                        if(className == String.Empty)
                        {
                            throw new
                                ConfigurationErrorsException(SR.GetString(SR.DevCapSect_EmptyClass), child);
                        }

                        if(methodName == null)
                        {
                            throw new
                                ConfigurationErrorsException(SR.GetString(SR.ConfigSect_MissingAttr, methodAttributeName), child);
                        }

                        if(methodName == String.Empty)
                        {
                            throw new
                                ConfigurationErrorsException(SR.GetString(SR.ConfigSect_MissingValue, methodAttributeName), child);
                        }

                        if(capabilityName != null || argumentValue != null)
                        {
                            String msg;
                            if (capabilityName != null)
                            {
                                msg = SR.GetString(SR.DevCapSect_ExtraCompareDelegator);
                            }
                            else
                            {
                                msg = SR.GetString(SR.DevCapSect_ExtraArgumentDelegator);
                            }
                            
                            throw new ConfigurationErrorsException(msg, child);
                        }

                        MobileCapabilities.EvaluateCapabilitiesDelegate evaluator;

                        Type evaluatorClass = Type.GetType(className);

                        if(null == evaluatorClass)
                        {
                            String msg =
                                SR.GetString(SR.DevCapSect_NoTypeInfo, className);
                            throw new ConfigurationErrorsException(msg, child);
                        }

                        try
                        {
                            evaluator =
                                (MobileCapabilities.EvaluateCapabilitiesDelegate)
                                MobileCapabilities.EvaluateCapabilitiesDelegate.CreateDelegate(
                                    typeof(MobileCapabilities.EvaluateCapabilitiesDelegate),
                                    evaluatorClass, methodName);
                        }
                        catch(Exception e)
                        {
                            String msg =
                                SR.GetString(SR.DevCapSect_NoCapabilityEval,
                                             methodName, e.Message);
                            throw new ConfigurationErrorsException(msg, child);
                        }

                        currentFilterDictionary.AddCapabilityDelegate(name, evaluator);
                    }
                    else
                    {
                        String capabilityName = helper.RemoveStringAttribute("compare", false);
                        String argumentValue = helper.RemoveStringAttribute("argument", false);
                        String methodName = helper.RemoveStringAttribute("method", false);                        

                        helper.CheckForUnrecognizedAttributes();

                        if(String.IsNullOrEmpty(capabilityName))
                        {
                            throw new ConfigurationErrorsException(
                                SR.GetString(SR.DevCapSect_MustSpecify),
                                child);
                        }

                        if(methodName != null)
                        {
                            throw new ConfigurationErrorsException(
                                SR.GetString(SR.DevCapSect_ComparisonAlreadySpecified),
                                child);
                        }

                        try
                        {
                            currentFilterDictionary.AddComparisonDelegate(name, capabilityName, argumentValue);
                        }
                        catch(Exception e)
                        {
                            String msg = SR.GetString(SR.DevCapSect_UnableAddDelegate,
                                                      name, e.Message);
                            throw new ConfigurationErrorsException(msg, child);
                        }
                    }                    
                }
                else
                {
                    String msg = SR.GetString(SR.DevCapSect_UnrecognizedTag,
                                              child.Name);
                    throw new ConfigurationErrorsException(msg, child);
                }

                helper.Node = null;
            }
   
            return currentFilterDictionary;
        }
    }
}
