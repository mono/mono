// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation
{
    using System.Activities.Expressions;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using Microsoft.VisualBasic.Activities;

    internal static class NamespaceHelper
    {
        private static readonly List<string> BlackListedAssemblies = new List<string>
        {
            typeof(ViewStateService).Assembly.GetName().FullName,
            typeof(ViewStateService).Assembly.GetName().Name,
        };

        internal static IList<string> GetTextExpressionNamespaces(object root, out IList<AssemblyReference> references)
        {
            if (NamespaceHelper.ShouldUsePropertiesForImplementation(root))
            {
                references = TextExpression.GetReferencesForImplementation(root);
                return TextExpression.GetNamespacesForImplementation(root);
            }
            else
            {
                references = TextExpression.GetReferences(root);
                return TextExpression.GetNamespaces(root);
            }
        }

        internal static void SetTextExpressionNamespaces(object root, IList<string> namespaces, IList<AssemblyReference> references)
        {
            if (NamespaceHelper.ShouldUsePropertiesForImplementation(root))
            {
                TextExpression.SetNamespacesForImplementation(root, namespaces);
                TextExpression.SetReferencesForImplementation(root, references);
            }
            else
            {
                TextExpression.SetNamespaces(root, namespaces);
                TextExpression.SetReferences(root, references);
            }
        }

        internal static void SetVisualBasicSettings(object root, VisualBasicSettings settings)
        {
            if (NamespaceHelper.ShouldUsePropertiesForImplementation(root))
            {
                VisualBasic.SetSettingsForImplementation(root, settings);                
            }
            else
            {
                VisualBasic.SetSettings(root, settings);
            }
        }

        internal static void ConvertToTextExpressionImports(VisualBasicSettings settings, out IList<string> importedNamespace, out IList<AssemblyReference> references)
        {
            importedNamespace = new Collection<string>();
            List<string> assemblyNames = new List<string>();
            foreach (VisualBasicImportReference visualbasicImport in settings.ImportReferences)
            {
                if (!BlackListedAssemblies.Contains(visualbasicImport.Assembly))
                {
                    if (importedNamespace.IndexOf(visualbasicImport.Import) == -1)
                    {
                        importedNamespace.Add(visualbasicImport.Import);
                    }

                    string displayName = visualbasicImport.Assembly.Split(',')[0];
                    if (assemblyNames.IndexOf(displayName) == -1)
                    {
                        assemblyNames.Add(displayName);
                    }
                }
            }

            references = new Collection<AssemblyReference>();
            foreach (string assemblyName in assemblyNames)
            {
                AssemblyReference reference = new AssemblyReference
                {
                    AssemblyName = new AssemblyName(assemblyName)
                };

                references.Add(reference);
            }
        }

        internal static void ConvertToVBSettings(IList<string> importedNamespaces, IList<AssemblyReference> references, EditingContext context, out VisualBasicSettings settings)
        {
            Dictionary<string, List<string>> visualBasicImports = new Dictionary<string, List<string>>();
            foreach (string importedNamespace in importedNamespaces)
            {
                visualBasicImports.Add(importedNamespace, new List<string>());
            }

            Collection<Assembly> assemblies = new Collection<Assembly>();
            IMultiTargetingSupportService multiTargetingService = context.Services.GetService<IMultiTargetingSupportService>();
            foreach (AssemblyReference reference in references)
            {
                Assembly assembly;
                if (multiTargetingService == null)
                {
                    reference.LoadAssembly();
                    assembly = reference.Assembly;
                }
                else
                {
                    assembly = AssemblyContextControlItem.GetAssembly(reference.AssemblyName, multiTargetingService);
                }

                if (assembly != null)
                {
                    assemblies.Add(assembly);
                }
            }

            AssemblyContextControlItem assemblyContextItem = context.Items.GetValue<AssemblyContextControlItem>();
            AssemblyName localAssembly = null;
            if (assemblyContextItem != null)
            {
                localAssembly = assemblyContextItem.LocalAssemblyName;
            }

            if (localAssembly != null)
            {
                Assembly assembly = AssemblyContextControlItem.GetAssembly(localAssembly, multiTargetingService);
                if (assembly != null)
                {
                    assemblies.Add(assembly);
                }
            }

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    string ns = type.Namespace;
                    if ((ns != null) && visualBasicImports.ContainsKey(ns))
                    {
                        string assemblyName = assembly.GetName().Name;
                        visualBasicImports[ns].Add(assemblyName);
                    }
                }
            }

            settings = new VisualBasicSettings();
            foreach (KeyValuePair<string, List<string>> entries in visualBasicImports)
            {
                string importedNamespace = entries.Key;
                foreach (string assemblyName in entries.Value)
                {
                    settings.ImportReferences.Add(new VisualBasicImportReference
                    {
                        Import = importedNamespace,
                        Assembly = assemblyName
                    });
                }
            }
        }

        private static bool ShouldUsePropertiesForImplementation(object root)
        {
            if ((root is ActivityBuilder) || (root is DynamicActivity))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
