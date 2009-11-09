// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;
using Microsoft.Internal.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.ReflectionModel;

namespace System.ComponentModel.Composition.Hosting
{
    internal static class CompositionServices
    {
        internal static readonly Type InheritedExportAttributeType = typeof(InheritedExportAttribute);
        internal static readonly Type ExportAttributeType = typeof(ExportAttribute);
        internal static readonly Type AttributeType = typeof(Attribute);
        internal static readonly Type ObjectType = typeof(object);

        private static readonly string[] reservedMetadataNames = new string[]
        {
            CompositionConstants.PartCreationPolicyMetadataName
        };  

        internal static Type GetDefaultTypeFromMember(this MemberInfo member)
        {
            Assumes.NotNull(member);

            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;

                case MemberTypes.NestedType:
                case MemberTypes.TypeInfo:
                    return ((Type)member);

                case MemberTypes.Field:
                default:
                    Assumes.IsTrue(member.MemberType == MemberTypes.Field);
                    return ((FieldInfo)member).FieldType;
            }
        }

        internal static string GetContractNameFromExport(this MemberInfo member, ExportAttribute export)
        {
            if (!string.IsNullOrEmpty(export.ContractName))
            {
                return export.ContractName;
            }

            if (export.ContractType != null)
            {
                return AttributedModelServices.GetContractName(export.ContractType);
            }

            if (member.MemberType == MemberTypes.Method)
            {
                return AttributedModelServices.GetTypeIdentity((MethodInfo)member);
            }

            return AttributedModelServices.GetContractName(member.GetDefaultTypeFromMember());
        }

        internal static string GetTypeIdentityFromExport(this MemberInfo member, ExportAttribute export)
        {
            if (export.ContractType != null)
            {
                return AttributedModelServices.GetTypeIdentity(export.ContractType);
            }

            if (member.MemberType == MemberTypes.Method)
            {
                return AttributedModelServices.GetTypeIdentity((MethodInfo)member);
            }

            return AttributedModelServices.GetTypeIdentity(member.GetDefaultTypeFromMember());
        }

        internal static Type GetContractTypeFromImport(this IAttributedImport import, ImportType importType)
        {
            if (import.ContractType != null)
            {
                return import.ContractType;
            }

            return importType.ContractType;
        }

        internal static string GetContractNameFromImport(this IAttributedImport import, ImportType importType)
        {
            if (!string.IsNullOrEmpty(import.ContractName))
            {
                return import.ContractName;
            }

            Type contractType = import.GetContractTypeFromImport(importType);

            return AttributedModelServices.GetContractName(contractType); 
        }

        internal static string GetTypeIdentityFromImport(this IAttributedImport import, ImportType importType)
        {
            Type contractType = import.GetContractTypeFromImport(importType);

            // For our importers we treat object as not having a type identity
            if (contractType == CompositionServices.ObjectType)
            {
                return null;
            }

            return AttributedModelServices.GetTypeIdentity(contractType); 
        }

        internal static IDictionary<string, object> GetPartMetadataForType(this Type type, CreationPolicy creationPolicy)
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>(StringComparers.MetadataKeyNames);

            if (creationPolicy != CreationPolicy.Any)
            {
                dictionary.Add(CompositionConstants.PartCreationPolicyMetadataName, creationPolicy);
            }

            foreach (PartMetadataAttribute partMetadata in type.GetAttributes<PartMetadataAttribute>())
            {
                if (reservedMetadataNames.Contains(partMetadata.Name, StringComparers.MetadataKeyNames) 
                    || dictionary.ContainsKey(partMetadata.Name))
                {
                    // Perhaps we should log an error here so that people know this value is being ignored.
                    continue;
                }

                dictionary.Add(partMetadata.Name, partMetadata.Value);
            }

