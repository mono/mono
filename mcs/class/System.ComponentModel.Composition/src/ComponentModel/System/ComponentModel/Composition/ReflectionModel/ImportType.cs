// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Primitives;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.ReflectionModel
{
    // Describes the import type of a Reflection-based import definition
    internal class ImportType
    {
        private static readonly Type LazyOfTType = typeof(Lazy<>);
        private static readonly Type LazyOfTMType = typeof(Lazy<,>);
#if SILVERLIGHT
        private static readonly Type PartCreatorOfTType = typeof(PartCreator<>);
        private static readonly Type PartCreatorOfTMType = typeof(PartCreator<,>);
#endif

        private readonly Type _type;
        private readonly bool _isAssignableCollectionType;
        private readonly Type _contractType;
        private Func<Export, object> _castSingleValue;

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

#if SILVERLIGHT
        public bool IsPartCreator { get; private set; }
#endif

        public Type ContractType { get { return this._contractType; } }

        public Func<Export, object> CastExport { get { return this._castSingleValue; } }

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

        private Type CheckForLazyAndPartCreator(Type type)
        {
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                Type[] arguments = type.GetGenericArguments();

                if (genericType == LazyOfTType)
                {
                    this._castSingleValue = ExportServices.CreateStronglyTypedLazyFactory(arguments[0], null);
                    return arguments[0];
                }

                if (genericType == LazyOfTMType)
                {
                    this.MetadataViewType = arguments[1];
                    this._castSingleValue = ExportServices.CreateStronglyTypedLazyFactory(arguments[0], arguments[1]);
                    return arguments[0];
                }

#if SILVERLIGHT
                if (genericType == PartCreatorOfTType)
                {
                    this.IsPartCreator = true;
                    this._castSingleValue = ExportServices.CreateStronglyTypedPartCreatorFactory(arguments[0], null);
                    return arguments[0];
                }

                if (genericType == PartCreatorOfTMType)
                {
                    this.IsPartCreator = true;
                    this._castSingleValue = ExportServices.CreateStronglyTypedPartCreatorFactory(arguments[0], arguments[1]);
                    this.MetadataViewType = arguments[1];
                    return arguments[0];
                }
#endif
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
