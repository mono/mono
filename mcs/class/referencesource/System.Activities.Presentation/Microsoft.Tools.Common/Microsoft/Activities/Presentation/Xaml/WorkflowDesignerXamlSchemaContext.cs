// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Xaml;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Xaml;
    using Microsoft.Activities.Presentation;

    class WorkflowDesignerXamlSchemaContext : XamlSchemaContext
    {
        //post fix for xml namespace defined by CLR namespace in local assembly            
        readonly string localAssemblyNsPostfix;
        readonly string localAssemblyNsPostfixNoLeadingSemicolon;
        // Cache of custom XamlTypes
        Dictionary<Type, XamlType> customXamlTypes;
        EditingContext editingContext;
        bool environmentAssembliesLoaded;
        private readonly static FrameworkName CurrentFramework = FrameworkNameConstants.NetFramework45;
        private ResolverCache resolverCache;

        private static List<Type> supportedTypes;
        private static List<Type> conversionRequiredTypes;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static WorkflowDesignerXamlSchemaContext()
        {
            supportedTypes = new List<Type>();
            supportedTypes.Add(typeof(System.Activities.Presentation.Expressions.ExpressionActivityEditor));
            supportedTypes.Add(typeof(System.Activities.Presentation.Annotations.Annotation));

            conversionRequiredTypes = new List<Type>();
            conversionRequiredTypes.Add(typeof(System.Activities.Expressions.TextExpression));
            conversionRequiredTypes.Add(typeof(System.Activities.Expressions.AssemblyReference));
            conversionRequiredTypes.Add(typeof(System.Collections.ObjectModel.Collection<System.Activities.Expressions.AssemblyReference>));
            conversionRequiredTypes.Add(typeof(System.Activities.Debugger.Symbol.DebugSymbol));
            conversionRequiredTypes.Add(typeof(System.Activities.Presentation.ViewState.WorkflowViewState));
            conversionRequiredTypes.Add(typeof(System.Activities.Presentation.ViewState.ViewStateManager));
            conversionRequiredTypes.Add(typeof(System.Activities.Presentation.ViewState.ViewStateData));
        }

        public WorkflowDesignerXamlSchemaContext(string localAssembly) : this(localAssembly, null)
        {
        }

        public WorkflowDesignerXamlSchemaContext(string localAssembly, EditingContext editingContext)
        {
            if (!string.IsNullOrEmpty(localAssembly))
            {
                this.localAssemblyNsPostfix = XamlNamespaceHelper.ClrNamespaceAssemblyField + localAssembly;
                this.localAssemblyNsPostfixNoLeadingSemicolon = localAssemblyNsPostfix.Substring(1);
            }
            this.editingContext = editingContext;
        }

        internal bool ContainsConversionRequiredType { get; set; }

        protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
        {
            if (!string.IsNullOrEmpty(this.localAssemblyNsPostfix)
                && IsClrNamespaceWithNoAssembly(xamlNamespace))
            {
                xamlNamespace = AddLocalAssembly(xamlNamespace);
            }

            var xamlType = base.GetXamlType(xamlNamespace, name, typeArguments);

            if (xamlType == null && environmentAssembliesLoaded == false && editingContext != null)
            {
                // Failed to find the type, this might because the namespace is a custom namespace
                //  and the dependent assembly hasn't been loaded yet. Load all dependent assemblies
                //  and try to load the same xaml type again.
                AssemblyContextControlItem assemblyItem = this.editingContext.Items.GetValue<AssemblyContextControlItem>();
                var environmentAssemblies = assemblyItem.GetEnvironmentAssemblies(null);
                if (assemblyItem.LocalAssemblyName != null)
                {
                    AssemblyContextControlItem.GetAssembly(assemblyItem.LocalAssemblyName, null);
                }

                environmentAssembliesLoaded = true;
                xamlType = base.GetXamlType(xamlNamespace, name, typeArguments);
            }

            if (xamlType == null || xamlType.UnderlyingType == null || this.editingContext == null)
            {
                return xamlType;
            }

            MultiTargetingSupportService multiTargetingService = editingContext.Services.GetService<IMultiTargetingSupportService>() as MultiTargetingSupportService;
            DesignerConfigurationService config = editingContext.Services.GetService<DesignerConfigurationService>();
            if (multiTargetingService == null || config == null)
            {
                return xamlType;
            }

            // do not filter out new types and new properties if targeting to current framework and it's a full SKU
            if (config.TargetFrameworkName.Version == CurrentFramework.Version && config.TargetFrameworkName.IsFullProfile())
            {
                return xamlType;
            }

            // Filter out new types and new properties
            if (this.resolverCache == null)
            {
                this.resolverCache = new ResolverCache();
            }

            if (supportedTypes.Contains(xamlType.UnderlyingType))
            {
                return xamlType;
            }

            // only check if conversion is needed when target framework is less than 4.5
            if (config.TargetFrameworkName.Version < CurrentFramework.Version)
            {
                if (conversionRequiredTypes.Contains(xamlType.UnderlyingType))
                {
                    this.ContainsConversionRequiredType = true;
                    return xamlType;
                }
            }

            ResolverResult resolverResult = this.resolverCache.Lookup(xamlType.UnderlyingType);
            if (resolverResult == null)
            {
                resolverResult = MultiTargetingTypeResolver.Resolve(multiTargetingService, xamlType.UnderlyingType);
                this.resolverCache.Update(xamlType.UnderlyingType, resolverResult);
            }

            return MultiTargetingTypeResolver.GetXamlType(resolverResult, xamlType);
        }

        public override XamlType GetXamlType(Type type)
        {            
            XamlType xamlType = null;
            if (this.customXamlTypes != null && this.customXamlTypes.TryGetValue(type, out xamlType))
            {
                return xamlType;
            }
            bool isCustom = false;
            xamlType = GetCustomType(type);
            if (xamlType != null)
            {
                isCustom = true;
            }
            else
            {
                xamlType = base.GetXamlType(type);
                if (xamlType.GetXamlNamespaces().Any(ns => IsClrNamespaceInLocalAssembly(ns)))
                {
                    isCustom = true;
                    xamlType = new XamlTypeWithExplicitNamespace(xamlType, xamlType.GetXamlNamespaces().Select(ns => IsClrNamespaceInLocalAssembly(ns) ? TrimLocalAssembly(ns) : ns));
                }
            }
            if (isCustom)
            {
                if (this.customXamlTypes == null)
                {
                    this.customXamlTypes = new Dictionary<Type, XamlType>();
                }
                this.customXamlTypes[type] = xamlType;
            }
            return xamlType;
        }

        public override IEnumerable<string> GetAllXamlNamespaces()
        {
            foreach (string ns in base.GetAllXamlNamespaces())
            {
                if (IsClrNamespaceInLocalAssembly(ns))
                {
                    yield return TrimLocalAssembly(ns);
                }
                else
                {
                    yield return ns;
                }
            }
        }

        internal bool HasLocalAssembly
        {
            get { return !string.IsNullOrEmpty(this.localAssemblyNsPostfix); }
        }

        internal string AddLocalAssembly(string ns)
        {
            string result = ns;
            // clr-namespace:X.Y.Z ==> clr-namespace:X.Y.Z;assembly=MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
            if (result[result.Length - 1] != ';')
            {
                result += this.localAssemblyNsPostfix;
            }
            else
            {
                result += this.localAssemblyNsPostfixNoLeadingSemicolon;
            }
            return result;
        }

        internal bool IsClrNamespaceWithNoAssembly(string ns)
        {
            //could be more sophisticated with a RegEx, but let's keep it simple for now
            return ns.StartsWith(XamlNamespaceHelper.ClrNamespacePrefix, StringComparison.OrdinalIgnoreCase) &&
                ns.IndexOf(XamlNamespaceHelper.ClrNamespaceAssemblyField, StringComparison.OrdinalIgnoreCase) == -1;
        }

        internal bool IsClrNamespaceInLocalAssembly(string ns)
        {
            //could be more sophisticated with a RegEx, but let's keep it simple for now

            return !string.IsNullOrEmpty(this.localAssemblyNsPostfix) && ns.EndsWith(this.localAssemblyNsPostfix, StringComparison.OrdinalIgnoreCase);
        }

        internal string TrimLocalAssembly(string ns)
        {
            return string.IsNullOrEmpty(this.localAssemblyNsPostfix) ? ns : ns.Substring(0, ns.Length - this.localAssemblyNsPostfix.Length);
        }

        XamlType GetCustomType(Type type)
        {
            if (type == typeof(DesignerAttribute))
            {
                return new AttributeXamlType<DesignerAttribute, DesignerAttributeInfo>(this);
            }
            if (type == typeof(EditorAttribute))
            {
                return new AttributeXamlType<EditorAttribute, EditorAttributeInfo>(this);
            }
            if (type == typeof(DefaultValueAttribute))
            {
                return new AttributeXamlType<DefaultValueAttribute, DefaultValueAttributeInfo>(this);
            }
            if (type.Namespace == "System.ComponentModel.Composition")
            {
                return GetCustomMefType(type);
            }
#if ERROR_TOLERANT_SUPPORT
            if (ErrorTolerantObjectWriter.IsErrorActivity(type))
            {
                return new ShimAsPublicXamlType(type, this);
            }
#endif
            return null;
        }

        // Avoid loading System.ComponentModel.Composition unless we need it
        [MethodImpl(MethodImplOptions.NoInlining)]
        XamlType GetCustomMefType(Type type)
        {
            if (type == typeof(ImportAttribute))
            {
                return new AttributeXamlType<ImportAttribute, ImportAttributeInfo>(this);
            }
            if (type == typeof(ImportManyAttribute))
            {
                return new AttributeXamlType<ImportAttribute, ImportAttributeInfo>(this);
            }
            return null;
        }
    }
}