            if (dictionary.Count == 0)
            {
                return MetadataServices.EmptyMetadata;
            }
            else
            {
                return dictionary;
            }
        }

        internal static void TryExportMetadataForMember(this MemberInfo member, out IDictionary<string, object> dictionary)
        {
            dictionary = new Dictionary<string, object>();

            foreach (var attr in member.GetAttributes<Attribute>())
            {
                var provider = attr as ExportMetadataAttribute;

                if (provider != null)
                {
                    if (reservedMetadataNames.Contains(provider.Name, StringComparers.MetadataKeyNames))
                    {
                        throw ExceptionBuilder.CreateDiscoveryException(Strings.Discovery_ReservedMetadataNameUsed, member.GetDisplayName(), provider.Name);
                    }

                    if (!dictionary.TryContributeMetadataValue(provider.Name, provider.Value, provider.IsMultiple))
                    {
                        throw ExceptionBuilder.CreateDiscoveryException(Strings.Discovery_DuplicateMetadataNameValues, member.GetDisplayName(), provider.Name);
                    }
                }
                else
                {
                    Type attrType = attr.GetType();
                    if ((attrType != CompositionServices.ExportAttributeType) && attrType.IsAttributeDefined<MetadataAttributeAttribute>(true))
                    {
                        bool allowsMultiple = false;
                        AttributeUsageAttribute usage = attrType.GetFirstAttribute<AttributeUsageAttribute>(true);

                        if (usage != null)
                        {
                            allowsMultiple = usage.AllowMultiple;
                        }

                        foreach (PropertyInfo pi in attrType.GetProperties())
                        {
                            if (pi.DeclaringType == CompositionServices.ExportAttributeType || pi.DeclaringType == CompositionServices.AttributeType)
                            {
                                // Don't contribute metadata properies from the base attribute types.
                                continue;
                            }
                 
                            if (reservedMetadataNames.Contains(pi.Name, StringComparers.MetadataKeyNames))
                            {
                                throw ExceptionBuilder.CreateDiscoveryException(Strings.Discovery_ReservedMetadataNameUsed, member.GetDisplayName(), provider.Name);
                            }

                            object value = pi.GetValue(attr, null);

                            if (value != null && !IsValidAttributeType(value.GetType()))
                            {
                                throw ExceptionBuilder.CreateDiscoveryException(Strings.Discovery_MetadataContainsValueWithInvalidType, pi.GetDisplayName(), value.GetType().GetDisplayName());
                            }

                            if (!dictionary.TryContributeMetadataValue(pi.Name, value, allowsMultiple))
                            {
                                throw ExceptionBuilder.CreateDiscoveryException(Strings.Discovery_DuplicateMetadataNameValues, member.GetDisplayName(), pi.Name);
                            }
                    }
                    }
                }
            }

            // Need Keys.ToArray because we alter the dictionary in the loop
            foreach (var key in dictionary.Keys.ToArray())
            {
                var list = dictionary[key] as MetadataList;
                if (list != null)
                {
                    dictionary[key] = list.ToArray();
                }
            }

            return;
        }

        private static bool TryContributeMetadataValue(this IDictionary<string, object> dictionary, string name, object value, bool allowsMultiple)
        {
            object metadataValue;
            if (!dictionary.TryGetValue(name, out metadataValue))
            {
                if (allowsMultiple)
                {
                    var list = new MetadataList();
                    list.Add(value);
                    value = list;
                }

                dictionary.Add(name, value);
            }
            else
            {
                var list = metadataValue as MetadataList;
                if (!allowsMultiple || list == null)
                {
                    // Either single value already found when should be multiple
                    // or a duplicate name already exists
                    dictionary.Remove(name);
                    return false;
                }

                list.Add(value);
            }
            return true;
        }

        private class MetadataList : Collection<object>
        {
            private Type _arrayType = null;
            private bool _mustBeReferenceType = false;

            protected override void InsertItem(int index, object item)
            {
                if (item == null)
                {
                    this._mustBeReferenceType = true;
                }
                else
                {
                    if (this._arrayType == null)
                    {
                        this._arrayType = item.GetType();
                    }
                    else if (this._arrayType != item.GetType())
                    {
                        this._arrayType = typeof(object);
                    }
                }

                base.InsertItem(index, item);
            }

            public Array ToArray()
            {
                if (this._mustBeReferenceType && this._arrayType.IsValueType)
                {
                    this._arrayType = typeof(object);
                }

                Array array = Array.CreateInstance(this._arrayType, this.Count);

                for(int i = 0; i < array.Length; i++)
                {
                    array.SetValue(this[i], i);
                }
                return array;
            }
        }

        //UNDONE: Need to add these warnings somewhere...Dev10:472538 should address this.
        //internal static CompositionResult MatchRequiredMetadata(this IDictionary<string, object> metadata, IEnumerable<string> requiredMetadata, string contractName)
        //{
        //    Assumes.IsTrue(metadata != null);

        //    var result = CompositionResult.SucceededResult;

        //    var missingMetadata = (requiredMetadata == null) ? null : requiredMetadata.Except<string>(metadata.Keys);
        //    if (missingMetadata != null && missingMetadata.Any())
        //    {
        //        result = result.MergeIssue(
        //            CompositionError.CreateIssueAsWarning(CompositionErrorId.RequiredMetadataNotFound,
        //            Strings.RequiredMetadataNotFound,
        //            contractName,
        //            string.Join(", ", missingMetadata.ToArray())));

        //        return new CompositionResult(false, result.Issues);
        //    }

        //    return result;
        //}

        internal static IEnumerable<KeyValuePair<string, Type>> GetRequiredMetadata(Type metadataViewType)
        {
            if ((metadataViewType == null) ||
                ExportServices.IsDefaultMetadataViewType(metadataViewType) ||
                ExportServices.IsDictionaryConstructorViewType(metadataViewType) ||
                !metadataViewType.IsInterface)
            {
                return Enumerable.Empty<KeyValuePair<string, Type>>();
            }

            // A metadata view is required to be an Intrerface, and therefore only properties are allowed
            List<PropertyInfo> properties = metadataViewType.GetAllProperties().
                Where(property => property.GetFirstAttribute<DefaultValueAttribute>() == null).
                ToList();

            // NOTE : this is a carefully found balance between eager and delay-evaluation - the properties are filtered once and upfront
            // whereas the key/Type pairs are created every time. The latter is fine as KVPs are structs and as such copied on access regardless.
            // This also allows us to avoid creation of List<KVP> which - at least according to FxCop - leads to isues with NGEN
            return properties.Select(property => new KeyValuePair<string, Type>(property.Name, property.PropertyType));
        }

        internal static object GetExportedValueFromComposedPart(ImportEngine engine, ComposablePart part, ExportDefinition definition)
        {
            try
            {
                engine.SatisfyImports(part);
            }
            catch (CompositionException ex)
            {
                throw ExceptionBuilder.CreateCannotGetExportedValue(part, definition, ex);
            }

            try
            {
                return part.GetExportedValue(definition);
            }
            catch (ComposablePartException ex)
            {
                throw ExceptionBuilder.CreateCannotGetExportedValue(part, definition, ex);
            }
        }
        
        internal static bool IsRecomposable(this ComposablePart part)
        {
            return part.ImportDefinitions.Any(import => import.IsRecomposable);
        }

        internal static CompositionResult<T> TryInvoke<T>(Func<T> action)
        {
            try
            {
                T value = action();
                return new CompositionResult<T>(value);
            }
            catch (CompositionException ex)
            {
                return new CompositionResult<T>(ex.Errors);
            }
        }

        internal static CompositionResult TryInvoke(Action action)
        {
            try
            {
                action();
                return CompositionResult.SucceededResult;
            }
            catch (CompositionException ex)
            {
                return new CompositionResult(ex.Errors);
            }
        }

        internal static CompositionResult TryFire<TEventArgs>(EventHandler<TEventArgs> _delegate, object sender, TEventArgs e)
            where TEventArgs : EventArgs
        {
            CompositionResult result = CompositionResult.SucceededResult;
            foreach (EventHandler<TEventArgs> _subscriber in _delegate.GetInvocationList())
            {
                try
                {
                    _subscriber.Invoke(sender, e);
                }
                catch (CompositionException ex)
                {
                    result = result.MergeErrors(ex.Errors);
                }
            }

            return result;
        }

        internal static CreationPolicy GetRequiredCreationPolicy(this ImportDefinition definition)
        {
            ContractBasedImportDefinition contractDefinition = definition as ContractBasedImportDefinition;

            if (contractDefinition != null)
            {
                return contractDefinition.RequiredCreationPolicy;
            }

            return CreationPolicy.Any;
        }

        /// <summary>
        ///     Returns a value indicating whether cardinality is 
        ///     <see cref="ImportCardinality.ZeroOrOne"/> or 
        ///     <see cref="ImportCardinality.ExactlyOne"/>.
        /// </summary>
        internal static bool IsAtMostOne(this ImportCardinality cardinality)
        {
            return cardinality == ImportCardinality.ZeroOrOne || cardinality == ImportCardinality.ExactlyOne;
        }

        private static bool IsValidAttributeType(Type type)
        {
            return IsValidAttributeType(type, true);
        }

        private static bool IsValidAttributeType(Type type, bool arrayAllowed)
        {
            Assumes.NotNull(type);
            // Definitions of valid attribute type taken from C# 3.0 Specification section 17.1.3.

            // One of the following types: bool, byte, char, double, float, int, long, sbyte, short, string, uint, ulong, ushort.
            if (type.IsPrimitive)
            {
                return true;
            }

            if (type == typeof(string))
            {
                return true;
            }

            // An enum type, provided it has public accessibility and the types in which it is nested (if any) also have public accessibility 
            if (type.IsEnum && type.IsVisible)
            {
                return true;
            }

            if (typeof(Type).IsAssignableFrom(type))
            {
                return true;
            }

            // Single-dimensional arrays of the above types.
            if (arrayAllowed && type.IsArray && 
                type.GetArrayRank() == 1 &&
                IsValidAttributeType(type.GetElementType(), false))
            {
                return true;
            }

            return false;
        }
    }
}
