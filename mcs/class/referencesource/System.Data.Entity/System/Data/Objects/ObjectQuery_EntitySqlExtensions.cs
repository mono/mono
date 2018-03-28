//---------------------------------------------------------------------
// <copyright file="ObjectQuery_EntitySqlExtensions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupowner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.ELinq;
    using System.Data.Objects.Internal;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///   ObjectQuery implements strongly-typed queries at the object-layer.
    ///   Queries are specified using Entity-SQL strings and may be created by calling
    ///   the Entity-SQL-based query builder methods declared by ObjectQuery.
    /// </summary>
    /// <typeparam name="T">The result type of this ObjectQuery</typeparam>
    public partial class ObjectQuery<T> : IEnumerable<T>
    {
        #region Private Static Members

        // -----------------
        // Static Fields
        // -----------------

        /// <summary>
        ///   The default query name, which is used in query-building to refer to an
        ///   element of the ObjectQuery; e.g., in a call to ObjectQuery.Where(), a predicate of
        ///   the form "it.Name = 'Foo'" can be specified, where "it" refers to a T.
        ///   Note that the query name may eventually become a parameter in the command
        ///   tree, so it must conform to the parameter name restrictions enforced by
        ///   ObjectParameter.ValidateParameterName(string).
        /// </summary>
        private const string DefaultName = "it";

        private static bool IsLinqQuery(ObjectQuery query)
        {
            return (query.QueryState is ELinqQueryState);
        }

        #endregion

        #region Private Instance Members
        
        // -------------------
        // Private Fields
        // -------------------

        /// <summary>
        ///   The name of the current sequence, which defaults to "it". Used in query-
        ///   builder methods that process an Entity-SQL command text fragment to refer to an
        ///   instance of the return type of this query.
        /// </summary>
        private string _name = ObjectQuery<T>.DefaultName;

        #endregion

        #region Public Constructors

        // -------------------
        // Public Constructors
        // -------------------

        #region ObjectQuery (string, ObjectContext)

        /// <summary>
        ///   This constructor creates a new ObjectQuery instance using the specified Entity-SQL
        ///   command as the initial query. The context specifies the connection on
        ///   which to execute the query as well as the metadata and result cache.
        /// </summary>
        /// <param name="commandText">
        ///   The Entity-SQL query string that initially defines the query.
        /// </param>
        /// <param name="context">
        ///   The ObjectContext containing the metadata workspace the query will
        ///   be built against, the connection on which to execute the query, and the
        ///   cache to store the results in.
        /// </param>
        /// <returns>
        ///   A new ObjectQuery instance.
        /// </returns>
        public ObjectQuery (string commandText, ObjectContext context)
            : this(new EntitySqlQueryState(typeof(T), commandText, false, context, null, null))
        {
            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <T>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            context.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), System.Reflection.Assembly.GetCallingAssembly());
        }

        #endregion

        #region ObjectQuery (string, ObjectContext, MergeOption)

        /// <summary>
        ///   This constructor creates a new ObjectQuery instance using the specified Entity-SQL
        ///   command as the initial query. The context specifies the connection on
        ///   which to execute the query as well as the metadata and result cache.
        ///   The merge option specifies how the cache should be populated/updated.
        /// </summary>
        /// <param name="commandText">
        ///   The Entity-SQL query string that initially defines the query.
        /// </param>
        /// <param name="context">
        ///   The ObjectContext containing the metadata workspace the query will
        ///   be built against, the connection on which to execute the query, and the
        ///   cache to store the results in.
        /// </param>
        /// <param name="mergeOption">
        ///   The MergeOption to use when executing the query.
        /// </param>
        /// <returns>
        ///   A new ObjectQuery instance.
        /// </returns>
        public ObjectQuery (string commandText, ObjectContext context, MergeOption mergeOption)
            : this(new EntitySqlQueryState(typeof(T), commandText, false, context, null, null))
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            this.QueryState.UserSpecifiedMergeOption = mergeOption;

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <T>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            context.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), System.Reflection.Assembly.GetCallingAssembly());
        }

        #endregion

        #endregion

        #region internal ObjectQuery (EntitySet, ObjectContext, MergeOption) constructor.

        /// <summary>
        ///   This method creates a new ObjectQuery instance that represents a scan over
        ///   the specified <paramref name="entitySet"/>. This ObjectQuery carries the scan as <see cref="DbExpression"/> 
        ///   and as Entity SQL. This is needed to allow case-sensitive metadata access (provided by the <see cref="DbExpression"/> by default).
        ///   The context specifies the connection on which to execute the query as well as the metadata and result cache.
        ///   The merge option specifies how the cache should be populated/updated.
        /// </summary>
        /// <param name="entitySet">
        ///   The entity set this query scans.
        /// </param>
        /// <param name="context">
        ///   The ObjectContext containing the metadata workspace the query will
        ///   be built against, the connection on which to execute the query, and the
        ///   cache to store the results in.
        /// </param>
        /// <param name="mergeOption">
        ///   The MergeOption to use when executing the query.
        /// </param>
        /// <returns>
        ///   A new ObjectQuery instance.
        /// </returns>
        internal ObjectQuery (EntitySetBase entitySet, ObjectContext context, MergeOption mergeOption)
            : this(new EntitySqlQueryState(typeof(T), BuildScanEntitySetEsql(entitySet), entitySet.Scan(), false, context, null, null))
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            this.QueryState.UserSpecifiedMergeOption = mergeOption;

            // SQLBUDT 447285: Ensure the assembly containing the entity's CLR type
            // is loaded into the workspace. If the schema types are not loaded
            // metadata, cache & query would be unable to reason about the type. We
            // either auto-load <T>'s assembly into the ObjectItemCollection or we
            // auto-load the user's calling assembly and its referenced assemblies.
            // If the entities in the user's result spans multiple assemblies, the
            // user must manually call LoadFromAssembly. *GetCallingAssembly returns
            // the assembly of the method that invoked the currently executing method.
            context.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(T), System.Reflection.Assembly.GetCallingAssembly());
        }

        private static string BuildScanEntitySetEsql(EntitySetBase entitySet)
        {
            EntityUtil.CheckArgumentNull(entitySet, "entitySet");
            return String.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1}",
                    EntityUtil.QuoteIdentifier(entitySet.EntityContainer.Name),
                    EntityUtil.QuoteIdentifier(entitySet.Name));
        }

        #endregion

        #region Public Properties
        
        /// <summary>
        ///   The name of the query, which can be used to identify the current sequence
        ///   by name in query-builder methods. By default, the value is "it".
        /// </summary>
        /// <exception cref="ArgumentException">
        ///   If the value specified on set is invalid.
        /// </exception>
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                EntityUtil.CheckArgumentNull(value, "value");

                if (!ObjectParameter.ValidateParameterName(value))
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_InvalidQueryName(value), "value");
                }

                this._name = value;
            }
        }

        #endregion

        #region Query-builder Methods

        // ---------------------
        // Query-builder Methods
        // ---------------------

        /// <summary>
        ///   This query-builder method creates a new query whose results are the
        ///   unique results of this query.
        /// </summary>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        public ObjectQuery<T> Distinct ()
        {
            if (IsLinqQuery(this))
            {
                return (ObjectQuery<T>)Queryable.Distinct<T>(this);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Distinct(this.QueryState));
        }

        /// <summary>
        ///   This query-builder method creates a new query whose results are all of
        ///   the results of this query, except those that are also part of the other
        ///   query specified.
        /// </summary>
        /// <param name="query">
        ///   A query representing the results to exclude.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If the query parameter is null.
        /// </exception>
        public ObjectQuery<T> Except(ObjectQuery<T> query)
        {
            EntityUtil.CheckArgumentNull(query, "query");

            if (IsLinqQuery(this) || IsLinqQuery(query))
            {
                return (ObjectQuery<T>)Queryable.Except(this, query);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Except(this.QueryState, query.QueryState));
        }
                
        /// <summary>
        ///   This query-builder method creates a new query whose results are the results
        ///   of this query, grouped by some criteria.
        /// </summary>
        /// <param name="keys">
        ///   The group keys.
        /// </param>
        /// <param name="projection">
        ///   The projection list. To project the group, use the keyword "group".
        /// </param>
        /// <param name="parameters">
        ///   An optional set of query parameters that should be in scope when parsing.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        public ObjectQuery<DbDataRecord> GroupBy(string keys, string projection, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckArgumentNull(keys, "keys");
            EntityUtil.CheckArgumentNull(projection, "projection");
            EntityUtil.CheckArgumentNull(parameters, "parameters");
            
            if (StringUtil.IsNullOrEmptyOrWhiteSpace(keys))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidGroupKeyList, "keys");
            }

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(projection))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidProjectionList, "projection");
            }

            return new ObjectQuery<DbDataRecord>(EntitySqlQueryBuilder.GroupBy(this.QueryState, this.Name, keys, projection, parameters));
        }

        /// <summary>
        ///   This query-builder method creates a new query whose results are those that
        ///   are both in this query and the other query specified.
        /// </summary>
        /// <param name="query">
        ///   A query representing the results to intersect with.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If the query parameter is null.
        /// </exception>
        public ObjectQuery<T> Intersect (ObjectQuery<T> query)
        {
            EntityUtil.CheckArgumentNull(query, "query");

            if (IsLinqQuery(this) || IsLinqQuery(query))
            {
                return (ObjectQuery<T>)Queryable.Intersect(this, query);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Intersect(this.QueryState, query.QueryState));
        }
        
        /// <summary>
        ///   This query-builder method creates a new query whose results are filtered
        ///   to include only those of the specified type.
        /// </summary>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="EntitySqlException">
        ///   If the type specified is invalid.
        /// </exception>
        public ObjectQuery<TResultType> OfType<TResultType>()
        {
            if (IsLinqQuery(this))
            {
                return (ObjectQuery<TResultType>)Queryable.OfType<TResultType>(this);
            }

            // SQLPUDT 484477: Make sure TResultType is loaded.
            this.QueryState.ObjectContext.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResultType), System.Reflection.Assembly.GetCallingAssembly());

            // Retrieve the O-Space type metadata for the result type specified. If no
            // metadata can be found for the specified type, fail. Otherwise, if the
            // type metadata found for TResultType is not either an EntityType or a
            // ComplexType, fail - OfType() is not a valid operation on scalars,
            // enumerations, collections, etc.
            Type clrOfType = typeof(TResultType);
            EdmType ofType = null;
            if (!this.QueryState.ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.OSpace).TryGetType(clrOfType.Name, clrOfType.Namespace ?? string.Empty, out ofType) ||
                !(Helper.IsEntityType(ofType) || Helper.IsComplexType(ofType)))
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidResultType(typeof(TResultType).FullName));
            }

            return new ObjectQuery<TResultType>(EntitySqlQueryBuilder.OfType(this.QueryState, ofType, clrOfType));
        }
        
        /// <summary>
        ///   This query-builder method creates a new query whose results are the
        ///   results of this query, ordered by some criteria. Note that any relational
        ///   operations performed after an OrderBy have the potential to "undo" the
        ///   ordering, so OrderBy should be considered a terminal query-building
        ///   operation.
        /// </summary>
        /// <param name="keys">
        ///   The sort keys.
        /// </param>
        /// <param name="parameters">
        ///   An optional set of query parameters that should be in scope when parsing.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If either argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   If the sort key command text is empty.
        /// </exception>
        public ObjectQuery<T> OrderBy (string keys, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckArgumentNull(keys, "keys");
            EntityUtil.CheckArgumentNull(parameters, "parameters");

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(keys))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidSortKeyList, "keys");
            }

            return new ObjectQuery<T>(EntitySqlQueryBuilder.OrderBy(this.QueryState, this.Name, keys, parameters));
        }
        
        /// <summary>
        ///   This query-builder method creates a new query whose results are data
        ///   records containing selected fields of the results of this query.
        /// </summary>
        /// <param name="projection">
        ///   The projection list.
        /// </param>
        /// <param name="parameters">
        ///   An optional set of query parameters that should be in scope when parsing.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If either argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   If the projection list command text is empty.
        /// </exception>
        public ObjectQuery<DbDataRecord> Select (string projection, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckArgumentNull(projection, "projection");
            EntityUtil.CheckArgumentNull(parameters, "parameters");

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(projection))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidProjectionList, "projection");
            }

            return new ObjectQuery<DbDataRecord>(EntitySqlQueryBuilder.Select(this.QueryState, this.Name, projection, parameters));
        }
         
        /// <summary>
        ///   This query-builder method creates a new query whose results are a sequence
        ///   of values projected from the results of this query.
        /// </summary>
        /// <param name="projection">
        ///   The projection list.
        /// </param>
        /// <param name="parameters">
        ///   An optional set of query parameters that should be in scope when parsing.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If either argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   If the projection list command text is empty.
        /// </exception>
        public ObjectQuery<TResultType> SelectValue<TResultType> (string projection, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckArgumentNull(projection, "projection");
            EntityUtil.CheckArgumentNull(parameters, "parameters");

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(projection))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidProjectionList, "projection");
            }

            // SQLPUDT 484974: Make sure TResultType is loaded.
            this.QueryState.ObjectContext.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResultType), System.Reflection.Assembly.GetCallingAssembly());

            return new ObjectQuery<TResultType>(EntitySqlQueryBuilder.SelectValue(this.QueryState, this.Name, projection, parameters, typeof(TResultType)));
        }

        /// <summary>
        ///   This query-builder method creates a new query whose results are the
        ///   results of this query, ordered by some criteria and with the specified
        ///   number of results 'skipped', or paged-over.
        /// </summary>
        /// <param name="keys">
        ///   The sort keys.
        /// </param>
        /// <param name="count">
        ///   Specifies the number of results to skip. This must be either a constant or
        ///   a parameter reference.
        /// </param>
        /// <param name="parameters">
        ///   An optional set of query parameters that should be in scope when parsing.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If any argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   If the sort key or skip count command text is empty.
        /// </exception>
        public ObjectQuery<T> Skip (string keys, string count, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckArgumentNull(keys, "keys");
            EntityUtil.CheckArgumentNull(count, "count");
            EntityUtil.CheckArgumentNull(parameters, "parameters");

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(keys))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidSortKeyList, "keys");
            }

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(count))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidSkipCount, "count");
            }

            return new ObjectQuery<T>(EntitySqlQueryBuilder.Skip(this.QueryState, this.Name, keys, count, parameters));
        }
        
        /// <summary>
        ///   This query-builder method creates a new query whose results are the
        ///   first 'count' results of this query.
        /// </summary>
        /// <param name="count">
        ///   Specifies the number of results to return. This must be either a constant or
        ///   a parameter reference.
        /// </param>
        /// <param name="parameters">
        ///   An optional set of query parameters that should be in scope when parsing.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If the top count command text is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   If the top count command text is empty.
        /// </exception>
        public ObjectQuery<T> Top (string count, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckArgumentNull(count, "count");

            if (StringUtil.IsNullOrEmptyOrWhiteSpace(count))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidTopCount, "count");
            }

            return new ObjectQuery<T>(EntitySqlQueryBuilder.Top(this.QueryState, this.Name, count, parameters));
        }

        /// <summary>
        ///   This query-builder method creates a new query whose results are all of
        ///   the results of this query, plus all of the results of the other query,
        ///   without duplicates (i.e., results are unique).
        /// </summary>
        /// <param name="query">
        ///   A query representing the results to add.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If the query parameter is null.
        /// </exception>
        public ObjectQuery<T> Union (ObjectQuery<T> query)
        {
            EntityUtil.CheckArgumentNull(query, "query");

            if (IsLinqQuery(this) || IsLinqQuery(query))
            {
                return (ObjectQuery<T>)Queryable.Union(this, query);
            }
            return new ObjectQuery<T>(EntitySqlQueryBuilder.Union(this.QueryState, query.QueryState));
        }
        
        /// <summary>
        ///   This query-builder method creates a new query whose results are all of
        ///   the results of this query, plus all of the results of the other query,
        ///   including any duplicates (i.e., results are not necessarily unique).
        /// </summary>
        /// <param name="query">
        ///   A query representing the results to add.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If the query parameter is null.
        /// </exception>
        public ObjectQuery<T> UnionAll (ObjectQuery<T> query)
        {
            EntityUtil.CheckArgumentNull(query, "query");
            
            return new ObjectQuery<T>(EntitySqlQueryBuilder.UnionAll(this.QueryState, query.QueryState));
        }
        
        /// <summary>
        ///   This query-builder method creates a new query whose results are the
        ///   results of this query filtered by some criteria.
        /// </summary>
        /// <param name="predicate">
        ///   The filter predicate.
        /// </param>
        /// <param name="parameters">
        ///   An optional set of query parameters that should be in scope when parsing.
        /// </param>
        /// <returns>
        ///   a new ObjectQuery instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If either argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   If the filter predicate command text is empty.
        /// </exception>
        public ObjectQuery<T> Where (string predicate, params ObjectParameter[] parameters)
        {
            EntityUtil.CheckArgumentNull(predicate, "predicate");
            EntityUtil.CheckArgumentNull(parameters, "parameters");
            
            if (StringUtil.IsNullOrEmptyOrWhiteSpace(predicate))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidFilterPredicate, "predicate");
            }

            return new ObjectQuery<T>(EntitySqlQueryBuilder.Where(this.QueryState, this.Name, predicate, parameters));
        }

        #endregion        
    }
}
