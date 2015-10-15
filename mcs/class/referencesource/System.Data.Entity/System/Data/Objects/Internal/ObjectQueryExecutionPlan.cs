//---------------------------------------------------------------------
// <copyright file="ObjectQueryExecutionPlan.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects.Internal
{
    using System;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.Internal.Materialization;
    using System.Data.Common.QueryCache;
    using System.Data.Common.Utils;
    using System.Data.EntityClient;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Diagnostics;
    using CompiledQueryParameters = System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<ObjectParameter, System.Data.Objects.ELinq.QueryParameterExpression>>;
    
    /// <summary>
    /// Represents the 'compiled' form of all elements (query + result assembly) required to execute a specific <see cref="ObjectQuery"/>
    /// </summary>
    internal sealed class ObjectQueryExecutionPlan
    {
        internal readonly DbCommandDefinition CommandDefinition;
        internal readonly ShaperFactory ResultShaperFactory;
        internal readonly TypeUsage ResultType;
        internal readonly MergeOption MergeOption;
        internal readonly CompiledQueryParameters CompiledQueryParameters;
        
        /// <summary>If the query yields entities from a single entity set, the value is stored here.</summary>
        private readonly EntitySet _singleEntitySet;

        private ObjectQueryExecutionPlan(DbCommandDefinition commandDefinition, ShaperFactory resultShaperFactory, TypeUsage resultType, MergeOption mergeOption, EntitySet singleEntitySet, CompiledQueryParameters compiledQueryParameters)
        {
            Debug.Assert(commandDefinition != null, "A command definition is required");
            Debug.Assert(resultShaperFactory != null, "A result shaper factory is required");
            Debug.Assert(resultType != null, "A result type is required");

            this.CommandDefinition = commandDefinition;
            this.ResultShaperFactory = resultShaperFactory;
            this.ResultType = resultType;
            this.MergeOption = mergeOption;
            this._singleEntitySet = singleEntitySet;
            this.CompiledQueryParameters = compiledQueryParameters;
        }

        internal static ObjectQueryExecutionPlan Prepare(ObjectContext context, DbQueryCommandTree tree, Type elementType, MergeOption mergeOption, Span span, CompiledQueryParameters compiledQueryParameters, AliasGenerator aliasGenerator)
        {
            TypeUsage treeResultType = tree.Query.ResultType;

            // Rewrite this tree for Span?
            DbExpression spannedQuery = null;
            SpanIndex spanInfo;
            if (ObjectSpanRewriter.TryRewrite(tree, span, mergeOption, aliasGenerator, out spannedQuery, out spanInfo))
            {
                tree = DbQueryCommandTree.FromValidExpression(tree.MetadataWorkspace, tree.DataSpace, spannedQuery);
            }
            else
            {
                spanInfo = null;
            }

            DbConnection connection = context.Connection;
            DbCommandDefinition definition = null;

            // The connection is required to get to the CommandDefinition builder.
            if (connection == null)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ObjectQuery_InvalidConnection);
            }
                        
            DbProviderServices services = DbProviderServices.GetProviderServices(connection);

            try
            {
                definition = services.CreateCommandDefinition(tree);
            }
            catch (EntityCommandCompilationException)
            {
                // If we're running against EntityCommand, we probably already caught the providers'
                // exception and wrapped it, we don't want to do that again, so we'll just rethrow
                // here instead.
                throw;
            }
            catch (Exception e)
            {
                // we should not be wrapping all exceptions
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    // we don't wan't folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a CommandDefinitionException and make whatever we caught  
                    // the inner exception of it.
                    throw EntityUtil.CommandCompilation(System.Data.Entity.Strings.EntityClient_CommandDefinitionPreparationFailed, e);
                }
                throw;
            }

            if (definition == null)
            {
                throw EntityUtil.ProviderDoesNotSupportCommandTrees();
            }

            EntityCommandDefinition entityDefinition = (EntityCommandDefinition)definition;
            QueryCacheManager cacheManager = context.Perspective.MetadataWorkspace.GetQueryCacheManager();
            
            ShaperFactory shaperFactory = ShaperFactory.Create(elementType, cacheManager, entityDefinition.CreateColumnMap(null),
                context.MetadataWorkspace, spanInfo, mergeOption, false);

            // attempt to determine entity information for this query (e.g. which entity type and which entity set)
            //EntityType rootEntityType = null;

            EntitySet singleEntitySet = null;

            if (treeResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
            {
                // determine if the entity set is unambiguous given the entity type
                if (null != entityDefinition.EntitySets)
                {
                    foreach (EntitySet entitySet in entityDefinition.EntitySets)
                    {
                        if (null != entitySet)
                        {
                            if (entitySet.ElementType.IsAssignableFrom(((CollectionType)treeResultType.EdmType).TypeUsage.EdmType))
                            {
                                if (singleEntitySet == null)
                                {
                                    // found a single match
                                    singleEntitySet = entitySet;
                                }
                                else
                                {
                                    // there's more than one matching entity set
                                    singleEntitySet = null;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return new ObjectQueryExecutionPlan(definition, shaperFactory, treeResultType, mergeOption, singleEntitySet, compiledQueryParameters);
        }

        internal string ToTraceString()
        {
            string traceString = string.Empty;
            EntityCommandDefinition entityCommandDef = this.CommandDefinition as EntityCommandDefinition;
            if (entityCommandDef != null)
            {
                traceString = entityCommandDef.ToTraceString();
            }
            return traceString;
        }

        internal ObjectResult<TResultType> Execute<TResultType>(ObjectContext context, ObjectParameterCollection parameterValues)
        {
            DbDataReader storeReader = null;
            try
            {
                // create entity command (just do this to snarf store command)
                EntityCommandDefinition commandDefinition = (EntityCommandDefinition)this.CommandDefinition;
                EntityCommand entityCommand = new EntityCommand((EntityConnection)context.Connection, commandDefinition);

                // pass through parameters and timeout values
                if (context.CommandTimeout.HasValue)
                {
                    entityCommand.CommandTimeout = context.CommandTimeout.Value;
                }

                if (parameterValues != null)
                {
                    foreach (ObjectParameter parameter in parameterValues)
                    {
                        int index = entityCommand.Parameters.IndexOf(parameter.Name);

                        if (index != -1)
                        {
                            entityCommand.Parameters[index].Value = parameter.Value ?? DBNull.Value;
                        }
                    }
                }

                // acquire store reader
                storeReader = commandDefinition.ExecuteStoreCommands(entityCommand, CommandBehavior.Default);

                ShaperFactory<TResultType> shaperFactory = (ShaperFactory<TResultType>)this.ResultShaperFactory;
                Shaper<TResultType> shaper = shaperFactory.Create(storeReader, context, context.MetadataWorkspace, this.MergeOption, true);

                // create materializer delegate
                TypeUsage resultItemEdmType;

                if (ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
                {
                    resultItemEdmType = ((CollectionType)ResultType.EdmType).TypeUsage;
                }
                else
                {
                    resultItemEdmType = ResultType;
                }

                return new ObjectResult<TResultType>(shaper, this._singleEntitySet, resultItemEdmType);
            }
            catch (Exception)
            {
                if (null != storeReader)
                {
                    // Note: The caller is responsible for disposing reader if creating
                    // the enumerator fails.
                    storeReader.Dispose();
                }
                throw;
            }
        }

        internal static ObjectResult<TResultType> ExecuteCommandTree<TResultType>(ObjectContext context, DbQueryCommandTree query, MergeOption mergeOption)
        {
            Debug.Assert(context != null, "ObjectContext cannot be null");
            Debug.Assert(query != null, "Command tree cannot be null");

            ObjectQueryExecutionPlan execPlan = ObjectQueryExecutionPlan.Prepare(context, query, typeof(TResultType), mergeOption, null, null, System.Data.Common.CommandTrees.ExpressionBuilder.DbExpressionBuilder.AliasGenerator);
            return execPlan.Execute<TResultType>(context, null);
        }
    }
}
