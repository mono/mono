// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Diagnostics;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.AttributedModel
{
    internal class AttributedPartCreationInfo : IReflectionPartCreationInfo
    {
        private readonly Type _type;
        private readonly bool _ignoreConstructorImports = false;
        private readonly ICompositionElement _origin;
        private PartCreationPolicyAttribute _partCreationPolicy = null;
        private ConstructorInfo _constructor;
        private IEnumerable<ExportDefinition> _exports;
        private IEnumerable<ImportDefinition> _imports;
        private HashSet<string> _contractNamesOnNonInterfaces;

        public AttributedPartCreationInfo(Type type, PartCreationPolicyAttribute partCreationPolicy, bool ignoreConstructorImports, ICompositionElement origin)
        {
            Assumes.NotNull(type);
            this._type = type;
            this._ignoreConstructorImports = ignoreConstructorImports;
            this._partCreationPolicy = partCreationPolicy;
            this._origin = origin;
        }

        public Type GetPartType()
        {
            return this._type;
        }

        public Lazy<Type> GetLazyPartType()
        {
            return new Lazy<Type>(this.GetPartType, false);
        }

        public ConstructorInfo GetConstructor()
        {
            if (this._constructor == null && !this._ignoreConstructorImports)
            {
                this._constructor = SelectPartConstructor(this._type);
            }
            return this._constructor;
        }

        public IDictionary<string, object> GetMetadata()
        {
            return this._type.GetPartMetadataForType(this.CreationPolicy);
        }

        public IEnumerable<ExportDefinition> GetExports()
        {
            DiscoverExportsAndImports();
            return this._exports;
        }

        public IEnumerable<ImportDefinition> GetImports()
        {
            DiscoverExportsAndImports();
            return this._imports;
        }

        public bool IsDisposalRequired
        {
            get
            {
                return typeof(IDisposable).IsAssignableFrom(this.GetPartType());
            }
        }

        public bool IsPartDiscoverable()
        {
            if (this._type.IsAttributeDefined<PartNotDiscoverableAttribute>())
            {
                CompositionTrace.DefinitionMarkedWithPartNotDiscoverableAttribute(this._type);
                return false;
            }

            if (this._type.ContainsGenericParameters)
            {
                CompositionTrace.DefinitionContainsGenericsParameters(this._type);
                return false;
            }

            if (!HasExports())
            {
                CompositionTrace.DefinitionContainsNoExports(this._type);
                return false;
            }

            return true;
        }

        private bool HasExports()
        {
            return GetExportMembers(this._type).Any() ||
                   GetInheritedExports(this._type).Any();
        }

        string ICompositionElement.DisplayName
        {
            get { return this.GetDisplayName(); }
        }

        ICompositionElement ICompositionElement.Origin
        {
            get { return this._origin; }
        }

        public override string ToString()
        {
            return GetDisplayName();
        }

        private string GetDisplayName()
        {
            return this.GetPartType().GetDisplayName();
        }

        private CreationPolicy CreationPolicy
        {
            get
            {
                if (this._partCreationPolicy == null)
                {
                    this._partCreationPolicy = this._type.GetFirstAttribute<PartCreationPolicyAttribute>() ?? PartCreationPolicyAttribute.Default;
                }
                return this._partCreationPolicy.CreationPolicy;
            }
        }

        private static ConstructorInfo SelectPartConstructor(Type type)
        {
            Assumes.NotNull(type);

            if (type.IsAbstract)
            {
                return null;
            }

            // Only deal with non-static constructors
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            ConstructorInfo[] constructors = type.GetConstructors(flags);

            // Should likely only happen for static or abstract types
            if (constructors.Length == 0)
            {
                return null;
            }

            // Optimize single default constructor.
            if (constructors.Length == 1 && constructors[0].GetParameters().Length == 0)
            {
                return constructors[0];
            }

            // Select the marked constructor if there is exactly one marked
            IEnumerable<ConstructorInfo> importingConstructors = constructors.Where(
                ctor => ctor.IsAttributeDefined<ImportingConstructorAttribute>());

            switch (importingConstructors.GetCardinality())
            {
                case EnumerableCardinality.One:
                    {
                        return importingConstructors.First();
                    }

                case EnumerableCardinality.TwoOrMore:
                    {
                        // Return null, the part will error on instantiation.
                        return null;
                    }
            }

            // If there are no marked constructors then select the default constructor
            IEnumerable<ConstructorInfo> defaultConstructors = constructors.Where(
                ctor => ctor.GetParameters().Length == 0);

            // There should only ever be zero or one default constructors  
            return defaultConstructors.SingleOrDefault();
        }

        private void DiscoverExportsAndImports()
        {
            // NOTE : in most cases both of these will be null or not null at the same time
            // the only situation when that is not the case is when there was a failure during the previous discovery
            // and one of them ended up not being set. In that case we will force the discovery again so that the same exception is thrown.
            if ((this._exports != null) && (this._imports != null))
            {
                return;
            }

            this._exports = GetExportDefinitions();
            this._imports = GetImportDefinitions();
        }

        private IEnumerable<ExportDefinition> GetExportDefinitions()
        {
            List<ExportDefinition> exports = new List<ExportDefinition>();

            this._contractNamesOnNonInterfaces = new HashSet<string>();

            // GetExportMembers should only contain the type itself along with the members declared on it, 
            // it should not contain any base types, members on base types or interfaces on the type.
            foreach (MemberInfo member in GetExportMembers(this._type))
            {
                foreach (ExportAttribute exportAttribute in member.GetAttributes<ExportAttribute>())
                {
                    var attributedExportDefinition = new AttributedExportDefinition(this, member, exportAttribute);

                    if (exportAttribute.GetType() == CompositionServices.InheritedExportAttributeType)
                    {
                        // Any InheritedExports on the type itself are contributed during this pass 
                        // and we need to do the book keeping for those.
                        if (!this._contractNamesOnNonInterfaces.Contains(attributedExportDefinition.ContractName))
                        {
                            exports.Add(new ReflectionMemberExportDefinition(member.ToLazyMember(), attributedExportDefinition, this));
                            this._contractNamesOnNonInterfaces.Add(attributedExportDefinition.ContractName);
                        }
                    }
                    else
                    {
                        exports.Add(new ReflectionMemberExportDefinition(member.ToLazyMember(), attributedExportDefinition, this));
                    }
                }
            }

            // GetInheritedExports should only contain InheritedExports on base types or interfaces.
            // The order of types returned here is important because it is used as a 
            // priority list of which InhertedExport to choose if multiple exists with 
            // the same contract name. Therefore ensure that we always return the types
            // in the hiearchy from most derived to the lowest base type, followed
            // by all the interfaces that this type implements.
            foreach (Type type in GetInheritedExports(this._type))
            {
                foreach (InheritedExportAttribute exportAttribute in type.GetAttributes<InheritedExportAttribute>())
                {
                    var attributedExportDefinition = new AttributedExportDefinition(this, type, exportAttribute);

                    if (!this._contractNamesOnNonInterfaces.Contains(attributedExportDefinition.ContractName))
                    {
                        exports.Add(new ReflectionMemberExportDefinition(type.ToLazyMember(), attributedExportDefinition, this));

                        if (!type.IsInterface)
                        {
                            this._contractNamesOnNonInterfaces.Add(attributedExportDefinition.ContractName);
                        }
                    }
                }
            }

            this._contractNamesOnNonInterfaces = null; // No need to hold this state around any longer

            return exports;
        }

        private IEnumerable<MemberInfo> GetExportMembers(Type type)
        {
            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            // If the type is abstract only find local static exports
            if (type.IsAbstract)
            {
                flags &= ~BindingFlags.Instance;
            }
            else if (IsExport(type))
            {
                yield return type;
            }

            // Walk the fields 
            foreach (var member in type.GetFields(flags))
            {
                if (IsExport(member))
                {
                    yield return member;
                }
            }

            // Walk the properties 
            foreach (var member in type.GetProperties(flags))
            {
                if (IsExport(member))
                {
                    yield return member;
                }
            }

            // Walk the methods 
            foreach (var member in type.GetMethods(flags))
            {
                if (IsExport(member))
                {
                    yield return member;
                }
            }
        }

        private IEnumerable<Type> GetInheritedExports(Type type)
        {
            // If the type is abstract we aren't interested in type level exports
            if (type.IsAbstract)
            {
                yield break;
            }

            // The order of types returned here is important because it is used as a 
            // priority list of which InhertedExport to choose if multiple exists with 
            // the same contract name. Therefore ensure that we always return the types
            // in the hiearchy from most derived to the lowest base type, followed
            // by all the interfaces that this type implements.

            Type currentType = type.BaseType;

            if (currentType == null)
            {
                yield break;
            }
            
            // Stopping at object instead of null to help with performance. It is a noticable performance
            // gain (~5%) if we don't have to try and pull the attributes we know don't exist on object.
            // We also need the null check in case we're passed a type that doesn't live in the runtime context.
            while (currentType != null && currentType != CompositionServices.ObjectType)
            {
                if (IsInheritedExport(currentType))
                {
                    yield return currentType;
                }
                currentType = currentType.BaseType;
            }

            foreach (Type iface in type.GetInterfaces())
            {
                if (IsInheritedExport(iface))
                {
                    yield return iface;
                }
            }
        }

        private static bool IsExport(ICustomAttributeProvider attributeProvider)
        {
            return attributeProvider.IsAttributeDefined<ExportAttribute>(false);
        }

        private static bool IsInheritedExport(ICustomAttributeProvider attributedProvider)
        {
            return attributedProvider.IsAttributeDefined<InheritedExportAttribute>(false);
        }

        private IEnumerable<ImportDefinition> GetImportDefinitions()
        {
            List<ImportDefinition> imports = new List<ImportDefinition>();

            foreach (MemberInfo member in GetImportMembers(this._type))
            {
                ReflectionMemberImportDefinition importDefinition = AttributedModelDiscovery.CreateMemberImportDefinition(member, this);
                imports.Add(importDefinition);
            }

            var constructor = this.GetConstructor();

            if (constructor != null)
            {
                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    ReflectionParameterImportDefinition importDefinition = AttributedModelDiscovery.CreateParameterImportDefinition(parameter, this);
                    imports.Add(importDefinition);
                }
            }

            return imports;
        }

        private IEnumerable<MemberInfo> GetImportMembers(Type type)
        {
            if (type.IsAbstract)
            {
                yield break;
            }

            foreach (MemberInfo member in GetDeclaredOnlyImportMembers(type))
            {
                yield return member;
            }

            // Walk up the type chain until you hit object.
            if (type.BaseType != null)
            {
                Type baseType = type.BaseType;

                // Stopping at object instead of null to help with performance. It is a noticable performance
                // gain (~5%) if we don't have to try and pull the attributes we know don't exist on object.
                // We also need the null check in case we're passed a type that doesn't live in the runtime context.
                while (baseType != null && baseType != CompositionServices.ObjectType)
                {
                    foreach (MemberInfo member in GetDeclaredOnlyImportMembers(baseType))
                    {
                        yield return member;
                    }
                    baseType = baseType.BaseType;
                }
            }
        }

        private IEnumerable<MemberInfo> GetDeclaredOnlyImportMembers(Type type)
        {
            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // Walk the fields 
            foreach (var member in type.GetFields(flags))
            {
                if (IsImport(member))
                {
                    yield return member;
                }
            }

            // Walk the properties 
            foreach (var member in type.GetProperties(flags))
            {
                if (IsImport(member))
                {
                    yield return member;
                }
            }
        }

        private static bool IsImport(ICustomAttributeProvider attributeProvider)
        {
            return attributeProvider.IsAttributeDefined<IAttributedImport>(false);
        }
    }
}
