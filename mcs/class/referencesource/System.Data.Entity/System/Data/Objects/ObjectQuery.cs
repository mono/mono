//---------------------------------------------------------------------
// <copyright file="ObjectQuery.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupowner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.Internal;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///   This class implements untyped queries at the object-layer. 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class ObjectQuery : IEnumerable, IQueryable, IOrderedQueryable, IListSource
    {
        #region Private Instance Members

        // -----------------
        // Instance Fields
        // -----------------
        
        /// <summary>
        ///   The underlying implementation of this ObjectQuery as provided by a concrete subclass
        ///   of ObjectQueryImplementation. Implementations currently exist for Entity-SQL- and Linq-to-Entities-based ObjectQueries.
        /// </summary>
        private ObjectQueryState _state;

        /// <summary>
        ///   The result type of the query - 'TResultType' expressed as an O-Space type usage. Cached here and
        ///   only instantiated if the <see cref="GetResultType"/> method is called.
        /// </summary>
        private TypeUsage _resultType;

        /// <summary>
        /// Every instance of ObjectQuery get a unique instance of the provider. This helps propagate state information
        /// using the provider through LINQ operators.
        /// </summary>
        private IQueryProvider _provider;

        #endregion

        #region Internal Constructors

        // --------------------
        // Internal Constructors
        // --------------------

        /// <summary>
        ///   The common constructor.
        /// </summary>
        /// <param name="queryState">
        ///   The underlying implementation of this ObjectQuery
        /// </param>
        /// <returns>
        ///   A new ObjectQuery instance.
        /// </returns>
        internal ObjectQuery(ObjectQueryState queryState)
        {
            Debug.Assert(queryState != null, "ObjectQuery state cannot be null");
            
            // Set the query state.
            this._state = queryState;
        }
                
        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets an untyped instantiation of the underlying ObjectQueryState that implements this ObjectQuery.
        /// </summary>
        internal ObjectQueryState QueryState { get { return this._state; } }
                
        #endregion

        #region IQueryable implementation

        /// <summary>
        /// Gets the result element type for this query instance.
        /// </summary>
        Type IQueryable.ElementType
        {
            get { return this._state.ElementType; }
        }

        /// <summary>
        /// Gets the expression describing this query. For queries built using
        /// LINQ builder patterns, returns a full LINQ expression tree; otherwise,
        /// returns a constant expression wrapping this query. Note that the
        /// default expression is not cached. This allows us to differentiate
        /// between LINQ and Entity-SQL queries.
        /// </summary>
        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get
            {
                return this.GetExpression();
            }
        }

        internal abstract System.Linq.Expressions.Expression GetExpression();
        
        /// <summary>
        /// Gets the IQueryProvider associated with this query instance.
        /// </summary>
        IQueryProvider IQueryable.Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new System.Data.Objects.ELinq.ObjectQueryProvider(this);
                }
                return _provider;
            }
        }

        #endregion

        #region Public Properties

        // ----------
        // Properties
        // ----------

        // ----------------------
        // IListSource  Properties
        // ----------------------
        /// <summary>
        ///   IListSource.ContainsListCollection implementation. Always returns true.
        /// </summary>
        bool IListSource.ContainsListCollection
        {
            get
            {
                return false; // this means that the IList we return is the one which contains our actual data, it is not a collection
            }
        }

        /// <summary>
        /// Gets the Command Text (if any) for this ObjectQuery.
        /// </summary>
        public string CommandText
        {
            get
            {
                string commandText;
                if (!_state.TryGetCommandText(out commandText))
                {
                    return String.Empty;
                }

                Debug.Assert(commandText != null && commandText.Length != 0, "Invalid Command Text returned");
                return commandText;
            }
        }

        /// <summary>
        ///   The context for the query, which includes the connection, cache and
        ///   metadata. Note that only the connection property is mutable and must be
        ///   set before a query can be executed.
        /// </summary>
        public ObjectContext Context
        {
            get
            {
                return this._state.ObjectContext;
            }
        }

        /// <summary>
        ///   Allows optional control over how queried results interact with the object state manager.
        /// </summary>
        public MergeOption MergeOption
        {
            get
            {
                return this._state.EffectiveMergeOption;
            }

            set
            {
                EntityUtil.CheckArgumentMergeOption(value);
                this._state.UserSpecifiedMergeOption = value;
            }
        }

        /// <summary>
        ///   The parameter collection for this query.
        /// </summary>
        public ObjectParameterCollection Parameters
        {
            get
            {
                return this._state.EnsureParameters();
            }
        }

        /// <summary>
        ///   Defines if the query plan should be cached.
        /// </summary>
        public bool EnablePlanCaching
        {
            get
            {
                return this._state.PlanCachingEnabled;
            }

            set
            {
                this._state.PlanCachingEnabled = value;
            }
        }

        #endregion

        #region Public Methods

        // --------------
        // Public Methods
        // --------------

        // ----------------------
        // IListSource  method
        // ----------------------
        /// <summary>
        ///   IListSource.GetList implementation
        /// </summary>
        /// <returns>
        ///   IList interface over the data to bind
        /// </returns>
        IList IListSource.GetList()
        {
            return this.GetIListSourceListInternal();
        }

        /// <summary>
        ///   Get the provider-specific command text used to execute this query
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        public string ToTraceString() 
        {
            return this._state.GetExecutionPlan(null).ToTraceString();
        }
                        
        /// <summary>
        ///   This method returns information about the result type of the ObjectQuery.
        /// </summary>
        /// <returns>
        ///   The TypeMetadata that describes the shape of the query results.
        /// </returns>
        public TypeUsage GetResultType ()
        {
            Context.EnsureMetadata();
            if (null == this._resultType)
            {
                // Retrieve the result type from the implementation, in terms of C-Space.
                TypeUsage cSpaceQueryResultType = this._state.ResultType;
                
                // Determine the 'TResultType' equivalent type usage based on the mapped O-Space type.
                // If the result type of the query is a collection[something], then
                // extract out the 'something' (element type) and use that. This
                // is the equivalent of saying the result type is T, rather than
                // IEnumerable<T>, which aligns with users' expectations.
                TypeUsage tResultType;
                if (!TypeHelpers.TryGetCollectionElementType(cSpaceQueryResultType, out tResultType))
                {
                    tResultType = cSpaceQueryResultType;
                }

                // Map the C-space result type to O-space.
                tResultType = this._state.ObjectContext.Perspective.MetadataWorkspace.GetOSpaceTypeUsage(tResultType);
                if (null == tResultType)
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectQuery_UnableToMapResultType);
                }

                this._resultType = tResultType;
            }

            return this._resultType;
        }
         
        /// <summary>
        ///   This method allows explicit query evaluation with a specified merge
        ///   option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption">
        ///   The MergeOption to use when executing the query.
        /// </param>
        /// <returns>
        ///   An enumerable for the ObjectQuery results.
        /// </returns>
        public ObjectResult Execute(MergeOption mergeOption)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            return this.ExecuteInternal(mergeOption);
        }
                
        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorInternal();
        }

        #endregion

        #endregion

        #region Internal Methods
        internal abstract IEnumerator GetEnumeratorInternal();
        internal abstract IList GetIListSourceListInternal();
        internal abstract ObjectResult ExecuteInternal(MergeOption mergeOption);
        #endregion
    }
}
