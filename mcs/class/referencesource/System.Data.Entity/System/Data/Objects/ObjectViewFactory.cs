//---------------------------------------------------------------------
// <copyright file="ObjectViewFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Metadata;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using System.Data.Objects.Internal;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Data.Objects
{
    /// <summary>
    /// Creates instances of ObjectView that provide a binding list for ObjectQuery results and EntityCollections.
    /// </summary>
    /// <remarks>
    /// The factory methods construct an ObjectView whose generic type parameter (and typed of elements in the binding list)
    /// is of the same type or a more specific derived type of the generic type of the ObjectQuery or EntityCollection.
    /// The EDM type of the query results or EntityType or the EntityCollection is examined to determine 
    /// the appropriate type to be used.
    /// For example, if you have an ObjectQuery whose generic type is "object", but the EDM result type of the Query maps
    /// to the CLR type "Customer", then the ObjectView returned will specify a generic type of "Customer", and not "object".
    /// </remarks>
    internal static class ObjectViewFactory
    {
        // References to commonly-used generic type definitions.
        private static readonly Type genericObjectViewType = typeof(ObjectView<>);

        private static readonly Type genericObjectViewDataInterfaceType = typeof(IObjectViewData<>);
        private static readonly Type genericObjectViewQueryResultDataType = typeof(ObjectViewQueryResultData<>);
        private static readonly Type genericObjectViewEntityCollectionDataType = typeof(ObjectViewEntityCollectionData<,>);

        /// <summary>
        /// Return a list suitable for data binding using the supplied query results.
        /// </summary>
        /// <typeparam name="TElement">
        /// CLR type of query result elements declared by the caller.
        /// </typeparam>
        /// <param name="elementEdmTypeUsage">
        /// The EDM type of the query results, used as the primary means of determining the 
        /// CLR type of list returned by this method.
        /// </param>
        /// <param name="queryResults">
        /// IEnumerable used to enumerate query results used to populate binding list.
        /// Must not be null.
        /// </param>
        /// <param name="objectContext">
        /// <see cref="ObjectContext"/> associated with the query from which results were obtained.
        /// Must not be null.
        /// </param>
        /// <param name="forceReadOnly">
        /// <b>True</b> to prevent modifications to the binding list built from the query result; otherwise <b>false</b>.
        /// Note that other conditions may prevent the binding list from being modified, so a value of <b>false</b>
        /// supplied for this parameter doesn't necessarily mean that the list will be writable.
        /// </param>
        /// <param name="singleEntitySet">
        /// If the query results are composed of entities that only exist in a single <see cref="EntitySet"/>, 
        /// the value of this parameter is the single EntitySet.
        /// Otherwise the value of this parameter should be null.
        /// </param>
        /// <returns>
        /// <see cref="IBindingList"/> that is suitable for data binding.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static IBindingList CreateViewForQuery<TElement>(TypeUsage elementEdmTypeUsage, IEnumerable<TElement> queryResults, ObjectContext objectContext, bool forceReadOnly, EntitySet singleEntitySet)
        {
            EntityUtil.CheckArgumentNull(queryResults, "queryResults");
            EntityUtil.CheckArgumentNull(objectContext, "objectContext");

            Type clrElementType = null;
            TypeUsage ospaceElementTypeUsage = GetOSpaceTypeUsage(elementEdmTypeUsage, objectContext);

            // Map the O-Space EDM type to a CLR type.
            // If the mapping is unsuccessful, fallback to TElement type.
            if (ospaceElementTypeUsage == null)
            {
                clrElementType = typeof(TElement);
            }
            {
                clrElementType = GetClrType<TElement>(ospaceElementTypeUsage.EdmType);
            }

            IBindingList objectView;
            object eventDataSource = objectContext.ObjectStateManager;

            // If the clrElementType matches the declared TElement type, optimize the construction of the ObjectView
            // by avoiding a reflection-based instantiation.
            if (clrElementType == typeof(TElement))
            {
                ObjectViewQueryResultData<TElement> viewData = new ObjectViewQueryResultData<TElement>((IEnumerable)queryResults, objectContext, forceReadOnly, singleEntitySet);

                objectView = new ObjectView<TElement>(viewData, eventDataSource);
            }
            else if (clrElementType == null)
            {
                ObjectViewQueryResultData<DbDataRecord> viewData = new ObjectViewQueryResultData<DbDataRecord>((IEnumerable)queryResults, objectContext, true, null);
                objectView = new DataRecordObjectView(viewData, eventDataSource, (RowType)ospaceElementTypeUsage.EdmType, typeof(TElement));
            }
            else
            {
                if (!typeof(TElement).IsAssignableFrom(clrElementType))
                {
                    throw EntityUtil.ValueInvalidCast(clrElementType, typeof(TElement));
                }

                // Use reflection to create an instance of the generic ObjectView and ObjectViewQueryResultData classes, 
                // using clrElementType as the value of TElement generic type parameter for both classes.

                Type objectViewDataType = genericObjectViewQueryResultDataType.MakeGenericType(clrElementType);

                ConstructorInfo viewDataConstructor = objectViewDataType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                                                        null,
                                                                                        new Type[] { typeof(IEnumerable), typeof(ObjectContext), typeof(bool), typeof(EntitySet) },
                                                                                        null);

                Debug.Assert(viewDataConstructor != null, "ObjectViewQueryResultData constructor not found. Please ensure constructor signature is correct.");

                // Create ObjectViewQueryResultData instance
                object viewData = viewDataConstructor.Invoke(new object[] { queryResults, objectContext, forceReadOnly, singleEntitySet });

                // Create ObjectView instance
                objectView = CreateObjectView(clrElementType, objectViewDataType, viewData, eventDataSource);
            }

            return objectView;
        }

        /// <summary>
        /// Return a list suitable for data binding using the supplied EntityCollection
        /// </summary>
        /// <typeparam name="TElement">
        /// CLR type of the elements of the EntityCollection.
        /// </typeparam>
        /// <param name="entityType">
        /// The EntityType of the elements in the collection.
        /// This should either be the same as the EntityType that corresponds to the CLR TElement type,
        /// or a EntityType derived from the declared EntityCollection element type.
        /// </param>
        /// <param name="entityCollection">
        /// The EntityCollection from which a binding list is created.
        /// </param>
        /// <returns>
        /// <see cref="IBindingList"/> that is suitable for data binding.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static IBindingList CreateViewForEntityCollection<TElement>(EntityType entityType, EntityCollection<TElement> entityCollection)
            where TElement : class
        {
            Type clrElementType = null;
            TypeUsage entityTypeUsage = entityType == null ? null : TypeUsage.Create(entityType);
            TypeUsage ospaceElementTypeUsage = GetOSpaceTypeUsage(entityTypeUsage, entityCollection.ObjectContext);

            // Map the O-Space EDM type to a CLR type.
            // If the mapping is unsuccessful, fallback to TElement type.
            if (ospaceElementTypeUsage == null)
            {
                clrElementType = typeof(TElement);
            }
            else
            {
                clrElementType = GetClrType<TElement>(ospaceElementTypeUsage.EdmType);

                // A null clrElementType is returned by GetClrType if the EDM type is a RowType with no specific CLR type mapping.
                // This should not happen when working with EntityCollections, but if it does, fallback to TEntityRef type.
                Debug.Assert(clrElementType != null, "clrElementType has unexpected value of null.");

                if (clrElementType == null)
                {
                    clrElementType = typeof(TElement);
                }
            }

            IBindingList objectView;

            // If the clrElementType matches the declared TElement type, optimize the construction of the ObjectView
            // by avoiding a reflection-based instantiation.
            if (clrElementType == typeof(TElement))
            {
                ObjectViewEntityCollectionData<TElement, TElement> viewData = new ObjectViewEntityCollectionData<TElement, TElement>(entityCollection);
                objectView = new ObjectView<TElement>(viewData, entityCollection);
            }
            else
            {
                if (!typeof(TElement).IsAssignableFrom(clrElementType))
                {
                    throw EntityUtil.ValueInvalidCast(clrElementType, typeof(TElement));
                }

                // Use reflection to create an instance of the generic ObjectView and ObjectViewEntityCollectionData classes, 
                // using clrElementType as the value of TElement generic type parameter for both classes.

                Type objectViewDataType = genericObjectViewEntityCollectionDataType.MakeGenericType(clrElementType, typeof(TElement));

                ConstructorInfo viewDataConstructor = objectViewDataType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                                                        null,
                                                                                        new Type[] { typeof(EntityCollection<TElement>) },
                                                                                        null);

                Debug.Assert(viewDataConstructor != null, "ObjectViewEntityCollectionData constructor not found. Please ensure constructor signature is correct.");

                // Create ObjectViewEntityCollectionData instance
                object viewData = viewDataConstructor.Invoke(new object[] { entityCollection });

                // Create ObjectView instance
                objectView = CreateObjectView(clrElementType, objectViewDataType, viewData, entityCollection);
            }

            return objectView;
        }

        /// <summary>
        /// Create an ObjectView using reflection.
        /// </summary>
        /// <param name="clrElementType">Type to be used for the ObjectView's generic type parameter.</param>
        /// <param name="objectViewDataType">The type of class that implements the IObjectViewData to be used by the ObjectView.</param>
        /// <param name="viewData">The IObjectViewData to be used by the ObjectView to access the binding list.</param>
        /// <param name="eventDataSource">Event source used by ObjectView for entity and membership changes.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IBindingList CreateObjectView(Type clrElementType, Type objectViewDataType, object viewData, object eventDataSource)
        {
            Type objectViewType = genericObjectViewType.MakeGenericType(clrElementType);

            Type[] viewDataInterfaces = objectViewDataType.FindInterfaces((Type type, object unusedFilter) => type.Name == genericObjectViewDataInterfaceType.Name, null);
            Debug.Assert(viewDataInterfaces.Length == 1, "Could not find IObjectViewData<T> interface definition for ObjectViewQueryResultData<T>.");

            ConstructorInfo viewConstructor = objectViewType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                                            null,
                                                                            new Type[] { viewDataInterfaces[0], typeof(object) },
                                                                            null);

            Debug.Assert(viewConstructor != null, "ObjectView constructor not found. Please ensure constructor signature is correct.");

            // Create ObjectView instance
            return (IBindingList)viewConstructor.Invoke(new object[] { viewData, eventDataSource });
        }

        /// <summary>
        /// Map the supplied TypeUsage to O-Space.
        /// </summary>
        /// <param name="typeUsage">
        /// The TypeUsage to be mapped to O-Space.  Should either be associated with C-Space or O-Space.
        /// </param>
        /// <param name="objectContext">
        /// ObjectContext used to perform type mapping.
        /// </param>
        /// <returns></returns>
        private static TypeUsage GetOSpaceTypeUsage(TypeUsage typeUsage, ObjectContext objectContext)
        {
            TypeUsage ospaceTypeUsage;

            if (typeUsage == null || typeUsage.EdmType == null)
            {
                ospaceTypeUsage = null;
            }
            else
            {
                if (typeUsage.EdmType.DataSpace == DataSpace.OSpace)
                {
                    ospaceTypeUsage = typeUsage;
                }
                else
                {
                    Debug.Assert(typeUsage.EdmType.DataSpace == DataSpace.CSpace, String.Format(System.Globalization.CultureInfo.InvariantCulture, "Expected EdmType.DataSpace to be C-Space, but instead it is {0}.", typeUsage.EdmType.DataSpace.ToString()));

                    // The ObjectContext is needed to map the EDM TypeUsage from C-Space to O-Space.
                    if (objectContext == null)
                    {
                        ospaceTypeUsage = null;
                    }
                    else
                    {
                        objectContext.EnsureMetadata();
                        ospaceTypeUsage = objectContext.Perspective.MetadataWorkspace.GetOSpaceTypeUsage(typeUsage);
                    }
                }
            }

            return ospaceTypeUsage;
        }

        /// <summary>
        /// Determine CLR Type to be exposed for data binding using the supplied EDM item type.
        /// </summary>
        /// <typeparam name="TElement">
        /// CLR element type declared by the caller.
        /// 
        /// There is no requirement that this method return the same type, or a type compatible with the declared type;
        /// it is merely a suggestion as to which type might be used.
        /// </typeparam>
        /// <param name="ospaceEdmType">
        /// The EDM O-Space type of the items in a particular query result.
        /// </param>
        /// <returns>
        /// <see cref="Type"/> instance that represents the CLR type that corresponds to the supplied EDM item type;
        /// or null if the EDM type does not map to a CLR type.
        /// Null is returned in the case where <paramref name="ospaceEdmType"/> is a <see cref="RowType"/>, 
        /// and no CLR type mapping is specified in the RowType metadata.
        /// </returns>
        private static Type GetClrType<TElement>(EdmType ospaceEdmType)
        {
            Type clrType;

            // EDM RowTypes are generally represented by CLR MaterializedDataRecord types
            // that need special handling to properly expose the properties available for binding (using ICustomTypeDescriptor and ITypedList implementations, for example).
            //
            // However, if the RowType has InitializerMetadata with a non-null CLR Type, 
            // that CLR type should be used to determine the properties available for binding.
            if (ospaceEdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
            {
                RowType itemRowType = (RowType)ospaceEdmType;

                if (itemRowType.InitializerMetadata != null && itemRowType.InitializerMetadata.ClrType != null)
                {
                    clrType = itemRowType.InitializerMetadata.ClrType;
                }
                else
                {
                    // If the generic parameter TElement is not exactly a data record type or object type,
                    // use it as the CLR type.
                    Type elementType = typeof(TElement);

                    if (typeof(IDataRecord).IsAssignableFrom(elementType) || elementType == typeof(object))
                    {
                        // No CLR type mapping exists for this RowType.
                        clrType = null;
                    }
                    else
                    {
                        clrType = typeof(TElement);
                    }
                }
            }
            else
            {
                clrType = ospaceEdmType.ClrType;

                // If the CLR type cannot be determined from the EDM type,
                // fallback to the element type declared by the caller.
                if (clrType == null)
                {
                    clrType = typeof(TElement);
                }
            }

            return clrType;
        }
    }
}
