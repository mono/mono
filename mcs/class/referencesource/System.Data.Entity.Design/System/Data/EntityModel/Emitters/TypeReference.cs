//---------------------------------------------------------------------
// <copyright file="TypeReference.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data.EntityModel.SchemaObjectModel;
using System.Data.Common.Utils;
using System.Reflection;

namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// Summary description for TypeReferences.
    /// </summary>
    internal class TypeReference
    {
        #region Fields
        internal static readonly Type ComplexTypeBaseClassType = typeof(System.Data.Objects.DataClasses.ComplexObject);
        internal static readonly Type EntityTypeBaseClassType = typeof(System.Data.Objects.DataClasses.EntityObject);
        private const string IEntityWithRelationshipsTypeBaseClassName = "IEntityWithRelationships";
        private const string NewContextClassName = "ObjectContext";
        private const string EntitySetClassName = "EntitySet";
        private const string ObjectResultClassName = "ObjectResult";

        public const string FQMetaDataWorkspaceTypeName = "System.Data.Metadata.Edm.MetadataWorkspace";

        private static CodeTypeReference _byteArray;
        private static CodeTypeReference _dateTime;
        private static CodeTypeReference _dateTimeOffset;
        private static CodeTypeReference _guid;
        private static CodeTypeReference _objectContext;
        private static CodeTypeReference _string;
        private static CodeTypeReference _timeSpan;
        private readonly Memoizer<Type, CodeTypeReference> _forTypeMemoizer;
        private readonly Memoizer<Type, CodeTypeReference> _nullableForTypeMemoizer;
        private readonly Memoizer<KeyValuePair<string, bool>, CodeTypeReference> _fromStringMemoizer;
        private readonly Memoizer<KeyValuePair<string, CodeTypeReference>, CodeTypeReference> _fromStringGenericMemoizer;
        #endregion

        #region Constructors
        internal TypeReference()
        {
            _forTypeMemoizer = new Memoizer<Type, CodeTypeReference>(ComputeForType, null);
            _fromStringMemoizer = new Memoizer<KeyValuePair<string, bool>, CodeTypeReference>(ComputeFromString, null);
            _nullableForTypeMemoizer = new Memoizer<Type, CodeTypeReference>(ComputeNullableForType, null);
            _fromStringGenericMemoizer = new Memoizer<KeyValuePair<string, CodeTypeReference>, CodeTypeReference>(ComputeFromStringGeneric, null);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the cached CodeTypeReference for a type represented by a Type object
        /// </summary>
        /// <param name="type">the type object</param>
        /// <returns>the associated TypeReference object</returns>
        public CodeTypeReference ForType(Type type)
        {
            return _forTypeMemoizer.Evaluate(type);
        }

        private CodeTypeReference ComputeForType(Type type)
        {
            // we know that we can safely global:: qualify this because it was already 
            // compiled before we are emitting or else we wouldn't have a Type object
            CodeTypeReference value = new CodeTypeReference(type, CodeTypeReferenceOptions.GlobalReference);
            return value;
        }

        /// <summary>
        /// Get TypeReference for a type represented by a Generic Type object.
        /// We don't cache the TypeReference for generic type object since the type would be the same
        /// irresepective of the generic arguments. We could potentially cache it using both the type name
        /// and generic type arguments.
        /// </summary>
        /// <param name="type">the generic type object</param>
        /// <returns>the associated TypeReference object</returns>
        public CodeTypeReference ForType(Type generic, CodeTypeReference argument)
        {
            // we know that we can safely global:: qualify this because it was already 
            // compiled before we are emitting or else we wouldn't have a Type object
            CodeTypeReference typeRef = new CodeTypeReference(generic, CodeTypeReferenceOptions.GlobalReference);
            typeRef.TypeArguments.Add(argument);
            return typeRef;
        }


        /// <summary>
        /// Get TypeReference for a type represented by a namespace quailifed string 
        /// </summary>
        /// <param name="type">namespace qualified string</param>
        /// <returns>the TypeReference</returns>
        public CodeTypeReference FromString(string type)
        {
            return FromString(type, false);
        }

        /// <summary>
        /// Get TypeReference for a type represented by a namespace quailifed string,
        /// with optional global qualifier
        /// </summary>
        /// <param name="type">namespace qualified string</param>
        /// <param name="addGlobalQualifier">indicates whether the global qualifier should be added</param>
        /// <returns>the TypeReference</returns>
        public CodeTypeReference FromString(string type, bool addGlobalQualifier)
        {
            return _fromStringMemoizer.Evaluate(new KeyValuePair<string, bool>(type, addGlobalQualifier));
        }

        private CodeTypeReference ComputeFromString(KeyValuePair<string, bool> arguments)
        {
            string type = arguments.Key;
            bool addGlobalQualifier = arguments.Value;
            CodeTypeReference value;
            if (addGlobalQualifier)
            {
                value = new CodeTypeReference(type, CodeTypeReferenceOptions.GlobalReference);
            }
            else
            {
                value = new CodeTypeReference(type);
            }
            return value;
        }

        /// <summary>
        /// Get TypeReference for a framework type
        /// </summary>
        /// <param name="name">unqualified name of the framework type</param>
        /// <returns>the TypeReference</returns>
        public CodeTypeReference AdoFrameworkType(string name)
        {
            return FromString(Utils.FQAdoFrameworkName(name), true);
        }

        /// <summary>
        /// Get TypeReference for a framework DataClasses type
        /// </summary>
        /// <param name="name">unqualified name of the framework DataClass type</param>
        /// <returns>the TypeReference</returns>
        public CodeTypeReference AdoFrameworkDataClassesType(string name)
        {
            return FromString(Utils.FQAdoFrameworkDataClassesName(name), true);
        }

        /// <summary>
        /// Get TypeReference for a framework Metadata Edm type
        /// </summary>
        /// <param name="name">unqualified name of the framework metadata edm type</param>
        /// <returns>the TypeReference</returns>
        public CodeTypeReference AdoFrameworkMetadataEdmType(string name)
        {
            return FromString(Utils.FQAdoFrameworkMetadataEdmName(name), true);
        }

        /// <summary>
        /// Get TypeReference for a framework Entity Client type
        /// </summary>
        /// <param name="name">unqualified name of the framework type</param>
        /// <returns>the TypeReference</returns>
        public CodeTypeReference AdoEntityClientType(string name)
        {
            return FromString(Utils.FQAdoEntityClientName(name), true);
        }

        /// <summary>
        /// Get TypeReference for a bound generic framework class
        /// </summary>
        /// <param name="name">the name of the generic framework class</param>
        /// <param name="typeParameter">the type parameter for the framework class</param>
        /// <returns>TypeReference for the bound framework class</returns>
        public CodeTypeReference AdoFrameworkGenericClass(string name, CodeTypeReference typeParameter)
        {
            return FrameworkGenericClass(Utils.AdoFrameworkNamespace, name, typeParameter);
        }
        /// <summary>
        /// Get TypeReference for a bound generic framework data class
        /// </summary>
        /// <param name="name">the name of the generic framework data class</param>
        /// <param name="typeParameter">the type parameter for the framework data class</param>
        /// <returns>TypeReference for the bound framework data class</returns>
        public CodeTypeReference AdoFrameworkGenericDataClass(string name, CodeTypeReference typeParameter)
        {
            return FrameworkGenericClass(Utils.AdoFrameworkDataClassesNamespace, name, typeParameter);
        }

        /// <summary>
        /// Get TypeReference for a bound generic framework class
        /// </summary>
        /// <param name="namespaceName">the namespace of the generic framework class</param>
        /// <param name="name">the name of the generic framework class</param>
        /// <param name="typeParameter">the type parameter for the framework class</param>
        /// <returns>TypeReference for the bound framework class</returns>
        private CodeTypeReference FrameworkGenericClass(string namespaceName, string name, CodeTypeReference typeParameter)
        {
            return _fromStringGenericMemoizer.Evaluate(new KeyValuePair<string, CodeTypeReference>(namespaceName + "." + name, typeParameter));
        }

        private CodeTypeReference ComputeFromStringGeneric(KeyValuePair<string, CodeTypeReference> arguments)
        {
            string name = arguments.Key;
            CodeTypeReference typeParameter = arguments.Value;
            CodeTypeReference typeRef = ComputeFromString(new KeyValuePair<string, bool>(name, true));
            typeRef.TypeArguments.Add(typeParameter);
            return typeRef;
        }

        /// <summary>
        /// Get TypeReference for a bound Nullable&lt;T&gt;
        /// </summary>
        /// <param name="innerType">Type of the Nullable&lt;T&gt; type parameter</param>
        /// <returns>TypeReference for a bound Nullable&lt;T&gt;</returns>
        public CodeTypeReference NullableForType(Type innerType)
        {
            return _nullableForTypeMemoizer.Evaluate(innerType);
        }

        private CodeTypeReference ComputeNullableForType(Type innerType)
        {
            // can't use FromString because it will return the same Generic type reference
            // but it will already have a previous type parameter (because of caching)
            CodeTypeReference typeRef = new CodeTypeReference(typeof(System.Nullable<>), CodeTypeReferenceOptions.GlobalReference);
            typeRef.TypeArguments.Add(ForType(innerType));
            return typeRef;
        }

        /// <summary>
        /// Gets an ObjectResult of elementType CodeTypeReference. 
        /// </summary>
        public CodeTypeReference ObjectResult(CodeTypeReference elementType)
        {
            return AdoFrameworkGenericClass(ObjectResultClassName, elementType);
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Gets a CodeTypeReference to the System.Byte[] type.
        /// </summary>
        /// <value></value>
        public CodeTypeReference ByteArray
        {
            get
            {
                if (_byteArray == null)
                {
                    _byteArray = ForType(typeof(byte[]));
                }

                return _byteArray;
            }
        }

        /// <summary>
        /// Gets a CodeTypeReference object for the System.DateTime type.
        /// </summary>
        public CodeTypeReference DateTime
        {
            get
            {
                if (_dateTime == null)
                {
                    _dateTime = ForType(typeof(System.DateTime));
                }

                return _dateTime;
            }
        }

        /// <summary>
        /// Gets a CodeTypeReference object for the System.DateTimeOffset type.
        /// </summary>
        public CodeTypeReference DateTimeOffset
        {
            get
            {
                if (_dateTimeOffset == null)
                {
                    _dateTimeOffset = ForType(typeof(System.DateTimeOffset));
                }

                return _dateTimeOffset;
            }
        }

        /// <summary>
        /// Gets a CodeTypeReference object for the System.Guid type.
        /// </summary>
        public CodeTypeReference Guid
        {
            get
            {
                if (_guid == null)
                {
                    _guid = ForType(typeof(System.Guid));
                }

                return _guid;
            }
        }

        /// <summary>
        /// TypeReference for the Framework's ObjectContext class
        /// </summary>
        public CodeTypeReference ObjectContext
        {
            get
            {
                if (_objectContext == null)
                {
                    _objectContext = AdoFrameworkType(NewContextClassName);
                }

                return _objectContext;
            }
        }

        /// <summary>
        /// TypeReference for the Framework base class to types that can used in InlineObjectCollection
        /// </summary>
        public CodeTypeReference ComplexTypeBaseClass
        {
            get
            {
                return ForType(ComplexTypeBaseClassType);
            }
        }


        /// <summary>
        /// TypeReference for the Framework base class for EntityTypes
        /// </summary>
        public CodeTypeReference EntityTypeBaseClass
        {
            get
            {
                return ForType(EntityTypeBaseClassType);
            }
        }

        /// <summary>
        /// TypeReference for the Framework base class for IEntityWithRelationships
        /// </summary>
        public CodeTypeReference IEntityWithRelationshipsTypeBaseClass
        {
            get
            {
                return AdoFrameworkDataClassesType(IEntityWithRelationshipsTypeBaseClassName);
            }
        }

        /// <summary>
        /// Gets a CodeTypeReference object for the System.String type.
        /// </summary>
        public CodeTypeReference String
        {
            get
            {
                if (_string == null)
                {
                    _string = ForType(typeof(string));
                }
                return _string;
            }
        }

        /// <summary>
        /// Gets a CodeTypeReference object for the System.TimeSpan type.
        /// </summary>
        public CodeTypeReference TimeSpan
        {
            get
            {
                if (_timeSpan == null)
                {
                    _timeSpan = ForType(typeof(System.TimeSpan));
                }
                return _timeSpan;
            }
        }
        #endregion

    }
}
