// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Primitives;
using Microsoft.Internal;
using Microsoft.Internal.Collections;
using System.Reflection;

namespace System.ComponentModel.Composition.ReflectionModel
{
    // Describes the import type of a Reflection-based import definition
    internal class ImportType
    {
        private static readonly Type LazyOfTType = typeof(Lazy<>);
        private static readonly Type LazyOfTMType = typeof(Lazy<,>);
        private static readonly Type ExportFactoryOfTType = typeof(ExportFactory<>);
        private static readonly Type ExportFactoryOfTMType = typeof(ExportFactory<,>);

        private readonly Type _type;
        private readonly bool _isAssignableCollectionType;
        private readonly Type _contractType;
        private Func<Export, object> _castSingleValue;
        private bool _isOpenGeneric = false;

        public ImportType(Type type, ImportCardinality cardinality)
        {
            Assumes.NotNull(type);

            this._type = type;
            this._contractType = type;

            if (cardinality == ImportCardinality.ZeroOrMore)
            {
                this._isAssignableCollectionType = IsTypeAssignableCollectionType(type);
                this._contractType = CheckForCollection(type);
            }

            this._isOpenGeneric = type.ContainsGenericParameters;
            this._contractType = CheckForLazyAndPartCreator(this._contractType);
        }

        public bool IsAssignableCollectionType
        {
            get { return this._isAssignableCollectionType; }
        }

        public Type ElementType { get; private set; }

        public Type ActualType
        {
            get { return this._type; }
        }

        public bool IsPartCreator { get; private set; }

        public Type ContractType { get { return this._contractType; } }

        public Func<Export, object> CastExport
        {
            get
            {
                Assumes.IsTrue(!this._isOpenGeneric);
                return this._castSingleValue;
            }
        }

        public Type MetadataViewType { get; private set; }

        private Type CheckForCollection(Type type)
        {
            this.ElementType = CollectionServices.GetEnumerableElementType(type);
            if (this.ElementType != null)
            {
                return this.ElementType;
            }
            return type;
        }

        private static bool IsGenericDescendentOf(Type type, Type baseGenericTypeDefinition)
        {
            if (type == typeof(object) || type == null)
            {
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == baseGenericTypeDefinition)
            {
                    return true;
            }

            return IsGenericDescendentOf(type.BaseType, baseGenericTypeDefinition);
        }


        public static bool IsDescendentOf(Type type, Type baseType)
        {
            Assumes.NotNull(type);
            Assumes.NotNull(baseType);

            if (!baseType.IsGenericTypeDefinition)
            {
                return baseType.IsAssignableFrom(type);
            }

            return IsGenericDescendentOf(type, baseType.GetGenericTypeDefinition());
        }

        private Type CheckForLazyAndPartCreator(Type type)
        {
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition().UnderlyingSystemType;
                Type[] arguments = type.GetGenericArguments();

                if (genericType == LazyOfTType)
                {
                    if (!_isOpenGeneric)
                    {
                        this._castSingleValue = ExportServices.CreateStronglyTypedLazyFactory(arguments[0].UnderlyingSystemType, null);
                    }
                    return arguments[0];
                }

                if (genericType == LazyOfTMType)
                {
                    this.MetadataViewType = arguments[1];
                    if (!_isOpenGeneric)
                    {
                        this._castSingleValue = ExportServices.CreateStronglyTypedLazyFactory(arguments[0].UnderlyingSystemType, arguments[1].UnderlyingSystemType);
                    }
                    return arguments[0];
                }

                if(genericType != null && IsDescendentOf(genericType, ExportFactoryOfTType))
                {
                    this.IsPartCreator = true;
                    if (arguments.Length == 1)
                    {
                        if (!_isOpenGeneric)
                        {
                            this._castSingleValue = new ExportFactoryCreator(genericType).CreateStronglyTypedExportFactoryFactory(arguments[0].UnderlyingSystemType, null);
                        }
                    }
                    else if (arguments.Length == 2)
                    {
                        if (!_isOpenGeneric)
                        {
                            this._castSingleValue = new ExportFactoryCreator(genericType).CreateStronglyTypedExportFactoryFactory(arguments[0].UnderlyingSystemType, arguments[1].UnderlyingSystemType);
                        }
                        this.MetadataViewType = arguments[1];
                    }
                    else
                    {
                        throw ExceptionBuilder.ExportFactory_TooManyGenericParameters(genericType.FullName);
                    }
                    return arguments[0];
                }
            }

            return type;
        }

        private static bool IsTypeAssignableCollectionType(Type type)
        {
            if (type.IsArray || CollectionServices.IsEnumerableOfT(type))
            {
                return true;
            }

            return false;
        }
    }
}
