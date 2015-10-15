//------------------------------------------------------------------------------
// <copyright file="EntityUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Data.Objects.Internal;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Security.Permissions;
    using System.Text;

    internal static class EntityUtil {

        internal const int AssemblyQualifiedNameIndex = 3;
        internal const int InvariantNameIndex = 2;

        internal const string Parameter = "Parameter";

        internal const CompareOptions StringCompareOptions = CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase;

        internal static bool? ThreeValuedNot(bool? operand) {
            // three-valued logic 'not' (T = true, F = false, U = unknown)
            //      !T = F
            //      !F = T
            //      !U = U
            return operand.HasValue ? !operand.Value : (bool?)null;
        }
        internal static bool? ThreeValuedAnd(bool? left, bool? right) {
            // three-valued logic 'and' (T = true, F = false, U = unknown)
            //
            //      T & T = T
            //      T & F = F
            //      F & F = F
            //      F & T = F
            //      F & U = F
            //      U & F = F
            //      T & U = U
            //      U & T = U
            //      U & U = U
            bool? result;
            if (left.HasValue && right.HasValue) {
                result = left.Value && right.Value;
            }
            else if (!left.HasValue && !right.HasValue) {
                result = null; // unknown
            }
            else if (left.HasValue) {
                result = left.Value ?
                    (bool?)null :// unknown
                    false;
            }
            else {
                result = right.Value ?
                    (bool?)null :
                    false;
            }
            return result;
        }

        internal static bool? ThreeValuedOr(bool? left, bool? right) {
            // three-valued logic 'or' (T = true, F = false, U = unknown)
            //
            //      T | T = T
            //      T | F = T
            //      F | F = F
            //      F | T = T
            //      F | U = U
            //      U | F = U
            //      T | U = T
            //      U | T = T
            //      U | U = U
            bool? result;
            if (left.HasValue && right.HasValue) {
                result = left.Value || right.Value;
            }
            else if (!left.HasValue && !right.HasValue) {
                result = null; // unknown
            }
            else if (left.HasValue) {
                result = left.Value ?
                    true :
                    (bool?)null; // unknown
            }
            else {
                result = right.Value ?
                    true :
                    (bool?)null; // unknown
            }
            return result;
        }

        /// <summary>
        /// Zips two enumerables together (e.g., given {1, 3, 5} and {2, 4, 6} returns {{1, 2}, {3, 4}, {5, 6}})
        /// </summary>
        internal static IEnumerable<KeyValuePair<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
        {
            if (null == first || null == second) { yield break; }
            using (IEnumerator<T1> firstEnumerator = first.GetEnumerator())
            using (IEnumerator<T2> secondEnumerator = second.GetEnumerator())
            {
                while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext())
                {
                    yield return new KeyValuePair<T1, T2>(firstEnumerator.Current, secondEnumerator.Current);
                }
            }
        }

        /// <summary>
        /// Returns true if the type implements ICollection<>
        /// </summary>
        internal static bool IsAnICollection(Type type)
        {
            return typeof(ICollection<>).IsAssignableFrom(type.GetGenericTypeDefinition()) ||
                    type.GetInterface(typeof(ICollection<>).FullName) != null;

        }

        /// <summary>
        /// Given a type that represents a collection, determine if the type implements ICollection&lt&gt, and if
        /// so return the element type of the collection.  Currently, if the collection implements ICollection&lt&gt
        /// multiple times with different types, then we will return false since this is not supported.
        /// </summary>
        /// <param name="collectionType">the collection type to examine</param>
        /// <param name="elementType">the type of element</param>
        /// <returns>true if the collection implement ICollection&lt&gt; false otherwise</returns>
        internal static bool TryGetICollectionElementType(Type collectionType, out Type elementType)
        {
            elementType = null;
            // We have to check if the type actually is the interface, or if it implements the interface:
            try
            {
                Type collectionInterface =
                     (collectionType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(collectionType.GetGenericTypeDefinition())) ?
                     collectionType :
                     collectionType.GetInterface(typeof(ICollection<>).FullName);

                // We need to make sure the type is fully specified otherwise we won't be able to add element to it.
                if (collectionInterface != null && !collectionInterface.ContainsGenericParameters)
                {
                    elementType = collectionInterface.GetGenericArguments()[0];
                    return true;
                }

            }
            catch (AmbiguousMatchException)
            {
                // Thrown if collection type implements ICollection<> more than once
            }
            return false;
        }

        /// <summary>
        /// Helper method to determine the element type of the collection contained by the given property.
        /// If an unambiguous element type cannot be found, then an InvalidOperationException is thrown.
        /// </summary>
        internal static Type GetCollectionElementType(Type propertyType)
        {
            Type elementType;
            if (!EntityUtil.TryGetICollectionElementType(propertyType, out elementType))
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.PocoEntityWrapper_UnexpectedTypeForNavigationProperty(
                                                  propertyType.FullName,
                                                  typeof(ICollection<>)));
            }
            return elementType;
        }

        /// <summary>
        /// This is used when we need to determine a concrete collection type given some type that may be
        /// abstract or an interface.
        /// </summary>
        /// <remarks>
        /// The rules are:
        /// If the collection is defined as a concrete type with a publicly accessible parameterless constructor, then create an instance of that type
        /// Else, if HashSet<T> can be assigned to the type, then use HashSet<T>
        /// Else, if List<T> can be assigned to the type, then use List<T>
        /// Else, throw a nice exception.
        /// </remarks>
        /// <param name="requestedType">The type of collection that was requested</param>
        /// <returns>The type to instantiate, or null if we cannot find a supported type to instantiate</returns>
        internal static Type DetermineCollectionType(Type requestedType)
        {
            const BindingFlags constructorBinding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance;

            var elementType = EntityUtil.GetCollectionElementType(requestedType);

            if (requestedType.IsArray)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectQuery_UnableToMaterializeArray(
                                                  requestedType, typeof(List<>).MakeGenericType(elementType)));
            }

            if (!requestedType.IsAbstract &&
                requestedType.GetConstructor(constructorBinding, null, System.Type.EmptyTypes, null) != null)
            {
                return requestedType;
            }

            var hashSetOfT = typeof(HashSet<>).MakeGenericType(elementType);
            if (requestedType.IsAssignableFrom(hashSetOfT))
            {
                return hashSetOfT;
            }

            var listOfT = typeof(List<>).MakeGenericType(elementType);
            if (requestedType.IsAssignableFrom(listOfT))
            {
                return listOfT;
            }

            return null;
        }

        /// <summary>
        /// Returns the Type object that should be used to identify the type in the o-space
        /// metadata.  This is normally just the type that is passed in, but if the type
        /// is a proxy that we have generated, then its base type is returned instead.
        /// This ensures that both proxy entities and normal entities are treated as the
        /// same kind of entity in the metadata and places where the metadata is used.
        /// </summary>
        internal static Type GetEntityIdentityType(Type entityType)
        {
            return EntityProxyFactory.IsProxyType(entityType) ? entityType.BaseType : entityType;
        }

        /// <summary>
        /// Provides a standard helper method for quoting identifiers
        /// </summary>
        /// <param name="identifier">Identifier to be quoted. Does not validate that this identifier is valid.</param>
        /// <returns>Quoted string</returns>
        internal static string QuoteIdentifier(string identifier)
        {
            Debug.Assert(identifier != null, "identifier should not be null");
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        // The class contains functions that take the proper informational variables and then construct
        // the appropriate exception with an error string obtained from the resource file.
        // The exception is then returned to the caller, so that the caller may then throw from its
        // location so that the catcher of the exception will have the appropriate call stack.
        // This class is used so that there will be compile time checking of error messages.

        static internal ArgumentException Argument(string error) {
            return new ArgumentException(error);
        }
        static internal ArgumentException Argument(string error, Exception inner) {
            return new ArgumentException(error, inner);
        }
        static internal ArgumentException Argument(string error, string parameter) {
            return new ArgumentException(error, parameter);
        }
        static internal ArgumentException Argument(string error, string parameter, Exception inner) {
            return new ArgumentException(error, parameter, inner);
        }
        static internal ArgumentNullException ArgumentNull(string parameter) {
            return new ArgumentNullException(parameter);
        }
        static internal ArgumentOutOfRangeException ArgumentOutOfRange(string parameterName) {
            return new ArgumentOutOfRangeException(parameterName);
        }
        static internal ArgumentOutOfRangeException ArgumentOutOfRange(string message, string parameterName) {
            return new ArgumentOutOfRangeException(parameterName, message);
        }
        static internal EntityCommandExecutionException CommandExecution(string message) {
            return new EntityCommandExecutionException(message);
        }
        static internal EntityCommandExecutionException CommandExecution(string message, Exception innerException) {
            return new EntityCommandExecutionException(message, innerException);
        }
        static internal EntityCommandCompilationException CommandCompilation(string message, Exception innerException) {
            return new EntityCommandCompilationException(message, innerException);
        }
        static internal PropertyConstraintException PropertyConstraint(string message, string propertyName)
        {
            return new PropertyConstraintException(message, propertyName);
        }
        static internal ConstraintException Constraint(string message)
        {
            return new ConstraintException(message);
        }
        static internal IndexOutOfRangeException IndexOutOfRange(string error)
        {
            return new IndexOutOfRangeException(error);
        }
        static internal InvalidOperationException InvalidOperation(string error) {
            return new InvalidOperationException(error);
        }
        static internal InvalidOperationException InvalidOperation(string error, Exception inner) {
            return new InvalidOperationException(error, inner);
        }
        static internal ArgumentException InvalidStringArgument(string parameterName) {
            return Argument(System.Data.Entity.Strings.InvalidStringArgument(parameterName), parameterName);
        }
        static internal MappingException Mapping(string message)
        {
            return new MappingException(message);
        }
        static internal MetadataException Metadata(string message, Exception inner)
        {
            return new MetadataException(message, inner);
        }
        static internal MetadataException Metadata(string message)
        {
            return new MetadataException(message);
        }
        static internal NotSupportedException NotSupported()
        {
            return new NotSupportedException();
        }
        static internal NotSupportedException NotSupported(string error) {
            return new NotSupportedException(error);
        }
        static internal ObjectDisposedException ObjectDisposed(string error) {
            return new ObjectDisposedException(null, error);
        }
        static internal ObjectNotFoundException ObjectNotFound(string error) {
            return new ObjectNotFoundException(error);
        }

        // SSDL Generator
        //static internal StrongTypingException StrongTyping(string error, Exception innerException) {
        //    StrongTypingException e = new StrongTypingException(error, innerException);
        //    TraceExceptionAsReturnValue(e);
        //    return e;
        //}
        #region Query Exceptions
        /// <summary>
        /// EntityException factory method
        /// </summary>
        /// <param name="message"></param>
        /// <returns>EntityException</returns>
        static internal EntitySqlException EntitySqlError( string message )
        {
            return new EntitySqlException(message);
        }

        /// <summary>
        /// EntityException factory method
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        static internal EntitySqlException EntitySqlError( string message, Exception innerException)
        {
            return new EntitySqlException(message, innerException);
        }

        /// <summary>
        /// EntityException factory method
        /// </summary>
        /// <param name="errCtx"></param>
        /// <param name="message"></param>
        /// <returns>EntityException</returns>
        static internal EntitySqlException EntitySqlError( System.Data.Common.EntitySql.ErrorContext errCtx, string message )
        {
            return EntitySqlException.Create(errCtx, message, null);
        }

        /// <summary>
        /// EntityException factory method
        /// </summary>
        /// <param name="errCtx"></param>
        /// <param name="message"></param>
        /// <returns>EntityException</returns>
        static internal EntitySqlException EntitySqlError( System.Data.Common.EntitySql.ErrorContext errCtx, string message, Exception innerException )
        {
            return EntitySqlException.Create(errCtx, message, null);
        }

        /// <summary>
        /// EntityException factory method
        /// </summary>
        /// <param name="queryText"></param>
        /// <param name="errorMessage"></param>
        /// <param name="errorPosition"></param>
        /// <returns></returns>
        static internal EntitySqlException EntitySqlError( string queryText, string errorMessage, int errorPosition )
        {
            return EntitySqlException.Create(queryText, errorMessage, errorPosition, null, false, null);
        }

        /// <summary>
        /// EntityException factory method. AdditionalErrorInformation will be used inlined if loadContextInfoFromResource is false.
        /// </summary>
        /// <param name="queryText"></param>
        /// <param name="errorMessage"></param>
        /// <param name="errorPosition"></param>
        /// <param name="additionalErrorInformation"></param>
        /// <param name="loadContextInfoFromResource"></param>
        /// <returns></returns>
        static internal EntitySqlException EntitySqlError( string queryText,
                                                   string errorMessage,
                                                   int errorPosition,
                                                   string additionalErrorInformation,
                                                   bool loadContextInfoFromResource )
        {
            return EntitySqlException.Create(queryText,
                                                  errorMessage,
                                                  errorPosition,
                                                  additionalErrorInformation,
                                                  loadContextInfoFromResource,
                                                  null);
        }
        #endregion

        #region Bridge Errors
        static internal ProviderIncompatibleException CannotCloneStoreProvider() {
            return ProviderIncompatible(System.Data.Entity.Strings.EntityClient_CannotCloneStoreProvider);
        }
        static internal InvalidOperationException ClosedDataReaderError() {
            return InvalidOperation(System.Data.Entity.Strings.ADP_ClosedDataReaderError);
        }
        static internal InvalidOperationException DataReaderClosed(string method) {
            return InvalidOperation(System.Data.Entity.Strings.ADP_DataReaderClosed(method));
        }
        static internal InvalidOperationException ImplicitlyClosedDataReaderError() {
            return InvalidOperation(System.Data.Entity.Strings.ADP_ImplicitlyClosedDataReaderError);
        }
        static internal IndexOutOfRangeException InvalidBufferSizeOrIndex(int numBytes, int bufferIndex) {
            return IndexOutOfRange(System.Data.Entity.Strings.ADP_InvalidBufferSizeOrIndex(numBytes.ToString(CultureInfo.InvariantCulture), bufferIndex.ToString(CultureInfo.InvariantCulture)));
        }
        static internal IndexOutOfRangeException InvalidDataLength(long length) {
            return IndexOutOfRange(System.Data.Entity.Strings.ADP_InvalidDataLength(length.ToString(CultureInfo.InvariantCulture)));
        }
        static internal ArgumentOutOfRangeException InvalidDestinationBufferIndex(int maxLen, int dstOffset, string parameterName) {
            return ArgumentOutOfRange(System.Data.Entity.Strings.ADP_InvalidDestinationBufferIndex(maxLen.ToString(CultureInfo.InvariantCulture), dstOffset.ToString(CultureInfo.InvariantCulture)), parameterName);
        }
        static internal ArgumentOutOfRangeException InvalidSourceBufferIndex(int maxLen, long srcOffset, string parameterName) {
            return ArgumentOutOfRange(System.Data.Entity.Strings.ADP_InvalidSourceBufferIndex(maxLen.ToString(CultureInfo.InvariantCulture), srcOffset.ToString(CultureInfo.InvariantCulture)), parameterName);
        }
        static internal InvalidOperationException MustUseSequentialAccess() {
            return InvalidOperation(System.Data.Entity.Strings.ADP_MustUseSequentialAccess);
        }
        static internal InvalidOperationException NoData() {
            return InvalidOperation(System.Data.Entity.Strings.ADP_NoData);
        }
        static internal InvalidOperationException NonSequentialArrayOffsetAccess(long badIndex, long currIndex, string method) {
            return InvalidOperation(System.Data.Entity.Strings.ADP_NonSequentialChunkAccess(badIndex.ToString(CultureInfo.InvariantCulture), currIndex.ToString(CultureInfo.InvariantCulture), method));
        }
        static internal InvalidOperationException NonSequentialColumnAccess(int badCol, int currCol) {
            return InvalidOperation(System.Data.Entity.Strings.ADP_NonSequentialColumnAccess(badCol.ToString(CultureInfo.InvariantCulture), currCol.ToString(CultureInfo.InvariantCulture)));
        }
        static internal NotSupportedException KeysRequiredForJoinOverNest(Query.InternalTrees.Op op) {
            return NotSupported(System.Data.Entity.Strings.ADP_KeysRequiredForJoinOverNest(op.OpType.ToString()));
        }
        static internal NotSupportedException KeysRequiredForNesting() {
            return NotSupported(System.Data.Entity.Strings.ADP_KeysRequiredForNesting);
        }
        static internal NotSupportedException NestingNotSupported(Query.InternalTrees.Op parentOp, Query.InternalTrees.Op childOp) {
            return NotSupported(System.Data.Entity.Strings.ADP_NestingNotSupported(parentOp.OpType.ToString(), childOp.OpType.ToString()));
        }
        static internal NotSupportedException ProviderDoesNotSupportCommandTrees() {
            return NotSupported(System.Data.Entity.Strings.ADP_ProviderDoesNotSupportCommandTrees);
        }
        static internal EntityCommandExecutionException CommandExecutionDataReaderFieldCountForScalarType() {
            return CommandExecution(System.Data.Entity.Strings.ADP_InvalidDataReaderFieldCountForScalarType);
        }
        static internal EntityCommandExecutionException CommandExecutionDataReaderMissingColumnForType(EdmMember member, EdmType currentType) {
            return CommandExecution(System.Data.Entity.Strings.ADP_InvalidDataReaderMissingColumnForType(
                currentType.FullName, member.Name));
        }
        static internal EntityCommandExecutionException CommandExecutionDataReaderMissinDiscriminatorColumn(string columnName, EdmFunction functionImport) {
            return CommandExecution(System.Data.Entity.Strings.ADP_InvalidDataReaderMissingDiscriminatorColumn(columnName, functionImport.FullName));
        }

        #endregion

        #region EntityClient Errors
        static internal ProviderIncompatibleException ProviderIncompatible(string error) {
            return new ProviderIncompatibleException(error);
        }
        static internal ProviderIncompatibleException ProviderIncompatible(string error, Exception innerException) {
            return new ProviderIncompatibleException(error, innerException);
        }
        static internal EntityException Provider(string error) {
            return new EntityException(error);
        }
        static internal EntityException Provider(Exception inner) {
            return new EntityException(System.Data.Entity.Strings.EntityClient_ProviderGeneralError, inner);
        }
        static internal EntityException Provider(string parameter, Exception inner) {
            return new EntityException(System.Data.Entity.Strings.EntityClient_ProviderSpecificError(parameter), inner);
        }
        static internal EntityException ProviderExceptionWithMessage(string message, Exception inner) {
            return new EntityException(message, inner);
        }
        #endregion //EntityClient Errors

        #region SqlClient Errors

        static internal InvalidOperationException SqlTypesAssemblyNotFound()
        {
            return InvalidOperation(System.Data.Entity.Strings.SqlProvider_SqlTypesAssemblyNotFound);
        }

        static internal ProviderIncompatibleException GeographyValueNotSqlCompatible()
        {
            return ProviderIncompatible(System.Data.Entity.Strings.SqlProvider_GeographyValueNotSqlCompatible);
        }

        static internal ProviderIncompatibleException GeometryValueNotSqlCompatible()
        {
            return ProviderIncompatible(System.Data.Entity.Strings.SqlProvider_GeometryValueNotSqlCompatible);
        }

        #endregion //SqlClient Errors

        #region Metadata Errors
        static internal MetadataException InvalidSchemaEncountered(string errors) {
            // EntityRes.GetString implementation truncates the string arguments to a max length of 1024. 
            // Since csdl, ssdl, providermanifest can have bunch of errors in them and we want to
            // show all of them, we are using String.Format to form the error message.
            // Using CurrentCulture since that's what EntityRes.GetString uses.
            return Metadata(String.Format(CultureInfo.CurrentCulture, EntityRes.GetString(EntityRes.InvalidSchemaEncountered), errors));
        }
        static internal MetadataException InvalidCollectionForMapping(DataSpace space) {
            return Metadata(System.Data.Entity.Strings.InvalidCollectionForMapping(space.ToString()));
        }
        // MemberCollection.cs
        static internal ArgumentException MemberInvalidIdentity(string identity, string parameter) {
            return Argument(System.Data.Entity.Strings.MemberInvalidIdentity(identity), parameter);
        }
        // MetadataCollection.cs
        static internal ArgumentException ArrayTooSmall(string parameter) {
            return Argument(System.Data.Entity.Strings.ArrayTooSmall, parameter);
        }
        static internal ArgumentException ItemDuplicateIdentity(string identity, string parameter, Exception inner) {
            return Argument(System.Data.Entity.Strings.ItemDuplicateIdentity(identity), parameter, inner);
        }
        static internal ArgumentException ItemInvalidIdentity(string identity, string parameter) {
            return Argument(System.Data.Entity.Strings.ItemInvalidIdentity(identity), parameter);
        }
        static internal InvalidOperationException MoreThanOneItemMatchesIdentity(string identity) {
            return InvalidOperation(System.Data.Entity.Strings.MoreThanOneItemMatchesIdentity(identity));
        }
        static internal InvalidOperationException OperationOnReadOnlyCollection() {
            return InvalidOperation(System.Data.Entity.Strings.OperationOnReadOnlyCollection);
        }
        // MetadataWorkspace.cs
        static internal InvalidOperationException ItemCollectionAlreadyRegistered(DataSpace space) {
            return InvalidOperation(System.Data.Entity.Strings.ItemCollectionAlreadyRegistered(space.ToString()));
        }
        static internal InvalidOperationException NoCollectionForSpace(DataSpace space) {
            return InvalidOperation(System.Data.Entity.Strings.NoCollectionForSpace(space.ToString()));
        }
        static internal InvalidOperationException InvalidCollectionSpecified(DataSpace space) {
            return InvalidOperation(System.Data.Entity.Strings.InvalidCollectionSpecified(space));
        }
        static internal MetadataException DifferentSchemaVersionInCollection(string itemCollectionType, double versionToRegister, double currentSchemaVersion)
        {
            return Metadata(Strings.DifferentSchemaVersionInCollection(itemCollectionType, versionToRegister, currentSchemaVersion));
        }
        // TypeUsage.cs
        static internal ArgumentException NotBinaryTypeForTypeUsage() {
            return Argument(System.Data.Entity.Strings.NotBinaryTypeForTypeUsage);
        }
        static internal ArgumentException NotDateTimeTypeForTypeUsage() {
            return Argument(System.Data.Entity.Strings.NotDateTimeTypeForTypeUsage);
        }
        static internal ArgumentException NotDateTimeOffsetTypeForTypeUsage()
        {
            return Argument(System.Data.Entity.Strings.NotDateTimeOffsetTypeForTypeUsage);
        }
        static internal ArgumentException NotTimeTypeForTypeUsage()
        {
            return Argument(System.Data.Entity.Strings.NotTimeTypeForTypeUsage);
        }
        static internal ArgumentException NotDecimalTypeForTypeUsage() {
            return Argument(System.Data.Entity.Strings.NotDecimalTypeForTypeUsage);
        }
        static internal ArgumentException NotStringTypeForTypeUsage() {
            return Argument(System.Data.Entity.Strings.NotStringTypeForTypeUsage);
        }
        // EntityContainer.cs
        static internal ArgumentException InvalidEntitySetName(string name) {
            return Argument(System.Data.Entity.Strings.InvalidEntitySetName(name));
        }
        static internal ArgumentException InvalidRelationshipSetName(string name) {
            return Argument(System.Data.Entity.Strings.InvalidRelationshipSetName(name));
        }
        static internal ArgumentException InvalidEDMVersion(double edmVersion)
        {
            return Argument(System.Data.Entity.Strings.InvalidEDMVersion(edmVersion.ToString(CultureInfo.CurrentCulture)));
        }

        // EntitySetBaseCollection.cs
        static internal ArgumentException EntitySetInAnotherContainer(string parameter) {
            return Argument(System.Data.Entity.Strings.EntitySetInAnotherContainer, parameter);
        }
        // util.cs
        static internal InvalidOperationException OperationOnReadOnlyItem() {
            return InvalidOperation(System.Data.Entity.Strings.OperationOnReadOnlyItem);
        }
        //FacetDescription.cs
        static internal ArgumentException MinAndMaxValueMustBeSameForConstantFacet(string facetName, string typeName) {
            return Argument(System.Data.Entity.Strings.MinAndMaxValueMustBeSameForConstantFacet(facetName, typeName));
        }
        static internal ArgumentException MissingDefaultValueForConstantFacet(string facetName, string typeName) {
            return Argument(System.Data.Entity.Strings.MissingDefaultValueForConstantFacet(facetName, typeName));
        }
        static internal ArgumentException BothMinAndMaxValueMustBeSpecifiedForNonConstantFacet(string facetName, string typeName) {
            return Argument(System.Data.Entity.Strings.BothMinAndMaxValueMustBeSpecifiedForNonConstantFacet(facetName, typeName));
        }
        static internal ArgumentException MinAndMaxValueMustBeDifferentForNonConstantFacet(string facetName, string typeName) {
            return Argument(System.Data.Entity.Strings.MinAndMaxValueMustBeDifferentForNonConstantFacet(facetName, typeName));
        }
        static internal ArgumentException MinAndMaxMustBePositive(string facetName, string typeName) {
            return Argument(System.Data.Entity.Strings.MinAndMaxMustBePositive(facetName, typeName));
        }
        static internal ArgumentException MinMustBeLessThanMax(string minimumValue, string facetName, string typeName) {
            return Argument(System.Data.Entity.Strings.MinMustBeLessThanMax(minimumValue, facetName, typeName));
        }
        static internal ArgumentException EntitySetNotInCSpace(string name) {
            return Argument(System.Data.Entity.Strings.EntitySetNotInCSPace(name));
        }

        static internal ArgumentException TypeNotInEntitySet(string entitySetName, string rootEntityTypeName, string entityTypeName) {
            return Argument(System.Data.Entity.Strings.TypeNotInEntitySet(entityTypeName, rootEntityTypeName, entitySetName));
        }

        static internal ArgumentException AssociationSetNotInCSpace(string name) {
            return Argument(System.Data.Entity.Strings.EntitySetNotInCSPace(name));
        }

        static internal ArgumentException TypeNotInAssociationSet(string setName, string rootEntityTypeName, string typeName) {
            return Argument(System.Data.Entity.Strings.TypeNotInAssociationSet(typeName, rootEntityTypeName, setName));
        }
        #endregion //Metadata Errors

        #region Internal Errors

        // Internal error code to use with the InternalError exception.
        //
        // error numbers end up being hard coded in test cases; they can be removed, but should not be changed.
        // reusing error numbers is probably OK, but not recommended.
        //
        // The acceptable range for this enum is
        // 1000 - 1999
        //
        // The Range 10,000-15,000 is reserved for tools
        //
        /// You must never renumber these, because we rely upon them when
        /// we get an exception report once we release the bits.
        internal enum InternalErrorCode {
            WrongNumberOfKeys = 1000,
            UnknownColumnMapKind = 1001,
            NestOverNest = 1002,
            ColumnCountMismatch = 1003,

            /// <summary>
            /// Some assertion failed
            /// </summary>
            AssertionFailed = 1004,

            UnknownVar = 1005,
            WrongVarType = 1006,
            ExtentWithoutEntity = 1007,
            UnnestWithoutInput = 1008,
            UnnestMultipleCollections = 1009,
            CodeGen_NoSuchProperty = 1011,
            JoinOverSingleStreamNest = 1012,
            InvalidInternalTree = 1013,
            NameValuePairNext = 1014,
            InvalidParserState1 = 1015,
            InvalidParserState2 = 1016,
            /// <summary>
            /// Thrown when SQL gen produces parameters for anything other than a 
            /// modification command tree.
            /// </summary>
            SqlGenParametersNotPermitted = 1017,
            EntityKeyMissingKeyValue = 1018,
            /// <summary>
            /// Thrown when an invalid data request is presented to a PropagatorResult in
            /// the update pipeline (confusing simple/complex values, missing key values, etc.).
            /// </summary>
            UpdatePipelineResultRequestInvalid = 1019,
            InvalidStateEntry = 1020,
            /// <summary>
            /// Thrown when the update pipeline encounters an invalid PrimitiveTypeKind
            /// during a cast.
            /// </summary>
            InvalidPrimitiveTypeKind = 1021,
            /// <summary>
            /// Thrown when an unknown node type is encountered in ELinq expression translation.
            /// </summary>
            UnknownLinqNodeType = 1023,
            /// <summary>
            /// Thrown by result assembly upon encountering a collection column that does not use any columns
            /// nor has a descriminated nested collection.
            /// </summary>
            CollectionWithNoColumns = 1024,
            /// <summary>
            /// Thrown when a lambda expression argument has an unexpected node type.
            /// </summary>
            UnexpectedLinqLambdaExpressionFormat = 1025,
            /// <summary>
            /// Thrown when a CommandTree is defined on a stored procedure EntityCommand instance.
            /// </summary>
            CommandTreeOnStoredProcedureEntityCommand = 1026,
            /// <summary>
            /// Thrown when an operation in the BoolExpr library is exceeding anticipated complexity.
            /// </summary>
            BoolExprAssert = 1027,
            // AttemptToGenerateDefinitionForFunctionWithoutDef = 1028,
            /// <summary>
            /// Thrown when type A is promotable to type B, but ranking algorithm fails to rank the promotion.
            /// </summary>
            FailedToGeneratePromotionRank = 1029,
        }

        static internal Exception InternalError(InternalErrorCode internalError) {
            return InvalidOperation(System.Data.Entity.Strings.ADP_InternalProviderError((int)internalError));
        }

        static internal Exception InternalError(InternalErrorCode internalError, int location, object additionalInfo) {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}, {1}", (int)internalError, location);
            if (null != additionalInfo) {
                sb.AppendFormat(", {0}", additionalInfo);
            }
            return InvalidOperation(System.Data.Entity.Strings.ADP_InternalProviderError(sb.ToString()));
        }

        static internal Exception InternalError(InternalErrorCode internalError, int location) {
            return InternalError(internalError, location, null);
        }

        #endregion

        #region ObjectStateManager errors
        internal static InvalidOperationException OriginalValuesDoesNotExist() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_OriginalValuesDoesNotExist);
        }

        internal static InvalidOperationException CurrentValuesDoesNotExist() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_CurrentValuesDoesNotExist);
        }

        internal static ArgumentException InvalidTypeForComplexTypeProperty(string argument) {
            return EntityUtil.Argument(System.Data.Entity.Strings.ObjectStateEntry_InvalidTypeForComplexTypeProperty, argument);
        }

        internal static InvalidOperationException ObjectStateEntryinInvalidState() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_InvalidState);
        }

        internal static InvalidOperationException CantModifyDetachedDeletedEntries() {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_CantModifyDetachedDeletedEntries);
        }
        
        internal static InvalidOperationException SetModifiedStates(string methodName)
        {
            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_SetModifiedStates(methodName));
        }

        internal static InvalidOperationException EntityCantHaveMultipleChangeTrackers() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.Entity_EntityCantHaveMultipleChangeTrackers);
        }

        internal static InvalidOperationException CantModifyRelationValues() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_CantModifyRelationValues);
        }

        internal static InvalidOperationException CantModifyRelationState() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_CantModifyRelationState);
        }

        internal static InvalidOperationException CannotModifyKeyProperty(string fieldName) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_CannotModifyKeyProperty(fieldName));
        }

        internal static InvalidOperationException CantSetEntityKey() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_CantSetEntityKey);
        }

        internal static InvalidOperationException CannotAccessKeyEntryValues() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
        }

        internal static InvalidOperationException CannotModifyKeyEntryState() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_CannotModifyKeyEntryState);
        }


        internal static InvalidOperationException CannotCallDeleteOnKeyEntry() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_CannotDeleteOnKeyEntry);
        }

        internal static ArgumentException InvalidModifiedPropertyName(string propertyName) {
            return EntityUtil.Argument(System.Data.Entity.Strings.ObjectStateEntry_SetModifiedOnInvalidProperty(propertyName));
        }
        internal static InvalidOperationException NoEntryExistForEntityKey() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_NoEntryExistForEntityKey);
        }
        internal static ArgumentException DetachedObjectStateEntriesDoesNotExistInObjectStateManager() {
            return EntityUtil.Argument(System.Data.Entity.Strings.ObjectStateManager_DetachedObjectStateEntriesDoesNotExistInObjectStateManager);
        }

        internal static InvalidOperationException ObjectStateManagerContainsThisEntityKey() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_ObjectStateManagerContainsThisEntityKey);
        }
        internal static InvalidOperationException ObjectStateManagerDoesnotAllowToReAddUnchangedOrModifiedOrDeletedEntity(EntityState state) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_DoesnotAllowToReAddUnchangedOrModifiedOrDeletedEntity(state));
        }
        internal static InvalidOperationException CannotFixUpKeyToExistingValues() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_CannotFixUpKeyToExistingValues);
        }
        internal static InvalidOperationException KeyPropertyDoesntMatchValueInKey(bool forAttach)
        {
            if (forAttach)
            {
                return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_KeyPropertyDoesntMatchValueInKeyForAttach);
            }
            else
            {
                return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_KeyPropertyDoesntMatchValueInKey);
            }
        }

        internal static void CheckValidStateForChangeEntityState(EntityState state)
        {
            switch(state)
            {
                case EntityState.Added:
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Deleted:
                case EntityState.Detached:
                    break;
                default:
                    throw InvalidEntityStateArgument("state");
            }
        }

        internal static void CheckValidStateForChangeRelationshipState(EntityState state, string paramName)
        {
            switch (state)
            {
                case EntityState.Added:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                case EntityState.Detached:
                    break;
                default:
                    throw InvalidRelationshipStateArgument(paramName);
            }
        }

        internal static InvalidOperationException InvalidKey() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_InvalidKey);
        }
        internal static InvalidOperationException AcceptChangesEntityKeyIsNotValid() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_AcceptChangesEntityKeyIsNotValid);
        }
        internal static InvalidOperationException EntityConflictsWithKeyEntry() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_EntityConflictsWithKeyEntry);
        }
        internal static InvalidOperationException ObjectDoesNotHaveAKey(object entity) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_GetEntityKeyRequiresObjectToHaveAKey(entity.GetType().FullName));
        }
        internal static InvalidOperationException EntityValueChangedWithoutEntityValueChanging() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_EntityMemberChangedWithoutEntityMemberChanging);
        }
        internal static InvalidOperationException ChangedInDifferentStateFromChanging(EntityState currentState, EntityState previousState) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateEntry_ChangedInDifferentStateFromChanging(previousState, currentState));
        }
        internal static ArgumentException ChangeOnUnmappedProperty(string entityPropertyName) {
            return EntityUtil.Argument(System.Data.Entity.Strings.ObjectStateEntry_ChangeOnUnmappedProperty(entityPropertyName));
        }

        internal static ArgumentException ChangeOnUnmappedComplexProperty(string complexPropertyName) {
            return EntityUtil.Argument(System.Data.Entity.Strings.ObjectStateEntry_ChangeOnUnmappedComplexProperty(complexPropertyName));
        }

        internal static ArgumentException EntityTypeDoesNotMatchEntitySet(string entityType, string entitysetName, string argument) {
            return Argument(System.Data.Entity.Strings.ObjectStateManager_EntityTypeDoesnotMatchtoEntitySetType(entityType, entitysetName), argument);
        }
        internal static InvalidOperationException NoEntryExistsForObject(object entity)
        {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_NoEntryExistsForObject(entity.GetType().FullName));
        }
        internal static InvalidOperationException EntityNotTracked()
        {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectStateManager_EntityNotTracked);
        }
        internal static InvalidOperationException SetOriginalComplexProperties(string propertyName)
        {
            return InvalidOperation(Strings.ObjectStateEntry_SetOriginalComplexProperties(propertyName));
        }
        internal static InvalidOperationException NullOriginalValueForNonNullableProperty(string propertyName, string clrMemberName, string clrTypeName)
        {
            return InvalidOperation(Strings.ObjectStateEntry_NullOriginalValueForNonNullableProperty(propertyName, clrMemberName, clrTypeName));
        }
        internal static InvalidOperationException SetOriginalPrimaryKey(string propertyName)
        {
            return InvalidOperation(Strings.ObjectStateEntry_SetOriginalPrimaryKey(propertyName));
        }

        #endregion

        #region ObjectMaterializer errors

        internal static void ThrowPropertyIsNotNullable(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
            {

                throw EntityUtil.Constraint(
                  System.Data.Entity.Strings.Materializer_PropertyIsNotNullable);
            }
            else
            {
                throw EntityUtil.PropertyConstraint(
                    System.Data.Entity.Strings.Materializer_PropertyIsNotNullableWithName(propertyName), propertyName);
            }
        }

        internal static void ThrowSetInvalidValue(object value, Type destinationType, string className, string propertyName)
        {
            if (null == value)
            {
                throw EntityUtil.Constraint(
                    System.Data.Entity.Strings.Materializer_SetInvalidValue(
                        (Nullable.GetUnderlyingType(destinationType) ?? destinationType).Name,
                        className, propertyName, "null"));
            }
            else
            {
                throw EntityUtil.InvalidOperation(
                    System.Data.Entity.Strings.Materializer_SetInvalidValue(
                        (Nullable.GetUnderlyingType(destinationType) ?? destinationType).Name,
                        className, propertyName, value.GetType().Name));
            }
        }        
        internal static InvalidOperationException ValueInvalidCast(Type valueType, Type destinationType)
        {
            Debug.Assert(null != valueType, "null valueType");
            Debug.Assert(null != destinationType, "null destinationType");
            if (destinationType.IsValueType && destinationType.IsGenericType && (typeof(Nullable<>) == destinationType.GetGenericTypeDefinition()))
            {
                return EntityUtil.InvalidOperation(
                    System.Data.Entity.Strings.Materializer_InvalidCastNullable(
                        valueType, destinationType.GetGenericArguments()[0]));
            }
            else
            {
                return EntityUtil.InvalidOperation(
                    System.Data.Entity.Strings.Materializer_InvalidCastReference(
                        valueType, destinationType));
            }
        }
        internal static InvalidOperationException ValueNullReferenceCast(Type destinationType)
        {
            Debug.Assert(null != destinationType, "null value");
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.Materializer_NullReferenceCast(destinationType.Name));
        }

        internal static NotSupportedException RecyclingEntity(EntityKey key, Type newEntityType, Type existingEntityType) {
            return NotSupported(System.Data.Entity.Strings.Materializer_RecyclingEntity(System.Data.Common.TypeHelpers.GetFullName(key.EntityContainerName, key.EntitySetName), newEntityType.FullName, existingEntityType.FullName, key.ConcatKeyValue()));
        }
        internal static InvalidOperationException AddedEntityAlreadyExists(EntityKey key) {
            return InvalidOperation(System.Data.Entity.Strings.Materializer_AddedEntityAlreadyExists(key.ConcatKeyValue()));
        }
        internal static InvalidOperationException CannotReEnumerateQueryResults() {
            return InvalidOperation(System.Data.Entity.Strings.Materializer_CannotReEnumerateQueryResults);
        }
        internal static NotSupportedException MaterializerUnsupportedType() {
            return NotSupported(System.Data.Entity.Strings.Materializer_UnsupportedType);
        }
        #endregion

        #region ObjectView errors
        internal static InvalidOperationException CannotReplacetheEntityorRow() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectView_CannotReplacetheEntityorRow);
        }
        internal static NotSupportedException IndexBasedInsertIsNotSupported() {
            return NotSupported(System.Data.Entity.Strings.ObjectView_IndexBasedInsertIsNotSupported);
        }
        internal static InvalidOperationException WriteOperationNotAllowedOnReadOnlyBindingList() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectView_WriteOperationNotAllowedOnReadOnlyBindingList);
        }
        internal static InvalidOperationException AddNewOperationNotAllowedOnAbstractBindingList() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectView_AddNewOperationNotAllowedOnAbstractBindingList);
        }
        internal static ArgumentException IncompatibleArgument() {
            return Argument(System.Data.Entity.Strings.ObjectView_IncompatibleArgument);
        }
        internal static InvalidOperationException CannotResolveTheEntitySetforGivenEntity(Type type) {
            return InvalidOperation(System.Data.Entity.Strings.ObjectView_CannotResolveTheEntitySet(type.FullName));
        }
        

        #endregion
       

        #region EntityCollection Errors
        internal static InvalidOperationException NoRelationshipSetMatched(string relationshipName) {
            Debug.Assert(!String.IsNullOrEmpty(relationshipName), "empty relationshipName");
            return InvalidOperation(System.Data.Entity.Strings.Collections_NoRelationshipSetMatched(relationshipName));
        }        
        internal static InvalidOperationException ExpectedCollectionGotReference(string typeName, string roleName, string relationshipName) {
            return InvalidOperation(System.Data.Entity.Strings.Collections_ExpectedCollectionGotReference(typeName, roleName, relationshipName));
        }
        internal static InvalidOperationException CannotFillTryDifferentMergeOption(string relationshipName, string roleName) {
            return InvalidOperation(Strings.Collections_CannotFillTryDifferentMergeOption(relationshipName, roleName));
        }
        internal static InvalidOperationException CannotRemergeCollections() {
            return InvalidOperation(System.Data.Entity.Strings.Collections_UnableToMergeCollections);
        }
        internal static InvalidOperationException ExpectedReferenceGotCollection(string typeName, string roleName, string relationshipName) {
            return InvalidOperation(System.Data.Entity.Strings.EntityReference_ExpectedReferenceGotCollection(typeName, roleName, relationshipName));
        }
        internal static InvalidOperationException CannotAddMoreThanOneEntityToEntityReference(string roleName, string relationshipName) {
            return InvalidOperation(System.Data.Entity.Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(roleName, relationshipName));
        }
        internal static ArgumentException CannotSetSpecialKeys() {
            return Argument(System.Data.Entity.Strings.EntityReference_CannotSetSpecialKeys, "value");
        }
        internal static InvalidOperationException EntityKeyValueMismatch() {
            return InvalidOperation(System.Data.Entity.Strings.EntityReference_EntityKeyValueMismatch);
        }
        internal static InvalidOperationException RelatedEndNotAttachedToContext(string relatedEndType) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_RelatedEndNotAttachedToContext(relatedEndType));
        }
        internal static InvalidOperationException CannotCreateRelationshipBetweenTrackedAndNoTrackedEntities(string roleName) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_CannotCreateRelationshipBetweenTrackedAndNoTrackedEntities(roleName));
        }
        internal static InvalidOperationException CannotCreateRelationshipEntitiesInDifferentContexts() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_CannotCreateRelationshipEntitiesInDifferentContexts);
        }
        internal static InvalidOperationException InvalidContainedTypeCollection(string entityType, string relatedEndType) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_InvalidContainedType_Collection(entityType, relatedEndType));
        }
        internal static InvalidOperationException InvalidContainedTypeReference(string entityType, string relatedEndType) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_InvalidContainedType_Reference(entityType, relatedEndType));
        }
        internal static InvalidOperationException CannotAddToFixedSizeArray(object collectionType) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_CannotAddToFixedSizeArray(collectionType.GetType()));
        }
        internal static InvalidOperationException CannotRemoveFromFixedSizeArray(object collectionType) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_CannotRemoveFromFixedSizeArray(collectionType.GetType()));
        }
        internal static InvalidOperationException OwnerIsNull() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_OwnerIsNull);
        }
        internal static InvalidOperationException UnableToAddRelationshipWithDeletedEntity() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_UnableToAddRelationshipWithDeletedEntity);
        }
        internal static InvalidOperationException ConflictingChangeOfRelationshipDetected() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_ConflictingChangeOfRelationshipDetected);
        }
        internal static InvalidOperationException InvalidRelationshipFixupDetected(string propertyName, string entityType) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_InvalidRelationshipFixupDetected(propertyName, entityType));
        }
        internal static InvalidOperationException LessThanExpectedRelatedEntitiesFound()
        {
            return InvalidOperation(System.Data.Entity.Strings.EntityReference_LessThanExpectedRelatedEntitiesFound);
        }
        internal static InvalidOperationException MoreThanExpectedRelatedEntitiesFound() {
            return InvalidOperation(System.Data.Entity.Strings.EntityReference_MoreThanExpectedRelatedEntitiesFound);
        }
        internal static InvalidOperationException CannotChangeReferentialConstraintProperty() {
            return InvalidOperation(System.Data.Entity.Strings.EntityReference_CannotChangeReferentialConstraintProperty);
        }
        internal static InvalidOperationException RelatedEndNotFound() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_RelatedEndNotFound);
        }
        internal static InvalidOperationException LoadCalledOnNonEmptyNoTrackedRelatedEnd() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_LoadCalledOnNonEmptyNoTrackedRelatedEnd);
        }
        internal static InvalidOperationException LoadCalledOnAlreadyLoadedNoTrackedRelatedEnd() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_LoadCalledOnAlreadyLoadedNoTrackedRelatedEnd);
        }   
        internal static InvalidOperationException MismatchedMergeOptionOnLoad(MergeOption mergeOption) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_MismatchedMergeOptionOnLoad(mergeOption));
        }
        internal static InvalidOperationException EntitySetIsNotValidForRelationship(string entitySetContainerName, string entitySetName, string roleName, string associationSetContainerName, string associationSetName) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_EntitySetIsNotValidForRelationship(entitySetContainerName, entitySetName, roleName, associationSetContainerName, associationSetName));
        }
        internal static InvalidOperationException UnableToRetrieveReferentialConstraintProperties() {
            return InvalidOperation(System.Data.Entity.Strings.RelationshipManager_UnableToRetrieveReferentialConstraintProperties);
        }
        internal static InvalidOperationException InconsistentReferentialConstraintProperties() {
            return InvalidOperation(System.Data.Entity.Strings.RelationshipManager_InconsistentReferentialConstraintProperties);
        }
        internal static InvalidOperationException CircularRelationshipsWithReferentialConstraints() {
            return InvalidOperation(System.Data.Entity.Strings.RelationshipManager_CircularRelationshipsWithReferentialConstraints);
        }
        internal static ArgumentException UnableToFindRelationshipTypeInMetadata(string relationshipName, string parameterName) {
            return Argument(System.Data.Entity.Strings.RelationshipManager_UnableToFindRelationshipTypeInMetadata(relationshipName), parameterName);
        }
        internal static ArgumentException InvalidTargetRole(string relationshipName, string targetRoleName, string parameterName) {
            return Argument(System.Data.Entity.Strings.RelationshipManager_InvalidTargetRole(relationshipName, targetRoleName), parameterName);
        }
        internal static InvalidOperationException OwnerIsNotSourceType(string ownerType, string sourceRoleType, string sourceRoleName, string relationshipName) {
            return InvalidOperation(System.Data.Entity.Strings.RelationshipManager_OwnerIsNotSourceType(ownerType, sourceRoleType, sourceRoleName, relationshipName));
        }                
        internal static InvalidOperationException UnexpectedNullContext() {
            return InvalidOperation(System.Data.Entity.Strings.RelationshipManager_UnexpectedNullContext);
        }
        internal static InvalidOperationException ReferenceAlreadyInitialized() {
            return InvalidOperation(System.Data.Entity.Strings.RelationshipManager_ReferenceAlreadyInitialized(System.Data.Entity.Strings.RelationshipManager_InitializeIsForDeserialization));
        }        
        internal static InvalidOperationException RelationshipManagerAttached() {
            return InvalidOperation(System.Data.Entity.Strings.RelationshipManager_RelationshipManagerAttached(System.Data.Entity.Strings.RelationshipManager_InitializeIsForDeserialization));
        }
        internal static InvalidOperationException CollectionAlreadyInitialized() {
            return InvalidOperation(System.Data.Entity.Strings.RelationshipManager_CollectionAlreadyInitialized(System.Data.Entity.Strings.RelationshipManager_CollectionInitializeIsForDeserialization));
        }
        internal static InvalidOperationException CollectionRelationshipManagerAttached() {
            return InvalidOperation(System.Data.Entity.Strings.RelationshipManager_CollectionRelationshipManagerAttached(System.Data.Entity.Strings.RelationshipManager_CollectionInitializeIsForDeserialization));
        }
        internal static void CheckContextNull(ObjectContext context)
        {
            if ((object)context == null)
            {
                throw EntityUtil.UnexpectedNullContext();
            }
        }

        internal static void CheckArgumentMergeOption(MergeOption mergeOption) {
            switch(mergeOption) {
            case MergeOption.NoTracking:
            case MergeOption.AppendOnly:
            case MergeOption.OverwriteChanges:
            case MergeOption.PreserveChanges:
                break;
            default:
                throw EntityUtil.InvalidMergeOption(mergeOption);
            }
        }
        internal static void CheckArgumentRefreshMode(RefreshMode refreshMode) {
            switch(refreshMode) {
            case RefreshMode.ClientWins:
            case RefreshMode.StoreWins:
                break;
            default:
                throw EntityUtil.InvalidRefreshMode(refreshMode);
            }
        }
        internal static InvalidOperationException InvalidEntityStateSource() {
            return InvalidOperation(System.Data.Entity.Strings.Collections_InvalidEntityStateSource);
        }
        internal static InvalidOperationException InvalidEntityStateLoad(string relatedEndType) {
            return InvalidOperation(System.Data.Entity.Strings.Collections_InvalidEntityStateLoad(relatedEndType));
	    }
        internal static InvalidOperationException InvalidOwnerStateForAttach() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_InvalidOwnerStateForAttach);
        }
        internal static InvalidOperationException InvalidNthElementNullForAttach(int index) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_InvalidNthElementNullForAttach(index));
        }
        internal static InvalidOperationException InvalidNthElementContextForAttach(int index) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_InvalidNthElementContextForAttach(index));
        }
        internal static InvalidOperationException InvalidNthElementStateForAttach(int index) {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_InvalidNthElementStateForAttach(index));
        }
        internal static InvalidOperationException InvalidEntityContextForAttach() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_InvalidEntityContextForAttach);
        }
        internal static InvalidOperationException InvalidEntityStateForAttach() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_InvalidEntityStateForAttach);
        }
        internal static InvalidOperationException UnableToAddToDisconnectedRelatedEnd() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_UnableToAddEntity);
        }
        internal static InvalidOperationException UnableToRemoveFromDisconnectedRelatedEnd() {
            return InvalidOperation(System.Data.Entity.Strings.RelatedEnd_UnableToRemoveEntity);
        }
        internal static InvalidOperationException ProxyMetadataIsUnavailable(Type type, Exception inner) {
            return InvalidOperation(System.Data.Entity.Strings.EntityProxyTypeInfo_ProxyMetadataIsUnavailable(type.FullName), inner);
        }
        internal static InvalidOperationException DuplicateTypeForProxyType(Type type) {
            return InvalidOperation(System.Data.Entity.Strings.EntityProxyTypeInfo_DuplicateOSpaceType(type.FullName));
        }
        #endregion

        #region ObjectContext errors
        internal static InvalidOperationException ClientEntityRemovedFromStore(string entitiesKeys) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_ClientEntityRemovedFromStore(entitiesKeys));
        }
        internal static InvalidOperationException StoreEntityNotPresentInClient() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_StoreEntityNotPresentInClient);
        }
        internal static InvalidOperationException ContextMetadataHasChanged() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_MetadataHasChanged);
        }
        internal static ArgumentException InvalidConnection(bool isConnectionConstructor, Exception innerException)
        {
            if (isConnectionConstructor)
            {
                return InvalidConnection("connection", innerException);
            }
            else
            {
                return InvalidConnectionString("connectionString", innerException);
            }
        }
        internal static ArgumentException InvalidConnectionString(string parameter, Exception inner) {
            return EntityUtil.Argument(System.Data.Entity.Strings.ObjectContext_InvalidConnectionString, parameter, inner);
        }
        internal static ArgumentException InvalidConnection(string parameter, Exception inner) {
            return EntityUtil.Argument(System.Data.Entity.Strings.ObjectContext_InvalidConnection, parameter, inner);
        }
        internal static InvalidOperationException InvalidDataAdapter() {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_InvalidDataAdapter);
        }
        internal static ArgumentException InvalidDefaultContainerName(string parameter, string defaultContainerName) {
            return EntityUtil.Argument(System.Data.Entity.Strings.ObjectContext_InvalidDefaultContainerName(defaultContainerName), parameter);
        }
        internal static InvalidOperationException NthElementInAddedState(int i) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_NthElementInAddedState(i));
        }
        internal static InvalidOperationException NthElementIsDuplicate(int i) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_NthElementIsDuplicate(i));
        }
        internal static InvalidOperationException NthElementIsNull(int i) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_NthElementIsNull(i));
        }
        internal static InvalidOperationException NthElementNotInObjectStateManager(int i) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_NthElementNotInObjectStateManager(i));
        }
        internal static ObjectDisposedException ObjectContextDisposed() {
            return EntityUtil.ObjectDisposed(System.Data.Entity.Strings.ObjectContext_ObjectDisposed);
        }
        internal static ObjectNotFoundException ObjectNotFound() {
            return EntityUtil.ObjectNotFound(System.Data.Entity.Strings.ObjectContext_ObjectNotFound);
        }
        internal static InvalidOperationException InvalidEntityType(Type type) {
            Debug.Assert(type != null, "The type cannot be null.");
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_NoMappingForEntityType(type.FullName));
        }
        internal static InvalidOperationException CannotDeleteEntityNotInObjectStateManager() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_CannotDeleteEntityNotInObjectStateManager);
        }
        internal static InvalidOperationException CannotDetachEntityNotInObjectStateManager() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_CannotDetachEntityNotInObjectStateManager);
        }
        internal static InvalidOperationException EntitySetNotFoundForName(string entitySetName) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_EntitySetNotFoundForName(entitySetName));
        }
        internal static InvalidOperationException EntityContainterNotFoundForName(string entityContainerName) {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_EntityContainerNotFoundForName(entityContainerName));
        }
        internal static ArgumentException InvalidCommandTimeout(string argument) {
            return Argument(System.Data.Entity.Strings.ObjectContext_InvalidCommandTimeout, argument);
        }
        internal static InvalidOperationException EntityAlreadyExistsInObjectStateManager() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_EntityAlreadyExistsInObjectStateManager);
        }
        internal static InvalidOperationException InvalidEntitySetInKey(string keyContainer, string keyEntitySet, string expectedContainer, string expectedEntitySet) {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_InvalidEntitySetInKey(keyContainer, keyEntitySet, expectedContainer, expectedEntitySet));
        }
        internal static InvalidOperationException InvalidEntitySetInKeyFromName(string keyContainer, string keyEntitySet, string expectedContainer, string expectedEntitySet, string argument) {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_InvalidEntitySetInKeyFromName(keyContainer, keyEntitySet, expectedContainer, expectedEntitySet, argument));
        }
        internal static InvalidOperationException CannotAttachEntityWithoutKey() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_CannotAttachEntityWithoutKey);
        }
        internal static InvalidOperationException CannotAttachEntityWithTemporaryKey() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_CannotAttachEntityWithTemporaryKey);
        }
        internal static InvalidOperationException EntitySetNameOrEntityKeyRequired() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_EntitySetNameOrEntityKeyRequired);
        }
        internal static InvalidOperationException ExecuteFunctionTypeMismatch(Type typeArgument, EdmType expectedElementType) {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_ExecuteFunctionTypeMismatch(
                typeArgument.FullName,
                expectedElementType.FullName));
        }
        internal static InvalidOperationException ExecuteFunctionCalledWithNonReaderFunction(EdmFunction functionImport) {
            // report ExecuteNonQuery return type if no explicit return type is given
            string message;
            if (null == functionImport.ReturnParameter)
            {
                message = System.Data.Entity.Strings.ObjectContext_ExecuteFunctionCalledWithNonQueryFunction(
                    functionImport.Name);
            }
            else
            {
                message = System.Data.Entity.Strings.ObjectContext_ExecuteFunctionCalledWithScalarFunction(
                    functionImport.ReturnParameter.TypeUsage.EdmType.FullName, functionImport.Name);
            }
            return InvalidOperation(message);
        }        
        internal static ArgumentException QualfiedEntitySetName(string parameterName) {
            return Argument(System.Data.Entity.Strings.ObjectContext_QualfiedEntitySetName, parameterName);
        }
        internal static ArgumentException ContainerQualifiedEntitySetNameRequired(string argument) {
            return Argument(System.Data.Entity.Strings.ObjectContext_ContainerQualifiedEntitySetNameRequired, argument);
        }
        internal static InvalidOperationException CannotSetDefaultContainerName() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_CannotSetDefaultContainerName);
        }
        internal static ArgumentException EntitiesHaveDifferentType(string originalEntityTypeName, string changedEntityTypeName) {
            return Argument(System.Data.Entity.Strings.ObjectContext_EntitiesHaveDifferentType(originalEntityTypeName, changedEntityTypeName));
        }
        internal static InvalidOperationException EntityMustBeUnchangedOrModified(EntityState state) {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_EntityMustBeUnchangedOrModified(state.ToString()));
        }
        internal static InvalidOperationException EntityMustBeUnchangedOrModifiedOrDeleted(EntityState state) {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_EntityMustBeUnchangedOrModifiedOrDeleted(state.ToString()));
        }
        internal static InvalidOperationException EntityNotTrackedOrHasTempKey()
        {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectContext_EntityNotTrackedOrHasTempKey);
        }

        internal static InvalidOperationException AcceptAllChangesFailure(Exception e) {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_AcceptAllChangesFailure(e.Message));
        }
        internal static ArgumentException InvalidEntitySetOnEntity(string entitySetName, Type entityType, string parameter) {
            return Argument(System.Data.Entity.Strings.ObjectContext_InvalidEntitySetOnEntity(entitySetName, entityType), parameter);
        }
        internal static ArgumentException InvalidEntityTypeForObjectSet(string tEntityType, string entitySetType, string entitySetName, string parameter) {
            return Argument(System.Data.Entity.Strings.ObjectContext_InvalidObjectSetTypeForEntitySet(tEntityType, entitySetType, entitySetName), parameter);
        }
        internal static InvalidOperationException RequiredMetadataNotAvailable() {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_RequiredMetadataNotAvailble);
        }

        internal static ArgumentException MultipleEntitySetsFoundInSingleContainer(string entityTypeName, string entityContainerName, string exceptionParameterName) {
            return Argument(System.Data.Entity.Strings.ObjectContext_MultipleEntitySetsFoundInSingleContainer(entityTypeName, entityContainerName), exceptionParameterName);
        }

        internal static ArgumentException MultipleEntitySetsFoundInAllContainers(string entityTypeName, string exceptionParameterName) {
            return Argument(System.Data.Entity.Strings.ObjectContext_MultipleEntitySetsFoundInAllContainers(entityTypeName), exceptionParameterName);
        }

        internal static ArgumentException NoEntitySetFoundForType(string entityTypeName, string exceptionParameterName) {
            return Argument(System.Data.Entity.Strings.ObjectContext_NoEntitySetFoundForType(entityTypeName), exceptionParameterName);
        }
        internal static InvalidOperationException EntityNotInObjectSet_Delete(string actualContainerName, string actualEntitySetName, string expectedContainerName, string expectedEntitySetName) {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_EntityNotInObjectSet_Delete(actualContainerName, actualEntitySetName, expectedContainerName, expectedEntitySetName));
        }

        internal static InvalidOperationException EntityNotInObjectSet_Detach(string actualContainerName, string actualEntitySetName, string expectedContainerName, string expectedEntitySetName) {
            return InvalidOperation(System.Data.Entity.Strings.ObjectContext_EntityNotInObjectSet_Detach(actualContainerName, actualEntitySetName, expectedContainerName, expectedEntitySetName));
        }

        internal static ArgumentException InvalidRelationshipStateArgument(string paramName)
        {
            return new ArgumentException(Strings.ObjectContext_InvalidRelationshipState, paramName);
        }

        internal static ArgumentException InvalidEntityStateArgument(string paramName)
        {
            return new ArgumentException(Strings.ObjectContext_InvalidEntityState, paramName);
        }

        #endregion

        #region Complex Types Errors
        // Complex types exceptions
        internal static InvalidOperationException NullableComplexTypesNotSupported(string propertyName)
        {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ComplexObject_NullableComplexTypesNotSupported(propertyName));
        }
        internal static InvalidOperationException ComplexObjectAlreadyAttachedToParent()
        {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.ComplexObject_ComplexObjectAlreadyAttachedToParent);
        }
        internal static ArgumentException ComplexChangeRequestedOnScalarProperty(string propertyName)
        {
            return EntityUtil.Argument(System.Data.Entity.Strings.ComplexObject_ComplexChangeRequestedOnScalarProperty(propertyName));
        }
        #endregion

        internal static ArgumentException SpanPathSyntaxError() {
            return Argument(System.Data.Entity.Strings.ObjectQuery_Span_SpanPathSyntaxError);
        }

        /// <summary>
        /// This is only used for Include path argument, thus the parameter name is hardcoded to "path"
        /// </summary>
        /// <returns></returns>
        static internal ArgumentException ADP_InvalidMultipartNameDelimiterUsage()
        {
            return Argument(System.Data.Entity.Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
        }

        static internal Exception InvalidConnectionOptionValue(string key) 
        {
            return Argument(System.Data.Entity.Strings.ADP_InvalidConnectionOptionValue(key));
        }

        static internal ArgumentException InvalidSizeValue(int value)
        {
            return Argument(System.Data.Entity.Strings.ADP_InvalidSizeValue(value.ToString(CultureInfo.InvariantCulture)));
        }

        static internal ArgumentException ConnectionStringSyntax(int index)
        {
            return Argument(System.Data.Entity.Strings.ADP_ConnectionStringSyntax(index));
        }

        internal static InvalidOperationException DataRecordMustBeEntity() {
            return InvalidOperation(System.Data.Entity.Strings.EntityKey_DataRecordMustBeEntity);
        }
        internal static ArgumentException EntitySetDoesNotMatch(string argument, string entitySetName) {
            return Argument(System.Data.Entity.Strings.EntityKey_EntitySetDoesNotMatch(entitySetName), argument);
        }
        internal static InvalidOperationException EntityTypesDoNotMatch(string recordType, string entitySetType) {
            return InvalidOperation(System.Data.Entity.Strings.EntityKey_EntityTypesDoNotMatch(recordType, entitySetType));
        }
        internal static ArgumentException IncorrectNumberOfKeyValuePairs(string argument, string typeName, int expectedNumFields, int actualNumFields) {
            return Argument(System.Data.Entity.Strings.EntityKey_IncorrectNumberOfKeyValuePairs(typeName, expectedNumFields, actualNumFields), argument);
        }
        internal static InvalidOperationException IncorrectNumberOfKeyValuePairsInvalidOperation(string typeName, int expectedNumFields, int actualNumFields)
        {
            return InvalidOperation(System.Data.Entity.Strings.EntityKey_IncorrectNumberOfKeyValuePairs(typeName, expectedNumFields, actualNumFields));
        }
        internal static ArgumentException IncorrectValueType(string argument, string keyField, string expectedTypeName, string actualTypeName) {
            return Argument(System.Data.Entity.Strings.EntityKey_IncorrectValueType(keyField, expectedTypeName, actualTypeName), argument);
        }
        internal static InvalidOperationException IncorrectValueTypeInvalidOperation(string keyField, string expectedTypeName, string actualTypeName) {
            return InvalidOperation(System.Data.Entity.Strings.EntityKey_IncorrectValueType(keyField, expectedTypeName, actualTypeName));
        }
        internal static ArgumentException NoCorrespondingOSpaceTypeForEnumKeyField(string argument, string keyField, string cspaceTypeName) {
            return Argument(System.Data.Entity.Strings.EntityKey_NoCorrespondingOSpaceTypeForEnumKeyMember(keyField, cspaceTypeName), argument);
        }
        internal static InvalidOperationException NoCorrespondingOSpaceTypeForEnumKeyFieldInvalidOperation(string keyField, string cspaceTypeName) {
            return InvalidOperation(System.Data.Entity.Strings.EntityKey_NoCorrespondingOSpaceTypeForEnumKeyMember(keyField, cspaceTypeName));
        }
        internal static ArgumentException MissingKeyValue(string argument, string keyField, string typeName) {
            return MissingKeyValue(argument, keyField, typeName, null);
        }
        internal static ArgumentException MissingKeyValue(string argument, string keyField, string typeName, Exception inner) {
            return Argument(System.Data.Entity.Strings.EntityKey_MissingKeyValue(keyField, typeName), argument);
        }
        internal static InvalidOperationException NullKeyValue(string keyField, string typeName)
        {
            return InvalidOperation(System.Data.Entity.Strings.EntityKey_NullKeyValue(keyField, typeName));
        }
        internal static InvalidOperationException MissingKeyValueInvalidOperation(string keyField, string typeName)
        {
            return InvalidOperation(System.Data.Entity.Strings.EntityKey_MissingKeyValue(keyField, typeName));
        }
        internal static ArgumentException NoNullsAllowedInKeyValuePairs(string argument) {
            return Argument(System.Data.Entity.Strings.EntityKey_NoNullsAllowedInKeyValuePairs, argument);
        }
        internal static ArgumentException EntityKeyMustHaveValues(string argument) {
            return Argument(System.Data.Entity.Strings.EntityKey_EntityKeyMustHaveValues, argument);
        }
        internal static ArgumentException InvalidQualifiedEntitySetName() {
            return Argument(System.Data.Entity.Strings.EntityKey_InvalidQualifiedEntitySetName, "qualifiedEntitySetName");
        }
        internal static ArgumentException EntityKeyInvalidName(string invalidName) {
            return Argument(System.Data.Entity.Strings.EntityKey_InvalidName(invalidName));
        }
        internal static InvalidOperationException MissingQualifiedEntitySetName() {
            return InvalidOperation(System.Data.Entity.Strings.EntityKey_MissingEntitySetName);
        }
        internal static InvalidOperationException CannotChangeEntityKey() {
            return InvalidOperation(System.Data.Entity.Strings.EntityKey_CannotChangeKey);
        }

        internal static InvalidOperationException UnexpectedNullEntityKey()
        {
            return new InvalidOperationException(System.Data.Entity.Strings.EntityKey_UnexpectedNull);
        }
        internal static InvalidOperationException EntityKeyDoesntMatchKeySetOnEntity(object entity)
        {
            return new InvalidOperationException(System.Data.Entity.Strings.EntityKey_DoesntMatchKeyOnEntity(entity.GetType().FullName));
        }
        internal static void CheckEntityKeyNull(EntityKey entityKey)
        {
            if ((object)entityKey == null)
            {
                throw EntityUtil.UnexpectedNullEntityKey();
            }
        }
        internal static void CheckEntityKeysMatch(IEntityWrapper wrappedEntity, EntityKey key)
        {
            if (wrappedEntity.EntityKey != key)
            {
                throw EntityUtil.EntityKeyDoesntMatchKeySetOnEntity(wrappedEntity.Entity);
            }
        }
        internal static InvalidOperationException UnexpectedNullRelationshipManager()
        {
            return new InvalidOperationException(System.Data.Entity.Strings.RelationshipManager_UnexpectedNull);
        }
        internal static InvalidOperationException InvalidRelationshipManagerOwner()
        {
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.RelationshipManager_InvalidRelationshipManagerOwner);
        }

        internal static void ValidateEntitySetInKey(EntityKey key, EntitySet entitySet)
        {
            ValidateEntitySetInKey(key, entitySet, null);
        }
        internal static void ValidateEntitySetInKey(EntityKey key, EntitySet entitySet, string argument)
        {
            Debug.Assert(null != (object)key, "Null entity key");
            Debug.Assert(null != entitySet, "Null entity set");
            Debug.Assert(null != entitySet.EntityContainer, "Null entity container in the entity set");

            string containerName1 = key.EntityContainerName;
            string setName1 = key.EntitySetName;
            string containerName2 = entitySet.EntityContainer.Name;
            string setName2 = entitySet.Name;

            if (!StringComparer.Ordinal.Equals(containerName1, containerName2) ||
                !StringComparer.Ordinal.Equals(setName1, setName2))
            {
                if (String.IsNullOrEmpty(argument))
                {
                    throw EntityUtil.InvalidEntitySetInKey(
                        containerName1, setName1,
                        containerName2, setName2);
                }
                else
                {
                    throw EntityUtil.InvalidEntitySetInKeyFromName(
                        containerName1, setName1,
                        containerName2, setName2, argument);
                }
            }
        }



        // IDataParameter.Direction
        static internal ArgumentOutOfRangeException InvalidMergeOption(MergeOption value) {
#if DEBUG
            switch(value) {
            case MergeOption.NoTracking:
            case MergeOption.OverwriteChanges:
            case MergeOption.PreserveChanges:
            case MergeOption.AppendOnly:
                Debug.Assert(false, "valid MergeOption " + value.ToString());
                break;
            }
#endif
            return InvalidEnumerationValue(typeof(MergeOption), (int) value);
        }

        static internal ArgumentOutOfRangeException InvalidRefreshMode(RefreshMode value) {
#if DEBUG
            switch(value) {
            case RefreshMode.ClientWins:
            case RefreshMode.StoreWins:
                Debug.Assert(false, "valid RefreshMode " + value.ToString());
                break;
            }
#endif
            return InvalidEnumerationValue(typeof(RefreshMode), (int) value);
        }

        //
        // : IDataParameter
        //
        static internal ArgumentException InvalidDataType(TypeCode typecode) {
            return Argument(System.Data.Entity.Strings.ADP_InvalidDataType(typecode.ToString()));
        }

        static internal ArgumentException UnknownDataTypeCode(Type dataType, TypeCode typeCode) {
            return Argument(System.Data.Entity.Strings.ADP_UnknownDataTypeCode(((int) typeCode).ToString(CultureInfo.InvariantCulture), dataType.FullName));
        }

        static internal ArgumentOutOfRangeException InvalidParameterDirection(ParameterDirection value)
        {
#if DEBUG
            switch (value)
            {
                case ParameterDirection.Input:
                case ParameterDirection.Output:
                case ParameterDirection.InputOutput:
                case ParameterDirection.ReturnValue:
                    Debug.Assert(false, "valid ParameterDirection " + value.ToString());
                    break;
            }
#endif
            return InvalidEnumerationValue(typeof(ParameterDirection), (int)value);
        }
        static internal ArgumentOutOfRangeException InvalidDataRowVersion(DataRowVersion value)
        {
#if DEBUG
            switch (value)
            {
                case DataRowVersion.Default:
                case DataRowVersion.Current:
                case DataRowVersion.Original:
                case DataRowVersion.Proposed:
                    Debug.Assert(false, "valid DataRowVersion " + value.ToString());
                    break;
            }
#endif

            return InvalidEnumerationValue(typeof(DataRowVersion), (int)value);
        }
        //
        // UpdateException
        //
        static private IEnumerable<ObjectStateEntry> ProcessStateEntries(IEnumerable<IEntityStateEntry> stateEntries)
        {
            return stateEntries
            // In a future release, IEntityStateEntry will be public so we will be able to throw exceptions 
            // with this more general type. For now we cast to ObjectStateEntry (the only implementation
            // of the internal interface).
                .Cast<ObjectStateEntry>()
            // Return distinct entries (no need to report an entry multiple times even if it contributes
            // to the exception in multiple ways)
                .Distinct();
        }
        static internal void ValidateNecessaryModificationFunctionMapping(
                StorageModificationFunctionMapping mapping, string currentState,
                IEntityStateEntry stateEntry, string type, string typeName)
        {
            if (null == mapping)
            {
                throw EntityUtil.Update(Strings.Update_MissingFunctionMapping(currentState, type, typeName),
                null,
                new List<IEntityStateEntry>() { stateEntry });
            }
        }
        static internal UpdateException Update(string message, Exception innerException, params IEntityStateEntry[] stateEntries) {
            return Update(message, innerException, (IEnumerable<IEntityStateEntry>)stateEntries);
        }
        static internal UpdateException Update(string message, Exception innerException, IEnumerable<IEntityStateEntry> stateEntries)
        {
            return new UpdateException(message, innerException, ProcessStateEntries(stateEntries));
        }
        static internal OptimisticConcurrencyException UpdateConcurrency(long rowsAffected, Exception innerException, IEnumerable<IEntityStateEntry> stateEntries)
        {
            string message = System.Data.Entity.Strings.Update_ConcurrencyError(rowsAffected);
            return new OptimisticConcurrencyException(message, innerException, ProcessStateEntries(stateEntries));
        }
        static internal UpdateException UpdateRelationshipCardinalityConstraintViolation(string relationshipSetName,
            int minimumCount, int? maximumCount, string entitySetName, int actualCount, string otherEndPluralName, IEntityStateEntry stateEntry) {
            string minimumCountString = ConvertCardinalityToString(minimumCount);
            string maximumCountString = ConvertCardinalityToString(maximumCount);
            string actualCountString = ConvertCardinalityToString(actualCount);
            if (minimumCount == 1 && (minimumCountString == maximumCountString))
            {
                // Just one acceptable value and itis value is 1
                return Update(System.Data.Entity.Strings.Update_RelationshipCardinalityConstraintViolationSingleValue(
                    entitySetName, relationshipSetName, actualCountString, otherEndPluralName,
                    minimumCountString), null, stateEntry);
            }
            else
            {
                // Range of acceptable values
                return Update(System.Data.Entity.Strings.Update_RelationshipCardinalityConstraintViolation(
                    entitySetName, relationshipSetName, actualCountString, otherEndPluralName,
                    minimumCountString, maximumCountString), null, stateEntry);
            }
        }
        static internal UpdateException UpdateEntityMissingConstraintViolation(string relationshipSetName, string endName, IEntityStateEntry stateEntry) {
            string message = System.Data.Entity.Strings.Update_MissingRequiredEntity(relationshipSetName, stateEntry.State, endName);
            return Update(message, null, stateEntry);
        }
        static private string ConvertCardinalityToString(int? cardinality) {
            string result;
            if (!cardinality.HasValue) { // null indicates * (unlimited)
                result = "*";
            }
            else {
                result = cardinality.Value.ToString(CultureInfo.CurrentCulture);
            }
            return result;
        }
        static internal UpdateException UpdateMissingEntity(string relationshipSetName, string entitySetName) {
            return Update(System.Data.Entity.Strings.Update_MissingEntity(relationshipSetName, entitySetName), null);
        }

        static internal ArgumentException CollectionParameterElementIsNull(string parameterName) {
            return Argument(System.Data.Entity.Strings.ADP_CollectionParameterElementIsNull(parameterName));
        }
        static internal ArgumentException CollectionParameterElementIsNullOrEmpty(string parameterName) {
            return Argument(System.Data.Entity.Strings.ADP_CollectionParameterElementIsNullOrEmpty(parameterName));
        }

        static internal InvalidOperationException FunctionHasNoDefinition(EdmFunction function)
        {
            return InvalidOperation(System.Data.Entity.Strings.Cqt_UDF_FunctionHasNoDefinition(function.Identity));
        }

        static internal InvalidOperationException FunctionDefinitionResultTypeMismatch(EdmFunction function, TypeUsage generatedDefinitionResultType)
        {
            return InvalidOperation(System.Data.Entity.Strings.Cqt_UDF_FunctionDefinitionResultTypeMismatch(
                TypeHelpers.GetFullName(function.ReturnParameter.TypeUsage),
                function.FullName,
                TypeHelpers.GetFullName(generatedDefinitionResultType)));
        }

        static internal Exception EntityParameterCollectionInvalidIndex(int index, int count)
        {
            return new IndexOutOfRangeException(System.Data.Entity.Strings.EntityParameterCollectionInvalidIndex(index.ToString(CultureInfo.InvariantCulture), count.ToString(CultureInfo.InvariantCulture)));
        }
        static internal Exception EntityParameterCollectionInvalidParameterName(string parameterName)
        {
            return new IndexOutOfRangeException(System.Data.Entity.Strings.EntityParameterCollectionInvalidParameterName(parameterName));
        }
        static internal Exception EntityParameterNull(string parameter)
        {
            return new ArgumentNullException(parameter, System.Data.Entity.Strings.EntityParameterNull);
        }
        static internal Exception InvalidEntityParameterType(object invalidValue)
        {
            return new InvalidCastException(System.Data.Entity.Strings.InvalidEntityParameterType(invalidValue.GetType().Name));
        }
        static internal ArgumentException EntityParameterCollectionRemoveInvalidObject()
        {
            return new ArgumentException(System.Data.Entity.Strings.EntityParameterCollectionRemoveInvalidObject); 
        }
        static internal ArgumentException EntityParameterContainedByAnotherCollection()
        {
            return new ArgumentException(System.Data.Entity.Strings.EntityParameterContainedByAnotherCollection);
        }
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        //
        // Helper Functions
        //
        internal static void ThrowArgumentNullException(string parameterName)
        {
            throw ArgumentNull(parameterName);
        }
        internal static void ThrowArgumentOutOfRangeException(string parameterName)
        {
            throw ArgumentOutOfRange(parameterName);
        }
        internal static T CheckArgumentOutOfRange<T>(T[] values, int index, string parameterName)
        {
            Debug.Assert(null != values, "null values"); // use a different method if values can be null
            if (unchecked((uint)values.Length <= (uint)index))
            {
                ThrowArgumentOutOfRangeException(parameterName);
            }
            return values[index];
        }

        static internal T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                ThrowArgumentNullException(parameterName);
            }
            return value;
        }

        static internal IEnumerable<T> CheckArgumentContainsNull<T>(ref IEnumerable<T> enumerableArgument, string argumentName) where T : class
        {
            GetCheapestSafeEnumerableAsCollection(ref enumerableArgument);
            foreach (T item in enumerableArgument)
            {
                if(item == null)
                {
                    throw EntityUtil.Argument(Strings.CheckArgumentContainsNullFailed(argumentName));
                }
            }
            return enumerableArgument;
        }

        static internal IEnumerable<T> CheckArgumentEmpty<T>(ref IEnumerable<T> enumerableArgument, Func<string, string> errorMessage, string argumentName)
        {
            int count;
            GetCheapestSafeCountOfEnumerable(ref enumerableArgument, out count);
            if (count <= 0)
            {
                throw EntityUtil.Argument(errorMessage(argumentName));
            }
            return enumerableArgument;
        }

        private static void GetCheapestSafeCountOfEnumerable<T>(ref IEnumerable<T> enumerable, out int count)
        {
            ICollection<T> collection = GetCheapestSafeEnumerableAsCollection(ref enumerable);
            count = collection.Count;
        }

        private static ICollection<T> GetCheapestSafeEnumerableAsCollection<T>(ref IEnumerable<T> enumerable)
        {
            ICollection<T> collection = enumerable as ICollection<T>;
            if (collection != null)
            {
                // cheap way
                return collection;
            }

            // expensive way, but we don't know if the enumeration is rewindable so...
            enumerable = new List<T>(enumerable);
            return enumerable as ICollection<T>;
        }

        static internal T GenericCheckArgumentNull<T>(T value, string parameterName) where T: class
        {
            return CheckArgumentNull(value, parameterName);
        }

        // EntityConnectionStringBuilder
        static internal ArgumentException KeywordNotSupported(string keyword)
        {
            return Argument(System.Data.Entity.Strings.EntityClient_KeywordNotSupported(keyword));
        }

        static internal ArgumentException ADP_KeywordNotSupported(string keyword)
        {
            return Argument(System.Data.Entity.Strings.ADP_KeywordNotSupported(keyword));
        }

        // Invalid Enumeration

        static internal ArgumentOutOfRangeException InvalidEnumerationValue(Type type, int value) {
            return EntityUtil.ArgumentOutOfRange(System.Data.Entity.Strings.ADP_InvalidEnumerationValue(type.Name, value.ToString(System.Globalization.CultureInfo.InvariantCulture)), type.Name);
        }

        /// <summary>
        /// Given a provider factory, this returns the provider invariant name for the provider. 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static bool TryGetProviderInvariantName(DbProviderFactory providerFactory, out string invariantName)
        {
            Debug.Assert(providerFactory != null);

            var connectionProviderFactoryType = providerFactory.GetType();
            var connectionProviderFactoryAssemblyName = new AssemblyName(
                connectionProviderFactoryType.Assembly.FullName);

            foreach (DataRow row in DbProviderFactories.GetFactoryClasses().Rows)
            {
                var assemblyQualifiedTypeName = (string)row[AssemblyQualifiedNameIndex];

                AssemblyName rowProviderFactoryAssemblyName = null;

                // parse the provider factory assembly qualified type name
                Type.GetType(
                    assemblyQualifiedTypeName,
                    a =>
                    {
                        rowProviderFactoryAssemblyName = a;

                        return null;
                    },
                    (_, __, ___) => null);

                if (rowProviderFactoryAssemblyName != null)
                {
                    if (string.Equals(
                        connectionProviderFactoryAssemblyName.Name,
                        rowProviderFactoryAssemblyName.Name,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var foundFactory = DbProviderFactories.GetFactory(row);

                            if (foundFactory.GetType().Equals(connectionProviderFactoryType))
                            {
                                invariantName = (string)row[InvariantNameIndex];
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail("GetFactory failed with: " + ex);
                            // Ignore bad providers.
                        }
                    }
                }
            }
            invariantName = null;
            return false;
        }

        static internal bool AssemblyNamesMatch(string infoRowProviderAssemblyName, AssemblyName targetAssemblyName)
        {
            if (string.IsNullOrWhiteSpace(infoRowProviderAssemblyName))
            {
                return false;
            }

            AssemblyName assemblyName = null;
            try
            {
                assemblyName = new AssemblyName(infoRowProviderAssemblyName);
            }
            catch (Exception e)
            {
                // Ignore broken provider entries
                if (!IsCatchableExceptionType(e))
                {
                    throw;
                }
                return false;
            }

            Debug.Assert(assemblyName != null, "assemblyName should not be null at this point");

            // Match the provider assembly details
            if (! string.Equals(targetAssemblyName.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (targetAssemblyName.Version == null || assemblyName.Version == null)
            {
                return false;
            }

            if ((targetAssemblyName.Version.Major != assemblyName.Version.Major) || 
                (targetAssemblyName.Version.Minor != assemblyName.Version.Minor))
            {
                return false;
            }

            var targetPublicKeyToken = targetAssemblyName.GetPublicKeyToken();        
            return (targetPublicKeyToken != null)
                && targetPublicKeyToken.SequenceEqual(assemblyName.GetPublicKeyToken());
        }

        // Invalid string argument
        static internal void CheckStringArgument(string value, string parameterName)
        {
            // Throw ArgumentNullException when string is null
            CheckArgumentNull(value, parameterName);

            // Throw ArgumentException when string is empty
            if (value.Length == 0)
            {
                throw InvalidStringArgument(parameterName);
            }
        }

        // only StackOverflowException & ThreadAbortException are sealed classes
        static private readonly Type StackOverflowType   = typeof(System.StackOverflowException);
        static private readonly Type OutOfMemoryType     = typeof(System.OutOfMemoryException);
        static private readonly Type ThreadAbortType     = typeof(System.Threading.ThreadAbortException);
        static private readonly Type NullReferenceType   = typeof(System.NullReferenceException);
        static private readonly Type AccessViolationType = typeof(System.AccessViolationException);
        static private readonly Type SecurityType        = typeof(System.Security.SecurityException);
        static private readonly Type CommandExecutionType = typeof(EntityCommandExecutionException);
        static private readonly Type CommandCompilationType = typeof(EntityCommandCompilationException);
        static private readonly Type QueryType = typeof(EntitySqlException);

        static internal bool IsCatchableExceptionType (Exception e) {
            // a 'catchable' exception is defined by what it is not.
            Debug.Assert(e != null, "Unexpected null exception!");
            Type type = e.GetType();

            return ( (type != StackOverflowType) &&
                     (type != OutOfMemoryType)   &&
                     (type != ThreadAbortType)   &&
                     (type != NullReferenceType) &&
                     (type != AccessViolationType) &&
                     !SecurityType.IsAssignableFrom(type));
        }

        static internal bool IsCatchableEntityExceptionType(Exception e)
        {
            Debug.Assert(e != null, "Unexpected null exception!");
            Type type = e.GetType();

            return IsCatchableExceptionType(e) &&
                type != CommandExecutionType &&
                type != CommandCompilationType &&
                type != QueryType;
        }

        static internal bool IsNull(object value) {
            if ((null == value) || (DBNull.Value == value)) {
                return true;
            }
            INullable nullable = (value as INullable);
            return ((null != nullable) && nullable.IsNull);
        }

        /// <summary>
        /// Utility method to raise internal error when a throttling constraint is violated during
        /// Boolean expression analysis. An internal exception is thrown including the given message
        /// if the given condition is false. This allows us to give up on an unexpectedly difficult
        /// computation rather than risk hanging the user's machine.
        /// </summary>
        static internal void BoolExprAssert(bool condition, string message)
        {
            if (!condition)
            {
                throw InternalError(InternalErrorCode.BoolExprAssert, 0, message);
            }
        }

        static internal PropertyInfo GetTopProperty(Type t, string propertyName)
        {
            return GetTopProperty(ref t, propertyName);
        }

        /// <summary>
        /// Returns the PropertyInfo and Type where a given property is defined
        /// This is done by traversing the type hierarchy to find the type match.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        static internal PropertyInfo GetTopProperty(ref Type t, string propertyName)
        {
            PropertyInfo propertyInfo = null;
            while (propertyInfo == null && t != null)
            {
                propertyInfo = t.GetProperty(propertyName, BindingFlags.Instance |
                                                           BindingFlags.Public |
                                                           BindingFlags.NonPublic |
                                                           BindingFlags.DeclaredOnly);
                t = t.BaseType;
            }
            t = propertyInfo.DeclaringType;
            return propertyInfo;
        }

        static internal int SrcCompare(string strA, string strB)
        { 
            return ((strA == strB) ? 0 : 1);
        }
        static internal int DstCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, EntityUtil.StringCompareOptions);
        }

        internal static Dictionary<string,string> COMPILER_VERSION = new Dictionary<string, string>() { { "CompilerVersion", "V3.5" } }; //v3.5 required for compiling model files with partial methods.

        [FileIOPermission(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.PathDiscovery)]
        [Security.SecuritySafeCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        static internal string GetFullPath(string filename)
        { // MDAC 77686
            return Path.GetFullPath(filename);
        }
#if false
        public static T FieldCast<T>(object value) {
            try {
                // will result in an InvalidCastException if !(value is T)
                // this pattern also supports handling System.Data.SqlTypes
                return (T)((DBNull.Value == value) ? null : value);
            }
            catch(NullReferenceException) {
                // (value == null) and (T is struct) and (T is not Nullable<>), convert to InvalidCastException
                return (T)(object)System.DBNull.Value;
            }
        }
#endif    
    
        public static Type[] GetTypesSpecial(Assembly assembly)
        {
            return ReferenceEquals(assembly, typeof(ObjectContext).Assembly)
                       ? new Type[0]
                       : assembly.GetTypes();
        }

        /// <summary>
        /// This method uses the .net Fx target framework moniker (introduced in .net 4.0 Multitargeting feature)
        /// to provide a 'quirks' mode that serves as a compatibility flag for features that can be considered
        /// breaking changes from 4.0 to 4.5 which is a in-place upgrade to 4.0.  For details see DevDiv2 

        static bool? useFx40CompatMode;
        static public bool UseFx40CompatMode
        {
            get
            {
                if (!useFx40CompatMode.HasValue)
                {
                    string fxname = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;

                    if (string.IsNullOrWhiteSpace(fxname))
                    {
                        Assembly entryAssembly = Assembly.GetEntryAssembly(); // entry assembly can be unmanaged.

                        if (entryAssembly != null)
                        {
                            TargetFrameworkAttribute fxAttrib = entryAssembly.GetCustomAttribute<TargetFrameworkAttribute>();
                            if (fxAttrib != null)
                            {
                                fxname = fxAttrib.FrameworkName;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(fxname))
                    {
                        try
                        {
                            FrameworkName compiledFxName = new FrameworkName(fxname);
                            Version fxv45 = new Version(4, 5);

                            useFx40CompatMode = compiledFxName.Version < fxv45;
                        }
                        catch (System.ArgumentException)
                        {
                        }
                    }

                    if (!useFx40CompatMode.HasValue)
                    {
                        useFx40CompatMode = true;
                    }
                }

                return useFx40CompatMode.Value;
            }
        }

    }
}
