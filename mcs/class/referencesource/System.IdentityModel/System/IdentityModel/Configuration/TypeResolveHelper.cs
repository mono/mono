//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.Configuration;
using System.Reflection;
using System.Runtime;
using System.Xml;

namespace System.IdentityModel.Configuration
{
    class TypeResolveHelper
    {
        public static T Resolve<T>(ConfigurationElementInterceptor customTypeElement, Type customType) where T : class
        {
            if (customTypeElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("customTypeElement");
            }

            if (customType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TypeLoadException(SR.GetString(SR.ID8030, customTypeElement.ElementAsXml.OuterXml)));
            }
            
            try
            {
                if (!typeof(T).IsAssignableFrom(customType))
                {
                    throw DiagnosticUtility.ThrowHelperConfigurationError(
                        customTypeElement,
                        ConfigurationStrings.Type,
                        SR.GetString(SR.ID1029, customType.AssemblyQualifiedName, typeof(T)));
                }

                if (customTypeElement.ElementAsXml != null)
                {
                    //
                    // Remove any non-element children such as comments.
                    //
                    foreach (XmlNode node in customTypeElement.ElementAsXml.ChildNodes)
                    {
                        if (node.NodeType != XmlNodeType.Element)
                        {
                            customTypeElement.ElementAsXml.RemoveChild(node);
                        }
                    }
                }

                T createdObject = (T)Activator.CreateInstance(
                    customType,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                    null,
                    null,
                    null
                    );

                if (customTypeElement.ElementAsXml != null && customTypeElement.ElementAsXml.ChildNodes.Count > 0)
                {
                    ICustomIdentityConfiguration customConfiguration = createdObject as ICustomIdentityConfiguration;
                    if (customConfiguration != null)
                    {
                        customConfiguration.LoadCustomConfiguration(customTypeElement.ElementAsXml.ChildNodes);
                    }
                }

                return createdObject;
            }
            catch (Exception inner)
            {
                if (inner is ConfigurationErrorsException || Fx.IsFatal(inner))
                {
                    throw;
                }
                else if (inner is TargetInvocationException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ID0012, customType.AssemblyQualifiedName), inner));
                }
                else
                {
                    throw DiagnosticUtility.ThrowHelperConfigurationError(customTypeElement, ConfigurationStrings.Type, inner);
                }
            }
        }


    }
}
