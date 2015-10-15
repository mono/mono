//---------------------------------------------------------------------
// <copyright file="ObjectQueryState.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.EntitySql;
    using System.Data.EntityClient;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Data.Objects.ELinq;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///   An instance of a class derived from ObjectQueryState is used to model every instance of <see cref="ObjectQuery&lt;TResultType&gt;"/>.
    ///   A different ObjectQueryState-derived class is used depending on whether the ObjectQuery is an Entity SQL, 
    ///   Linq to Entities, or compiled Linq to Entities query.
    /// </summary>
    internal abstract class ObjectQueryState
    {       
        /// <summary>
        ///   The <see cref="MergeOption"/> that should be used in the absence of an explicitly specified
        ///   or user-specified merge option or a merge option inferred from the query definition itself.
        /// </summary>
        internal static readonly MergeOption DefaultMergeOption = MergeOption.AppendOnly;

        /// <summary>
        ///   The context of the ObjectQuery
        /// </summary>
        private readonly ObjectContext _context;

        /// <summary>
        ///   The element type of this query, as a CLR type
        /// </summary>
        private readonly Type _elementType;

        /// <summary>
        ///   The collection of parameters associated with the ObjectQuery
        /// </summary>
        private ObjectParameterCollection _parameters;
                
        /// <summary>
        ///   The full-span specification
        /// </summary>
        private Span _span;

        /// <summary>
        ///   The user-specified default merge option 
        /// </summary>
        private MergeOption? _userMergeOption;

        /// <summary>
        ///  Indicates whether query caching is enabled for the implemented ObjectQuery.
        /// </summary>
        private bool _cachingEnabled = true;

        /// <summary>
        ///   Optionally used by derived classes to record the most recently used <see cref="ObjectQueryExecutionPlan"/>.
        /// </summary>
        protected ObjectQueryExecutionPlan _cachedPlan;
        
               
        /// <summary>
        ///   Constructs a new <see cref="ObjectQueryState"/> instance that uses the specified context and parameters collection.
        /// </summary>
        /// <param name="context">
        ///   The ObjectContext to which the implemented ObjectQuery belongs
        /// </param>
        protected ObjectQueryState(Type elementType, ObjectContext context, ObjectParameterCollection parameters, Span span)
        {
            // Validate the element type
            EntityUtil.CheckArgumentNull(elementType, "elementType");

            // Validate the context
            EntityUtil.CheckArgumentNull(context, "context");
            
            // Parameters and Span are specifically allowed to be null

            this._elementType = elementType;
            this._context = context;
            this._span = span;
            this._parameters = parameters;
        }

        /// <summary>
        ///  Constructs a new <see cref="ObjectQueryState"/> copying the state information from the specified
        ///  <see cref="ObjectQuery"/>.
        /// </summary>
        /// <param name="elementType">The element type of the implemented ObjectQuery, as a CLR type.</param>
        /// <param name="query">The ObjectQuery from which the state should be copied.</param>
        protected ObjectQueryState(Type elementType, ObjectQuery query)
            : this(elementType, query.Context, null, null)
        {
            this._cachingEnabled = query.EnablePlanCaching;
        }

        /// <summary>
        ///   Gets the element type - the type of each result item - for this query as a CLR type instance.
        /// </summary>
        internal Type ElementType { get { return _elementType; } }

        /// <summary>
        ///   Gets the ObjectContext with which the implemented ObjectQuery is associated
        /// </summary>
        internal ObjectContext ObjectContext { get { return _context; } }

        /// <summary>
        ///   Gets the collection of parameters associated with the implemented ObjectQuery. May be null.
        ///   Call <see cref="EnsureParameters"/> if a guaranteed non-null collection is required.
        /// </summary>
        internal ObjectParameterCollection Parameters
        {
            get { return _parameters; }
        }

        internal ObjectParameterCollection EnsureParameters()
        {
            if (_parameters == null)
            {
                _parameters = new ObjectParameterCollection(ObjectContext.Perspective);
                if (this._cachedPlan != null)
                {
                    _parameters.SetReadOnly(true);
                }
            }

            return _parameters;
        }
                
        /// <summary>
        ///   Gets the Span specification associated with the implemented ObjectQuery. May be null.
        /// </summary>
        internal Span Span
        {
            get { return _span; }
        }

        /// <summary>
        ///   The merge option that this query considers currently 'in effect'. This may be a merge option set via the ObjectQuery.MergeOption
        ///   property, or the merge option that applies to the currently cached execution plan, if any, or the global default merge option.
        /// </summary>
        internal MergeOption EffectiveMergeOption
        {
            get
            {
                if (_userMergeOption.HasValue)
                {
                    return _userMergeOption.Value;
                }

                ObjectQueryExecutionPlan plan = this._cachedPlan;
                if (plan != null)
                {
                    return plan.MergeOption;
                }

                return ObjectQueryState.DefaultMergeOption;
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating which <see cref="MergeOption"/> should be used when preparing this query for execution via
        ///   <see cref="GetExecutionPlan(MergeOption?)"/> if no option is explicitly specified - for example during foreach-style enumeration.
        ///   <see cref="ObjectQuery.MergeOption"/> sets this property on its underlying query state instance.
        /// </summary>
        internal MergeOption? UserSpecifiedMergeOption
        {
            get { return _userMergeOption; }
            set { _userMergeOption = value; }
        }

        /// <summary>
        ///   Gets or sets a user-defined value indicating whether or not query caching is enabled for the implemented ObjectQuery.
        /// </summary>
        internal bool PlanCachingEnabled
        {
            get { return _cachingEnabled; }
            set { _cachingEnabled = value; }
        }

        /// <summary>
        ///   Gets the result type - not just the element type - for this query as an EDM Type usage instance.
        /// </summary>
        internal TypeUsage ResultType
        {
            get
            {
                ObjectQueryExecutionPlan plan = this._cachedPlan;
                if (plan != null)
                {
                    return plan.ResultType;
                }
                else
                {
                    return this.GetResultType();
                }
            }
        }

        /// <summary>
        ///   Sets the values the <see cref="PlanCachingEnabled"/> and <see cref="UserSpecifiedMergeOption"/> properties on
        ///   <paramref name="other"/> to match the values of the corresponding properties on this instance.
        /// </summary>
        /// <param name="other">The query state to which this instances settings should be applied.</param>
        internal void ApplySettingsTo(ObjectQueryState other)
        {
            other.PlanCachingEnabled = this.PlanCachingEnabled;
            other.UserSpecifiedMergeOption = this.UserSpecifiedMergeOption;
                                    
            // _cachedPlan is intentionally not copied over - since the parameters of 'other' would have to be locked as
            // soon as its execution plan was set, and that may not be appropriate at the time ApplySettingsTo is called. 
        }
                
        /// <summary>
        ///   Must return <c>true</c> and set <paramref name="commandText"/> to a valid value
        ///   if command text is available for this query; must return <c>false</c> otherwise.
        ///   Implementations of this method must not throw exceptions.
        /// </summary>
        /// <param name="commandText">The command text of this query, if available.</param>
        /// <returns><c>true</c> if command text is available for this query and was successfully retrieved; otherwise <c>false</c>.</returns>
        internal abstract bool TryGetCommandText(out string commandText);

        /// <summary>
        ///   Must return <c>true</c> and set <paramref name="expression"/> to a valid value if a
        ///   LINQ Expression is available for this query; must return <c>false</c> otherwise.
        ///   Implementations of this method must not throw exceptions.
        /// </summary>
        /// <param name="expression">The LINQ Expression that defines this query, if available.</param>
        /// <returns><c>true</c> if an Expression is available for this query and was successfully retrieved; otherwise <c>false</c>.</returns>
        internal abstract bool TryGetExpression(out System.Linq.Expressions.Expression expression);

        /// <summary>
        ///   Retrieves an <see cref="ObjectQueryExecutionPlan"/> that can be used to retrieve the results of this query using the specified merge option.
        ///   If <paramref name="forMergeOption"/> is null, an appropriate default value will be used.
        /// </summary>
        /// <param name="forMergeOption">The merge option which should be supported by the returned execution plan</param>
        /// <returns>an execution plan capable of retrieving the results of this query using the specified merge option</returns>
        internal abstract ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption);

        /// <summary>
        ///   Must returns a new ObjectQueryState instance that is a duplicate of this instance and additionally contains the specified Include path in its <see cref="Span"/>.
        /// </summary>
        /// <typeparam name="TElementType">The element type of the source query on which Include was called</typeparam>
        /// <param name="sourceQuery">The source query on which Include was called</param>
        /// <param name="includePath">The new Include path to add</param>
        /// <returns>Must returns an ObjectQueryState that is a duplicate of this instance and additionally contains the specified Include path</returns>
        internal abstract ObjectQueryState Include<TElementType>(ObjectQuery<TElementType> sourceQuery, string includePath);

        /// <summary>
        ///   Retrieves the result type of the query in terms of C-Space metadata. This method is called once, on-demand, if a call
        ///   to <see cref="ObjectQuery.GetResultType"/> cannot be satisfied using cached type metadata or a currently cached execution plan.
        /// </summary>
        /// <returns>
        ///   Must return a <see cref="TypeUsage"/> that describes the result typeof this query in terms of C-Space metadata
        /// </returns>
        protected abstract TypeUsage GetResultType();

        /// <summary>
        ///   Helper method to return the first non-null merge option from the specified nullable merge options,
        ///   or the <see cref="DefaultMergeOption"/> if the value of all specified nullable merge options is <c>null</c>.
        /// </summary>
        /// <param name="preferredMergeOptions">The available nullable merge option values, in order of decreasing preference</param>
        /// <returns>the first non-null merge option; or the default merge option if the value of all <paramref name="preferredMergeOptions"/> is null</returns>
        protected static MergeOption EnsureMergeOption(params MergeOption?[] preferredMergeOptions)
        {
            foreach (MergeOption? preferred in preferredMergeOptions)
            {
                if (preferred.HasValue)
                {
                    return preferred.Value;
                }
            }

            return ObjectQueryState.DefaultMergeOption;
        }

        /// <summary>
        ///   Helper method to return the first non-null merge option from the specified nullable merge options.
        /// </summary>
        /// <param name="preferredMergeOptions">The available nullable merge option values, in order of decreasing preference</param>
        /// <returns>the first non-null merge option; or <c>null</c> if the value of all <paramref name="preferredMergeOptions"/> is null</returns>
        protected static MergeOption? GetMergeOption(params MergeOption?[] preferredMergeOptions)
        {
            foreach (MergeOption? preferred in preferredMergeOptions)
            {
                if (preferred.HasValue)
                {
                    return preferred.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Helper method to create a new ObjectQuery based on this query state instance.
        /// </summary>
        /// <returns>A new <see cref="ObjectQuery&lt;TResultType&gt;"/> - typed as <see cref="ObjectQuery"/></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal ObjectQuery CreateQuery()
        {
            MethodInfo createMethod = typeof(ObjectQueryState).GetMethod("CreateObjectQuery", BindingFlags.Static | BindingFlags.Public);
            Debug.Assert(createMethod != null, "Unable to retrieve ObjectQueryState.CreateObjectQuery<> method?");

            createMethod = createMethod.MakeGenericMethod(this._elementType);
            return (ObjectQuery)createMethod.Invoke(null, new object[] { this } );
        }

        /// <summary>
        /// Helper method used to create an ObjectQuery based on an underlying ObjectQueryState instance.
        /// Although not called directly, this method must be public to be reliably callable from <see cref="CreateQuery()"/> using reflection.
        /// </summary>
        /// <typeparam name="TResultType">The required element type of the new ObjectQuery</typeparam>
        /// <param name="queryState">The underlying ObjectQueryState instance that should back the returned ObjectQuery</param>
        /// <returns>A new ObjectQuery based on the specified query state, with the specified element type</returns>
        public static ObjectQuery<TResultType> CreateObjectQuery<TResultType>(ObjectQueryState queryState)
        {
            Debug.Assert(queryState != null, "Query state is required");
            Debug.Assert(typeof(TResultType) == queryState.ElementType, "Element type mismatch");

            return new ObjectQuery<TResultType>(queryState);
        }
    }    
}
