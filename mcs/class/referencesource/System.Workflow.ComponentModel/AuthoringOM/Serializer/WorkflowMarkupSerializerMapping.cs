namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.IO;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Globalization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Collections.ObjectModel;
    using System.Drawing;

    #region Mapping Support

    #region Mapping
    internal sealed class WorkflowMarkupSerializerMapping
    {
        private static readonly Dictionary<string, Type> wellKnownTypes;
        private static readonly List<WorkflowMarkupSerializerMapping> wellKnownMappings;

        private static readonly WorkflowMarkupSerializerMapping Activities;
        private static readonly WorkflowMarkupSerializerMapping ComponentModel;
        private static readonly WorkflowMarkupSerializerMapping Serialization;
        private static readonly WorkflowMarkupSerializerMapping Rules;
        private static readonly WorkflowMarkupSerializerMapping ComponentModelDesign;

        private string xmlns = String.Empty;
        private string clrns = String.Empty;
        private string targetAssemblyName = String.Empty;
        private string prefix = String.Empty;
        private string unifiedAssemblyName = String.Empty;

        static WorkflowMarkupSerializerMapping()
        {
            WorkflowMarkupSerializerMapping.wellKnownTypes = new Dictionary<string, Type>();
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(ThrowActivity).Name, typeof(ThrowActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(ThrowDesigner).Name, typeof(ThrowDesigner));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(SuspendActivity).Name, typeof(SuspendActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(SuspendDesigner).Name, typeof(SuspendDesigner));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(CancellationHandlerActivity).Name, typeof(CancellationHandlerActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(CancellationHandlerActivityDesigner).Name, typeof(CancellationHandlerActivityDesigner));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(CompensateActivity).Name, typeof(CompensateActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(CompensateDesigner).Name, typeof(CompensateDesigner));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(CompensationHandlerActivity).Name, typeof(CompensationHandlerActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(CompensationHandlerActivityDesigner).Name, typeof(CompensationHandlerActivityDesigner));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(FaultHandlerActivity).Name, typeof(FaultHandlerActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(FaultHandlerActivityDesigner).Name, typeof(FaultHandlerActivityDesigner));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(FaultHandlersActivity).Name, typeof(FaultHandlersActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(FaultHandlersActivityDesigner).Name, typeof(FaultHandlersActivityDesigner));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(SynchronizationScopeActivity).Name, typeof(SynchronizationScopeActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(SequenceDesigner).Name, typeof(SequenceDesigner));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(TransactionScopeActivity).Name, typeof(TransactionScopeActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(TransactionScopeActivityDesigner).Name, typeof(TransactionScopeActivityDesigner));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(PropertySegment).Name, typeof(PropertySegment));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(CompensatableTransactionScopeActivity).Name, typeof(CompensatableTransactionScopeActivity));
            WorkflowMarkupSerializerMapping.wellKnownTypes.Add(typeof(ActivityDesigner).Name, typeof(ActivityDesigner));


            //I am hard coding the well known mappings here instead of going through the assemblies as we want the mappings to be in
            //a specific order for performance optimization when searching for type
            WorkflowMarkupSerializerMapping.wellKnownMappings = new List<WorkflowMarkupSerializerMapping>();

            WorkflowMarkupSerializerMapping.Activities = new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.Activities", AssemblyRef.ActivitiesAssemblyRef);
            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(WorkflowMarkupSerializerMapping.Activities);

            WorkflowMarkupSerializerMapping.ComponentModel = new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.ComponentModel", Assembly.GetExecutingAssembly().FullName);
            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(WorkflowMarkupSerializerMapping.ComponentModel);

            WorkflowMarkupSerializerMapping.Serialization = new WorkflowMarkupSerializerMapping(StandardXomlKeys.Definitions_XmlNs_Prefix, StandardXomlKeys.Definitions_XmlNs, "System.Workflow.ComponentModel.Serialization", Assembly.GetExecutingAssembly().FullName);
            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(WorkflowMarkupSerializerMapping.Serialization);

            WorkflowMarkupSerializerMapping.Rules = new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.Activities.Rules", AssemblyRef.ActivitiesAssemblyRef);
            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(WorkflowMarkupSerializerMapping.Rules);

            WorkflowMarkupSerializerMapping.ComponentModelDesign = new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.ComponentModel.Design", Assembly.GetExecutingAssembly().FullName);
            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(WorkflowMarkupSerializerMapping.ComponentModelDesign);

            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.Runtime", AssemblyRef.RuntimeAssemblyRef));
            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.ComponentModel.Compiler", Assembly.GetExecutingAssembly().FullName));

            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.Activities.Rules.Design", AssemblyRef.ActivitiesAssemblyRef));
            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.Runtime.Configuration", AssemblyRef.RuntimeAssemblyRef));
            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.Runtime.Hosting", AssemblyRef.RuntimeAssemblyRef));
            WorkflowMarkupSerializerMapping.wellKnownMappings.Add(new WorkflowMarkupSerializerMapping(StandardXomlKeys.WorkflowPrefix, StandardXomlKeys.WorkflowXmlNs, "System.Workflow.Runtime.Tracking", AssemblyRef.RuntimeAssemblyRef));
        }

        public WorkflowMarkupSerializerMapping(string prefix, string xmlNamespace, string clrNamespace, string assemblyName)
        {
            if (prefix == null)
                throw new ArgumentNullException("prefix");
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");
            if (clrNamespace == null)
                throw new ArgumentNullException("clrNamespace");
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");

            this.prefix = prefix;
            this.xmlns = xmlNamespace;
            this.clrns = clrNamespace;
            this.targetAssemblyName = assemblyName;
            this.unifiedAssemblyName = assemblyName;
        }

        public WorkflowMarkupSerializerMapping(string prefix, string xmlNamespace, string clrNamespace, string targetAssemblyName, string unifiedAssemblyName)
        {
            if (prefix == null)
                throw new ArgumentNullException("prefix");
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");
            if (clrNamespace == null)
                throw new ArgumentNullException("clrNamespace");
            if (targetAssemblyName == null)
                throw new ArgumentNullException("targetAssemblyName");
            if (unifiedAssemblyName == null)
                throw new ArgumentNullException("unifiedAssemblyName");

            this.prefix = prefix;
            this.xmlns = xmlNamespace;
            this.clrns = clrNamespace;
            this.targetAssemblyName = targetAssemblyName;
            this.unifiedAssemblyName = unifiedAssemblyName;
        }

        public override bool Equals(object value)
        {
            WorkflowMarkupSerializerMapping mapping = value as WorkflowMarkupSerializerMapping;
            if (mapping == null)
            {
                return false;
            }
            //
            // This class is intended to make MT scenarios easier by holding both the target and the unified (current)
            // assembly names.  They both represent the same type in this container and thus the both need to match to be equal.
            // This makes it easier to make this classes default (static constructor) work better with MT in the rest of the codebase.
            if (this.clrns == mapping.clrns &&
                this.targetAssemblyName == mapping.targetAssemblyName &&
                this.unifiedAssemblyName == mapping.unifiedAssemblyName)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (ClrNamespace.GetHashCode() ^ this.unifiedAssemblyName.GetHashCode());
        }

        public string ClrNamespace
        {
            get
            {
                return this.clrns;
            }
        }

        public string XmlNamespace
        {
            get
            {
                return this.xmlns;
            }
        }

        public string AssemblyName
        {
            get
            {
                return this.targetAssemblyName;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        #region Namespace Helpers
        internal static IList<WorkflowMarkupSerializerMapping> WellKnownMappings
        {
            get
            {
                return WorkflowMarkupSerializerMapping.wellKnownMappings;
            }
        }

        internal static Type ResolveWellKnownTypes(WorkflowMarkupSerializationManager manager, string xmlns, string typeName)
        {
            Type resolvedType = null;

            List<WorkflowMarkupSerializerMapping> knownMappings = new List<WorkflowMarkupSerializerMapping>();
            if (xmlns.Equals(StandardXomlKeys.WorkflowXmlNs, StringComparison.Ordinal))
            {
                if (!WorkflowMarkupSerializerMapping.wellKnownTypes.TryGetValue(typeName, out resolvedType))
                {
                    if (typeName.EndsWith("Activity", StringComparison.OrdinalIgnoreCase))
                    {
                        knownMappings.Add(WorkflowMarkupSerializerMapping.Activities);
                        knownMappings.Add(WorkflowMarkupSerializerMapping.ComponentModel);
                    }
                    if (typeName.EndsWith("Designer", StringComparison.OrdinalIgnoreCase))
                    {
                        knownMappings.Add(WorkflowMarkupSerializerMapping.Activities);
                        knownMappings.Add(WorkflowMarkupSerializerMapping.ComponentModel);
                        knownMappings.Add(WorkflowMarkupSerializerMapping.ComponentModelDesign);
                    }
                    else if (typeName.EndsWith("Theme", StringComparison.OrdinalIgnoreCase))
                    {
                        knownMappings.Add(WorkflowMarkupSerializerMapping.ComponentModelDesign);
                        knownMappings.Add(WorkflowMarkupSerializerMapping.Activities);
                    }
                    else if (typeName.StartsWith("Rule", StringComparison.OrdinalIgnoreCase) ||
                        typeName.EndsWith("Action", StringComparison.OrdinalIgnoreCase))
                    {
                        knownMappings.Add(WorkflowMarkupSerializerMapping.Rules);
                    }
                }
            }
            else if (xmlns.Equals(StandardXomlKeys.Definitions_XmlNs, StringComparison.Ordinal))
            {
                knownMappings.Add(WorkflowMarkupSerializerMapping.Serialization);
            }

            if (resolvedType == null)
            {
                foreach (WorkflowMarkupSerializerMapping mapping in knownMappings)
                {
                    string fullyQualifiedTypeName = mapping.ClrNamespace + "." + typeName + ", " + mapping.AssemblyName;
                    resolvedType = manager.GetType(fullyQualifiedTypeName);
                    if (resolvedType != null)
                        break;
                }
            }

            return resolvedType;
        }

        internal static void GetMappingsFromXmlNamespace(WorkflowMarkupSerializationManager serializationManager, string xmlNamespace, out IList<WorkflowMarkupSerializerMapping> matchingMappings, out IList<WorkflowMarkupSerializerMapping> collectedMappings)
        {
            matchingMappings = new List<WorkflowMarkupSerializerMapping>();
            collectedMappings = new List<WorkflowMarkupSerializerMapping>();

            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (reader != null)
            {
                if (xmlNamespace.StartsWith(StandardXomlKeys.CLRNamespaceQualifier, StringComparison.OrdinalIgnoreCase))
                {
                    //Format for the xmlnamespace: clr-namespace:[Namespace][;Assembly=[AssemblyName]]
                    bool invalidXmlnsFormat = false;
                    string clrNamespace = xmlNamespace.Substring(StandardXomlKeys.CLRNamespaceQualifier.Length).Trim();
                    string assemblyName = String.Empty;
                    int index = clrNamespace.IndexOf(';');
                    if (index != -1)
                    {
                        assemblyName = (index + 1 < clrNamespace.Length) ? clrNamespace.Substring(index + 1).Trim() : String.Empty;
                        clrNamespace = clrNamespace.Substring(0, index).Trim();

                        if (!assemblyName.StartsWith(StandardXomlKeys.AssemblyNameQualifier, StringComparison.OrdinalIgnoreCase))
                            invalidXmlnsFormat = true;

                        assemblyName = assemblyName.Substring(StandardXomlKeys.AssemblyNameQualifier.Length);
                    }

                    if (!invalidXmlnsFormat)
                    {
                        if (clrNamespace.Equals(StandardXomlKeys.GlobalNamespace, StringComparison.OrdinalIgnoreCase))
                            clrNamespace = String.Empty;
                        matchingMappings.Add(new WorkflowMarkupSerializerMapping(reader.Prefix, xmlNamespace, clrNamespace, assemblyName));
                    }
                }
                else
                {
                    List<Assembly> referencedAssemblies = new List<Assembly>();
                    if (serializationManager.LocalAssembly != null)
                        referencedAssemblies.Add(serializationManager.LocalAssembly);

                    ITypeProvider typeProvider = serializationManager.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (typeProvider != null)
                        referencedAssemblies.AddRange(typeProvider.ReferencedAssemblies);

                    foreach (Assembly assembly in referencedAssemblies)
                    {
                        object[] xmlnsDefinitions = assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), true);
                        if (xmlnsDefinitions != null)
                        {
                            foreach (XmlnsDefinitionAttribute xmlnsDefinition in xmlnsDefinitions)
                            {
                                string assemblyName = String.Empty;
                                if (serializationManager.LocalAssembly != assembly)
                                {
                                    if (xmlnsDefinition.AssemblyName != null && xmlnsDefinition.AssemblyName.Trim().Length > 0)
                                        assemblyName = xmlnsDefinition.AssemblyName;
                                    else
                                        assemblyName = assembly.FullName;
                                }

                                if (xmlnsDefinition.XmlNamespace.Equals(xmlNamespace, StringComparison.Ordinal))
                                    matchingMappings.Add(new WorkflowMarkupSerializerMapping(reader.Prefix, xmlNamespace, xmlnsDefinition.ClrNamespace, assemblyName));
                                else
                                    collectedMappings.Add(new WorkflowMarkupSerializerMapping(reader.Prefix, xmlNamespace, xmlnsDefinition.ClrNamespace, assemblyName));
                            }
                        }
                    }
                }
            }
        }

        internal static void GetMappingFromType(WorkflowMarkupSerializationManager manager, Type type, out WorkflowMarkupSerializerMapping matchingMapping, out IList<WorkflowMarkupSerializerMapping> collectedMappings)
        {
            matchingMapping = null;
            collectedMappings = new List<WorkflowMarkupSerializerMapping>();

            string clrNamespace = (type.Namespace != null) ? type.Namespace : String.Empty;
            string xmlNamespace = String.Empty;
            string assemblyName = String.Empty;
            string prefix = String.Empty;

            assemblyName = GetAssemblyName(type, manager);

            if (type.Assembly.FullName.Equals(AssemblyRef.RuntimeAssemblyRef, StringComparison.Ordinal))
            {
                xmlNamespace = StandardXomlKeys.WorkflowXmlNs;
                prefix = StandardXomlKeys.WorkflowPrefix;
            }
            if (type.Assembly.FullName.Equals(AssemblyRef.ActivitiesAssemblyRef, StringComparison.Ordinal))
            {
                xmlNamespace = StandardXomlKeys.WorkflowXmlNs;
                prefix = StandardXomlKeys.WorkflowPrefix;
            }
            else if (type.Assembly == Assembly.GetExecutingAssembly())
            {
                xmlNamespace = StandardXomlKeys.WorkflowXmlNs;
                prefix = StandardXomlKeys.WorkflowPrefix;
            }

            if (xmlNamespace.Length == 0)
            {
                //First lookup the type's assembly for XmlNsDefinitionAttribute
                object[] xmlnsDefinitions = type.Assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), true);
                foreach (XmlnsDefinitionAttribute xmlnsDefinition in xmlnsDefinitions)
                {
                    xmlNamespace = xmlnsDefinition.XmlNamespace;
                    assemblyName = xmlnsDefinition.AssemblyName;

                    if (type.Assembly == manager.LocalAssembly)
                        assemblyName = String.Empty;
                    else if (String.IsNullOrEmpty(assemblyName))
                        assemblyName = GetAssemblyName(type, manager);

                    if (String.IsNullOrEmpty(xmlNamespace))
                        xmlNamespace = GetFormatedXmlNamespace(clrNamespace, assemblyName);
                    prefix = GetPrefix(manager, type.Assembly, xmlNamespace);

                    WorkflowMarkupSerializerMapping mapping = new WorkflowMarkupSerializerMapping(prefix, xmlNamespace, clrNamespace, assemblyName, type.Assembly.FullName);
                    if (xmlnsDefinition.ClrNamespace.Equals(clrNamespace, StringComparison.Ordinal) && matchingMapping == null)
                        matchingMapping = mapping;
                    else
                        collectedMappings.Add(mapping);
                }
            }

            if (matchingMapping == null)
            {
                if (type.Assembly == manager.LocalAssembly)
                    assemblyName = String.Empty;
                else if (String.IsNullOrEmpty(assemblyName))
                    assemblyName = GetAssemblyName(type, manager);

                xmlNamespace = GetFormatedXmlNamespace(clrNamespace, assemblyName);

                if (String.IsNullOrEmpty(prefix))
                    prefix = GetPrefix(manager, type.Assembly, xmlNamespace);

                matchingMapping = new WorkflowMarkupSerializerMapping(prefix, xmlNamespace, clrNamespace, assemblyName, type.Assembly.FullName);
            }
        }

        private static string GetAssemblyName(Type type, WorkflowMarkupSerializationManager manager)
        {

            TypeProvider typeProvider = manager.GetService(typeof(ITypeProvider)) as TypeProvider;

            if (typeProvider != null)
            {
                return typeProvider.GetAssemblyName(type);
            }
            //
            // Handle DesignTimeType
            if (type.Assembly == null)
            {
                return string.Empty;
            }
            else
            {
                return type.Assembly.FullName;
            }
        }

        //Format for the xmlnamespace: clr-namespace:[Namespace][;Assembly=[AssemblyName]]
        private static string GetFormatedXmlNamespace(string clrNamespace, string assemblyName)
        {
            string xmlNamespace = StandardXomlKeys.CLRNamespaceQualifier;
            xmlNamespace += (String.IsNullOrEmpty(clrNamespace)) ? StandardXomlKeys.GlobalNamespace : clrNamespace;
            if (!String.IsNullOrEmpty(assemblyName))
                xmlNamespace += ";" + StandardXomlKeys.AssemblyNameQualifier + assemblyName;
            return xmlNamespace;
        }

        private static string GetPrefix(WorkflowMarkupSerializationManager manager, Assembly assembly, string xmlNamespace)
        {
            string prefix = String.Empty;

            object[] xmlnsPrefixes = assembly.GetCustomAttributes(typeof(XmlnsPrefixAttribute), true);
            if (xmlnsPrefixes != null)
            {
                foreach (XmlnsPrefixAttribute xmlnsPrefix in xmlnsPrefixes)
                {
                    if (xmlnsPrefix.XmlNamespace.Equals(xmlNamespace, StringComparison.Ordinal))
                    {
                        prefix = xmlnsPrefix.Prefix;
                        break;
                    }
                }
            }

            if (String.IsNullOrEmpty(prefix) || !IsNamespacePrefixUnique(prefix, manager.PrefixBasedMappings.Keys))
            {
                string basePrefix = (String.IsNullOrEmpty(prefix)) ? "ns" : prefix;

                int index = 0;
                prefix = basePrefix + string.Format(CultureInfo.InvariantCulture, "{0}", index++);
                while (!IsNamespacePrefixUnique(prefix, manager.PrefixBasedMappings.Keys))
                    prefix = basePrefix + string.Format(CultureInfo.InvariantCulture, "{0}", index++);
            }

            return prefix;
        }

        private static bool IsNamespacePrefixUnique(string prefix, ICollection existingPrefixes)
        {
            bool isUnique = true;
            foreach (string existingPrefix in existingPrefixes)
            {
                if (existingPrefix.Equals(prefix, StringComparison.Ordinal))
                {
                    isUnique = false;
                    break;
                }
            }
            return isUnique;
        }
        #endregion
    }
    #endregion

    #endregion

}

