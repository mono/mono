// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities.Expressions;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xaml;

    public class AssemblyReferenceConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;
            if (stringValue != null)
            {
                AssemblyReference result = new AssemblyReference
                {
                    AssemblyName = new AssemblyName(stringValue)
                };

                XamlSchemaContext schemaContext = GetSchemaContext(context);
                if (schemaContext != null &&
                    schemaContext.ReferenceAssemblies != null &&
                    schemaContext.ReferenceAssemblies.Count > 0)
                {
                    Assembly assembly = ResolveAssembly(result.AssemblyName, schemaContext.ReferenceAssemblies);
                    if (assembly != null)
                    {
                        result.Assembly = assembly;
                    }
                    else
                    {
                        // SchemaContext.ReferenceAssemblies is an exclusive list.
                        // Disallow referencing assemblies that are not included in the list.
                        result = null;
                    }
                }

                return result;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            AssemblyReference reference = value as AssemblyReference;
            if (destinationType == typeof(string) && reference != null)
            {
                if (reference.AssemblyName != null)
                {
                    return reference.AssemblyName.ToString();
                }
                else if (reference.Assembly != null)
                {
                    XamlSchemaContext schemaContext = GetSchemaContext(context);
                    if (schemaContext == null || schemaContext.FullyQualifyAssemblyNamesInClrNamespaces)
                    {
                        return reference.Assembly.FullName;
                    }
                    else
                    {
                        AssemblyName assemblyName = AssemblyReference.GetFastAssemblyName(reference.Assembly);
                        return assemblyName.Name;
                    }
                }
                else
                {
                    return null;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static XamlSchemaContext GetSchemaContext(ITypeDescriptorContext context)
        {
            IXamlSchemaContextProvider provider = context.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider;
            return provider != null ? provider.SchemaContext : null;
        }

        private static Assembly ResolveAssembly(AssemblyName assemblyReference, IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                AssemblyName assemblyName = AssemblyReference.GetFastAssemblyName(assembly);
                if (AssemblyReference.AssemblySatisfiesReference(assemblyName, assemblyReference))
                {
                    return assembly;
                }
            }

            return null;
        }
    }
}
